using System.Windows.Controls;
using LightLifeAdminConsole.MVVM;
using FirstFloor.ModernUI.Windows;

namespace LightLifeAdminConsole.Content.PILED
{
    /// <summary>
    /// Interaction logic for Sequence.xaml
    /// </summary>
    public partial class Sequence : UserControl, IContent
    {
        private PiledVM dc;

        public Sequence()
        {
            InitializeComponent();
            dc = PiledVM.GetInstance();
            DataContext = dc;
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)  {  }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }

        private void btnApplySequence_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dc.ApplySequence();
        }

        private void btnApplyScene_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dc.ApplyScene();
        }
    }
}
