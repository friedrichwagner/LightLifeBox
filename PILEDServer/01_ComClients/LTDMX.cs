using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Security.Permissions;


namespace PiledToolbox.Interfaces
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

    public class PILEDDMX : LTDMXBase
    {
        public PILEDDMX()
            : base(3)
        {
        }
    }

    public class CWWWDMX : LTDMXBase
    {
        public CWWWDMX() : base(2)
        {
        }

        public override void setXy(Single[] cie, byte brightness)
        {
            throw new ArgumentOutOfRangeException("Function not supported by this Interface!");
        }
    }

    public class LTDMXBase : IPILed
    {
        private const int iDMXBroadCastStartAddress = 501;
        protected const int MINCCT = 2700;
        protected const int MAXCCT = 6500;

        private byte[] buffer = new byte[512+1]; //DMX Start Code byte[0]=0 muss hier mitgeschickt werden, d.h. eigentlich werden 513 bytes geschickt
        private byte[] artNetData = new byte[18+512];
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
            set { buseBroadcast = value; }
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

        public LTDMXBase(byte AnzChannels)
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

            if (useBroadcast == false)
            {
                setDmxValue(0, (byte) DMX_MODE.RGB_MODE);                
                setDMXChannels(brightness);
            }
            else
            {
                //0. Reset
                //myDMX.ResetDMXBroadCastChannels();

                //1. Start
                setDmxValueBroadcast(0, 3);
                setDmxValueBroadcast(11, 3);

                Thread.Sleep(100);

                //2. Werte setzen
                setDmxValueBroadcast(1, (byte)DMX_MODE.RGB_MODE);
                setDMXChannels(brightness , true);

                Thread.Sleep(100);
            }
        }

        public virtual void setBrightnessOneModule(byte[] b, byte modulenr)
        {
            //Ein Modul geht bei DMX nicht
            setBrightness(b);
        }

        public virtual void setCCT(Single CCT, byte b)
        {

            byte dmxcct = (byte)((CCT - MINCCT) / (MAXCCT - MINCCT) * 255);

            if (useBroadcast == false)
            {
                setDmxValue(0, (byte)DMX_MODE.CCT_MODE);
                setDmxValue(1, b);
                setDmxValue(2, dmxcct);
            }
            else
            {
                //0. Reset
                //myDMX.ResetDMXBroadCastChannels();

                //1. Start
                setDmxValueBroadcast(0, 3);
                setDmxValueBroadcast(11, 3);

                Thread.Sleep(100);

                //2. Werte setzen
                //setDmxValueBroadcast(0, 3);
                setDmxValueBroadcast(1, (byte)DMX_MODE.CCT_MODE);
                setDmxValueBroadcast(2, b);
                setDmxValueBroadcast(3, dmxcct);
                //setDmxValueBroadcast(11, 3);
            }
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

            if (useBroadcast)
            {
                //1. Start
                setDmxValueBroadcast(0, 3);
                setDmxValueBroadcast(11, 3);

                setDmxValueBroadcast(1, (byte)DMX_MODE.RGB_MODE);
            }
            else
                setDmxValue(0, (byte)DMX_MODE.RGB_MODE);

            for (int i = 0; i < turns; i++)
            {
                for (k = 0; k <= 5; k++)
                {
                    for (uint m = 0; m < b.Length; m++)
                        if (b[m] > 0)
                        {
                            if (useBroadcast)
                                setDmxValueBroadcast(m + 2, (byte)(k * 50));
                            else
                                setDmxValue(m + 1, (byte)(k * 50));
                        }

                    Thread.Sleep(25);
                }
                for (k = 5; k >= 0; k--)
                {
                    for (uint m = 0; m < b.Length; m++)
                        if (b[m] > 0)
                        {
                            if (useBroadcast)
                                setDmxValueBroadcast(m + 2, (byte)(k * 50));
                            else
                                setDmxValue(m + 1, (byte)(k * 50));
                        }

                    Thread.Sleep(25);
                }
            }


            if (useBroadcast)
            {
                setDmxValueBroadcast(1, 0);
                setDmxValueBroadcast(2, 0);
                setDmxValueBroadcast(3, 0);
                setDmxValueBroadcast(4, 0);
            }
            else
            {
                setDmxValue(0, 0);
                setDmxValue(1, 0);
                setDmxValue(2, 0);
                setDmxValue(3, 0);
            }
        }

        public virtual void setXy(Single[] cie, byte b)
        {

            byte x = (byte)((cie[0] - 0.17) / 0.00188);
            byte y = (byte)((cie[1] - 0.08) / 0.00153);

            if (useBroadcast == false)
            {
                setDmxValue(0, (byte) DMX_MODE.XY_MODE);
                setDmxValue(1, b);
                setDmxValue(2, x);
                setDmxValue(3, y);
            }
            else
            {
                //0. Reset
                //FW 7.3.2013 --> Problem, PI-LED stellt dann dazwischen immer auf 0
                //myDMX.ResetDMXBroadCastChannels();

                //1. Start
                setDmxValueBroadcast(0, 3);
                setDmxValueBroadcast(11, 3);

                Thread.Sleep(100);

                //2. Werte setzen
                //setDmxValueBroadcast(0, 3);
                setDmxValueBroadcast(1, (byte)DMX_MODE.XY_MODE);
                setDmxValueBroadcast(2, b);
                setDmxValueBroadcast(3, x);
                setDmxValueBroadcast(4, y);
                //setDmxValueBroadcast(11, 3);
            }
        }
