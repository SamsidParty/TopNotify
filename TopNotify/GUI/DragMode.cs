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
using WebFramework;
using WebFramework.Backend;
using WebFramework.PT;

namespace TopNotify.GUI
{
    public partial class Frontend : WebScript
    {
        public static PTWebWindow DragModeWindow;

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
        [JSFunction("EnterDragMode")]
        public async Task EnterDragMode()
        {
            Logger.LogInfo("Entering Drag Mode");

            var currentConfig = Settings.Get();

            //Hide The Main Window
            var mainHwnd = (WindowManager.MainWindow as PTWebWindow).Native.WindowHandle;
            ShowWindow(mainHwnd, 0);

            //Set Mode To Custom Position In Config
            currentConfig.Location = NotifyLocation.Custom;
            WriteConfigFile(JsonConvert.SerializeObject(currentConfig));

            var windowLocation = new Point((int)(currentConfig.CustomPositionPercentX / 100f * ResolutionFinder.GetRealResolution().Width), (int)(currentConfig.CustomPositionPercentY / 100f * ResolutionFinder.GetRealResolution().Height) + 32);
            var windowSize = new Size((int)(364f * ResolutionFinder.GetScale()), (int)(109f * ResolutionFinder.GetScale()));

            //Create A Seperate Thread To Lock The Draggable Window To The Cursor Position
            var t = new Thread(DragModeThread);

            //Move The Cursor To The Saved Location
            ShowCursor(false);
            SetCursorPos(windowLocation.X + 30 + (int)(16f * ResolutionFinder.GetScale()), windowLocation.Y + (int)(29f * ResolutionFinder.GetScale()));

            DragModeWindow = (PTWebWindow)(await WindowManager.Create(new WindowOptions()
            {
                EnableAcrylic = true,
                StartWidthHeight = new Rectangle(windowLocation, windowSize),
                LockWidthHeight = true,
                DisableTitlebar = true,
                URLSuffix = "?drag",
                IconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Image", "Blank.png")
            }));

            SetForegroundWindow(DragModeWindow.Native.WindowHandle);
            t.Start();
        }


        //Called By JavaScript
        //Captures The Draggable Window Position And Writes It To The Config
        [JSFunction("ExitDragMode")]
        public void ExitDragMode()
        {
            if (DragModeWindow != null)
            {
                Logger.LogInfo("Exiting Drag Mode");

                ShowCursor(true);

                //Show The Main Window
                var mainHwnd = (WindowManager.MainWindow as PTWebWindow).Native.WindowHandle;
                ShowWindow(mainHwnd, 5);
                SetForegroundWindow(mainHwnd);

                var native = DragModeWindow.Native;
                var hwnd = (IntPtr)native.WindowHandle;

                //Find Position Of Window
                Rectangle DragRect = new Rectangle();
                NativeInterceptor.GetWindowRect(hwnd, ref DragRect);


                //The Window Size Of Notifications Is 396 * 120 Scaled
                //The Draw Size Of Notifications Is 364 * 109 Scaled
                //Add 32 * 11 Padding

                //Write It To The Config
                var currentConfig = Settings.Get();
                currentConfig.CustomPositionPercentX = ((float)DragRect.X - (16f * ResolutionFinder.GetScale())) / (float)ResolutionFinder.GetRealResolution().Width * 100f;
                currentConfig.CustomPositionPercentY = ((float)DragRect.Y - (29f * ResolutionFinder.GetScale())) / (float)ResolutionFinder.GetRealResolution().Height * 100f;
                WriteConfigFile(JsonConvert.SerializeObject(currentConfig));

                DragModeWindow.Close();
                DragModeWindow = null;

                //Set Config On Main Window
                var mainWindowFrontend = (Frontend)(WindowManager.MainWindow.AttachedScripts.Where((ws) => ws.GetType() == typeof(Frontend)).FirstOrDefault());
                mainWindowFrontend.RequestConfig();
            }
        }

        //Constantly Updates The Location Of The Draggable Window To The Location Of The Cursor
        public static void DragModeThread()
        {
            Point cursorPos;
            IntPtr dragModeHandle = DragModeWindow.Native.WindowHandle;

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
