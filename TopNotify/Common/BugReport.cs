using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Daemon;
using TopNotify.GUI;

namespace TopNotify.Common
{
    public class BugReport
    {
        public static string CreateBugReport()
        {
            var report = "Please paste the below text into a GitHub issue (https://github.com/SamsidParty/TopNotify/issues)\n\n\n";

            var nativeInterceptor = InterceptorManager.Instance.Interceptors.Where((t) => t.GetType() == typeof(NativeInterceptor)).First() as NativeInterceptor;

            report += $"\n----------- Start Bug Report -----------\n";
            report += $"TopNotify Version: {MainCommands.GetVersion()}\n";
            report += $"Environment: {Environment.OSVersion}\n";
            report += $"Architecture: {RuntimeInformation.ProcessArchitecture}\n";
            report += $"System Language: {CultureInfo.CurrentUICulture.Name}\n";
            report += $"Notification Name: {Language.NotificationName}\n";
            report += $"Needs Fallback Interceptor: {String.IsNullOrEmpty(Language.NotificationName)}\n";
            report += $"Forces Fallback Interceptor: {InterceptorManager.Instance.CurrentSettings.EnableDebugForceFallbackMode}\n";
            report += $"Active Interceptors: {string.Join(", ", InterceptorManager.Instance.Interceptors.Select((i) => i.GetType().Name))}\n";
            report += $"Notification Handle: {nativeInterceptor.hwnd}\n";
            report += $"Scale: {ResolutionFinder.GetScale()}\n";
            report += $"Inverse Scale: {ResolutionFinder.GetInverseScale()}\n";
            report += $"Real Resolution: {ResolutionFinder.GetRealResolution().Size.Width}x{ResolutionFinder.GetRealResolution().Size.Height}\n";
            report += $"Scaled Resolution: {ResolutionFinder.GetScaledResolution().Size.Width}x{ResolutionFinder.GetScaledResolution().Size.Height}\n";
            report += $"Config File:\n\n{Settings.GetForIPC()}\n";
            report += $"\n----------- Finish Bug Report ----------\n";

            return report;
        }

        public static void DisplayBugReport(string report)
        {
            new Thread(() =>
            {
                var tempPath = Path.GetTempFileName();
                File.WriteAllText(tempPath, report);
                Util.SimpleCMD($"notepad.exe \"{tempPath}\"");
                File.Delete(tempPath);
            }).Start();
        }
    }
}
