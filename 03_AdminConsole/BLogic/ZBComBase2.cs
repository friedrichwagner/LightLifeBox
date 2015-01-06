using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Lumitech
{
    public enum ZBCommand : int
    {
        //MM1 und MM2
        GetZigbeeAddresses = 2,
        GetADValue = 42,
        SetCCT = 43,        //FW 10.2.2014, für CW-WW
        SetLedChannels = 44,
        SetCIECoords = 45,
        Flash = 46,
        GetOperTime = 47,
        GetCritTime = 48,
        GetCurrGains = 49,  //FW 28.3.2012
        ResetCounter = 50,
        GetCurrCoords = 51,
        SetRpBTempComp = 52,
        GetVersions = 53,
        GetDMXMode = 54,

        //MM1 only
        SetSerialNr = 60,
        SetTriData = 61, //Set noch zu machen
        SetTempData = 64, //Set noch zu machen
        SetNTCCoeffs = 67,
        GetSerialNr = 90,
        GetTriData = 91,
        GetTempData = 94,
        GetNTCCoeffs = 97,
        SetADLimits = 71,
        SetMiscData = 70,
        GetMiscData = 100,
        GetADLimits = 101,

        //MM2 only
        GetEEPROMData1 = 110,
        GetEEPROMData2 = 111,
        GetCritTemp = 112,
        GetChannelData = 113,
        GetTriVsT = 114,
        GetTriVsTObs = 115,
        GetTriVsI = 116,
        GetEEPROMData = 117,
        GetAtmelData = 118,
        SetEEPROMData = 119,


        //Set commands not used in Toolbox, only SetEEPROMData       = 119 (works only for MM2 than)
        SetEEPROMData1 = 80,
        SetEEPROMData2 = 81,
        SetCritTemp = 82,
        SetChannelData = 83,
        SetTriVsT = 84,
        SetTriVsTObs = 85,
        SetTriVsI = 86,
        SetChecksum = 87,
        DelEEPROM = 88,
        SetPowerMeasurements = 89

    };

    public enum ZBReceiver : byte
    {
        USB = 1,
        LTC = 2,
        LTE = 3,
        SERIAL2USB = 4
    };

    struct ZBInterfaceData
    {
        private const int BUFFERSIZE_ZIGBEE = 49;
        private const int BUFFERSIZE_SERIAL2USB = 30;
        private const int FADETIME = 200;

        private byte byStart;
        public ZBCommand byCmd;

        public byte ModuleNr;
        public ZBReceiver Receiver;
        public Int64 ZigBeeAddress;

        private byte byCRC;
        private byte byStop;
        //private byte isvalid;
        public byte bytes2send;
        //private byte[] datatosend;
       // private byte datalength;

        public byte[] byArrBuffer;

        public static ZBInterfaceData NewPIFData()
        {
            ZBInterfaceData pifData = new ZBInterfaceData();

            pifData.Receiver = ZBReceiver.LTE;
            pifData.byStart = 0x02;
            pifData.byCmd = 0x00;
            pifData.ModuleNr = 0;
            //pifData.datatosend = new Byte[30];

            pifData.byStop = 0x03;
            //pifData.isvalid = 0;
            //pifData.byArrBuffer;            
            return pifData;
        }

        public byte[] ToByteArray(byte[] data)
        {
            //for (int i = 0; i < byArrBuffer.Length; i++) byArrBuffer[i] = 0x00;

            if (Receiver == ZBReceiver.SERIAL2USB)
            {
                byArrBuffer = new Byte[BUFFERSIZE_SERIAL2USB];
                byArrBuffer[0] = byStart;
                byArrBuffer[1] = (byte)byCmd;
                byArrBuffer[2] = ModuleNr;
                byArrBuffer[29] = byStop;

                if (data != null)
                    for (int i = 0; i < data.Length; i++) byArrBuffer[i + 3] = data[i];

                byArrBuffer[28] = calcCRC(28);
                bytes2send = 30;
            }
            else
            {
                byArrBuffer = new Byte[BUFFERSIZE_ZIGBEE];
                byArrBuffer[0] = byStart;
                byArrBuffer[1] = (byte)Receiver;

                for (int i = 10; i < 18; i++) byArrBuffer[i] = BitConverter.GetBytes(ZigBeeAddress)[i - 10];

                byArrBuffer[21] = (byte)byCmd;
                byArrBuffer[22] = ModuleNr;

                if (data != null)
                    for (int i = 0; i < data.Length; i++) byArrBuffer[i + 23] = data[i];

                //byArrBuffer[47] = calcCRC(28);    //Wird von USB Stick gemacht
                byArrBuffer[48] = byStop;
                bytes2send = 49;
            }

            return byArrBuffer;
        }

        private byte calcCRC(int num)
        {
            int j, k, crc8, m, x;

            crc8 = 0;
            //Achtung: m startet mit 1, nicht 0
            for (m = 1; m < num; m++)
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

    class ZBComBase2
    {
        //private ZBInterfaceData pifdata;
        private SerialPort serial;
        private const int BAUDRATE = 19200;
        private const int DATABITS = 8;
        private const int ZBARRAYSIZE = 47;

        private ZBReceiver Receiver;
        private static readonly object asynclock = new object();

        public byte[] receivedDataArray = new byte[ZBARRAYSIZE];
        public byte[] internalDataArray = new byte[ZBARRAYSIZE];
        public Int64 ZigBeeAddress;
        public byte ModuleNr;
        public byte StartData = 0;
        public ZBCommand ActualMsgCode;

        private int iMMVersion;
        public int MMVersion
        {
            get { return iMMVersion; }
        }

        private bool bhasMMVersion;
        public bool hasMMVersion
        {
            get { return bhasMMVersion; }
        }

        public ZBComBase2()
        {
            serial = new SerialPort();
            ZigBeeAddress = 0;
            iMMVersion = -1;

            //FW 14.2.2014: default = true
            bhasMMVersion = true;
        }

        public ZBComBase2(bool pHasMemoryMap) : this()
        {
            bhasMMVersion = pHasMemoryMap;
        }

        public bool Connect(string portname, ZBReceiver recv)
        {
#if TESTWITHOUTSERIAL
            return true;
#else
            serial.PortName = portname;
            serial.BaudRate = BAUDRATE;
            serial.Parity = Parity.None;
            serial.DataBits = DATABITS;
            serial.StopBits = StopBits.One;
            serial.Handshake = Handshake.None;
            serial.Open();

            Receiver = recv;

            serial.WriteTimeout = 2000;
            serial.ReadTimeout = 2000;

            try
            {
                if (recv == ZBReceiver.SERIAL2USB) ZigBeeAddress = 99999;
                else getZigbeeAddress();

                if (ZigBeeAddress<=0) throw new ArgumentOutOfRangeException("Zigbee Address not found!");

                if (bhasMMVersion)
                {
                    iMMVersion = getMMVersion();
                    if (iMMVersion <= 0) throw new ArgumentOutOfRangeException("Memory Map Version not detected!");
                }

                return serial.IsOpen;
            }
            catch
            {
                serial.Close();
                throw;
            }
#endif
            
        }

        private void getZigbeeAddress()
        {
            Send(ZBReceiver.USB, ZBCommand.GetZigbeeAddresses, null);
            ZigBeeAddress = BitConverter.ToInt64(internalDataArray, 9);
        }

        public bool Disconnect()
        {
#if TESTWITHOUTSERIAL
            return false;
#else
            serial.Close();
            return serial.IsOpen;
#endif
        }

        public bool isConnected
        {
#if TESTWITHOUTSERIAL
            get { return true; }
#else
            get { return serial.IsOpen; }
#endif
        }

        private int getMMVersion()
        {
            if (!bhasMMVersion) return -1;

            try
            {
                Send(this.Receiver, ZBCommand.GetEEPROMData, new byte[] { 0, 25 });
                return 2;
            }
            catch{}

            try
            {
                Send(this.Receiver, ZBCommand.GetSerialNr, null);
                return 1;
            }
            catch { }

            return -1;

        }

        private bool Send(ZBReceiver Receiver, ZBCommand cmd, byte[] data)
        {
            lock (asynclock)
            {
                bool throwException = false;
                Exception myEx;

                try
                {
                    bool ret = false;

                    ActualMsgCode = cmd;
                    ZBInterfaceData pifData = ZBInterfaceData.NewPIFData();
                    pifData.Receiver = Receiver;
                    pifData.byCmd = ActualMsgCode;
                    pifData.ZigBeeAddress = ZigBeeAddress;
                    pifData.ModuleNr = ModuleNr;

                    pifData.ToByteArray(data);

#if TESTWITHOUTSERIAL
                    ret = true;
#else
                    serial.Write(pifData.byArrBuffer, 0, pifData.byArrBuffer.Length);
                    //Do receive immediately
                    ret = Receive(pifData);
#endif
                    return ret;
                }
                catch (Exception ex)
                {
                    throwException = true;
                    myEx = ex;
                }

                if (throwException)  throw myEx;
                return false;
            }
        }

        public bool Send(ZBCommand cmd, byte[] data)
        {
            return Send(this.Receiver, cmd, data);
        }

        private bool Receive( ZBInterfaceData pifData)
        {
            int i;
            try
            {
                if (pifData.Receiver == ZBReceiver.SERIAL2USB)
                {
                    for (i = 0; i < 30; i++)
                    {
                        internalDataArray[i] = Convert.ToByte(serial.ReadByte());
                        if (i >= 3) receivedDataArray[i - 3] = internalDataArray[i];
                    }

                    if (internalDataArray[0] != 0x02 || internalDataArray[29] != 0x03)
                    {
                        throw new System.ArgumentException("ZBComBase._DataReceived: Received Data Error 1 !");
                    }
                    if (internalDataArray[1] != 0xFF && internalDataArray[1] != (byte)pifData.byCmd) throw new System.ArgumentException("ZBComBase._DataReceived: Received Data Error 2 !");


                    //Read everything whats left
                    while (serial.BytesToRead > 0) serial.ReadByte();
                    
                    return true;
                }
                else
                {
                    for (i = 0; i < internalDataArray.Length; i++) internalDataArray[i] = Convert.ToByte(serial.ReadByte());

                    byte stat1 = internalDataArray[0];
                    byte stat2 = internalDataArray[20];

                    if (stat1 == 0xFF)   throw new System.ArgumentException("LTC not found!");

                    //for (i = 0; i < internalDataArray.Length; i++) internalDataArray[i] = 0x00;

                    // Wenn Startbyte vom Ack-Array gleich 254 ist --> Daten empfangen
                    if (stat1 == 0xFE && stat2 == 0x33)
                    {
                        for (i = 0; i < internalDataArray.Length; i++)
                        {
                            internalDataArray[i] = Convert.ToByte(serial.ReadByte());
                            if (i >= 22) receivedDataArray[i - 22] = internalDataArray[i];
                        }
                    }
                    //AChtung bei getZigbeeAddress
                    //else receivedDataArray = internalDataArray;


                    if (internalDataArray[0] == 0xFF) 
                            throw new System.ArgumentException("ZBComBase._DataReceived: Received Data Array[0]=0xFF !");

                    //Read everything whats left
                    while (serial.BytesToRead > 0) serial.ReadByte();

                    return true;
                }

            }
            /*catch (TimeoutException et)
            {
                throw;
            }*/
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
