using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows;
using System.IO;
using System.ComponentModel;

namespace ModernUIApp2.Content.Readout2
{
    /// <summary>
    /// Interaction logic for SettingsAppearance.xaml
    /// </summary>
    public partial class Step1_Header : UserControl, IContent
    {
        private Lumitech.Readout2 ro2;

        public Step1_Header()
        {
            try
            {
                InitializeComponent();
                ro2 = Lumitech.Readout2.GetInstance();
                this.DataContext = ro2;        
                GetSetLastFaufKauf(true);
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) 
        {
            try
            {
                if (e.Source == null)
                {
                    e.Cancel = true;
                    return; //Der Trennstrich im linken Menü
                }

                if (!e.Source.ToString().StartsWith("/Content/Readout2")) return;

                ro2.checkTxtLength(cbFauf, "Please enter FAUF");
                ro2.checkTxtLength(cbKauf, "Please enter KAUF");
                ro2.checkTxtLength(txtUsername, "Please enter Username");

                ro2.data.KZ=txtUsername.Text;

                GetSetLastFaufKauf(false);
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
                e.Cancel = true;
            }
        }

        private void GetSetLastFaufKauf(bool doget)
        {
            string tmp = AppDomain.CurrentDomain.BaseDirectory + Environment.MachineName + "_FAUFKAUF.txt";
            if (doget)
            {
                string[] s;
                if (!File.Exists(tmp)) return;
                using (StreamReader fs = new StreamReader(tmp))
                {
                    s=fs.ReadLine().Split(';');
                    fs.Close();
                }

                if (s.GetLength(0) == 2)
                {
                    cbFauf.Items.Add(s[0]);
                    cbKauf.Items.Add(s[1]);
                }
            }
            else
            {               
                using (StreamWriter fs = new StreamWriter(tmp))
                {
                    fs.WriteLine(ro2.sap.FAUF + ";" + ro2.KAUF);
                    fs.Close();
                }
            }
        }

        void getSAPDataCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null) throw (e.Error);
                ro2.KAUF = cbKauf.Text;
                txtUsername.Focus();
            }
            catch (Exception ex)
            {
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void cbFauf_LostFocus(object sender, RoutedEventArgs e)
        {
            if (cbFauf.Text.Length > 0 && cbKauf.Text.Length > 0 && !cbFauf.Text.Equals(ro2.sap.FAUF) && !cbKauf.Text.Equals(ro2.KAUF))
            {
                ro2.sap.getDataAsync(cbFauf.Text, getSAPDataCompleted);  
            }

        }

        private void txtMenge_LostFocus(object sender, RoutedEventArgs e)
        {
            NavigationCommands.GoToPage.Execute("/Content/Readout2/Step2_Readout.xaml", this);
        }
    }
}
