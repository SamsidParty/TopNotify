using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    public class MonitorData
    {
        /// <summary>
        /// Used By TopNotify To Identify The Monitor
        /// </summary>
        public string ID;

        /// <summary>
        /// The Path To The Monitor, Eg. \\.\DISPLAY1\...
        /// </summary>
        public string Path;

        /// <summary>
        /// The Friendly Name Of The Monitor
        /// </summary>
        public string FriendlyName;

        /// <summary>
        /// The Name Of The GPU As Reported By Windows, Eg: RADEON RX 7800 XT
        /// </summary>
        public string GraphicsDriverName;

        /// <summary>
        /// The Name Shown To The User
        /// </summary>
        public string DisplayName
        {
            get
            {
                return FriendlyName + " (" + Path + ") Connected To " + GraphicsDriverName;
            }
        }
    }
}
