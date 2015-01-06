﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PILEDServer
{
    public class SysTrayApp : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private UDPServer UDPSrv;

        public SysTrayApp()
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Stop", OnStartStop);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "PI-LED Server";
            trayIcon.Icon = Properties.Resources.faviconCAIE033I; //Lumitech

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;


            //FW 2.12.2014
            UDPSrv = new UDPServer();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
            
            
            OnStartStop(this, e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            //Stop UDPSrv
            OnStartStop(this, e);
            Application.Exit();
        }

        private void OnStartStop(object sender, EventArgs e)
        {
            if (UDPSrv.isStarted) 
            { 
                //Stop it
                UDPSrv.Stop();
                trayMenu.MenuItems[0].Text = "Start";
            }
            else
            {
                //Start it
                UDPSrv.Start();
                trayMenu.MenuItems[0].Text = "Stop";
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}