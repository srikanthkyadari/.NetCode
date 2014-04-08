using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Office.Interop.Word;

namespace ReFindWordAddin
{

    // An attempt to create a ReFind WORD addin - proof of concept worked - able to create ReFind button that is loaded as a
    // new toolbar item in MS Word.

    public partial class ThisAddIn
    {
        private Office.CommandBar _bar;
        private Office.CommandBarButton _button;
       
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            PrepareToolbar();
            Application.DocumentOpen += Application_DocumentOpen;
        }

        void Application_DocumentOpen(Word.Document Doc)
        {
            try
            {
               
                //Word.Range rng = Doc.Range(0, 0);
                //rng.Text = "ReFind!!!";
                //rng.Select();
               // Word.Range rng = Doc.Range(missing, 300);

                foreach (InlineShape shape in Doc.InlineShapes)
                {
                    if (shape.Type == WdInlineShapeType.wdInlineShapePicture)
                    {
                        //shape.
                    }
                }

                //foreach (ContentControl ctrl in Doc.InlineShapes) //Doc.ContentControls)
                //{
                //    if (ctrl.Type == Word.WdContentControlType.wdContentControlPicture)
                //    {
                //        PictureContentControl tempCtrl = (PictureContentControl)ctrl;
                //        Image img = tempCtrl.Image;
                //        CheckImageColors(img);
                //    }
                //}
            }
            catch (Exception ex)
            {
            }
        }

        private void CheckImageColors(Image img)
        {
            //img.r
        }

        private void PrepareToolbar()
        {
            try
            {
                _bar = Application.CommandBars["ReFind"];
            }
            catch (ArgumentException)
            {
            }

            if (_bar == null)
                _bar = Application.CommandBars.Add("ReFind", 1, missing, true);


            _button = (Office.CommandBarButton)_bar.Controls.Add(1, missing, missing, missing, missing);
            _button.Style = Office.MsoButtonStyle.msoButtonCaption;
            _button.Caption = "ReFind IT!";
            _button.DescriptionText = "ReFind Toolbar";
           
            _button.Tag = Guid.NewGuid().ToString();
            _button.Click += _button_Click;
            
            _bar.Visible = true;
        }

        void _button_Click(Office.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            MessageBox.Show("Message", "Hello from ReFind!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