#endregion

#region DMX
        public void Reset()
        {
            //1. Start Resetting
             ResetDMXBroadCastChannels();

             setDmxValueBroadcast(0, 1);
            for (uint c=1;c<11;c++)
                setDmxValueBroadcast(c, (byte)(10*c));            

            Thread.Sleep(100);

            //2. 
            setDmxValueBroadcast(11, 1);

            Thread.Sleep(1000);

            //3. 
            ResetDMXBroadCastChannels();
        }

        public void Address(uint newAddress)
        {
            //1. Start Adressing
            ResetDMXBroadCastChannels();
            setDmxValueBroadcast(0 , 2);         
            setDmxValueBroadcast(11, 2);

            Thread.Sleep(100);

            //2. 
            if (newAddress < 256)          
                setDmxValueBroadcast(1, (byte) newAddress);          
            else
                setDmxValueBroadcast(2, (byte) (newAddress-256));

            Thread.Sleep(100);

            //2. 
            if (newAddress < 256)          
                setDmxValueBroadcast(3, 255);          
            else
                setDmxValueBroadcast(4, 255);

            Thread.Sleep(1000);

            //3. 
            ResetDMXBroadCastChannels();
        }

        public void ResetDMXBroadCastChannels()
        {
            for (uint c=0;c<12;c++)
                setDmxValueBroadcast(c, 0);
        }

        //FW 9.2.2014 - relative channel now: 0..4 --> 1,2,3,4
        protected void setDmxValue(uint channel, byte value)
        {
            if ( (iStartAddress + channel) < (buffer.Length-1))
                buffer[iStartAddress + channel] = value;

            if ( (18 + iStartAddress + channel - 1) < (artNetData.Length))
                artNetData[18 + iStartAddress + channel - 1] = value;
        }

        protected void setDmxValueBroadcast(uint channel, byte value)
        {
            //channel: 0(= DMX 501) to 11 (= DMX 512)
            if ( (iDMXBroadCastStartAddress + channel) <= (buffer.Length-1)) //501+11 <= 513-1
                buffer[iDMXBroadCastStartAddress + channel] = value;

            if ( (18 + iDMXBroadCastStartAddress + channel - 1) <= (artNetData.Length-1)) // 18+501+11-1 < 530-1
                artNetData[18 + iDMXBroadCastStartAddress + channel - 1] = value;
                //artNetData[18 + iDMXBroadCastStartAddress + channel] = value;
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
                if (useBroadcast)
                {
                    setDmxValueBroadcast(1, b[2]); setDmxValueBroadcast(2, b[0]); setDmxValueBroadcast(3, b[1]);
                }
                else
                {
                    setDmxValue(1, b[2]); setDmxValue(2, b[0]); setDmxValue(3, b[1]);
                }
            }
            else
            {
                if (useBroadcast)
                    for (byte i = 1; i <= iCntChannels; i++) setDmxValueBroadcast((uint)(i+1), b[i-1]);
                else
                    for (byte i = 1; i <= iCntChannels; i++) setDmxValue(i, b[i-1]);
            }
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

    }

}