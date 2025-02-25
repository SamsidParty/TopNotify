using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    public class Util
    {

        /// <summary>
        /// Runs A Command Prompt Command And Returns The Output
        /// </summary>
        /// <param name="cmdString"></param>
        /// <returns></returns>
        public static string SimpleCMD(string cmdString)
        {
            var command = "/c " + cmdString;
            var cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = command;
            cmdsi.RedirectStandardOutput = true;
            cmdsi.UseShellExecute = false;
            cmdsi.CreateNoWindow = true;
            var cmd = Process.Start(cmdsi);
            var output = cmd.StandardOutput.ReadToEnd();

            cmd.WaitForExit();

            output = (new Regex("[ ]{2,}", RegexOptions.None)).Replace(output, " "); //Remove Double Spaces
            return output;
        }

        public static Icon FindAppIcon()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Image", "Icon.ico");
            return new Icon(path);
        }

        public static string FindExe()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TopNotify.exe");
        }
    }
}
