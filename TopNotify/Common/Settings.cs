using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using Windows.Foundation.Metadata;

namespace TopNotify.Common
{
    [Serializable]
    public class Settings
    {
        public NotifyLocation Location = NotifyLocation.TopRight;
        public bool RunOnStartup = true;
        public bool EnableClickThrough = false;

        // Debug
        public bool EnableDebugNotifications = false;
        public bool EnableDebugForceFallbackMode = false;

        // From 0 To 5 (0 Is Fully Opaque, 5 Is Mostly Transparent)
        public float Opacity = 0;

        // Position Where Origin Is The Top Left Of The Screen
        // 0% On Both Is The Top Left
        // 100% On Both Is Bottom Right
        public float CustomPositionPercentX = 0;
        public float CustomPositionPercentY = 0;

        // Relative Path To The WAV File Stored In WWW/Audio, Without .wav Extension
        public string SoundPath = "windows/win11";

        // Deprecated Settings
        [Deprecated("Use CustomPositionPercentX Instead", DeprecationType.Deprecate, 241)] public int CustomPositionX = 0; // Deprecated In Favor Of Percentage Units
        [Deprecated("Use CustomPositionPercentY Instead", DeprecationType.Deprecate, 241)] public int CustomPositionY = 0; // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        public static Settings Get()
        {
            return JsonConvert.DeserializeObject<Settings>(GetRaw());
        }

        public static string GetRaw()
        {
            var path = GetFilePath();

            var content = "";
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream))
            {
                content = textReader.ReadToEnd();
            }


            return content;
        }

        /// <summary>
        /// Returns The Full Path Of The Settings File
        /// </summary>
        public static string GetFilePath()
        {
            var defaultSettings = JsonConvert.SerializeObject(new Settings(), Formatting.Indented);
            var value = GetFilePath("Settings.json", Encoding.UTF8.GetBytes(defaultSettings));
            return value;
        }

        /// <summary>
        /// Returns The Full Path Of fileName Located In TopNotify's AppData Directory
        /// Uses defaultValue if the file doesn't exist
        /// </summary>
        public static string GetFilePath(string fileName, byte[] defaultValue)
        {
            var appFolder = GetAppDataFolder();
            var file = Path.Combine(appFolder, fileName);
            if (!File.Exists(file))
            {
                //Create Default File
                File.WriteAllBytes(file, defaultValue != null ? defaultValue : new byte[0]); // Write default value if it's not null
                Validate(Get());

                if (fileName == "Settings.json")
                {
                    //Show First Launch Notification
                    NotificationTester.Toast("TopNotify Has Been Installed", "You Can Find The Settings For TopNotify In The System Tray");
                }

            }
            return file;
        }

        public static string GetAppDataFolder()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(localAppData, "SamsidParty", "TopNotify");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return appFolder;
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
            if (Util.FindExe().Contains("WindowsApps")) { return; } // App Is In An MSIX Container
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk.GetValue("TopNotify") == null)
            {
                rk.SetValue("TopNotify", System.Environment.ProcessPath);
            }
        }

        public static void DeleteStartupShortcut()
        {
            if (Util.FindExe().Contains("WindowsApps")) { return; } // App Is In An MSIX Container
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
        BottomRight,
        Custom
    }
}
