using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControlBoxTester
{
    public partial class Form1 : Form
    {
        TCPServer srv;
        public Form1()
        {
            InitializeComponent();
            srv = new TCPServer(10000);
            srv.NewClientConnected += NewClientConnected;
            srv.NewMessage += NewMessage;
        }

        void NewClientConnected(string msg, int cnt, bool added)
        {
             if (this.InvokeRequired)
             {
                 this.Invoke(new Action<string, int, bool>(NewClientConnected), msg, cnt, added);
             }
             else
             {
                 if (added)
                 {
                     rtbServer.AppendText("Added: " + msg + Environment.NewLine);
                 }
                 else
                     rtbServer.AppendText("Remove: " + msg + Environment.NewLine);

                 txtNrConnections.Text = cnt.ToString();
             }
        }

        void NewMessage(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(NewMessage), msg);
            }
            else
            {
                rtbServer.AppendText(msg + Environment.NewLine);                
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            srv.Close();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            /*if (sender is Button)
            {
                string s = ((sender as Button).Tag).ToString();
                srv.SetButton(s, 5);
            }*/
        }

        private void tb1_Scroll(object sender, EventArgs e)
        {
            if (sender is TrackBar)
            {
                string s = ((sender as TrackBar).Tag).ToString();
                int val = (sender as TrackBar).Value;
                srv.SetPoti(s, val);
            }

        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Button)
            {
                string s = ((sender as Button).Tag).ToString();
                srv.SetButton(s, -1);
            }
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is Button)
            {
                string s = ((sender as Button).Tag).ToString();
                srv.SetButton(s, 1001);
            }
        }
    }
}
