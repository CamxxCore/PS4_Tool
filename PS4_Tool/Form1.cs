using System.Windows.Forms;
using System.Net;
using PSXRPC;
using System;
using System.Diagnostics;
using JekyllLibrary.Library;

namespace PS4_Tool
{
    public partial class Form1 : Form
    {
        PSXConsole _console = null;

        private readonly IPAddress _ipAddress = IPAddress.Parse("192.168.1.70");

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
             var jekyllInstance = new JekyllInstance();

            _console = new PSXConsole(_ipAddress);

            if (_console.Connect())
            {
                jekyllInstance.Reader = _console;

                JekyllStatus status = jekyllInstance.LoadGame();

                if (status == JekyllStatus.Success)
                {
                    Console.WriteLine("Jekyll Initialized.");

                    foreach (GameXAsset xasset in jekyllInstance.XAssets)
                    {
                        xasset.XAssetPool.Export(xasset, jekyllInstance);
                    }
                }
            }

            /* var ipAddress = IPAddress.Parse("192.168.1.70");

             _console = new PSXConsole(ipAddress);

             if (_console.Connect())
             {
                 Debug.WriteLine("Connected to PS4 at {0}", ipAddress.ToString());

                 label2.Text = "Online";

                var bytes = _console.ReadBytes(_console.GetBaseAddress() + 0x54B4C40, 0x1000);

                System.IO.File.WriteAllBytes("assetpools.bin", bytes);

               // MessageBox.Show(_console.GetModuleBase().ToString("X"));

                /* var bytes = _console.ReadBytes(0x11C69D9, 5);

                 MessageBox.Show(BitConverter.ToString(bytes));

                 var resolvedString = _console.ReadString(0x1857BB2);

                 MessageBox.Show(resolvedString);
             }*/

        }

        protected override void OnClosed(EventArgs e)
        {
            _console?.Dispose();

            base.OnClosed(e);
        }
    }
}
