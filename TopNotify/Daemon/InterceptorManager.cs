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
        public const int ReflowTimeout = 50;

        public List<uint> HandledNotifications = new List<uint>();
        public UserNotificationListener Listener;
        public bool CanListenToNotifications = false;

        public static Interceptor[] InstalledInterceptors = { new NativeInterceptor(), new SoundInterceptor(), new TeamsInterceptor(), new ReadAloudInterceptor() };

        public void Start()
        {
            Instance = this;
            CurrentSettings = Settings.Get();
            AppReference.EnsurePresetsAreValid();

            foreach (var possibleInterceptor in InstalledInterceptors)
            {
                // Check If It's Eligible To Be Enabled
                if (possibleInterceptor.ShouldEnable())
                {
                    Interceptors.Add(possibleInterceptor);
                }
            }


            Listener = UserNotificationListener.Current;
            Task.Run(async () =>
            {

                //Ask For Permissions To Read Notifications
                var access = await Listener.RequestAccessAsync();
                if (access != UserNotificationListenerAccessStatus.Allowed)
                {
                    var msg = "Failed To Start Notification Listener: Permission Denied";
                    DaemonErrorHandler.ThrowNonCritical(new DaemonError("listener_failure_no_permission", msg));
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
                    var msg = "Failed To Start Notification Listener: Not Packaged";
                    DaemonErrorHandler.ThrowNonCritical(new DaemonError("listener_failure_not_packaged", msg));
                    return;
                }

                CanListenToNotifications = true;

            });

            foreach (Interceptor i in Interceptors)
            {
                Logger.LogInfo("Started Interceptor: " + i.GetType().Name);
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
            if (Daemon.Instance.Server != null)
            {
                Daemon.Instance.Server.UpdateHandles();
            }

            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.Reflow();
                }
                catch (Exception ex) { Logger.LogError(ex.ToString()); }
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
                catch (Exception ex) { Logger.LogError(ex.ToString()); }
            }
        }

        //Runs When A New Notification Is Added Or Removed
        public async void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            var userNotifications = await Listener.GetNotificationsAsync(NotificationKinds.Toast);
            var toBeRemoved = new List<uint>(HandledNotifications);

            foreach (var userNotification in userNotifications)
            {
                if (HandledNotifications.Contains(userNotification.Id))
                {
                    toBeRemoved.Remove(userNotification.Id);
                }
                else
                {
                    //Only Count As New Notification If It's Less Than A Second Old
                    if (DateTime.UtcNow - userNotification.CreationTime.UtcDateTime < TimeSpan.FromSeconds(1))
                    {
                        foreach (Interceptor i in Interceptors)
                        {
                            try
                            {
                                i.OnNotification(userNotification);
                            }
                            catch { }
                        }
                        HandledNotifications.Add(userNotification.Id);
                    }
                }
            }

            foreach (uint id in toBeRemoved)
            {
                HandledNotifications.Remove(id);
            }

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
                catch (Exception ex) { Logger.LogError(ex.ToString()); }
            }
        }
    }
}
