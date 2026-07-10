using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Threading.Tasks;
using TopNotify.Common;
using SamsidParty_TopNotify.Daemon;
using Windows.UI.Notifications.Management;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using static TopNotify.Daemon.ResolutionFinder;

namespace TopNotify.Daemon
{

    public class NativeInterceptor : Interceptor
    {
        #region WinAPI Methods

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rectangle rectangle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        const UInt32 WM_CLOSE = 0x0010;
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;

        [DllImport("TopNotify.Native")]
        private static extern bool TopNotifyEnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        #endregion

        public IntPtr hwnd;
        public ExtendedStyleManager ExStyleManager;
        public int ScaledPreferredDisplayWidth;
        public int ScaledPreferredDisplayHeight;
        public int RealPreferredDisplayWidth;
        public int RealPreferredDisplayHeight;
        public float ScaleFactor;
        public DateTime CurrentNotificationStarted = DateTime.MinValue;
        public NotificationStickyMode CurrentStickyMode = NotificationStickyMode.WindowsDefault;
        public int CurrentStickyDurationSeconds = 5;
        public bool CurrentNotificationClosed = false;
        public bool WasNotificationVisible = false;
        public uint? CurrentNotificationId = null;
        public IntPtr LastClosedHwnd = IntPtr.Zero;
        public Dictionary<IntPtr, string> ClosedNotificationSignatures = new();

        public override void Start()
        {
            base.Start();
            ExStyleManager = new ExtendedStyleManager(new IntPtr(0x00200008)); // Magic Number, Default Notification Style
            Reflow();
        }

        public override void Restart()
        {
            base.Restart();
        }

