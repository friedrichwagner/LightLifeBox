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
    public partial class Step1 : UserControl, IContent
    {
        private LEDTestViewModel VModel;

        public Step1()
        {
            try
            {
                InitializeComponent();                
                VModel = LEDTestViewModel.GetInstance(false, false);
                this.DataContext = VModel;

                if (!VModel.UsePowerSource) btnStartTest.Visibility = Visibility.Hidden;                
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

                if (!e.Source.ToString().StartsWith("/Content/LEDTest2")) return;

                VModel.SaveHead();
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
                e.Cancel = true;
            }
        }
        

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (VModel.UsePowerSource && e.Key == Key.F12)
            {
                btnStartTest_Click(sender, null);
                e.Handled = true;
            }
        }

        private void btnStartTest_Click(object sender, RoutedEventArgs e)
        {
            NavigationCommands.GoToPage.Execute("/Content/LEDTest2/Step7.xaml", this);
        }

    }
}
