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
    enum BoxUIButtons { START, STOP, PREV, NEXT, PAUSE, UPDATE};

    class BoxVM: ObservableObject
    {
        private static BoxVM _instance;
        private Settings ini;
        public IDictionary<int, Box> boxes;

        public int SequenceID
        {
            get {
                if (SelectedBox > 0) return boxes[SelectedBox].SequenceID;
                else return -1;
            }
            /*set {
                ReloadSequence(value);                
            }*/
        }

        public bool IsBusy { get; private set; } //Display Wait Cursor

        public int StepID
        {
            get {
                if (SelectedBox > 0) return boxes[SelectedBox].StepID;
                else return -1;
            }
        }

        private AdminBase _testSequencePos;
        public DataView TestSequencePos 
        {
            get {
                if (SelectedBox > 0)
                    return _testSequencePos.select(" where SequenceID=" + boxes[SelectedBox].SequenceID.ToString()).DefaultView;
                else return null;
            }
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


        public int cntBoxes { get { return boxes.Count; } }

        private int _selectedBox;
        public int SelectedBox
        {
            get { return _selectedBox; }
            set
            {
                if (value > 0 && value <= cntBoxes)
                {
                    _selectedBox = value;
                    RaisePropertyChanged("SelectedProband");
                    RaisePropertyChanged("SelectedRemark");
                    RaisePropertyChanged("TestSequencePos");
                    RaisePropertyChanged("SequenceID");
                    try
                    {
                        IsBusy = true;
                        boxes[SelectedBox].Ping();
                        IsBusy = false;
                        if (boxes[SelectedBox].IsActive == false)
                            InfoText = "Box #" + _selectedBox.ToString() + " is inactive!";
                    }
                    catch(Exception ex)
                    {
                        IsBusy = false;
                        ErrorText = ex.Message;
                    }
                }
            }
        }

        public IDictionary<int, string> probanden { get { return LLSQL.probanden; } }

        private int _selectedProband;
        public int SelectedProband
        {
            get {
                if (_selectedProband > -1)
                    return boxes[SelectedBox].ProbandID;
                else
                    return _selectedProband;
            }
            set
            {
                if (value > -1)
                {
                    boxes[SelectedBox].ProbandID = value;                    
                }

                _selectedProband = value;
                RaisePropertyChanged("SelectedProband");
                RaisePropertyChanged("BtnStartEnabled");
            }
        }

        private string _selectedRemark;
        public string SelectedRemark
        {
            get
            {
                if (_selectedProband > -1)
                    return boxes[SelectedBox].Remark;
                else
                    return _selectedRemark;
            }
            set
            {
                if (_selectedProband > -1)
                    boxes[SelectedBox].Remark = value;
                _selectedRemark = value;
                RaisePropertyChanged("SelectedRemark");
            }
        }

        public bool BtnStartEnabled { get { return BtnEnabled(BoxUIButtons.START); } }
        public bool BtnStopEnabled  { get { return BtnEnabled(BoxUIButtons.STOP); } }
        public bool BtnPauseEnabled { get { return BtnEnabled(BoxUIButtons.PAUSE); } }
        public bool BtnPrevEnabled  { get { return BtnEnabled(BoxUIButtons.PREV); } }
        public bool BtnNextEnabled  { get { return BtnEnabled(BoxUIButtons.NEXT); } }
        public bool BtnUpdateEnabled { get { return BtnEnabled(BoxUIButtons.UPDATE); } }

           
        public static BoxVM GetInstance()
        {
            if (_instance == null)
                _instance = new BoxVM();

            return _instance;
        }

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

        private BoxVM()
        {
            try
            {
                ini = Settings.GetInstance();

                boxes = new Dictionary<int, Box>();
                getBoxes(ref boxes);
                _selectedProband = -1;
                _testSequencePos = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequencePos"]);
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private void getBoxes(ref IDictionary<int, Box> b)
        {
            int n=ini.Read<int>("ControlBox", "NrOfBoxes", 4);
            for (int i=1; i <= n; i++)
            {
                b.Add(i, new Box(i));
            }
        }

        private void doSequence(string cmd)
        {
            try
            {
                if (cmd.ToUpper() == "START") boxes[SelectedBox].StartSequence();
                if (cmd.ToUpper() == "PAUSE") boxes[SelectedBox].PauseSequence();
                if (cmd.ToUpper() == "STOP") boxes[SelectedBox].StopSequence();
                if (cmd.ToUpper() == "PREV") boxes[SelectedBox].PrevStep();
                if (cmd.ToUpper() == "NEXT") boxes[SelectedBox].NextStep();
                if (cmd.ToUpper() == "UPDATE") boxes[SelectedBox].UpdateRemark(SelectedRemark);

                RaisePropertyChanged("BtnStartEnabled");
                RaisePropertyChanged("BtnStopEnabled");
                RaisePropertyChanged("BtnPrevEnabled");
                RaisePropertyChanged("BtnNextEnabled");
                RaisePropertyChanged("BtnPauseEnabled");
                RaisePropertyChanged("BtnUpdateEnabled");                
                RaisePropertyChanged("TestSequencePos");
                RaisePropertyChanged("SequenceID");
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        public void RefreshPos()
        {
            RaisePropertyChanged("TestSequencePos");
        }

        private bool BtnEnabled(BoxUIButtons btn)
        {
            if (SelectedBox < 1) return false;
            if (!boxes[SelectedBox].IsActive) return false;

            switch (btn)
            {
                case BoxUIButtons.START:
                    if (boxes[SelectedBox].State == BoxStatus.STOPPED || boxes[SelectedBox].State == BoxStatus.NONE || boxes[SelectedBox].State == BoxStatus.PAUSED) return true; 
                        else return false;

                case BoxUIButtons.STOP:
                        if (boxes[SelectedBox].State == BoxStatus.STARTED) return true;
                        else return false;

                case BoxUIButtons.PREV:
                        if (boxes[SelectedBox].State == BoxStatus.STARTED && boxes[SelectedBox].StepID > 1) return true; 
                        else return false;

                case BoxUIButtons.NEXT:
                         if (boxes[SelectedBox].State == BoxStatus.STARTED && boxes[SelectedBox].StepID < 4) return true; 
                         else return false; 

                case BoxUIButtons.PAUSE:
                         if (boxes[SelectedBox].State == BoxStatus.STARTED) return true; 
                         else return false; 

                case BoxUIButtons.UPDATE:
                         //if (boxes[SelectedBox].State == BoxStatus.STARTED) return true; 
                         //else 
                        //kann immer upgedated werden
                         return true;
            }

            return false;
        }

        public void ReloadSequence(int seqID)
        {
            try
            {
                Box newBox = Box.ReloadSequence(seqID, ref boxes);

                boxes[newBox.BoxNr] = newBox;
                
                _selectedBox = newBox.BoxNr;
                _selectedProband = newBox.ProbandID;
                _selectedRemark = newBox.Remark;

                if (boxes[newBox.BoxNr].State == BoxStatus.FINISHED)
                    ErrorText = "TestSequence already finished!";

                RaiseAllProperties();
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
            RaisePropertyChanged("TestSequencePos");
            RaisePropertyChanged("BtnStartEnabled");
            RaisePropertyChanged("BtnStopEnabled");
            RaisePropertyChanged("BtnPrevEnabled");
            RaisePropertyChanged("BtnNextEnabled");
            RaisePropertyChanged("BtnPauseEnabled");
            RaisePropertyChanged("BtnUpdateEnabled");
            RaisePropertyChanged("TestSequencePos");
            RaisePropertyChanged("SequenceID");
            RaisePropertyChanged("StepID");
        }
    }
}
