using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenNETCF.MTConnect;

namespace SampleClient
{
    public partial class MainForm : Form
    {
        private void connect_Click(object sender, EventArgs e)
        {
            try
            {
                m_client = new EntityClient(agentAddress.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            connect.Enabled = false;

            // probe the client for devices
            var devices = m_client.Probe();

            PopulateAgentTree(devices);
        }
    }
}
