using System;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using LightLifeAdminConsole.MVVM;
using Lumitech;
using Lumitech.Helpers;
using System.Windows.Input;
using LightLifeAdminConsole.Data;
using System.Text;
using System.IO;

namespace LightLifeAdminConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        private MainVM dc;
        public RelayCommand AboutCommand { get; private set; }

        public MainWindow()
        {            
            try
            {
                InitializeComponent();

                /*string s = "Lumitech LightLife"; //asciisum=1747
                byte[] barr = Encoding.ASCII.GetBytes(s);
                int asciisum = 0;
                foreach (byte b in barr)
                {
                    asciisum += b;
                }*/

                /*PathGeometry geom = new PathGeometry();
                Geometry g = new RectangleGeometry(new Rect(0, 0, 28, 28));
                geom.AddGeometry(g);*/

                LLSQL.InitSQLs();

                dc = MainVM.GetInstance(this.MenuLinkGroups, this);                
                DataContext= dc;                

                Settings ini = Settings.GetInstance();
                ContentSource = new Uri(ini.ReadString("Pages", "StartPage", ""), UriKind.Relative); //LoginPage
                
                this.ResizeMode = ResizeMode.CanMinimize;

                AboutCommand = new RelayCommand(o => DisplayAboutBox(o));
                LinkNavigator.Commands.Add(new Uri("cmd://About", UriKind.Absolute), AboutCommand);
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
            if (dc.wndBrightness != null)
                dc.wndBrightness.Close();
            p.Done();

            LLSQL.Done();
        }

        private void ModernWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if ((e.Key == Key.F12))
                {
                    dc.ShowBrightnessWindow();
                    e.Handled = true;
                }

            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            } 
        }

        private void DisplayAboutBox(object o)
        {
            dc.ShowAboutBox();
        }

    }
}
