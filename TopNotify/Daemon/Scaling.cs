//Modified From https://stackoverflow.com/a/76402258/18071273

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

namespace TopNotify.Daemon
{
    public static class DPIUtil
    {
        /// <summary>
        /// Min OS version build that supports DPI per monitor
        /// </summary>
        private const int MinOSVersionBuild = 14393;

        /// <summary>
        /// Min OS version major build that support DPI per monitor
        /// </summary>
        private const int MinOSVersionMajor = 10;

        /// <summary>
        /// Flag, if OS supports DPI per monitor
        /// </summary>
        private static bool _isSupportingDpiPerMonitor;

        /// <summary>
        /// Flag, if OS version checked
        /// </summary>
        private static bool _isOSVersionChecked;

        /// <summary>
        /// Flag, if OS supports DPI per monitor
        /// </summary>
        internal static bool IsSupportingDpiPerMonitor
        {
            get
            {
                if (_isOSVersionChecked)
                {
                    return _isSupportingDpiPerMonitor;
                }

                _isOSVersionChecked = true;
                var osVersionInfo = new OSVERSIONINFOEXW
                {
                    dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEXW))
                };

                if (RtlGetVersion(ref osVersionInfo) != 0)
                {
                    _isSupportingDpiPerMonitor = Environment.OSVersion.Version.Major >= MinOSVersionMajor && Environment.OSVersion.Version.Build >= MinOSVersionBuild;

                    return _isSupportingDpiPerMonitor;
                }

                _isSupportingDpiPerMonitor = osVersionInfo.dwMajorVersion >= MinOSVersionMajor && osVersionInfo.dwBuildNumber >= MinOSVersionBuild;

                return _isSupportingDpiPerMonitor;
            }
        }

        /// <summary>
        /// Get scale factor for an each monitor
        /// </summary>
        /// <param name="control"> Any control for OS who doesn't support DPI per monitor </param>
        /// <param name="monitorPoint"> Monitor point (Screen.Bounds) </param>
        /// <returns> Scale factor </returns>
        public static double ScaleFactor(Point monitorPoint)
        {
            var dpi = GetDpi(monitorPoint);

            return dpi * 100 / 96.0;
        }

        /// <summary>
        /// Get DPI for a monitor
        /// </summary>
        /// <param name="control"> Any control for OS who doesn't support DPI per monitor </param>
        /// <param name="monitorPoint"> Monitor point (Screen.Bounds) </param>
        /// <returns> DPI </returns>
        public static uint GetDpi(Point monitorPoint)
        {
            uint dpiX;

            if (IsSupportingDpiPerMonitor)
            {
                var monitorFromPoint = MonitorFromPoint(monitorPoint, 2);

                GetDpiForMonitor(monitorFromPoint, DpiType.Effective, out dpiX, out _);
            }
            else
            {
                return 1;
            }

            return dpiX;
        }

        /// <summary>
        /// Retrieves a handle to the display monitor that contains a specified point.
        /// </summary>
        /// <param name="pt"> Specifies the point of interest in virtual-screen coordinates. </param>
        /// <param name="dwFlags"> Determines the function's return value if the point is not contained within any display monitor. </param>
        /// <returns> If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor. </returns>
        /// <remarks>
        /// <see cref="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-monitorfrompoint"/>
        /// </remarks>
        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

        /// <summary>
        /// Queries the dots per inch (dpi) of a display.
        /// </summary>
        /// <param name="hmonitor"> Handle of the monitor being queried. </param>
        /// <param name="dpiType"> The type of DPI being queried. </param>
        /// <param name="dpiX"> The value of the DPI along the X axis. </param>
        /// <param name="dpiY"> The value of the DPI along the Y axis. </param>
        /// <returns> Status success </returns>
        /// <remarks>
        /// <see cref="https://learn.microsoft.com/en-us/windows/win32/api/shellscalingapi/nf-shellscalingapi-getdpiformonitor"/>
        /// </remarks>
        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        /// <summary>
        /// The RtlGetVersion routine returns version information about the currently running operating system.
        /// </summary>
        /// <param name="versionInfo"> Operating system version information </param>
        /// <returns> Status success</returns>
        /// <remarks>
        /// <see cref="https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/nf-wdm-rtlgetversion"/>
        /// </remarks>
        [SecurityCritical]
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlGetVersion(ref OSVERSIONINFOEXW versionInfo);

        /// <summary>
        /// Contains operating system version information.
        /// </summary>
        /// <remarks>
        /// <see cref="https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-osversioninfoexw"/>
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEXW
        {
            /// <summary>
            /// The size of this data structure, in bytes
            /// </summary>
            internal int dwOSVersionInfoSize;

            /// <summary>
            /// The major version number of the operating system.
            /// </summary>
            internal int dwMajorVersion;

            /// <summary>
            /// The minor version number of the operating system.
            /// </summary>
            internal int dwMinorVersion;

            /// <summary>
            /// The build number of the operating system.
            /// </summary>
            internal int dwBuildNumber;

            /// <summary>
            /// The operating system platform.
            /// </summary>
            internal int dwPlatformId;

            /// <summary>
            /// A null-terminated string, such as "Service Pack 3", that indicates the latest Service Pack installed on the system.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string szCSDVersion;

            /// <summary>
            /// The major version number of the latest Service Pack installed on the system. 
            /// </summary>
            internal ushort wServicePackMajor;

            /// <summary>
            /// The minor version number of the latest Service Pack installed on the system.
            /// </summary>
            internal ushort wServicePackMinor;

            /// <summary>
            /// A bit mask that identifies the product suites available on the system. 
            /// </summary>
            internal short wSuiteMask;

            /// <summary>
            /// Any additional information about the system.
            /// </summary>
            internal byte wProductType;

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            internal byte wReserved;
        }

        /// <summary>
        /// DPI type
        /// </summary>
        /// <remarks>
        /// <see cref="https://learn.microsoft.com/en-us/windows/win32/api/shellscalingapi/ne-shellscalingapi-monitor_dpi_type"/>
        /// </remarks>
        private enum DpiType
        {
            /// <summary>
            /// The effective DPI. This value should be used when determining the correct scale factor for scaling UI elements.
            /// </summary>
            Effective = 0,

            /// <summary>
            /// The angular DPI. This DPI ensures rendering at a compliant angular resolution on the screen.
            /// </summary>
            Angular = 1,

            /// <summary>
            /// The raw DPI. This value is the linear DPI of the screen as measured on the screen itself. Use this value when you want to read the pixel density and not the recommended scaling setting.
            /// </summary>
            Raw = 2,
        }
    }
}

