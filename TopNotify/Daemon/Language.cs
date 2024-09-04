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
            string systemLanguage = ci.TwoLetterISOLanguageName.ToLower();

            //Currently Only A Handful Of Lanugages
            //Open An Issue To Request A Language
            //ISO 639-1 Format
            if (systemLanguage == "en")
            {
                return "New notification";
            }
            else if (systemLanguage == "fr")
            {
                return "Nouvelle notification";
            }
            else if (systemLanguage == "he")
            {
                return "הודעה חדשה";
            }
            else if (systemLanguage == "es")
            {
                return "Nueva notificación";
            }
            else if (systemLanguage == "ja")
            {
                return "新しい通知";
            }
            else if (systemLanguage == "pt")
            {
                return "Nova notificação";
            }
            else if (systemLanguage == "de")
            {
                return "Neue Benachrichtigung";
            }
            else if (systemLanguage == "zh")
            {
                return "新通知";
            }
            else if (systemLanguage == "it")
            {
                return "Nuova notifica";
            }
            else if (systemLanguage == "pl")
            {
                return "Nowe powiadomienie";
            }
            else if (systemLanguage == "sv")
            {
                return "Ny avisering";
            }
            else if (systemLanguage == "da")
            {
                return "Ny meddelelse";
            }
            else if (systemLanguage == "no")
            {
                return "Ny melding";
            }
            else if (systemLanguage == "ru")
            {
                return "Новое уведомление";
            }
            else if (systemLanguage == "ar")
            {
                return "إشعار جديد";
            }
            else if (systemLanguage == "hi")
            {
                return "नई सूचना";
            }
            else if (systemLanguage == "ko")
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
