using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Printing;
using System.Printing.IndexedProperties;
using System.Collections;
using System.IO;
using System.Management;

namespace ReFindPrinter
{
    class Program
    {

        //<summary>
        //print queue object
        //</summary>
        private static PrintQueue _pQueue;

        static void Main(string[] args)
        {
            string line = string.Empty;
            int i = 1;

            // Get the default print queue - this is usually the default printer selected

            _pQueue = System.Printing.LocalPrintServer.GetDefaultPrintQueue();

            if (_pQueue == null)
                return;
            Console.WriteLine("Queue name detected: " + _pQueue.FullName);

            while (line == "")
            {
                Console.WriteLine("Refreshing " + i);

                // refresh queue - we need to call Refresh() every time we attempt to read the queue.
                _pQueue.Refresh();

                // Get any print jobs currently in queue
                var jobCollection = _pQueue.GetPrintJobInfoCollection();

                // Iterate through print jobs
                foreach (PrintSystemJobInfo job in jobCollection)
                {
                    Console.WriteLine("==== Attempting to read " + job.JobName + "=====");

                    // This was a trial to attempt abd read the job stream (i'm certain that the file stream is passed onto the
                    // print queue - however I didn't see this occur. Not sure why? ..
                    if (job.JobStream != null)
                    {
                        using (StreamReader reader = new StreamReader(job.JobStream))
                        {
                            Console.WriteLine(reader.ReadToEnd());
                        }
                    }
                    else
                    {
                        Console.WriteLine("JobStream is null");
                    }

                    Console.WriteLine(" ");
                    Console.WriteLine("========" + job.JobName + "=========");
                    Console.WriteLine("========== { Properties } ==========");

                    // Iterate through the job properties (we get all kinds of info from here, FileName, username of who printed it
                    // etc... 
                    foreach (DictionaryEntry entry in job.PropertiesCollection)
                    {

                        // we need to cast the value object to PrintProperty class
                        PrintProperty prop = (PrintProperty)entry.Value;
                        var val = prop.Value != null ? prop.Value.ToString() : "";
                        var typ = prop.Value != null ? prop.Value.GetType().ToString() : "";
                        Console.WriteLine(prop.Name + " = (Type): " + typ + " ToString: " + val);
                    }
                }

                // Keep fetching queue information until some character is inputed
                line = Console.ReadLine();
                i++;
            }

            Console.WriteLine("Ending.... press any key");

            Console.ReadLine();
        }



    }
}
