using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows;
using System.Threading;
using PILEDTestSuite.MVVM;
using FirstFloor.ModernUI.Windows.Controls;

namespace FlashNPrint.Pages
{
    /// <summary>
    /// Interaction logic for SimpleTest.xaml
    /// </summary>
    public partial class SimpleTest : UserControl, IContent
    {
        LEDTestViewModel VModel;
        private int teststep = 0;

        public SimpleTest()
        {
            try
            {
                InitializeComponent();
                VModel = LEDTestViewModel.GetInstance(false, false);
                this.DataContext = VModel;

                //if (!VModel.UsePowerSource) btnStartTest.Visibility = Visibility.Hidden;    

            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)  {   }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtFauf.Focus();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VModel.isConnected)
                {
                    VModel.Disconnect();
                    if (VModel.UsePowerSource) VModel.PowerSourceOff();
                    EnableButtons(true);
                }
                else
                {
                    if (VModel.UsePowerSource) VModel.PowerSourceOn();
                    Thread.Sleep(1000);
                    VModel.Connect();
                }

            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void EnableButtons(bool enable)
        {
            /*btnPhosphor.IsEnabled = enable;
            btnBlue.IsEnabled = enable;
            btnRed.IsEnabled = enable;
            btnTSensor.IsEnabled = enable;
            btnEeprom.IsEnabled = enable;*/

            btnStartTest.IsEnabled = enable;
            //btnConnect.IsEnabled = enable;
        }


        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                VModel.head.FAUF = txtFauf.Text;
                if (VModel.StartTest())
                {
                    teststep = 0;
                    ExecuteWorkFlow();
                }

                e.Handled = true;
            }

            if (!VModel.UsePowerSource && e.Key == Key.Y)
            {

                setCBManually(true);
                e.Handled = true;
            }

            if (!VModel.UsePowerSource && e.Key == Key.N)
            {
                setCBManually(false);
                e.Handled = true;
            }

        }

        private void setCBManually(bool bOK)
        {
            if (teststep == 1)
                rbCheckOK(rbRedOK, rbRedNOTOK, bOK);
            else if (teststep == 2)
                rbCheckOK(rbPhosphorOK, rbPhosphorNOTOK, bOK);
            else if (teststep == 3)
                rbCheckOK(rbBlueOK, rbBlueNOTOK, bOK);

            if (teststep >= 1 && teststep <= 3)
            {
                teststep++;
                ExecuteWorkFlow();
            }
        }

        private bool? rbCheckOK(RadioButton rbOK, RadioButton rbNotOK, bool? bOK)
        {
            if (bOK == null)
            {
                rbOK.IsChecked = null;
                rbNotOK.IsChecked = null;
            }
            else
            {
                rbOK.IsChecked = bOK;
                rbNotOK.IsChecked = !bOK;
            }

            return bOK;
        }

        #region MANUAL_HANDLING

        private void ExecuteWorkFlow()
        {
            try
            {
                if (VModel.UsePowerSource) return;

                switch (teststep)
                {
                    case 0: //Step0: All Off
                        EnableButtons(false);
                        VModel.pos.Reset();                      
                        teststep++;
                        ExecuteWorkFlow();
                        break;

                    case 1: 
                        VModel.Brightness = new byte[] { 0, 0, 0 };
                        VModel.RedIsOn = true;
                        rbRedOK.Focus();
                        break;

                    case 2: //Step2: Phosphor Channel                        
                        VModel.Brightness = new byte[] { 0, 0, 0 };
                        VModel.PhosphorIsOn = true;
                        rbPhosphorOK.Focus();
                        break;

                    case 3: //Step3: Blue Channel                                               
                        VModel.Brightness = new byte[] { 0, 0, 0 };
                        VModel.BlueIsOn = true;
                        rbBlueOK.Focus();
                        break;

                    case 4: //Step4: T-Sensor  
                        VModel.Brightness = new byte[] { 0, 0, 0 };
                        VModel.checkTemperature();
                        rbTSensorOK.IsChecked = VModel.TSensorIsOK;
                        teststep++;
                        ExecuteWorkFlow();
                        break;

                    case 5:
                        VModel.checkEeprom();
                        rbEepromOK.IsChecked = VModel.EepromIsOK;
                        teststep++;
                        ExecuteWorkFlow();
                        break;

                    case 6:
                        EnableButtons(true);

                        VModel.Disconnect();                
                        btnStartTest.Focus();
                        VModel.SaveToFile();
                        teststep = 0;
                        break;

                    default:
                        teststep = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
                teststep = 0;
                EnableButtons(true);
            }

        }

        private void rbPhosphorOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as Control).Tag.ToString() == teststep.ToString())
                {
                    teststep++;
                    ExecuteWorkFlow();
                }
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void btnStartTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VModel.StartTest())
                {
                    teststep = 0;
                    ExecuteWorkFlow();
                }
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
          

        #endregion  
        }
    }
}

