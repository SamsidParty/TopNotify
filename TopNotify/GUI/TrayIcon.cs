using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using WebFramework.Backend;

namespace TopNotify.GUI
{
    public class TrayIcon
    {

        public static Assembly WinForms;


        public static dynamic Application = null;
        //Used By Interceptors To Find Screen Resolution
        public static dynamic Screen = null;


        /// <summary>
        /// Dynamically Loads Winforms And Sets Up A Tray Icon
        /// </summary>
        public static void Setup()
        {
            WinForms = Assembly.LoadFile(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Windows.Forms.dll");

            AppDomain.CurrentDomain.AssemblyResolve += FindAssembly;

            dynamic notify = null;
            dynamic menuStrip = null;
            dynamic handler = null;

            //Find WinForms Types
            foreach (Type type in WinForms.GetExportedTypes())
            {
                if (type.Name == "Application")
                {
                    Application = type.GetMethods()
                        .Where((method) => method.Name == "Run" && method.IsStatic && method.GetParameters().Length == 0)
                        .First();
                }
                else if (type.Name == "Screen")
                {
                    Screen = type.GetProperties()
                        .Where((prop) => prop.Name == "PrimaryScreen")
                        .First()
                        .GetValue(null, null);
                }
                else if (type.Name == "NotifyIcon")
                {
                    // notify = new NotifyIcon();
                    notify = Activator.CreateInstance(type);
                }
                else if (type.Name == "ContextMenuStrip")
                {;
                    // menuStrip = new ContextMenuStrip();
                    menuStrip = Activator.CreateInstance(type);
                }
                else if (type.Name == "ToolStripItemClickedEventHandler")
                {
                    // handler = new ToolStripItemClickedEventHandler(Quit);
                    handler = Delegate.CreateDelegate(type, typeof(TrayIcon).GetMethod(nameof(Quit)));
                }
            }

            //Use WinForms Methods To Create A Tray Icon
            notify.Visible = true;
            notify.Icon = Util.FindAppIcon();
            notify.Text = "SamsidParty Top Notify";
            notify.DoubleClick += new EventHandler(LaunchSettingsMode);
            notify.ContextMenuStrip = menuStrip;
            notify.ContextMenuStrip.Items.Add("Quit TopNotify");
            notify.ContextMenuStrip.ItemClicked += handler;
        }

        //Quick And Dirty Method Of Loading WinForms Dependencies
        private static Assembly? FindAssembly(object? sender, ResolveEventArgs args)
        {

            if (args.Name.StartsWith("Accessibility"))
            {
                return Assembly.LoadFile(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Accessibility.dll");
            }            

            return null;
        }

        public static void MainLoop()
        {
            Application.Invoke(null, null);
        }


        public static void Quit(object Sender, EventArgs e)
        {
            Logger.CloseLog();

            //Clean Up
            Daemon.Daemon.Instance.Server.Stop();

            //Kill Other Instances
            var instances = Process.GetProcessesByName("TopNotify");
            foreach (var instance in instances)
            {
                if (instance.Id != Process.GetCurrentProcess().Id)
                {
                    try
                    {
                        instance.Kill();
                    }
                    catch { }
                }
            }


            Environment.Exit(0);
        }

        public static void LaunchSettingsMode(object Sender, EventArgs e)
        {
            try
            {
                var exe = Util.FindExe();
                var psi = new ProcessStartInfo(exe, "--settings" + (Debugger.IsAttached ? " --debug-process" : "")); // Use Debug Args If Needed
                psi.UseShellExecute = false;
                psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var proc = Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

    }
}
