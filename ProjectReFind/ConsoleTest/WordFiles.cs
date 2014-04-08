using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Code7248.word_reader;
using System.IO.Packaging;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Configuration;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ConsoleTest
{
    public class WordFiles
    {
        public string GetWordTextcontent(string file)
        {
            TextExtractor extractor = new TextExtractor(file);
            string text = extractor.ExtractText();
            return text;
        }

        public static void InsertWordText(string path, string filehash)
        {
            try
            {
                var pages = WordFiles.GetNoOfPagesDOC(path);
                
                if (!string.IsNullOrEmpty(filehash))
                {
                    string ConStr = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
                    SQLiteConnection con = new SQLiteConnection(ConStr);
                    SQLiteCommand slitecmd = null;
                    
                    WordFiles wf = new WordFiles();
                    string textcontent = wf.GetWordTextcontent(path);                    
                    if (con.State.ToString() == "Closed")
                    { con.Open(); }
                    slitecmd = new SQLiteCommand("insert into pages(file,page,text)values('" + filehash + "'," + pages + ",'" + textcontent.ToString().Replace("'", "''") + "')", con);
                    slitecmd.ExecuteNonQuery();
                    if (con.State.ToString() == "Open")
                    { con.Close(); }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured at the method InsertWordText and the exception is  " + ex.Message.ToString());
            }
            finally
            {

            }
        }

        public static int GetNoOfPagesDOC(string FileName)
        {

            CheckMSOffice.Program prog = new CheckMSOffice.Program();
            int num = 0;
            try
            {
                if (prog.IsMSOfficeInstalled())
                {
                    Microsoft.Office.Interop.Word.Application WordApp = new Microsoft.Office.Interop.Word.Application();
                    WordApp.Visible = false;
                    // give any file name of your choice. 
                    object fileName = FileName;
                    object readOnly = true;
                    object isVisible = false;
                    object savechanges = false;
                    //  the way to handle parameters you don't care about in .NET 
                    object missing = System.Reflection.Missing.Value;

                    //   Open the document that was chosen by the dialog 
                    Microsoft.Office.Interop.Word.Document aDoc = WordApp.Documents.Open(fileName, ref missing, ref readOnly, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref isVisible);
                    Microsoft.Office.Interop.Word.WdStatistic stat = Microsoft.Office.Interop.Word.WdStatistic.wdStatisticPages;
                    num = aDoc.ComputeStatistics(stat, ref missing);
                    aDoc.Close(ref savechanges, ref missing, ref missing);
                    WordApp.NormalTemplate.Saved = true;
                    WordApp.Quit(ref savechanges, ref missing, ref missing);

                    return num;
                }
                
            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
            return num;
        }

        //using openxml word library
        public static int CountPagesUsingOpenXML(string fileType, string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            try
            {
                if (file.Extension.ToUpper() != ".DOCX")
                    return 0;

                XmlDocument doc = new XmlDocument();
                FileStream fsd = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                Package wordDoc = Package.Open(fsd);
                Uri uriData = new Uri("/docProps/app.xml", UriKind.Relative);
                PackagePart part = wordDoc.GetPart(uriData);
                doc.Load(part.GetStream());
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("vt", "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes");
                nsMgr.AddNamespace("def", "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties");
                int count = 0;

                switch (fileType.ToUpper())
                {
                    case "DOCX":
                        XmlNode node1 = doc.SelectSingleNode("/def:Properties/def:Pages", nsMgr);
                        count = Convert.ToInt32(node1.InnerXml);
                        break;
                }

                return count;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                file = null;
            }
        }
    }
}
