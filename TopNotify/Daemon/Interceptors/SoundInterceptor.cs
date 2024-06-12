using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebFramework.Backend;

namespace TopNotify.Daemon
{
    public class SoundInterceptor : Interceptor
    {
        /// <summary>
        /// Sets The Notification Sound In The Registry To The TopNotify Wav File
        /// </summary>
        private void InstallSoundInRegistry()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey("AppEvents\\Schemes\\Apps\\.Default\\Notification.Default\\.Current", true);
                key.SetValue("", "TODO");
                key.Close();
            }
            catch (Exception ex) { Logger.LogError(ex.ToString()); }
        }

        public override void Reflow()
        {
            base.Reflow();
            InstallSoundInRegistry();
        }
    }
}
