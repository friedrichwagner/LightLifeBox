using System;
using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using LightLife;
using LightLifeAdminConsole.Data;
using LightLifeAdminConsole.MVVM;
using System.Data;
using System.Windows.Input;

namespace LightLifeAdminConsole.Pages
{
    /// <summary>
    /// Interaction logic for ControlBox.xaml
    /// </summary>
    public partial class ControlBox : UserControl, IContent
    {
        private AdminBase ab;

        public ControlBox()
        {
            try
            {
                InitializeComponent();
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                //pffh, das ist wohl eher ein schneller Workaround
                MainVM m = MainVM.GetInstance();
                Box2.VLID = m.login.UserId;
                Box2.getBoxes();
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }


        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                //ModernDialog.ShowMessage(e.Fragment.ToString(), "Error", MessageBoxButton.OK);
                ab = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLBoxState"]);

                //dgRoles.DataContext = ab.selecttable("");
                dgBoxOverview.ItemsSource = ab.select(" where active=1").DefaultView;
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }

        private void dgBoxOverview_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (dgBoxOverview.SelectedItems != null && dgBoxOverview.SelectedItems.Count == 1)
            {
                var rowview = dgBoxOverview.SelectedItem as DataRowView;
                if (rowview != null)
                {
                    DataRow row = rowview.Row;
                    int boxnr = (int)row[0];
                    Box2.boxWindows[boxnr].Show();
                }
                
            }
        }
    }
}
