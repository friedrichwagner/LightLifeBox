using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using Lumitech;
using System.Data.SqlClient;

namespace ModernUIApp2.Content.Readout2
{
    /// <summary>
    /// Interaction logic for SettingsAppearance.xaml
    /// </summary>
    public partial class StepX_ListAll : UserControl, IContent
    {
        Lumitech.Readout2 ro2;

        public StepX_ListAll()
        {
            InitializeComponent();
            ro2 = Lumitech.Readout2.GetInstance();
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) {}
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) {}
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) 
        {
            try
            {
                if (ro2.sap.FAUF.Length==0 || ro2.KAUF.Length==0)
                    throw new ArgumentException("No FAUF/KAUF entered!");

                DisplayData();
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);

                var f = NavigationHelper.FindFrame(null, this);
                NavigationCommands.BrowseBack.Execute(null,f);
            }
        }

        private void mnuExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.Items.Count > 0)
            {
                DataItemReadout d = ((DataItemReadout)dgData.Items[0]);
                ro2.ExportToExcel(d);
            }
        }

        private void Reprint( DataItemReadout d, enumLabelType l)
        {
            if (d == null)
                throw new ArgumentException("Please select a LTL!");

            ro2.Print(ro2.DefaultPrinter, d, enumLabelType.ltLTE);
        }

        private void mnuReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Reprint((DataItemReadout) dgData.SelectedItem, enumLabelType.ltLTS);
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void mnuLTE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Reprint((DataItemReadout)dgData.SelectedItem, enumLabelType.ltLTE);
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void DisplayData()
        {
            SqlDataReader dr = ro2.getDatatoExport(ro2.sap.FAUF, ro2.KAUF);

            //int total = LTCSData.getTotalData(sap.FAUF, txtKundenAuftrag.Text);

            int i = 1;
            while (dr.Read())
            {
                dgData.Items.Add(new DataItemReadout { Pos = i, ModuleNr = dr["ModuleNr"].ToString(), SerialNr = dr["SerialNr"].ToString(), mA = Int32.Parse(dr["mA"].ToString()), OK = Boolean.Parse(dr["OK"].ToString()), Comment = dr["Comment"].ToString(), KZ = dr["KZ"].ToString(), TestDate = DateTime.Parse(dr["testDate"].ToString()) });
                i++;
            }
        }
    }
}
