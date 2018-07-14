//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace core.Api
//{

//    public class msgData
//    {
//        public msgData(msgType type, string msg)
//        {
//            TYPE = type;
//            MSG = msg;
//            decimal id = 0;
//            if (decimal.TryParse(DateTime.Now.ToString("yyyyMMddHHmmssfff000"), out id))
//                id += new Random().Next(1, 999);
//            ID = id;
//        }

//        public decimal ID { set; get; }
//        public msgType TYPE { set; get; }
//        public string MSG { set; get; }

//        public static string PingNone
//        {
//            get
//            {
//                return "data:";
//            }
//        }
//        public static string PingTime
//        {
//            get
//            {
//                return "data:";
//            }
//        }

//        //public static msgData create_PING( )
//        //{
//        //    return new msgData(msgType.NONE, "data:"); 
//        //}

//        //public static msgData create(msgType type, string data)
//        //{
//        //    if (type == msgType.NONE) return new msgData(type, "data:");
//        //    string t = buildType(type);
//        //    string msg = string.Format("data:{0}{1}\n\n", t, data);
//        //    return new msgData(type, msg);
//        //}

//        private static string buildType(msgType type)
//        {
//            string s = type.ToString();
//            int len = s.Length, max = 99;
//            for (int k = 0; k < (max - len); k++)
//                s += "$";
//            return s;
//        }
//    }
//}
