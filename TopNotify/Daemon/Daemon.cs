using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using TopNotify.Common;
using TopNotify.GUI;
using KdSoft.MailSlot;

namespace TopNotify.Daemon
{
    public class Daemon
    {
        public static Daemon Instance;

        public InterceptorManager Manager;

        public Daemon() {
            Instance = this;

            TrayIcon.Setup();

            Thread managerThread = new Thread(CreateManager);
            managerThread.Start();

            Task.Run(MailSlotListener);

            TrayIcon.MainLoop();
        }

        /// <summary>
        /// Listens for messages that affect the app lifecycle
        /// </summary>
        async Task MailSlotListener()
        {
            var listener = new AsyncMailSlotListener("samsidparty_topnotify", Encoding.ASCII.GetBytes("\n")[0]);
            await foreach (var msgBytes in listener.GetNextMessage())
            {
                var msg = Encoding.UTF8.GetString(msgBytes);
                
                if (msg == "UpdateConfig") // Runs when the user changes a setting from the GUI
                {
                    InterceptorManager.Instance.OnSettingsChanged();
                }
            }
        }

        /// <summary>
        /// This should be called from an external (non-daemon) process to send a message to the daemon
        /// </summary>
        public static void SendCommandToDaemon(string message)
        {
            try
            {
                var buffer = new byte[1024];
                using (var client = MailSlot.CreateClient("samsidparty_topnotify"))
                {
                    var bytes = Encoding.UTF8.GetBytes(message + "\n");
                    client.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex) { }
        }

        public void CreateManager()
        {
            Manager = new InterceptorManager();
            Manager.Start();
        }
    }
}
