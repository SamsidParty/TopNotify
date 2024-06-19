using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TopNotify.Daemon
{
    public class IPCServer
    {
        private WebSocketServer SocketServer;


        public IPCServer()
        {
            SocketServer = new WebSocketServer("ws://127.0.0.1:"); // TODO: Assign A Port
        }

        public void Start()
        {

        }
    }
}
