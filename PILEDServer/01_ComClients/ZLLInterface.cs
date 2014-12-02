using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using Lumitech.Helpers;

namespace PiledToolbox.Interfaces
{

    enum ZLLCommand : byte
    {
        //ZLL
        SET_BRIGHTNESS = 1,
        SET_XY = 2,
        SET_CCT = 3,
		SET_RGB = 4,

        GET_CCT_MIN_MAX = 20,
        GET_MODEL = 21,
    }

    enum InterfaceIdentifier : byte
    {
        IF_TEST = 0,
        IF_ZLL = 1,
        IF_DALI = 2
    }

    struct PILEDInterfaceData
    {
        private const int BUFFERSIZE=30;
        private const int FADETIME = 0; // hier in ms Einheiten, // Beim Messen ist fading eher nicht erwünscht
        private const int FILLER_SIZE = 12;

        private byte byStart;        
        public byte byCmd;
        public byte bySubCmd;
        private InterfaceIdentifier byIF;

        public byte brightness;
        public UInt16 fadetime;

        public UInt16 ciex;
        public UInt16 ciey;
        public UInt16 cct;

        public byte byR;
        public byte byG;
        public byte byB;

        private byte[] filler; //muss 8 bytes sein

        private byte byCRC;
        private byte byStop;
        //private byte isvalid;

        public byte[] byArrBuffer;

        public static PILEDInterfaceData NewPIFData()
        {
            PILEDInterfaceData pifData = new PILEDInterfaceData();

            pifData.byStart = 0x02;            
            pifData.byCmd = 0x00;
            pifData.bySubCmd = 0x00;
            pifData.byIF = InterfaceIdentifier.IF_TEST;
            pifData.brightness = 255;
            pifData.fadetime = FADETIME; // Beim Messen ist fading eher nicht erwünscht
            pifData.ciex = 0;
            pifData.ciey = 0;
            pifData.cct = 2700;
            pifData.byR = 0;
            pifData.byG = 0;
            pifData.byB = 0;
            pifData.filler = new byte[FILLER_SIZE];
            pifData.byCRC = 0;
            pifData.byStop = 0x03;
            //pifData.isvalid = 0;

            pifData.byArrBuffer = new Byte[BUFFERSIZE];

            return pifData;
        }

        public byte[] ToByteArray()
        {
            byte[] b2=new byte[2];
            byte i = 0;

            byArrBuffer[i++] = byStart;            
            byArrBuffer[i++] = byCmd;
            byArrBuffer[i++] = bySubCmd;
            byArrBuffer[i++] = (byte)byIF;
            byArrBuffer[i++] = brightness;

            b2 = BitConverter.GetBytes((UInt16)(fadetime / 100)); //Fadetime in 100ms Einheiten
            byArrBuffer[i++] = b2[0];
            byArrBuffer[i++] = b2[1];

            b2 = BitConverter.GetBytes(ciex);
            byArrBuffer[i++] = b2[0];
            byArrBuffer[i++] = b2[1];

            b2 = BitConverter.GetBytes(ciey);
            byArrBuffer[i++] = b2[0];
            byArrBuffer[i++] = b2[1];

            b2 = BitConverter.GetBytes(cct);
            byArrBuffer[i++] = b2[0];
            byArrBuffer[i++] = b2[1];

            byArrBuffer[i++] = byR;
            byArrBuffer[i++] = byG;
            byArrBuffer[i++] = byB;

            for (int k = 0; k < filler.Length; k++) byArrBuffer[i++] = filler[k];

            byArrBuffer[i++] = calcCRC();
            byArrBuffer[i++] = byStop;


            return byArrBuffer;
        }

        private byte calcCRC()
        {
            int num = 24; // CRC Berechnung von Byte 4(=Brightness) bis 27 (=vor CRC Byte)
            int j, k, crc8, m, x;

            crc8 = 0;
            //Achtung: m startet mit 4 (Brightness), nicht 0
            for (m = 4; m < num; m++)
            {
                x = byArrBuffer[m];
                for (k = 0; k < 8; k++)
                {
                    j = 1 & (x ^ crc8);
                    crc8 = (crc8 / 2) & 0xFF;
                    x = (x / 2) & 0xFF;
                    if (j != 0)
                        crc8 = crc8 ^ 0x8C;
                }
            }
            byCRC = (byte)crc8;

            return byCRC;
        }

    }

