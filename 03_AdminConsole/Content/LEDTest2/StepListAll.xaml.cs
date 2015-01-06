using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows;
using Lumitech;
using System.Data;
using PILEDTestSuite.MVVM;

namespace ModernUIApp2.Content.LEDTest2
{
    /// <summary>
    /// Interaction logic for SettingsAppearance.xaml
    /// </summary>
    public partial class StepListAll : UserControl, IContent
    {
        private LEDTestViewModel ledtest;
        private DataView dv;

        public StepListAll()
        {
            InitializeComponent();
            ledtest = LEDTestViewModel.GetInstance(false, false);
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) 
        {                       
            //dv = ledtest.ltqs.ds.Tables["QCOrderPos"].DefaultView;
            dv = ledtest.getQCOrderPos();
            dv.RowFilter = "HeadID='" + ledtest.head.HeadID + "'";
            dgData.DataContext = dv;
        }

        private void mnuExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgData.Items.Count > 0)
                {
                    ledtest.ExportToExcel();
                }
            }
            catch (Exception ex)
            {
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }
    }
}
