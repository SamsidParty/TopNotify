using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using WebFramework.Backend;

namespace TopNotify.Daemon
{
    public class WhatsAppInterceptor : Interceptor
    {
        public override bool ShouldEnable()
        {
            var allApps = Directory.GetDirectories("C:\\Program Files\\WindowsApps");

            foreach (var app in allApps)
            {
                if (app.Contains("WhatsAppDesktop"))
                {
                    Logger.LogInfo("WhatsApp Desktop Interception Is Enabled");
                    AppReference.EnsurePresetExists("WhatsApp");
                    return true;
                }
            }

            Logger.LogInfo("WhatsApp Desktop Interception Is Not Enabled");

            AppReference.EnsurePresetDoesntExist("WhatsApp");
            return false;
        }
    }
}
