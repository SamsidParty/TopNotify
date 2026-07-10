using IgniteView.Core;
using IgniteView.Desktop;
using Newtonsoft.Json;
using SamsidParty_TopNotify.Daemon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using TopNotify.Daemon;
using Windows.ApplicationModel.Store;

namespace TopNotify.GUI
{
    public class MainCommands
    {
        static bool isSaving = false;

        //Called By JavaScript
        //Spawns A Test Notification
        [Command("SpawnTestNotification")]
        public static void SpawnTestNotification()
        {
            NotificationTester.SpawnTestNotification();
        }

        //Called By JavaScript
        //Opens The About Page
        [Command("About")]
        public static void About()
        {
            WebWindow.Create("/index.html?about")
                .WithTitle("About TopNotify")
                .WithBounds(new LockedWindowBounds((int)(400f * ResolutionFinder.GetScale()), (int)(300f * ResolutionFinder.GetScale())))
                .With((w) => (w as Win32WebWindow).BackgroundMode = Win32WebWindow.WindowBackgroundMode.Acrylic)
                .WithoutTitleBar()
                .Show();
        }

        [Command("Donate")] 
        public static async void Donate()
        {
            try
            {
                var results = await Program.Context.RequestPurchaseAsync("9P92HZT8QC8R");

                if (results.Status == Windows.Services.Store.StorePurchaseStatus.Succeeded)
                {
                    NotificationTester.MessageBox("Thank You For Donating", "Your contribution is much appreciated ❤️");
                }
                else
                {
                    NotificationTester.MessageBox("Failed To Purchase Donation", results.Status.ToString());
                }
            }
            catch (Exception ex)
            {
                NotificationTester.MessageBox("Failed To Purchase Donation", ex.Message);
            }
        }

        [Command("GetVersion")]
        public static string GetVersion()
        {
            // Read version from Appx Manifest
            var appxManifest = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "AppxManifest.xml");
            if (File.Exists(appxManifest)) { 
                var manifestData = File.ReadAllText(appxManifest);

                var from = manifestData.IndexOf("Version=\"") + "Version=\"".Length;
                var to = manifestData.LastIndexOf("\"");

                var result = manifestData.Substring(from, to - from);
                return " v" + result.Substring(0, 5);
            }

            return " Debug";
        }

        //Called By JavaScript
        //Sends The Config File
        [Command("RequestConfig")]
        public static void RequestConfig(WebWindow target)
        {
            target.SendConfig();
        }

        [Command("FindInstalledApps")]
        public static string FindInstalledApps()
        {
            try
            {
                var output = Util.SimpleCMD("powershell -NoProfile -Command \"Get-StartApps | Sort-Object Name | Select-Object Name, AppID | ConvertTo-Json -Compress\"").Trim();
                if (String.IsNullOrWhiteSpace(output)) { return "[]"; }
                if (output.StartsWith("{")) { output = "[" + output + "]"; }
                return output;
            }
            catch
            {
                return "[]";
            }
        }

        [Command("FindInstalledAppIcon")]
        public static string FindInstalledAppIcon(string name, string appID)
        {
            var shortcuts = FindStartMenuShortcuts();
            var app = new InstalledAppInfo()
            {
                Name = name,
                AppID = appID
            };

            return FindInstalledAppIcon(app, shortcuts) ?? "/Image/DefaultAppReferenceIcon.svg";
        }

