using IgniteView.Core;
using IgniteView.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    internal class Logging
    {
        /// <summary>
        /// Writes a copyright watermark as well as debug information to the log
        /// </summary>
        public static void WriteWatermark(string mode)
        {
            Program.Logger.Information($"Copyright © SamsidParty {DateTime.Now.Year}\nLicensed to you under the GPL v3.0 License");
            Program.Logger.Information($"Launching in {mode} mode...");
            Program.Logger.Information($"System Username: {Environment.UserName}");
            Program.Logger.Information($"System Architecture: {RuntimeInformation.ProcessArchitecture}");
            Program.Logger.Information($"System Name: {Environment.MachineName}");
        }
    }
}
