using core.Db;
using core.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using System.Net;

namespace core.Api
{
    public static class pageProxy {
        /// <summary>
        /// Copies all headers and content (except the URL) from an incoming to an outgoing
        /// request.
        /// </summary>
        /// <param name="source">The request to copy from</param>
        /// <param name="destination">The request to copy to</param>
        public static void CopyTo(this HttpRequest source, HttpWebRequest destination)
        {
            destination.Method = source.HttpMethod;

            // Copy unrestricted headers (including cookies, if any)
            foreach (var headerKey in source.Headers.AllKeys)
            {
                switch (headerKey)
                {
                    case "Connection":
                    case "Content-Length":
                    case "Date":
                    case "Expect":
                    case "Host":
                    case "If-Modified-Since":
                    case "Range":
                    case "Transfer-Encoding":
                    case "Proxy-Connection":
                        // Let IIS handle these
                        break;

                    case "Accept":
                    case "Content-Type":
                    case "Referer":
                    case "User-Agent":
                        // Restricted - copied below
                        break;

                    default:
                        destination.Headers[headerKey] = source.Headers[headerKey];
                        break;
                }
            }

            // Copy restricted headers
            if (source.AcceptTypes.Any())
            {
                destination.Accept = string.Join(",", source.AcceptTypes);
            }
            destination.ContentType = source.ContentType;
            //destination.Referer = source.UrlReferrer.AbsoluteUri;
            destination.UserAgent = source.UserAgent;

            // Copy content (if content body is allowed)
            if (source.HttpMethod != "GET"
                && source.HttpMethod != "HEAD"
                && source.ContentLength > 0)
            {
                Stream stream = source.InputStream;
                stream.Position = 0;
                byte[] originalStream = null;
                using (var binaryReader = new BinaryReader(stream))
                {
                    //originalStream = binaryReader.ReadBytes(source.ContentLength);
                    originalStream = binaryReader.ReadBytes((int)source.InputStream.Length);
                }
                
                var destinationStream = destination.GetRequestStream();
                destinationStream.Position = 0;
                destinationStream.Write(originalStream, 0, originalStream.Length);
                destinationStream.Close();
            }
        }
    }

    public class page
    {

        static Regex regJsCss = new Regex(@"(.*?)\.(css|js|jpg|png|woff2|ttf)", RegexOptions.IgnoreCase);
        //private readonly Regex r = new Regex("^/tieu-de/(.*)$", RegexOptions.IgnoreCase);
        //Regex regJS = new Regex("^(.*)([.]v[0-9]+)([.](js|css))$", RegexOptions.IgnoreCase);
        //Context.RewritePath(string.Format("{0}{1}", matchJS.Groups[1].Value, matchJS.Groups[3].Value)); 

