using System;
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
