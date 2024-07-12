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

namespace TopNotify.Daemon
{
    public class NativeInterceptor : Interceptor
    {
        #region WinAPI Methods
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rectangle rectangle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

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
        public int MainDisplayWidth;
        public int MainDisplayHeight;
        public float ScaleFactor;

        public override void Start()
        {
            base.Start();
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

                MainDisplayWidth = ResolutionFinder.GetResolution().Width;
                MainDisplayHeight = ResolutionFinder.GetResolution().Height;
                ScaleFactor = ResolutionFinder.GetScale();

                //The Notification Isn't In A Supported Language
                if (foundHwnd == IntPtr.Zero)
                {
                    //The Notification Window Is The Only One That Is 396 x 152
                    foreach (var win in FindCoreWindows())
                    {
                        Rectangle rect = new Rectangle();
                        GetWindowRect(win, ref rect);
                        StringBuilder sb = new StringBuilder();
                        GetWindowText(hwnd, sb, 260);

                        if ((MainDisplayWidth - rect.X) == 396)
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
                WindowOpacity.ApplyToWindow(hwnd);
                WindowClickThrough.ApplyToWindow(hwnd);

            }
            catch { }
        }

        public override void Update()
        {
            base.Update();

            Rectangle NotifyRect = new Rectangle();
            GetWindowRect(hwnd, ref NotifyRect);

            NotifyRect.Width = (int)((NotifyRect.Width - NotifyRect.X * ScaleFactor));
            NotifyRect.Height = (int)((NotifyRect.Height - NotifyRect.Y * ScaleFactor));

            if (Settings.Location == NotifyLocation.TopLeft)
            {
                //Easy Peesy
                SetWindowPos(hwnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.TopRight)
            {
                SetWindowPos(hwnd, 0, MainDisplayWidth - NotifyRect.Width, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.BottomLeft)
            {
                SetWindowPos(hwnd, 0, 0, MainDisplayHeight - NotifyRect.Height - (int)Math.Round(50f), 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else if (Settings.Location == NotifyLocation.BottomRight) // Default In Windows, But Here For Completeness Sake
            {
                SetWindowPos(hwnd, 0, MainDisplayWidth - NotifyRect.Width, MainDisplayHeight - NotifyRect.Height - (int)Math.Round(50f), 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else // Custom Position
            {
                //Check If The Position Is Out Of Frame
                if (Settings.CustomPositionX > MainDisplayWidth + 1 - NotifyRect.Width || Settings.CustomPositionY > MainDisplayHeight + 1 - NotifyRect.Height)
                {
                    Logger.LogWarning("Notification Out Of Bounds");
                    return;
                }

                SetWindowPos(hwnd, 0, Settings.CustomPositionX, Settings.CustomPositionY, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }

        }
    }
}
