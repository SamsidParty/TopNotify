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

        private static List<int> InjectedProcesses = new List<int>();

        public Process FindTeamsProcess()
        {
            var possibleTeamsProcesses = Process.GetProcessesByName("ms-teams").Where((Process p) =>
            {
                try
                {
                    return !p.HasExited;
                }
                catch { return false; }
            });

            //Return The Process Of Teams If It's Running
            //Otherwise Returns Null
            return possibleTeamsProcesses.Any() ? possibleTeamsProcesses.FirstOrDefault()! : null!;
        }

        public override void Reflow()
        {
            var teamsProcess = FindTeamsProcess();

            if (teamsProcess != null && !InjectedProcesses.Contains(teamsProcess.Id))
            {
                InjectedProcesses.Add(teamsProcess.Id);
                var result = InjectIntoProcess(teamsProcess.Id, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TopNotifyHook.dll"));
                Logger.LogInfo(Marshal.PtrToStringUni(result));
            }


            base.Reflow();
        }
    }
}
