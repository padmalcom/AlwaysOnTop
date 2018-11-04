using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

namespace AlwaysOnTop
{
    public partial class Form1 : Form
    {
        private List<Process> processes = new List<Process>();

        const string APP_NAME = "AlwaysOnTop";

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTTOPMOST = new IntPtr(-2);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        const UInt32 WS_EX_TOPMOST = 0x0008;
        const int GWL_EXSTYLE = (-20);

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public Form1()
        {
            InitializeComponent();
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayMenu.Popup += new System.EventHandler(trayIconClick);

            trayIcon = new NotifyIcon();
            trayIcon.Text = APP_NAME;
            trayIcon.Icon = AlwaysOnTop.Properties.Resources.top;

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        private void trayIconClick(object sender, System.EventArgs e)
        {

            trayMenu.MenuItems.Clear();
            processes.Clear();
            Process[] localProcesses = Process.GetProcesses();

            for (int i = localProcesses.Length - 1; i >= 0; i--)
            {
                if (localProcesses[i].MainWindowHandle != IntPtr.Zero)
                {
                    processes.Add(localProcesses[i]);
                    MenuItem newItem = trayMenu.MenuItems.Add(localProcesses[i].ProcessName, OnMenuItemClick);

                    if ((GetWindowLong(localProcesses[i].MainWindowHandle, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0)
                    {
                        newItem.Checked = true;
                    }
                }
            }

            trayMenu.MenuItems.Add("-");
            MenuItem autostartMenuItem = trayMenu.MenuItems.Add("Autostart", OnStartUpClick);

            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk.GetValue(APP_NAME) != null)
            {
                autostartMenuItem.Checked = true;
            }
            trayMenu.MenuItems.Add("About", OnAbout);
            trayMenu.MenuItems.Add("Exit", OnExit);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        private void OnMenuItemClick(object sender, EventArgs e)
        {
            int processId = ((MenuItem)sender).Index;
            bool isSet = ((MenuItem)sender).Checked;

            if (processes.Count > 0)
            {
                if (!isSet)
                {
                    IntPtr handle = processes[processId].MainWindowHandle;
                    SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                } else
                {
                    IntPtr handle = processes[processId].MainWindowHandle;
                    SetWindowPos(handle, HWND_NOTTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                }
            }
        }

        private void OnStartUpClick(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk.GetValue(APP_NAME) == null)
            {
                rk.SetValue(APP_NAME, Application.ExecutablePath.ToString());
            } else
            {
                rk.DeleteValue(APP_NAME, false);
            }
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(APP_NAME + " v1.1 - 2015 - Jonas Freiknecht - Visit www.jofre.de.");
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
 
    }
}
