using core.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace core.Db
{
    public enum typeView
    {
        NONE,
        STATIC_FILE_HTML,
        TEMPLATE_HOME,
        TEMPLATE_FILE_HTML,
        TEMPLATE_NOT_FIND
    }

    public class pageView
    {
        public pageView(int id)
        {
            ID = id;
        }

        public pageView(int id, typeView type)
        {
            ID = id;
            Type = type;
        }

        public pageView(int id, typeView type, string path)
        {
            ID = id;
            Type = type;
            Path = path;
        }

        public int ID { set; get; }
        public typeView Type { set; get; }
        public string Path { set; get; }

        public override string ToString()
        {
            return string.Format("{0}, {1} , {2}", ID, Type, Path);
        }

        public bool isStaticHTML
        {
            get
            {
                if (Type == typeView.STATIC_FILE_HTML) return true;
                return false;
            }
        }
         
    }

    public class dbVidew
    {
        private static string _view_path_root = string.Empty;
        private static string _view_default = ConfigurationManager.AppSettings["view_default"];

        static SynchronizedDictionary<string, int> _dbDomainView = new SynchronizedDictionary<string, int>() { };
        static SynchronizedDictionary<string, string> _dbUriView = new SynchronizedDictionary<string, string>() { };
        static SynchronizedDictionary<string, byte[]> _dbFileCache = new SynchronizedDictionary<string, byte[]>() { };

        static SynchronizedDictionary<string, string> _dbViewItemData = new SynchronizedDictionary<string, string>() { };

        public static string PathRoot {
            get { return _view_path_root; }
        }

        public static void Init(string path_root)
        {
            _view_path_root = path_root + @"Views\";

            int vDefault = 0;
            int.TryParse(_view_default, out vDefault);

            string fi = path_root + "dns.json";
            if (File.Exists(fi)) {
                string json = File.ReadAllText(fi);
                omDns[] a = JsonConvert.DeserializeObject<omDns[]>(json);
                if (a.Length > 0) {
                    a = a.GroupBy(x => x.host).Select(x => x.Last()).ToArray();
                    for (int k = 0; k < a.Length; k++) {
                        domain_Update(a[k].host, a[k].viewid);
                    }
                }
            }

            string fiTem = path_root + "dnstem.json";
            if (File.Exists(fiTem)) {
                string json = File.ReadAllText(fiTem);
                omDnsTemplate[] a = JsonConvert.DeserializeObject<omDnsTemplate[]>(json);
                if (a.Length > 0) {
                    a = a.GroupBy(x => x.path).Select(x => x.Last()).ToArray();
                    for (int k = 0; k < a.Length; k++) {
                        uri_Update(a[k].path, a[k].template);
                    }
                }
            }

            //domain_Update("localhost", vDefault);
            //domain_Update("ebds.vn", vDefault);
            //domain_Update("phuquy.ebds.vn", vDefault);
            //domain_Update("phuquy.mbds.vn", vDefault);
            //domain_Update("192.168.1.100", vDefault);
            //domain_Update("118.70.81.249", vDefault);
            //domain_Update("118.70.81.253", vDefault);

            ////domain_Update("localhost", 1000);
            ////domain_Update("localhost", 1001);
            ////domain_Update("localhost", 1002);

            //domain_Update("vieclambaove.iot.vn", 2000);
            //uri_Update("2000/tin-tuc.html", "news");
            //uri_Update("2000/viec-lam-bao-ve-vip-chu-tich-quoc-hoi-nguyen-thi-kim-ngan.html", "news-detail");


        }

        public static byte[] file_GetCache(string fileKey, string filePath)
        {
            byte[] v = new byte[] { };
            if (_dbFileCache.TryGetValue(filePath, out v) == false)
            {
                if (File.Exists(filePath))
                {
                    v = File.ReadAllBytes(filePath);
                    _dbFileCache.Add(fileKey, v);
                }
            }
            return v;
        }

        public static string get_MapPath(int viewID, string filename) {
            return string.Format(@"{0}page\{1}\{2}", PathRoot, viewID, filename); 
        }

        public static void domain_Update(string uri, int view)
        {
            _dbDomainView.Add(uri, view);
        }

        public static int domain_GetView(string domain)
        {
            int v = 0;
            if (_dbDomainView.TryGetValue(domain, out v)) return v;
            return v;
        }

        public static void uri_Update(string uri, string view)
        {
            _dbUriView.Add(uri, view);
        }

        public static pageView uri_GetView(int viewID, string path)
        { 
            string v = string.Empty, viewKey = string.Format("{0}{1}", viewID, path);
            if (_dbUriView.TryGetValue(viewKey, out v))
                return new pageView(viewID, typeView.TEMPLATE_FILE_HTML, viewID.ToString() + @"\" + v);
            else
            {
                string fi = _view_path_root + @"page\" + viewID.ToString() + path;
                fi = fi.Replace('/', '\\');
                if (File.Exists(fi))
                    return new pageView(viewID, typeView.STATIC_FILE_HTML, fi);
            }
            return new pageView(viewID, typeView.TEMPLATE_NOT_FIND);
        }

        public static pageView uri_GetView(Uri uri)
        {
            string domain = uri.Host, path = uri.LocalPath == "/" ? string.Empty : uri.LocalPath;
            int id = domain_GetView(domain);
            if (path.Equals("/index.html")) return new pageView(id, typeView.TEMPLATE_HOME, id.ToString() + @"\" + "index");

            string key = id.ToString() + path;
            string v = string.Empty;
            if (_dbUriView.TryGetValue(key, out v)) return new pageView(id, typeView.TEMPLATE_HOME, id.ToString() + @"\" + v);
            else
            {
                string fi = _view_path_root + @"page\" + id.ToString() + path;
                fi = fi.Replace('/', '\\');
                if (File.Exists(fi))
                    return new pageView(id, typeView.STATIC_FILE_HTML, fi);
            }
            return new pageView(id, typeView.TEMPLATE_NOT_FIND);
        }
    }
}
