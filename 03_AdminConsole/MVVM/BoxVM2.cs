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
using System.Data;
using System.Windows.Threading;
using System.Windows.Media;

namespace LightLifeAdminConsole.MVVM
{
    enum BoxUIButtons { START, STOP, PREV, NEXT, PAUSE, UPDATE, SAVENEW, DELTATEST};
    //enum BoxUIButtons { START, STOP, PAUSE, UPDATE };

    class BoxVM2: ObservableObject
    {
        private Box2 _box;
        public Box2 box { get { return _box; } }

        public int SelectedBox { get { return _box.BoxNr; } }

        public int SequenceID
        {
            get { return _box.testsequence.SequenceID; }
            /*set {
                ReloadSequence(value);                
            }*/
        }

        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        //private bool _IsBusy;
        public bool IsBusy 
        {
            get
            {
                if (_box.testsequence.State > TestSequenceState.NONE && _box.testsequence.State < TestSequenceState.STOPPED) return true;
                else return false;
            }

        }

        public Brush BoxBackgroundColor
        {
            get
            {
                Brush ret = Brushes.White;
                switch (_box.testsequence.State)
                {
                    case TestSequenceState.NONE: ret = Brushes.White; break;
                    case TestSequenceState.IN_PROGRESS: ret = Brushes.Beige; break;
                    case TestSequenceState.PAUSED: ret = Brushes.LightGray; break;
                    case TestSequenceState.STOPPED: ret = Brushes.LightGreen; break;
                    case TestSequenceState.FINISHED: ret = Brushes.DarkGray; break;
                }

                return ret;
            }
        }

        
        private AdminBase _testSequencePos;
        public DataView TestSequencePos 
        {
            get { return _testSequencePos.select(" where SequenceID=" + _box.testsequence.SequenceID.ToString()+" order by PosID").DefaultView; }
        }

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

        public int cntBoxes { get { return Box2.boxes.Count; } }

        public IDictionary<int, string> probanden { get { return LLSQL.probanden; } }
        public IDictionary<int, string> llactivationstate { get { return LLSQL.llactivationstate; } }
        public IDictionary<string, string> lltestsequencedef { get { return LLSQL.lltestsequencedef; } }
        //public DataTable llstep { get { return LLSQL.llstep; } }

        private int _selectedProband;
        public int SelectedProband
        {
            get { return _box.testsequence.ProbandID; }
            set
            {
                if (value > -1)
                {
                    _box.testsequence.ProbandID = value;                    
                }

                _selectedProband = value;
                RaisePropertyChanged("SelectedProband");
                RaisePropertyChanged("BtnSaveNewEnabled");
            }
        }

        private string _selectedRemark;
        public string SelectedRemark
        {
            get { return _box.testsequence.Remark; }
            set
            {

                _box.testsequence.Remark = value;
                _selectedRemark = value;
                RaisePropertyChanged("SelectedRemark");
            }
        }

        private int _selectedActivationState;
        public int SelectedActivationState
        {
            get  { return _box.testsequence.ActivationID;  }
            set
            {
                _box.testsequence.ActivationID = value;
                _selectedActivationState = value;
                RaisePropertyChanged("SelectedActivationState");
                RaisePropertyChanged("BtnSaveNewEnabled");
            }
        }

        private int _selectedStep;
        public int SelectedStep
        {
            get { return (int)_box.testsequence.StepID; }
            set
            {
                _box.testsequence.StepID = (TestSequenceStep)value;
                    _selectedStep = value;
                    RaisePropertyChanged("SelectedStep");
            }
        }
        private string _selectedSequenceDef;
        public string SelectedSequenceDef
        {
            get { return _box.testsequence.SequenceDef; }
            set
            {
                _box.testsequence.SequenceDef = value;
                _selectedSequenceDef = value;
                RaisePropertyChanged("SelectedSequenceDef");
                RaisePropertyChanged("BtnSaveNewEnabled");
            }
        }