        public static Dictionary<string, string> FindStartMenuShortcuts()
        {
            var result = new Dictionary<string, string>();
            var folders = new List<string>()
            {
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
            };

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder)) { continue; }

                var enumerationOptions = new EnumerationOptions()
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true
                };

                foreach (var shortcut in Directory.EnumerateFiles(folder, "*.lnk", enumerationOptions))
                {
                    var key = Path.GetFileNameWithoutExtension(shortcut).ToLowerInvariant();
                    if (!result.ContainsKey(key))
                    {
                        result.Add(key, shortcut);
                    }
                }
            }

            return result;
        }

        public static string FindInstalledAppIcon(InstalledAppInfo app, Dictionary<string, string> shortcuts)
        {
            // Get-StartApps returns an AppUserModelID for many packaged and classic
            // applications. Asking the AppsFolder shell namespace for its thumbnail
            // handles package manifest logos, indirect resources and shell-generated
            // icons that Icon.ExtractAssociatedIcon cannot resolve.
            var shellIcon = GetShellAppIconDataURL(app.AppID);
            if (shellIcon != null)
            {
                return shellIcon;
            }

            var appID = Environment.ExpandEnvironmentVariables(app.AppID ?? "");
            if (File.Exists(appID))
            {
                return GetIconDataURL(appID);
            }

            var key = (app.Name ?? "").ToLowerInvariant();
            if (shortcuts.ContainsKey(key))
            {
                return GetShortcutIconDataURL(shortcuts[key]);
            }

            return null;
        }

        public static string GetShortcutIconDataURL(string path)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) { return null; }

                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(path);
                string iconLocation = shortcut.IconLocation;
                string targetPath = shortcut.TargetPath;

                if (!String.IsNullOrWhiteSpace(iconLocation))
                {
                    var iconPath = iconLocation.Split(',')[0].Trim().Trim('"');
                    iconPath = Environment.ExpandEnvironmentVariables(iconPath);
                    if (File.Exists(iconPath))
                    {
                        var result = GetIconDataURL(iconPath);
                        if (result != null) { return result; }
                    }
                }

                targetPath = Environment.ExpandEnvironmentVariables(targetPath ?? "").Trim().Trim('"');
                if (File.Exists(targetPath))
                {
                    return GetIconDataURL(targetPath);
                }
            }
            catch
            {
            }

            return null;
        }

        public static string GetIconDataURL(string path)
        {
            try
            {
                using var icon = Icon.ExtractAssociatedIcon(path);
                if (icon == null) { return null; }

                using var bitmap = icon.ToBitmap();
                using var resized = new Bitmap(bitmap, new Size(32, 32));
                using var memory = new MemoryStream();
                resized.Save(memory, ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(memory.ToArray());
            }
            catch
            {
                return null;
            }
        }

        public static string GetShellAppIconDataURL(string appID)
        {
            if (String.IsNullOrWhiteSpace(appID)) { return null; }

            IShellItemImageFactory imageFactory = null;
            IntPtr bitmapHandle = IntPtr.Zero;
            try
            {
                var iid = typeof(IShellItemImageFactory).GUID;
                var parsingName = "shell:AppsFolder\\" + appID;
                var result = SHCreateItemFromParsingName(parsingName, IntPtr.Zero, ref iid, out imageFactory);
                if (result != 0 || imageFactory == null) { return null; }

                imageFactory.GetImage(new NativeSize(32, 32), ShellItemImageFlags.IconOnly | ShellItemImageFlags.BiggerSizeOk, out bitmapHandle);
                if (bitmapHandle == IntPtr.Zero) { return null; }

                using var bitmap = Bitmap.FromHbitmap(bitmapHandle);
                using var memory = new MemoryStream();
                bitmap.Save(memory, ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(memory.ToArray());
            }
            catch
            {
                return null;
            }
            finally
            {
                if (bitmapHandle != IntPtr.Zero) { DeleteObject(bitmapHandle); }
                if (imageFactory != null && Marshal.IsComObject(imageFactory)) { Marshal.ReleaseComObject(imageFactory); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeSize
        {
            public int Width;
            public int Height;

            public NativeSize(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        [Flags]
        public enum ShellItemImageFlags
        {
            BiggerSizeOk = 0x1,
            IconOnly = 0x4
        }

        [ComImport]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItemImageFactory
        {
            void GetImage(NativeSize size, ShellItemImageFlags flags, out IntPtr bitmapHandle);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName(
            string path,
            IntPtr bindContext,
            ref Guid requestedInterface,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory shellItem);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr objectHandle);

        //Called By JavaScript
        //Write Settings File
        [Command("WriteConfigFile")]
        public static void WriteConfigFile(WebWindow target, string data)
        {

            if (isSaving) { return; }
            isSaving = true;

            Settings.Overwrite(data);

            Thread.Sleep(100); // Prevent Crashing Daemon From Spamming Button

            // Tell The Daemon The Config Has Changed
            Daemon.Daemon.SendCommandToDaemon("UpdateConfig");

            isSaving = false;
        }

        [Command("OpenAppFolder")]
        public static async Task OpenAppFolder(WebWindow target)
        {
            Process.Start("explorer.exe", Settings.GetAppDataFolder());
        }

        [Command("OpenSoundFolder")]
        public static async Task OpenSoundFolder(WebWindow target)
        {
            Process.Start("explorer.exe", SoundFinder.ImportedSoundFolder);
        }
    }

    public class InstalledAppInfo
    {
        public string Name;
        public string AppID;
        public string DisplayIcon;
    }
}
