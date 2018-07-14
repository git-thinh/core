using System;
using System.Collections.Generic;
using System.Text;


namespace core.Api
{
    public enum msgFor
    {
        NONE = 0,
        API = 1,
        EVENT = 2
    }

    [Serializable]
    public class msg
    {
        public static msgFor msgFor_GET(decimal msg_id) {
            msgFor mfor = msgFor.NONE;
            int k = 0;
            int.TryParse(msg_id.ToString()[0].ToString(), out k);
            mfor = (msgFor)k;
            return mfor;
        }

        private static decimal _id_create(object vid_)
        {
            // 7 + 15 + 3 = 25
            string s = string.Format("{0}{1}{2}", vid_, DateTime.Now.ToString("yyMMddHHmmssfff"), new Random().Next(100, 999));
            decimal id = 0;
            decimal.TryParse(s, out id);
            return id;
        }

        public msg(msLAYER layer, msLAYOUT layout, msCONTROL control, msGRID grid, string data)
        {
            int vid = (int)layer + (int)layout + (int)control + (int)grid;
            ID = _id_create(vid);
            DATA = data;
        }

        public msg(string data)
        {
            ID = 0;
            DATA = data;
        }

        public decimal ID { set; get; }

        public string DATA { set; get; }

        public static string PingNone
        {
            get
            {
                return string.Format("data:{0}", string.Empty);
            }
        }

        public static string PingTime
        {
            get
            {
                return string.Format("data:{0}{1}\n\n", _id_create((int)msLAYER.PING_TIME), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss fff"));
            }
        }

        public override string ToString()
        {
            //Char none = ID.ToString()[0];
            //if(none == '1') return "data:\n\n";
            return string.Format("data:{0}{1}\n\n", ID, DATA);
        }
    }

    public enum msLAYER
    {
        NONE = 0,
        API = 1000000,
        PING_NONE = 1000001,
        PING_TIME = 1000002,
        FILE_UPLOAD = 1000003,
        FILE_UPLOAD_IMPORT = 1000004,
        API_CONFIG = 1000005,
        API_CONFIG_POUP = 1000006,
        MAIN = 2000000,
        POPUP_MAIN = 3000000,
        POPUP_SUB = 4000000
    }

    public enum msLAYOUT
    {
        NONE = 0,
        TOP = 100000,
        LEFT = 200000,
        RIGHT = 300000,
        MAIN = 400000,
        PREVIEW = 500000,
        BOTTOM = 600000,
        POPUP_MAIN = 700000,
        POPUP_SUB = 800000,
    }

    public enum msCONTROL
    {
        NONE = 0,
        TOOLBAR = 10000,
        SIDEBAR = 20000,
        TAB = 30000,
        GRID = 40000,
        FORM = 50000,
    }

    public enum msGRID
    {
        NONE = 0,
        COL = 1000,
        TOOLBAR = 2000,
        DATA = 3000
    }


    public enum msgType
    {
        // LAYOUT: TOP = 1, LEFT = 2, MAIN = 3, RIGHT = 4   * 1,000,000
        // CONTROL: TOOLBAR = 1, SIDEBAR = 2, TAB = 3, GRID = 4, FROM = 5 * 100,000
        // GRID: COL = 1, TOOLBAR = 2, DATA = 3   * 10,000


        PING = 0,
        UI_TOP_TOOLBAR = 1000000,

        UI_LEFT_TOOLBAR = 2000000,
        UI_LEFT_SIDEBAR = 2000000,


        UI_MAIN_TAB = 3000000,
        UI_MAIN_TAB_GRID = 3000000,
        UI_MAIN_TAB_GRID_COL = 3000000,
        UI_MAIN_TAB_GRID_TOOLBAR = 3000000,
        UI_MAIN_TAB_GRID_DATA = 3000000,

        UI_RIGHT_TOOLBAR = 4000000,
        UI_RIGHT_SIDEBAR = 4000000,
        UI_RIGHT_GRID = 4000000,
        UI_RIGHT_FORM = 4000000,


        LOGIN_DATA = 10,
        LOGIN_POPUP = 11,
        LOGIN_PAGE = 12,
        MSG_NOLOGIN = 20,
        MSG_NEWS = 21,
        MSG_SALE_CUSTOMER = 22,
        MSG_SYSTEM = 23,
        MSG_BOSS = 24,
        MSG_CALENDAR = 25,
        MSG_TASK_PROCESS = 26,
    }


}
