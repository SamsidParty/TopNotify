using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    // Dictates The Value Of AppReference.ID
    public enum AppReferenceType
    {
        AppName, // Identify The App By It's Display Name
        WebsiteDomain // Identify The App By It's Domain (For Web Browser Notifications)
    }

    [Serializable]
    public class AppReference
    {
        /// <summary>
        /// Helps Interceptors Identify Which App Notifications Belong To
        /// </summary>
        public AppReferenceType ReferenceType;

        /// <summary>
        /// The Value Depending On The ReferenceType
        /// </summary>
        public string ID = null;

        public string DisplayName = "All Other Apps";

        /// <summary>
        /// Relative Path To The WAV File Stored In WWW/Audio, Without .wav Extension
        /// </summary>
        public string SoundPath = "internal/win11";
    }
}
