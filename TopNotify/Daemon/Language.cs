using IgniteView.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;

namespace TopNotify.Daemon
{
    public class Language
    {
        /// <summary>
        /// The Name "New notification" Is Different In Every Language
        /// Therefore, To Grab The Notification Window, We Need To Know The Language
        /// </summary>
        public static string NotificationName
        {
            get
            {
                if (string.IsNullOrEmpty(_NotificationName))
                {
                    var currentLanguageID = CultureInfo.CurrentUICulture.Name;

                    // Parse the CSV file
                    var namesCSVFile = Util.GetFileResolver().ReadFileAsText("/Meta/NotificationNames.csv");
                    var namesCSVLines = namesCSVFile.Trim().Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    // Iterate languages
                    foreach (var entry in namesCSVLines) {
                        var entrySplit = entry.Trim().Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var languageID = entrySplit[0];
                        var languageNotificationName = entrySplit[1];

                        // In the CSV file, the languages will be stored as xx-*, we need to pattern match them
                        // Some languages will be stored as their full form like xx-YY, so we still have to check them
                        if (FileSystemName.MatchesSimpleExpression(languageID, currentLanguageID) || languageID == currentLanguageID)
                        {
                            _NotificationName = languageNotificationName;
                            break;
                        }
                    }
                }

                return _NotificationName;
            }
        }

        static string _NotificationName;
    }
}
