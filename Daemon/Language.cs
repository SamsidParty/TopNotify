using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamsidParty_TopNotify
{
    public class Language
    {

        /// <summary>
        /// The Name "New notification" Is Different In Every Language
        /// Therefore, To Grab The Notification Window, We Need To Know The Language
        /// </summary>
        public static string GetNotificationName()
        {
            var ci = CultureInfo.CurrentUICulture;

            //Currently Only Supports French
            //Open An Issue To Request A Language
            //ISO 639-1 Format
            if (ci.TwoLetterISOLanguageName.ToUpper() == "FR")
            {
                return "Nouvelle notification";
            }

            return "New notification";
        }
    }
}
