using System;
using System.Windows;
using System.Media;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using MvvmFoundation.Wpf;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using System.Data.SqlClient;
using Lumitech.Helpers;

namespace LightLife
{
    class AdminVM //: NotifyPropertyChanged
    {
        //Singleton
        private static AdminVM _instance;

        private AdminVM()
        {           
        }

        public static AdminVM GetInstance()
        {
            if (_instance == null)
                _instance = new AdminVM();

            return _instance;
        }
    }
}
