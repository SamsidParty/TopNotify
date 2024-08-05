using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WebFramework.Backend;
using WindowsDisplayAPI;

namespace TopNotify.Daemon
{
    public class ResolutionFinder
    {
        #region WinAPI

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

        private enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2,
        }

        #endregion

        private static Display GetPrimaryDisplay()
        {
            return Display.GetDisplays().Where((d) => d.IsGDIPrimary).FirstOrDefault();
        }

        public static Rectangle GetScaledResolution()
        {
            var factor = 1f + (((1f / GetInverseScale()) - 1f) / 2);
            var realRes = GetRealResolution();

            return new Rectangle(0, 0, (int)(realRes.Width * factor), (int)(realRes.Height * factor));
        }

        public static Rectangle GetRealResolution()
        {
            var display = GetPrimaryDisplay();
            return new Rectangle(0, 0, (int)(display.CurrentSetting.Resolution.Width), (int)(display.CurrentSetting.Resolution.Height));
        }

        public static float GetInverseScale()
        {
            var display = GetPrimaryDisplay();
            uint dpiX;
            var monitorFromPoint = MonitorFromPoint(display.CurrentSetting.Position, 2);
            GetDpiForMonitor(monitorFromPoint, DpiType.Effective, out dpiX, out _);
            return 100f / (dpiX * 100 / 96f);
        }

        public static float GetScale()
        {
            return 1f / GetInverseScale();
        }
    }
}
