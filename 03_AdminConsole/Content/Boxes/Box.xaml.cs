using FirstFloor.ModernUI.Windows;
using System.Windows.Controls;
using LightLifeAdminConsole.MVVM;
using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Input;

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
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                InitializeComponent();
                dc = BoxVM.GetInstance();
                DataContext = dc;
                dgBox.CanUserAddRows = false;
                dgBox.CanUserDeleteRows = false;
                dgBox.CanUserReorderColumns = false;
                dgBox.CanUserSortColumns = false;
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }

        }
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                dc.SelectedBox = Int32.Parse(e.Fragment);
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)  { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) {}
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }

        private void dgBox_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (dc.SelectedBox > 0)
            {
                var rowNum = e.Row.GetIndex()+1;
                if (rowNum == dc.boxes[dc.SelectedBox].StepID)
                    e.Row.Background = Brushes.LightGray;
                //else e.Row.Background = Brushes.White;
               /* var row = e.Row.GetIndex();
                if (row % 2 == 0)
                {
                    e.Row.Background = new SolidColorBrush(Colors.LightGray);
                    e.Row.Foreground = new SolidColorBrush(Colors.White);
                    e.Row.FontStyle = FontStyles.Italic;
                }

                else
                {
                    // defaults - without these you'll get randomly colored rows
                    e.Row.Background = new SolidColorBrush(Colors.Green);
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontStyle = FontStyles.Normal;
                }*/
            }
        }

        private void dgBox_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString().ToLower() == "remark")
            {
                e.Column.IsReadOnly = false;
            }
        }

        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }

        private void dgBox_LostFocus(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgBox.Items.Count; i++)
            {
                DataRowView rv = (DataRowView)dgBox.Items[i];
                if (rv.Row.RowState == DataRowState.Modified)
                {
                    string remark = rv.Row[7].ToString();
                }
            }
        }

        /*private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try {
                dc.RefreshPos();
            }
            catch (Exception ex)
            {
                dc.ErrorText = ex.Message;
            }
        }*/

        private void txtSequenceID_LostFocus(object sender, RoutedEventArgs e)
        {
            try {

                //if (Int32.Parse(txtSequenceID.Text) > 0)
                if (dc.boxes[dc.SelectedBox].SequenceID != Int32.Parse(txtSequenceID.Text))
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    dc.ReloadSequence(Int32.Parse(txtSequenceID.Text));

                    if (dc.boxes[dc.SelectedBox].IsActive)
                        NavigationCommands.GoToPage.Execute("/Content/Boxes/Box.xaml#" + dc.SelectedBox.ToString(), this);
                }
            }
            catch (Exception ex)
            {
                 dc.ErrorText = ex.Message;
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }
    }
}
