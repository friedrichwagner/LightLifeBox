﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows;
using LightLifeAdminConsole.MVVM;

namespace LightLife.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Login : UserControl, IContent
    {
        private MainVM dc;

        public Login()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;

            dc = MainVM.GetInstance();     

        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            // select first control on the form
            ClearInputs();
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)  { }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)  {     }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) 
        {
            ClearInputs();

            if (dc.login.IsLoggedIn)
            {
                dc.MainWin.MenuLinkGroups = dc.UpdateMenu(false);
                dc.MainWin.ContentSource =  new Uri("/Pages/Login.xaml", UriKind.Relative); //LoginPage
                dc.MainWin.ResizeMode = ResizeMode.NoResize;
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) 
        {
            try
            {
                if (!dc.login.IsLoggedIn  && e.Source.OriginalString != "/Pages/Settings.xaml")
                {
                    dc.Login(txtUsername.Text, txtPassword.Password);
                    //NavigationCommands.GoToPage.Execute("/Pages/SimpleTest.xaml", null);
                    //this.Content = new Uri("Pages/SimpleTest.xaml", UriKind.Relative);
                    dc.MainWin.ResizeMode = ResizeMode.CanResizeWithGrip;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
                ClearInputs();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) { }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter))            
            {
                //Just try to navigate in order that OnNavigatingFrom is called
                NavigationCommands.GoToPage.Execute("/Pages/SimpleTest.xaml", null);

            }
            
        }

        private void ClearInputs()
        {
            Keyboard.Focus(this.txtUsername);
            txtUsername.Text = String.Empty;
            txtPassword.Password = String.Empty;
        }


    }
}