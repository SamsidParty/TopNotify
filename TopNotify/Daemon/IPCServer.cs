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
using SamsidParty_TopNotify.Daemon;

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

        //Sends The Settings File To All Clients
        //Called When The Settings File Changes
        public void UpdateSettingsFile()
        {
            if (SocketServer.IsListening)
            {
                var requestData = new byte[] { (byte)IPCPacketType.FulfillConfigRequest };
                requestData = requestData.Concat(Encoding.UTF8.GetBytes(Settings.GetForIPC())).ToArray();
                SocketServer.WebSocketServices["/ipc"].Sessions.Broadcast(requestData);
            }
        }

        //Requests Window Handles From All Clients
        public void UpdateHandles()
        {
            if (SocketServer.IsListening)
            {
                var requestData = new byte[] { (byte)IPCPacketType.RequestHandle };
                SocketServer.WebSocketServices["/ipc"].Sessions.Broadcast(requestData);
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
                    Logger.LogInfo("Sending FulfillConfigRequest Packet To Client");
                    var requestData = new byte[] { (byte)IPCPacketType.FulfillConfigRequest };
                    requestData = requestData.Concat(Encoding.UTF8.GetBytes(Settings.GetForIPC())).ToArray();
                    Send(requestData);
                }
                else if (type == IPCPacketType.FulfillHandleRequest)
                {
                    var handle = new IntPtr(int.Parse(Encoding.UTF8.GetString(packet)));
                    ExtendedStyleManager.AnonymousUpdate(handle, 00280024);
                }
                else if (type == IPCPacketType.UpdateConfig)
                {
                    InterceptorManager.Instance.OnSettingsChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }
    }

}
