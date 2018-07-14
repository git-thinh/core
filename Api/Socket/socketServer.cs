//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using SuperSocket.Common;
//using SuperSocket.SocketBase;
//using SuperSocket.SocketEngine;
//using SuperSocket.SocketEngine.Configuration;
//using SuperWebSocket;
//using System.Net;
//using core.Db;
//using System.Text;

//namespace core.Api
//{
//    public class socketServer
//    {
//        private static string m_SessionClientWeb_Name = "webClient";
//        private static WebSocketSession m_SessionClientWeb = null;
//        private static SynchronizedCollection<WebSocketSession> m_Sessions = new SynchronizedCollection<WebSocketSession>() { };
//        public static int port = 0;
//        public static string datademo = string.Empty;
//        public static IPEndPoint ipEndPoint = null;

//        public static void Open()
//        {
//            LogUtil.Setup();

//            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
//            if (!SocketServerManager.Initialize(serverConfig))
//                return;

//            var socketServer = SocketServerManager.GetServerByName("SuperWebSocket") as WebSocketServer;
//            port = socketServer.Config.Port;
//            ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

//            socketServer.NewMessageReceived += new SessionEventHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
//            socketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(socketServer_NewSessionConnected);
//            socketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(socketServer_SessionClosed);


//            if (!SocketServerManager.Start())
//                SocketServerManager.Stop();
//        }

//        static void socketServer_NewMessageReceived(WebSocketSession session, string e)
//        {
//            string name = "null";
//            if (session.Host == "localhost")
//            {

//            }
//            else
//            {
//                name = session.Cookies["name"] == null ? "NameNull" : session.Cookies["name"];
//                SendToAll(name + ": " + e);
//            }
//        }

//        static void socketServer_NewSessionConnected(WebSocketSession session)
//        {
//            string name = "null";
//            if (session.Host == "localhost")
//            {
//                var b = Convert.FromBase64String(session.Origin);
//                var data = Encoding.UTF8.GetString(b);
//                datademo = session.Origin;
//                session.SendResponseAsync("OK");
//                message_Process(data);
//            }
//            else
//            {
//                m_Sessions.Add(session);
//                name = session.Cookies["name"] == null ? Guid.NewGuid().ToString().Substring(0, 8) : session.Cookies["name"];
//                SendToAll(name + ": " + e);
//                SendToAll("System: " + name + " connected");
//            }
//        }

//        static void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
//        {
//            string name = "null";
//            if (session.Host == "localhost")
//            {
//                name = m_SessionClientWeb_Name;
//                m_SessionClientWeb = session;
//            }
//            else
//            {
//                name = session.Cookies["name"] == null ? Guid.NewGuid().ToString().Substring(0, 8) : session.Cookies["name"];
//                m_Sessions.Remove(session);

//                if (reason == CloseReason.ServerShutdown)
//                    return;

//                SendToAll("System: " + name + " disconnected");
//            }
//        }

//        static void message_Process(string data)
//        {
//            wsData m = new wsData(data);
//            if (m != null)
//            {
//                switch (m.Type)
//                {
//                    case wsType.DB_UPDATE_FILE:
//                        db.update_FileLOG(m.Data);
//                        break;
//                }
//            }
//        }

//        static void SendToAll(string message)
//        {
//            foreach (var s in m_Sessions)
//            {
//                s.SendResponse(message);
//                s.SendResponseAsync(message);
//            }
//        }

//    }
//}
