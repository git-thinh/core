using Ionic.Zip;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web; 

namespace core.Api
{ 
    public class excel 
    {

        static HSSFWorkbook hssfworkbook;
        private static DataSet dataSet1 = new DataSet();

        //switch(cell.CellType)
        //{
        //    case HSSFCellType.BLANK:
        //        dr[i] = "[null]";
        //        break;
        //    case HSSFCellType.BOOLEAN:
        //        dr[i] = cell.BooleanCellValue;
        //        break;
        //    case HSSFCellType.NUMERIC:
        //        dr[i] = cell.ToString();    //This is a trick to get the correct value of the cell. NumericCellValue will return a numeric value no matter the cell value is a date or a number.
        //        break;
        //    case HSSFCellType.STRING:
        //        dr[i] = cell.StringCellValue;
        //        break;
        //    case HSSFCellType.ERROR:
        //        dr[i] = cell.ErrorCellValue;
        //        break;
        //    case HSSFCellType.FORMULA:
        //    default:
        //        dr[i] = "="+cell.CellFormula;
        //        break;
        //}

        private static string jsonCell(int rid, int max, int col, Cell cel)
        {
            string v = cel == null ? string.Empty : cel.ToString();


            v = v.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty)
                .Replace(Environment.NewLine, string.Empty)
                .Replace("'", string.Empty).Replace(@"\s+", "_").Replace(" ", "_")
                .Replace("/", "-")
                .Replace(@"""", string.Empty).Replace("\\", string.Empty).Replace("_", " ").Trim();

            v = Regex.Replace(v, @"^[a-zA-Z0-9@.-_]*$", string.Empty).Trim();

            return
             (col == 0 ? (@"{""recid"":" + rid.ToString() + ",") : string.Empty) +
             string.Format(@"""c{0}"":""{1}""", col, v) +
             (col == max - 1 ? "}," + Environment.NewLine : ",");
        }
        
        //////public static void data2()
        //////{

        //////    try
        //////    {
        //////        foreach (string file in Request.Files)
        //////        {
        //////            var fileContent = Request.Files[file];
        //////            if (fileContent != null && fileContent.ContentLength > 0)
        //////            {
        //////                // get a stream
        //////                var ms = fileContent.InputStream;
        //////                // and optionally write the file to disk
        //////                //var fileName = Path.GetFileName(file);
        //////                //var path = Path.Combine(Server.MapPath("~/App_Data/Images"), fileName);
        //////                //using (var fileStream = File.Create(path))
        //////                //{
        //////                //    stream.CopyTo(fileStream);
        //////                //}


        //////                StringBuilder bi = new StringBuilder("[");

        //////                hssfworkbook = new HSSFWorkbook(ms);

        //////                Sheet sheet = hssfworkbook.GetSheetAt(0);
        //////                System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

        //////                int r = 0;
        //////                while (rows.MoveNext())
        //////                {
        //////                    if (r == 11)
        //////                        ;

        //////                    Row row = (HSSFRow)rows.Current;
        //////                    int kCol = row.LastCellNum;

        //////                    StringBuilder br = new StringBuilder();
        //////                    for (int i = 0; i < kCol; i++)
        //////                    {
        //////                        br.Append(jsonCell((r + 1), kCol, i, row.GetCell(i)));
        //////                    }

        //////                    bi.Append(br.ToString());
        //////                    r++;
        //////                }

        //////                bi.Append(@"{}]");

        //////                return Content(bi.ToString(), "application/json");

        //////            }
        //////        }
        //////    }
        //////    catch (Exception)
        //////    {
        //////        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //////        return Json("Upload failed");
        //////    }

        //////    return Json("File uploaded successfully");
        //////}
         
        ////////public static void data(string data)
        ////////{
        ////////    if (string.IsNullOrEmpty(data))
        ////////        return Content("[]", "application/json");

        ////////    StringBuilder bi = new StringBuilder("[");

        ////////    byte[] ab = System.Convert.FromBase64String(data);
        ////////    MemoryStream ms = new MemoryStream(ab);

        ////////    hssfworkbook = new HSSFWorkbook(ms);

        ////////    Sheet sheet = hssfworkbook.GetSheetAt(0);
        ////////    System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

        ////////    int r = 0;
        ////////    while (rows.MoveNext())
        ////////    {
        ////////        if (r == 11)
        ////////            ;

        ////////        Row row = (HSSFRow)rows.Current;
        ////////        int kCol = row.LastCellNum;

        ////////        StringBuilder br = new StringBuilder();
        ////////        for (int i = 0; i < kCol; i++)
        ////////        {
        ////////            br.Append(jsonCell(r + 1, kCol, i, row.GetCell(i)));
        ////////        }

        ////////        bi.Append(br.ToString());
        ////////        r++;
        ////////    }

        ////////    bi.Append(@"{}]");

        ////////    return Content(bi.ToString(), "application/json");
        ////////}
         
        ////////public static void Upload(HttpPostedFileBase photo)
        ////////{


        ////////    StringBuilder bi = new StringBuilder("[");
        ////////    if (photo != null && photo.ContentLength > 0)
        ////////    {

        ////////        hssfworkbook = new HSSFWorkbook(photo.InputStream);

        ////////        Sheet sheet = hssfworkbook.GetSheetAt(0);
        ////////        System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

        ////////        int r = 0;
        ////////        while (rows.MoveNext())
        ////////        {
        ////////            if (r == 11)
        ////////                ;

        ////////            Row row = (HSSFRow)rows.Current;
        ////////            int kCol = row.LastCellNum;

        ////////            StringBuilder br = new StringBuilder();
        ////////            for (int i = 0; i < kCol; i++)
        ////////            {
        ////////                br.Append(jsonCell(r + 1, kCol, i, row.GetCell(i)));
        ////////            }

        ////////            bi.Append(br.ToString());
        ////////            r++;
        ////////        }

        ////////    }

        ////////    bi.Append(@"{}]");

        ////////    return Content(bi.ToString(), "application/json");
        ////////}

         
        ////////public static void Upload99(HttpPostedFileBase photo)
        ////////{
        ////////    string directory = Server.MapPath("~/File");
        ////////    if (photo != null && photo.ContentLength > 0)
        ////////    {
        ////////        //var fileName = Path.GetFileName(photo.FileName);
        ////////        //photo.SaveAs(Path.Combine(directory, fileName));


        ////////        //read the template via FileStream, it is suggested to use FileAccess.Read to prevent file lock.
        ////////        //book1.xls is an Excel-2007-generated file, so some new unknown BIFF records are added. 
        ////////        //using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
        ////////        //{
        ////////        //    hssfworkbook = new HSSFWorkbook(file);
        ////////        //}

        ////////        hssfworkbook = new HSSFWorkbook(photo.InputStream);

        ////////        Sheet sheet = hssfworkbook.GetSheetAt(0);
        ////////        System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

        ////////        DataTable dt = new DataTable();

        ////////        bool buildDT = false;
        ////////        while (rows.MoveNext())
        ////////        {
        ////////            Row row = (HSSFRow)rows.Current;
        ////////            DataRow dr = dt.NewRow();
        ////////            int kCol = row.LastCellNum;

        ////////            if (buildDT == false)
        ////////            {
        ////////                for (int j = 0; j < kCol; j++)
        ////////                    dt.Columns.Add("c" + j.ToString());
        ////////                buildDT = true;
        ////////            }

        ////////            for (int i = 0; i < kCol; i++)
        ////////            {
        ////////                try
        ////////                {
        ////////                    Cell cell = row.GetCell(i);


        ////////                    if (cell == null)
        ////////                    {
        ////////                        dr[i] = null;
        ////////                    }
        ////////                    else
        ////////                    {
        ////////                        dr[i] = cell.ToString();
        ////////                    }
        ////////                }
        ////////                catch (Exception ex)
        ////////                {
        ////////                    dr[i] = string.Empty;
        ////////                }
        ////////            }
        ////////            dt.Rows.Add(dr);
        ////////        }
        ////////        dataSet1.Tables.Add(dt);

        ////////        //dataGridView1.DataSource = dataSet1.Tables[0];

        ////////        //ViewData["Message"] = fileName;
        ////////    }
        ////////    return RedirectToAction("Upload");
        ////////}
         
        public void ZipFiles(HttpContext Context)
        {
            HttpResponse Response = Context.Response;

            using (ZipFile zip = new ZipFile())
            {
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;

                //var file_collection = _repository.GetListOfFiles();
                //foreach (var file in file_collection)
                //{
                //    // location , folder name
                //    zip.AddFile(file.FileLocation, file.Name);
                //}

                Response.Clear();
                Response.BufferOutput = false;
                //sets the zip folder name with date and time
                string zipName = String.Format("FolderName_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                Response.ContentType = "application/zip";
                Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
                zip.Save(Response.OutputStream);
                Response.End();
            }
        }
    }
}
