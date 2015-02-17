using System;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using LightLifeAdminConsole.MVVM;
using Lumitech;
using Lumitech.Helpers;

namespace LightLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        private MainVM dc;
        public MainWindow()
        {            
            try
            {
                InitializeComponent();

                LLSQL.InitSQLs();

                dc = MainVM.GetInstance(this.MenuLinkGroups, this);                
                DataContext= dc;                

                Settings ini = Settings.GetInstance();
                ContentSource = new Uri(ini.ReadString("Pages", "StartPage", ""), UriKind.Relative); //LoginPage
                
                this.ResizeMode = ResizeMode.NoResize;           
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AppearanceManager.Current.AccentColor = Color.FromRgb(0xe5, 0x14, 0x00);
            Left = 0;
            Top = 0;

           MenuLinkGroups = dc.UpdateMenu();
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PiledVM p = PiledVM.GetInstance();

            p.Done();
        }
    }
}
