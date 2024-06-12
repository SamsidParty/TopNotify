using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
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
                key.SetValue("", GetFullSoundPath(Settings.SoundPath)); // Sets (Default) Value In Registry
                key.Close();
            }
            catch (Exception ex) { Logger.LogError(ex.ToString()); }
        }

        /// <summary>
        /// Sets The Permissions For The Sound File And Makes Sure It Exists
        /// </summary>
        private void EnsureSoundValidity()
        {
            if (!Directory.Exists(Path.GetDirectoryName(GetFullSoundPath(Settings.SoundPath))))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GetFullSoundPath(Settings.SoundPath)));
            }

            if (!File.Exists(GetFullSoundPath(Settings.SoundPath)))
            {
                //Copy The File If It Doesn't Exist
                Logger.LogInfo("Copying Sound File Into" +  GetFullSoundPath(Settings.SoundPath));
                File.Copy(GetSourceSoundPath(Settings.SoundPath), GetFullSoundPath(Settings.SoundPath));
            }

            //Check Permissions, Add "ALL APPLICATION PACKAGES" If Needed
            var fileInfo = new FileInfo(GetFullSoundPath(Settings.SoundPath));
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


        /// <summary>
        /// Gets The Full Path Of A Sound From It's Relative Path
        /// </summary>
        public static string GetFullSoundPath(string soundRelativePath)
        {
            return Path.Combine(Common.Settings.GetAppDataFolder(), "NotificationSounds", soundRelativePath.Replace("/", "\\") + ".wav");
        }

        /// <summary>
        /// Gets The Path Of The Sound Built In The Application Folder
        /// </summary>
        public static string GetSourceSoundPath(string soundRelativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Audio", soundRelativePath.Replace("/", "\\") + ".wav");
        }


        public override void Reflow()
        {
            base.Reflow();
            InstallSoundInRegistry();
            EnsureSoundValidity();
        }

        public override void Start()
        {
            InstallSoundInRegistry();
            EnsureSoundValidity();
            base.Start();
        }
    }
}
