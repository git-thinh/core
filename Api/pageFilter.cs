using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using core.Db;

namespace core.Api
{
    /// <summary>
    /// PageFilter does all the dirty work of tinkering the 
    /// outgoing HTML stream. This is a good place to
    /// enforce some compilancy with web standards.
    /// </summary>
    public class PageFilter : Stream
    {
        string ViewID = string.Empty;
        Stream responseStream;
        long position;
        StringBuilder responseHtml;

        public PageFilter(Stream inputStream , string viewID)
        {
            ViewID = viewID;
            responseStream = inputStream;
            responseHtml = new StringBuilder();
        }

        #region Filter overrides

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Close()
        {
            responseStream.Close();
        }

        public override void Flush()
        {
            responseStream.Flush();
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get { return position; }
            set { position = value; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return responseStream.Seek(offset, origin);
        }

        public override void SetLength(long length)
        {
            responseStream.SetLength(length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return responseStream.Read(buffer, offset, count);
        }

        #endregion
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            string s = System.Text.UTF8Encoding.UTF8.GetString(buffer, offset, count);
            string tem = s.ToLower();
            if (tem.Contains("<control ")) { 
                Regex rgx = new Regex("<control.+?name=[\"'](.+?)[\"'].*?>"); 
                foreach (Match m in rgx.Matches(s))
                {
                    string key = m.Groups[1].Value, tag = m.Value;
                    string si = string.Empty; 
                    string fi = dbVidew.PathRoot + @"page\" + ViewID + @"\" + key + ".htm";
                    if (File.Exists(fi))
                        si = File.ReadAllText(fi);
                    s = s.Replace(tag, si);
                }
            }

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(s);
            responseStream.Write(data, 0, data.Length);



            ////////string strBuffer = System.Text.UTF8Encoding.UTF8.GetString(buffer, offset, count);
            ////////// ---------------------------------
            ////////// Wait for the closing </html> tag
            ////////// ---------------------------------
            ////////Regex eof = new Regex("</html>", RegexOptions.IgnoreCase);
            ////////if (!eof.IsMatch(strBuffer))
            ////////{
            ////////    responseHtml.Append(strBuffer);
            ////////}
            ////////else
            ////////{
            ////////    responseHtml.Append(strBuffer);
            ////////    string finalHtml = responseHtml.ToString();
            ////////    // Transform the response and write it back out
            ////////    byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(finalHtml);
            ////////    responseStream.Write(data, 0, data.Length);
            ////////}
        }
    }
}