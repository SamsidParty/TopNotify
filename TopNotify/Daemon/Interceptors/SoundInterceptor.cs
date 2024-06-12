using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using WebFramework.Backend;

namespace TopNotify.Daemon
{
    public class SoundInterceptor : Interceptor
    {
        public string AudioPath = Common.Settings.GetFilePath("topnotify.wav", null);

        /// <summary>
        /// Sets The Notification Sound In The Registry To The TopNotify Wav File
        /// </summary>
        private void InstallSoundInRegistry()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey("AppEvents\\Schemes\\Apps\\.Default\\Notification.Default\\.Current", true);
                key.SetValue("", AudioPath); // Sets (Default) Value In Registry
                key.Close();
            }
            catch (Exception ex) { Logger.LogError(ex.ToString()); }
        }

        /// <summary>
        /// Sets The Permissions For The Sound File And Makes Sure It's Not Empty
        /// </summary>
        private void EnsureSoundValidity()
        {
            var fileInfo = new FileInfo(AudioPath);

            //Check If File Is Empty, If It Is, Write The Default Sound To It
            var fileSize = fileInfo.Length;
            if (fileSize == 0) { 
                var defaultFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Audio", "windows", "win11.wav");

                //Overwrite Instead Of Copying
                //Prevents Resetting Permissions
                var defaultBytes = File.ReadAllBytes(defaultFile);
                File.WriteAllBytes(AudioPath, defaultBytes);
            }

            //Check Permissions, Add "ALL APPLICATION PACKAGES" If Needed
            var fileSecurity = fileInfo.GetAccessControl();
            var perms = fileSecurity.GetAccessRules(true, false, typeof(SecurityIdentifier));
            var hasAllAppPerms = false;

            //For Some Reason Linq .Where() Doesn't Work
            //Use Normal Iteration
            foreach (FileSystemAccessRule perm in perms)
            {
                if (perm.IdentityReference.Value == "S-1-15-2-1") // S-1-15-2-1 Means All App Packages, https://renenyffenegger.ch/notes/Windows/security/SID/index
                {
                    hasAllAppPerms = true;
                }
            }

            if (!hasAllAppPerms)
            {
                Logger.LogInfo("Writing ALL APPLICATION PACKAGES Permission");
                InheritanceFlags iFlags = InheritanceFlags.None;
                PropagationFlags pFlags = PropagationFlags.None;
                fileSecurity.AddAccessRule(new FileSystemAccessRule("ALL APPLICATION PACKAGES", FileSystemRights.ReadAndExecute, iFlags, pFlags, AccessControlType.Allow));
                fileInfo.SetAccessControl(fileSecurity);
            }

        }

        public override void Reflow()
        {
            base.Reflow();
            InstallSoundInRegistry();
            EnsureSoundValidity();
        }
    }
}
