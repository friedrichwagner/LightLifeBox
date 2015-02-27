using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Navigation;

namespace LightLifeAdminConsole.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var f = NavigationHelper.FindFrame(null, this);

            if ((e.Key == Key.Escape) && f != null)
                NavigationCommands.BrowseBack.Execute(null, f);
        }
    }
}
