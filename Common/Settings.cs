﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace SamsidParty_TopNotify
{
    [Serializable]
    public class Settings
    {
        public NotifyLocation Location = NotifyLocation.TopRight;
        public bool RunOnStartup = true;
        public bool EnableClickThrough = false; // Not Available Yet
        public bool EnableDebugNotifications = false;


        // From 0 To 5 (0 Is Fully Opaque, 5 Is Mostly Transparent)
        public float Opacity = 0;

        public static Settings Get()
        {
            var path = GetFilePath();
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
        }

        public static string GetFilePath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(localAppData, "SamsidParty", "TopNotify");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            var settingsFile = Path.Combine(appFolder, "Settings.json");
            if (!File.Exists(settingsFile))
            {
                //Create Default Settings File
                var defaultSettings = JsonConvert.SerializeObject(new Settings(), Formatting.Indented);
                File.WriteAllText(settingsFile, defaultSettings);
                Validate(Get());
            }
            return settingsFile;
        }

        public static string GetLogPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(localAppData, "SamsidParty", "TopNotify");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            var logFile = Path.Combine(appFolder, "Log.txt");
            if (!File.Exists(logFile))
            {
                File.WriteAllText(logFile, "[Start Of Log File]\n");
            }
            return logFile;
        }

        public static void Validate(Settings settings)
        {
            if (settings.RunOnStartup)
            {
                CreateStartupShortcut();
            }
            else
            {
                DeleteStartupShortcut();
            }
        }

        public static void CreateStartupShortcut()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk.GetValue("TopNotify") == null)
            {
                rk.SetValue("TopNotify", System.Environment.ProcessPath);
            }
        }

        public static void DeleteStartupShortcut()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk.GetValue("TopNotify") != null)
            {
                rk.DeleteValue("TopNotify");
            }
        }
    }

    public enum NotifyLocation
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}