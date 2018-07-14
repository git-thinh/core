using core.Db;
using core.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;

namespace core.Api
{

    public class msgSSE
    {

        public static void Connect(HttpContext Context)
        {
            HttpResponse view = Context.Response;
            HttpRequest get = Context.Request;
            view.ContentType = "text/event-stream";
            view.Charset = Encoding.UTF8.WebName;

            string _sessionid = get.Cookies["sessionid"] == null ? string.Empty : get.Cookies["sessionid"].Value;
            decimal sid = 0;
            decimal.TryParse(_sessionid, out sid);

            do
            {
                if (view.IsClientConnected)
                {
                    string m = dbMSG.get_MsgInQueue(sid, msg.PingNone);  
                    view.Write(m);
                    view.Flush();
                }

                Thread.Sleep(50);
            } while (true);

        }

        /*
        static SynchronizedCollection<string> lsUserKey = new SynchronizedCollection<string>() { };
        static SynchronizedDictionary<long, string> lsMsgAction = new SynchronizedDictionary<long, string>() { };

        public static string current_Msg_nologin = string.Empty;

        public static void test_Create_Msg_nologin()
        {
            new Thread(new ThreadStart(() =>
            {
                current_Msg_nologin = "Bảng hàng Cocobay đã thay đổi: " + Guid.NewGuid().ToString() + DateTime.Now.ToString(" HH:mm:ss dd-MM-yyyy");
                Thread.Sleep(1000);
            })).Start();
        }
        public static void Connect2(HttpContext Context)
        {
            //HttpResponse view = Context.Response;
            //HttpRequest get = Context.Request;
            //view.ContentType = "text/event-stream";
            //string _user = string.Empty, _pass = string.Empty;
            //string _sessionid = get.Cookies["sessionid"] == null ? string.Empty : get.Cookies["sessionid"].Value;
            //if (string.IsNullOrEmpty(_sessionid))
            //{
            //    var m = msgData.create(msgType.LOGIN_POPUP, "Vui lòng đăng nhập hệ thống");
            //    view.Write(m.MSG);
            //    view.End();
            //}
            //else
            //{
            //    do
            //    {
            //        int timeOut = 200;

            //        string data = msgData.Ping;
            //        bool push = false;
            //        try
            //        {
            //            string _userkey = get.Cookies["userkey"] == null ? string.Empty : get.Cookies["userkey"].Value;
            //            if (string.IsNullOrEmpty(_userkey) || _userkey.Length < 100)
            //            {
            //                var m = msgData.create(msgType.LOGIN_POPUP, "Vui lòng đăng nhập hệ thống");
            //                view.Write(m.MSG);
            //                timeOut = 1000;
            //                view.Flush();
            //            }
            //            else
            //            {
            //                _user = get_Username(_userkey);
            //                _pass = get_UserPass(_userkey);

            //                if (lsUserKey.IndexOf(_userkey) == -1)
            //                {
            //                    var m = msgData.create(msgType.LOGIN_POPUP, "Vui lòng đăng nhập hệ thống");
            //                    view.Write(m.MSG);
            //                    timeOut = 1000;

            //                    string key_msg_nologin = string.Format("{0}.{1}.msg", _const.user_nologin, _sessionid);
            //                    string ms = cacheHashtable.Get<string>(key_msg_nologin);
            //                    if (!string.IsNullOrEmpty(ms) && current_Msg_nologin != ms)
            //                    {
            //                        cacheHashtable.Set(key_msg_nologin, current_Msg_nologin);
            //                        var mo = msgData.create(msgType.MSG_NOLOGIN, current_Msg_nologin);
            //                        view.Write(mo.MSG);
            //                    }
            //                    view.Flush();
            //                    //break;
            //                }
            //                else
            //                {
            //                    string m = cacheHashtable.Get<string>(_user);
            //                    if (!string.IsNullOrEmpty(m))
            //                    {
            //                        data = m;
            //                        push = true;
            //                        cacheHashtable.Set(_user, string.Empty);
            //                        timeOut = 100;
            //                        view.Write(data);
            //                        view.Flush();
            //                    }
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            if (push)
            //                cacheHashtable.Set(_user, string.Empty);
            //        }

            //        System.Threading.Thread.Sleep(100);
            //    } while (true);
            //}
        }

        private static string get_Username(string userkey)
        {
            return userkey.Substring(0, 99).Replace("$", string.Empty);
        }

        private static string get_UserPass(string userkey)
        {
            return db.Decrypt(userkey.Substring(99, userkey.Length - 99));
        }
        */
    } //end class
}// end namespace
