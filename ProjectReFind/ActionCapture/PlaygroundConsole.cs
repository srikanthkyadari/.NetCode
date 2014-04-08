using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActionCapture
{
    public class PlaygroundConsole
    {
        private static TextBox _tb;
        private delegate void WriteLineDelegate(string str);

        public PlaygroundConsole(TextBox textboxControl)
        {
            _tb = textboxControl;
        }

        public static void Write(string str)
        {
            if (_tb.InvokeRequired)
                _tb.Invoke(new WriteLineDelegate(WriteLine), new object[] { str });
            else
            {
                if (_tb != null)
                    _tb.Text = str;
            }
        }

        public static void WriteLine(string str)
        {
            if (_tb.InvokeRequired)
                _tb.Invoke(new WriteLineDelegate(WriteLine), new object[] { str });
            else
            {
                if (_tb != null)
                    _tb.Text = _tb.Text + Environment.NewLine + str;
            }
        }
    }
}
