using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using WebFramework.Backend;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace TopNotify.Daemon
{
    public class InterceptorManager
    {
        public static InterceptorManager Instance;
        public List<Interceptor> Interceptors = new();
        public Settings CurrentSettings;
        public int TimeSinceReflow = 0;
        public UserNotificationListener Listener;

        public const int ReflowTimeout = 50;

        public void Start()
        {
            Instance = this;
            CurrentSettings = Settings.Get();

            Interceptors.Add(new NativeInterceptor());
            Interceptors.Add(new SoundInterceptor());
            Interceptors.Add(new TeamsInterceptor());


            Listener = UserNotificationListener.Current;
            Task.Run(async () =>
            {

                //Ask For Permissions To Read Notifications (Should Be Automatically Granted)
                var access = await Listener.RequestAccessAsync();
                if (access != UserNotificationListenerAccessStatus.Allowed)
                {
                    var msg = "Failed To Start Notification Listener: Permission Denied";
                    Logger.LogError(msg);
                    NotificationTester.Toast("Something Went Wrong", msg);
                    return;
                }

                try
                {
                    //Throws a COM exception if not packaged into an MSIX app
                    //Currently no workaround
                    Listener.NotificationChanged += OnNotificationChanged;
                }
                catch (Exception ex)
                {
                    Logger.LogError("Could Not Intercept Notifications Because The Application Is Not Packaged");
                }


            });

            foreach (Interceptor i in Interceptors)
            {
                i.Start();
            }
            MainLoop();
        }

        public void MainLoop()
        {
            while (true)
            {
                TimeSinceReflow++;

                if (TimeSinceReflow > ReflowTimeout)
                {
                    TimeSinceReflow = 0;
                    Reflow();
                }

                Update();

                Thread.Sleep(10);
            }
        }

        public void Reflow()
        {
            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.Reflow();
                }
                catch { }
            }
        }

        public void Update()
        {
            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.Update();
                }
                catch { }
            }
        }

        //Runs When A New Notification Is Added Or Removed
        public void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            Update();
        }

        public void OnSettingsChanged()
        {

            //Send The New Settings File To All IPC Clients
            if (Daemon.Instance.Server != null)
            {
                Daemon.Instance.Server.UpdateSettingsFile();
            }

            CurrentSettings = Settings.Get();

            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.Restart();
                    i.Reflow();
                }
                catch { }
            }
        }
    }
}
