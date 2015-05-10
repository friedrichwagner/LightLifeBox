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

        private bool _IsBusy;
        public bool IsBusy 
        { 
            get { return _IsBusy; }
            set {   _IsBusy = value;
                    RaisePropertyChanged("IsBusy");
            } //Display Wait Cursor
        }
        
        private AdminBase _testSequencePos;
        public DataView TestSequencePos 
        {
            get { return _testSequencePos.select(" where SequenceID=" + _box.testsequence.SequenceID.ToString()).DefaultView; }
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
        public DataTable llstep { get { return LLSQL.llstep; } }

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

        public bool BtnStartEnabled { get { return BtnEnabled(BoxUIButtons.START); } }
        public bool BtnStopEnabled  { get { return BtnEnabled(BoxUIButtons.STOP); } }
        public bool BtnPauseEnabled { get { return BtnEnabled(BoxUIButtons.PAUSE); } }
        public bool BtnUpdateEnabled { get { return BtnEnabled(BoxUIButtons.UPDATE); } }
        public bool BtnPrevEnabled { get { return BtnEnabled(BoxUIButtons.PREV); } }
        public bool BtnNextEnabled { get { return BtnEnabled(BoxUIButtons.NEXT); } }
        public bool BtnSaveNewEnabled { get { return BtnEnabled(BoxUIButtons.SAVENEW); } }
        public bool BtnDeltaTestEnabled { get { return BtnEnabled(BoxUIButtons.DELTATEST); } }
        
           
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

                //RaiseAllProperties();
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ErrorText = ex.Message;
            }
        }


        private void doSequence(string cmd)
        {
            try
            {
                switch (cmd.ToUpper())
                {
                    case "START": _box.testsequence.Start(); break;
                    case "PAUSE": _box.testsequence.Pause(); break;
                    case "STOP": _box.testsequence.Stop(); break;
                    case "UPDATE": _box.testsequence.UpdateRemark(SelectedRemark); break;
                    case "REFRESH": _box.Refresh(); break;
                    case "SAVENEW": _box.testsequence.SaveNew(); break;
                    case "PREV": _box.testsequence.Prev(); break;
                    case "NEXT": _box.testsequence.Next(); break;
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

            if (!_box.IsActive) return false;
            switch (btn)
            {
                case BoxUIButtons.DELTATEST:
                    if (_box.IsPracticeBox) return true;
                    break;
                default:
                    return _box.testsequence.btnEnabled(btn);

            }

            return false;
        }

        public void ReloadSequence(int seqID)
        {
            try                
            {              
            /*   int bnr =  Box2.ReloadSequence(seqID, ref boxes);

                //boxes[newBox.BoxNr] = newBox;

               //_selectedBox = boxes[bnr].BoxNr;
                //_selectedProband = boxes[bnr].ProbandID;
                //_selectedRemark = boxes[bnr].Remark;

               SelectedBox = boxes[bnr].BoxNr;

                if (boxes[bnr].State == BoxStatus.FINISHED)
                    ErrorText = "TestSequence already finished!";

                RaiseAllProperties();*/
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }

        }

        private void RaiseAllProperties()
        {
            RaisePropertyChanged("SelectedProband");
            RaisePropertyChanged("SelectedRemark");
            RaisePropertyChanged("SelectedActivationState");
            RaisePropertyChanged("SelectedStep");

            RaisePropertyChanged("BtnStartEnabled");
            RaisePropertyChanged("BtnStopEnabled");
            RaisePropertyChanged("BtnPauseEnabled");
            RaisePropertyChanged("BtnUpdateEnabled");
           
            RaisePropertyChanged("SequenceID");
            RaisePropertyChanged("TestSequencePos");
        }
    }
}
