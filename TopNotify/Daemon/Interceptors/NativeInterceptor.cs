using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
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

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        const UInt32 WM_CLOSE = 0x0010;
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

        public override void Update()
        {
            base.Update();

            // Update extended styles
            ExStyleManager.Update(hwnd);

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
    }
}
