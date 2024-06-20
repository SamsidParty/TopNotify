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
using TopNotify.Common;

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

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                var packet = e.RawData;
                var type = (IPCPacketType)e.RawData[0];
                packet = packet.Skip(1).ToArray(); // Removes The First (IPCPacketType) Byte

                Logger.LogInfo("Recieved Packet Of Type: " + type.ToString());

                if (type == IPCPacketType.RequestConfig)
                {
                    var requestData = new byte[] { (byte)IPCPacketType.FulfillConfigRequest };
                    requestData = requestData.Concat(Encoding.UTF8.GetBytes(Settings.GetRaw())).ToArray();
                    Send(requestData);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }
    }

}