        public static void OnBeginRequest(object sender, System.EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext Context = app.Context;
            StreamReader readerInput = null;

            string path = Context.Request.Url.AbsolutePath.Split('?')[0].ToLower(),
                sResult = string.Empty,
                msg_ = string.Empty,
                sessionid_ = string.Empty,
                mid_ = string.Empty,
                key_ = string.Empty,
                control_ = string.Empty,
                tab = string.Empty,
                action = string.Empty;
            decimal msg_id = 0;

            switch (path)
            {
                case "/msg/get":
                    Context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                    Context.Response.Cache.SetNoStore();
                    Context.Response.Cache.SetNoServerCaching();

                    msg_ = Global.msg_Get();
                    Context.Response.Write(msg_);
                    Context.Response.End();
                    break;
                case "/favicon.ico":
                    Context.Response.End();
                    break;
                case "/":
                case "/default.aspx":
                    RenderHome(Context, "/index.htm");
                    break;
                case "/admin.html":
                case "/admin":
                    response_Admin(Context);
                    break;
                case "/message":
                    msgSSE.Connect(Context);
                    break;
                case "/socket":
                    socket.response_Port(Context);
                    break;
                case "/proxy":
                    proxy.response_Forward(sender);
                    break;
                case "/excel-temp":
                    key_ = Context.Request["key"];
                    db<omKeyword>.get_Excel_Temp(Context.Response);
                    break;
                case "/excel-export":
                    key_ = Context.Request["key"];
                    db<omKeyword>.get_Excel_Temp(Context.Response);
                    break;
                case "/excel-import":
                    #region
                    HttpPostedFile file = Context.Request.Files["file"];
                    if (file != null && file.ContentLength > 0)
                    {
                        string api = Context.Request["api"];
                        sessionid_ = Context.Request.Cookies["sessionid"] == null ? string.Empty : Context.Request.Cookies["sessionid"].Value;
                        //file.SaveAs(Context.Server.MapPath(Path.Combine("~/App_Data/", fname)));
                        msg m = new msg(msLAYER.FILE_UPLOAD_IMPORT, msLAYOUT.NONE, msCONTROL.NONE, msGRID.NONE, string.Empty);
                        msg_id = m.ID;
                        string fname = msg_id.ToString() + sessionid_ + "," + api + "," + Path.GetFileName(file.FileName);
                        cacheHashtable.Set(msg_id.ToString(), fname);
                        cacheHashtable.Set(fname, file.InputStream);
                        socket.Send(msg_id, sessionid_, true);
                        Context.Response.Write(fname);
                    }
                    Context.Response.End();
                    #endregion
                    break;
                case "/upload/photo":
                    #region
                    HttpPostedFile photo = Context.Request.Files["file"];
                    if (photo != null && photo.ContentLength > 0)
                    {
                        string model = Context.Request["model"];
                        string viewid = Context.Request["viewid"];
                        sessionid_ = Context.Request.Cookies["sessionid"] == null ? string.Empty : Context.Request.Cookies["sessionid"].Value;

                        string dir = Context.Server.MapPath("~/Views/Page/" + viewid + "/upload/" + model);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        string fname = Context.Server.MapPath(Path.Combine("~/Views/Page/" + viewid + "/upload/" + model, photo.FileName));
                        photo.SaveAs(fname);

                        Context.Response.Write(viewid + "/upload/" + model + "/" + photo.FileName);
                    }
                    Context.Response.End();

                    #endregion
                    break;

                case "/data/file":
                    #region
                    tab = Context.Request["tab"];
                    if (!string.IsNullOrEmpty(tab))
                    {
                        string model = tab.model_Format(),
                            viewid = Context.Request["viewid"];
                        sResult = "[]";

                        string fi = string.Format(@"{0}page\{1}\upload\{2}", dbVidew.PathRoot, viewid, model);


                        new Switch("om" + model)
                            .Case<omEmail_temp>(it =>
                            {
                                var afi = Directory.GetFiles(fi).Select(x => x.ToLower()).ToArray();
                                var ai = db<omEmail_temp>.All().Where(x => afi.Any(o => o.Contains(x.title.ToLower()))).ToArray();
                                sResult = JsonConvert.SerializeObject(ai);
                            });

                        Context.Response.Write(sResult);
                    }
                    Context.Response.End();

                    #endregion
                    break;
                case "/data/grid":
                    #region

                    tab = Context.Request["tab"];
                    string json = string.Empty;
                    if (!string.IsNullOrEmpty(tab))
                    {
                        readerInput = new StreamReader(HttpContext.Current.Request.InputStream);
                        string s = readerInput.ReadToEnd();
                        if (!string.IsNullOrEmpty(s) && s.Length > 8)
                        {
                            s = HttpUtility.UrlDecode(s);
                            s = s.Substring(8, s.Length - 8);

                            w2uiGridRequest o = JsonConvert.DeserializeObject<w2uiGridRequest>(s);
                            int page_number = 0, page_size = 50;
                            if (o.offset > 0)
                                page_number = o.offset / page_size + 1;

                            string textSearch = Context.Request.Cookies["search_page_tabgrid_" + tab] == null ?
                                string.Empty : Context.Request.Cookies["search_page_tabgrid_" + tab].Value;

                            json = db.queryJson(page_number, page_size, tab, textSearch);
                        }
                    }
                    if (string.IsNullOrEmpty(json))
                        json = @"{ status: ""error"", message: ""Không có dữ liệu"", postData: [] }";

                    Context.Response.ContentType = "application/json; charset=utf-8";
                    Context.Response.Write(json);
                    Context.Response.End();

                    #endregion
                    break;
                case "/api/link":
                    #region

                    readerInput = new StreamReader(HttpContext.Current.Request.InputStream);
                    msg_ = readerInput.ReadToEnd();
                    sResult = string.Empty;
                    if (!string.IsNullOrEmpty(msg_))
                    {
                        try
                        {
                            omAction o = JsonConvert.DeserializeObject<omAction>(msg_);
                            if (o._type == "link")
                            {
                                string model = o._model.model_Format();
                                o._model = model;
                                sResult = view.get_config_Grid(o._viewid, "tab", o._model, true);

                                new Switch("om" + model)
                                    .Case<omEmail_temp>(x =>
                                    {
                                    });
                            }
                        }
                        catch { }
                    }
                    Context.Response.ContentType = "text/plain; charset=utf-8";
                    Context.Response.Write(sResult);
                    Context.Response.End();

                    #endregion
                    break;
                case "/api/action":
                    #region

                    readerInput = new StreamReader(HttpContext.Current.Request.InputStream);
                    msg_ = readerInput.ReadToEnd();
                    sResult = string.Empty;
                    if (!string.IsNullOrEmpty(msg_))
                    {
                        try
                        {
                            omAction o = JsonConvert.DeserializeObject<omAction>(msg_);
                            if (o._type == "action")
                            {
                                string fi = string.Empty;
                                if (string.IsNullOrEmpty(o.command))
                                {
                                }
                                else
                                {
                                    fi = string.Format(@"{0}page\{1}\admin\action\_cmd\{2}.js", dbVidew.PathRoot, o._viewid, o.command);
                                }
                                if (File.Exists(fi))
                                    sResult = File.ReadAllText(fi);
                                else
                                {
                                }
                            }
                            sResult = sResult
                                .Replace("[model_key]", o._model);
                        }
                        catch { }
                    }
                    Context.Response.ContentType = "text/plain; charset=utf-8";
                    Context.Response.Write(sResult);
                    Context.Response.End();

                    #endregion 
                    break;
                case "/api":
                    #region
                    msg_ = Context.Request["msg"];
                    // vid,sid,mid,control,action,base64
                    // 0  , 1 , 2 , 3     ,  4   ,  5

                    if (!string.IsNullOrEmpty(msg_))
                    {
                        string[] a = msg_.Split(',');
                        if (a.Length > 5)
                        {
                            string mid = a[2];
                            if (decimal.TryParse(mid, out msg_id))
                            {
                                Global.msg_Add(msg_);

                                cacheHashtable.Set(mid, msg_);
                                sessionid_ = Context.Request.Cookies["sessionid"] == null ? string.Empty : Context.Request.Cookies["sessionid"].Value;
                                socket.Send(msg_id, sessionid_, true);

                                //omApiLog.Create(viewID, sessionid_, msg_id, Context.Request.UrlReferrer, msg_);
                            }
                        }
                    }
                    Context.Response.End();
                    #endregion
                    break;
                default:
                    #region
                    if (path.Contains(".html"))
                    {
                        int viewID = dbVidew.domain_GetView(Context.Request.Url.Host);
                        Context.Items[_const.viewID] = viewID;
                        RenderPage(Context, viewID, path);
                        //socket.Send(new wsData(wsType.USER_LOG, ))
                    }
                    else
                    {
                        int viewID = dbVidew.domain_GetView(Context.Request.Url.Host);
                        Context.Items[_const.viewID] = viewID;
                        string fi = string.Format("/views/page/{0}", viewID);
                        Context.RewritePath(fi + path);
                    }
                    #endregion
                    break;
            }
        }

