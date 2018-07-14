using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace core.Api
{
    public class proxy
    {
        public static void response_Forward(object sender)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext context = app.Context;

            HttpWebResponse newResponse = null;
            string redirUrl = "http://newHost/services/newMyService.asmx";

            string data = WebServiceRedirect(app, redirUrl, out newResponse);
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentType = newResponse.ContentType;
            context.Response.Write(data);
            context.Response.End();
        }

        static string WebServiceRedirect(HttpApplication context, string url, out HttpWebResponse newResponse)
        {
            byte[] bytes = context.Request.BinaryRead(context.Request.TotalBytes);
            char[] responseBody = Encoding.UTF8.GetChars(bytes, 0, bytes.Length);

            HttpWebRequest newRequest = (HttpWebRequest)WebRequest.Create(url);
            newRequest.AllowAutoRedirect = false;
            newRequest.ContentLength = context.Request.ContentLength;
            newRequest.ContentType = context.Request.ContentType;
            newRequest.UseDefaultCredentials = true;
            newRequest.UserAgent = ".NET Web Proxy";
            newRequest.Referer = url;
            newRequest.Method = context.Request.RequestType;

            if (context.Request.AcceptTypes.Length > 0)
                newRequest.MediaType = context.Request.AcceptTypes[0];

            foreach (string str in context.Request.Headers.Keys)
            {
                try { newRequest.Headers.Add(str, context.Request.Headers[str]); }
                catch { }
            }

            if (newRequest.Method.ToLower() == "post")
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter((newRequest.GetRequestStream())))
                {
                    sw.Write(responseBody); sw.Flush(); sw.Close();
                }
            }

            string temp = "";
            try
            {
                newResponse = (HttpWebResponse)newRequest.GetResponse();
                using (System.IO.StreamReader sw = new System.IO.StreamReader((newResponse.GetResponseStream())))
                {
                    temp = sw.ReadToEnd();
                    sw.Close();
                }
            }
            catch (WebException exc)
            {
                using (System.IO.StreamReader sw = new System.IO.StreamReader((exc.Response.GetResponseStream())))
                {
                    newResponse = (HttpWebResponse)exc.Response;
                    temp = sw.ReadToEnd();
                    sw.Close();
                }
            }

            return temp;
        }
    }
}
