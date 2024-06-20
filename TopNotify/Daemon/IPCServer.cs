using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using Logger = WebFramework.Backend.Logger;

namespace TopNotify.Daemon
{
    public class IPCServer
    {
        private WebSocketServer SocketServer;


        public IPCServer()
        {
            SocketServer = new WebSocketServer("ws://127.0.0.1:27631");
            SocketServer.AddWebSocketService<IPCHandler>("/ipc");
        }

        public void Start()
        {
            if (SocketServer != null)
            {
                Logger.LogInfo("Starting Daemon WebSocket Server");
                SocketServer.Start();
            }
        }

        public void Stop()
        {
            if (SocketServer != null)
            {
                Logger.LogInfo("Stopping Daemon WebSocket Server");
                SocketServer.Stop();
            }
        }
    }

    public class IPCHandler : WebSocketBehavior 
    {
        protected override void OnOpen()
        {
            Logger.LogInfo("Connected!");

            base.OnOpen();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                Logger.LogInfo("Got Message: " + e.Data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }
    }

}
