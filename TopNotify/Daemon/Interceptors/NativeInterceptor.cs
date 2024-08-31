using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using WebFramework.Backend;
using TopNotify.Common;
using SamsidParty_TopNotify.Daemon;
using Windows.UI.Notifications.Management;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

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

        [DllImport("IVPluginTopNotify.dll")]
        private static extern bool Gui_RealEnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        #endregion

        public IntPtr hwnd;
        public ExtendedStyleManager ExStyleManager;
        public int ScaledMainDisplayWidth;
        public int ScaledMainDisplayHeight;
        public int RealMainDisplayWidth;
        public int RealMainDisplayHeight;
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

            Gui_RealEnumWindows(delegate (IntPtr hwnd, IntPtr param)
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
            base.Reflow();

            try
            {
                var foundHwnd = FindWindow("Windows.UI.Core.CoreWindow", Language.GetNotificationName());

                if (Settings.EnableDebugForceFallbackMode)
                {
                    foundHwnd = IntPtr.Zero; // Always use fallback mode if this setting is enabled
                }

                ScaledMainDisplayWidth = ResolutionFinder.GetScaledResolution().Width;
                ScaledMainDisplayHeight = ResolutionFinder.GetScaledResolution().Height;
                RealMainDisplayWidth = ResolutionFinder.GetRealResolution().Width;
                RealMainDisplayHeight = ResolutionFinder.GetRealResolution().Height;
                ScaleFactor = ResolutionFinder.GetInverseScale();

                //The Notification Isn't In A Supported Language
                if (foundHwnd == IntPtr.Zero)
                {
                    //The Notification Window Is The Only One That Is 396 x 152
                    foreach (var win in FindCoreWindows())
                    {
                        Rectangle rect = new Rectangle();
                        GetWindowRect(win, ref rect);

                        if ((ScaledMainDisplayWidth - rect.X) == 396)
                        {
                            foundHwnd = win;
                        }
                    }
                }

                if (foundHwnd != IntPtr.Zero)
                {
                    hwnd = foundHwnd;
                }

                Update();
                ExStyleManager.Update(hwnd);

            }
            catch { }
        }

        public override void Update()
        {
            base.Update();

            Rectangle NotifyRect = new Rectangle();
            GetWindowRect(hwnd, ref NotifyRect);

            var scaledWidth = (int)((NotifyRect.Width - NotifyRect.X * ScaleFactor));
            var scaledHeight = (int)((NotifyRect.Height - NotifyRect.Y * ScaleFactor));
            var unscaledWidth = (int)((NotifyRect.Width - NotifyRect.X));
            var unscaledHeight = (int)((NotifyRect.Height - NotifyRect.Y));

            if (Settings.Location == NotifyLocation.TopLeft)
            {
                //Easy Peesy
                SetWindowPos(hwnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.TopRight)
            {
                SetWindowPos(hwnd, 0, ScaledMainDisplayWidth - scaledWidth, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.BottomLeft)
            {
                SetWindowPos(hwnd, 0, 0, ScaledMainDisplayHeight - scaledHeight - (int)Math.Round(50f), 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.BottomRight) // Default In Windows, But Here For Completeness Sake
            {
                SetWindowPos(hwnd, 0, ScaledMainDisplayWidth - scaledWidth, ScaledMainDisplayHeight - scaledHeight - (int)Math.Round(50f), 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else // Custom Position
            {
                var xPosition = (int)(Settings.CustomPositionPercentX / 100f * RealMainDisplayWidth);
                var yPosition = (int)(Settings.CustomPositionPercentY / 100f * RealMainDisplayHeight);

                //Make Sure Position Isn't Out Of Bounds
                xPosition = Math.Clamp(xPosition, 0, RealMainDisplayWidth - unscaledWidth);
                yPosition = Math.Clamp(yPosition, 0, RealMainDisplayHeight - unscaledHeight);

                SetWindowPos(hwnd, 0, xPosition, yPosition, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }

        }
    }
}
