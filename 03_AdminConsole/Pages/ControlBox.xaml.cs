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

        private int getSelectedBoxNr()
        {
            int ret = -1;

            if (dgBoxOverview.SelectedItems != null && dgBoxOverview.SelectedItems.Count == 1)
            {
                var rowview = dgBoxOverview.SelectedItem as DataRowView;
                if (rowview != null)
                {
                    DataRow row = rowview.Row;
                    ret = (int)row[0];
                }

            }

            return ret;
        }

        private void dgBoxOverview_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int boxnr = getSelectedBoxNr();
            if (boxnr > -1)
            {
                    Box2.boxes[boxnr].Ping();

                    if (Box2.boxes[boxnr].IsPracticeBox)
                        Box2.practiceboxWindows[boxnr].Show();
                    else
                        Box2.boxWindows[boxnr].Show();
            }
        }

        private void mnuPing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                int boxnr = getSelectedBoxNr();
                if (boxnr > -1)
                {
                    Box2.boxes[boxnr].Ping();

                    Mouse.OverrideCursor = Cursors.Arrow;

                    if (Box2.boxes[boxnr].IsActive)
                        ModernDialog.ShowMessage("Ping OK!", "Info", MessageBoxButton.OK);
                    else
                        ModernDialog.ShowMessage("Ping NOT OK!", "Info", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {

                ModernDialog.ShowMessage(ex.Message,"Error", MessageBoxButton.OK);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void mnuReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                int boxnr = getSelectedBoxNr();
                if (boxnr > -1)
                {
                    bool ok = Box2.boxes[boxnr].ResetLLBox();

                    Mouse.OverrideCursor = Cursors.Arrow;
                    if (ok)
                        ModernDialog.ShowMessage("Reset OK!", "Info", MessageBoxButton.OK);
                    else
                        ModernDialog.ShowMessage("Reset NOT OK!", "Error", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message,"Error", MessageBoxButton.OK);
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }

        }

        private void mnuReboot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                int boxnr = getSelectedBoxNr();
                if (boxnr > -1)
                {
                    bool ok = Box2.boxes[boxnr].RebootRaspi();

                    Mouse.OverrideCursor = Cursors.Arrow;

                    if (ok)
                        ModernDialog.ShowMessage("Reboot OK!", "Info", MessageBoxButton.OK);
                    else
                        ModernDialog.ShowMessage("Reboot NOT OK!", "Error", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }
    }
}
