using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdfSniffer
{
    /// <summary>
    /// PORT 80 snifer
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Data array
        /// </summary>
        static byte[] arrRes = new byte[1024 * 64];

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void OnClientReceive(IAsyncResult res)
        {
            Socket sock = (Socket)res.AsyncState;
            //IPEndPoint ep = sock.RemoteEndPoint as IPEndPoint;

            // End recieve
            int count = sock.EndReceive(res);
            if (count >= 40)
            {

                //Get the data
                string s = Encoding.UTF8.GetString(arrRes, 40, count - 40);
                string bin = BitConverter.ToString(arrRes, 40, count - 40);


                if (s.StartsWith("GET"))
                    textBox1.Text = "DATA: " + s + " - " + bin;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] input = new byte[] { 1 };
            byte[] buffer = new byte[4096];
            try
            {
                // Initialize a socket with an IPV4 address scheme
                // Raw socket type will allow you to "sniff" LOL, the underlying TCP port, i.e. 80,
                // Without locking up the port 80 (TCP)
                // Protocol type IP
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);

                // Bind the socket to port 80 
                s.Bind(new IPEndPoint(IPAddress.Broadcast, 80));
                s.IOControl(IOControlCode.ReceiveAll, input, null);

                // Asynchronously receive data, calling a callback function OnClientRecieve which will handle the request.
                s.BeginReceive(arrRes, 0, arrRes.Length, SocketFlags.None, new AsyncCallback(OnClientReceive), s);

                // Thread notification once data is recieved
                System.Threading.ManualResetEvent res = new System.Threading.ManualResetEvent(false);
                res.WaitOne();
            }
            catch (Exception ex) {
                textBox1.Text = "ERROR: " + ex.Message;
            }
        }
    }
}
