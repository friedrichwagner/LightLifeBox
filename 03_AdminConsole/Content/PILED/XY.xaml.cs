using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LightLifeAdminConsole.MVVM;
using LightLife;
using Lumitech.Helpers;
using FirstFloor.ModernUI.Windows;
using LightLife.Data;
using System.Drawing;

namespace LightLifeAdminConsole.Content.PILED
{
    /// <summary>
    /// Interaction logic for RGB.xaml
    /// </summary>
    public partial class XY : UserControl, IContent
    {
        private PiledVM dc;

        public XY()
        {
            InitializeComponent();
            dc = PiledVM.GetInstance();
            DataContext = dc;
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) { }
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            //damit in den CCT Mode umgeschalten wird
            dc.CCT = dc.lldata.piled.cct;
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CIEChart.DrawPlanck(cnvCIE);
            CIEChart.DrawPILedTriangle(cnvCIE);            
        }

        private void txtX_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void txtX_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                float val = Math.Abs(Single.Parse((sender as TextBox).Text));

                if (val > 1)
                {
                    (sender as TextBox).Text = "0," + (sender as TextBox).Text;
                }

                PointF p = new PointF(Single.Parse(txtX.Text), Single.Parse(txtY.Text));

                CIEChart.RemoveAllPoints(cnvCIE);
                CIEChart.DrawPoint(cnvCIE, CIEChart.XyToPixel(ref p), "lastpoint");
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void txtCCT_LostFocus(object sender, RoutedEventArgs e)
        {
            //in "LostFocus" hat das "Binding" die Daten im VieModel noch nicht aktualisiert --> also Workaround
            CIECoords cie = Photometric.CCT2xy(Double.Parse(txtCCT.Text), PILEDData.obs);
            PointF p = new PointF((float)cie.x, (float)cie.y);
            CIEChart.RemoveAllPoints(cnvCIE);
            CIEChart.DrawPoint(cnvCIE, CIEChart.XyToPixel(ref p), "lastpoint");
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            char[] charsToTrim = { '(', ')'};
            System.Windows.Point p = e.GetPosition((Canvas)sender);
            PointF pf = new PointF((float)p.X, (float)p.Y);

            CIEChart.RemoveAllPoints(cnvCIE);
            CIEChart.DrawPoint(cnvCIE, pf, "lastpoint");
            dc.X = Single.Parse(lblX.Content.ToString().Trim(charsToTrim));
            dc.Y = Single.Parse(lblY.Content.ToString().Trim(charsToTrim));
        }


        private void cnvCIE_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition((Canvas)sender);
            PointF pf = new PointF((float)p.X, (float)p.Y);
            PointF xy = CIEChart.PixelToXY(ref pf);

            lblX.Content = String.Format("({0:0.000})", xy.X);
            lblY.Content = String.Format("({0:0.000})", xy.Y);
        }
    }
}
