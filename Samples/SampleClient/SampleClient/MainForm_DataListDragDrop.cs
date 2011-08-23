using System.Windows.Forms;
using OpenNETCF.MTConnect;
using System.IO;
using System.Text;
using System;

namespace SampleClient
{
    public partial class MainForm
    {
        private void InitializeDataListDragDrop()
        {
            dataList.DragEnter += new DragEventHandler(dataList_DragEnter);
            dataList.DragDrop += new DragEventHandler(dataList_DragDrop);
        }

        void dataList_DragDrop(object sender, DragEventArgs e)
        {
            var di = m_selectedNode.Tag as DataItem;
            if (di == null) return;

            var lvi = new ListViewItem(new string[]
                    {
                        di.Category.ToString(),
                        di.ID,
                        di.Name,
                        di.Type,
                        string.Empty // value place holder
                    });

            lvi.Tag = di;
            dataList.Items.Add(lvi);

            for (int i = 0; i < dataList.Columns.Count; i++)
            {
                dataList.Columns[i].Width = i < dataList.Columns.Count - 1 ? -1 : -2;
            }
        }

        void dataList_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }
}
