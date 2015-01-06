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
    public class DataItem
    {
        public string RowDescription { get; set; }
        public string ArtNo { get; set; }
        public string Desc { get; set; }
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Qty { get; set; }
    }

    class FlashViewModel : ObservableObject
    {
        private SAPData sap;
        private  FlashAtmel Atmel;
        private LTCSData ltcs;

        public SAPArticle Fert { get { return sap.Fert; } }
        public SAPArticle LTL { get { return sap.LTL; } }
        public SAPArticle LTE { get { return sap.LTE; } }
        public SAPArticle LTEF { get { return sap.LTEF; } }
        public Dictionary<string, SAPArticle> Labels { get { return sap.Labels; } }

        private int _flashedQty;
        public int flashedQty { get { return _flashedQty; } set { _flashedQty = value; RaisePropertyChanged("flashedQty"); } }

        public string ErrorText { get { return Atmel.ErrorText; } private set { ErrorText = value; RaisePropertyChanged("ErrorText"); } }

        private bool? _WasFlashOK;
        public bool? WasFlashOK { get { return _WasFlashOK; } private set { _WasFlashOK = value; RaisePropertyChanged("WasFlashOK"); RaisePropertyChanged("bgColor"); } }

        public string FAUF { get; set; }

        public Brush bgColor
        {
            get
            {                
                if (_WasFlashOK == true) return Brushes.Green;
                if (_WasFlashOK == false) return Brushes.Red;
                return Brushes.LightGray;
            }
        }

        public FlashViewModel()
        {
            sap = new SAPData();
            Atmel = new FlashAtmel();
            ltcs = new LTCSData();
            flashedQty = 0;
            WasFlashOK = null;
            FAUF = "";
        }

        private ICommand _FlashCommand;
        public ICommand FlashCommand
        {
            get
            {
                if (_FlashCommand == null)
                {
                    _FlashCommand = new RelayCommand(() => this.Flash(),()=> sap.LTEF.Nr.Length > 0);
                }
                return _FlashCommand;
            }
        }

        private ICommand _getDataCommand;
        public ICommand getDataCommand
        {
            get
            {
                if (_getDataCommand == null)
                {
                    //_getDataCommand = new RelayCommand<string>(param => this.getData(this.FAUF), param => (FAUF.Length>0));
                    _getDataCommand = new RelayCommand<string>(param => this.getData(this.FAUF), param => (FAUF.Length > 0));
                }
                return _getDataCommand;
            }
        }

        public bool? Flash()
        {
            WasFlashOK = null;

            try
            {
                if (sap.Fert.Nr.Length == 0) return WasFlashOK;


                WasFlashOK = this.Atmel.Flash(sap.Fert.Nr, sap.LTEF.UProp1);
                this.ErrorText = Atmel.ErrorText;

                if (WasFlashOK== false)
                    SystemSounds.Beep.Play();
                else
                    SystemSounds.Asterisk.Play();


                if (sap.isFauf)
                    flashedQty = ltcs.updFlashedQty(sap.FAUF);
            }
            catch (Exception ex)
            {
                SystemSounds.Beep.Play();
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }

            return WasFlashOK;
        }

        public bool getData(string input)
        {
            bool ret = false;
            
            try
            {
                sap.Fert.setEmpty();

                if (input == null) return ret;
                if (input.Length == 0) return ret;

                ret = this.sap.getData(input);

                if (sap.LTE.Nr.Length == 0)
                    ModernDialog.ShowMessage("No LTE found!", "Error", MessageBoxButton.OK);

                return ret;
            }
            catch (Exception ex)
            {
                SystemSounds.Beep.Play();
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }

            return ret;
        }
    }
}
