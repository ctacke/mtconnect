using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenNETCF.MTConnect;

namespace SampleClient
{
    public partial class MainForm : Form
    {
        private void InitializeAgentTree()
        {
            agentTree.AfterSelect += new TreeViewEventHandler(agentTree_AfterSelect);

            InitializeAgentTreeDragDrop();
        }

        void agentTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            PopulatePropertyList(e.Node.Tag);
        }

        private void PopulateAgentTree(DeviceCollection devices)
        {
            agentTree.Nodes.Clear();

            // create a root node
            var root = new TreeNode(devices.AgentInformation.Name);
            root.Tag = devices.AgentInformation;

            // fill the node with devices
            foreach (var device in devices)
            {
                //var deviceNode = new TreeNode(string.Format("{0} [id:{1}]", device.Name, device.ID));
                var deviceNode = new TreeNode(device.Name);
                deviceNode.Tag = device;

                foreach (var component in device.Components)
                {
                    FillComponents(component, deviceNode);
                }

                foreach (var dataItem in device.DataItems)
                {
                    string displayName = dataItem.ID;
                    if (!string.IsNullOrEmpty(dataItem.Name)) displayName += string.Format(" ({0})", dataItem.Name);
                    var dataNode = new TreeNode(displayName);
                    dataNode.Tag = dataItem;
                    deviceNode.Nodes.Add(dataNode);
                }

                root.Nodes.Add(deviceNode);
            }

            root.ExpandAll();

            // put the root node into the tree
            agentTree.Nodes.Add(root);

            // cache the device collection
            agentTree.Tag = devices;
        }

        private void FillComponents(OpenNETCF.MTConnect.Component component, TreeNode parent)
        {
            //var componentNode = new TreeNode(string.Format("{0} [id:{1}]", component.Name, component.ID));
            var componentNode = new TreeNode(component.Name);
            componentNode.Tag = component;

            foreach (var subcomponent in component.Components)
            {
                FillComponents(subcomponent, componentNode);
            }

            foreach (var dataItem in component.DataItems)
            {
                string displayName = dataItem.ID;
                if (!string.IsNullOrEmpty(dataItem.Name)) displayName += string.Format(" ({0})", dataItem.Name);
                var dataNode = new TreeNode(displayName);
                dataNode.Tag = dataItem;
                componentNode.Nodes.Add(dataNode);
            }

            parent.Nodes.Add(componentNode);
        }
    }
}
