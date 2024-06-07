using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Daemon
{
    public class WindowClickThrough
    {
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static void ApplyToWindow(IntPtr hwnd)
        {
            if (InterceptorManager.Instance.CurrentSettings.EnableClickThrough)
            {
                SetWindowLong(hwnd, -20, GetWindowLong(hwnd, -20) | 0x80000 | 0x20);
            }
        }
    }
}
