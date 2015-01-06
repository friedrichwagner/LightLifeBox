using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Lumitech.Helpers;

namespace Lumitech
{
    public class LabelPrinter
    {
        public int ID {get; set;}
        public string Printername { get; set; }
        public string PrinterIP { get; private set; }
        public int xOffset { get; private set; }
        public int yOffset { get; private set; }
        //public bool defaultPrinter { get; set; }

        public LabelPrinter(int pID, string iniFileString)
        {
            ID = pID;

            string[] arrProp = iniFileString.Split(';');
            if (arrProp.Length == 4)
            {
                Printername = arrProp[0];
                PrinterIP = arrProp[1];
                xOffset = Int32.Parse(arrProp[2]);
                yOffset = Int32.Parse(arrProp[3]);
                //defaultPrinter = false;
            }
            else
                throw new ArgumentException("Ivalid number of Paremeters for LabelPrinter!");

            //if (defPrinter == Printername)  defaultPrinter = true;
        }
    }

    public class LabelPrint
    {
        private List<LabelPrinter> _Printers = new List<LabelPrinter>();
        public List<LabelPrinter> Printers { get { return _Printers; } }

        private string sTemplate = String.Empty;
        private string sTemplatePath = String.Empty;
        private StringBuilder sActualLabel;
        private const int IPPort = 9100;
        private string sOutFile = "label.zpl";

        public LabelPrinter DefaultPrinter { get; set; }

        /*public LabelPrinter DefaultPrinter 
        {
            get
            {
                int i = _Printers.FindIndex(p => (p.defaultPrinter == true));
                if (i >= 0)
                    return _Printers[i];
                else
                    return null;
            }

            set
            {
                for (int i = 0; i < _Printers.Count; i++)
                {
                    if (value.ID == _Printers[i].ID) _Printers[i].defaultPrinter = true;
                    else _Printers[i].defaultPrinter = false;
                }
            }
        }*/

        public LabelPrint()
        {                       
            Settings ini = Settings.GetInstance();

            sTemplatePath = ini.Read<string>("Labels", "Templates", "");

            int i = 1;
            string prop = ini.Read<string>("LabelPrinters", "Printer" + i.ToString(), "");
            string defPrinter = ini.Read<string>("LabelPrinters", "DefaultPrinter", "");

            while (prop.Length > 0)
            {
                _Printers.Add(new LabelPrinter(i, prop));
                if (defPrinter == "Printer"+i.ToString())
                    DefaultPrinter = _Printers[i - 1];

                i++;
                prop = ini.Read<string>("LabelPrinters", "Printer" + i.ToString(), "");
            }
        }

        public bool Print(LabelPrinter p, string LabelNr, Dictionary<string, string> data)
        {
            sActualLabel = new StringBuilder(getTemplate(LabelNr));

            //SelectedPrinterIndex = liPrinters.FindIndex(p => (p.Printername == PrinterName));
            if (p == null) 
                throw new ArgumentOutOfRangeException("No Printer selected!\r\n");

            try
            {
                data.Add("xshift", p.xOffset.ToString());
                data.Add("yshift", p.yOffset.ToString());
            }
            catch (ArgumentException)
            {
                //if it already exists in the dictionary --> just update
                data["xshift"] = p.xOffset.ToString();
                data["yshift"] = p.yOffset.ToString();
            }

            setData(data);
            SendToPrinter(p);

            return true;
        }

        private bool SendToPrinter(LabelPrinter p)
        {
            bool isIP = false;
            IPAddress address;

            try
            {
                address = IPAddress.Parse(p.PrinterIP);
                isIP = true;
            }
            catch
            {
                //Keine IPAdresse
                isIP = false;
            }

            if (isIP)
            {
                using (TcpClient c = new TcpClient(p.PrinterIP, IPPort))
                {
                    Stream inOut = c.GetStream();
                    byte[] array = Encoding.ASCII.GetBytes(sActualLabel.ToString());

                    inOut.Write(array, 0, array.Length);
                    c.Close();
                }
            }
            else
            {
                RawPrinterHelper.SendStringToPrinter(p.PrinterIP, sActualLabel.ToString());
            }

            return true;
        }

        private string getTemplate(string labelnr)
        {
            string ret = "";
            string[] files = Directory.GetFiles(sTemplatePath, labelnr + "*.zpl");
            if (files.Length == 0)
                throw new ArgumentException("No Label Template found!\r\n" + sTemplatePath + "\r\n" + labelnr);

            if (files.Length == 1)
            {
                using (StreamReader s = new StreamReader(files[0]))
                {
                   ret = s.ReadToEnd();
                }
                return ret;
            }

            throw new ArgumentException("More than 1 Template found!\r\n" + sTemplatePath + "\r\n" + labelnr);
        }

        private void setData(Dictionary<string, string> d)
        {
            foreach (var pair in d)
            {
                sActualLabel.Replace("%" + pair.Key + "%", pair.Value);
            }

            ZPLReplace();

            using (StreamWriter s = new StreamWriter(sOutFile))
            {
                s.WriteLine(sActualLabel);
            }
        }

        private void ZPLReplace()
        {
            if (sActualLabel.ToString().Contains("^FXVITA"))
            {
                sActualLabel.Replace("ä", "\\84");
                sActualLabel.Replace("Ä", "\\8E");

                sActualLabel.Replace("ö", "\\94");
                sActualLabel.Replace("Ö", "\\99");

                sActualLabel.Replace("ü", "\\84");
                sActualLabel.Replace("Ü", "\\8E");

                sActualLabel.Replace("ß", "\\E1");
                sActualLabel.Replace("°", "");
            }
            else
            {
                sActualLabel.Replace("ä", "\\7B");
                sActualLabel.Replace("Ä", "\\5B");

                sActualLabel.Replace("ö", "\\7C");
                sActualLabel.Replace("Ö", "\\5C");

                sActualLabel.Replace("ü", "\\7D");
                sActualLabel.Replace("Ü", "\\5D");

                sActualLabel.Replace("ß", "\\7E");
                sActualLabel.Replace("°", "\\F8");
                sActualLabel.Replace("®", "\\A9");
            }
        }
    }
}
