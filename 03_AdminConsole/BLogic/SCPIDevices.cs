using System;
using System.Collections.Generic;
using System.Text;
using Lumitech.Helpers;

namespace Lumitech
{
    public struct ResultSource
    {
        public Single Current;
        public Single Voltage;
    }

    public class PowerSource:SCPI
    {
        protected ResultSource result;
        protected ResultSource source;
        protected OUTPUT_MODE eMode;
        public OUTPUT_MODE Mode
        {
            get { return eMode; }
            set { eMode = value; }
        }

        protected static PowerSource _instance = null;
        protected static readonly object singletonLock = new object();

        public Single Current
        {
            get { return result.Current; }
        }

        public Single SourceVoltage
        {
            get { return source.Voltage; }
            set { source.Voltage = value; }
        }

        public Single SourceCurrent
        {
            get { return source.Current; }
            set { source.Current=value; }
        }

        public Single Voltage
        {
            get { return result.Voltage; }
        }

        public static PowerSource GetInstance(string DeviceName)
        {
            lock (singletonLock)
            {
                if (_instance == null)
                {
                    _instance = new PowerSource(DeviceName);
                }
                return _instance;
            }
        }

        public static PowerSource GetInstance()
        {
            lock (singletonLock)
            {
                if (_instance == null)
                {
                    throw new ArgumentNullException("PowerSource._instance");
                }
                return _instance;
            }
        }

        protected PowerSource(string DeviceName)
            : base(DeviceName)
        {
            Settings ini = Settings.GetInstance();
           source.Voltage = ini.Read<float>(DeviceName, "Voltage", (float)24.0);
           source.Current = ini.Read<float>(DeviceName, "Current", (float)1000.0);
           mAFactor = ini.Read<int>(DeviceName, "mAFactor", 1);

           eMode = (OUTPUT_MODE)ini.Read<int>(DeviceName, "OutputMode", (int)OUTPUT_MODE.CONSTANT_VOLTAGE);
        }

        public virtual void Set(Single Voltage, Single Current, OUTPUT_MODE mode)
        {
            eMode = mode;
            source.Voltage = Voltage;
            source.Current = Current;

            switch (mode)
            {
                case OUTPUT_MODE.CONSTANT_VOLTAGE:
                    WriteData(":SOUR:FUNC VOLT");
                    WriteData(":SOUR:VOLT:MODE FIX");
                    WriteData(":SOUR:VOLT:RANG 60");
                    WriteData(":SOUR:VOLT:LEV " + Voltage.ToString());

                    WriteData(":SENS:FUNC 'CURR'");
                    WriteData(":SENS:CURR:PROT " + Current.ToString() + "E-3");                    
                    WriteData(":SENS:CURR:RANG " + Current.ToString() + "E-3");
                    break;

                case OUTPUT_MODE.CONSTANT_CURRENT:
                    WriteData(":SOUR:FUNC CURR");
                    WriteData(":SOUR:CURR:MODE FIX");
                    //WriteData(":SOUR:CURR:RANG " + Current.ToString() + "E-3");
                    WriteData(":SOUR:CURR:LEV " + Current.ToString() + "E-3");                    

                    WriteData(":SENS:FUNC 'VOLT'");
                    WriteData(":SENS:VOLT:PROT " + Voltage.ToString());                                        
                    WriteData(":SENS:VOLT:RANG " + Voltage.ToString());
                    break;
            }
            base.On();
        }

        public new void On()
        //public override void On()
        {
            Set(source.Voltage, source.Current, eMode);
        }

        public virtual new ResultSource Measure()
        {
            string[] arr = base.Measure();
            result.Voltage = Single.Parse(arr[0]);
            result.Current = Single.Parse(arr[1]);

            return result;
        }

        public new void Done()
        {
            base.Done();

            ini.Write<float>(DeviceName, "Voltage", SourceVoltage);
            ini.Write<float>(DeviceName, "Current", SourceCurrent);
            ini.Write<float>(DeviceName, "OutputMode", (int)eMode);
            ini.Flush();
        }
    }

    public class Toellner : PowerSource
    {
        //private static Toellner _instance = null;
        //private static readonly object singletonLock = new object();        

        public static new Toellner GetInstance(string DeviceName)
        {
            lock (singletonLock)
            {
                if (_instance == null)
                {
                    _instance = new Toellner(DeviceName);
                }
                return (Toellner) _instance;
            }
        }

        private Toellner(string DeviceName)
            : base(DeviceName)
        {
            outp = OUTPUT_PORT.OUTP1;
            port.NewLine = "\n";
            port.Handshake = System.IO.Ports.Handshake.XOnXOff;
        }

        /*public new void Init(string portname)
        {
            base.Init(portname);
            SelectPort(outp);
        }

        public new void Init()
        {
            base.Init();
            SelectPort(outp);
        }*/

        public override void Set(Single Voltage, Single Current, OUTPUT_MODE mode)
        {
            eMode = mode;
            source.Voltage = Voltage;
            source.Current = Current;

            SelectPort(outp);

            switch (mode)
            {
                case OUTPUT_MODE.CONSTANT_VOLTAGE:
                    WriteData(":SOUR:VOLT:LEV " + Voltage.ToString());
                    WriteData(":SOUR:CURR " + Current.ToString() + "E-3");
                    break;

                case OUTPUT_MODE.CONSTANT_CURRENT:
                    WriteData(":SOUR:CURR:LEV " + Current.ToString() + "E-3");
                    WriteData(":SOUR:VOLT "     + Voltage.ToString());
                    break;
            }

            //base.On() // funktioniert nicht da Keithleysource.On() aufgerufen wird --> endless loop
            WriteData(":OUTP ON");
            bisOn = true;

            if (DoStateChanged != null) DoStateChanged(sDeviceName, bisOn);
        }

        public override ResultSource Measure()
        {           
            //IEEE-488.2
            if (!bisOn) throw new Exception("Device not turned on");

            string reply = ReadData(":MEAS:VOLT?");
            StringBuilder sb = new StringBuilder(reply);
            sb.Replace('.', ',');
            result.Voltage = Single.Parse(sb.ToString());

            reply = ReadData(":MEAS:CURR?");
            sb = new StringBuilder(reply);
            sb.Replace('.', ',');
            result.Current = Single.Parse(sb.ToString()) * mAFactor;

            return result;
        }
    }

    public class KeithleyTemperature : SCPI
    {
        private static KeithleyTemperature _instance = null;
        private static readonly object singletonLock = new object();

        private Single sTemperature;
        public Single Temperature
        {
            get { return sTemperature; }
        }

        public static KeithleyTemperature GetInstance(string DeviceName)
        {
            lock (singletonLock)
            {
                if (_instance == null)
                {
                    _instance = new KeithleyTemperature(DeviceName);
                }
                return _instance;
            }
        }

        private KeithleyTemperature(string DeviceName)
            : base(DeviceName)
        {
            Settings ini = Settings.GetInstance();
        }

        new public Single Measure()
        {
            string[] arr = base.Measure();
            sTemperature = Single.Parse(arr[0]);
            return sTemperature;
        }
    }
}