        private static void RenderPage(HttpContext Context, int viewID, string pathHtml)
        {
            string viewKey = string.Format("{0}{1}", viewID, pathHtml);
            var v = dbVidew.uri_GetView(viewID, pathHtml);
            if (v.isStaticHTML)
            {

            }
            else
            {

                //////HttpApplication app = sender as HttpApplication;
                ////var headerValue = Context.Request.Headers["If-None-Match"];
                ////if (headerValue == null) headerValue = "0";
                ////long modifiedSince = 0;
                ////long.TryParse(headerValue, out modifiedSince);

                ////if (modifiedSince > 0)
                ////{
                ////    //return new HttpStatusCodeResult(304, "Page has not been modified");
                ////    Context.Response.StatusCode = 304;
                ////    Context.Response.StatusDescription = "Page has not been modified";
                ////    Context.Response.End();
                ////}
                ////else
                ////{

                #region [ read file run-time ]

                string file = string.Format(@"{0}page\{1}.htm", dbVidew.PathRoot, v.Path);
                string s = "";
                if (File.Exists(file))
                {
                    s = File.ReadAllText(file);
                }

                string tem = s.ToLower();
                if (tem.Contains("<control "))
                {
                    Regex rgx = new Regex("<control.+?name=[\"'](.+?)[\"'].*?>");
                    foreach (Match m in rgx.Matches(s))
                    {
                        string key = m.Groups[1].Value, tag = m.Value;
                        string si = string.Empty, fi = string.Empty;
                        switch (key)
                        {
                            case "_file":
                                string fii = Context.Request.Url.LocalPath;
                                fii = fii.Substring(1, fii.Length - 6);
                                fi = dbVidew.PathRoot + @"page\" + viewID.ToString() + @"\file\" + fii + ".htm";
                                if (File.Exists(fi))
                                    si = File.ReadAllText(fi);
                                s = s.Replace(tag, si);
                                break;
                            case "_img":
                                string fi_name = Context.Request.Url.LocalPath;
                                fi_name = fi_name.Substring(1).Replace(".html", string.Empty);
                                string fi_img = dbVidew.PathRoot + @"page\" + viewID.ToString() + @"\page-img\" + fi_name + ".png";
                                if (File.Exists(fi_img) == false)
                                    fi_name = "page-not-find";
                                si = @"<img class=""w100 page-img"" src=""page-img/" + fi_name + @".png""/>";
                                s = s.Replace(tag, si);
                                break;
                            default:
                                fi = dbVidew.PathRoot + @"page\" + viewID.ToString() + @"\" + key + ".htm";
                                if (File.Exists(fi))
                                    si = File.ReadAllText(fi);
                                s = s.Replace(tag, si);
                                break;
                        }

                    }
                }

                s = Render_SESSIONID(s, Context, viewID);

                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.ContentType = "text/html";
                Context.Response.Write(s);
                Context.Response.End();

                #endregion

                ////}
            }
        }

