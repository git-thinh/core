//using api.Interface;

using core.Db;
using core.Model;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Web;
using System.Linq;
using System.Web.Hosting;
using System.IO;

namespace core.Api
{
    public class Global : System.Web.HttpApplication
    {
        static int m_push_Total = 0; 
        static SynchronizedQuereString cache_Push = new SynchronizedQuereString();

        public static void msg_Add(string msg)
        {
            if (msg.Length > 51)
            {
                m_push_Total = cache_Push.Update(msg);
            }
        }
        public static string msg_Get()
        {
            if(m_push_Total > 0)
                return cache_Push.get_Item_Last_And_Remove();
            return string.Empty;
        }

        public override void Init()
        {
            this.BeginRequest += page.OnBeginRequest;
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            socket.Open();
            string path = Server.MapPath("~/");

            db.Init(path);
            //db.job_UpdateAll_FileLOG();

            dbVidew.Init(path);

            Thread thread = new Thread(new ThreadStart(ThreadFunc));
            thread.IsBackground = true;
            thread.Name = "ThreadFunc";
            thread.Start();
        }

        protected void ThreadFunc()
        {
            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += new System.Timers.ElapsedEventHandler(TimerWorker);
            t.Interval = 10;
            t.Enabled = true;
            t.AutoReset = true;
            t.Start();
        }

        protected void TimerWorker(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_push_Total > 0)
            {
                //string m = cache_Push.get_Item_Last_And_Remove();
                //if (!string.IsNullOrEmpty(m))
                //{
                //    //socket.Send(m.Key, "", true);//work args

                //    string textFilePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "log.txt");
                //    StreamWriter file2 = new StreamWriter(textFilePath, true); 
                //    file2.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " --> " + m);
                //    file2.Close();
                //}
            }
        }

        void Application_End(object sender, EventArgs e)
        {
            SocketServerManager.Stop();
        }

        void Session_Start(object sender, EventArgs e)
        {
        }

        void Session_End(object sender, EventArgs e)
        {
        }


    }
}