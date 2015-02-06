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
        public SqlConnection sqlCon;


        private AdminVM()
        {           
            Settings ini = Settings.GetInstance();

            SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();
            sqlsb.DataSource = ini.ReadString("Database", "DataSource", "");
            sqlsb.InitialCatalog = ini.ReadString("Database", "InitialCatalog", "");
            sqlsb.UserID = ini.ReadString("Database", "UserID", "");
            sqlsb.Password = ini.ReadString("Database", "Password", "");
            sqlCon = new SqlConnection(sqlsb.ConnectionString);
        }

        public static AdminVM GetInstance(LinkGroupCollection pLG, MainWindow mw)
        {
            if (_instance == null)
                _instance = new AdminVM();

            return _instance;
        }

        public static AdminVM GetInstance()
        {
            return _instance;
        }
    }
}
