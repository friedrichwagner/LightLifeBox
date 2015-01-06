using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Security.Permissions;
using Lumitech.Helpers;
using PILEDServer;

namespace Lumitech.Interfaces
{
    public enum FT_STATUS
    {
        FT_OK = 0,
        FT_INVALID_HANDLE,
        FT_DEVICE_NOT_FOUND,
        FT_DEVICE_NOT_OPENED,
        FT_IO_ERROR,
        FT_INSUFFICIENT_RESOURCES,
        FT_INVALID_PARAMETER,
        FT_INVALID_BAUD_RATE,
        FT_DEVICE_NOT_OPENED_FOR_ERASE,
        FT_DEVICE_NOT_OPENED_FOR_WRITE,
        FT_FAILED_TO_WRITE_DEVICE,
        FT_EEPROM_READ_FAILED,
        FT_EEPROM_WRITE_FAILED,
        FT_EEPROM_ERASE_FAILED,
        FT_EEPROM_NOT_PRESENT,
        FT_EEPROM_NOT_PROGRAMMED,
        FT_INVALID_ARGS,
        FT_OTHER_ERROR
    };

    public enum LTDMX_INTERFACE
    {
        LTDMX_IF_USB,
        LTDMX_IF_UDP
    }

    public enum DMX_MODE:byte { CCT_MODE=0, RGB_MODE=101, XY_MODE=201 };

    public class PILEDDMX : DMX
    {
        public PILEDDMX()
            : base(3)
        {
        }
    }


    public class DMX : IPILed, IObserver<PILEDData>
    {
        private const int iDMXBroadCastStartAddress = 501;
        private const int ARTNET_HEADER_SIZE = 18;
        protected const int MINCCT = 2700;
        protected const int MAXCCT = 6500;

        private byte[] buffer = new byte[512+1]; //DMX Start Code byte[0]=0 muss hier mitgeschickt werden, d.h. eigentlich werden 513 bytes geschickt
        private byte[] artNetData = new byte[ARTNET_HEADER_SIZE + 512];
        private uint handle;
        private  bool done = false;
        private int bytesWritten = 0;
        private FT_STATUS status;
        //private long istat;

        private const byte BITS_8 = 8;
        private const byte STOP_BITS_2 = 2;
        private const byte PARITY_NONE = 0;
        private const UInt16 FLOW_NONE = 0;
        private const byte PURGE_RX = 1;
        private const byte PURGE_TX = 2;

        //FW 29.3.29012
        UdpClient udpclient=new UdpClient();

        //FW 3.12.2014
        private IDisposable cancellation;

        private uint iStartAddress;  //0..511
        public uint StartAddress    //1..512
        {
            get { return iStartAddress; }
            set 
            { 
                if (value<1 || value>512) throw new ArgumentOutOfRangeException("StartAddress between 1 and 497");
                iStartAddress= value; 
            }
        } 

        private LTDMX_INTERFACE enumDMX_IF;
        public LTDMX_INTERFACE DMX_IF { get { return enumDMX_IF; } }

        private string strUDPAddress;

        private bool buseBroadcast;
        public bool useBroadcast
        {
            get { return buseBroadcast; }
            set 
            {
                buseBroadcast = value;
                //steuern über Broadcast: Kanal 501=3, Kanal 512=3, auss bei "Address" und Reset
                setDmxValue(-1, 3);
                setDmxValue(10, 3);
            }
        }

        private bool bConnected;
        public bool connected  { get { return bConnected; } }

        private DMX_MODE eMode;
        public virtual DMX_MODE Mode
        {
            get { return eMode; }
            set { eMode = value; setDmxValue(0, (byte) value); }
        }

        private byte iCntChannels;
        public byte CntChannels
        {
            get { return iCntChannels; }
        }

