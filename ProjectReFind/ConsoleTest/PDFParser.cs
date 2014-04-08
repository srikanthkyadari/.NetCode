using System;
using System.IO;
using iTextSharp.text.pdf;
using System.Text;
using iTextSharp.text.pdf.parser;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.Collections.Generic;
using GhostscriptSharp;
using GhostscriptSharp.Settings;
using System.Threading;
using ReFind.BusinessLayer;

namespace ConsoleTest
{    
    public class PDFParser
    {
        //Inserts the PDF Text into pages table
        public static string InsertPDFText(String pdfPath)
        {            
            //String builder to build the insert query
            StringBuilder strPdfContent = new StringBuilder();
            bool exeqry = false; 
            PdfReader reader = new PdfReader(pdfPath);            
            SQLiteConnection con = null; SQLiteCommand slitecmd = null;
            try
            {
                FileHash fh = new FileHash();
                string filehash = fh.HashFile(pdfPath);
                var result = 0;
                if (!string.IsNullOrEmpty(filehash))
                {

                    //Building the insert query for all the pages
                    var sqllitecmdqry = "insert into pages('file','page','text') select ";
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        ITextExtractionStrategy objExtractStrategy = new SimpleTextExtractionStrategy();
                        string strLineText = PdfTextExtractor.GetTextFromPage(reader, i, objExtractStrategy);
                        strLineText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(strLineText)));
                        strPdfContent.Append(strLineText);
                        if (i < reader.NumberOfPages)
                        { sqllitecmdqry += "'" + filehash + "'," + i + ",'" + strPdfContent.ToString().Replace("'", "''") + "'" + " union all select "; exeqry = true; }
                        else if(i == reader.NumberOfPages)
                        { sqllitecmdqry += "'" + filehash + "'," + i + ",'" + strPdfContent.ToString().Replace("'", "''") + "' "; exeqry = true; }
                        strPdfContent.Clear();
                    }

                    if (exeqry)
                    {
                        string ConStr = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
                        con = new SQLiteConnection(ConStr);
                        if (con.State.ToString() == "Closed")
                        { con.Open(); }
                        var sqlqry = sqllitecmdqry;

                        slitecmd = new SQLiteCommand(sqlqry, con);
                        result = slitecmd.ExecuteNonQuery();
                        strPdfContent.Clear();

                        if (con.State.ToString() == "Open")
                        { con.Close(); }
                    }

