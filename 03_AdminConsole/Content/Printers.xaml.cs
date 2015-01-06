using System;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using MvvmFoundation.Wpf;
using PILEDTestSuite.MVVM;
using Lumitech;
using Lumitech.Helpers;

namespace ModernUIApp2.Content
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class SelectPrinter : UserControl
    {
        private Lumitech.Readout2 ro2;
        private Settings ini;
        private LEDTestViewModel ledtest;

        public SelectPrinter()
        {
            try
            {
                InitializeComponent();
                ro2 = Lumitech.Readout2.GetInstance();
                ledtest = LEDTestViewModel.GetInstance(false, false);
                ini = Settings.GetInstance();
                this.DataContext = ro2;

                cbComPort.ItemsSource = SerialPort.GetPortNames();
                string defaultport = ini.Read<string>("PI-LED", "COM-Port", "");

                if (defaultport.Length > 0)
                    cbComPort.SelectedItem = defaultport;
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void cbComPort_DropDownClosed(object sender, EventArgs e)
        {
        if (cbComPort.SelectedIndex >= 0)
            {
                ini.Write<string>("PI-LED", "COM-Port", cbComPort.SelectedItem.ToString());
                ini.Flush();

                ledtest.TestBoxComport = cbComPort.SelectedItem.ToString();
            }
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            cbPrinters.SelectedIndex=0;
        }
    }
}
