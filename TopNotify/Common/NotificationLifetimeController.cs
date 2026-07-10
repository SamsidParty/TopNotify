using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    public class NotificationLifetimeController
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);

        const uint HWND_BROADCAST = 0xffff;
        const uint WM_SETTINGCHANGE = 0x001A;
        const uint SMTO_ABORTIFHUNG = 0x0002;
        const uint SPI_SETMESSAGEDURATION = 0x2017;
        const uint SPIF_UPDATEINIFILE = 0x01;
        const uint SPIF_SENDCHANGE = 0x02;
        const int PermanentDurationSeconds = 86400;

        public static void Apply(Settings settings)
        {
            var duration = GetWindowsDuration(settings);
            if (duration == null) { return; }

            try
            {
                SystemParametersInfo(SPI_SETMESSAGEDURATION, 0, (uint)duration.Value, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Accessibility");
                key.SetValue("MessageDuration", duration.Value, RegistryValueKind.DWord);
                SendMessageTimeout(new IntPtr(HWND_BROADCAST), WM_SETTINGCHANGE, UIntPtr.Zero, @"Control Panel\Accessibility", SMTO_ABORTIFHUNG, 1000, out _);
                Program.Logger?.Information($"Applied Windows notification duration: {duration.Value} seconds");
            }
            catch (Exception ex)
            {
                Program.Logger?.Warning($"Failed to update Windows notification duration: {ex.Message}");
            }
        }

        public static int? GetWindowsDuration(Settings settings)
        {
            var durations = new List<int>();

            AddDuration(durations, settings.StickyMode, settings.StickyDurationSeconds);

            foreach (var appReference in settings.AppReferences)
            {
                var mode = appReference.GetStickyMode(settings);
                var seconds = appReference.GetStickyDurationSeconds(settings);
                AddDuration(durations, mode, seconds);
            }

            if (!durations.Any()) { return null; }

            return durations.Max();
        }

        public static int GetPermanentDurationSeconds()
        {
            return PermanentDurationSeconds;
        }

        private static void AddDuration(List<int> durations, NotificationStickyMode mode, int seconds)
        {
            if (mode == NotificationStickyMode.Permanent)
            {
                durations.Add(PermanentDurationSeconds);
            }
            else if (mode == NotificationStickyMode.Seconds)
            {
                durations.Add(Math.Clamp(seconds, 1, PermanentDurationSeconds));
            }
        }
    }
}
