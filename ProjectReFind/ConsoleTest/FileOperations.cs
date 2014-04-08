using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using System.Configuration;
using System.Net;
using iTextSharp.text;
using System.Drawing;
using System.Drawing.Imaging;
using GhostscriptSharp;
using GhostscriptSharp.Settings;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO.Packaging;
using DocumentFormat.OpenXml.Packaging;
using System.Reflection;

namespace ConsoleTest
{
    public class FileOperations
    {       
        /// <summary>
        /// Insert the Hash code to the doc/docx files to the Tags attribute
        /// </summary>        
        public static void InsertHashToWord(string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            try
            {
                if (file.Extension.ToUpper() == ".DOCX")
                {
                    XmlDocument doc = new XmlDocument();
                    Package wordDoc = Package.Open(fileName);
                    FileHash fh = new FileHash();
                    var hashcode = fh.HashFile(fileName);
                    wordDoc.PackageProperties.Keywords = hashcode;
                }
            }
            catch (Exception)
            {


            }
            finally
            {
                file = null;
            }
        }

        /// <summary>
        /// Indexing the file for the first time
        /// Saves the thumbnail and the file to the Inklii folder with hash code
        /// </summary>
        public static int InsertFileDetails(string FilePath)
        {
            try
            {
                int fileexists = 0;
                FileHash fh = new FileHash();
                string HashCode = fh.HashFile(FilePath);
                SQLiteConnection con = new SQLiteConnection();
                con.ConnectionString = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;

                if (!string.IsNullOrEmpty(HashCode))
                {
                    FileInfo fi = new FileInfo(FilePath);
                    PdfReader pdf = null; int numberOfPages = 0;                    
                    var doctags = false;
                    switch (fi.Extension.ToUpper())
                    {
                        case ".PDF":
                            GetThumbnail(FilePath);
                            pdf = new PdfReader(FilePath);
                            numberOfPages = pdf.NumberOfPages;
                            //Check if the file exists in the Db
                            SQLiteCommand cmand = new SQLiteCommand("select Count(*) from files where file = '" + HashCode + "'", con);
                            if (con.State.ToString() == "Closed")
                            { con.Open(); }
                            SQLiteDataReader dr = cmand.ExecuteReader();
                            while (dr.Read())
                            { fileexists = Convert.ToInt32(dr[0]); }
                            //Extracting the PDF content 
                            string textcontent = PDFParser.InsertPDFText(FilePath);
                            break;
                        case ".DOCX":
                            numberOfPages = WordFiles.GetNoOfPagesDOC(FilePath);
                            doctags = IsHashExists(FilePath);
                            //WordFiles.InsertWordText(FilePath, HashCode);
                            break;
                        case ".DOC":
                            numberOfPages = WordFiles.GetNoOfPagesDOC(FilePath);
                            doctags = IsHashExists(FilePath);
                            //WordFiles.InsertWordText(FilePath,HashCode);
                            break;
                     }

                    var fsize = fi.Length;
                    string inkliifolder = ConfigurationManager.AppSettings["InkliiFolder"].ToString();

                    //Saving the PDF file to the Inklii folder
                    System.IO.File.Copy(FilePath, inkliifolder + HashCode + fi.Extension, true);

                    int result = 0;
                    if ((fileexists == 0 && fi.Extension.ToUpper() == ".PDF") || !doctags)
                    {
                        if (!doctags && (fi.Extension.ToUpper() == ".DOCX" || fi.Extension.ToUpper() == ".DOC"))
                        {
                            FileStream fstream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            WordprocessingDocument wdoc = WordprocessingDocument.Open(fstream, true);
                            wdoc.PackageProperties.Keywords = ConfigurationManager.AppSettings["DocTagName"].ToString() + ":" + HashCode;
                            wdoc.Close();
                        }

                        SQLiteCommand cmd = new SQLiteCommand("insert into files(file,pages,size,ocr,exclude) values('" + HashCode + "'," + numberOfPages + "," + fsize + "," + 12323223 + "," + 0 + ")", con);
                        if (con.State.ToString() == "Closed")
                        { con.Open(); }
                        result = result + cmd.ExecuteNonQuery();
                        if (con.State.ToString() == "Open")
                        { con.Close(); }
                     }

                    FileInfo fin = new FileInfo(FilePath);
                    if (result > 0  && fin.Extension.ToUpper() == ".PDF")
                    {
                        PDFParser pps = new PDFParser();
                        PDFParser.GenerateThumbsforPdf(FilePath);
                        pps.GetColourofImage(HashCode);
                        PDFParser.GetImagesandDiagrams(FilePath, HashCode);
                        PDFParser.AnnotationsToDb(FilePath,HashCode,"Insert");
                    }

                    return result;
                }
            }
            catch (System.IO.IOException ex)
            {
                return 0;
            }
            catch(Exception exc)
            {
                return 0;
            }
            finally
            {

            }
            return 0;
        }

