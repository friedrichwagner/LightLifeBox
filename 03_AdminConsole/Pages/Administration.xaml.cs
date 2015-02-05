using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Navigation;

namespace ModernUIApp2.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Readout2 : UserControl
    {
        public Readout2()
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
