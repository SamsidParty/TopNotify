using IgniteView.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Media;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using Windows.UI.Notifications;

namespace TopNotify.Daemon
{
    public class SoundInterceptor : Interceptor
    {
        // This File Is Used To Replace The Default Notification Sounds, So That TopNotify Can Play A Different Sound
        const string FAKE_SOUND = "internal/silent";

        SoundPlayer Player;
        bool isPlaying = false;

        bool allowedToPlaySound = false;

        /// <summary>
        /// Sets The Notification Sound In The Registry To The Fake Sound File
        /// TopNotify Doesn't Have Registry Access Because Of MSIX, So Call CMD To Do It For Us
        /// </summary>
        [Command("InstallSoundInRegistry")]
        public static void InstallSoundInRegistry()
        {
            try
            {
                var command = $"reg add HKCU\\AppEvents\\Schemes\\Apps\\.Default\\Notification.Default\\.Current /t REG_SZ /ve /d \"{GetCopiedSoundPath(FAKE_SOUND)}\" /f";
                Util.SimpleCMD(command);
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// Reverts the notification sound to the default
        /// </summary>
        [Command("UninstallSoundInRegistry")]
        public static void UninstallSoundInRegistry()
        {
            try
            { 
                // This is the default soundpath found in the windows 11 registry
                var defaultSoundPath = Environment.ExpandEnvironmentVariables("%SystemRoot%\\media\\Windows Notify System Generic.wav");
                var command = $"reg add HKCU\\AppEvents\\Schemes\\Apps\\.Default\\Notification.Default\\.Current /t REG_SZ /ve /d \"{defaultSoundPath}\" /f";
                Util.SimpleCMD(command);
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// Detects whether the fake sound file is installed in the registry
        /// </summary>
        [Command("IsSoundInstalledInRegistry")]
        public static bool IsSoundInstalledInRegistry()
        {
            try
            {
                var command = $"reg query HKCU\\AppEvents\\Schemes\\Apps\\.Default\\Notification.Default\\.Current /t REG_SZ /ve";
                return Util.SimpleCMD(command)?.Contains("TopNotify") ?? false;
            }
            catch (Exception ex) { }

            return false;
        }

        /// <summary>
        /// Sets The Permissions For The Fake Sound File And Makes Sure It Exists
        /// </summary>
        void EnsureFakeSoundValidity()
        {
            if (!Directory.Exists(Path.GetDirectoryName(GetCopiedSoundPath(FAKE_SOUND))))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GetCopiedSoundPath(FAKE_SOUND)));
            }

            if (!File.Exists(GetCopiedSoundPath(FAKE_SOUND)))
            {
                // Copy The File If It Doesn't Exist
                // This Will Copy The File From The Application Directory (Read Only) To The AppData Directory
                // Because We Can't Change Permissions In The Application Directory
                File.Copy(GetSourceSoundPath(FAKE_SOUND), GetCopiedSoundPath(FAKE_SOUND));
            }

            // Check Permissions, Add "ALL APPLICATION PACKAGES" If Needed
            var fileInfo = new FileInfo(GetCopiedSoundPath(FAKE_SOUND));
            var fileSecurity = fileInfo.GetAccessControl();
            var perms = fileSecurity.GetAccessRules(true, false, typeof(SecurityIdentifier));
            var hasAllAppPerms = false;

            // For Some Reason Linq .Where() Doesn't Work
            // Use Normal Iteration
            foreach (FileSystemAccessRule perm in perms)
            {
                if (perm.IdentityReference.Value == "S-1-15-2-1") // S-1-15-2-1 Means All App Packages, https://renenyffenegger.ch/notes/Windows/security/SID/index
                {
                    hasAllAppPerms = true;
                }
            }

            // Windows Sometimes Won't Play The Sound Unless It Has Permission To Do So,
            // Give File Permissions To "ALL APPLICATION PACKAGES"
            if (!hasAllAppPerms)
            {
                try
                {
                    InheritanceFlags iFlags = InheritanceFlags.None;
                    PropagationFlags pFlags = PropagationFlags.None;
                    fileSecurity.AddAccessRule(new FileSystemAccessRule("ALL APPLICATION PACKAGES", FileSystemRights.ReadAndExecute, iFlags, pFlags, AccessControlType.Allow));
                    fileInfo.SetAccessControl(fileSecurity);
                }
                catch (Exception ex)
                {

                }
            }

            allowedToPlaySound = IsSoundInstalledInRegistry();

        }


        /// <summary>
        /// Gets The Path Of A Sound In The AppData Folder
        /// </summary>
        public static string GetCopiedSoundPath(string soundRelativePath)
        {
            return Path.Combine(Common.Settings.GetAppDataFolder(), "NotificationSounds", soundRelativePath.Replace("/", "\\") + ".wav");
        }

        /// <summary>
        /// Gets The Path Of The Sound Built In The Application Folder
        /// </summary>
        public static string GetSourceSoundPath(string soundRelativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "iv2runtime", "audio", soundRelativePath.Replace("/", "\\") + ".wav");
        }

        /// <summary>
        /// Gets The Path Of The Sound, Automatically Determining If It's In The App Folder, AppData Folder, Or Custom Path
        /// </summary>
        public static string GetSoundPath(string soundPath)
        {
            if (soundPath == "internal/default")
            {
                // Find The SoundPath Of The Default AppReference
                foreach (var appRef in Settings.Get().AppReferences)
                {
                    if (appRef.ID == "Other" && appRef.SoundPath != "internal/default")
                    {
                        return GetSoundPath(appRef.SoundPath);
                    }
                }
            }
            // Sound Uses A Custom Path
            else if (soundPath.StartsWith("custom_sound_path/"))
            {
                var customPath = soundPath.Replace("custom_sound_path/", "");

                if (File.Exists(customPath))
                {
                    return customPath;
                }
            }
            else
            {
                // Check If Sound Is Present In The AppData Folder
                // If It Is, Use That, Else Use The One Inside The App WWW Folder
                var copiedPath = GetCopiedSoundPath(soundPath);
                var sourcePath = GetSourceSoundPath(soundPath);

                return File.Exists(copiedPath) ? copiedPath : sourcePath;
            }

            return GetSourceSoundPath("windows/win11");
        }

        public override void OnNotification(UserNotification notification)
        {
            if (Settings.ReadAloud || !allowedToPlaySound) { return; } // Don't Play A Sound If Text-To-Speech Is Playing Or If Disabled

            var appRef = AppReference.FromNotification(notification);
            var soundFilePath = GetSoundPath(appRef.SoundPath);

            if (!isPlaying)
            {
                isPlaying = true;

                // Play Sound Without Blocking The Main Thread
                Task.Run(() =>
                {
                    Player = new SoundPlayer(soundFilePath);
                    Player.Load();
                    Player.PlaySync();
                    Player.Dispose();
                    isPlaying = false;
                });
            }

            base.OnNotification(notification);
        }

        /// <summary>
        /// Plays a sound without any delay or timeout
        /// </summary>
        public static void PlaySoundWithoutTimeout(string soundPath)
        {
            var soundFilePath = GetSoundPath(soundPath);

            Task.Run(() =>
            {
                using (var p = new SoundPlayer(soundFilePath))
                {
                    p.Play();
                }
            });
        }

        public override void Reflow()
        {
            base.Reflow();
        }

        public override void Restart()
        {
            EnsureFakeSoundValidity();
            base.Restart();
        }

        public override void Start()
        {
            Restart();
            base.Start();
        }
    }
}
