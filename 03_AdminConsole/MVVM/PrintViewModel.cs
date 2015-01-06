using System;
using System.Windows;
using System.Media;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using MvvmFoundation.Wpf;
using FirstFloor.ModernUI.Windows.Controls;
using Lumitech;

namespace PILEDTestSuite.MVVM
{
    class PrintViewModel : ObservableObject
    {
        private SAPData sap;
        private  LabelPrint label;
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        public SAPArticle Fert { get { return sap.Fert; } }
        public SAPArticle LTL { get { return sap.LTL; } }
        public SAPArticle LTE { get { return sap.LTE; } }
        public SAPArticle LTEF { get { return sap.LTEF; } }
        public Dictionary<string, SAPArticle> Labels { get { return sap.Labels; } }

        public string FAUF { get; set; }
        public int cntLabels { get; set; }
        public int cntrStart { get; set; }

        public PrintViewModel()
        {
            sap = new SAPData();
            label = new LabelPrint();

            FAUF = "";
            cntLabels = 1;
            cntrStart = 1;
            SelectedPrinter = label.DefaultPrinter;
        }

        public List<LabelPrinter> Printers { get { return label.Printers; }}
        //public LabelPrinter DefaultPrinter { get { return label.DefaultPrinter; }   set { label.DefaultPrinter = value; } }
        public LabelPrinter SelectedPrinter { get; set; }

        private ICommand _PrintCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_PrintCommand == null)
                {
                    _PrintCommand = new RelayCommand<SAPArticle>(param => this.Print(param), param => param != null);
                    //_PrintCommand = new RelayCommand<DataItem>((param) => this.Print(param));
                }
                return _PrintCommand;
            }
        }

        private ICommand _getDataCommand;
        public ICommand getDataCommand
        {
            get
            {
                if (_getDataCommand == null)
                {
                    _getDataCommand = new RelayCommand<string>(param => this.getData(param), param => (param.Length > 0));
                }
                return _getDataCommand;
            }
        }

        public bool Print(SAPArticle saparticle)
        {
            bool ret = false;
            try
            {
                if (saparticle == null) 
                {
                    ModernDialog.ShowMessage("No article selected!", "Error", MessageBoxButton.OK);
                    return false;
                }

                if (saparticle.type != SAPArticleType.SAP_LABEL)
                {
                    ModernDialog.ShowMessage("No Label selected!", "Error", MessageBoxButton.OK);
                    return false;
                }


                Mouse.SetCursor(Cursors.Wait);   

                if (SelectedPrinter != null)
                    ret = label.Print(SelectedPrinter, saparticle.Nr, dict);

                Mouse.SetCursor(Cursors.Arrow);
            }
            catch (Exception ex)
            {
                SystemSounds.Beep.Play();
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }

            return ret;
        }

        public bool getData(string input)
        {
            bool ret = false;
            
            try
            {               
                if (input == null) return ret;
                if (input.Length == 0) return ret;

                sap.Fert.setEmpty();
                dict.Clear();

                ret = this.sap.getData(input);
                
                if (sap.Fert.Nr.Length == 0)
                    ModernDialog.ShowMessage("No Article found!", "Error", MessageBoxButton.OK);

                AddToDictionary();

                return ret;
            }
            catch (Exception ex)
            {
                SystemSounds.Beep.Play();
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }

            return ret;
        }

        private void AddToDictionary()
        {
            //dict.Clear();
            dict.Add("FAUF", sap.FAUF);
            dict.Add("ArtNr", sap.Fert.Nr);
            dict.Add("ArtBez", sap.Fert.Description);
            dict.Add("UProp1", sap.Fert.UProp1);
            dict.Add("UProp2", sap.Fert.UProp2);
            dict.Add("Prop1", sap.Fert.UProp1);
            dict.Add("Prop2", sap.Fert.UProp2);

            dict.Add("LTEF", sap.LTEF.Nr);
            dict.Add("LTE", sap.LTE.Nr);

            dict.Add("LabelQty", cntLabels.ToString());
            dict.Add("CntrStart", cntrStart.ToString());

            dict.Add("ProdDate", String.Format("mm.dd.yyyy", DateTime.Now));
        }
    }
}
