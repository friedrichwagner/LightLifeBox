using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using Lumitech.Helpers;

namespace Lumitech
{
    class AtmelCPUProperties
    {
        public string CPUName { get; private set; }
        public string FlashInterface { get; private set; }
        public string Fuses { get; private set; }
        public string Frequency { get; private set; }

        public AtmelCPUProperties(string pCPU, string pInterface, string pFuses, string pFrequency)
        {
            CPUName = pCPU;
            FlashInterface = pInterface;
            Fuses = pFuses;
            Frequency = pFrequency;    
        }

        public AtmelCPUProperties(string iniFileString)
        {
            string[] arrProp = iniFileString.Split(';');
            if (arrProp.Length == 4)
            {
                CPUName = arrProp[0];
                FlashInterface = arrProp[1];
                Fuses = arrProp[2];
                Frequency = arrProp[3];
            }
            else
                throw new ArgumentException("Ivalid number of Paremeters for AtmelCPUProperties!");
        }

    }

    class FlashAtmel
    {
        private const string sFlashCommand = "@echo off \r\n" + " Path=@AVR-Path@;%Path% \r\n" + "@echo on\r\n" +
                                            "atprogram -t @tool@ -i @interface@ -d @cpu@ -cl @frequency@ write -fs --values @fuses@ --verify" +
                                            " program -c -fl -f \"@hexfile@\" --verify > \"@outfile@\"";

        private List<AtmelCPUProperties> CPUProps = new List<AtmelCPUProperties>();

        private string sTool = string.Empty;
        private string sHexFilePath = string.Empty;
        private string sHexFileName = string.Empty;
        private StringBuilder sCmd;
        private string sOutFile = string.Empty;
        private string sFlashFile = string.Empty;
        private string sErrorText = string.Empty;
        public string ErrorText { get { return sErrorText; } }

        public FlashAtmel()
        {
            Settings ini = Settings.GetInstance();
            
            sCmd = new StringBuilder(sFlashCommand);
            sCmd.Replace("@AVR-Path", ini.Read<string>("Flash", "AVRPath", ""));
            sHexFilePath = ini.Read<string>("Flash", "HexFiles","");
            sOutFile =  AppDomain.CurrentDomain.BaseDirectory + "out.txt";
            sFlashFile = AppDomain.CurrentDomain.BaseDirectory + "flash.bat";

            int i = 1;
            string prop = ini.ReadString("Flash", "CPUProp" + i.ToString(), "");
            while (prop.Length > 0)
            {
                CPUProps.Add(new AtmelCPUProperties(prop));
                i++;
                prop = ini.ReadString("Flash", "CPUProp" + i.ToString(), "");
            }
        }

        private string getHexFileName(string ltef)
        {
            string[] files = Directory.GetFiles(sHexFilePath, ltef + "*.hex");
            if (files.Length == 0)
                throw new ArgumentException("No Hex File found!\r\n" + sHexFilePath + "\r\n" + ltef);

            if (files.Length==1)
                return files[0];

            throw new ArgumentException("More than 1 Hexfile found!\r\n" + sHexFilePath + "\r\n" + ltef);
        }

        private string getFlashTool()
        {
            //Connection credentials to the remote computer - not needed if the logged in account has access 
            ConnectionOptions oConn = new ConnectionOptions(); 

            System.Management.ManagementScope oMs = new System.Management.ManagementScope("\\\\localhost", oConn);     

            //get Fixed disk stats 
            System.Management.ObjectQuery oQuery = new System.Management.ObjectQuery("Select * From Win32_PnPEntity where Manufacturer like 'Atmel%'"); 

            //Execute the query  
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs,oQuery); 

            //Get the results
            ManagementObjectCollection oReturnCollection = oSearcher.Get();   
          
            //loop through found drives and write out info 
            foreach( ManagementObject oReturn in oReturnCollection ) 
            { 
                StringBuilder myDev = new StringBuilder(oReturn["Description"].ToString().ToLower());
                myDev.Replace(" ", "");

                if (myDev.Equals("avrispmkii"))
                {
                    myDev.Clear(); myDev.Append("avrispmk2");
                }

                if (myDev.ToString().StartsWith("jtagice3"))
                {
                    myDev.Clear(); myDev.Append("jtagice3");
                }

                return myDev.ToString();
            }

            throw new ArgumentException("Tool not connected!");
        }

        private bool createFlashFile(string cpu)
        {
            int i = CPUProps.FindIndex(p => (p.CPUName == cpu));
            if (i == -1) throw new ArgumentOutOfRangeException("CPU properties not found!\r\n" + cpu);
 
            sCmd.Replace("@cpu@", CPUProps[i].CPUName);
            sCmd.Replace("@fuses@", CPUProps[i].Fuses);
            sCmd.Replace("@interface@", CPUProps[i].FlashInterface);
            sCmd.Replace("@frequency@", CPUProps[i].Frequency);

            sCmd.Replace("@hexfile@", sHexFileName);
            sCmd.Replace("@outfile@", sOutFile);
            sCmd.Replace("@tool@", sTool);

            using (StreamWriter s = new StreamWriter(sFlashFile))
            {
                s.WriteLine(sCmd);
            }

            return true;
        }

        private bool checkError()
        {
            if (!File.Exists(sOutFile)) return false;

            using (StreamReader r = new StreamReader(sOutFile))
            {
                sErrorText = r.ReadToEnd();
            }

            if (sErrorText.Contains("Programming and verification completed successfully")) return true;

            return false;

        }

        public bool Flash(string ltef, string cpu)
        {
            if (sTool.Length == 0) sTool = getFlashTool();
            sHexFileName = getHexFileName(ltef);

            string sCPU=cpu;

            if (cpu.Length==0)
            {
                if (cpu.StartsWith("LTE-1006")) sCPU = "atmega328p";
                else if (cpu.StartsWith("LTE-1013")) sCPU = "atxmega32a4";
            }
            createFlashFile(sCPU);

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents=false;
            proc.StartInfo.FileName="flash.bat";
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            proc.WaitForExit(40000);

            bool ret=checkError();

            if (ret) System.Media.SystemSounds.Asterisk.Play();
            else System.Media.SystemSounds.Exclamation.Play();

            return ret;
        }
    }
}
