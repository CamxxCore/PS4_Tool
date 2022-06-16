using System.Windows.Forms;
using System.Net;
using PSXRPC;

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
            _console = new PSXConsole(IPAddress.Parse("192.168.1.70"));
            _console.Connect();
            //...
        }
    }
}
