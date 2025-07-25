using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    public class FirstLaunch
    {
        public static void SetupTopNotify()
        {
            NotificationTester.Toast("Thank You For Installing TopNotify", "Please double click the icon in the system tray to start customizing");
        }

        public static Settings GetDefaultSettings()
        {
            var defaultSettings = new Settings()
            {
                AppReferences = ScanInstalledApps()
            };

            return defaultSettings;
        }

        public static List<AppReference> ScanInstalledApps()
        {
            List<AppReference> apps = new List<AppReference>();
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Default
            apps.Add(new AppReference()
                {
                    ReferenceType = AppReferenceType.AppName,
                    ID = "Other",
                    DisplayName = "All Other Apps",
                    DisplayIcon = "/Image/DefaultAppReferenceIcon.svg",
                    SoundPath = "windows/win11",
                    SoundDisplayName = "Notify 11"
                }
            );

            // Discord
            if (Directory.Exists(Path.Join(localAppDataPath, "Discord")))
            {
                apps.Add(new AppReference()
                    {
                        ReferenceType = AppReferenceType.AppName,
                        ID = "Discord",
                        DisplayName = "Discord",
                        DisplayIcon = "/Image/ThirdParty/Discord.svg",
                        SoundPath = "internal/silent",
                        SoundDisplayName = "No Sound"
                    }
                );
            }
            // TODO: Add automatic adding of more apps

            return apps;
        }
    }
}
