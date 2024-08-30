using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WebFramework.Backend;

namespace TopNotify.Daemon
{
    public class ResolutionFinder
    {
        #region WinAPI

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

        [DllImport("User32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, [In, Out] MonitorInfo lpmi);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public class MonitorInfo
        {
            public int Size = Marshal.SizeOf(typeof(MonitorInfo));
            public Rect Monitor = new Rect();
            public Rect WorkArea = new Rect();
            public uint Flags = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2,
        }

        #endregion

        private static IntPtr GetPrimaryDisplay()
        {
            return MonitorFromPoint(new Point(0, 0), 0x00000001);
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
            MonitorInfo monitorInfo = new MonitorInfo();
            GetMonitorInfo(display, monitorInfo);
            return new Rectangle(0, 0, monitorInfo.Monitor.Right - monitorInfo.Monitor.Left, monitorInfo.Monitor.Bottom - monitorInfo.Monitor.Top);
        }

        public static float GetInverseScale()
        {
            uint dpiX;
            GetDpiForMonitor(GetPrimaryDisplay(), DpiType.Effective, out dpiX, out _);
            return 100f / (dpiX * 100 / 96f);
        }

        public static float GetScale()
        {
            return 1f / GetInverseScale();
        }
    }
}
