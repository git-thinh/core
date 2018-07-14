//using System;
//using System.Net.Sockets;
//using System.Text;
//using System.Net;
//using System.Threading;
//using System.IO;
//using System.Web;

//namespace core.Api
//{
//    public class socketClient
//    {


























//        // The port number for the remote device.
//        private const int port = 2011;

//        // ManualResetEvent instances signal completion.
//        private static ManualResetEvent connectDone = new ManualResetEvent(false);
//        private static ManualResetEvent sendDone = new ManualResetEvent(false);
//        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

//        // The response from the remote device.
//        private static String response = String.Empty;
//        private static Socket client;
//        public static StatusSocket statusProvider = StatusSocket.Close;



//        public static void Dispose()
//        {
//            client.Close();
//        }

//        public static void Send2(string data)
//        {
//            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, port);

//            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
//            connectDone.WaitOne();

//            var bytes = Encoding.UTF8.GetBytes(data);
//            var base64 = Convert.ToBase64String(bytes);

//            //var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
//            var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: localhost\r\nOrigin: http://" + base64 + "\r\n\r\n";
//            var byteData = Encoding.UTF8.GetBytes(handshake);
//            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
//        }

//        public static void Send(string data)
//        {
//            if (statusProvider == StatusSocket.Connected || statusProvider == StatusSocket.Opened)
//            {
//                // Convert the string data to byte data using ASCII encoding.
//                //byte[] byteData = Encoding.ASCII.GetBytes(data);

//                // Send HS
//                //var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
//                var byteData = Encoding.UTF8.GetBytes(data);

//                // Begin sending the data to the remote device.
//                //client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
//                client.Send(byteData);
//            }
//            else
//            {
//                Open();
//                Send(data);
//            }
//        }

//        public static void SendCookie(string data)
//        {
//            if (statusProvider == StatusSocket.Connected)
//            {
//                string url = "ws://localhost:2011/sample";
//                System.Net.Configuration.HttpWebRequestElement wr = new System.Net.Configuration.HttpWebRequestElement();
//                wr.UseUnsafeHeaderParsing = true;
//                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

//                HttpCookie myCookie = new HttpCookie("name");
//                myCookie.Value = "web";
//                myCookie.Expires = DateTime.Now.AddMinutes(180);
//                //res.Cookies.Add(myCookie);

//                CookieContainer cookieContainer = new CookieContainer();
//                request.CookieContainer = cookieContainer;
//                //request.CookieContainer.Add(cookieContainer.GetCookies(new Uri(url)));
//                request.CookieContainer.Add(new Cookie("", "", "sample", "localhost:2011"));

//                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//                Stream responsestream = response.GetResponseStream();
//                byte[] buffer = new byte[StateObject.BufferSize];
//                int rebu = responsestream.Read(buffer, 0, StateObject.BufferSize);
//                while (rebu != 0)
//                {
//                    client.Send(buffer, rebu, 0);
//                    rebu = responsestream.Read(buffer, 0, StateObject.BufferSize);
//                }

//                //// Convert the string data to byte data using ASCII encoding.
//                ////byte[] byteData = Encoding.ASCII.GetBytes(data);

//                //// Send HS
//                ////var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
//                //var byteData = Encoding.UTF8.GetBytes(data);

//                //// Begin sending the data to the remote device.
//                //client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client); 
//            }
//        }

//        public static void Open()
//        {
//            //Thread.Sleep(3000);
//            // Connect to a remote device.
//            try
//            {
//                // Establish the remote endpoint for the socket.
//                // The name of the 
//                // remote device is "host.contoso.com".
//                IPHostEntry ipHostInfo = Dns.Resolve("127.0.0.1");
//                IPAddress ipAddress = ipHostInfo.AddressList[0];
//                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

//                // Create a TCP/IP socket.
//                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//                // Connect to the remote endpoint.
//                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
//                connectDone.WaitOne();

//                // Send test data to the remote device.
//                //var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
//                var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: localhost\r\nOrigin: http://localhost/\r\n\r\n";
//                Send(client, handshake);
//                sendDone.WaitOne();

//                statusProvider = StatusSocket.Opened;

//                // Receive the response from the remote device.
//                Receive(client);
//                //receiveDone.WaitOne();

//                // Write the response to the console.
//                ////Console.WriteLine("Response received : {0}", response);

//                ////// Release the socket.
//                ////client.Shutdown(SocketShutdown.Both);
//                ////client.Close();
//                //while (true)
//                //{
//                //    Send(Guid.NewGuid().ToString());
//                //    Thread.Sleep(5000);
//                //}
//            }
//            catch (Exception e)
//            {
//                ////Console.WriteLine(e.ToString());
//            }
//        }

