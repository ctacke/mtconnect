using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenNETCF.MTConnect;

namespace SampleClient
{
    public partial class MainForm : Form
    {
        private void PopulatePropertyList(object source)
        {
            propertyList.Items.Clear();

            var item = source as ComponentBase;
            var dataItem = source as DataItem;
            var info = source as AgentInformation;

            if (item != null)
            {
                foreach (var prop in item.Properties)
                {
                    propertyList.Items.Add(new ListViewItem(new string[]
                    {
                        prop.Key,
                        prop.Value
                    }));
                }
            }
            else if (dataItem != null)
            {
                var lvi = new ListViewItem(new string[]
                    {
                        "value",
                        string.Empty
                    });
                lvi.Tag = dataItem;

                propertyList.Items.Add(lvi);

                foreach (var prop in dataItem.Properties)
                {
                    propertyList.Items.Add(new ListViewItem(new string[]
                    {
                        prop.Key,
                        prop.Value
                    }));
                }
            }
            else if (info != null)
            {
                propertyList.Items.Add(new ListViewItem(new string[]
                {
                    "Name",
                    info.Name
                }));

                propertyList.Items.Add(new ListViewItem(new string[]
                {
                    "Instance ID",
                    info.InstanceID
                }));

                propertyList.Items.Add(new ListViewItem(new string[]
                {
                    "Version",
                    info.Version
                }));
            }

            propertyList.Columns[0].Width = -1;
            propertyList.Columns[1].Width = -2;
        }
    }
}
