using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    internal class Logging
    {
        public static string GetLogWatermark(string mode)
        {
            return $"\n\nCopyright © SamsidParty {DateTime.Now.Year}\nLicensed to you under the GPL v3.0 License\nLaunching in {mode} mode...";
        }
    }
}
