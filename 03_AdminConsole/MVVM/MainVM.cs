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
using LightLife;

namespace LightLifeAdminConsole.MVVM
{
    class MainVM //: NotifyPropertyChanged
    {
        //Singleton
        private static MainVM _instance;
        public MainWindow MainWin { get; private set; }

        private SqlConnection sqlCon;
        public LinkGroupCollection NewMenuLinkGroups { get; private set; }
        public LinkGroupCollection OldMenuLinkGroups { get; set; }
        private string pagesVisible;
        public ConsoleLogin login;

        private MainVM(LinkGroupCollection pLG, MainWindow mw)
        {
            this.NewMenuLinkGroups = new LinkGroupCollection();
            OldMenuLinkGroups = pLG;
            MainWin = mw;
            
            Settings ini = Settings.GetInstance();
            pagesVisible = ini.ReadString("Pages", "PagesVisible", "");

            SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();
            sqlsb.DataSource = ini.ReadString("Database", "DataSource", "");
            sqlsb.InitialCatalog = ini.ReadString("Database", "InitialCatalog", "");
            sqlsb.UserID = ini.ReadString("Database", "UserID", "");
            sqlsb.Password = ini.ReadString("Database", "Password", "");

            sqlCon = new SqlConnection(sqlsb.ConnectionString);

            login = new ConsoleLogin(sqlCon);
        }

        public static MainVM GetInstance(LinkGroupCollection pLG, MainWindow mw)
        {
            if (_instance == null)
                _instance = new MainVM(pLG, mw);

            return _instance;
        }

        public static MainVM GetInstance()
        {
            return _instance;
        }

        public LinkGroupCollection UpdateMenu(bool loggedIn)
        {
            this.NewMenuLinkGroups.Clear();
            if (loggedIn)
            {
                return DisplayMenuItems(pagesVisible);
            }
            else
                login.IsLoggedIn = false;

            //return DisplayMenuItems(pagesVisible);
            return DisplayMenuItems("login");
       }

       public LinkGroupCollection DisplayMenuItems(string list)
       {
            string[] pageVisible = list.Split(';');

            if (OldMenuLinkGroups == null) return NewMenuLinkGroups;

            this.NewMenuLinkGroups.Clear();

            for (int k = 0; k < pageVisible.GetLength(0); k++)
            {
                for (int j = 0; j < OldMenuLinkGroups.Count; j++)
                {
                    if (pageVisible[k].ToLower() == OldMenuLinkGroups[j].DisplayName.ToLower())
                    {
                        NewMenuLinkGroups.Add(OldMenuLinkGroups[j]);
                    }
                }
            }

            return NewMenuLinkGroups;
       }

        public void Login(string uname, string pwd)
        {
            if (login.CheckUser(uname, pwd))
            {
                MainWin.MenuLinkGroups = UpdateMenu(true);
            }
            else
                throw new ArgumentException("Wrong Username/Password!");

       }
    }
}
