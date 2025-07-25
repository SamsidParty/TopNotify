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
using Windows.ApplicationModel.Store;

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
                .WithBounds(new LockedWindowBounds((int)(400f * ResolutionFinder.GetScale()), (int)(300f * ResolutionFinder.GetScale())))
                .With((w) => (w as Win32WebWindow).BackgroundMode = Win32WebWindow.WindowBackgroundMode.Acrylic)
                .WithoutTitleBar()
                .Show();
        }

        [Command("Donate")] 
        public static async void Donate()
        {
            try
            {
                var results = await Program.Context.RequestPurchaseAsync("9P92HZT8QC8R");

                if (results.Status == Windows.Services.Store.StorePurchaseStatus.Succeeded)
                {
                    NotificationTester.MessageBox("Thank You For Donating", "Your contribution is much appreciated ❤️");
                }
                else
                {
                    NotificationTester.MessageBox("Failed To Purchase Donation", results.Status.ToString());
                }
            }
            catch (Exception ex)
            {
                NotificationTester.MessageBox("Failed To Purchase Donation", ex.Message);
            }
        }

        [Command("GetVersion")]
        public static string GetVersion()
        {
            // Read version from Appx Manifest
            var appxManifest = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "AppxManifest.xml");
            if (File.Exists(appxManifest)) { 
                var manifestData = File.ReadAllText(appxManifest);

                var from = manifestData.IndexOf("Version=\"") + "Version=\"".Length;
                var to = manifestData.LastIndexOf("\"");

                var result = manifestData.Substring(from, to - from);
                return " v" + result.Substring(0, 5);
            }

            return " Debug";
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
            Process.Start("explorer.exe", Settings.GetAppDataFolder());
        }
    }
}
