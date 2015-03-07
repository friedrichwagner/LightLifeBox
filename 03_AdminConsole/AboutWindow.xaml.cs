using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace LightLifeAdminConsole
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : ModernWindow
    {
        private const string sVersion = "0.1";

        public AboutWindow()
        {
            InitializeComponent();
        }

        private void ModernWindow_Initialized(object sender, EventArgs e)
        {
            txtVersion.Text = "Light Life - Admin Console " + sVersion;
            txtBuild.Text = File.GetLastWriteTime(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
        }

        private void ModernWindow_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if ((e.Key == Key.Escape))
                {
                    Close();
                    e.Handled = true;
                }

            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            } 
        }
    }
}
