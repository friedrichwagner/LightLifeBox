using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumitech.Helpers;
using MvvmFoundation.Wpf;
using FirstFloor.ModernUI.Windows.Controls;
using LightLifeAdminConsole.Data;
using System.Windows;
using System.Windows.Media;
using System.Data;
using System.Windows.Threading;

namespace LightLifeAdminConsole.MVVM
{


    class PracticeBoxVM: ObservableObject
    {
        private Box2 _box;
        public Box2 box { get { return _box; } }
        public DeltaTest dTest;

        public IDictionary<int, string> lldeltatestmode { get { return LLSQL.lldeltatestmode; } }

        private IDictionary<int, string> _lltestcct;
        public IDictionary<int, string> lltestcct { get { return _lltestcct; } }
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();  

        private string _errorText;
        public string ErrorText 
        {
            get { return _errorText;  }
            set { 
                _errorText = value;
                ModernDialog.ShowMessage(_errorText, "Error", MessageBoxButton.OK);
            }
        }

        private string _infoText;
        public string InfoText
        {
            get { return _infoText; }
            set
            {
                _infoText = value;
                ModernDialog.ShowMessage(_infoText, "Info", MessageBoxButton.OK);
            }
        }


        public int Proband
        {
            get { return dTest.Proband; }
            set
            {
                dTest.Proband = value;
                RaisePropertyChanged("Proband");
                RaisePropertyChanged("BtnStartEnabled");
            }
        }

        public int SelectedCCT
        {
            get { return dTest.CCT; }
            set
            {

                dTest.CCT = value;
                RaisePropertyChanged("SelectedCCT");
                RaisePropertyChanged("BtnStartEnabled");
            }
        }

        public int SelectedTestMode
        {
            get { return (int)dTest.TestMode; }
            set
            {
                dTest.TestMode = (TestMode)value;
                RaisePropertyChanged("SelectedTestMode");
                RaisePropertyChanged("BtnStartEnabled");
            }
        }

        public int Brightness
        {
            get { return dTest.Brightness; }
            set
            {
                dTest.Brightness = value;
                RaisePropertyChanged("Brightness");
                RaisePropertyChanged("BtnStartEnabled");
            }
        }

        public bool BtnStartEnabled { get { return BtnEnabled(BoxUIButtons.START); } }
        public bool BtnStopEnabled { get { return BtnEnabled(BoxUIButtons.STOP); } }

        public bool IsBusy
        {
            get { return dTest.isRunning; }
        }
        public TimeSpan TimeElapsed { get; private set; }

        public string TestResult { get { return dTest.Result; } }

        private ICommand _doDeltaTestCommand;
        public ICommand doDeltaTestCommand
        {
            get
            {
                if (_doDeltaTestCommand == null)
                {
                    _doDeltaTestCommand = new RelayCommand<string>(param => this.doDeltaTest(param), param => (Proband >0));
                }
                return _doDeltaTestCommand;
            }
        }

        public PracticeBoxVM(Box2 box)
        {
            try
            {
                _box = box;
                _lltestcct = new Dictionary<int, string>();
                _lltestcct.Add(0, "");
                _lltestcct.Add(4000, "4000K");

                TimeElapsed = TimeSpan.Zero;

                dTest = new DeltaTest(box);
                box.dTest = dTest;
                dTest.DeltaTestEvent += DeltaTestEventHandler;

                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1); 
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private void DeltaTestEventHandler(DeltaTestCommand cmd, TestMode mode)
        {
            switch (cmd)
            {
                case DeltaTestCommand.START:
                    TimeElapsed = TimeSpan.Zero;
                    dispatcherTimer.Start();
                    break;
                case DeltaTestCommand.STOP:
                    dispatcherTimer.Stop();
                    break;
                case DeltaTestCommand.SAVE:
                    dispatcherTimer.Stop();
                    break;
            }

            RaisePropertyChanged("BtnStartEnabled");
            RaisePropertyChanged("BtnStopEnabled");
            RaisePropertyChanged("TestResult");
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            TimeElapsed = TimeElapsed.Add(TimeSpan.FromSeconds(1));

            RaisePropertyChanged("IsBusy");
            RaisePropertyChanged("TimeElapsed");
        }

        private void doDeltaTest(string cmd)
        {
            try
            {
                switch (cmd)
                {
                    case "START":
                        dTest.Start();
                        break;
                    case "STOP":
                        dTest.Stop();
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private bool BtnEnabled(BoxUIButtons btn)
        {
            if (!_box.IsActive) return false;

            switch (btn)
            {
                case BoxUIButtons.START:
                    if (dTest.CanStart() && !dTest.isRunning) return true;
                    break;
                case BoxUIButtons.STOP:
                    if (dTest.isRunning) return true;
                    break;
            }
            return false;
        }
    }
}
