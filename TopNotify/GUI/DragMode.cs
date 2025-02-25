using IgniteView.Core;
using IgniteView.Desktop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using TopNotify.Daemon;
using static TopNotify.Daemon.ResolutionFinder;

namespace TopNotify.GUI
{
    public partial class Frontend
    {
        public static WebWindow DragModeWindow;

        #region WinAPI 

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        #endregion

        //Called By JavaScript
        //Creates A Draggable Window To Position Notifications
        [Command("EnterDragMode")]
        public static async Task EnterDragMode(WebWindow target)
        {

            var currentConfig = Settings.Get();

            //Hide The Main Window
            var mainHwnd = target.NativeHandle;
            ShowWindow(mainHwnd, 0);

            //Set Mode To Custom Position In Config
            currentConfig.Location = NotifyLocation.Custom;
            WriteConfigFile(target, JsonConvert.SerializeObject(currentConfig));

            var windowLocation = new Point((int)(currentConfig.CustomPositionPercentX / 100f * ResolutionFinder.GetRealResolution().Width), (int)(currentConfig.CustomPositionPercentY / 100f * ResolutionFinder.GetRealResolution().Height) + 32);
            var windowBounds = new LockedWindowBounds((int)(364f * ResolutionFinder.GetScale()), (int)(109f * ResolutionFinder.GetScale()));

            //Create A Seperate Thread To Lock The Draggable Window To The Cursor Position
            var t = new Thread(DragModeThread);

            //Move The Cursor To The Saved Location
            ShowCursor(false);
            SetCursorPos(windowLocation.X + 30 + (int)(16f * ResolutionFinder.GetScale()), windowLocation.Y + (int)(29f * ResolutionFinder.GetScale()));

            DragModeWindow =
                WebWindow.Create("/index.html?drag")
                .WithTitle("")
                .WithBounds(windowBounds);

            SetForegroundWindow(DragModeWindow.NativeHandle);
            t.Start();
        }


        // Called By JavaScript
        // Captures The Draggable Window Position And Writes It To The Config
        [Command("ExitDragMode")]
        public void ExitDragMode()
        {
            if (DragModeWindow != null)
            {
                ShowCursor(true);

                //Show The Main Window
                var mainWindow = DragModeWindow.CurrentAppManager.OpenWindows[0];
                var mainHwnd = mainWindow.NativeHandle;
                ShowWindow(mainHwnd, 5);
                SetForegroundWindow(mainHwnd);

                var hwnd = DragModeWindow.NativeHandle;

                //Find Position Of Window
                Rectangle DragRect = new Rectangle();
                NativeInterceptor.GetWindowRect(hwnd, ref DragRect);

                // Find The Bounds Of The Preferred Monitor
                var hMonitor = ResolutionFinder.GetPreferredDisplay();
                MonitorInfo currentMonitorInfo = new MonitorInfo();
                ResolutionFinder.GetMonitorInfo(hMonitor, currentMonitorInfo);
                var originX = currentMonitorInfo.Monitor.Left;
                var originY = currentMonitorInfo.Monitor.Top;

                // Offset The Bounds Of The Window To Match The Preferred Monitor
                DragRect = new Rectangle(DragRect.X - originX, DragRect.Y - originY, DragRect.Width, DragRect.Height);

                //The Window Size Of Notifications Is 396 * 120 Scaled
                //The Draw Size Of Notifications Is 364 * 109 Scaled
                //Add 32 * 11 Padding

                //Write It To The Config
                var currentConfig = Settings.Get();
                currentConfig.CustomPositionPercentX = ((float)DragRect.X - (16f * ResolutionFinder.GetScale())) / (float)ResolutionFinder.GetRealResolution().Width * 100f;
                currentConfig.CustomPositionPercentY = ((float)DragRect.Y - (29f * ResolutionFinder.GetScale())) / (float)ResolutionFinder.GetRealResolution().Height * 100f;
                WriteConfigFile(mainWindow, JsonConvert.SerializeObject(currentConfig));

                DragModeWindow.Close();
                DragModeWindow = null;
            }
        }

        //Constantly Updates The Location Of The Draggable Window To The Location Of The Cursor
        public static void DragModeThread()
        {
            Point cursorPos;
            IntPtr dragModeHandle = DragModeWindow.NativeHandle;

            while (DragModeWindow != null)
            {
                //Set The Drag Mode Window To Be At The Cursor's Position
                GetCursorPos(out cursorPos);

                //Add Offset To Keep Cursor Inside The Window
                //This Prevents The Cursor From Flashing In And Out Due To Being On The Edge Of The Window
                cursorPos.X -= 30;
                cursorPos.Y -= 30;

                SetWindowPos(dragModeHandle, 0, cursorPos.X, cursorPos.Y, 0, 0, 0x0001);

                Thread.Sleep(0);
            }
        }
    }
}
