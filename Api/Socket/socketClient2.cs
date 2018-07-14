//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Security.Cryptography;
//using System.IO;

//namespace core.Api
//{
//    public class socketClient2
//    {
//        private const int port = 2011;
//        private static readonly string _serverUrl = "ws://localhost:2011/";

//        public static void Send(string data)
//        {
//            //string url = "ws://localhost:2011/sample";
//            //System.Net.Configuration.HttpWebRequestElement wr = new System.Net.Configuration.HttpWebRequestElement();
//            //wr.UseUnsafeHeaderParsing = true;
//            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url); 

//            //CookieContainer cookieContainer = new CookieContainer();
//            //request.CookieContainer = cookieContainer;
//            ////request.CookieContainer.Add(cookieContainer.GetCookies(new Uri(url)));
//            //request.CookieContainer.Add(new Cookie("", "", "sample", "localhost:2011"));

//            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();


//            using (TcpClient client = new TcpClient("127.0.0.1", port))
//            using (var stream = client.GetStream())
//            { 
//                using (StreamWriter writer = new StreamWriter(stream))
//                using (StreamReader reader = new StreamReader(stream))
//                {
//                    writer.AutoFlush = true;

//                    string readed = "";
//                    string key = "";// DateTime.Now.ToString("yyMMddHHmmssfff");
//                    //while (!string.IsNullOrEmpty(readed = reader.ReadLine()))
//                    //{
//                    //    if (readed.Length > 20 && readed.Substring(0, 19) == "Sec-WebSocket-Key: ")
//                    //    {
//                    //        key = readed.Substring(19);
//                    //    }
//                    //}

//                    // Wait for your signal here

//                    var bytes = Encoding.UTF8.GetBytes(data);
//                    var base64 = Convert.ToBase64String(bytes);

//                    string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
//                    SHA1 sha = new SHA1CryptoServiceProvider();
//                    byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(key + guid));
//                    string acceptKey = Convert.ToBase64String(hash);

//                    //var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
//                    var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: "+ acceptKey +"\r\nHost: localhost\r\nOrigin: http://" + base64 + "\r\n\r\n";
//                    ////var byteData = Encoding.UTF8.GetBytes(handshake);
//                    ////var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
//                    writer.Write(handshake);

//                    string response = reader.ReadToEnd();
//                }
//            }



//            ////var listener = new TcpListener(IPAddress.Loopback, port);
//            ////listener.Start();
//            ////using (var client = listener.AcceptTcpClient())
//            ////using (var stream = client.GetStream())
//            ////{
//            ////    ////List<byte> requestList = new List<byte>() { };

//            ////    //////wait until there is data in the stream
//            ////    ////while (!stream.DataAvailable) { }

//            ////    //////read everything in the stream
//            ////    ////while (stream.DataAvailable)
//            ////    ////{
//            ////    ////    requestList.Add((byte)stream.ReadByte());
//            ////    ////}
//            ////    //////send response
//            ////    ////byte[] response = GenerateResponse(requestList.ToArray());
//            ////    ////stream.Write(response, 0, response.Length);

//            ////    var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: echo.websocket.org\r\nOrigin: http://echo.websocket.org/\r\n\r\n";
//            ////    //var handshake = "GET / HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: p2z/MFplfpRzjsVywqRQTg==\r\nHost: localhost\r\nOrigin: http://" + base64 + "\r\n\r\n";
//            ////    var byteData = Encoding.UTF8.GetBytes(handshake);
//            ////    stream.Write(byteData, 0, byteData.Length);
//            ////}
//            ////listener.Stop();
//        }

//        public static byte[] GenerateResponse(byte[] request)
//        {
//            //extract request token from end of request
//            byte[] requestToken = new byte[8];
//            Array.Copy(request, request.Length - 8, requestToken, 0, 8);

//            string requestString = Encoding.UTF8.GetString(request);
//            StringBuilder response = new StringBuilder();
//            response.Append("HTTP/1.1 101 WebSocket Protocol Handshake\r\n");
//            response.Append("Upgrade: WebSocket\r\n");
//            response.Append("Connection: Upgrade\r\n");
//            response.AppendFormat("Sec-WebSocket-Origin: {0}\r\n", GetOrigin(requestString));
//            response.AppendFormat("Sec-WebSocket-Location: {0}\r\n", _serverUrl);
//            response.Append("\r\n");

//            byte[] responseToken = GenerateResponseToken(GetKey1(requestString), GetKey2(requestString), requestToken);
//            return Encoding.UTF8.GetBytes(response.ToString()).Concat(responseToken).ToArray();
//        }

//        public static string GetOrigin(string request)
//        {
//            return Regex.Match(request, @"(?&lt;=Origin:\s).*(?=\r\n)").Value;
//        }

//        public static string GetKey1(string request)
//        {
//            return Regex.Match(request, @"(?&lt;=Sec-WebSocket-Key1:\s).*(?=\r\n)").Value;
//        }

//        public static string GetKey2(string request)
//        {
//            return Regex.Match(request, @"(?&lt;=Sec-WebSocket-Key2:\s).*(?=\r\n)").Value;
//        }

//        public static byte[] GenerateResponseToken(string key1, string key2, byte[] request_token)
//        {
//            int part1 = (int)(ExtractNums(key1) / CountSpaces(key1));
//            int part2 = (int)(ExtractNums(key2) / CountSpaces(key2));
//            byte[] key1CalcBytes = ReverseBytes(BitConverter.GetBytes(part1));
//            byte[] key2CalcBytes = ReverseBytes(BitConverter.GetBytes(part2));
//            byte[] sum = key1CalcBytes
//                        .Concat(key2CalcBytes)
//                        .Concat(request_token).ToArray();

//            return new MD5CryptoServiceProvider().ComputeHash(sum);
//        }

//        public static int CountSpaces(string key)
//        {
//            return key.Count(c => c == ' ');
//        }

//        public static long ExtractNums(string key)
//        {
//            char[] nums = key.Where(c => Char.IsNumber(c)).ToArray();
//            return long.Parse(new String(nums));
//        }

//        //converts to big endian
//        private static byte[] ReverseBytes(byte[] inArray)
//        {
//            byte temp;
//            int highCtr = inArray.Length - 1;

//            for (int ctr = 0; ctr < inArray.Length / 2; ctr++)
//            {
//                temp = inArray[ctr];
//                inArray[ctr] = inArray[highCtr];
//                inArray[highCtr] = temp;
//                highCtr -= 1;
//            }
//            return inArray;
//        }
//    }
//}
