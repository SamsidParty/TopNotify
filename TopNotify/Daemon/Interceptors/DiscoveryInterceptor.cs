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

            if (!Settings.DiscoveredApps.ContainsKey(appInfo.DisplayInfo.DisplayName))
            {
                // Add The AppInfo To The Settings File
                var settingsFile = TopNotify.Common.Settings.Get();
                settingsFile.DiscoveredApps[appInfo.DisplayInfo.DisplayName] = DiscoveredApp.FromAppInfo(appInfo);
                TopNotify.Common.Settings.Overwrite(JsonConvert.SerializeObject(settingsFile));
                InterceptorManager.Instance.OnSettingsChanged(); // Tells The InterceptorManager To Reload The Settings File
            }

            base.OnNotification(notification);
        }
    }
}
