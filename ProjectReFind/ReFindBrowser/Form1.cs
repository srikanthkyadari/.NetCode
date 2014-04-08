using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReFindBrowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            webRefind.DocumentCompleted += webRefind_DocumentCompleted;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            webRefind.Navigate(txtBar.Text);
        }

        void webRefind_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            lblLoad.Text = e.Url.ToString();
            //txtOutputText.Text = webRefind.DocumentText;
            txtOutputText.Text = webRefind.Document.Body.InnerText;
        }

    }
}
