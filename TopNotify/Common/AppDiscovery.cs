using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    public enum AppDiscoveryMethod
    {
        MatchAlways, // Always Active
        MatchMSIX, // Checks If An MSIX Package Exists That Contains The Specified Search Term
        MatchCustomPath // Checks If A Specified Path Exists
    }

    [Serializable]
    public class AppDiscovery
    {
        /// <summary>
        /// Determines How TopNotify Will Check If The App Is Installed
        /// </summary>
        public AppDiscoveryMethod Method = AppDiscoveryMethod.MatchAlways;

        /// <summary>
        /// How This Search Term Is Used Depends On The App Discovery Method
        /// </summary>
        public string SearchTerm;

        /// <summary>
        /// Checks If An App Is Installed Based On Multiple Discovery Parameters
        /// </summary>
        public static bool IsAppInstalled(AppDiscovery[] discoveries)
        {
            foreach (var discovery in discoveries)
            {
                if (IsAppInstalled(discovery))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks If An App Is Installed Based On It's Discovery Parameters
        /// </summary>
        public static bool IsAppInstalled(AppDiscovery discovery)
        {
            if (discovery == null) { return false; }

            if (discovery.Method == AppDiscoveryMethod.MatchAlways) { return true; }
            else if (discovery.Method == AppDiscoveryMethod.MatchMSIX) 
            {
                var allApps = Directory.GetDirectories("C:\\Program Files\\WindowsApps");

                foreach (var app in allApps)
                {
                    if (app.Contains(discovery.SearchTerm))
                    {
                        return true;
                    }
                }
            }
            else if (discovery.Method == AppDiscoveryMethod.MatchCustomPath)
            {
                // The Search Term May Contain Environment Variables
                var pathWithEnvVariables = Environment.ExpandEnvironmentVariables(discovery.SearchTerm);
                return Directory.Exists(pathWithEnvVariables) || File.Exists(pathWithEnvVariables);
            }

            return false;
        }
    }
}
