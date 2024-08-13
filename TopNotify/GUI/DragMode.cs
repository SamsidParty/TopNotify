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

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        #endregion

        //Called By JavaScript
        //Creates A Draggable Window To Position Notifications
        [JSFunction("EnterDragMode")]
        public static async Task EnterDragMode()
        {
            Logger.LogInfo("Entering Drag Mode");

            var currentConfig = Settings.Get();

            //Set Mode To Custom Position In Config
            currentConfig.Location = NotifyLocation.Custom;
            WriteConfigFile(JsonConvert.SerializeObject(currentConfig));

            var windowLocation = new Point((int)(currentConfig.CustomPositionPercentX / 100f * ResolutionFinder.GetRealResolution().Width), (int)(currentConfig.CustomPositionPercentY / 100f * ResolutionFinder.GetRealResolution().Height) + 32);
            var windowSize = new Size((int)(364f * ResolutionFinder.GetScale()), (int)(109f * ResolutionFinder.GetScale()));

            var t = new Thread(DragModeThread);

            ShowCursor(false);

            DragModeWindow = (PTWebWindow)(await WindowManager.Create(new WindowOptions()
            {
                EnableAcrylic = true,
                StartWidthHeight = new Rectangle(windowLocation, windowSize),
                LockWidthHeight = true,
                DisableTitlebar = true,
                URLSuffix = "?drag",
                IconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Image", "Blank.png")
            }));

            t.Start();
        }


        //Called By JavaScript
        //Captures The Draggable Window Position And Writes It To The Config
        [JSFunction("ExitDragMode")]
        public static void ExitDragMode()
        {
            if (DragModeWindow != null)
            {
                Logger.LogInfo("Exiting Drag Mode");

                ShowCursor(true);

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
                currentConfig.CustomPositionPercentX = ((float)DragRect.X - 16) / (float)ResolutionFinder.GetRealResolution().Width * 100f;
                currentConfig.CustomPositionPercentY = ((float)DragRect.Y - 29) / (float)ResolutionFinder.GetRealResolution().Height * 100f;
                Logger.LogError(currentConfig.CustomPositionPercentX.ToString());
                WriteConfigFile(JsonConvert.SerializeObject(currentConfig));

                DragModeWindow.Close();
                DragModeWindow = null;
            }
        }

        public static void DragModeThread()
        {
            Point cursorPos;
            IntPtr dragModeHandle = DragModeWindow.Native.WindowHandle;

            while (DragModeWindow != null)
            {
                //Set The Drag Mode Window To Be At The Cursor's Position
                GetCursorPos(out cursorPos);

                //Add Offset To Keep Cursor Inside The Window
                cursorPos.X -= 30;
                cursorPos.Y -= 30;

                SetWindowPos(dragModeHandle, -1, cursorPos.X, cursorPos.Y, 0, 0, 0x0001);

                Thread.Sleep(0);
            }
        }
    }
}
