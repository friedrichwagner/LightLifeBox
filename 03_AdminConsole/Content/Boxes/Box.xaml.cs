using FirstFloor.ModernUI.Windows;
using System.Windows.Controls;
using LightLifeAdminConsole.MVVM;
using System;
using System.Windows;
using System.Windows.Media;

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
            dc.SelectedBox = Int32.Parse(e.Fragment);
        }


        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) {}

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }

        private void dgBox_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (dc.SelectedBox > 0)
            {
                if (1== dc.boxes[dc.SelectedBox].StepID)
                    e.Row.Background = Brushes.LightGray;
                else
                    e.Row.Background = Brushes.White;
            }
        }

        private void dgBox_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString().ToLower() != "remark")
            {
                e.Column.IsReadOnly = true;
            }

        }
    }
}
