using core.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Linq;
using core.Model;
using System.Web;
using Newtonsoft.Json;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using core.Db;

namespace core
{
    public class db
    {
        public const string m_split_controlAction = "___";
        public static string[] m_tab_System = new string[] { };
        public static string[] m_tab_Model = new string[] { };
        public static string[] m_arrayModel = new string[] { };

        public static void Init(string path_root)
        {
            PathDirectory = path_root + @"DB\";

            db<omApi_model>.Open();
            db<omTab>.Open();
            db<omColumn>.Open();
            #region [ MODEL ]

            List<omColumn> lsCol = new List<omColumn>() { };
            var at = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                      from t in asm.GetTypes()
                          //where type.IsClass && type.Name == "TestClass"
                      where t.IsClass && t.FullName.StartsWith("core.Model.om")
                      select t).ToArray();
            m_arrayModel = at.Select(x => x.Name).ToArray();

            for (int k = 0; k < at.Length; k++)
            {
                PropertyInfo[] props = at[k].GetProperties();
                string tname = at[k].Name;
                tname = tname.Substring(2, tname.Length - 2);
                int ki = 0;
                foreach (PropertyInfo prop in props)
                {
                    ki++;
                    //object value = prop.GetValue(atype, new object[] { });
                    //dict.Add(prop.Name, value);
                    var attr = prop.GetCustomAttributes(typeof(colAttr), false);
                    if (attr != null && attr.Length > 0)
                    {
                        colAttr col = attr.Cast<colAttr>().Single();
                        if (col != null)
                        {
                            omColumn o = new omColumn(col);
                            o._key = db.get_KeyRandom();
                            o.field = prop.Name;
                            o._tabname = tname;
                            o._datatype = prop.PropertyType.Name.ToLower();
                            lsCol.Add(o);
                        }
                    }
                }
            }
            db<omColumn>.Cache(lsCol.ToArray());
            #endregion

            db<omUser>.Open();
            if (db<omUser>.Count == 0)
                db<omUser>.UpdateOrInsert(new omUser() { username = "admin", password = db.text_EncryptMD5("admin") });

            db<omAction>.Open(true);
            db<omKeyword>.Open(true);
            db<omArticle>.Open(true);
            db<omEmail>.Open(true);
            db<omEmail_temp>.Open(true);
            db<omEmail_account>.Open(true);
            db<omEmail_report_send>.Open(true);

            var xs = Directory.GetFiles(PathDirectory, "*.xls");
            if (xs.Length > 0)
            {
                for (int k = 0; k < xs.Length; k++)
                {
                    string fi = xs[k];
                    string[] a = fi.Split('\\');
                    string name = a[a.Length - 1];
                    name = name.Substring(0, name.Length - 4);
                    Stream stream = File.OpenRead(fi);

                    new Switch(name)
                      .Case<omUser>
                          (action =>
                          {
                          })
                      .Case<omApi_model>
                          (action =>
                          {
                              var ai = db<omApi_model>.restore_Excel_Import_DB(stream);
                              if (ai.Length > 0)
                                  db<omApi_model>.Cache(ai);
                          })
                      .Case<omTab>
                          (action =>
                          {
                              var ai = db<omTab>.restore_Excel_Import_DB(stream);
                              if (ai.Length > 0)
                              {
                                  m_tab_System = ai.Where(x => x.show_navi).Select(x => x.name).ToArray();
                                  m_tab_Model = ai.Where(x => x.show_navi == false).Select(x => x.name).ToArray();
                                  db<omTab>.Cache(ai);
                              }
                          })
                      .Case<omAction>
                          (action =>
                          {
                              var ai = db<omAction>.restore_Excel_Import_DB(stream);
                              if (ai.Length > 0)
                                  db<omAction>.Cache(ai);
                          })

                    #region [ EMAIL ] 
                      .Case<omEmail>
                          (action =>
                          {
                              var ai = db<omEmail>.restore_Excel_Import_DB(stream);
                              if (ai.Length > 0)
                              {
                                  ai = ai.Where(x => !string.IsNullOrEmpty(x.email) && x.email.Contains("@")).ToArray();
                                  db<omEmail>.Cache(ai);
                              }
                          })
                      .Case<omEmail_temp>
                          (action =>
                          {
                              var ai = db<omEmail_temp>.restore_Excel_Import_DB(stream);
                              if (ai.Length > 0)
                              {
                                  db<omEmail_temp>.Cache(ai);
                              }
                          })
                      .Case<omEmail_account>
                          (action =>
                          {
                              var ai = db<omEmail_account>.restore_Excel_Import_DB(stream);
                              if (ai.Length > 0)
                              {
                                  db<omEmail_account>.Cache(ai);
                              }
                          })
                    #endregion

                      .Case<omKeyword>
                          (action =>
                          {

                          })
                          ;
                }
            }
        }

