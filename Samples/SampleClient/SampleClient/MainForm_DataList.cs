using System.Windows.Forms;
using OpenNETCF.MTConnect;
using System.IO;
using System.Text;
using System;

namespace SampleClient
{
    public partial class MainForm
    {
        private void InitializeDataList()
        {
            InitializeDataListDragDrop();
            InitializeRecordData();
        }

        private void UpdateDataList()
        {
            var fileLine = new StringBuilder();

            for (int i = 0; i < dataList.Items.Count; i++ )
            {
                var lvi = dataList.Items[i];
                var currentValue = m_client.GetDataItemById(((DataItem)lvi.Tag).ID).Value ?? string.Empty;
                lvi.SubItems[4].Text = (currentValue ?? string.Empty).ToString();

                fileLine.AppendFormat("{0}{1}", currentValue, i < (dataList.Items.Count - 1) ? "," : Environment.NewLine);
            }

            WriteLineToFile(fileLine.ToString());
        }
    }
}
