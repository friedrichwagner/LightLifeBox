using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows;
using Lumitech;

namespace ModernUIApp2.Content.Readout2
{
    /// <summary>
    /// Interaction logic for SettingsAppearance.xaml
    /// </summary>
    public partial class Step3_Summary : UserControl, IContent
    {
        Lumitech.Readout2 ro2;

        public Step3_Summary()
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
            cbOK.IsChecked = ro2.OK;
            btnSave.Focus();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataItemReadout d = new DataItemReadout(0, ro2.ModuleNr, ro2.SerialNr, ro2.mA.ToString(), ro2.Username, ro2.OK, txtRemark.Text);

                if (!ro2.Save(d))
                   throw new ArgumentException("No record updated!");
                else
                    NavigationCommands.GoToPage.Execute("/Content/Readout2/Step2_Readout.xaml", this);
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }
    }
}
