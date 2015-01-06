//#define TESTPORT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.Management;
using Lumitech.Helpers;

namespace Lumitech
{
    public enum OUTPUT_PORT { OUTP1 = 1, OUTP2 = 2 };
    public enum OUTPUT_MODE { CONSTANT_VOLTAGE = 1, CONSTANT_CURRENT = 2, CONSTANT_POWER = 3 };
    public delegate void OnStateChanged(string DeviceName, bool On);

    public class SCPI
    {
        protected Settings ini;
        public OnStateChanged DoStateChanged;
        protected SerialPort port;
        protected string[] Identity;
        protected OUTPUT_PORT outp;
        protected int mAFactor;

        public string[] Model
        {
            get { return Identity; }
        }

        protected string sDeviceName;
        public string DeviceName 
        {
            get { return sDeviceName; }
        }

        public string PortName
        {
            get { return port.PortName; }
            //set { port.PortName = value; }
        }

        protected bool bisOn;
        public bool isOn
        {
            get { return bisOn; }
        }

        public bool isOpen
        {
            get { return port.IsOpen; }
        }

        protected bool bwasInitialized;
        public bool wasInitialized
        {
            get { return bwasInitialized; }
        }

        protected SCPI(string DeviceName)
        {
            this.sDeviceName = DeviceName;
            ini = Settings.GetInstance();

            string portname = "";
            int baudrate = 9600;
            int databits = 8;
            string parity = "None";
            int stopbits = 1;
            
            string portsettings = ini.ReadString(DeviceName,"COM-Port","");
            string[] arr = portsettings.Split(',');

            if (arr.Length == 5)
            {
                portname = arr[0];
                baudrate = Int32.Parse(arr[1]);
                databits = Byte.Parse(arr[2]);
                parity = arr[3];
                stopbits = Byte.Parse(arr[4]);
            }
            else
            {
                portname = getComPort(DeviceName);
            }

            if (portname.Length==0) throw new Exception("Com-Port is empty!");
            
            bisOn = false;
            bwasInitialized = false;

#if (!TESTPORT)            
            port = new SerialPort(portname, baudrate, (Parity)Enum.Parse(typeof(Parity), parity), databits, (StopBits)stopbits);
            //Synchrone Timeouts setzen
            port.WriteTimeout = 2000;
            port.ReadTimeout = 2000;
            port.Handshake = Handshake.None;
            port.NewLine = "\r";
#else
            port = new SerialPort();
            bisOn = false;
#endif
        }

        public virtual void Init()
        {
#if (!TESTPORT)
            if (port.IsOpen) port.Close();
            port.Open();

            Reset();
            Identify();
            bwasInitialized = true;

#else
            Debug.Print("Init()");
#endif
        }


        public virtual void Init(string portname)
        {
#if (!TESTPORT)
            if (port.IsOpen) port.Close();
            port.PortName = portname;
            port.Open();

            Reset();
            Identify();
            bwasInitialized = true;
#else
            Debug.Print("Init()");
#endif
        }

        public virtual void Done()
        {
            if (port.IsOpen) Off();
#if (!TESTPORT)
            port.Close();
#else
            Debug.Print("Done()");
#endif           
            string portsettings = port.PortName + "," + port.BaudRate + "," + port.DataBits + "," + port.Parity.ToString() + "," + ((byte)port.StopBits).ToString();
            ini.WriteString(DeviceName, "COM-Port", portsettings);
        }

        protected void WriteData(string data)
        {
#if (!TESTPORT)
            port.WriteLine(data);
#else
            Debug.Print(data);
#endif
        }

        protected string ReadData(string command)
        {
            string reply = "";

#if (!TESTPORT)
            //1. Query senden
            port.WriteLine(command);

            //2. synchron auf Daten warten
            reply = port.ReadLine();
#else
            if (command.Contains("READ")) reply = "24.00,345.34";
            else if (command.Contains("VOLT")) reply = "24.00";
            else if (command.Contains("CURR")) reply = "345.34";
#endif
            return reply;
        }


        //FW 14.3.2014 - Allgemein Kommandos get,set
        protected void SetCmd(string cmd)
        {
            WriteData(cmd);
        }

        protected string[] GetCmd(string cmd)
        {
            //IEEE-488.2
            string s = ReadData(cmd);
            Identity = s.Split(',');

            return Identity;
        }


        protected string[] Identify()
        {
            //IEEE-488.2
            string s=ReadData("*IDN?");
            Identity= s.Split(',');

            return Identity;
        }

        protected void Reset()
        {
            //IEEE-488.2
            WriteData("*RST");
            bisOn = false;
        }

        //FW 14.3.2014 - Toellner has 2 Output Ports
        public void SelectPort(OUTPUT_PORT p)
        {
            SetCmd(":INST:NSEL " + ((byte)p).ToString());
            outp = p;
        }

        protected void Clear()
        {
            //IEEE-488.2
            WriteData("*CLR");
        }

        public void On()
        {            
            WriteData(":OUTP ON");
            bisOn = true;

            if (DoStateChanged != null) DoStateChanged(sDeviceName, bisOn);
        }

        public void Off()
        {            
            WriteData(":OUTP OFF");
            bisOn = false;

            if (DoStateChanged != null) DoStateChanged(sDeviceName, bisOn);
        }

        // Beeper
        public void Beep(string freq, string time, int counts, int pause)
        {
#if (!TESTPORT)
            for (int i = 0; i < counts; i++)
            {
                WriteData(":SYST:BEEP:IMM " + freq + ", " + time);
                System.Threading.Thread.Sleep(pause);
            }
#else
            Debug.Print("Beep()");
#endif
        }

        protected virtual String[] Measure()
        {
            //IEEE-488.2
            if (!bisOn) throw new Exception("Device not turned on");           

            string reply = ReadData(":READ?");

            String[] newData = reply.Split(',');
            for (int i = 0; i < newData.Length; i++)
            {
                StringBuilder sb = new StringBuilder(newData[i]);
                sb.Replace('.', ',');
                newData[i] = sb.ToString();
            }

            return newData;
        }

        private string getComPort(string devicename)
        {
            string ret = String.Empty;

            //Connection credentials to the remote computer - not needed if the logged in account has access 
            ConnectionOptions oConn = new ConnectionOptions();

            System.Management.ManagementScope oMs = new System.Management.ManagementScope("\\\\localhost", oConn);

            //get Fixed disk stats 
            System.Management.ObjectQuery oQuery = new System.Management.ObjectQuery("Select * From Win32_PnPEntity where Manufacturer like '" + devicename +"' and Name like '%COM%'");
            //System.Management.ObjectQuery oQuery = new System.Management.ObjectQuery("Select * From Win32_PnPEntity where Name like '%COM%'");

            //Execute the query  
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);

            //Get the results
            ManagementObjectCollection oReturnCollection = oSearcher.Get();

            //loop through found drives and write out info 
            foreach (ManagementObject oReturn in oReturnCollection)
            {
                StringBuilder myDev = new StringBuilder(oReturn["Name"].ToString().ToUpper());

                int i = myDev.ToString().IndexOf("COM");
                if (i > 0)
                {
                    Int32 num = 0;

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
