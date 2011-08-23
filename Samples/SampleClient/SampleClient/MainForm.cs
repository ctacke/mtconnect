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

            InitializeAgentTree();

            InitializeDataList();
            
            InitializePlot();
            
            InitializeDataItemWatcher();
        }
    }
}