        public static void process_MSG(msgFor mfor, string msg_id_sid)
        {
            if (msg_id_sid.Length < (_const.msg_ID_LEN + _const.session_ID_LEN)) return;
            string sessionid = string.Empty, msg_id = string.Empty, data = string.Empty, json = string.Empty;
            msg_id = msg_id_sid.Substring(0, _const.msg_ID_LEN);
            sessionid = msg_id_sid.Substring(_const.msg_ID_LEN, _const.session_ID_LEN);

            string msg_result = string.Empty;
            omApi api = new omApi();
            Dictionary<string, string> values = new Dictionary<string, string>() { };
            switch (mfor)
            {
                case msgFor.API:
                    #region [ API ]
                    try
                    {
                        data = cacheHashtable.Get<string>(msg_id);
                        api.data = data;
                        if (!string.IsNullOrEmpty(data) && data.Length > _const.msg_ID_LEN)
                        {
                            #region [ DATA ]

                            msLAYER apitype = msLAYER.NONE;
                            string sapi_ = msg_id.Substring(0, 7);
                            int api_ = 0;
                            if (int.TryParse(sapi_, out api_)) apitype = (msLAYER)api_;

                            // vid,sid,mid,control,action,base64
                            // 0  , 1 , 2 , 3     ,  4   ,  5
                            string[] a = data.Split(',');

                            try
                            {
                                byte[] b = Convert.FromBase64String(a[a.Length - 1].Trim());
                                json = Encoding.UTF8.GetString(b);
                                api = JsonConvert.DeserializeObject<omApi>(json);
                            }
                            catch
                            {
                            }

                            api.msg_id = msg_id;
                            api.sessionid = sessionid;
                            api.data_type = "json";
                            api.OK = false;
                            api.relogin = false;
                            string para = api.data.ToString();
                            if (!string.IsNullOrEmpty(para))
                            {
                                try
                                {
                                    para = db.DecodeFrom64(para);
                                    values = JsonConvert.DeserializeObject<Dictionary<string, string>>(para);
                                }
                                catch { }
                            }

                            #endregion

                            #region
                            if (api.action == "login")
                            {
                                #region

                                string username_ = string.Empty, password_ = string.Empty;
                                values.TryGetValue("username", out username_);
                                values.TryGetValue("password", out password_);
                                var u0 = db<omUser>.Query(x => x.password == db.text_EncryptMD5(password_) && x.username == username_);
                                if (u0.Length > 0)
                                {
                                    api.data = u0;// db.Encrypt(JsonConvert.SerializeObject(u0[0]));
                                    dbMSG.set_SessionID_Login_OK(api.sessionid);
                                    api.OK = true;
                                }
                                else
                                {
                                    api.data = string.Empty;
                                    api.OK = false;
                                }
                                msg_result = JsonConvert.SerializeObject(api);
                                dbMSG.set_MsgInQueue(api.sessionid, msg_result);

                                #endregion
                            }
                            else
                            {
                                bool login = dbMSG.check_SessionID_Login_OK(api.sessionid);
                                if (login)
                                    dbProcess.Execute(apitype, api);
                                else
                                {
                                    api.relogin = true;
                                    msg_result = JsonConvert.SerializeObject(api);
                                    dbMSG.set_MsgInQueue(api.sessionid, msg_result);
                                }
                            }
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    #endregion
                    break;
                case msgFor.EVENT:
                    break;
                default:
                    break;
            }

            //wsData m = new wsData(ms);
            //if (m != null)
            //{
            //    switch (m.Type)
            //    {
            //        case wsType.DB_INIT:

            //            break;
            //        case wsType.DB_UPDATE_FILE:
            //            db.update_FileLOG(m.Data);
            //            break;
            //        case wsType.USER_LOGIN:
            //            omUser u = new omUser(m.Data);
            //            var au = db<omUser>.Query(x => x.username == u.username && x.password == u.password);

            //            break;
            //    }
            //}
        }

        public static string queryJson(int page_number, int page_size, string model_name, string textSearch = "")
        {
            string json = string.Empty;
            model_name = (model_name.StartsWith("om") ? string.Empty : "om") + model_name.model_Format();

            new Switch(model_name)
              .Case<omEmail>
                  (action_ =>
                  {
                      if (string.IsNullOrEmpty(textSearch))
                          json = db<omEmail>.grid_AllItem(page_number, page_size);
                      else
                          json = db<omEmail>.grid_Item(x => x.email.Contains(textSearch) || x.phone.Contains(textSearch), page_number, page_size);
                  })
              .Case<omEmail_temp>
                  (action_ =>
                  {
                      if (string.IsNullOrEmpty(textSearch))
                          json = db<omEmail_temp>.grid_AllItem(page_number, page_size);
                      else
                          json = db<omEmail_temp>.grid_Item(x => x.title.Contains(textSearch) || x.imgs.Contains(textSearch), page_number, page_size);
                  })
              .Case<omEmail_account>
                  (action_ =>
                  {
                      if (string.IsNullOrEmpty(textSearch))
                          json = db<omEmail_account>.grid_AllItem(page_number, page_size);
                      else
                          json = db<omEmail_account>.grid_Item(x => x.email.Contains(textSearch) || x.password.Contains(textSearch), page_number, page_size);
                  })
              .Case<omEmail_report_send>
                  (action_ =>
                  {
                      if (string.IsNullOrEmpty(textSearch))
                          json = db<omEmail_report_send>.grid_AllItem(page_number, page_size);
                      else
                          json = db<omEmail_report_send>.grid_Item(x => x.email.Contains(textSearch) || x.title.Contains(textSearch), page_number, page_size);
                  })
              .Case<omAction>
                  (action_ =>
                  {
                      if (string.IsNullOrEmpty(textSearch))
                          json = db<omAction>.grid_AllItem(page_number, page_size);
                      else
                          json = db<omAction>.grid_Item(x => x.id.Contains(textSearch), page_number, page_size);
                  });

            return json;
        }

        #region [ Init, Open, CreateFileBlank ]

        public static string Ext = ".db";
        static SynchronizedDictionary<string, dbStatus> dicSTATUS = new SynchronizedDictionary<string, dbStatus>() { };
        static SynchronizedDictionary<string, string> dicLINE = new SynchronizedDictionary<string, string>() { };

        public static string PathDirectory = string.Empty;
        static readonly object lockFile = new object();

        public static void CreateFileBlank(string file, int SizeDesired_MB)
        {
            if (SizeDesired_MB < 1) SizeDesired_MB = 1;

            //string file = @"e:\ifc.db";
            //long length_add = 1024L * 1024L * 1L; // 1MB
            long length_add = 1024L * 1024L * SizeDesired_MB; // SizeDesired_MB MB

            // create blank file of desired size (nice and quick!)
            FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
            fs.Seek(length_add, SeekOrigin.Begin);
            fs.WriteByte(0);
            fs.Close();
        }

        #endregion

        #region [ STATUS ]

        public static dbStatus status_GET(string name)
        {
            dbStatus s = dbStatus.CLOSED;
            dicSTATUS.TryGetValue(name, out s);
            return s;
        }

        public static void status_SET(string name, dbStatus status)
        {
            dicSTATUS[name] = status;
        }

        #endregion

        #region [ BASE64 ]

        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        static public string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);
            return returnValue;
        }

