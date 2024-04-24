using System;
using System.Diagnostics;
using System.Drawing;
using WebFramework;
using SamsidParty_TopNotify;
using WebFramework.PT;
using Microsoft.Toolkit.Uwp.Notifications;

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

        Util.Log("Working Directory: " + Environment.CurrentDirectory);
        Util.Log("Base Directory: " + AppDomain.CurrentDomain.BaseDirectory);

        if (args.Contains("--settings"))
        {
            if (Settings.Get().EnableDebugNotifications)
            {
                Console.WriteLine("Started Settings GUI");
                NotificationTester.Toast("Debug Notification", "Started Settings GUI");
            }

            PTWindowProvider.Activate();
            AppManager.Validate(args);
            App();
        }
        else
        {
            Background = new Daemon();
            Background.MainLoop();
        }

    }

    public static async Task App()
    {

        //DevTools.Enable();
        //DevTools.HotReload("C:\\Users\\SamarthCat\\Documents\\Programming Stuff\\TopNotify\\TopNotify\\WWW");

        //Change Color Based On Theme (light, dark)
        TitlebarColor = new ThemeBasedColor(Color.FromArgb(255, 255, 255), Color.FromArgb(34, 34, 34));


        WindowManager.Options = new WindowOptions() {
            TitlebarColor = TitlebarColor,
            StartWidthHeight = new Rectangle(400, 600, 520, 780),
            LockWidthHeight = true,
            IconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Image", "Icon.ico")
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