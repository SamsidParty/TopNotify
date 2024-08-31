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
        // This File Is Used To Replace The Default Notification Sounds, So That TopNotify Can Play A Different Sound
        const string FAKE_SOUND = "internal/silent";

        /// <summary>
        /// Sets The Notification Sound In The Registry To The Fake Sound File
        /// TopNotify Doesn't Have Registry Access Because Of MSIX, So Call CMD To Do It For Us
        /// </summary>
        private void InstallSoundInRegistry()
        {
            try
            {
                var command = $"reg add HKCU\\AppEvents\\Schemes\\Apps\\.Default\\Notification.Default\\.Current /t REG_SZ /ve /d \"{GetFullSoundPath(FAKE_SOUND)}\" /f";
                Util.SimpleCMD(command);
            }
            catch (Exception ex) { Logger.LogError(ex.ToString()); }
        }

        /// <summary>
        /// Sets The Permissions For The Fake Sound File And Makes Sure It Exists
        /// </summary>
        private void EnsureFakeSoundValidity()
        {
            if (!Directory.Exists(Path.GetDirectoryName(GetFullSoundPath(FAKE_SOUND))))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GetFullSoundPath(FAKE_SOUND)));
            }

            if (!File.Exists(GetFullSoundPath(FAKE_SOUND)))
            {
                //Copy The File If It Doesn't Exist
                //This Will Copy The File From The Application Directory (Read Only) To The AppData Directory
                //Because We Can't Change Permissions In The Application Directory
                Logger.LogInfo("Copying Sound File Into" +  GetFullSoundPath(FAKE_SOUND));
                File.Copy(GetSourceSoundPath(FAKE_SOUND), GetFullSoundPath(FAKE_SOUND));
            }

            //Check Permissions, Add "ALL APPLICATION PACKAGES" If Needed
            var fileInfo = new FileInfo(GetFullSoundPath(FAKE_SOUND));
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
        }

        public override void Start()
        {
            InstallSoundInRegistry();
            EnsureFakeSoundValidity();
            base.Start();
        }
    }
}
