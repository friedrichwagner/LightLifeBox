﻿using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Windows.Input;
using System.Windows.Controls;
using LightLifeAdminConsole.MVVM;
using FirstFloor.ModernUI.Windows.Controls;

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

                this.Title = "Box #" + _boxnr.ToString();
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void InitDataGrid()
        {
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

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dgBox_LoadingRow(object sender, DataGridRowEventArgs e)
        {
                /*var rowNum = e.Row.GetIndex()+1;
                if (rowNum == dc.box.testsequence.StepID)
                    e.Row.Background = Brushes.LightGray;*/
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
                /*if (dc.box.SequenceID != Int32.Parse(txtSequenceID.Text))
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    dc.ReloadSequence(Int32.Parse(txtSequenceID.Text));

                    if (dc.box.IsActive)
                        NavigationCommands.GoToPage.Execute("/Content/Boxes/Box.xaml#" + dc.box.BoxNr.ToString(), this);
                }*/
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