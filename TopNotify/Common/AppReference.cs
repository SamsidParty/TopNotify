using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Daemon;
using TopNotify.GUI;
using Windows.UI.Notifications;

namespace TopNotify.Common
{
    // Dictates The Value Of AppReference.ID
    public enum AppReferenceType
    {
        AppName, // Identify The App By It's Display Name
        WebsiteDomain // Identify The App By It's Domain (For Web Browser Notifications)
    }

    /// <summary>
    /// Stores Settings For Individual Apps
    /// </summary>
    [Serializable]
    public class AppReference
    {
        /// <summary>
        /// Helps Interceptors Identify Which App Notifications Belong To
        /// </summary>
        public AppReferenceType ReferenceType;

        /// <summary>
        /// Helps Identify If The App Is Installed Or Not, And Whether To Activate It
        /// </summary>
        public AppDiscovery[] Discovery;

        /// <summary>
        /// The Value Depending On The ReferenceType
        /// </summary>
        public string ID;

        public string DisplayName;

        /// <summary>
        /// URL Of The App's Icon, Can Be A Data URL
        /// </summary>
        public string DisplayIcon;

        /// <summary>
        /// Relative Path To The WAV File Stored In WWW/Audio, Without .wav Extension
        /// </summary>
        public string SoundPath;

        /// <summary>
        /// Identifies An AppReference Based On A Notification
        /// </summary>
        public static AppReference FromNotification(UserNotification notification)
        {
            var references = Settings.Get().AppReferences;

            foreach (var reference in references)
            {
                if (reference.ReferenceType == AppReferenceType.AppName && notification.AppInfo.DisplayInfo.DisplayName == reference.ID)
                {
                    return reference;
                }
                //TODO: Identify The Domain Of Browser Notifications
            }

            // Return The Default AppReference
            return references.Where((r) => r.ID == "Other").FirstOrDefault();
        }
    }
}
