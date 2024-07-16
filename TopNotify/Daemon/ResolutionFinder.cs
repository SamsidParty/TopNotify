using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Daemon
{
    public class ResolutionFinder
    {
        #region WinAPI
        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential)]
        struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
        #endregion


        public static Rectangle GetScaledResolution()
        {
            var factor = 1f + (((1f / GetInverseScale()) - 1f) / 2);
            var realRes = GetRealResolution();

            return new Rectangle(0, 0, (int)(realRes.Width * factor), (int)(realRes.Height * factor));
        }

        public static Rectangle GetRealResolution()
        {
            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            EnumDisplaySettings(null, -1, ref devMode);

            return new Rectangle(0, 0, (int)(devMode.dmPelsWidth), (int)(devMode.dmPelsHeight));
        }

        public static float GetInverseScale()
        {
            return 100f / (float)DPIUtil.ScaleFactor(Point.Empty);
        }

        public static float GetScale()
        {
            return 1f / GetInverseScale();
        }
    }
}
