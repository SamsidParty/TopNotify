using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Buffers;
using System.Reflection;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;

namespace TopNotify.Common
{
    public class NotificationTester
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        public static void Toast(string title, string content)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(content)
                .Show();
        }

        public static void MessageBox(string title, string content)
        {
            MessageBox(IntPtr.Zero, content, title, 0);
        }
    }
}