        public bool BtnStartEnabled { get { return BtnEnabled(BoxUIButtons.START); } }
        public bool BtnStopEnabled  { get { return BtnEnabled(BoxUIButtons.STOP); } }
        public bool BtnPauseEnabled { get { return BtnEnabled(BoxUIButtons.PAUSE); } }
        public bool BtnUpdateEnabled { get { return BtnEnabled(BoxUIButtons.UPDATE); } }
        public bool BtnPrevEnabled { get { return BtnEnabled(BoxUIButtons.PREV); } }
        public bool BtnNextEnabled { get { return BtnEnabled(BoxUIButtons.NEXT); } }
        public bool BtnSaveNewEnabled { get { return BtnEnabled(BoxUIButtons.SAVENEW); } }

        public bool TextBoxesEnabled { get { return SequenceID == 0; } }              
           
        private ICommand _doSequenceCommand;
        public ICommand doSequenceCommand
        {
            get
            {
                if (_doSequenceCommand == null)
                {
                    _doSequenceCommand = new RelayCommand<string>(param => this.doSequence(param), param => (SelectedProband>-1));
                }
                return _doSequenceCommand;
            }
        }

        public BoxVM2(Box2 box)
        {
            try
            {
                _box = box;
                _selectedProband = -1;
                _selectedActivationState = -1;
                _selectedStep = -1;
                _testSequencePos = new AdminBase(LLSQL.sqlCon, LLSQL.tables["VLLTestSequence"]);

                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1); 

                if (IsBusy)
                    dispatcherTimer.Start();

                //RaiseAllProperties();
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            RaisePropertyChanged("IsBusy");

            if (!IsBusy)
            {
                dispatcherTimer.Stop();
                RaisePropertyChanged("BtnStartEnabled");
            }
        }

        private void doSequence(string cmd)
        {
            try
            {
                switch (cmd.ToUpper())
                {
                    case "START": _box.testsequence.Start(); dispatcherTimer.Start();   break;
                    case "PAUSE": _box.testsequence.Pause();                            break;
                    case "STOP": _box.testsequence.Stop(); dispatcherTimer.Stop(); RaisePropertyChanged("IsBusy"); break;
                    case "UPDATE": _box.testsequence.UpdateRemark(SelectedRemark);      break;
                    case "REFRESH": _box.Refresh();                                     break;
                    case "SAVENEW": _box.testsequence.SaveNew();                        break;
                    case "PREV": _box.testsequence.Prev();                              break;
                    case "NEXT": _box.testsequence.Next();                              break;
                    //case "DELTATEST": _box.StartDeltaTest(); break;
                }
                
                RaiseAllProperties();
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private bool BtnEnabled(BoxUIButtons btn)
        {
            //TEST TEST
            //return true;
            //if (!_box.IsActive) return false;
            return _box.testsequence.btnEnabled(btn);
        }

        public void ReloadSequence(int seqID)
        {
            try                
            {       
       
              _box.ReloadSequence(seqID);

                RaiseAllProperties();
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;

                //DAmit wieder die richtige SequenceID im TextFeld steht
                RaisePropertyChanged("SequenceID");
            }

        }

        private void RaiseAllProperties()
        {
            RaisePropertyChanged("SelectedProband");
            RaisePropertyChanged("SelectedRemark");
            RaisePropertyChanged("SelectedActivationState");
            RaisePropertyChanged("SelectedStep");
            RaisePropertyChanged("SelectedSequenceDef");

            RaisePropertyChanged("BtnStartEnabled");
            RaisePropertyChanged("BtnStopEnabled");
            RaisePropertyChanged("BtnPauseEnabled");
            RaisePropertyChanged("BtnUpdateEnabled");
            RaisePropertyChanged("BtnSaveNewEnabled");
            RaisePropertyChanged("BtnPrevEnabled");
            RaisePropertyChanged("BtnNextEnabled");
            RaisePropertyChanged("TextBoxesEnabled");
           
            RaisePropertyChanged("SequenceID");
            RaisePropertyChanged("TestSequencePos");
            RaisePropertyChanged("BoxBackgroundColor");
        }
    }
}
