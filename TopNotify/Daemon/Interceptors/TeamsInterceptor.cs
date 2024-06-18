using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WebFramework.Backend;
using static TopNotify.Daemon.NativeInterceptor;

namespace TopNotify.Daemon
{
    public class TeamsInterceptor : Interceptor
    {
        [DllImport("IVPluginTopNotify.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr InjectIntoProcess(int processId, string dllPath);

        public Process FindTeamsProcess()
        {
            return Process.GetProcessesByName("ms-teams").Where((Process p) =>
            {
                try
                {
                    return !p.HasExited;
                }
                catch { return false; }
            }).FirstOrDefault();
        }

        public override void Start()
        {
            var result = InjectIntoProcess(FindTeamsProcess().Id, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TopNotifyHook.dll"));
            Logger.LogInfo(Marshal.PtrToStringUni(result));
            base.Start();
        }
    }
}
