using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace TopNotify.Common
{
    public class DiscoveredApp
    {
        public string AppUserModelId;
        public string PackageFamilyName;
        public string Id;
        public string DisplayName;
        public string Description;

        /// <summary>
        /// Converts An AppInfo Object Into A DiscoveredApp Object
        /// </summary>
        public static DiscoveredApp FromAppInfo(AppInfo appInfo)
        {
            return new DiscoveredApp()
            {
                AppUserModelId = appInfo.AppUserModelId,
                PackageFamilyName = appInfo.PackageFamilyName,
                Id = appInfo.Id,
                DisplayName = appInfo.DisplayInfo.DisplayName,
                Description = appInfo.DisplayInfo.Description
            };
        }
    }
}
