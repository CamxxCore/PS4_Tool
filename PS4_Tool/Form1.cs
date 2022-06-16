using System.Windows.Forms;
using System.Net;
using PSXRPC;
using System;
using System.Diagnostics;

namespace PS4_Tool
{
    public partial class Form1 : Form
    {
        PSXConsole _console = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            var ipAddress = IPAddress.Parse("192.168.1.70");

            _console = new PSXConsole(ipAddress);

            if (_console.Connect())
            {
                MessageBox.Show("Connected to PS4!");

                Debug.WriteLine("Connected to PS4 at {0}", ipAddress.ToString());

                var bytes = _console.ReadBytes(0x11C69D9, 5);

                MessageBox.Show(BitConverter.ToString(bytes));

                var resolvedString = _console.ReadString(0x1857BB2);

                MessageBox.Show(resolvedString);
            }
            //...
        }
    }
}
