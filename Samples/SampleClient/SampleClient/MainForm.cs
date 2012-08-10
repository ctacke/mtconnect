using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenNETCF.MTConnect;

namespace SampleClient
{
    public partial class MainForm : Form
    {
        private EntityClient m_client;

        public MainForm()
        {
            InitializeComponent();

            agentAddress.Text = "192.168.10.239:5000";

            InitializeAgentTree();

            InitializeDataList();
            
            InitializePlot();
            
            InitializeDataItemWatcher();
        }
    }
}
