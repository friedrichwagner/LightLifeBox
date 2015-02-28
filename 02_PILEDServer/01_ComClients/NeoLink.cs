using System;
using System.IO.Ports;
using System.Threading;
using Lumitech.Helpers;
using System.Net.Sockets;
using LightLife.Data;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Lumitech.Interfaces
{
    public enum NEOLINK_INTERFACE : byte
    {
        NONE,
        USB,
        UDP
    }

    enum NeoLinkGroups : byte
    {
	    NL_GROUP_BROADCAST = 0,
	    NL_GROUP_1 = 201,
	    NL_GROUP_2 = 202,
	    NL_GROUP_3 = 203,
	    NL_GROUP_4 = 204,
	    NL_GROUP_5 = 205,
	    NL_GROUP_6 = 206,
	    NL_GROUP_7 = 207,
	    NL_GROUP_8 = 208,
	    NL_GROUP_9 = 209,
	    NL_GROUP_10 = 210,
	    NL_GROUP_11 = 211,
	    NL_GROUP_12 = 212,
	    NL_GROUP_13 = 213,
	    NL_GROUP_14 = 214,
	    NL_GROUP_15 = 215,
	    NL_GROUP_16 = 216
    };

    enum NeoLinkMode : byte
    {
	    //ZLL
	    NL_STARTUP = 1,
	    NL_NETWORK_STATUS = 2,
	    NL_NETWORK = 3,
	    NL_IDENTIFY = 4,
	    NL_GROUPCONFIG = 5,
	    NL_GROUP_REFRESH_ADDRESS = 6,
	    NL_GROUP_REFRESH_NAME = 7,
	    NL_BRIGHTNESS = 10,
	    NL_CCT = 11,
	    NL_RGB = 12,
	    NL_SCENES = 13,
	    NL_SEQUENCES_CALL = 14,
	    NL_SEQUENCES_SET = 15,
	    NL_XY = 16
    };

    enum NeoLinkSubMode_NETWORK : byte
    {
	    //ZLL
	    NL_NETWORK_TOUCHLINK = 0,
	    NL_NETWORK_RESETNEW = 1,
	    NL_NETWORK_CLASSICAL = 2,
	    NL_NETWORK_RELEASELAMP = 3
    };

    enum NeoLinkSubMode_SCENES : byte
    {
	    //ZLL
	    NL_SCENES_ADD = 0,
	    NL_SCENES_DELETE = 1,
	    NL_SCENES_DELETE_ALL = 2,
	    NL_SCENES_CALL = 3
    };

    struct NeoLinkData
    {
        private const int NL_BUFFER_SIZE=30;
        private const int DATA_SIZE= 24;

        public byte byStart;
        public byte byMode;
        public byte byAddress;

        public byte[] data;

	    public byte byGroupUpdate;
	    public byte byCRC;
	    public byte byStop;

        public byte[] byArrBuffer;

        public static NeoLinkData NewFrame()
        {
            NeoLinkData nlFrame = new NeoLinkData();

            nlFrame.byStart = 0x02;
            nlFrame.byMode = 0x00;
            nlFrame.byAddress = 0x00;

            nlFrame.byGroupUpdate = 0x00;
            nlFrame.byCRC = 0x00;
            nlFrame.byStop = 0x03;
            
            nlFrame.data = new Byte[DATA_SIZE]; 
            nlFrame.byArrBuffer = new Byte[NL_BUFFER_SIZE];

            return nlFrame;
        }

        /*public NeoLinkData (NeoLinkData oldFrame)
        {
            byStart = oldFrame.byStart;
            byMode = oldFrame.byMode;
            byAddress = oldFrame.byAddress;

            byGroupUpdate = oldFrame.byGroupUpdate;
            byCRC = oldFrame.byCRC;
            byStop = oldFrame.byStop;

            data = new Byte[DATA_SIZE];
            for (int i = 0; i < data.Length; i++)
                data[i] = oldFrame.data[i];

            byArrBuffer = new Byte[NL_BUFFER_SIZE];
            for (int i = 0; i < byArrBuffer.Length; i++)
                byArrBuffer[i] = oldFrame.byArrBuffer[i];
        }*/

        public byte[] ToByteArray()
        {
            int i = 0;

            byArrBuffer[i++] = byStart;
            byArrBuffer[i++] = byMode;
            byArrBuffer[i++] = byAddress;

            for (int k = 0; k < DATA_SIZE; k++) byArrBuffer[i++] = data[k];

            byArrBuffer[i++] = byGroupUpdate;
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
            for (m = 3; m < num; m++)
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

    class NeoLink : INeoLink, IObserver<PILEDData>
    {
        //private NeoLinkData nlFrame = NeoLinkData.NewFrame();
        private Object thisLock = new Object();
        private ConcurrentQueue<NeoLinkData> frameQueue;
        private Thread sendThread;
        private bool done = false;
        private Logger log;

        UdpClient udpclient = new UdpClient();
        private string _UDPAddress;
        private int _UDPPort ;
        NEOLINK_INTERFACE enumIF;
        private IDisposable cancellation;

        private SerialPort serial;
        private string _Comport;
        private const int BAUDRATE = 19200;
        private const int DATABITS = 8;
        private const int RECVARRAYSIZE = 30;

        private byte[] recvDataArray = new byte[RECVARRAYSIZE];

        private byte lastBrightness;
        private Single lastCCT = 3000;
        private Single[] lastXy = new Single[2];
        private byte[] lastRGB = new byte[3];
        private int fadetime = 0;
        public int FadingTime
        {
            get { return fadetime; }
            set { 
                fadetime = value;
                byFadetime = BitConverter.GetBytes((UInt16)(fadetime / 100));
            }
        }
        private byte[] byFadetime = new byte[2] {0,0};

        private int _sendDelay;
        //private byte _groupid;
        public byte groupid { get; set; }

        public NeoLink()
        {
            log = Logger.GetInstance();
            Settings ini = Settings.GetInstance();
            fadetime = ini.Read<int>("NeoLink", "Fadetime", 0);
            _sendDelay = ini.Read<int>("NeoLink", "SendDelayTimeMS", 200);

            _UDPAddress = ini.Read<string>("NeoLink", "UDP-Address", "");
            _UDPPort = ini.Read<int>("NeoLink", "UDP-Port", 1025); //PortNr of NeoLinkBox = 1025

            _Comport = ini.Read<string>("NeoLink", "USBCom", "");

            enumIF = NEOLINK_INTERFACE.NONE;
            groupid = 0;

            frameQueue = new ConcurrentQueue<NeoLinkData>();

            done = false;
            sendThread = new Thread(new ThreadStart(DoSendFrames));
            sendThread.Start();
        }
        
#region INeoLink-Interface
        public bool Connect(string portname)
        {
            serial = new SerialPort();

            serial.PortName = portname;
            serial.BaudRate = BAUDRATE;
            serial.Parity = Parity.None;
            serial.DataBits = DATABITS;
            serial.StopBits = StopBits.One;
            serial.Open();

            serial.WriteTimeout = 2000;
            serial.ReadTimeout = 2000;

            enumIF = NEOLINK_INTERFACE.USB;

            return serial.IsOpen;
        }

        public bool Connect(string IPAddress, int PortNr)
        {
            enumIF = NEOLINK_INTERFACE.UDP;

            udpclient.Connect(IPAddress, PortNr);

            return true;
        }

        public bool Disconnect()
        {
            if (enumIF == NEOLINK_INTERFACE.USB)
            {
                serial.Close();
                return serial.IsOpen;
            }

            return true;            
        }

        public CommInterface Interface
        {
            get { return CommInterface.NEOLINK; }
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
            byte localBrightness = lastBrightness;
            for (int i = 0; i < turns; i++)
            {
                setBrightness(0);
                Thread.Sleep(200);
                setBrightness(255);
                Thread.Sleep(200);
            }

            setBrightness(localBrightness);
        }

        public void setBrightness(byte val)
        {
            NeoLinkData nlFrame = NeoLinkData.NewFrame();
            nlFrame.byMode = (byte)NeoLinkMode.NL_BRIGHTNESS;
            nlFrame.byAddress = groupid;
            
            //brightness
            nlFrame.data[0] = val;

            //fadetime            
            nlFrame.data[1] = byFadetime[0];
            nlFrame.data[2] = byFadetime[1];

            frameQueue.Enqueue(nlFrame);
            lastBrightness = val;
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
            NeoLinkData nlFrame = NeoLinkData.NewFrame();
            nlFrame.byMode = (byte)NeoLinkMode.NL_CCT;
            nlFrame.byAddress = groupid;

	        //cct in mirek
	        int mirek = (int)(1e6 / CCT);

            byte[] b2 = BitConverter.GetBytes((UInt16)(mirek));

            nlFrame.data[0] = b2[0];
            nlFrame.data[1] = b2[1];

            //fadetime            
            nlFrame.data[2] = byFadetime[0];
            nlFrame.data[3] = byFadetime[1];

            frameQueue.Enqueue(nlFrame);

            lastCCT = CCT;
        }

        public void setXy(Single[] cie, byte brightness)
        {
            NeoLinkData nlFrame = NeoLinkData.NewFrame();
            nlFrame.byMode = (byte)NeoLinkMode.NL_XY;
            nlFrame.byAddress = groupid;

            byte[] b2 = BitConverter.GetBytes((UInt16)(65535 * cie[0]));
            nlFrame.data[0] = b2[0];
            nlFrame.data[1] = b2[1];

            b2 = BitConverter.GetBytes((UInt16)(65535 * cie[1]));
            nlFrame.data[2] = b2[0];
            nlFrame.data[3] = b2[1];

            //fadetime            
            nlFrame.data[4] = byFadetime[0];
            nlFrame.data[5] = byFadetime[1];

            frameQueue.Enqueue(nlFrame);

            lastXy = cie;
        }


        public void setRGB(byte[] b)
        {
            NeoLinkData nlFrame = NeoLinkData.NewFrame();
            nlFrame.byMode = (byte)NeoLinkMode.NL_RGB;
            nlFrame.byAddress = groupid;

            nlFrame.data[0] = b[0];
            nlFrame.data[1] = b[1];
            nlFrame.data[2] = b[2];

            frameQueue.Enqueue(nlFrame);

            lastRGB = b;
        }

        public void identify()
        {
            NeoLinkData nlFrame = NeoLinkData.NewFrame();
            nlFrame.byMode = (byte)NeoLinkMode.NL_IDENTIFY;
            nlFrame.byAddress = groupid;

            nlFrame.data[0] = 2; //Blinkdauer in sekunden

            frameQueue.Enqueue(nlFrame);
        }

        public void setSequence(byte id, byte brightness)
        {
            NeoLinkData nlFrame = NeoLinkData.NewFrame();
            nlFrame.byMode = (byte)NeoLinkMode.NL_SEQUENCES_CALL;
            nlFrame.byAddress = groupid;


            if (id > 0)
            {
                nlFrame.data[0] = id;
                nlFrame.data[1] = (byte)DateTime.Now.Hour;
                nlFrame.data[2] = (byte)DateTime.Now.Minute;
                nlFrame.data[3] = (byte)(brightness / 255.0 * 100.0); //in % !!
                nlFrame.data[4] = (byte)DateTime.Today.Day;
                nlFrame.data[5] = (byte)DateTime.Today.Month;
            }
            else //Sequenz stoppen
            {
                nlFrame.data[0] = id;
                nlFrame.data[1] = (byte)24;
                nlFrame.data[2] = (byte)60;
                nlFrame.data[3] = (byte)0;
                nlFrame.data[4] = (byte)0;
                nlFrame.data[5] = (byte)0;
            }

            frameQueue.Enqueue(nlFrame);
        }

        public void setScene(byte id)
        {
            NeoLinkData nlFrame = NeoLinkData.NewFrame();
            nlFrame.byMode = (byte)NeoLinkMode.NL_SCENES;
            nlFrame.byAddress = groupid;

            nlFrame.data[0] = id;
            nlFrame.data[1] = 3; //Szene aufrufen

            frameQueue.Enqueue(nlFrame);
        }

        #endregion

        private void DoSendFrames()
        {
            try
            {
                NeoLinkData frame;
                while (!done)
                {
                    while (frameQueue.TryDequeue(out frame))
                    {
                        Send(frame);
                        Thread.Sleep(_sendDelay);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);

            }
            finally {  }
        }

        private void Send(NeoLinkData frame)
        {
            lock (thisLock)
            {
                frame.ToByteArray();

                if (enumIF == NEOLINK_INTERFACE.USB)
                    serial.Write(frame.byArrBuffer, 0, frame.byArrBuffer.Length);
                else if (enumIF == NEOLINK_INTERFACE.UDP)
                    udpclient.Send(frame.byArrBuffer, frame.byArrBuffer.Length);
            }
        }

        /*private void Send(bool waitReceive = false)
        {
            lock (thisLock)
            {
                nlFrame.ToByteArray();

                if (enumIF == NEOLINK_INTERFACE.USB)
                    serial.Write(nlFrame.byArrBuffer, 0, nlFrame.byArrBuffer.Length);
                else if (enumIF == NEOLINK_INTERFACE.UDP)
                    udpclient.Send(nlFrame.byArrBuffer,nlFrame.byArrBuffer.Length);

                if (waitReceive)
                    Receive();
            }
        }

        private void Receive()
        {
            //tbd
        }*/

#region Observer Pattern

        public virtual void Subscribe(IObservable<PILEDData> provider)
        {
            Settings ini = Settings.GetInstance();

            cancellation = provider.Subscribe(this);

            //First see, if we have a NeoLink Box
            if (_UDPAddress.Length>0)
                Connect(_UDPAddress, _UDPPort);

            
            if (_Comport.Length > 0)
                Connect(_Comport);

        }

        public virtual void Unsubscribe()
        {
            done = true;
            cancellation.Dispose();
        }

        //Called from UDP Server when closing application
        public virtual void OnCompleted()
        {
            done = true;
            Disconnect();
        }

        public virtual void OnError(Exception e)
        {
            Debug.Print("NeoLink.OnError:" + e.Message);
            log.Error("NeoLink.OnError:" + e.Message);
        }

        //Called from UDP Server when new data arrive
        public virtual void OnNext(PILEDData info)
        {
            groupid= info.groupid;
            Debug.Print("NeoLink.OnNext:"+info.ToString());

            switch (info.mode)
            {
                case PILEDMode.SET_BRIGHTNESS:
                    this.setBrightness(info.brightness);
                    break;
                case PILEDMode.SET_CCT:
                    this.setCCT(info.cct, info.brightness);
                    break;
                case PILEDMode.SET_XY:
                    this.setXy(info.xy, info.brightness);
                    break;
                case PILEDMode.SET_RGB:
                      this.setRGB(info.rgb);
                    break;
                case PILEDMode.SET_SCENE:
                    this.setScene(info.sceneid);
                    break;
                case PILEDMode.SET_SEQUENCE:
                    this.setSequence(info.sequenceid, info.brightness);
                    break;

                case PILEDMode.IDENTIFY:
                    this.identify();
                    break;
            }
        }
#endregion

    }
}