        //Records the events of file like Open, Close, delete
        public void RecordFileEvents(string path, string type, DateTime timestartend)
        {
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
            SQLiteCommand cmd = null; SQLiteDataReader dr = null;
            try
            {
                string hashcode = "";
                if (type == "Deleted")
                {
                    cmd = new SQLiteCommand("select file from events where details like '%" + path + "%'", con);
                    var constatus = con.State;
                    if (con.State.ToString() == "Closed")
                    { con.Open(); }
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        hashcode = dr["file"].ToString();
                    }
                    if (!string.IsNullOrEmpty(hashcode))
                    { cmd = new SQLiteCommand("insert into events(file,type,pc,timestart,timestop,details)values('" + hashcode + "','" + type + "'," + 0 + ",'" + timestartend + "','" + DateTime.Now.ToString() + "','" + path + "')", con); }
                }
                else
                {
                    FileHash fh = new FileHash();
                    hashcode = fh.HashFile(path);
                    if (!string.IsNullOrEmpty(hashcode))
                    {
                        cmd = new SQLiteCommand("insert into events(file,type,pc,timestart,timestop,details)values('" + hashcode + "','" + type + "'," + 0 + ",'" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", timestartend) + "','" + DBNull.Value + "','" + path + "')", con);
                    }

                    if (IsHashExists(path))
                    {
                        FileInfo fi = new FileInfo(path);
                        if (fi.Extension.ToUpper() == ".PDF")
                        { PDFParser.AnnotationsToDb(path, hashcode, "Update"); }
                    }                    
                }

                if (con.State.ToString() == "Closed")
                { con.Open(); }
                cmd.ExecuteNonQuery();
                if (con.State.ToString() == "Open")
                { con.Close(); }
            }
            catch (IOException ex)
            {
                var testing = ex.ToString();
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                con.Dispose();
                con = null;
                cmd = null;
            }
        }

        /// <summary>
        /// Updates the timestop value when the file is being closed
        /// </summary>
        internal static void UpdateFileDetails(string path)
        {
            if (IsHashExists(path))
            {
                SQLiteConnection con = new SQLiteConnection();
                con.ConnectionString = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
                SQLiteCommand cmd = null;
                try
                {
                    FileHash fh = new FileHash();
                    string hashcode = fh.HashFile(path);
                    if (!string.IsNullOrEmpty(hashcode))
                    {
                        cmd = new SQLiteCommand("update events set timestop = '" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now) + "' where file='" + hashcode + "' and type='Opened'", con);
                        if (con.State.ToString() == "Closed")
                        { con.Open(); }
                        cmd.ExecuteNonQuery();
                        if (con.State.ToString() == "Open")
                        { con.Close(); }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
                finally
                {
                    con.Dispose();
                    cmd = null;
                }
             }
         }

        /// <summary>
        /// Creates the thumnbnail of the first page
        /// </summary>
        private static void GetThumbnail(string pdfpath)
        {
            try
            {
                FileHash fh = new FileHash();
                string hashcode = fh.HashFile(pdfpath);
                if (!string.IsNullOrEmpty(hashcode))
                {
                    string outputPath = Path.Combine(ConfigurationManager.AppSettings["InkliiFolder"].ToString(), hashcode + ".png");
                    GhostscriptWrapper.GenerateOutput(pdfpath, outputPath,
                         new GhostscriptSettings
                         {
                             Device = GhostscriptDevices.pngalpha,
                             Page = new GhostscriptPages
                             {
                                 Start = 1,
                                 End = 1,
                                 AllPages = false
                             },
                             Resolution = new Size
                             {
                                 Height = 72,
                                 Width = 72
                             },
                             Size = new GhostscriptPageSize
                             {
                                 Native = GhostscriptPageSizes.a2
                             }
                         });
                }
            }
            catch (IOException ex)
            {
                
            }
            catch(Exception exc)
            {
                Console.WriteLine("Exception occured at the function GetThumbnail and exception is" + exc.Message);
            }
            finally
            {                

            }
         }

        /// <summary>
        /// Checks if the file is already been indexed or not
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Boolean IsHashExists(string path)
        {
            FileHash fh = new FileHash();
            var hash = fh.HashFile(path);
            FileInfo finfo = new FileInfo(path);
            
            SQLiteCommand cmd = null; SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
            SQLiteDataReader dr = null;
            bool hasrows = false; var doctags = ""; bool exeqry = false;
            try
            {
                if (finfo.Extension.ToUpper() == ".PDF")
                { cmd = new SQLiteCommand("select file from files where file ='" + hash + "'", con); exeqry = true; }
                else if (finfo.Extension.ToUpper() == ".DOCX" || finfo.Extension.ToUpper() == ".DOC")
                {
                    FileStream fsr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    object fileName = path;
                    object readOnly = true;
                    object isVisible = false;
                    object savechanges = false;                    
                    object missing = System.Reflection.Missing.Value;
                    object DoNotSaveChanges = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                    Microsoft.Office.Interop.Word.Application wapp = new Microsoft.Office.Interop.Word.Application();
                    wapp.Visible = false;
                    Microsoft.Office.Interop.Word.Document wdoc = wapp.Documents.Open(fileName, ref missing, ref readOnly, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref isVisible);
                    object wordproperties = wdoc.BuiltInDocumentProperties;
                    Type typeDocBuiltInProps = wordproperties.GetType();
                    Object Keywordsprop = typeDocBuiltInProps.InvokeMember("Item", BindingFlags.Default | BindingFlags.GetProperty, null, wordproperties, new object[] { "Keywords" });
                    Type typeKeywordsprop = Keywordsprop.GetType();
                    doctags = typeKeywordsprop.InvokeMember("Value", BindingFlags.Default | BindingFlags.GetProperty, null, Keywordsprop, new object[] { }).ToString();
                    wdoc.Close(ref DoNotSaveChanges, ref missing, ref missing);
                    wapp.NormalTemplate.Saved = true;
                    wapp.Quit(ref missing, ref missing, ref missing);
                    if (!string.IsNullOrEmpty(doctags))
                    { cmd = new SQLiteCommand("select file from files where file ='" + doctags.Split(":".ToCharArray())[1] + "'", con); exeqry = true; }                    
                }
                
                if (exeqry)
                {                    
                    if (con.State.ToString() == "Closed")
                    { con.Open(); }
                    dr = cmd.ExecuteReader();
                    hasrows = dr.HasRows;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                cmd = null;
                if (dr != null)
                { dr.Dispose(); }
                con.Dispose();
            }

            return hasrows;
        }
       
    }
}
