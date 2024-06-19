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

namespace TopNotify.Daemon
{
    public class Daemon
    {
        public static Daemon Instance;

        public InterceptorManager Manager;
        public IPCServer Server;

        public Daemon() {
            Instance = this;


            if (Settings.Get().EnableDebugNotifications)
            {
                NotificationTester.Toast("Debug Notification", "Interceptor Daemon Started");
            }

            TrayIcon.Setup();

            Thread managerThread = new Thread(CreateManager);
            managerThread.Start();

            Thread ipcThread = new Thread(CreateIPC);
            ipcThread.Start();

            TrayIcon.MainLoop();
        }

        public void CreateManager()
        {
            Manager = new InterceptorManager();
            Manager.Start();
        }

        public void CreateIPC()
        {
            Server = new IPCServer();
            Server.Start();
        }
    }
}
