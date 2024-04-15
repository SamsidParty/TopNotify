using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Buffers;
using System.Reflection;
using SamsidParty_TopNotify;

namespace SamsidParty_TopNotify
{
    public class NotificationTester
    {
        public static void Toast(string title, string content)
        {
            //Call The NotifyIcon.ShowBalloonTip Method
            NotifyIcon notify = new NotifyIcon();
            notify.Visible = true;
            notify.Icon = Util.FindAppIcon();
            notify.ShowBalloonTip(1000, title, content, ToolTipIcon.None);
            notify.Dispose();
        }
    }
}
