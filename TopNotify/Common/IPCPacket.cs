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
        FulfillConfigRequest, // Daemon To Client, Returns Config
        RequestHandle, // Daemon To Client, Asks For The Handle Of The Notification Window
        FulfillHandleRequest, // Client To Daemon, Tells The Interceptor The Notification Window Handle,
        UpdateConfig // GUI To Daemon, Tells The Interceptor That The Config Has Changed
    }
}
