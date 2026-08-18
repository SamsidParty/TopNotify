using System;
using System.Threading.Tasks;
using IgniteView.Core;
using TopNotify.Common;
using Windows.ApplicationModel;

namespace TopNotify.GUI
{
    /// <summary>
    /// Reads/toggles the OS-level "run at startup" registration declared in AppxManifest.xml
    /// (uap5:StartupTask). That manifest declaration only sets the default state Windows
    /// starts a fresh install with - RequestEnableAsync()/Disable() here is the actual
    /// per-user control surface, and is what backs the in-app toggle.
    /// </summary>
    public class StartupManager
    {
        // Must match the uap5:StartupTask TaskId in AppxManifest.xml
        const string TaskId = "TopNotifyUWP";

        [Command("GetStartupState")]
        public static async Task<string> GetStartupState()
        {
            try
            {
                var task = await StartupTask.GetAsync(TaskId);
                Program.Logger.Information($"StartupManager: current state is {task.State}");
                return task.State.ToString();
            }
            catch (Exception ex)
            {
                Program.Logger.Warning(ex, "StartupManager: failed to read startup task state");
                return "Unknown";
            }
        }

        [Command("SetStartupEnabled")]
        public static async Task<string> SetStartupEnabled(bool enabled)
        {
            try
            {
                var task = await StartupTask.GetAsync(TaskId);

                if (enabled)
                {
                    var newState = await task.RequestEnableAsync();
                    Program.Logger.Information($"StartupManager: RequestEnableAsync resulted in {newState}");
                    return newState.ToString();
                }

                task.Disable();
                Program.Logger.Information($"StartupManager: Disable() resulted in {task.State}");
                return task.State.ToString();
            }
            catch (Exception ex)
            {
                Program.Logger.Warning(ex, "StartupManager: failed to change startup task state");
                return "Unknown";
            }
        }
    }
}
