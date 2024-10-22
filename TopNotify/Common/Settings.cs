using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using TopNotify.Daemon;
using WebFramework.Backend;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TopNotify.Common
{
    [Serializable]
    public class Settings
    {
        public NotifyLocation Location = NotifyLocation.TopRight;
        public bool EnableClickThrough = false;

        // Debug
        public bool EnableDebugNotifications = false;
        public bool EnableDebugForceFallbackMode = false;
        public bool EnableDebugRemoveBoundsCorrection = false;

        // Accessibility
        public bool ReadAloud = false;

        // From 0 To 5 (0 Is Fully Opaque, 5 Is Mostly Transparent)
        public float Opacity = 0;

        // Position Where Origin Is The Top Left Of The Screen
        // 0% On Both Is The Top Left
        // 100% On Both Is Bottom Right
        public float CustomPositionPercentX = 0;
        public float CustomPositionPercentY = 0;

        public Dictionary<string, DiscoveredApp> DiscoveredApps = new Dictionary<string, DiscoveredApp>();
        public List<AppReference> AppReferences = new List<AppReference>();

        // Dynamic Fields That Are Cached, Useful For Interop
        public int __ScreenWidth = 0;
        public int __ScreenHeight = 0;
        public float __ScreenScale = 1;

        // Deprecated Settings
        [Deprecated("Use CustomPositionPercentX Instead", DeprecationType.Deprecate, 241)] public int CustomPositionX = 0; // Deprecated In Favor Of Percentage Units
        [Deprecated("Use CustomPositionPercentY Instead", DeprecationType.Deprecate, 241)] public int CustomPositionY = 0; // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        [Deprecated("Startup Is Now Managed By MSIX", DeprecationType.Deprecate, 244)] public bool RunOnStartup = false; // Startup Is Now Managed By MSIX


        public static Settings Get()
        {
            var value = JsonConvert.DeserializeObject<Settings>(GetRaw());
            return value;
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
        /// Returns A JSON String Of The Settings, Modified With Custom Fields For IPC
        /// </summary>
        public static string GetForIPC()
        {
            var settings = Settings.Get();
            settings.UpdateDynamicFields();
            return JsonConvert.SerializeObject(settings);
        }

        /// <summary>
        /// Returns The Full Path Of The Settings File
        /// </summary>
        public static string GetFilePath()
        {
            var defaultSettings = JsonConvert.SerializeObject(GetDefaultSettings(), Formatting.Indented);
            var value = GetFilePath("Settings.json", Encoding.UTF8.GetBytes(defaultSettings));
            return value;
        }

        public static Settings GetDefaultSettings()
        {
            var defaultSettings = new Settings();

            // Add The Default App Reference
            defaultSettings.AppReferences.Add(new AppReference()
                {
                    ReferenceType = 0,
                    ID = "Other",
                    DisplayName = "All Other Apps",
                    DisplayIcon = "/Image/DefaultAppReferenceIcon.svg",
                    SoundPath = "windows/win11",
                    Discovery = new AppDiscovery[]
                    {
                        new AppDiscovery() {
                            Method = AppDiscoveryMethod.MatchAlways,
                            SearchTerm = "",
                        }
                    }
                } 
            );

            return defaultSettings;
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


        /// <summary>
        /// Writes The Settings File With New Data
        /// </summary>
        public static void Overwrite(string newData)
        {
            try
            {
                File.WriteAllText(Settings.GetFilePath(), newData);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Updates The Temporary Fields Used By Other Parts Of TopNotify (eg. __ScreenWidth)
        /// </summary>
        public void UpdateDynamicFields()
        {
            __ScreenWidth = ResolutionFinder.GetRealResolution().Width;
            __ScreenHeight = ResolutionFinder.GetRealResolution().Height;
            __ScreenScale = ResolutionFinder.GetScale();
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
