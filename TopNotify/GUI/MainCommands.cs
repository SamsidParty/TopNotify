using IgniteView.Core;
using IgniteView.Desktop;
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
    public class MainCommands
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
        //Opens The About Page
        [Command("About")]
        public static void About()
        {
            WebWindow.Create("/index.html?about")
                .WithTitle("About TopNotify")
                .WithBounds(new LockedWindowBounds((int)(480f * ResolutionFinder.GetScale()), (int)(300f * ResolutionFinder.GetScale())))
                .With((w) => (w as Win32WebWindow).BackgroundMode = Win32WebWindow.WindowBackgroundMode.Acrylic)
                .WithoutTitleBar()
                .Show();
        }

        //Called By JavaScript
        //Opens a URL
        [Command("OpenURL")]
        public static void OpenURL(string url)
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        //Called By JavaScript
        //Sends The Config File
        [Command("RequestConfig")]
        public static void RequestConfig(WebWindow target)
        {
            target.SendConfig();
        }

        //Called By JavaScript
        //Write Settings File
        [Command("WriteConfigFile")]
        public static void WriteConfigFile(WebWindow target, string data)
        {

            if (isSaving) { return; }
            isSaving = true;

            Settings.Overwrite(data);

            Thread.Sleep(100); // Prevent Crashing Daemon From Spamming Button

            // Tell The Daemon The Config Has Changed
            Daemon.Daemon.SendCommandToDaemon("UpdateConfig");

            isSaving = false;
        }

        [Command("OpenAppFolder")]
        public static async Task OpenAppFolder(WebWindow target)
        {
            Process.Start("explorer.exe", target.CurrentAppManager.CurrentIdentity.AppDataPath);
        }
    }
}
