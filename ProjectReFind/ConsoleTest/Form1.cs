using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.IO;
using System.Net;

namespace ConsoleTest
{
    public partial class Form1 : Form
    {
        CefSharp.WinForms.WebView web_view;

        public Form1()
        {
            InitializeComponent();            
            CefSharp.Settings settings = new CefSharp.Settings();
            settings.PackLoadingDisabled = true;

            if (CEF.Initialize(settings))
            {
                //CEF.RegisterJsObject("dbm", new DBManager());
                web_view = new CefSharp.WinForms.WebView();
                web_view.Dock = DockStyle.Fill;
                web_view.PropertyChanged += OnWebViewPropertyChanged;
                web_view.Show();
                toolStripContainer1.ContentPanel.Controls.Add(web_view);
            }           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void OnWebViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsBrowserInitialized":
                    if (web_view.IsBrowserInitialized)
                    {                      
                      string urlToNavigate = @"E:\Inklii VEKA Project\Frontend ananya work\PhpProject2\index.html";                      
                      web_view.Load(urlToNavigate);                              
                    }
                    break;
            }
        } 

    } 
}
