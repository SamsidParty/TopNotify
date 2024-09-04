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
            if (lang == "en")
            {
                return "New notification";
            }
            else if (lang == "fr")
            {
                return "Nouvelle notification";
            }
            else if (lang == "he")
            {
                return "הודעה חדשה";
            }
            else if (lang == "es")
            {
                return "Nueva notificación";
            }
            else if (lang == "ja")
            {
                return "新しい通知";
            }
            else if (lang == "pt")
            {
                return "Nova notificação";
            }
            else if (lang == "de")
            {
                return "Neue Benachrichtigung";
            }
            else if (lang == "zh")
            {
                return "新通知";
            }
            else if (lang == "it")
            {
                return "Nuova notifica";
            }
            else if (lang == "pl")
            {
                return "Nowe powiadomienie";
            }
            else if (lang == "sv")
            {
                return "Ny avisering";
            }
            else if (lang == "da")
            {
                return "Ny meddelelse";
            }
            else if (lang == "no")
            {
                return "Ny melding";
            }
            else if (lang == "ru")
            {
                return "Новое уведомление";
            }
            else if (lang == "ar")
            {
                return "إشعار جديد";
            }
            else if (lang == "hi")
            {
                return "नई सूचना";
            }
            else if (lang == "ko")
            {
                return "새로운 알림";
            }
            else
            {
                return "New notification";
            }

        }
    }
}