        private byte[] brightness;


#region FTD2XX.DLL
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Open(UInt32 uiPort, ref uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Close(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Read(uint ftHandle, IntPtr lpBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesReturned);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Write(uint ftHandle, IntPtr lpBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesWritten);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetDataCharacteristics(uint ftHandle, byte uWordLength, byte uStopBits, byte uParity);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetFlowControl(uint ftHandle, char usFlowControl, byte uXon, byte uXoff);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_GetModemStatus(uint ftHandle, ref UInt32 lpdwModemStatus);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Purge(uint ftHandle, UInt32 dwMask);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_ClrRts(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetBreakOn(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetBreakOff(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_GetStatus(uint ftHandle, ref UInt32 lpdwAmountInRxQueue, ref UInt32 lpdwAmountInTxQueue, ref UInt32 lpdwEventStatus);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_ResetDevice(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetDivisor(uint ftHandle, char usDivisor);
 
#endregion

        Thread sendThread;

        public DMX(byte AnzChannels)
        {
            int i;
            for (i=0;i<buffer.Length;i++)           buffer[i]=0;
            for (i=0;i<artNetData.Length; i++) artNetData[i] = 0;

            if (!(AnzChannels==2 || AnzChannels==3)) throw new ArgumentOutOfRangeException("Wrong Nr of channels (only 2 or 3 allowed!");

            iCntChannels = AnzChannels;
            brightness = new byte[AnzChannels];
            setb(new byte[] { 0 });
        }

#region IPILed-Interface
        public bool Connect(string dmxstartaddress)
        {
            if (UInt16.Parse(dmxstartaddress) < 1 || UInt16.Parse(dmxstartaddress) > 512) throw new ArgumentOutOfRangeException("DMX Address between 1 and 497");

            done = false;
            iStartAddress = UInt16.Parse(dmxstartaddress);
            buseBroadcast = false;
            enumDMX_IF = LTDMX_INTERFACE.LTDMX_IF_USB;
            bConnected = startUSBThread();
            setDmxValue(0, 0);  //Set DMX Start Code
           
            Flash(3);

            bConnected = true;

            return bConnected;
        }

        public bool Connect(string dmxstartaddress, string UDPAddress)
        {
            int i;

            if (UInt16.Parse(dmxstartaddress) < 1 || UInt16.Parse(dmxstartaddress) > 512) throw new ArgumentOutOfRangeException("DMX Address between 1 and 497");

            for (i = 0; i < buffer.Length; i++) buffer[i] = 0;
            for (i = 0; i < artNetData.Length; i++) artNetData[i] = 0; 

            done = false;
            iStartAddress = UInt16.Parse(dmxstartaddress);
            enumDMX_IF = LTDMX_INTERFACE.LTDMX_IF_UDP;
            strUDPAddress = UDPAddress;


            udpclient.Connect(strUDPAddress, 0x1936);
            //udpclient.DontFragment = true;
            startUDPThread();

            Flash(3);

            bConnected = true;

            return bConnected;
        }

        public bool Disconnect()
        {
            done = true;
            Thread.Sleep(200);
            if (sendThread != null) sendThread.Join();           

            //istat = CloseDmx();

            bConnected = false;

            return bConnected;
        }

        public CommInterface Interface
        {
            get { return CommInterface.DMX; }
        }

        public bool isConnected
        {
            get { return bConnected;}
        }

        public virtual TuneableWhiteType TWType
        {
            get { return TuneableWhiteType.None; }
        }
        
        public virtual void setBrightness(byte b)
        {
            //Nur Helligkeitskanal setzen im CCT und xy-Modus
            if (eMode == DMX_MODE.CCT_MODE || eMode == DMX_MODE.XY_MODE)
                setDmxValue(1, b);
            else
            {
                setb(new byte[] { b, b, b });
                setBrightness(brightness);
            }
        }

        public virtual void setBrightness(byte[] b)
        {
            //P,B,R--> R,G,B
            setb(new byte[] { b[2], b[0], b[1] });


            setDmxValue(0, (byte) DMX_MODE.RGB_MODE);                
            setDMXChannels(brightness, buseBroadcast);

        }

        public virtual void setBrightnessOneModule(byte[] b, byte modulenr)
        {
            //Ein Modul geht bei DMX nicht
            setBrightness(b);
        }

        public virtual void setCCT(Single CCT, byte b)
        {

            byte dmxcct = (byte)((CCT - MINCCT) / (MAXCCT - MINCCT) * 255);

            setDmxValue(0, (byte)DMX_MODE.CCT_MODE);
            setDmxValue(1, b);
            setDmxValue(2, dmxcct);

        }

        public virtual void setRGB(byte[] b)
        {
            //Das ist immer RGB-Modus
            setBrightness(b);
        }

        public virtual void Flash(byte turns)
        {
            if (useBroadcast)
            {
                //Flash blue
                setb(new byte[] { 0 });
                brightness[brightness.GetLength(0)-1] = 255;
                Flash(brightness, turns); //Blau blinken wenn Broadcast
            }
            else
            {
                //Flash red 
                setb( new byte[]{255});
                Flash(brightness, turns); //sonst rot
            }
        }

        public virtual void Flash(byte[] b, byte turns)
        {
            int k;
 
            setDmxValue(0, (byte)DMX_MODE.RGB_MODE);

            for (int i = 0; i < turns; i++)
            {
                for (k = 0; k <= 5; k++)
                {
                    for (int m = 0; m < b.Length; m++)
                        if (b[m] > 0)
                        {
                              setDmxValue(m + 1, (byte)(k * 50));
                        }

                    Thread.Sleep(25);
                }
                for (k = 5; k >= 0; k--)
                {
                    for (int m = 0; m < b.Length; m++)
                        if (b[m] > 0)
                        {
                            setDmxValue(m + 1, (byte)(k * 50));
                        }

                    Thread.Sleep(25);
                }
            }


            setDmxValue(0, 0);
            setDmxValue(1, 0);
            setDmxValue(2, 0);
            setDmxValue(3, 0);

        }

        public virtual void setXy(Single[] cie, byte b)
        {

            byte x = (byte)((cie[0] - 0.17) / 0.00188);
            byte y = (byte)((cie[1] - 0.08) / 0.00153);

            setDmxValue(0, (byte) DMX_MODE.XY_MODE);
            setDmxValue(1, b);
            setDmxValue(2, x);
            setDmxValue(3, y);

        }
#endregion

#region DMX
        public void Reset()
        {
            //1. Start Resetting
             ResetDMXBroadCastChannels();
             buseBroadcast = true;
             try
             {
                 setDmxValue(-1, 1); //=501
                 for (int c = 0; c < 10; c++)
                     setDmxValue(c, (byte)(10 * c));

                 Thread.Sleep(100);

                 //2. 
                 setDmxValue(10, 1);

                 Thread.Sleep(1000);
             }
             finally
             {
                 buseBroadcast = false;
                 ResetDMXBroadCastChannels();
             }
        }

        public void Address(uint newAddress)
        {
            //1. Start Adressing
            ResetDMXBroadCastChannels();
            buseBroadcast = true;
            try
            {
                setDmxValue(-1, 2); // 501 + 1 (wird in setDmxValue bei bBroadcast gerechent --> deswegen -1 hier
                setDmxValue(10, 2);

                Thread.Sleep(100);

                //2. 
                if (newAddress < 256)
                    setDmxValue(1, (byte)newAddress);
                else
                    setDmxValue(2, (byte)(newAddress - 256));

                Thread.Sleep(100);

                //2. 
                if (newAddress < 256)
                    setDmxValue(3, 255);
                else
                    setDmxValue(4, 255);

                Thread.Sleep(1000);

                //3. 
            }
            finally
            {
                buseBroadcast = false;
                ResetDMXBroadCastChannels();
            }
        }

        public void ResetDMXBroadCastChannels()
        {
            for (int c = iDMXBroadCastStartAddress; c < iDMXBroadCastStartAddress+12; c++)
                setDmxValue(c, 0);
        }

        //FW 9.2.2014 - relative channel now: 0..4 --> 1,2,3,4
        protected void setDmxValue(int offset, byte value)
        {
            int channel = 0;

            if (buseBroadcast)
                channel = iDMXBroadCastStartAddress + offset + 1;
            else
                channel = (int)iStartAddress + offset;

            if (channel >= 1 && channel <= 512)
            {
                buffer[channel] = value;
                artNetData[ARTNET_HEADER_SIZE + channel - 1] = value;
            }
        }

        protected byte getDmxValue(uint channel)
        {
            if (channel < 1 || channel > 512) return 0;

            if (enumDMX_IF == LTDMX_INTERFACE.LTDMX_IF_USB)
            {
                return buffer[channel];

            }
            else if (enumDMX_IF == LTDMX_INTERFACE.LTDMX_IF_UDP)
            {
                return artNetData[18 + channel - 1];
            }

            return 0;
        }

        private void setb(byte[] b)
        {
            for (int i = 0; i < brightness.GetLength(0); i++)
            {
                if (b.GetLength(0) > i)
                    brightness[i] = b[i];
                else
                    brightness[i] = 0;
            }
        }

        private void setDMXChannels(byte[] b, bool useBroadcast = false)
        {
            if (TWType == TuneableWhiteType.PILED_MM1 || TWType == TuneableWhiteType.PILED_MM2)
            {
                setDmxValue(1, b[2]); setDmxValue(2, b[0]); setDmxValue(3, b[1]);
            }
            else
                for (byte i = 1; i <= iCntChannels; i++) setDmxValue(i, b[i - 1]);
    
        }
#endregion

#region USB-Handling
        //The whole stuff here is from the ENTTEC WEB-Site
        //http://www.enttec.com/download/examples/OpenDMX.cs

        private bool startUSBThread()
        {
            handle = 0;
            status = FT_Open(0, ref handle);

            if (status != FT_STATUS.FT_OK) return false;


            sendThread = new Thread(new ThreadStart(writeDataUSB));
            sendThread.Start();
            setDmxValue(0, 0);  //Set DMX Start Code

            return true;
        }

        private void writeDataUSB()
        {
            while (!done)
            {
                initOpenDMXUSB();
                status=FT_SetBreakOn(handle);
                status=FT_SetBreakOff(handle);
                bytesWritten = writeUSB(handle, buffer, buffer.Length);
                //Thread.Sleep(20);
            }

            FT_Close(handle);
        }

        private int writeUSB(uint handle, byte[] data, int length)
        {
            IntPtr ptr = Marshal.AllocHGlobal(length);
            Marshal.Copy(data, 0, ptr, length);
            uint bytesWritten = 0;
            status = FT_Write(handle, ptr, (uint)length, ref bytesWritten);
            return (int)bytesWritten;
        }

        private void initOpenDMXUSB()
        {
            status = FT_ResetDevice(handle);
            status = FT_SetDivisor(handle, (char) 12);  // set baud rate
            //status = FT_SetBaudRate(handle, (char)12);  // set baud rate
            status = FT_SetDataCharacteristics(handle, BITS_8, STOP_BITS_2, PARITY_NONE);
            status = FT_SetFlowControl(handle, (char)FLOW_NONE, 0, 0);
            status = FT_ClrRts(handle);
            //status = FT_SetLatencyTimer(handle, (byte)40);
            status = FT_Purge(handle, PURGE_TX);
            status = FT_Purge(handle, PURGE_RX);
        }
#endregion

#region UDP-Handling
        /* **************** UDP ************************ */
        private static byte[] artNetName = new System.Text.ASCIIEncoding().GetBytes("Art-Net");
        private void ArtNetHeader()
        {
            

            byte[] data = new byte[18 + 512]; // 18 + number of channels

            // ID
            artNetName.CopyTo(artNetData, 0);
            artNetData[7] = 0x00;

            // OpCode
            artNetData[8] = 0x00;
            artNetData[9] = 0x50;

            // ProtVerH
            artNetData[10] = 0x00;
            //ProtVer
            artNetData[11] = 0x0E;

            // Sequence
            artNetData[12] = 0x00;

            // Physical
            artNetData[13] = 0x00;

            // Universe
            artNetData[14] = 0x00; // <- Universe Setting
            artNetData[15] = 0x00;

            // LengthHi
            artNetData[16] = 0x02; // Length High Byte 512 Byte always
            // Length
            artNetData[17] = 0x00; // Length Low Byte

            // Data[Length]
        }

        private void startUDPThread()
        {
            ArtNetHeader();
            sendThread = new Thread(new ThreadStart(writeDataUDP));
            sendThread.Start();
            //setDmxValue(0, 0);  //Set DMX Start Code
            /*setDmxValue(1, 1);
            setDmxValue(2, 255);
            setDmxValue(3, 255);
            udpclient.Send(artNetData, artNetData.Length);*/
        }

        private void writeDataUDP()
        {
            while (!done)
            {
                udpclient.Send(artNetData, artNetData.Length);
                Thread.Sleep(25);
            }

        }
#endregion

#region Observer Pattern

        public virtual void Subscribe(UDPServer provider)
        {
            Settings ini = Settings.GetInstance();

            cancellation = provider.Subscribe(this);

            strUDPAddress = ini.Read<string>("LTDMX", "UDP_Address", "127.0.0.1");
            Connect("1", strUDPAddress);
        }

        public virtual void Unsubscribe()
        {
            done = true;            
            cancellation.Dispose();
        }

        //Called from UDP Server when closing application
        public virtual void OnCompleted()
        {
            Disconnect();
        }

        public virtual void OnError(Exception e)
        {

        }

        //Called from UDP Server when new data arrive
        public virtual void OnNext(PILEDData info)
        {
            iStartAddress = (uint)info.groupid;

            switch (info.mode)
            {
                case PILEDMode.PILED_SET_BRIGHTNESS:
                    this.setBrightness((byte)info.brightness);
                    break;
                case PILEDMode.PILED_SET_CCT:
                    this.setCCT(info.cct, (byte) info.brightness);
                    break;
                case PILEDMode.PILED_SET_XY:
                    float[] f = new float[2];
                    f[0] = (float)info.xy[0]; f[1] = (float)info.xy[1];
                    this.setXy( f, (byte)info.brightness);
                    break;
                case PILEDMode.PILED_SET_RGB:
                    byte[] b = new byte[3];
                    b[0] = (byte)info.rgb[0];b[1] = (byte)info.rgb[1];b[2] = (byte)info.rgb[2];
                    this.setRGB(b);
                    break;
            }
        }

 #endregion

    }

}