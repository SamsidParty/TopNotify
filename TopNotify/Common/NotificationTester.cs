using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Buffers;
using System.Reflection;
using Microsoft.Toolkit.Uwp.Notifications;

namespace TopNotify.Common
{
    public class NotificationTester
    {
        public static void Toast(string title, string content)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(content)
                .Show();
        }
    }
}
