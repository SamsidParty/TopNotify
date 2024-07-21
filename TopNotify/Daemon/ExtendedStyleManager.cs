using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Daemon;

namespace SamsidParty_TopNotify.Daemon
{
    public class ExtendedStyleManager
    {
        #region WinAPI

        [DllImport("user32.dll")]
        static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        #endregion

        #region Constants

        public const int GWL_EXSTYLE = -20;
        public const int LWA_ALPHA = 0x2;
        public const IntPtr WS_EX_LAYERED = 0x80000;
        public const IntPtr WS_EX_TRANSPARENT = 0x00000020;

        #endregion

        public List<IntPtr> Styles = new List<IntPtr>();
        public IntPtr BaseStyle = IntPtr.Zero;
        public IntPtr LastHandle = IntPtr.Zero;

        public ExtendedStyleManager(IntPtr baseStyle)
        {
            BaseStyle = baseStyle;
        }

        public void Update(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) { return; }

            LastHandle = hwnd;

            IntPtr styleToApply = AddExtendedStyles(BaseStyle);
            foreach (var style in Styles)
            {
                styleToApply |= style;
            }

            SetWindowLongPtr(hwnd, GWL_EXSTYLE, styleToApply);

            //Set Window Opacity
            SetLayeredWindowAttributes(hwnd, 0, (byte)(42.5 * (6 - InterceptorManager.Instance.CurrentSettings.Opacity)), LWA_ALPHA);
        }

        //Like Update, But Can Be Used Without An ExtendedStyleManager Object
        //Doesn't Apply Extra Styles From The Instance
        public static void AnonymousUpdate(IntPtr hwnd, IntPtr baseStyle)
        {
            if (hwnd == IntPtr.Zero) { return; }
            IntPtr styleToApply = AddExtendedStyles(baseStyle);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, styleToApply);
            SetLayeredWindowAttributes(hwnd, 0, (byte)(42.5 * (6 - InterceptorManager.Instance.CurrentSettings.Opacity)), LWA_ALPHA);
        }

        //Adds Extended Styles To The Provided Style Number Based On The User Config
        public static IntPtr AddExtendedStyles(IntPtr baseStyle)
        {
            IntPtr styleToApply = baseStyle;

            if (InterceptorManager.Instance.CurrentSettings.EnableClickThrough)
            {
                styleToApply |= WS_EX_TRANSPARENT;
            }
            styleToApply |= WS_EX_LAYERED;

            return styleToApply;
        }
    }
}
