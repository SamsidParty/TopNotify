using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    public enum IPCPacketType : byte
    {
        RequestConfig, // Client To Daemon, Asks For Config
        FulfillConfigRequest // Daemon To Client, Returns Config
    }
}
