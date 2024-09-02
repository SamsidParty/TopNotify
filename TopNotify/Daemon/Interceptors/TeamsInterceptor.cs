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

        /// <summary>
        /// Returns All The Running ms-teams.exe Processes
        /// </summary>
        public Process[] FindTeamsProcesses()
        {
            var possibleTeamsProcesses = Process.GetProcessesByName("ms-teams").Where((Process p) =>
            {
                try
                {
                    return !p.HasExited;
                }
                catch { return false; }
            });

            return possibleTeamsProcesses.ToArray()!;
        }

        public override void Reflow()
        {
            var teamsProcesses = FindTeamsProcesses();

            foreach (var teamsProcess in teamsProcesses)
            {
                if (teamsProcess != null && !InjectedProcesses.Contains(teamsProcess.Id)) // Check If It's Valid And Not Already Injected
                {
                    InjectedProcesses.Add(teamsProcess.Id);

                    //Inject Hook DLL Into Teams After 2 Seconds
                    //Allows Time For Teams To Init

                    Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        var result = InjectIntoProcess(teamsProcess.Id, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TopNotifyHook.dll"));
                        Logger.LogInfo("Teams Injector Returned: " + Marshal.PtrToStringUni(result));
                    });
                }
            }



            base.Reflow();
        }
    }
}
