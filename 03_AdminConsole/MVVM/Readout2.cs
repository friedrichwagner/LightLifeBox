using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using MvvmFoundation.Wpf;
using Lumitech.Helpers;

namespace Lumitech
{
    public enum enumLabelType { ltLTS = 2000, ltLTE = 2003, ltLTF = 9999 };

    public class Readout2 : ObservableObject
    {
        public static Readout2 _instance;

        private SAPData _sap;
        public SAPData sap { get { return _sap; } }

        private LTCSData _ltcs;
        public DataItemReadout data { get { return _ltcs.data; } }

        private LabelPrint label;
        Dictionary<string, string> dict = new Dictionary<string, string>();
        MMSmall mm2;
        string TemplateExelExport = string.Empty;
        private BackgroundWorker bw1 = new BackgroundWorker();

        public List<LabelPrinter> Printers { get { return label.Printers; } }
        public LabelPrinter DefaultPrinter
        {
            get { return label.DefaultPrinter; }
            set { label.DefaultPrinter = value; }
        }

        private string _KAUF;
        public string KAUF { get { return _KAUF; } set { _KAUF = value; RaisePropertyChanged("KAUF"); } }

        public static Readout2 GetInstance()
        {
            if (_instance == null)
                _instance = new Readout2();

            return _instance;
        }

        private Readout2()
        {
            _sap = new SAPData();
            _ltcs = new LTCSData();
            label = new LabelPrint();
            mm2 = new MMSmall();
            _KAUF = String.Empty;
        }

        public SqlDataReader getDataCollected(int cntrows)
        {
            return _ltcs.getDataAlreadyCollected(_sap.FAUF, KAUF, cntrows);
        }

        public void Readout()
        {
            try
            {
                mm2.getBatchNR_SerialNr();
                if (mm2.BatchNr == 0) throw new ArgumentException("No BatchNr!");
                if (mm2.SerialNr == 0) throw new ArgumentException("No SerialNr!");
                RaisePropertyChanged("ModuleNr");
                RaisePropertyChanged("SerialNr");

                bw1.DoWork += (obj, li) => bw_DoWork(this, null);
            }
            catch
            {
                throw;
            }
        }

        public bool ReadoutAsync(RunWorkerCompletedEventHandler del)
        {
            if (bw1.IsBusy == true) return false;

            //Das geht halt nur, wenn "del" immer gleich ist
            bw1.RunWorkerCompleted -= del;
            
            //Remove previously added Delegate
            /*EventInfo f = typeof(BackgroundWorker).GetEvent("RunWorkerCompleted");
            //EventInfo[] f1 = typeof(BackgroundWorker).GetEvents();

            if (f != null)
            {
                MethodInfo mi = f.GetRemoveMethod();
                //ParameterInfo[] aParamInfo = mi.GetParameters();
                object[] o = new object[1];
                mi.Invoke(bw1, o);
            }*/

            bw1.RunWorkerCompleted +=del;

            if (bw1.IsBusy != true)
                bw1.RunWorkerAsync();

            return true;
        }

        private void bw_DoWork(object sender, List<object> obj)
        {
            mm2.getBatchNR_SerialNr();

            _ltcs.data.ModuleNr = mm2.BatchNr.ToString();
            _ltcs.data.SerialNr = mm2.SerialNr.ToString();
        }

        public bool Print(LabelPrinter p, DataItemReadout d, enumLabelType labelType)
        {
            //DataItemReadout übergeben, da z.B: beim Reprint andere als aktuelle Daten gedruckt werden sollen
            AddToDictionary(labelType, d);
            int lblNum = (int)labelType;
            label.Print(p, lblNum.ToString(), dict);
            bool ret = _ltcs.updAfterPrint(_sap.FAUF, KAUF, d);
            return ret;
        }

        private void AddToDictionary(enumLabelType labelType, DataItemReadout d)
        {
            dict.Clear();

            dict.Add("LTLFAUF", d.ModuleNr);
            dict.Add("LTLSN", d.SerialNr);            

            if (labelType == enumLabelType.ltLTE)
            {
                dict.Add("ArtNr", _sap.LTE.Nr);
                dict.Add("ArtBez", _sap.LTE.Description);
                dict.Add("UProp1", _sap.LTL.UProp1);
                dict.Add("UProp2", _sap.LTL.UProp2);
            }
            else if (labelType == enumLabelType.ltLTS)
            {
                dict.Add("ArtNr", _sap.Fert.Nr);
                dict.Add("ArtBez", _sap.Fert.Description);
                dict.Add("UProp1", _sap.Fert.UProp1);
                dict.Add("UProp2", _sap.Fert.UProp2);
            }
            else
            {
                dict.Add("ArtNr", _sap.LTL.Nr);
                dict.Add("ArtBez", _sap.LTL.Description);
                dict.Add("UProp1", _sap.LTL.UProp1);
                dict.Add("UProp2", _sap.LTL.UProp2);
            }

            dict.Add("Fauf", _sap.FAUF);
            dict.Add("LTEF", _sap.LTEF.Nr);
            dict.Add("LTE", _sap.LTE.Nr);
            dict.Add("FaufQty", _sap.FaufQty.ToString());

            dict.Add("LabelQty", "1");
            dict.Add("OrderNr", KAUF);
            dict.Add("ProdDate", String.Format("mm.dd.yyyy", DateTime.Now));
        }

        public bool Save()
        {
            bool ret = _ltcs.updAfterTest();
            
            if (ret)
                mm2.ResetBatchNR_SerialNr();

            return ret;
        }
        public SqlDataReader getDatatoExport(string fauf, string OrderNr)
        {
            return _ltcs.getDatatoExport(fauf, OrderNr);
        }

        public void ExportToExcel(DataItemReadout d)
        {
            AddToDictionary(enumLabelType.ltLTS, d);
            SqlDataReader dr = _ltcs.getDatatoExport(_sap.FAUF, KAUF);

            Settings ini = Settings.GetInstance(true);
            TemplateExelExport = ini.Read<string>("ExcelExport", "MM2Template", "");

            if (!Path.IsPathRooted(TemplateExelExport))
                TemplateExelExport = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplateExelExport);

            ExportToExcel xl = new ExportToExcel();
            xl.GenerateReport(TemplateExelExport, dict, dr);
        }

        public bool checkTxtLength(object o, string msg)
        {
            if (o != null)
            {
                if (o is TextBlock)
                {
                    if ((o as TextBlock).Text.Length == 0)
                    {
                        (o as TextBlock).Focus();
                        throw new ArgumentException(msg);

                    }
                    return true;
                }

                if (o is ComboBox)
                {
                    if ((o as ComboBox).Text.Length == 0)
                    {
                        (o as ComboBox).Focus();
                        throw new ArgumentException(msg);

                    }
                    return true;
                }
            }
            return false;
        }
    }
}
