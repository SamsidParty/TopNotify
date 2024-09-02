using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Daemon
{
    [Serializable]
    public class DaemonError
    {
        public string ID = "generic_error";
        public string Text = "Something Went Wrong With TopNotify";

        public DaemonError(string id, string text) 
        { 
            this.ID = id;
            this.Text = text;
        }
    }
}
