using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows;
using System.ComponentModel;
using System.Threading;
using PILEDTestSuite.MVVM;
using Lumitech;

namespace ModernUIApp2.Content.LEDTest2
{
    /// <summary>
    /// Interaction logic for SettingsAppearance.xaml
    /// </summary>
    public partial class Step7 : UserControl, IContent
    {
        private LEDTestViewModel VModel;
        private BackgroundWorker bw1 = new BackgroundWorker();

        public Step7()
        {
            try
            {
                InitializeComponent();
                VModel = LEDTestViewModel.GetInstance(false, false);
                this.DataContext = VModel;

                bw1.DoWork += (obj, li) => bw_DoWork(this, null);
                bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_TestCompleted);
                bw1.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);

                bw1.WorkerReportsProgress = true;
                bw1.WorkerSupportsCancellation = false;
            }
            catch (Exception ex)
            {
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void SetControls()
        {
            bool bisEnabled = !VModel.UsePowerSource;

            if (!VModel.UsePowerSource)
            {
                btnStartTest.Visibility = Visibility.Hidden;
                grdDisplay.RowDefinitions[0].Height = new GridLength(0);
            }
            else
                grdDisplay.RowDefinitions[0].Height = new GridLength(80);

            txtmARed.IsReadOnly = !bisEnabled;
            txtmAPhosphor.IsReadOnly = !bisEnabled;
            txtmABlue.IsReadOnly = !bisEnabled;

            cbRed.IsEnabled = bisEnabled;
            cbPhosphor.IsEnabled = bisEnabled;
            cbBlue.IsEnabled = bisEnabled;

            cbTemp.IsEnabled = false;
            cbEeprom.IsEnabled = false;            
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) 
        {
            try
            {
                VModel.Disconnect();
            }
            catch (Exception ex)
            {                
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) 
        {
            try
            {
                SetControls();
                VModel.pos.Reset(false);
            }
            catch (Exception ex)
            {
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (VModel.UsePowerSource && e.Key == Key.F12)
                {
                    btnStartTest_Click(sender, null);
                    e.Handled = true;
                }

                if (e.Key == Key.F9)
                {
                    btnSave_Click(sender, null);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        #region MANUAL_HANDLING

        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            bool b;
            try
            {
                Mouse.SetCursor(Cursors.Wait);

                VModel.Connect();

                if ((sender as TextBox).Tag != null )
                {
                    int tag = Int32.Parse((sender as Control).Tag.ToString());

                    int tmp=0;
                    b = Int32.TryParse((sender as TextBox).Text, out tmp);
                    if (!b) (sender as TextBox).Text= String.Empty;

                    if (tag < 3)
                    {
                        VModel.Brightness = new byte[] { 0, 0, 0 };
                        VModel.SetLED((PILED_LEDNUM)tag, true);
                    }
                    else
                    {
                        VModel.Brightness = new byte[] { 0, 0, 0 };
                        b = VModel.checkTemperature(); // ledtest.pos.Temperature und ltedtest.pos.TSensorOK wird in Funktion gesetzt
                        b = VModel.checkEeprom(); // ledtest.pos.EepromOK wird in der FUnktion gesetzt
                    }
                }
                Mouse.SetCursor(Cursors.Arrow);
            }
            catch (ArgumentException)
            {
                VModel.Disconnect();
                Thread.Sleep(1000);
                VModel.Connect();
                Mouse.SetCursor(Cursors.Arrow);
            }
            catch (System.TimeoutException ex2)
            {
                VModel.Disconnect();
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex2.Message, "Error", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void cbPhosphor_Click(object sender, RoutedEventArgs e)
        {
             /*try
            {
                if ((sender as CheckBox).Tag != null)
                {
                    int tag = Int32.Parse((sender as Control).Tag.ToString());

                    if (tag == 0) VModel.pos.RedOK = (sender as CheckBox).IsChecked;
                    if (tag == 1) VModel.pos.PhosphorOK = (sender as CheckBox).IsChecked;
                    if (tag == 2) VModel.pos.BlueOK = (sender as CheckBox).IsChecked;
                }
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }*/
        }

        #endregion

        private void txtRemark_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!VModel.UsePowerSource)
                {
                    if (VModel.isConnected)
                    {
                        VModel.Brightness = new byte[] { 0, 0, 0 };
                        VModel.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VModel.SavePos();                
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        #region AUTOMATIC_HANDLING

        private void btnStartTest_Click(object sender, RoutedEventArgs e)
        {
            if (VModel.UsePowerSource)
                StartBackgroundThread();
        }

        private bool StartBackgroundThread()
        {
            if (bw1.IsBusy == true) return false;

            Mouse.SetCursor(Cursors.Wait);

            if (VModel.UsePowerSource)
            {
                VModel.PowerSourceOn();
                VModel.Disconnect();
                Thread.Sleep(1000);
                VModel.Connect();
            }

            btnSave.IsEnabled = false;

            if (bw1.IsBusy != true)
                bw1.RunWorkerAsync();

            return true;
        }

        private void bw_DoWork(object sender, List<object> obj)
        {
            try
            {
                //Step0: All Off
                //ledtest.mm2.setBrightness(new byte[] { 0, 0, 0 });
                VModel.Brightness = new byte[] { 0, 0, 0 };

                for (int i = 0; i <= (int)PILED_LEDNUM.BLUE; i++)
                {
                    VModel.SetLED((PILED_LEDNUM)i, true);
                    Thread.Sleep(VModel.LEDOnTime);
                    bw1.ReportProgress(i);
                    VModel.Brightness = new byte[] { 0, 0, 0 };
                }

                bool b = VModel.checkTemperature();
                bw1.ReportProgress(5);

                b = VModel.checkEeprom();
                bw1.ReportProgress(10);
            }
            catch
            {
                throw;
            }
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage <= (int)PILED_LEDNUM.BLUE)
                CheckPowerSource((PILED_LEDNUM)e.ProgressPercentage);
            else if (e.ProgressPercentage == 5)
                cbTemp.IsChecked = VModel.TSensorIsOK;
            else if (e.ProgressPercentage == 10)
                cbEeprom.IsChecked = VModel.EepromIsOK;
        }


        void bw_TestCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null) throw (e.Error);
                Mouse.SetCursor(Cursors.Arrow);
                txtRemark.Focus();
            }
            catch (Exception ex)
            {
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
            finally
            {
                btnSave.IsEnabled = true;
                VModel.Disconnect();
                if (VModel.UsePowerSource)
                    VModel.PowerSourceOff();
            }
        }

        private void CheckPowerSource(PILED_LEDNUM lednr)
        {
            bool ret = false;
            Single current = 0;
            if (VModel.UsePowerSource)
            {
                if (VModel.CheckCurrentInRange(ref current))
                    ret = true;

                switch (lednr)
                {
                    case PILED_LEDNUM.RED:
                        VModel.pos.mARed = current;
                        VModel.pos.RedOK = ret;
                        cbRed.IsChecked = ret; break;
                    case PILED_LEDNUM.PHOSPHOR:
                        VModel.pos.mAPhosphor = current;
                        VModel.pos.PhosphorOK = ret;
                        cbPhosphor.IsChecked = ret; break;
                    case PILED_LEDNUM.BLUE:
                        VModel.pos.mABlue = current;
                        VModel.pos.BlueOK = ret;
                        cbBlue.IsChecked = ret; break;
                }
            }
        }
        #endregion
    }
}
