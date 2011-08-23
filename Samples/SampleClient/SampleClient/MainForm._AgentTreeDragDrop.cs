using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenNETCF.MTConnect;

namespace SampleClient
{
    public partial class MainForm : Form
    {
        private TreeNode m_selectedNode;

        private void InitializeAgentTreeDragDrop()
        {
            agentTree.MouseDown += new MouseEventHandler(agentTree_MouseDown);
            agentTree.ItemDrag += new ItemDragEventHandler(agentTree_ItemDrag);
        }

        void agentTree_MouseDown(object sender, MouseEventArgs e)
        {
            m_selectedNode = ((TreeView)sender).GetNodeAt(new Point(e.X, e.Y));
        }

        void agentTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (m_selectedNode == null) return;

            // we can only drag DataItems
            var dataItem = m_selectedNode.Tag as DataItem;
            if (dataItem != null)
            {
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }
    }
}