    class ZLLInterface : IPILed
    {
        private PILEDInterfaceData pifData = PILEDInterfaceData.NewPIFData();
        private Object thisLock = new Object();

        private SerialPort serial;
        private const int BAUDRATE = 19200;
        private const int DATABITS = 8;
        private const int RECVARRAYSIZE = 30;

        private byte[] recvDataArray = new byte[RECVARRAYSIZE];

        private byte lastBrightness;
        private Single lastCCT= 3000;
        private Single[] lastXy = new Single[2];
        private byte[] lastRGB = new byte[3];
        private ZLLCommand lastCmd;
        private int fadetime = 0;
        public int FadingTime
        {
            get { return fadetime; }
            set { fadetime = value; }
        }

        private bool await8080;
        
        public ZLLInterface(bool pawait8080)
        {
            serial = new SerialPort();
            Settings ini = Settings.GetInstance();
            //ini.ForceReRead();
            fadetime = ini.Read<int>("ZLL", "Fadetime", 0);
            await8080 = pawait8080;
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
            get { return CommInterface.ZLL; }
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
            for (int i = 0; i < turns; i++)
            {
                pifData.brightness = 0;
                Send(ZLLCommand.SET_BRIGHTNESS);

                Thread.Sleep(200);

                pifData.brightness = 255;
                Send(ZLLCommand.SET_BRIGHTNESS);

                Thread.Sleep(200);
            }

            pifData.brightness = lastBrightness;
            Send(ZLLCommand.SET_BRIGHTNESS);

            /*pifData.byR = b[2];
            pifData.byG = b[0];
            pifData.byB = b[1];
            pifData.byRGBFlag = turns;
            Send(ZLLCommand.DO_FLASH);*/

        }

        public void setBrightness(byte b)
        {            
            pifData.brightness = b;
            Send(ZLLCommand.SET_BRIGHTNESS);
            lastBrightness = b;
        }

        public void setBrightness(byte[] b)
        {            
            setRGB(b);
        }


        public void setBrightnessOneModule(byte[] b, byte notused)
        {
            setBrightness(b);
        }

        public void setCCT(Single CCT, byte brightness)
        {
            pifData.cct = (UInt16) (1e6 / CCT);
            //pifData.brightness = brightness;
            //Send(ZLLCommand.SET_BRIGHTNESS);
            //Thread.Sleep(200);
            Send(ZLLCommand.SET_CCT);            

            lastCCT = CCT;
        }

        public void setXy(Single[] cie, byte brightness)
        {
            pifData.ciex = (UInt16)(65536 * cie[0]);
            pifData.ciey = (UInt16)(65536 * cie[1]);
            //pifData.brightness = brightness;

            //Send(ZLLCommand.SET_BRIGHTNESS);
            //Thread.Sleep(200);
            Send(ZLLCommand.SET_XY);

            lastXy = cie;
        }


        public void setRGB(byte[] b)
        {
            //Intern Phosphor, Blaue, Rot
            lastRGB = b;
            pifData.byG = b[0];
            pifData.byB = b[1];
            pifData.byR = b[2];
            Send(ZLLCommand.SET_RGB);
        }
#endregion

        private void Send(ZLLCommand cmd)
        {
            lock (thisLock)
            {
                pifData.byCmd = (byte)cmd;
                pifData.fadetime = (UInt16)fadetime;
                pifData.ToByteArray();

                serial.Write(pifData.byArrBuffer, 0, pifData.byArrBuffer.Length);

                lastCmd = cmd;
                //Do receive immediately
        
                if (await8080) Receive();
            }
        }

        private void Receive()
        {
            //Was kommt da zurück ?
            /*int i = serial.BytesToRead;
            while (serial.BytesToRead>0)  serial.ReadByte();
            int b = i * 2;*/
            int i;
            //try
            {
                for (i = 0; i < 30; i++)
                {
                    recvDataArray[i] = Convert.ToByte(serial.ReadByte());
                }
            }
           // catch { }
        }
    }
}
