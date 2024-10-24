using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using TopNotify.Daemon;
using Windows.UI.Notifications;

namespace TopNotify.Daemon
{
    public class DiscoveryInterceptor : Interceptor
    {
        public override void OnNotification(UserNotification notification)
        {
            // Store The App's Info, So That In The Future It Can Be Used To Create An AppReference
            var appInfo = notification.AppInfo;
            var appReferences = Settings.AppReferences;
            var alreadyHasAppReference = appReferences.Where((r) => r.ID == appInfo.DisplayInfo.DisplayName).Any();

            if (!alreadyHasAppReference)
            {
                // Add The AppInfo To The Settings File
                var settingsFile = TopNotify.Common.Settings.Get();

                var appReference = new AppReference()
                {
                    DisplayName = appInfo.DisplayInfo.DisplayName,
                    ID = appInfo.DisplayInfo.DisplayName,
                    SoundPath = "internal/default",
                    ReferenceType = AppReferenceType.AppName
                };

                settingsFile.AppReferences.Add(appReference);

                TopNotify.Common.Settings.Overwrite(JsonConvert.SerializeObject(settingsFile)); // Write The Settings File
                InterceptorManager.Instance.OnSettingsChanged(); // Tells The InterceptorManager To Reload The Settings File
            }

            base.OnNotification(notification);
        }
    }
}
