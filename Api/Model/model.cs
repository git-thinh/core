using core.Db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace core.Model
{

    [Serializable]
    public class omTab : oItem
    {
        //{ field: 'name_ascii', caption: 'Từ khóa không dấu', size: '290px', sortable: true, resizable: true, frozen: true },

        public string name { set; get; }
        public bool show_navi { set; get; }
        public bool grid_filter_project { set; get; }
        public bool grid_filter_category { set; get; }
    }



    //=========================================================
    [Serializable]
    public class omApi_model : oItem
    {
        [colAttr(_order = 10, caption = "Model", frozen = true, resizable = true, size = "250px", sortable = false, attr = "")]
        public string model { set; get; }

        [colAttr(_order = 20, caption = "API", frozen = true, resizable = true, size = "250px", sortable = false, attr = "")]
        public string api { set; get; }

        [colAttr(_order = 30, caption = "Description", frozen = false, resizable = true, size = "250px", sortable = false, attr = "")]
        public string description { set; get; }
    }


    [Serializable]
    public class omAction : oItem
    {
        public string _viewid { set; get; }
        public string _sessionid { set; get; }
        public string _username { set; get; }
        public string _tab { set; get; }
        public string _type { set; get; }
        public string _model { set; get; }
        public int position { set; get; } // 0: page; 1: popup


        [colAttr(_order = 10, caption = "Mã", frozen = true, resizable = true, size = "250px", sortable = false, attr = "")]
        public string id { set; get; }

        [colAttr(_order = 11, caption = "Tham chiếu", frozen = true, resizable = true, size = "250px", sortable = false, attr = "")]
        public string alias { set; get; }

        [colAttr(_order = 20, caption = "Loại", frozen = true, resizable = true, size = "80px", sortable = false, attr = "")]
        public string type { set; get; }

        [colAttr(_order = 30, caption = "Tên", frozen = true, resizable = true, size = "250px", sortable = false, attr = "")]
        public string caption { set; get; }

        [colAttr(_order = 40, caption = "Biểu tượng", frozen = false, resizable = true, size = "150px", sortable = false, attr = "")]
        public string icon { set; get; }

        [colAttr(_order = 50, caption = "Tag", frozen = false, resizable = true, size = "350px", sortable = false, attr = "")]
        public string tag { set; get; }

        [colAttr(_order = 60, caption = "Mô tả", frozen = false, resizable = true, size = "350px", sortable = false, attr = "")]
        public string description { set; get; }

        [colAttr(_order = 70, caption = "HTML", frozen = false, resizable = true, size = "350px", sortable = false, attr = "")]
        public string html { set; get; }

        [colAttr(_order = 70, caption = "Lệnh", frozen = false, resizable = true, size = "350px", sortable = false, attr = "")]
        public string command { set; get; }

        public omAction[] items { set; get; }
    }

    [Serializable]
    public class omProduct : oItem
    {
        public string key { set; get; }
        public string name_ascii { set; get; }
        public int level { set; get; }

        [colAttr(caption = "Từ khóa")]
        public string name_utf8 { set; get; }
        public string tag { set; get; }
        public string content_count { set; get; }
        public int gseo_position { set; get; }
        public string gseo_site { set; get; }
        public int gads_position { set; get; }
        public string gads_site { set; get; }
        public int adword_bid { set; get; }
        public string adword_site { set; get; }
        public int adword_click { set; get; }
        public int adword_view { set; get; }
        public int adword_click_competitor { set; get; }
        public int time_check { set; get; }
    }

    [Serializable]
    public class omKeyword : oItem
    {
        public string key { set; get; }
        public string name_ascii { set; get; }
        public int level { set; get; }

        [colAttr(caption = "Từ khóa")]
        public string name_utf8 { set; get; }
        public string tag { set; get; }
        public string content_count { set; get; }
        public int gseo_position { set; get; }
        public string gseo_site { set; get; }
        public int gads_position { set; get; }
        public string gads_site { set; get; }
        public int adword_bid { set; get; }
        public string adword_site { set; get; }
        public int adword_click { set; get; }
        public int adword_view { set; get; }
        public int adword_click_competitor { set; get; }
        public int time_check { set; get; }
    }


    [Serializable]
    public class omDns : oItem
    {
        public string host { set; get; }
        public int viewid { set; get; }
    }

    [Serializable]
    public class omDnsTemplate : oItem
    {
        public string path { set; get; }
        public string template { set; get; }
    }

    [Serializable]
    public class omApiLog : oItem
    {
        public decimal msg_id { set; get; }
        public string domain { set; get; }
        public decimal sessionid { set; get; }
        public int viewID { set; get; }
        public string Url { set; get; }
        public long TimeBegin { set; get; }
        public long TimeEnd { set; get; }
        public string Data { set; get; }

        public static void Create(int viewid, string sessionid, decimal msg_id, Uri uri, string data)
        {
            if (!string.IsNullOrEmpty(sessionid) && uri != null)
            {
                decimal sid = 0;
                if (decimal.TryParse(sessionid, out sid))
                {
                    omApiLog o = new omApiLog();
                    o.sessionid = sid;
                    o.msg_id = msg_id;

                    o.domain = uri.Host;
                    o.viewID = viewid;
                    o.Url = uri.ToString();
                    o.Data = data;

                    if (viewid == 0)
                    {
                        //dbVidew.domain_GetView()

                        cacheHashtable.Set(msg_id + ".log", o);
                    }
                }
            }

        }

    }


    [Serializable]
    public class omArticle : oItem
    {
        [colAttr(_order = 10, caption = "Tiêu đề", frozen = true, resizable = true, size = "350px", sortable = false, attr = "")]
        public string title { set; get; }

        [colAttr(_order = 20, caption = "Mô tả", frozen = false, resizable = true, size = "450px", sortable = false, attr = "")]
        public string description { set; get; }

        [colAttr(_order = 30, caption = "Nội dung", frozen = false, resizable = true, size = "550px", sortable = false, attr = "", _w2ui = false)]
        public string content { set; get; }

        [colAttr(_order = 40, caption = "Ngày tạo", frozen = false, resizable = true, size = "150px", sortable = false, attr = "")]
        public int date_create { set; get; }

        [colAttr(_order = 50, caption = "Thời gian tạo", frozen = false, resizable = true, size = "150px", sortable = false, attr = "")]
        public int time_create { set; get; }

        [colAttr(_order = 60, caption = "Nguồn", frozen = false, resizable = true, size = "150px", sortable = false, attr = "")]
        public int src_id { set; get; }

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}", _const.db_charSplitColumn,
                _key, (int)_status, title, description, date_create, src_id);
        }

        public omArticle() { }

        public omArticle(string text)
        {
        }
    }

    [Serializable]
    public class omApi : oItem
    {

        public string key_resource { set; get; } //3
        public string data_type { set; get; } //3
        public string control_redirect { set; get; } //3
        public string action_redirect { set; get; } //3

        public bool relogin { set; get; } //3
        public bool OK { set; get; } //3

        public string control { set; get; } //3
        public string action { set; get; } //3

        public string title { set; get; }//4
        public string decription { set; get; }//5
        public string tag { set; get; }//6


        public string viewid { set; get; }
        public string sessionid { set; get; }
        public string msg_id { set; get; }
        public object data { set; get; }


        public string callback { set; get; }
        public string callback_process { set; get; }


        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}", _const.db_charSplitColumn,
                _key, (int)_status, control, tag, decription, title);
        }

        public omApi() { }

        public omApi(string text)
        {
            string[] a = text.Split(_const.db_charSplitColumn);
            if (a.Length > 5)
            {
                int key = 0;
                if (int.TryParse(a[0], out key))
                {
                    setKey(key);
                    setStatus(a[1]);
                    control = a[2];
                    tag = a[3];
                    decription = a[4];
                    title = a[5];
                }
            }
        }
    }


    [Serializable]
    public class omCategory : oItem
    {
        public int parentid { set; get; } //3
        public string name { set; get; }//4
        public string description { set; get; }//5
        public string keyword { set; get; }//6

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}", _const.db_charSplitColumn,
                _key, (int)_status, parentid, name, description, keyword);
        }

        public omCategory() { }

        public omCategory(string text)
        {
            string[] a = text.Split(_const.db_charSplitColumn);
            if (a.Length > 5)
            {
                int key = 0;
                if (int.TryParse(a[0], out key))
                {
                    setKey(key);
                    setStatus(a[1]);
                    parentid = int.Parse(a[2]);
                    name = a[3];
                    description = a[4];
                    keyword = a[5];
                }
            }
        }
    }

    [Serializable]
    public class omSite : oItem
    {
        public string domain { set; get; } //3
        public string title { set; get; }//4
        public string description { set; get; }//5
        public string keyword { set; get; }//6

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}", _const.db_charSplitColumn,
                _key, (int)_status, domain, title, description, keyword);
        }

        public omSite() { }

        public omSite(string text)
        {
            string[] a = text.Split(_const.db_charSplitColumn);
            if (a.Length > 5)
            {
                int key = 0;
                if (int.TryParse(a[0], out key))
                {
                    setKey(key);
                    setStatus(a[1]);
                    domain = a[2];
                    title = a[3];
                    description = a[4];
                    keyword = a[5];
                }
            }
        }
    }

    [Serializable]
    public class omCustomer : oItem
    {
        public string fullname { set; get; } //3
        public string phone { set; get; }//4
        public string email { set; get; }//5
        public string address { set; get; }//6

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}", _const.db_charSplitColumn,
                _key, (int)_status, fullname, address, email, phone);
        }

        public omCustomer() { }

        public omCustomer(string text)
        {
            string[] a = text.Split(_const.db_charSplitColumn);
            if (a.Length > 5)
            {
                int key = 0;
                if (int.TryParse(a[0], out key))
                {
                    setKey(key);
                    setStatus(a[1]);
                    fullname = a[2];
                    address = a[3];
                    email = a[4];
                    phone = a[5];
                }
            }
        }
    }

    [Serializable]
    public class omUser : oItem
    {
        public string username { set; get; } //3
        public string phone { set; get; }//4
        public string email { set; get; }//5
        public string password { set; get; }//6

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}", _const.db_charSplitColumn,
                _key, (int)_status, username, password, email, phone);
        }

        public omUser() { }

        public omUser(string text)
        {
            string[] a = text.Split(_const.db_charSplitColumn);
            if (a.Length > 5)
            {
                int key = 0;
                if (int.TryParse(a[0], out key))
                {
                    setKey(key);
                    setStatus(a[1]);
                    username = a[2];
                    password = a[3];
                    email = a[4];
                    phone = a[5];
                }
            }
        }
    }
}
