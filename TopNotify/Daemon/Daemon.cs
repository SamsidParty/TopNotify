using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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

            TrayIcon.Setup();
            Thread managerThread = new Thread(CreateManager);
            managerThread.Start();

            TrayIcon.MainLoop();
        }

        public void CreateManager()
        {
            Manager = new InterceptorManager();
            Manager.Start();
        }
    }
}