                    return strPdfContent.ToString();
                }
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                if (reader != null)
                { reader.Dispose(); }
                con.Dispose();
            }

            return strPdfContent.ToString();
        }
        
        //Gets the colour from the PDF file and builds the sql query and passes it to
        //the insert colours function
        public void GetColourofImage(string hash)
        {
            string[] pdfthumbs = Directory.GetFiles(ConfigurationManager.AppSettings["ThumbsFolder"].ToString()); bool exeqry = false;      
            
            int pagenum =1;var sqlcmd = "insert into colours(file,page,colour)select ";
            foreach (var thumb in pdfthumbs)
            {
                Bitmap myBitmap = new Bitmap(thumb);
                List<string> colorslist = new List<string>();
                string pixelcolor = "";                

                for (int i = 0; i < myBitmap.Width; i++)
                {
                    for (int j = 0; j < myBitmap.Height; j++)
                    {
                        Color pixel = myBitmap.GetPixel(i, j);
                        pixelcolor = Classify(pixel);
                        if (!colorslist.Contains(pixelcolor))
                        {
                            colorslist.Add(pixelcolor);
                        }
                    }
                }
                
                foreach (var color in colorslist)
                {
                    sqlcmd += "'" + hash + "'," + pagenum + ",'" + color + "'" + " union all select ";
                    exeqry = true;
                }

                pagenum++;
            }

            if (exeqry)
            {
                sqlcmd = sqlcmd.Substring(0, (sqlcmd.Length - 17));
                InsertColorstoDb(sqlcmd);
            }
        }               

        /// <summary>
        /// Classifying to the RGB colours
        /// </summary>
        public string Classify(Color c)
        {
            float hue = c.GetHue();
            float sat = c.GetSaturation();
            float lgt = c.GetBrightness();

            if (lgt < 0.2) return "Blacks";
            if (lgt > 0.8) return "Whites";

            if (sat < 0.25) return "Grays";

            if (hue < 30) return "Reds";
            if (hue < 90) return "Yellows";
            if (hue < 150) return "Greens";
            if (hue < 210) return "Cyans";
            if (hue < 270) return "Blues";
            if (hue < 330) return "Magentas";
            return "Reds";
        }

        /// <summary>
        /// Generating the thumbnails for the PDF to read the colours
        /// </summary>
        internal static void GenerateThumbsforPdf(string pdfpath)
        {           
            string[] pdfthumbs = Directory.GetFiles(ConfigurationManager.AppSettings["ThumbsFolder"].ToString());

            foreach (var thumb in pdfthumbs)
            {
                try
                {
                    FileInfo fi = new FileInfo(thumb);
                    if (fi.Exists)
                    {
                        fi.Delete();
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    
                }
            }


            try
            {
                if (FileOperations.IsHashExists(pdfpath))
                {
                    PdfReader reader = new PdfReader(pdfpath);
                    string outputPath = "";
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {

                        outputPath = Path.Combine(ConfigurationManager.AppSettings["ThumbsFolder"].ToString(), i + ".jpg");
                        
                        GhostscriptWrapper.GeneratePageThumb(pdfpath, outputPath, i, 40, 60);                        
                    }
                }
            }
            catch (IOException ex)
            {

            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception occured at the function GetThumbnail and exception is" + exc.Message);
            }
            finally
            {

            }
        }

        /// <summary>
        //Inserting the colours into the Db
        /// </summary>
        internal void InsertColorstoDb(string sqllitecmd)
        {
            SQLiteConnection con = null; SQLiteCommand slitecmd = null;
            string ConStr = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;

            try
            {                
                con = new SQLiteConnection(ConStr);
                slitecmd = new SQLiteCommand(sqllitecmd, con);
                if (con.State.ToString() == "Closed")
                { con.Open(); }
                slitecmd.ExecuteNonQuery();
                if (con.State.ToString() == "Open")
                { con.Clone(); }
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception occured in the function InsertColorstoDb and the Exception is " + exp.Message.ToString());
            }
            finally
            {
                con.Dispose();
                slitecmd.Dispose();
            }
        }

        /// <summary>
        /// Function which inserts the Chart count and the page number into Db
        /// </summary>
        internal static int GetImagesandDiagrams(string pdfpath,string hashcode)
        {
            var reader = new PdfReader(pdfpath);

            List<string> imagesandtables = new List<string>();

            var slitecmdqry = "insert into charts(file,number,page) select ";
            Boolean chartsexists = false; int rowsaffected = 0;

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                int diagrams = 0;
                PdfReader pdf = new PdfReader(pdfpath);
                PdfDictionary pg = pdf.GetPageN(i);
                PdfDictionary res = (PdfDictionary)PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES));
                PdfDictionary xobj = (PdfDictionary)PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
                
                if (xobj == null) { return rowsaffected; }
                
                foreach (PdfName name in xobj.Keys)
                {
                    PdfObject obj = xobj.Get(name);
                    if (!obj.IsIndirect()) { continue; }
                    PdfDictionary tg = (PdfDictionary)PdfReader.GetPdfObject(obj);
                    PdfName type = (PdfName)PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));                  
                    if (PdfName.IMAGE.Equals(type) || PdfName.FORM.Equals(type) || PdfName.GROUP.Equals(type) || PdfName.FIGURE.Equals(type) || PdfName.IMAGEB.Equals(type) || PdfName.IMAGEB.Equals(type) || PdfName.IMAGEC.Equals(type) || PdfName.IMAGEI.Equals(type))
                    { diagrams++; }                    
                }
                
                if (diagrams > 0)
                {                    
                    slitecmdqry += "'" + hashcode + "'," + diagrams + "," + i + " union all select ";
                    chartsexists = true;
                }
            }
            
            if (chartsexists)
            {
                string ConStr = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
                SQLiteConnection con = new SQLiteConnection(ConStr);
                if (con.State.ToString() == "Closed")
                { con.Open(); }
                SQLiteCommand slitecmd = new SQLiteCommand(slitecmdqry.Substring(0,slitecmdqry.Length-17), con);
                rowsaffected = slitecmd.ExecuteNonQuery();
                if (con.State.ToString() == "Open")
                { con.Close(); }
            }

            return rowsaffected;
        }

        //Get the Annotation and insert/update it to Db
        internal static int AnnotationsToDb(string pdfpath, string hash, string hashexists= null)
        {
            var res = 0;

            try
            {
                PdfReader reader = new PdfReader(pdfpath);
                string sqlitecmdqry = "insert into annotations(hash,page,text) select ";
                StringBuilder sbuild = new StringBuilder();
                bool exeqry = false;
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfDictionary pg = reader.GetPageN(i);
                    PdfArray annotsarray = pg.GetAsArray(PdfName.ANNOTS);

                    if (annotsarray != null || annotsarray.Length != 0)
                    {
                        for (int j = 0; j < annotsarray.Size; ++j)
                        {
                            PdfDictionary curAnnot = annotsarray.GetAsDict(j);
                            if (curAnnot.Get(PdfName.SUBTYPE).Equals(PdfName.TEXT))
                            {
                                string Title = curAnnot.GetAsString(PdfName.T).ToString();
                                string Content = curAnnot.GetAsString(PdfName.CONTENTS).ToString();
                                sbuild.Append("'" + hash + "'," + i + ",'" + Title + "*" + Content + "'" + " union all select ");
                                exeqry = true;
                            }
                        }

                        sqlitecmdqry += sbuild.ToString();
                        sbuild.Clear();
                    }
                }
                
                if (exeqry)
                {
                    sqlitecmdqry = sqlitecmdqry.Substring(0, (sqlitecmdqry.Length - 17));
                    SQLiteConnection con = new SQLiteConnection(DBManager.GetConnectionString());
                    SQLiteCommand slitecmd = null;
                    switch (hashexists)
                    {
                        case "Insert":
                            slitecmd = new SQLiteCommand(sqlitecmdqry, con);
                            if (con.State.ToString() == "Closed")
                            { con.Open(); }
                            res = slitecmd.ExecuteNonQuery();
                            if (con.State.ToString() == "Open")
                            { con.Close(); }
                            break;

                        case "Update":
                            SQLiteCommand scommand = new SQLiteCommand("delete from annotations where file ='" + hash + "'", con);
                            if (con.State.ToString() == "Closed")
                            { con.Open(); }
                            var delrows = scommand.ExecuteNonQuery();
                            slitecmd = new SQLiteCommand(sqlitecmdqry, con);
                            if (con.State.ToString() == "Closed")
                            { con.Open(); }
                            res = slitecmd.ExecuteNonQuery();
                            break;
                    }
                }

                return res;
            }
            catch (Exception exc)
            {
                
            }
            finally
            {

            }

            return res;
         }

        /// <summary>
        /// Counts the number of words from text input
        /// </summary>
        /// <param name="text"></param>
        /// <returns> int </returns>
        public static int GetWordCountFromString(string text)
        {            
            if (string.IsNullOrEmpty(text))
                return 0;

            //Count the words
            return System.Text.RegularExpressions.Regex.Matches(text, "\\S+").Count;
        }
    }


    public class DocumentDetails : Document
    {
        public string GetOCR(string pdfpath)
        {
            PdfReader pdf = new PdfReader(pdfpath); Dictionary<int, string> imgsize = new Dictionary<int,string>();
            PdfDictionary pg = null; PdfObject imgobj = null; var pdftext = ""; Dictionary<int,string> ocrtext = new Dictionary<int,string>();

            try
            {
                for (int i = 1; i <= pdf.NumberOfPages; i++)
                {
                    pg = pdf.GetPageN(i);
                    imgobj = FindImageInPDFDictionary(pg);

                    ITextExtractionStrategy objExtractStrategy = new SimpleTextExtractionStrategy();
                    string strLineText = PdfTextExtractor.GetTextFromPage(pdf, i, objExtractStrategy);
                    strLineText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(strLineText)));
                    pdftext += strLineText;

                    if (imgobj != null)
                    {
                        int XrefIndex = Convert.ToInt32(((PRIndirectReference)imgobj).Number.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        PdfObject pdfObj = pdf.GetPdfObject(XrefIndex);
                        PdfStream pdfStrem = (PdfStream)pdfObj;
                        byte[] bytes = PdfReader.GetStreamBytesRaw((PRStream)pdfStrem);
                        iTextSharp.text.Image image1 = iTextSharp.text.Image.GetInstance(bytes);
                        iTextSharp.text.Rectangle pdfwidthheight = pdf.GetPageSize(i);                        
                        System.Drawing.Image ocrimg = Image.FromStream(new MemoryStream(bytes),false);                        
                        
                        if((image1.Height > pdfwidthheight.Height/2)  && (image1.Width > pdfwidthheight.Width/2))
                        {
                            imgsize.Add(i, image1.Width + "*" + image1.Height);
                            var res = System.Text.Encoding.UTF8.GetString(bytes);
                            //tesseract.TesseractProcessor processor = new tesseract.TesseractProcessor();
                            //processor.Init();
                            //processor.Apply(ocrimg);
                        }
                    }                   
                }

                var wordcount = PDFParser.GetWordCountFromString(pdftext);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            finally
            {
                pdf.Dispose();
                pg = null; imgobj = null;
            }
            
            return string.Empty;
        }

        private static PdfObject FindImageInPDFDictionary(PdfDictionary pg)
        {
            PdfDictionary res =
                (PdfDictionary)PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES));

            PdfDictionary xobj =
              (PdfDictionary)PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
            if (xobj != null)
            {
                foreach (PdfName name in xobj.Keys)
                {

                    PdfObject obj = xobj.Get(name);
                    if (obj.IsIndirect())
                    {
                        PdfDictionary tg = (PdfDictionary)PdfReader.GetPdfObject(obj);

                        PdfName type =
                          (PdfName)PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));

                        //image at the root of the pdf
                        if (PdfName.IMAGE.Equals(type))
                        {
                            return obj;
                        }// image inside a form
                        else if (PdfName.FORM.Equals(type))
                        {
                            return FindImageInPDFDictionary(tg);
                        } //image inside a group
                        else if (PdfName.GROUP.Equals(type))
                        {
                            return FindImageInPDFDictionary(tg);
                        }

                    }
                }
            }

            return null;
        }


        internal static Boolean IsColumnLayout(string path)
        {
            //@"D:\PDFs\Tulving_and_Thomson_1973-Encoding_Specificity_and_Retrieval_Processes_in_Episodic_Memory.pdf";
            try
            {
                PdfReader pdfreader = new PdfReader(path);
                StringBuilder strPdfContent = new StringBuilder(); int numberofcolumnlayouts = 0;
                int checklayoutpages = pdfreader.NumberOfPages > 10 ? 10 : pdfreader.NumberOfPages;

                for (int i = 1; i <= checklayoutpages; i++)
                {
                    var count = 0; int totallinesinpage = 0;
                    ITextExtractionStrategy objExtractStrategy = new SimpleTextExtractionStrategy();
                    string strLineText = PdfTextExtractor.GetTextFromPage(pdfreader, i, objExtractStrategy);
                    strLineText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(strLineText)));
                    strPdfContent.Append(strLineText);
                    var stringcontent = strPdfContent.ToString();
                    using (StringReader sread = new StringReader(stringcontent))
                    {                        
                        var linetext = "";
                        while ((linetext = sread.ReadLine()) != null)
                        {
                            totallinesinpage++;
                            int wordsineachline = PDFParser.GetWordCountFromString(linetext);
                            if (wordsineachline <= 11 && wordsineachline > 0)
                            { count++; }
                        }
                    }

                    if (count > totallinesinpage / 2)
                    {
                        numberofcolumnlayouts++;
                    }
                }

                if (numberofcolumnlayouts >= checklayoutpages / 2)
                { return true; }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {

            }
            return false;
        }


    }

}