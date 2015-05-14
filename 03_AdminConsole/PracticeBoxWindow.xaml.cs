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

namespace LightLifeAdminConsole
{
    /// <summary>
    /// Interaction logic for Box.xaml
    /// </summary>
    public partial class PracticeBoxWindow : ModernWindow
    {
        private int _boxnr;
        public int BoxNr { get { return _boxnr; } }

        private PracticeBoxVM dc;

        public PracticeBoxWindow(int boxnr)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                InitializeComponent();
                dc = new PracticeBoxVM(Box2.boxes[boxnr]);
                DataContext = dc;
                ResizeMode = ResizeMode.NoResize;

                _boxnr = boxnr;

                this.Title = "Box#" + _boxnr.ToString() + " - " + dc.box.Name;
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
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

        private void txtDeltaTestProband_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;

            }
        }

        private void txtDeltaTestBrigthness_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;

            }
        }

        private void ModernWindow_Activated(object sender, EventArgs e)
        {
            SolidColorBrush myBrush = new SolidColorBrush(Colors.White);
            if (!dc.box.IsActive)
                myBrush = new SolidColorBrush(Colors.LightGray);
            grdDeltaTest.Background= myBrush;
        }  
 
    }
}
