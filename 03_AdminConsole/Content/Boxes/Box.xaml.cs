using FirstFloor.ModernUI.Windows;
using System.Windows.Controls;
using LightLifeAdminConsole.MVVM;
using System;
using System.Windows;

namespace LightLifeAdminConsole.Content.Boxes
{
    /// <summary>
    /// Interaction logic for Box.xaml
    /// </summary>
    public partial class Box : UserControl, IContent
    {
        private BoxVM dc;

        public Box()
        {
            InitializeComponent();
            dc = BoxVM.GetInstance();
            DataContext = dc;

        }
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            dc.SelectedBox = Int32.Parse(e.Fragment)-1;
        }


        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) {}

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }

        private void btnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if ((sender as Button).Tag.ToString().Equals("START")) dc.boxes[dc.SelectedBox].StartSequence();
                if ((sender as Button).Tag.ToString().Equals("STOP")) dc.boxes[dc.SelectedBox].StopSequence();
                if ((sender as Button).Tag.ToString().Equals("PAUSE")) dc.boxes[dc.SelectedBox].PauseSequence();
                if ((sender as Button).Tag.ToString().Equals("PREV")) dc.boxes[dc.SelectedBox].PrevStep();
                if ((sender as Button).Tag.ToString().Equals("NEXT")) dc.boxes[dc.SelectedBox].NextStep();
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }
    }
}
