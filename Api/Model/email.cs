using System;
using System.Collections.Generic;
using System.Text;

namespace core.Model
{

    [Serializable]
    public class omEmail : oItem
    {
        [colAttr(_order = 10, caption = "Họ và tên", frozen = true, resizable = true, size = "250px", sortable = false, attr = "")]
        public string fullname { set; get; }

        [colAttr(_order = 11, caption = "Điện thoại", frozen = true, resizable = true, size = "90px", sortable = false, attr = "")]
        public string phone { set; get; }

        [colAttr(_order = 20, caption = "Email", frozen = true, resizable = true, size = "280px", sortable = false, attr = "")]
        public string email { set; get; }
    }

    [Serializable]
    public class omEmail_temp : oItem
    {
        public int viewid { set; get; }

        [colAttr(_order = 10, caption = "Tiêu đề", frozen = true, resizable = true, size = "250px", sortable = false, attr = "")]
        public string title { set; get; }

        [colAttr(_order = 11, caption = "Liên kết", frozen = false, resizable = true, size = "280px", sortable = false, attr = "")]
        public string link { set; get; }

        [colAttr(_order = 12, caption = "Hình ảnh", frozen = false, resizable = true, size = "280px", sortable = false, attr = "")]
        public string imgs { set; get; }

        [colAttr(_order = 12, caption = "Chữ ký", frozen = false, resizable = true, size = "90px", sortable = false, attr = "")]
        public string signature { set; get; }

        [colAttr(_order = 20, caption = "Khung HTML", frozen = false, resizable = true, size = "280px", sortable = false, attr = "")]
        public string html { set; get; }

        [colAttr(_order = 20, caption = "Lệnh giao diện CSS", frozen = false, resizable = true, size = "280px", sortable = false, attr = "")]
        public string css { set; get; }
    }

    [Serializable]
    public class omEmail_account : oItem
    {
        [colAttr(_order = 10, caption = "Email", frozen = true, resizable = true, size = "250px", sortable = false, attr = "")]
        public string email { set; get; }

        [colAttr(_order = 11, caption = "Mật khẩu", frozen = false, resizable = true, size = "180px", sortable = false, attr = "")]
        public string password { set; get; }

        [colAttr(_order = 20, caption = "Facebook", frozen = false, resizable = true, size = "80px", sortable = false, attr = "", editableType = typeof(editable_CheckBox))]
        public bool facebook { set; get; }

        [colAttr(_order = 30, caption = "Phone", frozen = false, resizable = true, size = "80px", sortable = false, attr = "")]
        public string phone { set; get; }

        [colAttr(_order = 40, caption = "Email khôi phục", frozen = false, resizable = true, size = "280px", sortable = false, attr = "")]
        public string emailrestore { set; get; }
    }

    [Serializable]
    public class omEmail_report_send : oItem
    {
        [colAttr(_order = 10, caption = "Mã kênh", frozen = true, resizable = true, size = "80px", sortable = false, attr = "")]
        public int viewid { set; get; }

        [colAttr(_order = 10, caption = "Tiêu đề", frozen = true, resizable = true, size = "80px", sortable = false, attr = "")]
        public string title { set; get; }

        [colAttr(_order = 20, caption = "Email", frozen = false, resizable = true, size = "250px", sortable = false, attr = "")]
        public string email { set; get; }

        [colAttr(_order = 30, caption = "Ngày gửi", frozen = false, resizable = true, size = "180px", sortable = false, attr = "")]
        public string date { set; get; }

        [colAttr(_order = 50, caption = "Số lượng gửi", frozen = false, resizable = true, size = "80px", sortable = false, attr = "", editableType = typeof(editable_CheckBox))]
        public int total { set; get; } 
    }
}
