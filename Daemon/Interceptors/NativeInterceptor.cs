using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SamsidParty_TopNotify
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

        const UInt32 WM_CLOSE = 0x0010;
        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;

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

        public override void Reflow()
        {
            base.Reflow();

            try
            {
                hwnd = FindWindow("Windows.UI.Core.CoreWindow", Language.GetNotificationName());
                MainDisplayWidth = Screen.PrimaryScreen.Bounds.Width;
                MainDisplayHeight = Screen.PrimaryScreen.Bounds.Height;
                ScaleFactor = 1f;
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

            NotifyRect.Width = NotifyRect.Width - NotifyRect.X;
            NotifyRect.Height = NotifyRect.Height - NotifyRect.Y;

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
                SetWindowPos(hwnd, 0, 0, MainDisplayHeight - NotifyRect.Height - (int)Math.Round(50f * ScaleFactor), 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            //BottomRight Does Nothing Because It's The Default In Windows


        }
    }
}
