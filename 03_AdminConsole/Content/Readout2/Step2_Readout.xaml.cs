using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using Lumitech;
using System.ComponentModel;

namespace ModernUIApp2.Content.Readout2
{
    /// <summary>
    /// Interaction logic for SettingsAppearance.xaml
    /// </summary>
    public partial class Step2_Readout : UserControl, IContent
    {
        Lumitech.Readout2 ro2;

        public Step2_Readout()
        {
            InitializeComponent();
            ro2 = Lumitech.Readout2.GetInstance();
            this.DataContext = ro2;
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)  
        {
            //txtmA.Focus();
            this.Focus();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ro2.ReadoutAsync(ReadoutCompleted);
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ro2.checkTxtLength(txtModuleNr, "Please enter ModuleNr");
                ro2.checkTxtLength(txtSerialNr, "Please enter SerialNr");
                ro2.checkTxtLength(txtmA, "Please enter Current Consumption in mA");

                //ro2.data.ModuleNr 
                //ro2.data.SerialNr
                //ro2.data.KZ
                ro2.data.mA = Int32.Parse(txtmA.Text);
                ro2.data.OK = cbOK.IsChecked;
                ro2.data.Comment = txtRemark.Text;
                ro2.data.TestDate = DateTime.Now;

                //DataItemReadout d = new DataItemReadout(0, ro2.ModuleNr, ro2.SerialNr, ro2.mA.ToString(), ro2.Username, ro2.OK, txtRemark.Text);

                if (!ro2.Save())
                    throw new ArgumentException("No record updated!");

                ro2.data.Empty();
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12) btnPrint_Click(sender, null);
        }

        void ReadoutCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null) throw(e.Error);
                
                if ((ro2.data.ModuleNr.Length>0) && (ro2.data.SerialNr.Length>0))
                    if (!ro2.Print(ro2.DefaultPrinter, ro2.data, enumLabelType.ltLTS))
                        FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage("No record updated!", "Error", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }
    }
}
