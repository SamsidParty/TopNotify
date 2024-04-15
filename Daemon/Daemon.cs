using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace SamsidParty_TopNotify
{
    public class Daemon
    {
        public InterceptorManager Manager;

        public Daemon() {

            if (Settings.Get().EnableDebugNotifications)
            {
                NotificationTester.Toast("Debug Notification", "Interceptor Daemon Started");
            }

            SetupTrayIcon();
            Thread managerThread = new Thread(CreateManager);
            managerThread.Start();
        }

        public void SetupTrayIcon()
        {
            //Use WinForms Methods To Create A Tray Icon
            NotifyIcon notify = new NotifyIcon();
            notify.Visible = true;
            notify.Icon = Util.FindAppIcon();
            notify.Text = "SamsidParty Top Notify";
            notify.DoubleClick += new EventHandler(LaunchSettingsMode);
            notify.ContextMenuStrip = new ContextMenuStrip();
            notify.ContextMenuStrip.Items.Add("Quit TopNotify");
            notify.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(Quit);
        }

        public void Quit(object Sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        public void LaunchSettingsMode(object Sender, EventArgs e)
        {
            try
            {
                var exe = Util.FindExe();
                var psi = new ProcessStartInfo(exe, "--settings");
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var proc = Process.Start(psi);
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }
        }

        public void CreateManager()
        {
            Manager = new InterceptorManager();
            Manager.Start();
        }

        public void MainLoop()
        {
            Application.Run();
        }
    }
}
