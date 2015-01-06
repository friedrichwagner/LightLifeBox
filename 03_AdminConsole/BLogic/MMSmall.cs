using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using Lumitech.Helpers;

namespace Lumitech
{
    public static class BcdConverter
    {
        // Int32 in BCD umwandeln
        public static int ConvertToBCD(object number)
        {
            try
            {
                string numberString = Convert.ToString(number);
                return Int32.Parse(numberString, System.Globalization.NumberStyles.HexNumber);
            }
            catch { return 0; }
        }

        // BCD in Int32 umwandeln
        public static int ConvertFromBCD(object number)
        {
            string s = string.Format("{0:x}", number);
            return Int32.Parse(s);
        }
    }

    class MMSmall:ZBComBase2
    {
        private const byte MM2_BATCHNR_POS=9;
        private Byte[] bBatchNr = new Byte[4];
        public UInt32 BatchNr
        {
            get { return Convert.ToUInt32(BcdConverter.ConvertFromBCD(BitConverter.ToUInt32(bBatchNr, 0))); }
            //get { return 0; }
        }

        private Byte[] bSerialNr = new Byte[4];
        public Int32 SerialNr
        {
            get { return BcdConverter.ConvertFromBCD(BitConverter.ToInt32(bSerialNr, 0)); }
        }
        private string _Comport;
        public string Comport { get { return _Comport; } set { _Comport=value;} }

        public MMSmall():base(false)
        {
            _Comport = String.Empty;
            ModuleNr = 0;
        }

        public MMSmall(bool bConnect)
            : base(false)
        {
            if (bConnect) Connect(); 
            else _Comport = String.Empty;

            ModuleNr = 0;
        }

        public bool Connect()
        {
            _Comport = getZBComPort();
            if (_Comport.Length == 0) throw new Exception("No serial port found!");
            return Connect(_Comport, ZBReceiver.SERIAL2USB);
        }

        public void setBrightness(byte[] b)
        {
            if (!base.isConnected) throw new ArgumentException("Not connected!");
            base.Send(ZBCommand.SetLedChannels, b);
        }

        public Single getTemperature(byte ModuleNr)
        {
#if TESTWITHOUTSERIAL
            Single tempCelsius = 23.123f;
#else
            if (!base.isConnected) throw new ArgumentException("Not connected!");

            base.ModuleNr = ModuleNr;
            base.Send(ZBCommand.GetADValue, new byte[] { });
            Single bAdValue = BitConverter.ToInt16(base.receivedDataArray, base.StartData);
            bAdValue = (0.0504f * bAdValue - 50); //Temperatur in °C bei MM2 LTE-1013
            Single tempCelsius = BitConverter.ToInt16(base.receivedDataArray, base.StartData+2)/10 - 273.15f;
#endif
            return tempCelsius;
        }

        public bool checkEEPROM(byte ModuleNr)
        {
#if TESTWITHOUTSERIAL
            return true;
#else
            byte[] dataIn = new byte[20];
            byte[] dataOut = new byte[20];

            if (!base.isConnected) throw new ArgumentException("Not connected!");

            base.ModuleNr = ModuleNr;

            for (byte i = 0; i < dataIn.Length; i++) 
            {
                dataIn[i] = 0;
                dataOut[i] = i;
            }

            WriteToEeprom(dataOut, 0);
            ReadFromEeprom(ref dataIn, 0);

            for (byte i = 0; i < dataIn.Length; i++)
            {
                if (dataIn[i] != dataOut[i]) return false;
            }
            return true;
#endif

        }

        private bool WriteToEeprom(byte[] bArr, byte start)
        {
            byte l = (byte)((bArr.Length > 25) ? 25 : bArr.Length);
            byte[] thisArr = new byte[l + 2];

            thisArr[0] = start;
            thisArr[1] = l;
            for (int i = 0; i < l; i++)
                thisArr[i + 2] = bArr[i];

            return base.Send(ZBCommand.SetEEPROMData, thisArr);
        }

        private int ReadFromEeprom(ref byte[] bArr, byte start)
        {
            int i;
            base.Send(ZBCommand.GetEEPROMData, new byte[] { (byte)start, (byte)bArr.Length });
            for (i = 0; i < bArr.Length; i++)
            {
                if (i >= 25) break;
                bArr[i] = base.receivedDataArray[base.StartData + i];
            }

            return i;
        }

        public void ResetBatchNR_SerialNr()
        {
            for (int i = this.bBatchNr.Length - 1; i >= 0; i--) this.bBatchNr[i] = 0;
            for (int i = this.bSerialNr.Length - 1; i >= 0; i--) this.bSerialNr[i] = 0;
        }

        public bool getBatchNR_SerialNr()
        {
            ResetBatchNR_SerialNr();

#if TESTWITHOUTSERIAL
            for (int i = this.bBatchNr.Length - 1; i >= 0; i--)
                this.bBatchNr[i] = (byte)i;

            //Achtung!! SerienNr nur 3 bytes lang, aber bSerialnr ist 4 bytes
            for (int i = this.bSerialNr.Length - 2; i >= 0; i--)
                this.bSerialNr[i] = (byte)i;
            return true;
#else
            if (!isConnected)
                this.Connect();

            base.Send(ZBCommand.GetEEPROMData, new byte[] { (MM2_BATCHNR_POS), (byte)10 });

            for (int i = this.bBatchNr.Length - 1; i >= 0; i--)
                this.bBatchNr[i] = receivedDataArray[this.StartData + (this.bBatchNr.Length - 1) - i];

            //Achtung!! SerienNr nur 3 bytes lang, aber bSerialnr ist 4 bytes
            for (int i = this.bSerialNr.Length - 2; i >= 0; i--)
                this.bSerialNr[i] = receivedDataArray[this.StartData + this.bBatchNr.Length + (this.bSerialNr.Length - 2) - i];

            return true;
#endif
        }

        private string getZBComPort()
        {
            string ret = _Comport;

            // Wenn schon einmal connected, dann immer gleiches Comport zurückgeben, da er beim Lesen mit WMI sehr langsam ist
            if (_Comport.Length > 0) return ret;

            //Alternativ, Comport fix in Ini File
            Settings ini = Settings.GetInstance();
            ret = ini.Read<string>("PI-LED", "COM-Port", "");
            if (ret.Length > 0) return ret;

            //Connection credentials to the remote computer - not needed if the logged in account has access 
            ConnectionOptions oConn = new ConnectionOptions(); 

            System.Management.ManagementScope oMs = new System.Management.ManagementScope("\\\\localhost", oConn);     

            //get Fixed disk stats 
            System.Management.ObjectQuery oQuery = new System.Management.ObjectQuery("Select * From Win32_PnPEntity where Manufacturer like 'FTDI' and Name like '%COM%'"); 

            //Execute the query  
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs,oQuery); 

            //Get the results
            ManagementObjectCollection oReturnCollection = oSearcher.Get();   
          
            //loop through found drives and write out info 
            foreach( ManagementObject oReturn in oReturnCollection ) 
            { 
                StringBuilder myDev = new StringBuilder(oReturn["Name"].ToString().ToUpper());

                int i = myDev.ToString().IndexOf("COM");
                if (i > 0)
                {
                    Int32 num=0;

                    //Comport zweistellig
                    bool b = Int32.TryParse(myDev.ToString().Substring(i + 3, 2), out num);

                    if (b)
                        ret = "COM" + num.ToString();
                    else
                        ret = "COM" + myDev.ToString().Substring(i + 3, 1);
                }
            }


            return ret;
        }
    }
}
