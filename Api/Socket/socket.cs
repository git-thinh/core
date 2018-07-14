using System;
using System.Collections.Generic;
using System.Configuration;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;
using System.Net;
using core.Db;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Web;

namespace core.Api
{
    public enum wsType
    {
        NONE = 0,
        DB_INIT = 10,
        DB_SELECT = 11,
        DB_RESULT = 12,
        DB_UPDATE_FILE = 13,
        USER_LOGIN = 20,
        USER_LOG = 21,
        USER_CHAT = 22,
        BOT_CRAWL = 30,
        BOT_SEARCH = 31
    }

    [Serializable]
    public class wsData
    {
        public wsData(wsType type, string data)
        {
            Type = type;
            Data = data;
        }

        public wsData(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 2)
            {

            }
            else
            {
                int t = 0;
                if (int.TryParse(text.Substring(0, 2), out t))
                {
                    Type = (wsType)t;
                    Data = text.Substring(2, text.Length - 2);
                }
            }
        }

        public wsType Type { set; get; }
        public string Data { set; get; }

        public override string ToString()
        {
            return string.Format("{0}{1}", (int)Type, Data);
        }
    }


    public class socket
    {



        private static string m_SessionClientWeb_Name = "webClient";
        private static WebSocketSession m_SessionClientWeb = null;
        private static SynchronizedCollection<WebSocketSession> m_Sessions = new SynchronizedCollection<WebSocketSession>() { };
        public static int port = 0;
        public static string datademo = string.Empty;
        public static IPEndPoint ipEndPoint = null;

        public static void Open()
        {
            //LogUtil.Setup();

            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            var socketServer = SocketServerManager.GetServerByName("SuperWebSocket") as WebSocketServer;
            port = socketServer.Config.Port;
            ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            socketServer.NewMessageReceived += new SessionEventHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            socketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(socketServer_SessionClosed);


            if (!SocketServerManager.Start())
                SocketServerManager.Stop();
        }

        static void socketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            string name = "null";
            if (session.Host == "localhost")
            {

            }
            else
            {
                name = session.Cookies["name"] == null ? "NameNull" : session.Cookies["name"];
                SendToAll(name + ": " + e);
            }
        }

        static void socketServer_NewSessionConnected(WebSocketSession session)
        {
            string name = "null", data = string.Empty;
            int ifor = 0;
            int.TryParse(session.Host[0].ToString(), out ifor);
            msgFor mfor = (msgFor)ifor;
            switch (mfor)
            {
                case msgFor.API:
                case msgFor.EVENT:
                    data = session.Origin;
                    session.Close();
                    db.process_MSG(mfor, data);
                    break;
                default:
                    m_Sessions.Add(session);
                    name = session.Cookies["name"] == null ? Guid.NewGuid().ToString().Substring(0, 8) : session.Cookies["name"];
                    //SendToAll(name + ": " + e);
                    SendToAll("System: " + name + " connected");
                    break;
            }
        }

        static void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            string name = "null";
            if (session.Host == "localhost")
            {
                name = m_SessionClientWeb_Name;
                m_SessionClientWeb = session;
            }
            else
            {
                name = session.Cookies["name"] == null ? Guid.NewGuid().ToString().Substring(0, 8) : session.Cookies["name"];
                m_Sessions.Remove(session);

                if (reason == CloseReason.ServerShutdown)
                    return;

                SendToAll("System: " + name + " disconnected");
            }
        }


        static void SendToAll(string message)
        {
            foreach (var s in m_Sessions)
            {
                //s.SendResponse(message);
                s.SendResponseAsync(message);
            }
        }

        //============================================================================================================

        //private static IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, 2011);
        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                //Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //public static void Send(wsData m)
        //{
        //    Send(m.ToString());
        //}

        public static void Send(decimal msg_id, string sessionid, bool isBase64 = false)
        {
            //new Thread(new ThreadStart(() =>
            //{

            using (Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                c.BeginConnect(ipEndPoint, new AsyncCallback(ConnectCallback), c);
                connectDone.WaitOne();

                string key = DateTime.Now.ToString("yyMMddHHmmssfff");
                string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(key + guid));
                string _Sec_WebSocket_Key = Convert.ToBase64String(hash);

                string base64 = msg_id.ToString() + sessionid;
                //if (isBase64 == false)
                //{
                //    //string data = Guid.NewGuid().ToString();
                //    var bytes = Encoding.UTF8.GetBytes(base64);
                //    base64 = Convert.ToBase64String(bytes);
                //}

                msgFor mfor = msg.msgFor_GET(msg_id);

                //var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
                var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: " + _Sec_WebSocket_Key + "\r\nHost: " + ((int)mfor).ToString() + "localhost\r\nOrigin: " + base64 + "\r\n\r\n";
                var byteData = Encoding.UTF8.GetBytes(handshake);

                c.Send(byteData, SocketFlags.Partial);

                if (c.Connected)
                {
                    c.Shutdown(SocketShutdown.Both);
                    c.Close();
                }
            }

            //})).Start();

        }

        public static void response_Port(HttpContext Context)
        {
            Context.Response.ContentType = "text/plain";
            Context.Response.Write(socket.port.ToString());
            Context.Response.End();
        }

    }
}