        private static void RenderHome(HttpContext Context, string pathHtml)
        {
            HttpRequest get = Context.Request;
            int viewID = dbVidew.domain_GetView(Context.Request.Url.Host);
            Context.Items[_const.viewID] = viewID;

            //////HttpApplication app = sender as HttpApplication;
            ////var headerValue = Context.Request.Headers["If-None-Match"];
            ////if (headerValue == null) headerValue = "0";
            ////long modifiedSince = 0;
            ////long.TryParse(headerValue, out modifiedSince);

            ////if (modifiedSince > 0)
            ////{
            ////    //return new HttpStatusCodeResult(304, "Page has not been modified");
            ////    Context.Response.StatusCode = 304;
            ////    Context.Response.StatusDescription = "Page has not been modified";
            ////    Context.Response.End();
            ////}
            ////else
            ////{

            #region [ read file run-time ]

            string file = string.Format(@"{0}page\{1}{2}", dbVidew.PathRoot, viewID, pathHtml);
            string s = "";
            if (File.Exists(file))
            {
                s = File.ReadAllText(file);
            }

            string tem = s.ToLower();
            if (tem.Contains("<control "))
            {
                Regex rgx = new Regex("<control.+?name=[\"'](.+?)[\"'].*?>");
                foreach (Match m in rgx.Matches(s))
                {
                    string key = m.Groups[1].Value, tag = m.Value;
                    string si = string.Empty, fi = string.Empty;
                    fi = dbVidew.PathRoot + @"page\" + viewID.ToString() + @"\" + key + ".htm";
                    if (File.Exists(fi))
                        si = File.ReadAllText(fi);
                    s = s.Replace(tag, si);
                }
            }


            s = Render_SESSIONID(s, Context, viewID);


            Context.Response.ContentEncoding = Encoding.UTF8;
            Context.Response.ContentType = "text/html";
            Context.Response.Write(s);
            Context.Response.End();

            #endregion

            ////}
        }

        public static void InstallResponseFilter(object sender, EventArgs e)
        {
            HttpResponse response = HttpContext.Current.Response;

            if (response.ContentType == "text/html")
            {
                HttpApplication app = sender as HttpApplication;
                string viewID = app.Context.Items[_const.viewID] as string;
                response.Filter = new PageFilter(response.Filter, viewID);
            }
        }

