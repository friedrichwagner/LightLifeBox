using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Lumitech;
using FirstFloor.ModernUI.Windows;
using PILEDTestSuite.MVVM;

namespace FlashNPrint.Pages
{


    /// <summary>
    /// Interaction logic for Print.xaml
    /// </summary>
    public partial class Print : UserControl, IContent
    {
        private PrintViewModel VModel;

        public Print()
        {
            InitializeComponent();
            VModel = new PrintViewModel();
            this.DataContext = VModel;

            //DisplayData(true);
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)  { }      

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtFAUF.Focus();
            txtFAUF.SelectAll();
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Tab))
            {
                if (VModel.getData(txtFAUF.Text))
                    DisplayData(false);

                txtFAUF.SelectAll();
                e.Handled = false;
            }

            if (e.Key == Key.F12) VModel.Print((SAPArticle)dgData.SelectedItem);
        }

        private void DisplayData(bool bEmpty)
        {
            try
            {
                dgData.Items.Clear();
                if (!bEmpty)
                {                    
                    //dgData.Items.Add(getDataItem("Fertigprodukt", VModel.Fert));
                    dgData.Items.Add(VModel.Fert);

                    int i = 1;
                    foreach (KeyValuePair<string, SAPArticle> p in VModel.Labels)
                    {
                        //dgData.Items.Add(getDataItem("Label" + i.ToString(), p.Value));
                        dgData.Items.Add(p.Value);
                        i++;
                    }

                    if (i > 1)
                        dgData.SelectedIndex = 1;
                }
                else
                {
                    /*dgData.Items.Add(getDataItem("Fertigprodukt", null));

                    for (int i=1;i<=4;i++) 
                        dgData.Items.Add(getDataItem("Label" + i.ToString(), null));*/
                }
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        /*private DataItem getDataItem(string descr, SAPArticle s)
        {
            if (s != null)
                return new DataItem { RowDescription = descr, ArtNo = s.Nr, Desc = s.Description, Prop1 = s.UProp1, Prop2 = s.UProp1, Qty = s.Qty.ToString() };
            else
                return new DataItem { RowDescription = descr, ArtNo = "", Desc = "", Prop1 = "", Prop2 = "", Qty = "" };
        }*/
    }
}
