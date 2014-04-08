using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using iTextSharp.text.pdf;
using System.Security.Permissions;
using CefSharp.WinForms;
using CefSharp;
using System.Management;
using System.Diagnostics;
using Microsoft.Win32;
using System.Printing;
using System.Collections;
using iTextSharp.text.pdf.parser;

namespace ConsoleTest
{
    public class Program
    {   
        /// <summary>
        /// Concurrent Dictionary - thread-safe key/value pairs collection that can be accessed by multiple threads
        /// </summary>
        private static ConcurrentDictionary<string, string> _openedFiles = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Time object - we will use this as our timed trigger to check for file locks
        /// </summary>
        private static Timer _timer;
                
        /// <summary>
        /// Lock object - this is used to lock up a section of the code and block it from other threads accessing it (use for checking file access)
        /// </summary>
        private static object _locker = new object();
        
        /// <summary>
        /// Delay Time
        /// </summary>
        private static TimeSpan _fileAccessCheckDelay = TimeSpan.FromMilliseconds(2000);

        public static DBManager dbmanger = new DBManager();

        [System.Security.Permissions.PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        static void Main(string[] args)
        {
            #region Regeditor
            //RegistryKey pRegKey = Registry.LocalMachine;
            //pRegKey = pRegKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\FileSystem");
            //Object val = pRegKey.GetValue("NtfsDisableLastAccessUpdate");
            //Console.WriteLine("NtfsDisableLastAccessUpdate Reg Editor value is " + val);
            //if (Convert.ToInt32(val).Equals(1))
            //    pRegKey.SetValue("NtfsDisableLastAccessUpdate", 0);
            #endregion                                                   

            CefSharp.Settings settings = new CefSharp.Settings();       
            settings.PackLoadingDisabled = true;
            CefSharp.CEF.Initialize(settings);
            CefSharp.CEF.RegisterJsObject("dbm", Program.dbmanger);

            //checks the type of layout of the pdf document
            //var layout = DocumentDetails.IsColumnLayout(@"C:\Sandbox\Kentico Document Database.pdf");

            double checkDelay = 0;

            // Load "FileAccessCheckDelay" application configuration setting (in milliseconds). If fails, default to 3000ms
            if(double.TryParse(ConfigurationManager.AppSettings["FileAccessCheckDelay"], out checkDelay))
                _fileAccessCheckDelay = TimeSpan.FromMilliseconds(checkDelay);

            // Initialize the time object, passing in reference of the function that it will execute ever X amount of seconds
            _timer = new Timer(CheckFileAccess, null, 15000, 30000);

            // Initialize FS watcher object, passing in the watch directory and filter (default C:\*.*)  
            FileSystemWatcher fs = new FileSystemWatcher();
            fs.InternalBufferSize = 15360;
            fs.IncludeSubdirectories = true;            
            fs.Path = ConfigurationManager.AppSettings["WatchDirectory"].ToString();
            fs.Filter = "*.pdf";
            fs.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Attach change event handler that will be fired when change occurs which satisies the above notification filters
            fs.Changed += new FileSystemEventHandler(OnChanged);
            fs.Created += new FileSystemEventHandler(OnCreated);
            fs.Deleted += new FileSystemEventHandler(OnDeleted);
            fs.Renamed += new RenamedEventHandler(OnRenamed);                        
            
            // We make sure to enable event raising on this particular FS Watcher instance
            fs.EnableRaisingEvents = true;

            Form1 f1 = new Form1();
            f1.ShowDialog();

            // Halt application exit - input of any character will exit the application.
            //Console.ReadLine();            

            // Some cleanup: gracefully shut down the timer, detaching the event handler and finally dispose of the FS watcher instance
            _timer.Dispose();
            fs.Changed -= OnChanged;
            fs.Created -= OnCreated;
            fs.Deleted -= OnDeleted;
            fs.Renamed -= OnRenamed;
            fs.Dispose();  
        }

        static void OnRenamed(object source, RenamedEventArgs e)
        {
             FileInfo fi = new FileInfo(e.FullPath);
             if (fi.Extension.ToUpper() == ".DOCX" || fi.Extension.ToUpper() == ".DOC" || fi.Extension.ToUpper() == ".PDF")
             {
                 Console.WriteLine("File: {0} renamed to {1}", e.OldName, e.Name);
                 FileOperations fo = new FileOperations();
                 //Save the event
                 fo.RecordFileEvents(e.FullPath, "Renamed", DateTime.Now);
             }
        }

        static void OnCreated(object sender, FileSystemEventArgs e)
        {           

             FileInfo fi = new FileInfo(e.FullPath);

             if (e.ChangeType.ToString() == "Created")
             {
                 FileStream fstream = null;
                 if (fi.Exists)
                 {
                     try
                     {
                         fstream = new FileStream(e.FullPath, FileMode.Open, FileAccess.ReadWrite);
                         if (fstream != null)
                         {
                             fstream.Dispose();
                             fstream.Close();
                             fstream = null;
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
             }

             if (fi.Extension.ToUpper() == ".DOCX" || fi.Extension.ToUpper() == ".DOC" || fi.Extension.ToUpper() == ".PDF")
             {
                 //var processes = Win32Processes.GetProcessesLockingFile(e.FullPath);
                 //foreach (var process in processes)
                 //{ Console.WriteLine(e.FullPath + " is accessed by the process " + process.ProcessName); }                 

                 Console.WriteLine("Pasted a new file at this directory {0}", e.FullPath);
                 var timestartend = DateTime.Now;
                 FileOperations fo = new FileOperations();
                 //Save the event
                 fo.RecordFileEvents(e.FullPath, "Created", timestartend);
             }
        }

        static void OnChanged(object sender, FileSystemEventArgs e)
        {
            FileInfo fi = new FileInfo(e.FullPath);

            if (fi.Extension.ToUpper() == ".DOCX" || fi.Extension.ToUpper() == ".DOC" || fi.Extension.ToUpper() == ".PDF")
            {
                //var processes = Win32Processes.GetProcessesLockingFile(e.FullPath);
                //foreach (var process in processes)
                //{ Console.WriteLine(e.FullPath + " is accessed by the process " + process.ProcessName); }
                

                if (_openedFiles.TryAdd(e.Name, e.FullPath + "*" + DateTime.Now.ToString("")))
                {
                    //Inserts the details into files table and pages if it is not indexed
                    int isresultinserted = FileOperations.InsertFileDetails(e.FullPath);
                    if (isresultinserted == 1)
                    { Console.WriteLine("File Details saved successfully into the Files table and the filename is {0}", e.Name); }
                    Console.WriteLine("[{0}] {1} opened", DateTime.Now.ToString("dd MMM yy hh:mm:ss"), e.Name);

                    var timestartend = DateTime.Now;

                    if (_openedFiles.Count > 0)
                    {
                        var dateinarray = _openedFiles.Where(m => m.Key == e.Name).FirstOrDefault().Value.Split("*".ToCharArray())[1]; ;
                        FileOperations fo = new FileOperations();
                        //Save the event
                        fo.RecordFileEvents(e.FullPath, "Opened", Convert.ToDateTime(dateinarray));
                    }
                }
            }
        }

        static void OnDeleted(object sender, FileSystemEventArgs e)
        {   
            FileInfo fi = new FileInfo(e.FullPath);
            if (fi.Extension.ToUpper() == ".DOCX" || fi.Extension.ToUpper() == ".DOC" || fi.Extension.ToUpper() == ".PDF")
            {
                var timestartend = DateTime.Now;
                FileOperations fo = new FileOperations();
                //Save the event
                fo.RecordFileEvents(e.FullPath, e.ChangeType.ToString(), timestartend);
            }
        }

        public static Dictionary<string, DateTime> GetPrintJobsCollection(string filename)
        {
            Dictionary<string,DateTime> printedjobs = new Dictionary<string,DateTime>();
            string searchQuery = "select * from win32_printjob WHERE Document like '%" + filename + "%'";
            ManagementObjectSearcher searchPrintJobs = new ManagementObjectSearcher(searchQuery);            
            ManagementObjectCollection prntJobCollection = searchPrintJobs.Get();
            foreach (ManagementObject prntJob in prntJobCollection)
            {
                System.String jobName = prntJob.Properties["Name"].Value.ToString();                
                char[] splitArr = new char[1];
                splitArr[0] = Convert.ToChar(",");
                string prnterName = jobName.Split(splitArr)[0];
                string documentName = prntJob.Properties["Document"].Value.ToString();                                                                 
                var date = Convert.ToDateTime(System.Management.ManagementDateTimeConverter.ToDateTime(prntJob.Properties["TimeSubmitted"].Value.ToString()));
                printedjobs.Add(documentName, date);    
            }
            return printedjobs;
        }

        public static void CheckFileAccess(object state)
        {            
            // Lock access to the below portion of the code
            lock (_locker)
            {                
                // Iterate through each item in the opened files array
                foreach (var file in _openedFiles)
                {  

                    FileStream fs = null;

                    string fullPath = string.Empty;
                    DateTime printeddoctime = new DateTime();

                    try
                    {

                        if (File.Exists(file.Value.Split("*".ToCharArray())[0]))
                        {
                            var filekeysplit = file.Key.Split("\\".ToCharArray());
                            var filekeyvalue = filekeysplit[filekeysplit.Length - 1];
                            Dictionary<string, DateTime> printedjobs = GetPrintJobsCollection(filekeyvalue);
                            printedjobs.TryGetValue(filekeyvalue, out printeddoctime);
                            if (printedjobs.ContainsKey(filekeyvalue) && (printeddoctime > DateTime.Now.AddSeconds(-30)))
                            { 
                                Console.WriteLine("Printed the document " + file.Key + " on " + printedjobs.FirstOrDefault(x => x.Key == file.Key).Value);
                                FileOperations fo = new FileOperations();
                                fo.RecordFileEvents(file.Value.Split("*".ToCharArray())[0].ToString(), "Printed", printedjobs.FirstOrDefault(x => x.Key == file.Key).Value);
                            }
                            
                            fs = new FileStream(file.Value.Split("*".ToCharArray())[0], FileMode.Open, FileAccess.Read, FileShare.None,100);                            

                            // Execution will reach below this point ONLY if the file resources are freed
                            // meaning our file has been closed!
                            fs.Close();
                            //Updates the timestop field in the events table
                            FileOperations.UpdateFileDetails(file.Value.Split("*".ToCharArray())[0].ToString());
                            Console.WriteLine("[{0}] {1} closed", DateTime.Now.ToString("dd MMM yy hh:mm:ss"), file.Key);                           
                            
                            // Remove the file from the opened files array.
                            _openedFiles.TryRemove(file.Key, out fullPath);
                        }
                    }
                    catch (IOException ioex)
                    {

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                    }
                    finally
                    {
                        fs = null;
                    }
                }
            }          
        }
    }
}
