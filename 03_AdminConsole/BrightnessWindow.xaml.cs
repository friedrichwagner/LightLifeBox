using FirstFloor.ModernUI.Windows.Controls;
using LightLifeAdminConsole.MVVM;
using System.Windows;
using System.Windows.Input;

namespace LightLifeAdminConsole
{
    /// <summary>
    /// Interaction logic for BrightnessWindow.xaml
    /// </summary>
    public partial class BrightnessWindow : ModernWindow
    {
        private PiledVM dc;

        public BrightnessWindow()
        {
            InitializeComponent();
            dc = PiledVM.GetInstance();
            DataContext = dc;

            ResizeMode = ResizeMode.NoResize;
            IsTitleVisible = false;
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public void Close()
        {
            this.Closing -= ModernWindow_Closing;
            //Add closing logic here.
            base.Close();
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void ModernWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.F12))
            {
                if (this.Visibility == Visibility.Hidden)
                    Show();
                else
                    Hide();
            }
        }
    }
}
