using core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace core.Model
{

    public interface IeditableAttr
    {
        string style { get; }
        string type { get; }
    }
    public struct editable_TextBox : IeditableAttr
    {
        public string style { get { return "text-align: left"; } }
        public string type { get { return "text"; } }
    }

    public struct editable_CheckBox : IeditableAttr
    {
        public string style { get { return "text-align: center"; } }
        public string type { get { return "checkbox"; } }
    }

    public class editableAttr : IeditableAttr
    {
        public string style { get; set; }
        public string type { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class colAttr : Attribute
    {
        public string field { set; get; }
        public string caption { set; get; }
        public string size { set; get; }
        public string attr { set; get; }
        public bool sortable { set; get; }
        public bool resizable { set; get; }
        public bool frozen { set; get; }
        public string style { set; get; }
        public Type editableType { set; get; }
        
        //==================================================
        public bool _isAdd { set; get; }
        public bool _isEdit { set; get; }
        public bool _isEditLookUp { set; get; }

        private bool w2ui = true;
        public bool _w2ui { set { w2ui = value; } get { return w2ui; } }
        private int order = 999;
        public int _order { set { order = value; } get { return order; } }


    }

    [Serializable]
    public class omColumn : oItem
    {
        //{ field: 'name_ascii', caption: 'Từ khóa không dấu', size: '290px', sortable: true, resizable: true, frozen: true },

        public string field { set; get; }
        public string caption { set; get; }
        public string size { set; get; }
        public string attr { set; get; }
        public bool sortable { set; get; }
        public bool resizable { set; get; }
        public bool frozen { set; get; }
        public string style { set; get; }
        public IeditableAttr editable { set; get; }
        //==========================================================

        public bool _isAdd { set; get; }
        public bool _isEdit { set; get; }
        public bool _isEditLookUp { set; get; }

        public string _tabname { set; get; }
        public string _datatype { set; get; }
        public bool _w2ui { set; get; }
        public int _order { set; get; }

        public omColumn() { }
        public omColumn(colAttr c)
        {
            this.field = c.field;
            this.attr = c.attr;
            this.caption = c.caption;
            this.frozen = c.frozen;
            this.resizable = c.resizable;
            this.size = c.size;
            this.sortable = c.sortable;

            this.style = c.style;
            //this.editable = c.editable;

            this._w2ui = c._w2ui;
            this._order = c._order;

            this._isAdd = c._isAdd;
            this._isEdit = c._isEdit;
            this._isEditLookUp = c._isEditLookUp;

            if (c.field == "recid" || c.field == "_key" || c.field == "_status")
                if (c._order == 999) this._order = 99999;

            if (c.editableType == null)
                this.editable = new editable_TextBox();
            else
            {
                var o = (IeditableAttr)Activator.CreateInstance(c.editableType);
                this.editable = new editableAttr() { style = o.style, type = o.type };
            }
        }

        public override string ToString()
        {
            return string.Format("{0}; {1}; {2}; {3}; {4}", _tabname, _order, field, caption, size);
        }
    }

}
