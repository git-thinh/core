using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace core.Db
{
    public class dbMSG
    {
        static SynchronizedDictionary<decimal, long> dicSessionID_Login_OK = new SynchronizedDictionary<decimal, long>() { };
        static SynchronizedDictionaryList<decimal, string> dicSessionID_MSG = new SynchronizedDictionaryList<decimal, string>() { };

        public static bool check_SessionID_Login_OK(string sessionid)
        {
            decimal sid = 0;
            if (decimal.TryParse(sessionid, out sid))
                return check_SessionID_Login_OK(sid);
            return false;
        }

        public static bool check_SessionID_Login_OK(decimal sessionid)
        {
            long idt = 0;
            if (dicSessionID_Login_OK.TryGetValue(sessionid, out idt))
            {
                var d1 = DateTime.ParseExact(idt.ToString(), "yyMMddHHmm00", CultureInfo.InvariantCulture);
                int mi = (int)(DateTime.Now - d1).TotalMinutes;
                if (mi < 60)
                    return true;
            }
            return false;
        }

        public static void set_SessionID_Login_OK(string sessionid)
        {
            long d = long.Parse(DateTime.Now.ToString("yyMMddHHmm00"));
            decimal sid = 0;
            if (decimal.TryParse(sessionid, out sid))
                dicSessionID_Login_OK.Add(sid, d);
        }



        public static string get_MsgInQueue(decimal sessionid, string v_default)
        {
            string s = dicSessionID_MSG.GetAndRemoveItemLast(sessionid, v_default);
            if (string.IsNullOrEmpty(s))
                s = v_default;
            //else
            //    s = string.Format("data:{0}\n\n", s);
            //s = HttpUtility.UrlEncode(s);
            //s = db.EncodeTo64(s);
            //s = HttpUtility.HtmlEncode(s);
            return string.Format("data:{0}\n\n", s);
        }

        public static void set_MsgInQueue(string sessionid, string msg)
        {
            decimal sid = 0;
            if (decimal.TryParse(sessionid, out sid))
                dicSessionID_MSG.AddToTop_Item(sid, msg);
        }

        public static void set_MsgInQueue(decimal sessionid, string msg)
        {
            dicSessionID_MSG.AddToTop_Item(sessionid, msg);
        }

        public static void set_MsgInQueue_ForAllUser(string msg)
        {
            dicSessionID_MSG.AddToTop_ItemAll(msg);
        }
    }

}