        #endregion

        #region [ LOG ]

        public static void update_FileLOG(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (cacheHashtable.Exist(name))
            {
                string data = cacheHashtable.Get<string>(name);
                string fi = string.Format("{0}{1}", PathDirectory, name);
                lock (lockFile)
                    File.WriteAllText(fi, data, Encoding.UTF8);
                cacheHashtable.Remove(name);
            }
        }

        public static void job_UpdateAll_FileLOG()
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Select(t => t.FullName).Where(x => x.StartsWith("core.Model.om"))
                .Select(x => x.Substring(12, x.Length - 12))
                .ToArray();

        }

        #endregion

        #region [ TEXT: Encrypt, Decrypt, IsASCII, EncodeHtml ]

        private static readonly string[] VietNamChar = new string[]
    {
        "aAeEoOuUiIdDyY",
        "áàạảãâấầậẩẫăắằặẳẵ",
        "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
        "éèẹẻẽêếềệểễ",
        "ÉÈẸẺẼÊẾỀỆỂỄ",
        "óòọỏõôốồộổỗơớờợởỡ",
        "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
        "úùụủũưứừựửữ",
        "ÚÙỤỦŨƯỨỪỰỬỮ",
        "íìịỉĩ",
        "ÍÌỊỈĨ",
        "đ",
        "Đ",
        "ýỳỵỷỹ",
        "ÝỲỴỶỸ"
    };
        public static string text_ConvertUnicode2Ascii(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            try
            {
                //Thay thế và lọc dấu từng char      
                for (int i = 1; i < VietNamChar.Length; i++)
                {
                    for (int j = 0; j < VietNamChar[i].Length; j++)
                        str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }
            }
            catch
            {

            }
            return str;
        }

        static string securityKey = "$$$$$";
        public static string text_EncryptMD5(string toEncrypt, bool useHashing = true)
        {

            string retVal = string.Empty;

            try
            {
                byte[] keyArray;

                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

                // Validate inputs 
                // If hashing use get hashcode regards to your key

                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(securityKey));
                    // Always release the resources and flush data
                    // of the Cryptographic service provide. Best Practice
                    hashmd5.Clear();
                }
                else
                {
                    keyArray = UTF8Encoding.UTF8.GetBytes(securityKey);
                }

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                // Set the secret key for the tripleDES algorithm
                tdes.Key = keyArray;
                // Mode of operation. there are other 4 modes.
                // We choose ECB (Electronic code Book)
                tdes.Mode = CipherMode.ECB;

                // Padding mode (if any extra byte added)

                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateEncryptor();
                // Transform the specified region of bytes array to resultArray
                byte[] resultArray =
                  cTransform.TransformFinalBlock(toEncryptArray, 0,
                  toEncryptArray.Length);

                // Release resources held by TripleDes Encryptor
                tdes.Clear();

                // Return the encrypted data into unreadable string format
                retVal = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                //throw new EncryptionException(EncryptionException.Code.EncryptionFailure, ex, MethodBase.GetCurrentMethod());
            }
            return retVal;
        }

        public static string text_DecryptMD5(string cipherString, bool useHashing = true)
        {

            string retVal = string.Empty;

            try
            {
                byte[] keyArray;

                byte[] toEncryptArray = Convert.FromBase64String(cipherString);

                if (useHashing)
                {
                    // If hashing was used get the hash code with regards to your key

                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(securityKey));
                    // Release any resource held by the MD5CryptoServiceProvider
                    hashmd5.Clear();
                }
                else
                {
                    // If hashing was not implemented get the byte code of the key
                    keyArray = UTF8Encoding.UTF8.GetBytes(securityKey);
                }
                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

                // Set the secret key for the tripleDES algorithm
                tdes.Key = keyArray;

                // Mode of operation. there are other 4 modes.
                // We choose ECB(Electronic code Book)
                tdes.Mode = CipherMode.ECB;

                // Padding mode(if any extra byte added)
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                // Release resources held by TripleDes Encryptor
                tdes.Clear();

                // Return the Clear decrypted TEXT
                retVal = UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                //throw new EncryptionException(EncryptionException.Code.DecryptionFailure, ex, MethodBase.GetCurrentMethod());
            }
            return retVal;
        }

        public static bool text_IsASCII(string value)
        {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        public static string text_EncodeHtml(string text)
        {
            char[] chars = HttpUtility.HtmlEncode(text).ToCharArray();
            StringBuilder result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (char c in chars)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        #endregion

        public static long get_KeyRandom()
        {
            Thread.Sleep(1);
            long _key = 0;
            int rid = new Random().Next(1000, 9999);
            string id = DateTime.Now.ToString("yyMMddHHmmssfff") + rid.ToString();
            long.TryParse(id, out _key);
            return _key;
        }
    }

    public class db<T>
    {
        // ManualResetEvent instances signal completion.
        private static ManualResetEvent updateDone = new ManualResetEvent(false);
        static SynchronizedDictionary<long, T> dic = new SynchronizedDictionary<long, T>() { };
        public static dbStatus Status = dbStatus.CLOSED;

        public static T[] All()
        {
            return dic.All();
        }

        public static T[] Query(Func<T, bool> where)
        {
            return dic.Query(where);
        }

        public static string grid_Item(Func<T, bool> where, int page_number, int page_size)
        {
            T[] a = dic.All().Where(where).ToArray();
            int count = a.Length;
            string s = string.Empty;
            if (dic.Count > 0)
                s = JsonConvert.SerializeObject(a.Page(page_number, page_size));
            return @"{ ""status"": ""success"",""message"": """ + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + @""",""page"": " + page_number.ToString() + @",""total"": " + count.ToString() + @",""records"": " + s + " }";
        }

        public static string grid_AllItem(int page_number, int page_size)
        {
            T[] a = dic.All();
            int count = a.Length;
            string s = string.Empty;
            if (dic.Count > 0)
                s = JsonConvert.SerializeObject(a.Page(page_number, page_size));
            return @"{ ""status"": ""success"",""message"": """ + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + @""",""page"": " + page_number.ToString() + @",""total"": " + count.ToString() + @",""records"": " + s + " }";
        }


        #region [ Cache ]

        public static void Cache(T[] a)
        {
            dic.AddArray("_key", a);
        }

        #endregion

        #region [ EXCEL ]

        public static void get_Excel_Temp(HttpResponse Response)
        {
            //object atype
            //if (atype == null) return new Dictionary<string, object>();
            //Type t = atype.GetType();

            Dictionary<string, string> dic_COL = new Dictionary<string, string>() { };
            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                //object value = prop.GetValue(atype, new object[] { });
                //dict.Add(prop.Name, value);
                var attr = prop.GetCustomAttributes(typeof(colAttr), false);
                if (attr != null && attr.Length > 0)
                {
                    colAttr col = attr.Cast<colAttr>().Single();
                    if (col != null)
                    {
                        dic_COL.Add(prop.Name, col.caption);
                    }
                }
            }

            //====================================================
            var wb = new HSSFWorkbook();

            ////create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "Mr Thinh - 0948 00 3456";
            wb.DocumentSummaryInformation = dsi;

            ////create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = t.Name;
            wb.SummaryInformation = si;

            //====================================================
            Sheet sheet1 = wb.CreateSheet(t.Name);
            var cHelp = wb.GetCreationHelper();
            HSSFCellStyle hStyle = (HSSFCellStyle)wb.CreateCellStyle();


            hStyle.BorderBottom = CellBorderType.MEDIUM;
            hStyle.FillBackgroundColor = HSSFColor.BLUE.index;
            hStyle.FillPattern = FillPatternType.SOLID_FOREGROUND;

            HSSFFont hFont = (HSSFFont)wb.CreateFont();
            hFont.Boldweight = (short)FontBoldWeight.BOLD;
            hFont.Color = HSSFColor.WHITE.index;
            hFont.FontHeightInPoints = 9;
            hStyle.SetFont(hFont);

            //sheet1.CreateRow(0).CreateCell(0).SetCellValue("Tệp tin dữ liệu mẫu để nạp lên hệ thống");
            Row headerTitle = sheet1.CreateRow(0);
            HSSFCell cellTitle = (HSSFCell)headerTitle.CreateCell(0);
            cellTitle.SetCellValue(cHelp.CreateRichTextString(("Tệp tin dữ liệu mẫu để nạp lên hệ thống")));
            cellTitle.CellStyle = hStyle;

            Row headerRowCOL = sheet1.CreateRow(1);
            int cellCount = 0;
            foreach (string str in dic_COL.Values.ToArray())
            {
                HSSFCell cellCOLTitle = (HSSFCell)headerRowCOL.CreateCell(cellCount);
                cellCOLTitle.SetCellValue(cHelp.CreateRichTextString((str)));
                cellCOLTitle.CellStyle = hStyle;

                sheet1.SetColumnWidth(cellCount, 20000);//300px 

                cellCount += 1;
            }

            //====================================================

            //int x = 1;
            //for (int i = 1; i <= 15; i++)
            //{
            //    Row row = sheet1.CreateRow(i);
            //    for (int j = 0; j < 15; j++)
            //    {
            //        row.CreateCell(j).SetCellValue(x++);
            //    }
            //}
            //int x = 1;
            //for (int i = 1; i <= 1; i++)
            //{
            //    Row row = sheet1.CreateRow(i);
            //    for (int j = 0; j < dic_COL.Count; j++)
            //    {
            //        row.CreateCell(j).SetCellValue(dic_COL.Values.ToArray()[j]);
            //    }
            //}
            //====================================================

            string filename = t.Name + ".xls";
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
            //Response.Clear();

            //Write the stream data of workbook to the root directory
            using (MemoryStream WriteToStream = new MemoryStream())
            {
                wb.Write(WriteToStream);
                Response.BinaryWrite(WriteToStream.GetBuffer());
            }

            Response.End();
            //    var prop = typeof(T).GetProperty("MyProperty");
            //    var attr = prop.GetCustomAttributes(typeof(colAttribute), false);
            //    colAttribute col = attr.Cast<colAttribute>().Single();
        }

        private static string jsonCell(string[] cs, int rid, int max, int col, Cell cel)
        {
            string v = cel == null ? string.Empty : cel.ToString();

            v = v.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty)
                .Replace(Environment.NewLine, string.Empty)
                .Replace("'", string.Empty).Replace(@"\s+", "_").Replace(" ", "_")
                .Replace("/", "-")
                .Replace(@"""", string.Empty).Replace("\\", string.Empty).Replace("_", " ").Trim();

            v = System.Text.RegularExpressions.Regex.Replace(v, @"^[a-zA-Z0-9@.-_]*$", string.Empty).Trim();

            return
             (col == 0 ? (@"{""recid"":" + rid.ToString() + ",") : string.Empty) +
             string.Format(@"""{0}"":""{1}""", cs[col], v) +
             (col == max - 1 ? "}," + Environment.NewLine : ",");
        }

        private static string jsonCell2(bool isRowFirst, int rid, int max, int col, Cell cel, List<string> lsColName, List<string> lsColType)
        {
            string val = cel == null ? (lsColType[col] == "string" ? string.Empty : "0") : cel.ToString();

            val = val
                .Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty)
                .Replace(Environment.NewLine, string.Empty)
                .Replace("'", string.Empty).Replace(@"\s+", " ")//.Replace(" ", "_")
                                                                //.Replace("/", "-")
                .Replace(@"""", string.Empty).Replace("\\", string.Empty)//.Replace("_", " ")
                .Trim();

            //val = System.Text.RegularExpressions.Regex.Replace(val, @"^[a-zA-Z0-9@.-_]*$", string.Empty).Trim();

            return
             (col == 0 ? (@"{""recid"":" + rid.ToString() + @",""_key"":" + db.get_KeyRandom().ToString() + ",") : ",") +

                    (lsColType[col] == "string" ? string.Format(@"""{0}"":""{1}""", lsColName[col], val) : string.Format(@"""{0}"":{1}", lsColName[col], val)) +

                    (col == max - 1 ? "}" + Environment.NewLine : string.Empty);
        }

        public static T[] convert_Excel_Import_DB(Stream ms)
        {
            // and optionally write the file to disk
            //var fileName = Path.GetFileName(file);
            //var path = Path.Combine(Server.MapPath("~/App_Data/Images"), fileName);
            //using (var fileStream = File.Create(path))
            //{
            //    stream.CopyTo(fileStream);
            //}

            List<string> ls = new List<string>() { };
            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                //object value = prop.GetValue(atype, new object[] { });
                //dict.Add(prop.Name, value);
                var attr = prop.GetCustomAttributes(typeof(colAttr), false);
                if (attr != null && attr.Length > 0)
                {
                    colAttr col = attr.Cast<colAttr>().Single();
                    if (col != null)
                    {
                        ls.Add(prop.Name);
                    }
                }
            }

            //====================================================

            StringBuilder bi = new StringBuilder("[");

            var hssfworkbook = new HSSFWorkbook(ms);

            Sheet sheet = hssfworkbook.GetSheetAt(0);
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            int r = 0;
            while (rows.MoveNext())
            {
                Row row = (HSSFRow)rows.Current;
                int kCol = row.LastCellNum;

                StringBuilder br = new StringBuilder();
                for (int i = 0; i < kCol; i++)
                    br.Append(jsonCell(ls.ToArray(), (r + 1), kCol, i, row.GetCell(i)));

                bi.Append(br.ToString());
                r++;
            }

            bi.Append(@"{}]");

            T[] a = new T[] { };
            try
            {
                a = JsonConvert.DeserializeObject<T[]>(bi.ToString());
            }
            catch { }

            return a;
        }

        public static T[] restore_Excel_Import_DB(Stream ms)
        {
            // and optionally write the file to disk
            //var fileName = Path.GetFileName(file);
            //var path = Path.Combine(Server.MapPath("~/App_Data/Images"), fileName);
            //using (var fileStream = File.Create(path))
            //{
            //    stream.CopyTo(fileStream);
            //}

            List<string> lsCOL = new List<string>() { };
            List<string> lsType = new List<string>() { };
            ////Type t = typeof(T);
            ////PropertyInfo[] props = t.GetProperties();
            ////foreach (PropertyInfo prop in props)
            ////{
            ////    //object value = prop.GetValue(atype, new object[] { });
            ////    //dict.Add(prop.Name, value);
            ////    var attr = prop.GetCustomAttributes(typeof(colAttribute), false);
            ////    if (attr != null && attr.Length > 0)
            ////    {
            ////        colAttribute col = attr.Cast<colAttribute>().Single();
            ////        if (col != null)
            ////        {
            ////            ls.Add(prop.Name);
            ////        }
            ////    }
            ////}

            //====================================================

            StringBuilder bi = new StringBuilder("[");

            var hssfworkbook = new HSSFWorkbook(ms);

            Sheet sheet = hssfworkbook.GetSheetAt(0);
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            int r = 0;
            while (rows.MoveNext())
            {
                Row row = (HSSFRow)rows.Current;
                int kCol = row.LastCellNum;

                switch (r)
                {
                    case 0:
                        for (int i = 0; i < kCol; i++)
                            lsType.Add(row.GetCell(i).ToString());
                        break;
                    case 1:
                        for (int i = 0; i < kCol; i++)
                            lsCOL.Add(row.GetCell(i).ToString());
                        break;
                    default:
                        StringBuilder br = new StringBuilder();
                        bool isRowFirst = r == 2;
                        br.Append((isRowFirst ? string.Empty : ","));
                        for (int i = 0; i < kCol; i++)
                            br.Append(jsonCell2(isRowFirst, (r + 1), kCol, i, row.GetCell(i), lsCOL, lsType));
                        bi.Append(br.ToString());
                        break;
                }
                r++;
            }

            bi.Append(@"]");
            string skk = bi.ToString();

            T[] a = new T[] { };
            try
            {
                a = JsonConvert.DeserializeObject<T[]>(bi.ToString());
            }
            catch { }

            return a;
        }

        #endregion

        #region [ LOG ] 

        static readonly object lockLOG = new object();
        private static void updateLog<T>(oRecordStatus itemStatus, T o, int recKey = 0)
        {
            string s = string.Empty;
            string fi = string.Format("{0}{1}.log", db.PathDirectory, Name);
            if (itemStatus == oRecordStatus.DELETE)
                s = string.Format("{0}{1}", (int)itemStatus, recKey);
            else
                s = string.Format("{0}{1}", (int)itemStatus, o.ToString());
            lock (lockLOG)
                File.AppendAllText(fi, s, Encoding.UTF8);
        }

        #endregion

        #region [ OPEN ]

        public static string Name = string.Empty;
        public static string PathFile
        {
            get
            {
                return string.Format("{0}{1}.db", db.PathDirectory, Name);
            }
        }

        public static void Open(bool isBackground = false)
        {
            Type ti = typeof(T);
            Name = typeof(T).Name;

            if (isBackground)
            {
                db.status_SET(Name, dbStatus.OPENING);
                //socket.Send(new wsData(wsType.DB_INIT, Name));
            }
            else
            {
                #region

                if (!Directory.Exists(db.PathDirectory)) Directory.CreateDirectory(db.PathDirectory);

                Dictionary<long, string> dicLine = new Dictionary<long, string>() { };
                Dictionary<long, T> dicLOG = new Dictionary<long, T>() { };
                List<long> lsInsert = new List<long>() { };
                List<long> lsUpdate = new List<long>() { };
                List<long> lsDelete = new List<long>() { };

                #region [ LOG ]

                string flog = string.Format("{0}{1}.log", db.PathDirectory, Name);
                if (File.Exists(flog))
                {
                    lock (lockLOG)
                    {
                        using (var sr = new StreamReader(flog))
                        {
                            string line; object id;
                            while ((line = sr.ReadLine()) != null)
                            {
                                int ks = 0;
                                if (int.TryParse(line[0].ToString(), out ks))
                                {
                                    oRecordStatus st = (oRecordStatus)ks;
                                    long _key = 0;
                                    switch (st)
                                    {
                                        case oRecordStatus.INSERT:
                                            T tn = (T)Activator.CreateInstance(ti, new object[] { line.Substring(1, line.Length - 1) });
                                            //T tn = (T)o.Invoke(new object[] { line });
                                            id = ti.GetProperty("_key").GetValue(tn, null);
                                            if (id != null)
                                                if (long.TryParse(id.ToString(), out _key))
                                                    if (_key > 0)
                                                    {
                                                        if (dicLOG.ContainsKey(_key))
                                                            dicLOG[_key] = tn;
                                                        else
                                                            dicLOG.Add(_key, tn);

                                                        if (lsInsert.IndexOf(_key) == -1)
                                                            lsInsert.Add(_key);
                                                    }
                                            break;
                                        case oRecordStatus.UPDATE:
                                            T tu = (T)Activator.CreateInstance(ti, new object[] { line.Substring(1, line.Length - 1) });
                                            id = ti.GetProperty("_key").GetValue(tu, null);
                                            if (id != null)
                                                if (long.TryParse(id.ToString(), out _key))
                                                    if (_key > 0 && lsDelete.IndexOf(_key) == -1)
                                                    {
                                                        if (dicLOG.ContainsKey(_key))
                                                            dicLOG[_key] = tu;
                                                        else
                                                            dicLOG.Add(_key, tu);

                                                        if (lsUpdate.IndexOf(_key) == -1)
                                                            lsUpdate.Add(_key);
                                                    }
                                            break;
                                        case oRecordStatus.DELETE:
                                            string key = line.Substring(1, line.Length - 1).Split(_const.db_charSplitColumn)[0];
                                            if (long.TryParse(key, out _key))
                                                if (_key > 0 && lsDelete.IndexOf(_key) == -1)
                                                    lsDelete.Add(_key);
                                            break;
                                    }
                                }
                            }
                        }

                        //File.Delete(flog);
                    }
                }

                #endregion

                #region [ DB ]

                if (File.Exists(PathFile))
                {
                    using (var sr = new StreamReader(PathFile))
                    {
                        string line; object id = null;
                        int _key = 0;
                        while ((line = sr.ReadLine()) != null)
                        {
                            _key = 0;
                            int sti = 0; int.TryParse(line[0].ToString(), out sti);
                            oRecordStatus str = (oRecordStatus)sti;
                            if (str == oRecordStatus.NONE)
                            {
                                T o = (T)Activator.CreateInstance(ti, new object[] { line });
                                if (o != null)
                                {
                                    id = ti.GetProperty("_key").GetValue(o, null);
                                    if (id != null)
                                        if (int.TryParse(id.ToString(), out _key))
                                            if (_key > 0)
                                            {
                                                dic.Add(_key, o);
                                                dicLine.Add(_key, line);
                                            }
                                }
                            }
                        }
                    }
                }
                else
                {
                    File.Create(PathFile);
                }

                #endregion

                #region [ UPDATE LOG - GET MAX _KEY ]

                bool hasUpdate = false;

                if (lsInsert.Count > 0)
                {
                    for (int k = 0; k < lsInsert.Count; k++)
                    {
                        long key = lsInsert[k];
                        if (dicLOG.ContainsKey(key))
                        {
                            T o = dicLOG[key];
                            if (dic.ContainsKey(key))
                                dic[key] = o;
                            else
                                dic.Add(key, o);

                            if (dicLine.ContainsKey(key))
                                dicLine[key] = o.ToString();
                            else
                                dicLine.Add(key, o.ToString());

                            if (!hasUpdate) hasUpdate = true;
                        }
                    }
                }

                if (lsUpdate.Count > 0)
                {
                    for (int k = 0; k < lsUpdate.Count; k++)
                    {
                        long key = lsUpdate[k];
                        if (dicLOG.ContainsKey(key))
                        {
                            if (dic.ContainsKey(key))
                                dic[key] = dicLOG[key];
                            else
                                dic.Add(key, dicLOG[key]);
                            if (!hasUpdate) hasUpdate = true;
                        }
                    }
                }

                if (lsDelete.Count > 0)
                {
                    for (int k = 0; k < lsDelete.Count; k++)
                    {
                        long key = lsDelete[k];
                        if (dicLine.ContainsKey(key))
                            dicLine.Remove(key);
                        if (dic.ContainsKey(key))
                            dic.Remove(key);
                        if (!hasUpdate) hasUpdate = true;
                    }
                }

                #endregion

                Status = dbStatus.OPENED_READ_WRITE;

                if (hasUpdate)
                {
                    string[] a = dicLine.OrderBy(k => k.Key).Select(x => x.Value).ToArray();
                    string sfile = string.Join(Environment.NewLine, a);
                    cacheHashtable.Set(Name + ".db", sfile);
                    //socket.Send(new wsData(wsType.DB_UPDATE_FILE, Name));
                }

                #endregion
            }
        }

        #endregion

        #region [ INSERT, UPDATE, REMOVE, COUNT ]

        public static int Count
        {
            get
            {
                return dic.Count;
            }
        }

        public static int UpdateOrInsert(T o, bool LOG = true)
        {
            Type ti = o.GetType();
            var id = ti.GetProperty("_key").GetValue(o, null);
            if (id != null)
            {
                int _key = 0;
                if (int.TryParse(id.ToString(), out _key))
                {
                    oRecordStatus st = oRecordStatus.UPDATE;
                    if (dic.ContainsKey(_key))
                        dic[_key] = o;
                    else
                    {
                        _key = dic.Count + 1;

                        MethodInfo mi = ti.GetMethod("setKey");
                        if (mi != null)
                            mi.Invoke(o, new object[] { _key });

                        dic.Add(_key, o);
                        st = oRecordStatus.INSERT;
                    }

                    if (LOG)
                        updateLog<T>(st, o);

                    return _key;
                }
            }

            return -1;
        }

        public static void Remove(int _key, bool LOG = true)
        {
            if (dic.Remove(_key) && LOG)
                updateLog<T>(oRecordStatus.DELETE, default(T), _key);
        }

        #endregion
    }


}
