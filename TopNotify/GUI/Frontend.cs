using Newtonsoft.Json;
using SamsidParty_TopNotify.Daemon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
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

        static bool isSaving = false;

        //Called By JavaScript
        //Spawns A Test Notification
        [JSFunction("SpawnTestNotification")]
        public async void SpawnTestNotification()
        {
            NotificationTester.Toast("Test Notification", "This Is A Test Notification");
        }

        //Called By JavaScript
        //Tells C# To Send The Config To JS
        [JSFunction("RequestConfig")]
        public void RequestConfig()
        {
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

            // Tell The Daemon The Config Has Changed Via JavaScript Websockets
            WindowManager.MainWindow.Document.RunFunction("window.UpdateConfig");

            isSaving = false;
        }

        [JSFunction("OpenAppFolder")]
        public static async Task OpenAppFolder()
        {
            Process.Start("explorer.exe", await SharedIO.File.GetAppdataDirectory());
        }
    }
}
