using IgniteView.Core;
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

namespace TopNotify.GUI
{
    public partial class Frontend
    {
        static bool isSaving = false;

        //Called By JavaScript
        //Spawns A Test Notification
        [Command("SpawnTestNotification")]
        public static void SpawnTestNotification()
        {
            NotificationTester.SpawnTestNotification();
        }

        //Called By JavaScript
        //Tells C# To Send The Config To JS
        [Command("RequestConfig")]
        public void RequestConfig(WebWindow target)
        {
            target.CallFunction("window.SetConfig", Settings.GetForIPC());
        }


        //Called By JavaScript
        //Write Settings File
        [Command("WriteConfigFile")]
        public static async void WriteConfigFile(WebWindow target, string data)
        {

            if (isSaving) { return; }
            isSaving = true;

            Settings.Overwrite(data);

            await Task.Delay(100); // Prevent Crashing Daemon From Spamming Button

            // Tell The Daemon The Config Has Changed Via JavaScript Websockets
            target.CallFunction("window.UpdateConfig");

            isSaving = false;
        }

        [Command("OpenAppFolder")]
        public static async Task OpenAppFolder(WebWindow target)
        {
            Process.Start("explorer.exe", target.CurrentAppManager.CurrentIdentity.AppDataPath);
        }
    }
}
