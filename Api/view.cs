using core.Db;
using core.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace core
{
    public class view
    {
        public static string get_config_Grid(string viewid, string folder, string file, bool isPopup = false)
        {
            string s_grid = "",
                model = file.model_Format(),
                ___model = db.m_split_controlAction + file;

            string filePath = string.Format(@"{0}page\{1}\admin\{2}\{3}.js", dbVidew.PathRoot, viewid, folder, model);
            if (File.Exists(filePath))
                s_grid = File.ReadAllText(filePath);
            else
            {
                #region

                var acol = db<omColumn>.Query(x => x._tabname == model && x._w2ui == true).OrderBy(x => x._order).ToArray();
                string jcol = JsonConvert.SerializeObject(acol, Formatting.Indented);
                if (acol.Length == 0) jcol = "[]";
                string jButton = "[]";

                var bar = new List<omAction>() { };
                var input_search = new omAction()
                {
                    type = "html",
                    id = "page_tabgridsearch_" + ___model,
                    html = @"<div style='padding: 3px 10px;'> SEARCH " +
                    @"<input size='10' placeholder='enter to search' onkeyup=""api.admin_tab_grid_search(event," +
                    @"'page_tabgrid" + ___model + @"','" + ___model + @"',this.value)"" id='search_page_tabgrid" + ___model + @"'/></div>"
                };

                if (bar.Count == 0)
                    bar.Add(input_search);
                else
                    bar.Insert(0, input_search);

                // LINK
                var alink = db.m_arrayModel.Where(x => x.StartsWith("om" + model + "_")).Select(x => x.Substring(2, x.Length - 2)).ToArray();
                for (int k = 0; k < alink.Length; k++)
                {
                    string name = alink[k].model_Format();
                    bar.Add(new omAction()
                    {
                        _model = name,
                        _type = "link",
                        _sessionid = "0",
                        _viewid = "0",
                        type = "button",
                        id = name,
                        caption = name.Replace('_', ' '),
                        icon = ""
                    });
                }

                // ACTION
                bar.Add(new omAction() { type = "spacer" });
                var mi = db<omAction>.All()
                    .Where(x => x._status == oItemStatus.ACTIVED)
                    .Select((x, k) =>
                    {
                        x._model = model;
                        x._type = "action";
                        x._sessionid = "0";
                        x._viewid = "0";
                        string cmd = x.id;
                        x.tag = cmd;
                        x.id = (k + 1).ToString();
                        return x;
                    })
                    .ToArray();
                var mn = new omAction()
                {
                    type = "menu",
                    caption = "Action",
                    icon = "fa-wrench",
                    items = mi
                };
                bar.Add(mn);

                for (int k = 0; k < jButton.Length; k++)
                    bar[k].position = 1;
                jButton = JsonConvert.SerializeObject(bar, Formatting.Indented);

                #endregion

                #region [ SCRIPT GRID ]

                string url_api = "/data/grid?tab=" + model;
                var ai = db<omApi_model>.Query(x => x.model == model);
                if (ai.Length > 0) {
                    url_api = ai[0].api + model;
                }

                s_grid =
@"
var v_page_tabgrid" + ___model + @" = {
    name: 'page_tabgrid" + ___model + @"',
    autoLoad: true,
    limit: 50, 
    disableCVS: true,
    url : '" + url_api + @"',
    show: {
        toolbar: true,
        footer: true,
        lineNumbers: true,
        selectColumn: true,
        toolbarSearch: false,
        toolbarInput: false,
        searchAll: false,
    },
    sortData: [{ field: 'recid', direction: 'asc' }], 
    columns: " + jcol + @",
    onClick: function (event) {
        var record = this.get(event.recid);
        var sr = record.recid + ' | ' + record.fname + ' | ' + record.lname + ' | ' + record.email + ' | ' + record.sdate;
        //w2alert(sr);
        //setTimeout(function () {
        //    w2ui['grid2'].clear();
        //    this.selectNone();
        //    this.remove(event.recid);
        //}, 150);
    },
    toolbar: {
        items: " + jButton + @",
        onClick: function (target, eventData) {
            api.admin_grid_toolbar__click(target, eventData);
        }
    },
    records: [
    ],
    onRender: function(event) {
        event.onComplete = function () {
            api.admin_tab_grid_search_text_set(this.name);
            w2ui['page_tabgrid" + ___model + @"'].toggleColumn('recid');
            w2ui['page_tabgrid" + ___model + @"'].toggleColumn('_key');
        }
    }
};

$().w2grid(v_page_tabgrid" + ___model + @");


function tab" + ___model + @"_reload(objectAPI) {
    var o = objectAPI; 
    w2ui['main_layout'].content('main', w2ui['page_tabgrid" + ___model + @"']);
}

";
                if (isPopup)
                {
                    s_grid +=
@"

api.popup({
    name: 'popup_login',
    height: 480,
    width: 800,
    title: '" + model.Replace('_', ' ') + @"',
    bg: false,
    closeshow: true,
    close: function (pid) { },
    open: function (pid) {
        api.log(pid);
    },
    grid: v_page_tabgrid" + ___model + @"
});

";
                }

                #endregion
            }

            string js = db.text_EncodeHtml(s_grid);

            js.Replace("&amp;", "&");
            js.Replace("&apos;", "'");
            js.Replace("&#x27;", "'");
            js.Replace("&#x2F;", "/");
            js.Replace("&#39;", "'");
            js.Replace("&#47;", "/");
            js.Replace("&lt;", "<");
            js.Replace("&gt;", ">");
            js.Replace("&nbsp;", " ");
            js.Replace("&quot;", @"""");

            return js;
        }

    }
}
