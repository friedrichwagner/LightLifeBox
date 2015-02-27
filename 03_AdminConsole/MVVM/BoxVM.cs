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

namespace LightLifeAdminConsole.MVVM
{
    enum BoxUIButtons { START, STOP, PREV, NEXT, PAUSE, UPDATE};

    class BoxVM: ObservableObject
    {
        private static BoxVM _instance;
        private Settings ini;
        public IDictionary<int, Box> boxes;

        public int cntBoxes { get { return boxes.Count; } }

        private int _selectedBox;
        public int SelectedBox
        {
            get { return _selectedBox; }
            set
            {
                _selectedBox = value;
                RaisePropertyChanged("SelectedProband");
                RaisePropertyChanged("SelectedRemark");
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
            ini = Settings.GetInstance();

            boxes = new Dictionary<int, Box>();
            getBoxes(ref boxes);
            _selectedProband = -1;
        }

        private void getBoxes(ref IDictionary<int, Box> b)
        {
            int n=ini.Read<int>("Pages", "NrOfBoxes", 3);
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
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private bool BtnEnabled(BoxUIButtons btn)
        {
            if (SelectedBox < 1) return false;

            switch (btn)
            {
                case BoxUIButtons.START:
                    if (boxes[SelectedBox].State == BoxStatus.STOPPED || boxes[SelectedBox].State == BoxStatus.NONE || boxes[SelectedBox].State == BoxStatus.PAUSED) return true; 
                        else return false;

                case BoxUIButtons.STOP:
                        if (boxes[SelectedBox].State == BoxStatus.STARTED) return true;
                        else return false;

                case BoxUIButtons.PREV:
                        if (boxes[SelectedBox].State == BoxStatus.STARTED && boxes[SelectedBox].StepID>0) return true; 
                        else return false;

                case BoxUIButtons.NEXT:
                         if (boxes[SelectedBox].State == BoxStatus.STARTED && boxes[SelectedBox].StepID < 3) return true; 
                         else return false; 

                case BoxUIButtons.PAUSE:
                         if (boxes[SelectedBox].State == BoxStatus.STARTED) return true; 
                         else return false; 

                case BoxUIButtons.UPDATE:
                         if (boxes[SelectedBox].State == BoxStatus.STARTED) return true; 
                         else return false;

            }

            return false;

        }
    }
}
