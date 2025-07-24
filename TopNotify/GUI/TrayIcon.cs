using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using TopNotify.Daemon;

namespace TopNotify.GUI
{
    /// <summary>
    /// This class makes heavy use of reflection because winforms isnt actually referenced in the project.
    /// </summary>
    public class TrayIcon
    {
        public static Assembly WinForms;
        public static dynamic Application = null;


        /// <summary>
        /// Dynamically Loads Winforms And Sets Up A Tray Icon.
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
                else if (type.Name == "NotifyIcon")
                {
                    // notify = new NotifyIcon();
                    notify = Activator.CreateInstance(type);
                }
                else if (type.Name == "ContextMenuStrip")
                {
                    // menuStrip = new ContextMenuStrip();
                    menuStrip = Activator.CreateInstance(type);
                }
                else if (type.Name == "ToolStripItemClickedEventHandler")
                {
                    // handler = new ToolStripItemClickedEventHandler(Quit);
                    handler = Delegate.CreateDelegate(type, typeof(TrayIcon).GetMethod(nameof(OnTrayButtonClicked)));
                }
            }

            //Use WinForms Methods To Create A Tray Icon
            notify.Visible = true;
            notify.Icon = Util.FindAppIcon();
            notify.Text = "SamsidParty TopNotify";
            notify.DoubleClick += new EventHandler(LaunchSettingsMode);
            notify.ContextMenuStrip = menuStrip;
            notify.ContextMenuStrip.Items.Add("Create Bug Report");
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


        public static void OnTrayButtonClicked(object Sender, EventArgs e)
        {
            // var item = e.ClickedItem;
            var item = e.GetType().GetProperty("ClickedItem")!.GetValue(e);
            var itemText = item.GetType().GetProperty("Text")!.GetValue(item).ToString();

            if (itemText == "Create Bug Report")
            {
                BugReport.DisplayBugReport(BugReport.CreateBugReport());
            }
            else if (itemText == "Quit TopNotify")
            {
                Quit();
            }   
        }

        public static void Quit()
        {
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
                
            }
        }

    }
}
