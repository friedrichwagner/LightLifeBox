using System;
using System.Windows.Controls;
using System.IO;


namespace LightLife.Content
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        private const string sVersion = "0.1";

        public About()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            txtVersion.Text="Light Life - Admin Console " + sVersion;
            txtBuild.Text= File.GetLastWriteTime(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
        }
    }
}
