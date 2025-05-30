﻿using System;
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
        public UserNotificationListener Listener;
        public bool CanListenToNotifications = false;

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
                foreach (Interceptor i in Interceptors)
                {
                    try
                    {
                        i.OnNotification(userNotification);
                    }
                    catch { }
                }
            }

            Update();
        }

        public void OnSettingsChanged()
        {
            CurrentSettings = Settings.Get();

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
