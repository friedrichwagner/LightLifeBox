using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LightLifeAdminConsole.MVVM;
using LightLife;
using Lumitech.Helpers;
using PILEDServer;

namespace LightLifeAdminConsole.Content.PILED
{
    /// <summary>
    /// Interaction logic for RGB.xaml
    /// </summary>
    public partial class XY : UserControl
    {
        private PiledVM dc;

        public XY()
        {
            InitializeComponent();
            dc = PiledVM.GetInstance();
            DataContext = dc;
        }

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

                Point p = new Point(Single.Parse(txtX.Text), Single.Parse(txtY.Text));

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
            Point p = new Point(cie.x, cie.y);
            CIEChart.RemoveAllPoints(cnvCIE);
            CIEChart.DrawPoint(cnvCIE, CIEChart.XyToPixel(ref p), "lastpoint");
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            char[] charsToTrim = { '(', ')'};
            Point p = e.GetPosition((Canvas)sender);

            CIEChart.RemoveAllPoints(cnvCIE);
            CIEChart.DrawPoint(cnvCIE, p, "lastpoint");
            dc.X = Single.Parse(lblX.Content.ToString().Trim(charsToTrim));
            dc.Y = Single.Parse(lblY.Content.ToString().Trim(charsToTrim));
        }


        private void cnvCIE_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition((Canvas)sender);
            Point xy = CIEChart.PixelToXY(ref p);

            lblX.Content = String.Format("({0:0.000})", xy.X);
            lblY.Content = String.Format("({0:0.000})", xy.Y);
        }
    }
}
