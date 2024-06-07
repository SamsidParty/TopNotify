using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Daemon
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
            string lang = ci.TwoLetterISOLanguageName.ToLower();

            //Currently Only A Handful Of Lanugages
            //Open An Issue To Request A Language
            //ISO 639-1 Format
            //Statement Is From Downstream Project https://github.com/RoyRiv3r/notifications-anywhere
            switch (lang)
            {
                case "en":
                    return "New notification";
                case "fr":
                    return "Nouvelle notification";
                case "es":
                    return "Nueva notificación";
                case "ja":
                    return "新しい通知";
                case "pt":
                    return "Nova notificação";
                case "de":
                    return "Neue Benachrichtigung";
                case "zh":
                    return "新通知";
                case "it":
                    return "Nuova notifica";
                case "pl":
                    return "Nowe powiadomienie";
                case "sv":
                    return "Ny avisering";
                case "da":
                    return "Ny meddelelse";
                case "no":
                    return "Ny melding";
                case "ru":
                    return "Новое уведомление";
                case "ar":
                    return "إشعار جديد";
                case "hi":
                    return "नई सूचना";
                case "ko":
                    return "새로운 알림";
                default:
                    return "New notification";
            }

        }
    }
}
