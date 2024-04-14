using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebFramework;

namespace SamsidParty_TopNotify
{
    public class Frontend : WebScript
    {
        public override async Task DOMContentLoaded()
        {
            Document.Body.AddEventListener("spawnTestNotification", SpawnTestNotification);
            Document.Body.AddEventListener("uploadConfig", UploadConfig);
            Document.Body.AddEventListener("reactReady", ReactReady);
        }

        //Runs After The First React Render (Called Twice Sometimes)
        public async void ReactReady(JSEvent e)
        {
            Document.RunFunction("window.SetConfig", File.ReadAllText(Settings.GetFilePath()));
        }

        //Create A Test Notification
        public async void SpawnTestNotification(JSEvent e)
        {
            NotificationTester.Toast("Test Notification", "This Is A Test Notification");
        }

        //Write Settings File
        public async void UploadConfig(JSEvent e)
        {
            File.WriteAllText(Settings.GetFilePath(), e.Data["newConfig"].ToString());
            Settings.Validate(Settings.Get());
        }
    }
}
