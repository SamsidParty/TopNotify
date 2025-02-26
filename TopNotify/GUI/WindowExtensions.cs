using IgniteView.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;

namespace TopNotify.GUI
{
    public static class WindowExtensions
    {
        public static void SendConfig(this WebWindow target)
        {
            target.CallFunction("SetConfig", Settings.GetForIPC());
        }
    }
}
