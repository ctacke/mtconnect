using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenNETCF.MTConnect;

namespace SampleClient
{
    public partial class MainForm : Form
    {
        private const int UpdateFrequency = 500;

        private void InitializeDataItemWatcher()
        {
            new Thread(DataItemWatcherProc) { IsBackground = true }
            .Start();
        }

        private void DataItemWatcherProc()
        {
            do
            {
                if (m_client != null)
                {
                    // get the latest data
                    var stream = m_client.Sample();

                    this.Invoke(new EventHandler(delegate
                    {
                        // update the property list if a DataItem is selected
                        if (propertyList.Items.Count > 0)
                        {
                            var selectedDataItem = propertyList.Items[0].Tag as DataItem;

                            if (selectedDataItem != null)
                            {
                                var currentValue = m_client.GetDataItemById(selectedDataItem.ID).Value;
                                propertyList.Items[0].SubItems[1].Text = (currentValue ?? string.Empty).ToString();
                            }
                        }

                        UpdateDataList();

                        UpdateDataPlot();
                    }));
                }
                Thread.Sleep(UpdateFrequency);
            } while (true);
        }
    }
}
