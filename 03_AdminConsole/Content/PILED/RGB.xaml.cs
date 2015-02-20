using System.Windows.Controls;
using FirstFloor.ModernUI.Windows;
using LightLifeAdminConsole.MVVM;

namespace LightLifeAdminConsole.Content.PILED
{
    /// <summary>
    /// Interaction logic for CCT.xaml
    /// </summary>
    public partial class RGB : UserControl, IContent
    {
        private PiledVM dc;

        public RGB()
        {
            InitializeComponent();
            dc = PiledVM.GetInstance();
            DataContext = dc;
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) 
        {
            //damit in den RGB Mode umgeschalten wird, Helligkeit immer 100%
            dc.R = dc.lldata.piled.r;
            dc.Brightness = 255;
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) 
        {
            //damit in den RGB Mode umgeschalten wird, Helligkeit immer 100%
            dc.R = dc.lldata.piled.r;
            dc.Brightness = 255;
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }
    }
}
