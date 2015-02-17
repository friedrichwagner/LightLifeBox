using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using Lumitech.Helpers;
using PILEDServer;

namespace Lumitech.Interfaces
{
    class DALI : IPILed, IObserver<PILEDData>
    {
        private Object thisLock = new Object();
        private const int DALI_SEND_SLEEPTIME = 50;

        private SerialPort serial;
        private const int BAUDRATE = 2400;
        private const int DATABITS = 8;
        private const int RECV_LENGTH = 1;
        private byte[] recvDataArray = new byte[RECV_LENGTH];

        private int fadetime = 0;
        public int FadingTime
        {
            get { return fadetime; }
            set { fadetime = value; }
        }

       private byte[] DALISendData = new byte[2];

       //FW 3.12.2014
       private IDisposable cancellation;

        public DALI()
        {
            serial = new SerialPort();
            Settings ini = Settings.GetInstance();
        }

        #region IPILed-Interface

        public bool Connect(string portname)
        {
            serial.PortName = portname;
            serial.BaudRate = BAUDRATE;
            serial.Parity = Parity.None;
            serial.DataBits = DATABITS;
            serial.StopBits = StopBits.One;
            serial.Open();

            serial.WriteTimeout = 2000;
            serial.ReadTimeout = 2000;

            setFadingTime(0);

            return serial.IsOpen;
        }

        public bool Connect(string portname, string Notused)
        {
            return Connect(portname);
        }

        public bool Disconnect()
        {
            serial.Close();

            return serial.IsOpen;
        }

        public CommInterface Interface
        {
            get { return CommInterface.DALI; }
        }

        public TuneableWhiteType TWType
        {
            get { return TuneableWhiteType.PILED_MM2; }
        }

        public bool isConnected
        {
            get { return serial.IsOpen; }
        }


        public void Flash(byte[] b, byte turns)
        {

        }

        public void setBrightness(byte b)
        {
            if (b == 255) b = 254;

            Send(new byte[2]{0xFE, b});

        }

        public void setBrightness(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
                if (b[i] == 255) b[i] = 254;

            setRGB(b);
        }

        public void setFadingTime(byte ftime)
        {
            Send(new byte[2] { 0xA3, 0x00 });  // fading time ins DTR            
            Send(new byte[2] { 0xFF, 0x2E }); //
            Send(new byte[2] { 0xFF, 0x2E }); //
       }


        public void setBrightnessOneModule(byte[] b, byte notused)
        {
            setBrightness(b);            
        }

        public void setCCT(Single CCT, byte brightness)
        {
            byte[] byMirek = BitConverter.GetBytes((UInt16)(1e6 / CCT));

            Send(new byte[] { 0xA3, byMirek[0] });  //LSB der CCT(in Mirek) ins DTR                        
            Send(new byte[] { 0xC3, byMirek[1] }); //MSB der CCT(in Mirek) ins DTR1            
            Send(new byte[] { 0xC1, 0x08 }); //Enable DT8
            Send(new byte[] { 0xFF, 0xE7 }); //Set Temp
            Send(new byte[] { 0xC1, 0x08 }); //Enable DT8
            Send(new byte[] { 0xFF, 0xE2 }); //Activate
            setBrightness(brightness);
        }

        public void setXy(Single[] cie, byte brightness)
        {
            byte[] byx = BitConverter.GetBytes((UInt16)(cie[0] * 65535));
            byte[] byy = BitConverter.GetBytes((UInt16)(cie[1] * 65535));

            Send(new byte[] { 0xA3, byx[0] });  //x-LSB ins DTR        
            Send(new byte[] { 0xC3, byx[1] }); //x-MSB ins DTR1
            Send(new byte[] { 0xC1, 0x08 }); //Enable DT8
            Send(new byte[] { 0xFF, 0xE0 }); //Set Temp x

            Send(new byte[] { 0xA3, byy[0] });  //y-LSB ins DTR        
            Send(new byte[] { 0xC3, byy[1] }); //y-MSB ins DTR1
            Send(new byte[] { 0xC1, 0x08 }); //Enable DT8
            Send(new byte[] { 0xFF, 0xE1 }); //Set Temp y

            Send(new byte[] { 0xC1, 0x08 }); //Enable DT8
            Send(new byte[] { 0xFF, 0xE2 }); //Activate

            setBrightness(brightness);
        }


        public void setRGB(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
                if (b[i] == 255) b[i] = 254;

            //Intern Phosphor, Blaue, Rot
            Send(new byte[2] { 0xA3, b[2] });  //R-Byte ins DTR         
            Send(new byte[2] { 0xC3, b[0] }); //p-Byte ins DTR1
            Send(new byte[2] { 0xC5, b[1] }); //B-Byte ins DTR2
            Send(new byte[2] { 0xC1, 0x08 }); //Enable DT8
            Send(new byte[2] { 0xFF, 0xEB }); //Set Temp RGB Dimmlevel
            Send(new byte[2] { 0xC1, 0x08 }); //Enable DT8
            Send(new byte[2] { 0xFF, 0xE2 }); //Activate
        }
        #endregion

        private void Send(byte[] data)
        {
            lock (thisLock)
            {
                serial.Write(data, 0, data.Length);
                Thread.Sleep(DALI_SEND_SLEEPTIME);
            }
        }

        private void Receive()
        {
            int i;
            //try
            {
                for (i = 0; i < RECV_LENGTH; i++)
                {
                    recvDataArray[i] = Convert.ToByte(serial.ReadByte());
                }
            }
            // catch { }
        }

        #region Observer Pattern

        public virtual void Subscribe(UDPServer provider)
        {
            Settings ini = Settings.GetInstance();
            string[] PortName = ini.Read<string>("DALI", "USBCom", "127.0.0.1").Split(','); //z.B. COM4,19200,8,N,1

            Connect(PortName[0]); 
            cancellation = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
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
            //Currently always broadcast
            //iStartAddress = (uint)info.groupid;

            switch (info.mode)
            {
                case PILEDMode.PILED_SET_BRIGHTNESS:
                    this.setBrightness((byte)info.brightness);
                    break;
                case PILEDMode.PILED_SET_CCT:
                    this.setCCT(info.cct, (byte)info.brightness);
                    break;
                case PILEDMode.PILED_SET_XY:
                    float[] f = new float[2];
                    f[0] = (float)info.xy[0]; f[1] = (float)info.xy[1];
                    this.setXy(f, (byte)info.brightness);
                    break;
                case PILEDMode.PILED_SET_RGB:
                    byte[] b = new byte[3];
                    b[0] = (byte)info.rgb[0]; b[1] = (byte)info.rgb[1]; b[2] = (byte)info.rgb[2];
                    this.setRGB(b);
                    break;
            }
        }

        #endregion
    }
}