        // Modified From https://stackoverflow.com/a/20276701/18071273
        public static IEnumerable<IntPtr> FindCoreWindows()
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            TopNotifyEnumWindows(delegate (IntPtr hwnd, IntPtr param)
            {
                var classGet = new StringBuilder(1024);
                GetClassName(hwnd, classGet, classGet.Capacity);
                if (classGet.ToString() == "Windows.UI.Core.CoreWindow")
                {
                    windows.Add(hwnd);
                }

                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public override void Reflow()
        {
            if (ExStyleManager == null) { return; } // Return If Start() Has Not Been Called Yet

            base.Reflow();

            try
            {
                var foundHwnd = FindWindow("Windows.UI.Core.CoreWindow", Language.NotificationName);

                if (Settings.EnableDebugForceFallbackMode)
                {
                    Program.Logger.Information($"Fallback detection is being forced");
                    foundHwnd = IntPtr.Zero; // Always use fallback mode if this setting is enabled
                }

                ScaledPreferredDisplayWidth = ResolutionFinder.GetScaledResolution().Width;
                ScaledPreferredDisplayHeight = ResolutionFinder.GetScaledResolution().Height;
                RealPreferredDisplayWidth = ResolutionFinder.GetRealResolution().Width;
                RealPreferredDisplayHeight = ResolutionFinder.GetRealResolution().Height;
                ScaleFactor = ResolutionFinder.GetInverseScale();

                //The Notification Isn't In A Supported Language
                if (foundHwnd == IntPtr.Zero)
                {
                    Program.Logger.Information($"Couldn't use language-specific window detection, using fallback detection");
                    //The Notification Window Is The Only One That Is 396 x 152
                    foreach (var win in FindCoreWindows())
                    {
                        Rectangle rect = new Rectangle();
                        GetWindowRect(win, ref rect);

                        if ((ScaledPreferredDisplayWidth - rect.X) == 396)
                        {
                            foundHwnd = win;
                        }
                    }
                }

                if (foundHwnd != IntPtr.Zero && hwnd != foundHwnd)
                {
                    Program.Logger.Information($"Found notification window {foundHwnd}");
                    hwnd = foundHwnd;
                    WasNotificationVisible = false;
                }
                else if (foundHwnd == IntPtr.Zero)
                {
                    Program.Logger.Error($"Couldn't find the handle of the notification window");
                }

                Update();

            }
            catch { }
        }

        public override void OnKeyUpdate()
        {
            // Delay until the keypress has been processed
            Task.Run(async () =>
            {
                await Task.Delay(100);
                ExStyleManager.Update(hwnd);
            });
            
            base.OnKeyUpdate();
        }

        public override void OnNotification(UserNotification notification)
        {
            if (notification == null) { return; }

            var appReference = AppReference.FromNotification(notification);
            StartNotificationLifetime(
                appReference?.GetStickyMode(Settings) ?? Settings.StickyMode,
                appReference?.GetStickyDurationSeconds(Settings) ?? Settings.StickyDurationSeconds,
                notification.AppInfo.DisplayInfo.DisplayName,
                notification.Id
            );
            Program.Logger.Information($"Notification sticky lifetime for {notification.AppInfo.DisplayInfo.DisplayName}: {CurrentStickyMode}, {CurrentStickyDurationSeconds} seconds");

            Reflow();

            base.OnNotification(notification);
        }

        public override void Update()
        {
            base.Update();

            if (hwnd == IntPtr.Zero) { return; }

            var isNotificationVisible = IsNotificationActuallyVisible(hwnd);
            var isSuppressedClosedHandle = isNotificationVisible && IsClosedNotificationWindow(hwnd);
            if (isSuppressedClosedHandle)
            {
                isNotificationVisible = false;
            }

            if (isNotificationVisible && !WasNotificationVisible && (CurrentNotificationStarted == DateTime.MinValue || CurrentNotificationClosed))
            {
                StartFallbackNotificationLifetime();
            }
            else if (!isNotificationVisible && !isSuppressedClosedHandle)
            {
                CurrentNotificationStarted = DateTime.MinValue;
                CurrentNotificationClosed = false;
                LastClosedHwnd = IntPtr.Zero;
            }
            WasNotificationVisible = isNotificationVisible;

            // Update extended styles
            ExStyleManager.Update(hwnd);

            if (ShouldCloseCurrentNotification())
            {
                if (CurrentNotificationId != null)
                {
                    try
                    {
                        InterceptorManager.Instance.Listener.RemoveNotification(CurrentNotificationId.Value);
                        InterceptorManager.Instance.KnownNotificationIds.TryRemove(CurrentNotificationId.Value, out _);
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Warning($"Failed to remove notification {CurrentNotificationId.Value}: {ex.Message}");
                    }
                }

                ClosedNotificationSignatures[hwnd] = GetNotificationWindowSignature(hwnd);
                DismissNotificationWindow(hwnd);
                ShowWindow(hwnd, SW_HIDE);
                SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                CurrentNotificationClosed = true;
                WasNotificationVisible = false;
                LastClosedHwnd = hwnd;
                Program.Logger.Information($"Closed notification after {CurrentStickyDurationSeconds} seconds");
                hwnd = IntPtr.Zero;
                return;
            }

            if (ShouldKeepCurrentNotificationVisible())
            {
                ShowWindow(hwnd, SW_SHOW);
            }

            // Find The Bounds Of The Notification Window
            Rectangle NotifyRect = new Rectangle();
            GetWindowRect(hwnd, ref NotifyRect);

            // Find The Bounds Of The Preferred Monitor
            var hMonitor = ResolutionFinder.GetPreferredDisplay();
            MonitorInfo currentMonitorInfo = new MonitorInfo();
            ResolutionFinder.GetMonitorInfo(hMonitor, currentMonitorInfo);
            var originX = currentMonitorInfo.Monitor.Left;
            var originY = currentMonitorInfo.Monitor.Top;

            var scaledWidth = (int)((NotifyRect.Width - NotifyRect.X * ScaleFactor));
            var scaledHeight = (int)((NotifyRect.Height - NotifyRect.Y * ScaleFactor));
            var unscaledWidth = (int)((NotifyRect.Width - NotifyRect.X));
            var unscaledHeight = (int)((NotifyRect.Height - NotifyRect.Y));

            if (Settings.Location == NotifyLocation.TopLeft)
            {
                //Easy Peesy
                SetWindowPos(hwnd, 0, originX + 0, originY + 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.TopRight)
            {
                SetWindowPos(hwnd, 0, originX + (RealPreferredDisplayWidth - unscaledWidth), 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.BottomLeft)
            {
                SetWindowPos(hwnd, 0, originX + 0, originY + (RealPreferredDisplayHeight - unscaledHeight - (int)Math.Round(50f)), 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.BottomRight) // Default In Windows, But Here For Completeness Sake
            {
                SetWindowPos(hwnd, 0, originX + (RealPreferredDisplayWidth - unscaledWidth), originY + (RealPreferredDisplayHeight - unscaledHeight - (int)Math.Round(50f)), 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else // Custom Position
            {
                var xPosition = (int)(Settings.CustomPositionPercentX / 100f * RealPreferredDisplayWidth);
                var yPosition = (int)(Settings.CustomPositionPercentY / 100f * RealPreferredDisplayHeight);

                if (!Settings.EnableDebugRemoveBoundsCorrection)
                {
                    // Make Sure Position Isn't Out Of Bounds
                    xPosition = Math.Clamp(xPosition, 0, RealPreferredDisplayWidth - unscaledWidth);
                    yPosition = Math.Clamp(yPosition, 0, RealPreferredDisplayHeight - unscaledHeight);
                }

                SetWindowPos(hwnd, 0, originX + xPosition, originY + yPosition, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }

        }

        public bool ShouldCloseCurrentNotification()
        {
            if (CurrentNotificationStarted == DateTime.MinValue) { return false; }
            if (CurrentNotificationClosed) { return false; }
            if (CurrentStickyMode != NotificationStickyMode.Seconds) { return false; }

            var duration = Math.Clamp(CurrentStickyDurationSeconds, 1, NotificationLifetimeController.GetPermanentDurationSeconds());
            return (DateTime.Now - CurrentNotificationStarted).TotalSeconds >= duration;
        }

        public bool ShouldKeepCurrentNotificationVisible()
        {
            if (CurrentNotificationStarted == DateTime.MinValue) { return false; }
            if (CurrentNotificationClosed) { return false; }
            return CurrentStickyMode == NotificationStickyMode.Seconds || CurrentStickyMode == NotificationStickyMode.Permanent;
        }

        public bool IsNotificationActuallyVisible(IntPtr targetHwnd)
        {
            if (!IsWindowVisible(targetHwnd)) { return false; }

            Rectangle rect = new Rectangle();
            GetWindowRect(targetHwnd, ref rect);
            return rect.Width > rect.X && rect.Height > rect.Y;
        }

        public void DismissNotificationWindow(IntPtr targetHwnd)
        {
            try
            {
                var root = AutomationElement.FromHandle(targetHwnd);
                if (root == null) { return; }

                var buttons = root.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
                if (buttons == null || buttons.Count == 0) { return; }

                var button = buttons[buttons.Count - 1];
                if (button.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern))
                {
                    ((InvokePattern)pattern).Invoke();
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Warning($"Failed to dismiss notification window: {ex.Message}");
            }
        }

        public bool IsClosedNotificationWindow(IntPtr targetHwnd)
        {
            if (!ClosedNotificationSignatures.TryGetValue(targetHwnd, out var signature)) { return false; }

            var currentSignature = GetNotificationWindowSignature(targetHwnd);
            if (string.IsNullOrEmpty(currentSignature)) { return false; }
            if (currentSignature == signature) { return true; }

            ClosedNotificationSignatures.Remove(targetHwnd);
            return false;
        }

        public string GetNotificationWindowSignature(IntPtr targetHwnd)
        {
            try
            {
                var root = AutomationElement.FromHandle(targetHwnd);
                if (root == null) { return string.Empty; }

                var names = new List<string>();
                CollectAutomationNames(root, names, 0);
                return string.Join("|", names.Where((n) => !string.IsNullOrWhiteSpace(n)));
            }
            catch
            {
                var length = GetWindowTextLength(targetHwnd);
                if (length <= 0) { return string.Empty; }

                var text = new StringBuilder(length + 1);
                GetWindowText(targetHwnd, text, text.Capacity);
                return text.ToString();
            }
        }

        public void CollectAutomationNames(AutomationElement element, List<string> names, int depth)
        {
            if (depth > 4 || element == null) { return; }

            try
            {
                var name = element.Current.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }

                var children = element.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
                foreach (AutomationElement child in children)
                {
                    CollectAutomationNames(child, names, depth + 1);
                }
            }
            catch { }
        }

        public void StartFallbackNotificationLifetime()
        {
            var appReference = GetNotificationAppReference(hwnd);
            var mode = appReference?.GetStickyMode(Settings) ?? Settings.StickyMode;
            var seconds = appReference?.GetStickyDurationSeconds(Settings) ?? Settings.StickyDurationSeconds;
            var appName = appReference?.DisplayName ?? "detected notification";
            StartNotificationLifetime(mode, seconds, appName);
            Program.Logger.Information($"Fallback notification sticky lifetime for {appName}: {CurrentStickyMode}, {CurrentStickyDurationSeconds} seconds");
        }

        public void StartNotificationLifetime(NotificationStickyMode mode, int seconds, string appName, uint? notificationId = null)
        {
            CurrentStickyMode = mode;
            CurrentStickyDurationSeconds = seconds;
            CurrentNotificationStarted = DateTime.Now;
            CurrentNotificationClosed = false;
            CurrentNotificationId = notificationId;
            LastClosedHwnd = IntPtr.Zero;
        }

        public AppReference GetNotificationAppReference(IntPtr targetHwnd)
        {
            var appName = GetNotificationAppName(targetHwnd);
            if (!string.IsNullOrWhiteSpace(appName))
            {
                var appReference = Settings.AppReferences.Where((r) =>
                    r.ReferenceType == AppReferenceType.AppName &&
                    (string.Equals(r.ID, appName, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(r.DisplayName, appName, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();

                if (appReference != null) { return appReference; }
            }

            return Settings.AppReferences.Where((r) => r.ID == "Other").FirstOrDefault();
        }

        public string GetNotificationAppName(IntPtr targetHwnd)
        {
            try
            {
                var root = AutomationElement.FromHandle(targetHwnd);
                if (root == null) { return string.Empty; }

                var textElements = root.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
                foreach (AutomationElement textElement in textElements)
                {
                    var name = textElement.Current.Name;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }
            }
            catch { }

            return string.Empty;
        }
    }
}
