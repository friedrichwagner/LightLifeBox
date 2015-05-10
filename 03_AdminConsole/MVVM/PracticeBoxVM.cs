﻿using System;
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


    class PracticeBoxVM: ObservableObject
    {
        private Box2 _box;
        public Box2 box { get { return _box; } }
        public DeltaTest dTest;

        public IDictionary<int, string> lldeltatestmode { get { return LLSQL.lldeltatestmode; } }

        private IDictionary<int, string> _lltestcct;
        public IDictionary<int, string> lltestcct { get { return _lltestcct; } }

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

        private ICommand _doStartDeltaTestCommand;
        public ICommand doStartDeltaTestCommand
        {
            get
            {
                if (_doStartDeltaTestCommand == null)
                {
                    _doStartDeltaTestCommand = new RelayCommand<string>(param => this.doStartDeltaTest(param), param => (dTest.CanStart()));
                }
                return _doStartDeltaTestCommand;
            }
        }

        public PracticeBoxVM(Box2 box)
        {
            try
            {
                _box = box;
                _lltestcct = new Dictionary<int, string>();
                _lltestcct.Add(3000, "3000K");
                _lltestcct.Add(4000, "4000K");
                _lltestcct.Add(5000, "5000K");
                _lltestcct.Add(6000, "6000K");
                _lltestcct.Add(7000, "7000K");

                dTest = new DeltaTest(box);
                box.dTest = dTest;
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }


        private void doStartDeltaTest(string cmd)
        {
            try
            {
                dTest.Start();
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private bool BtnEnabled(BoxUIButtons btn)
        {
            if (!_box.IsActive) return false;
            if (dTest.CanStart()) return true;

            return false;
        }
    }
}
