#define TRACE // Enable Trace.WriteLine

using System;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Toolkit.Uwp.Notifications;
using TopNotify.Daemon;
using TopNotify.Common;
using TopNotify.GUI;
using IgniteView.Core;
using IgniteView.Desktop;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TopNotify.Common
{
    public class Program
    {

        public static Daemon.Daemon Background;
        public static AppManager GUI;

        [STAThread]
        public static void Main(string[] args)
        {
            //By Default, The App Will Be Launched In Daemon Mode
            //Daemon Mode Is A Background Process That Handles Changing The Position Of Notifications
            //If The "--settings" Arg Is Used, Then The App Will Launch In Settings Mode
            //Settings Mode Shows A GUI That Can Be Used To Configure The App
            //These Mode Switches Ensure All Functions Of The App Use The Same Executable

            //Find Other Instances Of TopNotify
            var validTopNotifyInstances = Process.GetProcessesByName("TopNotify").Where((p) => !p.HasExited && p.Id != Process.GetCurrentProcess().Id);

            var isGUIRunning = validTopNotifyInstances.Where((p) => {
                string commandLine;
                ProcessCommandLine.Retrieve(p, out commandLine, ProcessCommandLine.Parameter.CommandLine);
                return commandLine.ToLower().Contains("--settings");
            }).Any();

            var isDaemonRunning = validTopNotifyInstances.Where((p) => {
                string commandLine;
                ProcessCommandLine.Retrieve(p, out commandLine, ProcessCommandLine.Parameter.CommandLine);
                return !commandLine.ToLower().Contains("--settings");
            }).Any();

            #if !GUI_DEBUG
            if (!args.Contains("--settings") && isDaemonRunning && !isGUIRunning)
            {
                //Open GUI Instead Of Daemon
                TrayIcon.LaunchSettingsMode(null, null);
                Environment.Exit(1);
            }
            else if (args.Contains("--settings") && isGUIRunning)
            {
                //Exit To Prevent Multiple GUIs
                Environment.Exit(2);
            }
            else if (!args.Contains("--settings") && isDaemonRunning && isGUIRunning)
            {
                //Exit To Prevent Multiple Daemons
                Environment.Exit(3);
            }
            #endif

            DesktopPlatformManager.Activate(); // Needed here to initiate plugin DLL loading

            if (args.Contains("--debug-process")) { Debugger.Launch(); } // Start Debugging

            #if !GUI_DEBUG
            if (args.Contains("--settings"))
            #else
            if (true)
            #endif
            {
                //Open The GUI App In Settings Mode
                GUI = new ViteAppManager();
                App();
            }
            else
            {
                //Open The Background Daemon
                Background = new Daemon.Daemon();
            }

        }

        public static async Task App()
        {
            // Copy The Wallpaper File So That The GUI Can Access It
            WallpaperFinder.CopyWallpaper();
            AppManager.Instance.RegisterDynamicFileRoute("/wallpaper.jpg", WallpaperFinder.WallpaperRoute);

            var mainWindow =
                WebWindow.Create()
                .WithTitle("TopNotify")
                .WithBounds(new LockedWindowBounds((int)(520f * ResolutionFinder.GetScale()), (int)(780f * ResolutionFinder.GetScale())))
                .With((w) => (w as Win32WebWindow).BackgroundMode = Win32WebWindow.WindowBackgroundMode.Acrylic)
                .WithoutTitleBar()
                .Show();

            // Clean Up
            GUI.OnCleanUp += () =>
            {
                WallpaperFinder.CleanUp();
                ToastNotificationManagerCompat.Uninstall();
            };

            GUI.Run();
        }

    }
}

