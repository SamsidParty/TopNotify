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
        public UserNotificationListener Listener;
        public bool CanListenToNotifications = false;

        // Update() Runs Every ~10ms Via MainLoop; A Persistently-Throwing Interceptor Would
        // Otherwise Flood daemon.log At That Rate. Log The First Occurrence Immediately, Then
        // At Most Once Per 30s Per (Interceptor, Method) Pair.
        readonly ConcurrentDictionary<string, DateTime> lastLoggedFailure = new ConcurrentDictionary<string, DateTime>();
        static readonly TimeSpan FailureLogThrottle = TimeSpan.FromSeconds(30);

        void LogThrottled(string interceptorName, string methodName, Exception ex)
        {
            var key = $"{interceptorName}.{methodName}";
            var now = DateTime.UtcNow;

            if (!lastLoggedFailure.TryGetValue(key, out var last) || now - last >= FailureLogThrottle)
            {
                lastLoggedFailure[key] = now;
                Program.Logger.Warning(ex, $"{key}() failed");
            }
        }

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
                catch (Exception ex) { LogThrottled(i.GetType().Name, "Reflow", ex); }
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
                catch (Exception ex) { LogThrottled(i.GetType().Name, "Update", ex); }
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
                catch (Exception ex) { LogThrottled(i.GetType().Name, "OnKeyUpdate", ex); }
            }
        }

        // Runs When A New Notification Is Added Or Removed.
        // WinRT Event Handlers Must Be void, So This Stays A Thin Wrapper Around An
        // async Task Body That Owns All Its Own Exceptions - An Unhandled Exception
        // Inside An `async void` Method Can't Be Caught By Anything Upstream And Would
        // Otherwise Take Down The Whole Notification Listener Silently.
        public void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            _ = OnNotificationChangedAsync(sender, args);
        }

        async Task OnNotificationChangedAsync(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            try
            {
                if (args.ChangeKind != UserNotificationChangedKind.Added)
                {
                    return;
                }

                // GetNotificationsAsync Is The Only WinRT Call Available - There's No
                // Single-Item Lookup By Id, So This Full Re-Fetch Isn't Avoidable.
                var userNotifications = await Listener.GetNotificationsAsync(NotificationKinds.Toast);
                var userNotification = userNotifications.FirstOrDefault((n) => n.Id == args.UserNotificationId);

                if (userNotification == null)
                {
                    // Can Happen If The Notification Was Already Dismissed/Replaced By The
                    // Time This Round Trip Resolves. Previously This Silently Passed null
                    // Into Every Interceptor; Now It's A Logged, Explicit Skip.
                    Program.Logger.Warning($"OnNotificationChanged: notification {args.UserNotificationId} not found in snapshot, skipping");
                    return;
                }

                foreach (Interceptor i in Interceptors)
                {
                    try
                    {
                        i.OnNotification(userNotification);
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Warning(ex, $"{i.GetType().Name}.OnNotification() failed");
                    }
                }

                Update();
            }
            catch (Exception ex)
            {
                // This Is The Critical Catch: A COM/WinRT Hiccup In GetNotificationsAsync
                // No Longer Kills The Listener For Every Notification After It.
                DaemonErrorHandler.ThrowNonCritical(new DaemonError("notification_changed_failure", "Failed to process an incoming notification: " + ex.Message));
            }
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
                catch (Exception ex) { Program.Logger.Warning(ex, $"{i.GetType().Name} failed to apply settings change"); }
            }
        }
    }
}
