using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using WebFramework;

namespace TopNotify.GUI
{
    /// <summary>
    /// This Class Implements Backwards Compatibility Features For Windows 10
    /// </summary>
    public class Windows10Support : WebScript
    {
        public override async Task DOMContentLoaded()
        {
            if (Platform.isWindows11) { return; }

            // Fix Transparent Background
            Document.SetCSSProperty("html body", "background-color", "var(--col-bg) !important");
        }
    }
}
