using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitMiracle.Docotic.Pdf;
using System.Collections;
using System.Text.RegularExpressions;



namespace ReFindPdfExtractor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           // var classifier = CRFClassifier.getClassifier("..");
            //var x = edu.stanford.nlp.ling.CoreAnnotations
        }

        private void button1_Click(object sender, EventArgs e)
        {

            // Open the PDF Document
            using (PdfDocument document = new PdfDocument(txtFile.Text))
            {
                //document.
                //PdfInfo info = document.Info;
                //info.
                //PdfPage page = document.Pages[0];
                    
                //var txt = page.GetText();

                //string[] arr = txt.Split(Environment.NewLine.ToCharArray());

                listBox1.Items.Clear();
                //page.Widgets[0].
               // PdfTextAnnotation textAnnotation = page.Widgets[0] as PdfTextAnnotation;
               // textAnnotation.

                // Iterate through the document pages
                foreach (PdfPage p in document.Pages)
                {

                    // Check for document objects (widgets)
                    foreach (PdfWidget widget in p.Widgets)
                    {
                        // of type text annotations (we can check for variety of different objects i.e. images to process
                        if (widget.Type == PdfWidgetType.TextAnnotation)
                        {
                            PdfTextAnnotation annotation = widget as PdfTextAnnotation;
                            listBox1.Items.Add(annotation.Contents);
                        }                        
                    }
                }

                //foreach (string item in arr)
                //{
                //    if (String.IsNullOrEmpty(item))
                //        continue;

                //    listBox1.Items.Add(item);

                //    //if(!Regex.IsMatch(item, txtRegExp.Text))
                //    //    continue;

                //    //listBox1.Items.Add("Possible Name: " + item);
                //}



            }
        }
    }
}
