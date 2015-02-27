﻿using System;
using System.Windows;
using FirstFloor.ModernUI.Presentation;
using Lumitech.Helpers;
using LightLifeAdminConsole.Data;
using FirstFloor.ModernUI.Windows.Controls;

namespace LightLifeAdminConsole.MVVM
{
    class MainVM //: NotifyPropertyChanged
    {
        //Singleton
        private static MainVM _instance;
        public MainWindow MainWin { get; private set; }

        private string _errorText;
        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                ModernDialog.ShowMessage(_errorText, "Error", MessageBoxButton.OK);
            }
        }

        public LinkGroupCollection NewMenuLinkGroups { get; private set; }
        public LinkGroupCollection OldMenuLinkGroups { get; set; }
        private string pagesVisible;
        public ConsoleLogin login;

        private string uname;
        private string pwd;

        public BrightnessWindow wndBrightness;

        private MainVM(LinkGroupCollection pLG, MainWindow mw)
        {
            this.NewMenuLinkGroups = new LinkGroupCollection();
            OldMenuLinkGroups = pLG;
            MainWin = mw;
            
            Settings ini = Settings.GetInstance();
            pagesVisible = ini.ReadString("Pages", "PagesVisible", "");

            //Automatic Login ?
            login = new ConsoleLogin(LLSQL.sqlCon);

            uname = ini.ReadString("Login", "Username", "");
            pwd = ini.ReadString("Login", "Password", "");

            login.CheckUser(uname, pwd);            
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

        public LinkGroupCollection UpdateMenu()
        {
            this.NewMenuLinkGroups.Clear();
            if (login.IsLoggedIn)
            {
                MainWin.Title = "Light Life - Admin Console / " + login.FirstName + " " + login.LastName;
                return DisplayMenuItems(pagesVisible);
            } 
            else
            {
                MainWin.Title = "Light Life - Admin Console";
                return DisplayMenuItems("login");
            }
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

        public void DoLogin(string uname, string pwd)
        {
            login.CheckUser(uname, pwd);

            if (!login.IsLoggedIn)
                throw new ArgumentException("Wrong Username/Password!");

            MainWin.MenuLinkGroups = UpdateMenu();            
       }

        public void ShowBrightnessWindow()
        {
            if (wndBrightness == null)
                wndBrightness = new BrightnessWindow();

            if (wndBrightness.Visibility == Visibility.Hidden)
                wndBrightness.Show();            
            else
                wndBrightness.Hide();
        }
    }
}
