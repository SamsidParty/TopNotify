using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using static TopNotify.Daemon.NativeInterceptor;

namespace TopNotify.Daemon
{
    public class InterceptorManager
    {
        #region WinAPI Methods

        #endregion

        public static InterceptorManager Instance;
        public List<Interceptor> Interceptors = new();

        public Settings CurrentSettings;

        public int TimeSinceReflow = 0;
        public const int ReflowTimeout = 50;

        public ConcurrentDictionary<uint, Action?> CleanUpFunctions = new ConcurrentDictionary<uint, Action?>(); // Maps HandledNotifications to the associated clean up function
        public ConcurrentDictionary<uint, bool> KnownNotificationIds = new ConcurrentDictionary<uint, bool>();
        public UserNotificationListener Listener;
        public bool CanListenToNotifications = false;
        public bool IsPollingNotifications = false;
        public DateTime LastNotificationPoll = DateTime.MinValue;

        public static Interceptor[] InstalledInterceptors =
        {
            new NativeInterceptor(),
            new DiscoveryInterceptor(),
            new SoundInterceptor(),
            new ReadAloudInterceptor()
        };

        public void Start()
        {
            Instance = this;
            CurrentSettings = Settings.Get();
            NotificationLifetimeController.Apply(CurrentSettings);

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
                    Program.Logger.Warning("Failed To Start Notification Listener: Permission Denied");
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
                    Program.Logger.Warning("Failed To Start Notification Listener: Not Packaged");
                    return;
                }

                CanListenToNotifications = true;
                PollNotifications();

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

                PollNotifications();
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
                catch (Exception ex) { }
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
                catch (Exception ex) { }
            }
        }

        // Called from C++ to safely invoke the OnKeyUpdate method
        public static void TryOnKeyUpdate()
        {
            if (Instance == null) { return; }
            Instance.OnKeyUpdate();
        }

        public void OnKeyUpdate()
        {
            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.OnKeyUpdate();
                }
                catch (Exception ex) { }
            }
        }

        // Runs When A New Notification Is Added Or Removed
        public async void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            var userNotifications = await Listener.GetNotificationsAsync(NotificationKinds.Toast);
            var userNotification = userNotifications.Where((n) => n.Id == args.UserNotificationId).FirstOrDefault();

            if (args.ChangeKind == UserNotificationChangedKind.Added)
            {
                if (KnownNotificationIds.TryAdd(args.UserNotificationId, true))
                {
                    DispatchNotification(userNotification);
                }
            }
            else
            {
                KnownNotificationIds.TryRemove(args.UserNotificationId, out _);
            }

            Update();
        }

        public void PollNotifications()
        {
            if (!CanListenToNotifications) { return; }
            if (IsPollingNotifications) { return; }
            if ((DateTime.Now - LastNotificationPoll).TotalMilliseconds < 500) { return; }

            IsPollingNotifications = true;
            LastNotificationPoll = DateTime.Now;

            Task.Run(async () =>
            {
                try
                {
                    var userNotifications = await Listener.GetNotificationsAsync(NotificationKinds.Toast);
                    foreach (var notification in userNotifications)
                    {
                        if (KnownNotificationIds.TryAdd(notification.Id, true))
                        {
                            DispatchNotification(notification);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.Logger?.Warning($"Failed to poll notifications: {ex.Message}");
                }
                finally
                {
                    IsPollingNotifications = false;
                }
            });
        }

        public void DispatchNotification(UserNotification userNotification)
        {
            if (userNotification == null) { return; }

            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.OnNotification(userNotification);
                }
                catch { }
            }
        }

        public void OnSettingsChanged()
        {
            CurrentSettings = Settings.Get();
            NotificationLifetimeController.Apply(CurrentSettings);

            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.Restart();
                    i.Reflow();
                }
                catch (Exception ex) { }
            }
        }
    }
}
