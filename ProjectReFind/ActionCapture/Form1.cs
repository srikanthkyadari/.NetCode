using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace ActionCapture
{
    public partial class Form1 : Form
    {
        private FileSystemWatcher _fsw;
        private PlaygroundConsole _console;
        //C:\Users\d273178\AppData\Local\Microsoft\Windows\Temporary Internet Files
        //C:\Users\d273178\AppData\Local\Temp

        // THE ONE (CHROME) : C:\Users\d273178\AppData\Local\Google\Chrome\User Data\Default\Cache
        private string _sandboxDirectory = ConfigurationManager.AppSettings["Directory"];

        public Form1()
        {
            InitializeComponent();

            _console = new PlaygroundConsole(txtConsole);

            Bootstrap();
        }

        private void Bootstrap()
        {
            if (!Directory.Exists(_sandboxDirectory))
                Directory.CreateDirectory(_sandboxDirectory);

            _fsw = new FileSystemWatcher(_sandboxDirectory, "*.pdf");
            _fsw.IncludeSubdirectories = true;

            _fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            

            PlaygroundConsole.WriteLine("[x] Filesystem Watcher Initialized...");
            PlaygroundConsole.WriteLine("\t[x] Monitoring \"" + _sandboxDirectory + "\" directory and all subdirectories");

            _fsw.Changed += _fsw_Changed;
            PlaygroundConsole.WriteLine("\t[x] Monitoring for file changes...");

            _fsw.Deleted += _fsw_Deleted;
            PlaygroundConsole.WriteLine("\t[x] Monitoring for file deletions...");
            _fsw.Error += _fsw_Error;
            PlaygroundConsole.WriteLine("\t[x] Monitoring for file errors...");
            _fsw.Renamed += _fsw_Renamed;
            PlaygroundConsole.WriteLine("\t[x] Monitoring for file renames...");
            _fsw.Created += _fsw_Created;
            PlaygroundConsole.WriteLine("\t[x] Monitoring for file creates...");
            _fsw.EnableRaisingEvents = true;

            //_fsw.WaitForChanged(WatcherChangeTypes.Changed);
        }

        void _fsw_Created(object sender, FileSystemEventArgs e)
        {
            File.Copy(e.FullPath, @"C:\Sandbox\" + e.Name + ".pdf");

            PlaygroundConsole.WriteLine("Create event captured: [" + e.ChangeType.ToString() + "] - File: " + e.Name);
            LogMessage("FILE", e.ChangeType.ToString(), "File: " + e.Name);
        }

        void _fsw_Renamed(object sender, RenamedEventArgs e)
        {
            PlaygroundConsole.WriteLine("Rename event captured: [" + e.ChangeType.ToString() + "] - Old: " + e.OldName + ", New: " + e.Name);
            LogMessage("FILE", e.ChangeType.ToString(), "Old: " + e.OldName + ", New: " + e.Name);
        }

        void _fsw_Error(object sender, ErrorEventArgs e)
        {
            PlaygroundConsole.WriteLine("Error event captured: [" + e.GetException().Message + "]");
            LogMessage("FILE", "ERROR", e.GetException().Message);
        }

        void _fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            PlaygroundConsole.WriteLine("Delete event captured: [" + e.ChangeType.ToString() + "] - File: " + e.Name);
            LogMessage("FILE", e.ChangeType.ToString(), "File: " + e.Name);
           
        }

        void _fsw_Changed(object sender, FileSystemEventArgs e)
        {
            PlaygroundConsole.WriteLine("Changed event captured: [" + e.ChangeType.ToString() + "] - File: " + e.Name);
            LogMessage("FILE", e.ChangeType.ToString(), "File: " + e.Name);
        }

        private delegate void LogMessageDelegate(string category, string action, string description);

        public void LogMessage(string category, string action, string description)
        {
            if (InvokeRequired)
                this.Invoke(new LogMessageDelegate(LogMessage), new object[] { category, action, description });
            else
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = 0;
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, category));
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, action));
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, description));
                lstEvents.Items.Add(item);
            }
        }
    }
}
