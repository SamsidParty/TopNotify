using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace TopNotify.Daemon
{
    public class ReadAloudInterceptor : Interceptor
    {
        SpeechSynthesizer Synthesizer;

        public override void OnNotification(UserNotification notification)
        {
            if (!Settings.ReadAloud) { return; }

            if (Synthesizer == null) { Synthesizer = new SpeechSynthesizer(); }

            Synthesizer.SetOutputToDefaultAudioDevice();
            Synthesizer.Speak(GetNotificationAsText(notification));


            base.OnNotification(notification);
        }


        /// <summary>
        /// Returns A Text-To-Speech Friendly String Based On A Notification's Contents
        /// </summary>
        public string GetNotificationAsText(UserNotification notification)
        {
            var text = "New notification from";
            text += notification.AppInfo.DisplayInfo.DisplayName + ".\n";
            foreach (var binding in notification.Notification.Visual.Bindings)
            {
                foreach (var notificationText in binding.GetTextElements())
                {
                    text += notificationText.Text + ".\n";
                }
            }

            return text;
        }
    }
}
