using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using WebFramework.Backend;
using Windows.Devices.Display.Core;
using Windows.Media.DialProtocol;

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

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DisplayDevice lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DisplayDevice
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class MonitorInfo
        {
            public int Size = 72;
            public Rect Monitor = new Rect();
            public Rect WorkArea = new Rect();
            public uint Flags = 0;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName = "";
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

        public static List<MonitorData> GetMonitors()
        {
            var monitors = new List<MonitorData>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
            {
                MonitorInfo currentMonitorInfo = new MonitorInfo();

                if (GetMonitorInfo(hMonitor, currentMonitorInfo))
                {
                    NotificationTester.MessageBox(currentMonitorInfo.DeviceName, currentMonitorInfo.DeviceName);
                }
                return true;
            }, IntPtr.Zero);

            return monitors;
        }

        private static IntPtr GetPreferredDisplay()
        {
            // Determine Whether To Use A Non-Primary Monitor
            if (InterceptorManager.Instance != null && InterceptorManager.Instance.CurrentSettings.PreferredMonitor != "primary")
            {
                
            }

            GetMonitors();

            // Return Primary Display
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
            var display = GetPreferredDisplay();
            MonitorInfo monitorInfo = new MonitorInfo();
            GetMonitorInfo(display, monitorInfo);
            return new Rectangle(0, 0, monitorInfo.Monitor.Right - monitorInfo.Monitor.Left, monitorInfo.Monitor.Bottom - monitorInfo.Monitor.Top);
        }

        public static float GetInverseScale()
        {
            uint dpiX;
            GetDpiForMonitor(GetPreferredDisplay(), DpiType.Effective, out dpiX, out _);
            return 100f / (dpiX * 100 / 96f);
        }

        public static float GetScale()
        {
            return 1f / GetInverseScale();
        }
    }
}
