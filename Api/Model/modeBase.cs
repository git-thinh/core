using System;
using System.Collections.Generic;
using System.Text;

namespace core.Model
{

    [Serializable]
    public class oItem
    {
        [colAttr(caption = "Recid", frozen = false, resizable = true, size = "90px", sortable = false, attr = "")]
        public int recid { get; private set; }

        [colAttr(caption = "Key", frozen = false, resizable = true, size = "190px", sortable = false, attr = "")]
        public long _key { get; set; }

        [colAttr(caption = "Trạng thái", _order = 1, field = "check", frozen = true, resizable = true, size = "83px", sortable = false, attr = "", style = "text-align: center", editableType = typeof(editable_CheckBox))]
        public oItemStatus _status { set; get; }

        public void setKey(int key)
        {
            _key = key;
        }
        public void setRecid(int recid_)
        {
            recid = recid_;
        }
        public void setStatus(string val)
        {
            int k = 0;
            if (int.TryParse(val, out k))
            {
                _status = (oItemStatus)k;
            }
        }
    }
}
