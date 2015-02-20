using System.Windows.Controls;
using LightLifeAdminConsole.MVVM;
using FirstFloor.ModernUI.Windows;

namespace LightLifeAdminConsole.Content.PILED
{
    /// <summary>
    /// Interaction logic for CCT.xaml
    /// </summary>
    public partial class CCT : UserControl, IContent
    {
        private PiledVM dc;

        public CCT()
        {
            InitializeComponent();
            dc = PiledVM.GetInstance();
            DataContext = dc;
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) 
        {
            //damit in den CCT Mode umgeschalten wird
            dc.CCT = dc.lldata.piled.cct;
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) 
        {
            //damit in den CCT Mode umgeschalten wird
            dc.CCT = dc.lldata.piled.cct;
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }
    }
}
