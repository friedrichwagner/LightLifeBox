using System;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using LightLife;
using System.Windows;

namespace LightLifeAdminConsole.Content.Admin
{
    /// <summary>
    /// Interaction logic for Roles.xaml
    /// </summary>
    public partial class Roles : UserControl, IContent
    {
        private AdminBase ab;

        public Roles()
        {
            InitializeComponent();
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            try
            {
                //ModernDialog.ShowMessage(e.Fragment.ToString(), "Error", MessageBoxButton.OK);
                ab = new AdminBase(LLSQL.sqlCon, LLSQL.tables[e.Fragment]);

                //dgRoles.DataContext = ab.selecttable("");
                dgRoles.ItemsSource = ab.select("").DefaultView;
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) 
        {
            //ModernDialog.ShowMessage(e.Source.ToString(),"Error", MessageBoxButton.OK);
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)  {  }
    }
}