        public static void response_JsCssFont(HttpContext Context)
        {
            Uri uri = Context.Request.Url;
            string path = uri.AbsolutePath.Split('?')[0];

            var match = regJsCss.Match(path);
            if (match.Success)
            {
                int v = dbVidew.domain_GetView(uri.Host);
                //var fileName = match.Groups[1].Value;
                //string uri =  string.Format("/views/page/{0}{1}", v, path);
                //Context.RewritePath(uri);

                string content_type = "text/plain";
                string fileExt = match.Groups[2].Value;
                switch (fileExt)
                {
                    case "css":
                        content_type = "text/css";
                        break;
                    case "js":
                        content_type = "application/javascript";
                        break;
                    case "jpg":
                        content_type = "image/jpeg";
                        break;
                    case "png":
                        content_type = "image/png";
                        break;
                    case "woff":
                        content_type = "application/font-woff";
                        break;
                    case "woff2":
                        content_type = "application/font-woff2";
                        break;
                    case "ttf":
                        content_type = "application/x-font-ttf";
                        break;
                }

                //HttpApplication app = sender as HttpApplication;
                var headerValue = Context.Request.Headers["If-None-Match"];
                if (headerValue == null) headerValue = "0";
                long modifiedSince = 0;
                long.TryParse(headerValue, out modifiedSince);

                ////if (modifiedSince > 0)
                ////{
                ////    //return new HttpStatusCodeResult(304, "Page has not been modified");
                ////    Context.Response.StatusCode = 304;
                ////    Context.Response.StatusDescription = "Page has not been modified";
                ////    Context.Response.End();
                ////}
                ////else
                ////{

                string fileKey = string.Format("/views/page/{0}{1}", v, path);
                string filePath = Context.Server.MapPath(fileKey);
                byte[] b = dbVidew.file_GetCache(fileKey, filePath);

                Context.Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                Context.Response.Cache.SetETag(DateTime.Now.ToString("yyyyMMddHHmmss"));

                Context.Response.ContentType = content_type; //app.Context.Request.ContentType;
                if (b != null)
                    Context.Response.BinaryWrite(b);
                Context.Response.End();
                ////}
            }//end if [css, js, png, jpg]
        }


        public static string _session_init(HttpContext Context)
        {
            HttpResponse res = Context.Response;
            HttpRequest get = Context.Request;
            string _sessionid = get.Cookies["sessionid"] == null ? string.Empty : get.Cookies["sessionid"].Value;
            if (string.IsNullOrEmpty(_sessionid))
            {
                _sessionid = DateTime.Now.ToString("1900MMddHHmmssfff") + (new Random().Next(10000, 99999)).ToString();
                //HttpCookie myCookie = new HttpCookie("sessionid");
                //myCookie.Value = _sessionid;
                //myCookie.Expires = DateTime.Now.AddMinutes(180);
                //res.Cookies.Add(myCookie);
            }
            return _sessionid;
        }

        public static string Render_SESSIONID(string s, HttpContext Context, int viewID)
        {
            HttpRequest get = Context.Request;
            string _sessionid = get.Cookies["sessionid"] == null ? string.Empty : get.Cookies["sessionid"].Value;
            if (string.IsNullOrEmpty(_sessionid))
            {
                _sessionid = DateTime.Now.ToString("1900MMddHHmmssfff") + (new Random().Next(10000, 99999)).ToString();

                string time = DateTime.Now.AddDays(10).ToString("ddd, d MMM yyyy HH:mm:ss UTC");
                //DateTime date = DateTime.ParseExact("Tue, 1 Jan 2008 00:00:00 UTC","ddd, d MMM yyyy HH:mm:ss UTC", System.Globalization.CultureInfo.InvariantCulture);

                string js = string.Format(@"<head>{0}<script type=""text/javascript""> document.cookie = 'viewid={1}; expires={2}; path=/'; document.cookie = 'sessionid={3}; expires={2}; path=/'; </script>{0}", Environment.NewLine, viewID, time, _sessionid);

                s = s.Replace("<head>", js);
            }

            return s;
        }


        public static void response_Admin(HttpContext Context)
        {
            HttpResponse res = Context.Response;
            int viewID = dbVidew.domain_GetView(Context.Request.Url.Host);
            Context.Items[_const.viewID] = viewID;

            //HttpCookie myCookie = new HttpCookie("sessionid");
            //myCookie.Value = DateTime.Now.ToString("1900MMddHHmmssfff") + (new Random().Next(1, 999999)).ToString();
            //myCookie.Expires = DateTime.Now.AddMinutes(180);
            //res.Cookies.Add(myCookie);

            //int viewID = dbVidew.domain_GetView(Context.Request.Url.Host);
            string fi = dbVidew.PathRoot + @"page\" + viewID.ToString() + @"\admin\admin.htm";
            string s = string.Empty;

            if (File.Exists(fi))
            {
                s = File.ReadAllText(fi);
                s = string.Format(s, Guid.NewGuid().ToString());
                s = Render_SESSIONID(s, Context, viewID);

                string js = string.Format(@"{0}<script type=""text/javascript""> api.admin(); </script>{0}</body>", Environment.NewLine);
                s = s.Replace("</body>", js);
            }

            res.ContentType = "text/html";
            res.Write(s);
            res.End();
        }
    }
}
