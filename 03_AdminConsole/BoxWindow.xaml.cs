using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Windows.Input;
using System.Windows.Controls;
using LightLifeAdminConsole.MVVM;
using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Media;
using Lumitech.Helpers;

namespace LightLifeAdminConsole
{
    /// <summary>
    /// Interaction logic for Box.xaml
    /// </summary>
    public partial class BoxWindow : ModernWindow
    {
        private int _boxnr;
        public int BoxNr { get { return _boxnr; } }

        private BoxVM2 dc;

        public BoxWindow(int boxnr)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                InitializeComponent();
                dc = new BoxVM2(Box2.boxes[boxnr]);
                DataContext = dc;
                InitDataGrid();

                _boxnr = boxnr;

                this.Title = "Box#" + _boxnr.ToString() + " - " + dc.box.Name;
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void InitDataGrid()
        {
            //dgBox.IsReadOnly = true;
            dgBox.CanUserAddRows = false;
            dgBox.CanUserDeleteRows = false;
            dgBox.CanUserReorderColumns = false;
            dgBox.CanUserSortColumns = false;
            dgBox.AutoGenerateColumns = false;
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public new void Close()
        {
            this.Closing -= ModernWindow_Closing;
            //Add closing logic here.
            dc.box.Close();
            base.Close();
        }

        private void dgBox_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            int rowNum = e.Row.GetIndex();
            if (rowNum == dc.box.testsequence.PosID - dc.box.testsequence.MinPosID)
                    e.Row.Background = Brushes.LightBlue;
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

        private void txtSequenceID_LostFocus(object sender, RoutedEventArgs e)
        {
            try {
                if (dc.SequenceID != Int32.Parse(txtSequenceID.Text))
                {
                    SavePosRemarks();

                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    dc.ReloadSequence(Int32.Parse(txtSequenceID.Text));

                    //if (dc.box.IsActive)
                      //  NavigationCommands.GoToPage.Execute("/Content/Boxes/Box.xaml#" + dc.box.BoxNr.ToString(), this);
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

        private void SavePosRemarks()
        {
            for (int i = 0; i < dgBox.Items.Count; i++)
            {
                DataRowView rv = (DataRowView)dgBox.Items[i];
                if (rv.Row.RowState == DataRowState.Modified)
                {
                    string remark = rv.Row["remark"].ToString();
                    int posid = Int32.Parse(rv.Row["PosID"].ToString());
                    dc.box.testsequence.UpdateRemarkPos(posid, remark);
                }
            }
        }

        private void txtProband_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;

            }
        }

        private void txtSequenceID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;

            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SavePosRemarks();

                //Always do ping here
                if (!dc.box.Ping())
                    throw new Exception("Ping NOT OK!");
                
                dc.ReloadSequence(dc.SequenceID);
            }
            catch (Exception ex)
            {
                dc.ErrorText = ex.Message;
            }

        }

        private void dgBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //if (dc.BtnNextEnabled || dc.BtnPrevEnabled)
                if (dc.BtnStartEnabled)
                {
                    var row = (sender as DataGrid).GetSelectedRow();
                    DataRowView rv = (DataRowView)dgBox.Items[row.GetIndex()];
                    int posid = Int32.Parse(rv.Row["PosID"].ToString());

                    dc.box.testsequence.Goto(CommandSender.GUI, posid);

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                dc.ErrorText = ex.Message;
            }

        }
    }
}
