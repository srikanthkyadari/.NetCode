using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ReFind;
using ReFind.Data;
using EasyHook;
using FileMonInject;
using System.Diagnostics;

namespace ReFindController
{
    public class FileMonInterface : MarshalByRefObject
    {
        public void IsInstalled(Int32 InClientPID)
        {
            //Console.WriteLine("FileMon has been installed in target {0}.\r\n", InClientPID);
        }

        public void OnCreateFile(Int32 InClientPID, String[] InFileNames)
        {
            for (int i = 0; i < InFileNames.Length; i++)
            {
                //Console.WriteLine(InFileNames[i]);
               // MessageBox.Show(InFileNames[i]);
            }
        }

        public void ReportException(Exception InInfo)
        {
            //MessageBox.Show("The target process has reported" +
                              //" an error:\r\n" + InInfo.ToString());
        }

        public void Ping()
        {
        }
    }
    public partial class Form1 : Form
    {
        static String ChannelName = null;

        public Form1()
        {
            InitializeComponent();
            
        }

        private void Bootstrap()
        {
            try
            {
                int processId = 0;

                foreach (Process p in Process.GetProcessesByName("WINWORD"))
                {
                    processId = p.Id;
                    break;
                }

                Config.Register("Doc File Monitor", "ReFindController.exe", "FileMonInject.dll");
                RemoteHooking.IpcCreateServer<FileMonInterface>(ref ChannelName, System.Runtime.Remoting.WellKnownObjectMode.SingleCall);
                RemoteHooking.Inject(processId, "FileMonInject.dll", "FileMonInject.dll", ChannelName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Bootstrap();
            //var sql = "select sqlite_version() as [Version]";
            //var ret = await DbProxy.ReadAsync<string>(sql, Make, null, CommandType.Text);
            //MessageBox.Show(ret);
        }

        private static Func<IDataReader, string> Make = m =>
            {
                return m["Version"].AsString();
            };
    }
}
