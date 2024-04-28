using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamsidParty_TopNotify
{
    public class InterceptorManager
    {
        public static InterceptorManager Instance;
        public List<Interceptor> Interceptors = new();
        public Settings CurrentSettings;
        public int TimeSinceReflow = 0;

        public const int ReflowTimeout = 50;

        public void Start()
        {
            Instance = this;
            CurrentSettings = Settings.Get();

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(Settings.GetFilePath());
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.json";
            watcher.Changed += new FileSystemEventHandler(OnSettingsChanged);
            watcher.EnableRaisingEvents = true;

            Interceptors.Add(new NativeInterceptor());

            foreach (Interceptor i in Interceptors)
            {
                i.Start();
            }
            MainLoop();
        }

        public void MainLoop()
        {
            while (true)
            {
                TimeSinceReflow++;

                if (TimeSinceReflow > ReflowTimeout)
                {
                    TimeSinceReflow = 0;

                    foreach (Interceptor i in Interceptors)
                    {
                        i.Reflow();
                    }
                }

                foreach (Interceptor i in Interceptors)
                {
                    i.Update();
                }

                Thread.Sleep(10);
            }
        }

        public void OnSettingsChanged(object sender, EventArgs e)
        {
            CurrentSettings = Settings.Get();

            foreach (Interceptor i in Interceptors)
            {
                i.Restart();
                i.Reflow();
            }
        }
    }
}