//        private static void ConnectCallback(IAsyncResult ar)
//        {
//            try
//            {
//                // Retrieve the socket from the state object.
//                Socket client = (Socket)ar.AsyncState;

//                // Complete the connection.
//                client.EndConnect(ar);

//                //Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

//                // Signal that the connection has been made.
//                connectDone.Set();
//                statusProvider = StatusSocket.Connected;

//            }
//            catch (Exception e)
//            {
//                //Console.WriteLine(e.ToString());
//            }
//        }

//        private static void Receive(Socket client)
//        {
//            try
//            {
//                // Create the state object.
//                StateObject state = new StateObject();
//                state.workSocket = client;

//                // Begin receiving the data from the remote device.
//                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
//            }
//            catch (Exception e)
//            {
//                //Console.WriteLine(e.ToString());
//            }
//        }

//        // Copyright (c) 2008-2013 Hafthor Stefansson
//        // Distributed under the MIT/X11 software license
//        // Ref: http://www.opensource.org/licenses/mit-license.php.
//        static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
//        {
//            if (a1 == null || a2 == null || a1.Length != a2.Length)
//                return false;
//            fixed (byte* p1 = a1, p2 = a2)
//            {
//                byte* x1 = p1, x2 = p2;
//                int l = a1.Length;
//                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
//                    if (*((long*)x1) != *((long*)x2)) return false;
//                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
//                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
//                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
//                return true;
//            }
//        }

//        static byte[] cache = new byte[StateObject.BufferSize];

//        private static void ReceiveCallback(IAsyncResult ar)
//        {
//            try
//            {
//                // Retrieve the state object and the client socket 
//                // from the asynchronous state object.
//                StateObject state = (StateObject)ar.AsyncState;
//                Socket client = state.workSocket;

//                //////// Read data from the remote device.
//                //////int bytesRead = client.EndReceive(ar);

//                //////if (bytesRead > 0)
//                //////{
//                //////    // There might be more data, so store the data received so far.
//                //////    string s = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
//                //////    state.sb.Append(s);

//                //////    //Console.WriteLine(s);

//                //////    // Get the rest of the data.
//                //////    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
//                //////}
//                //////else
//                //////{
//                //////    // All the data has arrived; put it in response.
//                //////    if (state.sb.Length > 1)
//                //////    {
//                //////        response = state.sb.ToString();
//                //////        //Console.WriteLine(response);
//                //////    }
//                //////    // Signal that all bytes have been received.
//                //////    receiveDone.Set();
//                //////}


//                // Read data from the remote device.
//                int bytesRead = client.EndReceive(ar);

//                while (bytesRead > 0)
//                {
//                    if (!UnsafeCompare(cache, state.buffer))
//                    {
//                        Array.Copy(state.buffer, cache, state.buffer.Length);

//                        // There might be more data, so store the data received so far.
//                        string s = Encoding.UTF8.GetString(cache, 0, bytesRead);
//                        if (s.StartsWith("�\u001a")) continue;

//                        //System.Diagnostics.Debug.WriteLine(s);

//                        //Console.WriteLine("=> Receive: " + s + "\n");

//                        //receiveDone.Set();

//                        // Get the rest of the data.
//                        //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
//                    }
//                    else
//                    {
//                        Thread.Sleep(100);
//                        ////Console.WriteLine("=> waiting ... ");
//                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
//                    }
//                }




//            }
//            catch (Exception e)
//            {
//                //Console.WriteLine(e.ToString());
//            }
//        }

//        private static void Send(Socket client, String data)
//        {
//            // Convert the string data to byte data using ASCII encoding.
//            //byte[] byteData = Encoding.ASCII.GetBytes(data);

//            // Send HS
//            //var data = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
//            var byteData = Encoding.UTF8.GetBytes(data);

//            // Begin sending the data to the remote device.
//            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
//        }

//        private static void SendCallback(IAsyncResult ar)
//        {
//            try
//            {
//                // Retrieve the socket from the state object.
//                Socket client = (Socket)ar.AsyncState;

//                // Complete sending the data to the remote device.
//                int bytesSent = client.EndSend(ar);
//                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

//                // Signal that all bytes have been sent.
//                sendDone.Set();
//            }
//            catch (Exception e)
//            {
//                //Console.WriteLine(e.ToString());
//            }
//        }

//        //public static void Main(String[] args)
//        //{
//        //    StartClient();

//        //    Console.ReadLine();
//        //}
//    }
//}
