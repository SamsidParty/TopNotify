using Newtonsoft.Json;
using SamsidParty_TopNotify.Daemon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using TopNotify.Daemon;
using WebFramework;
using WebFramework.Backend;
using WebFramework.PT;

namespace TopNotify.GUI
{
    public class Frontend : WebScript
    {
        public static PTWebWindow DragModeWindow;

        static bool isSaving = false;

        //Called By JavaScript
        //Spawns A Test Notification
        [JSFunction("SpawnTestNotification")]
        public async void SpawnTestNotification()
        {
            NotificationTester.Toast("Test Notification", "This Is A Test Notification");
        }

        //Called By JavaScript
        //Resizes The Window To Be The Same Size As A Notification
        //The User Then Drags The Window To The Position Of The Notification
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

            DragModeWindow = (PTWebWindow)(await WindowManager.Create(new WindowOptions()
            {
                EnableAcrylic = true,
                StartWidthHeight = new Rectangle(windowLocation, windowSize),
                LockWidthHeight = true,
                DisableTitlebar = true,
                URLSuffix = "?drag",
                IconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Image", "Blank.png")
            }));
        }

        //Called By JavaScript
        //Captures Window Position And Resizes The Window To Be The Default
        [JSFunction("ExitDragMode")]
        public static void ExitDragMode()
        {
            if (DragModeWindow != null)
            {
                Logger.LogInfo("Exiting Drag Mode");

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

        //Called By JavaScript
        //Tells C# To Send The Config To JS
        [JSFunction("RequestConfig")]
        public void RequestConfig()
        {
            Document.RunFunction("window.SetIsUWP", Util.FindExe().Contains("WindowsApps").ToString());
            Document.RunFunction("window.SetConfig", Settings.GetForIPC());
        }


        //Called By JavaScript
        //Write Settings File
        [JSFunction("WriteConfigFile")]
        public static async void WriteConfigFile(string data)
        {

            if (isSaving) { return; }
            isSaving = true;

            try
            {
                File.WriteAllText(Settings.GetFilePath(), data);
                Settings.Validate(Settings.Get());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            await Task.Delay(100); // Prevent Crashing Daemon From Spamming Button

            isSaving = false;
        }

        [JSFunction("OpenAppFolder")]
        public static async Task OpenAppFolder()
        {
            Process.Start("explorer.exe", await SharedIO.File.GetAppdataDirectory());
        }
    }
}
