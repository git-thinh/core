using core.Api;
using core.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace core.Db
{

    public class dbProcess
    {
        public static void Execute(msLAYER apitype, omApi api)
        {
            string 
                msg_ = string.Empty,
                msg_result = string.Empty, 
                file = string.Empty,
                data = api.data.ToString();

            switch (apitype)
            {
                case msLAYER.FILE_UPLOAD:
                    break;
                case msLAYER.FILE_UPLOAD_IMPORT:
                    #region

                    string[] a_ = data.Split(new string[] { ",", "." }, StringSplitOptions.None);
                    if (a_.Length > 2)
                    {
                        string f_call = a_[1], oName = a_[2];
                        new Switch(oName)
                          .Case<omKeyword>
                              (action =>
                              {
                                  var so = cacheHashtable.Get<Stream>(data);
                                  omKeyword[] at = db<omKeyword>.convert_Excel_Import_DB(so)
                                        .Where(x => x != null && !string.IsNullOrEmpty(x.name_utf8))
                                        .ToArray();

                                  for (int k = 0; k < at.Length; k++)
                                  {
                                      try
                                      {
                                          string name = at[k].name_utf8.ToLower().Trim();
                                          at[k].name_ascii = db.text_ConvertUnicode2Ascii(name);
                                          at[k].setRecid(k);
                                          at[k].level = name.Split(' ').Length;

                                          if (db.text_IsASCII(name))
                                          {
                                              at[k].key = name;
                                              at[k].name_utf8 = string.Empty;
                                          }
                                          else
                                              at[k].name_utf8 = name;
                                      }
                                      catch (Exception ex)
                                      {
                                      }
                                  }

                                  at = at.Where(x => x.recid > 1).GroupBy(x => x.name_ascii).Select(x => x.First()).OrderBy(x => x.level).ToArray();

                                  string js = JsonConvert.SerializeObject(at);
                                  if (at.Length > 0)
                                  {
                                      js = db.text_EncodeHtml(js);
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
                                  }

                                  api.OK = true;
                                  api.data_type = "js_fun." + f_call;
                                  api.data = db.EncodeTo64(js);
                                  msg_result = JsonConvert.SerializeObject(api);
                                  dbMSG.set_MsgInQueue(api.sessionid, msg_result);
                              })
                          .Case<omUser>
                              (action =>
                              {

                              });
                    }

                    #endregion
                    break;
                case msLAYER.API_CONFIG:
                    #region
                    switch (api.control)
                    {
                        case "action":
                            #region
                            if (string.IsNullOrEmpty(api.action))
                            {
                            }
                            #endregion
                            break;
                        case "tab":
                            #region

                            msg_ = view.get_config_Grid(api.viewid, api.control, api.action);
                            api.OK = true;
                            api.data_type = apitype.ToString().ToLower();
                            api.data = db.EncodeTo64(msg_);
                            msg_result = JsonConvert.SerializeObject(api);
                            dbMSG.set_MsgInQueue(api.sessionid, msg_result);

                            #endregion
                            break;
                        default:
                            #region
                            file = string.Format(@"{0}page\{1}\admin\{2}\{3}.js", dbVidew.PathRoot, api.viewid, api.control, api.action);
                            if (File.Exists(file))
                            {
                                string fjs = File.ReadAllText(file);
                                //js = HttpUtility.HtmlEncode(js);
                                fjs = db.text_EncodeHtml(fjs);

                                fjs.Replace("&amp;", "&");
                                fjs.Replace("&apos;", "'");
                                fjs.Replace("&#x27;", "'");
                                fjs.Replace("&#x2F;", "/");
                                fjs.Replace("&#39;", "'");
                                fjs.Replace("&#47;", "/");
                                fjs.Replace("&lt;", "<");
                                fjs.Replace("&gt;", ">");
                                fjs.Replace("&nbsp;", " ");
                                fjs.Replace("&quot;", @"""");

                                if (api.control == "layout" && api.action == "main")
                                {
                                    StringBuilder bi = new StringBuilder("items: [");
                                    for (int k = 0; k < db.m_tab_System.Length; k++)
                                        bi.Append("{ type: 'button', caption: '" + db.m_tab_System[k] + "', icon: 'fa-star-empty' },");
                                    bi.Append("]");
                                    fjs = fjs.Replace("items: [{}]", bi.ToString());

                                    StringBuilder bt = new StringBuilder("tabs: [");
                                    for (int k = 0; k < db.m_tab_Model.Length; k++)
                                        bt.Append("{ id: 'tab_" + db.m_tab_Model[k] + "', caption: '" + db.m_tab_Model[k] + "' },");
                                    bt.Append("]");
                                    fjs = fjs.Replace("tabs: [{}]", bt.ToString());
                                }

                                api.OK = true;
                                api.data_type = apitype.ToString().ToLower();
                                api.data = db.EncodeTo64(fjs);
                                msg_result = JsonConvert.SerializeObject(api);
                                dbMSG.set_MsgInQueue(api.sessionid, msg_result);
                            }
                            #endregion
                            break;
                    }


                    #endregion
                    break;
                default:
                    #region

                    //switch (api.control)
                    //{
                    //    case "view":
                    //        switch (api.action)
                    //        {
                    //            case "admin":
                    //                #region
                    //                string fi = string.Empty;// dbVidew.PathRoot + @"page\" + api.viewid + @"\admin\adm\ads.js";
                    //                fi = string.Format(@"{0}page\{1}\admin\{2}\{3}.js", dbVidew.PathRoot, api.viewid, api.control, api.action);

                    //                if (File.Exists(fi))
                    //                {
                    //                    string js = File.ReadAllText(fi);
                    //                    //js = HttpUtility.HtmlEncode(js);
                    //                    js = db.text_EncodeHtml(js);


                    //                    js.Replace("&amp;", "&");
                    //                    js.Replace("&apos;", "'");
                    //                    js.Replace("&#x27;", "'");
                    //                    js.Replace("&#x2F;", "/");
                    //                    js.Replace("&#39;", "'");
                    //                    js.Replace("&#47;", "/");
                    //                    js.Replace("&lt;", "<");
                    //                    js.Replace("&gt;", ">");
                    //                    js.Replace("&nbsp;", " ");
                    //                    js.Replace("&quot;", @"""");

                    //                    api.OK = true;
                    //                    api.data_type = "js_val";
                    //                    api.data = db.EncodeTo64(js);
                    //                    msg_result = JsonConvert.SerializeObject(api);
                    //                    dbMSG.set_MsgInQueue(api.sessionid, msg_result);
                    //                }
                    //                #endregion
                    //                break;
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}

                    #endregion
                    break;
            }
        }
    }

}
