using System;
using System.Diagnostics;
using System.Drawing;
using WebFramework;
using SamsidParty_TopNotify;
using WebFramework.PT;
using Microsoft.Toolkit.Uwp.Notifications;
using WebFramework.Backend;

public class Program
{

    public static ThemeBasedColor TitlebarColor;

    public static Daemon Background;

    [STAThread]
    public static void Main(string[] args)
    {
        //By Default, The App Will Be Launched In Daemon Mode
        //Daemon Mode Is A Background Process That Handles Changing The Position Of Notifications
        //If The "--settings" Arg Is Used, Then The App Will Launch In Settings Mode
        //Settings Mode Shows A GUI That Can Be Used To Configure The App
        //These Mode Switches Ensure All Functions Of The App Use The Same Executable

        if (args.Contains("--settings"))
        {

            if (Settings.Get().EnableDebugNotifications)
            {
                Console.WriteLine("Started Settings GUI");
                NotificationTester.Toast("Debug Notification", "Started Settings GUI");
            }

            PTWindowProvider.Activate();
            AppManager.Validate(args, "SamsidParty", "TopNotify");
            App();
        }
        else
        {
            Background = new Daemon();
        }

    }

    public static async Task App()
    {

        DevTools.Enable();
        DevTools.HotReload("http://127.0.0.1:25631"); // Vite Dev URL
        Logger.ForceOpenConsole();

        //Change Color Based On Theme (light, dark)
        TitlebarColor = new ThemeBasedColor(Color.FromArgb(255, 255, 255), Color.FromArgb(34, 34, 34));


        WindowManager.Options = new WindowOptions() {
            TitlebarColor = TitlebarColor,
            StartWidthHeight = new Rectangle(400, 600, 520, 780),
            LockWidthHeight = true,
            EnableAcrylic = true,
            IconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Image", "IconSmall.png")
        };

        WebScript.Register<Frontend>("frontend");

        await AppManager.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW"), OnReady);

        //Clean Up
        ToastNotificationManagerCompat.Uninstall();
    }

    public static async Task OnReady(WebWindow w)
    {
        w.BackgroundColor = TitlebarColor;
    }

}