// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://mtconnect.codeplex.com
//
// Copyright (c) 2010 OpenNETCF Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// -------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace OpenNETCF.MTConnect
{
    public class Adapter : IAdapter
    {
        private EventHandler<DataItemValue> DataItemValueSetDelegate;
        private static int m_instanceCount = 0;
        private int m_instanceNumber = 0;

        private Device m_device;
        private Dictionary<string, string> m_defaultDataMap = new Dictionary<string, string>();

        public event EventHandler<DataItemValue> DataItemValueSet
        {
            add 
            {
                Debug.WriteLine("Subscribed " + m_instanceNumber.ToString());
                DataItemValueSetDelegate += value;
            }
            remove 
            {
                Debug.WriteLine("Unsubscribed " + m_instanceNumber.ToString());
                DataItemValueSetDelegate -= value;
            }
        }

        public AdapterCollection Container { get; internal set; }
        public String InstanceName { get; set; }
        public String AdapterType { get; set; }

        public virtual void Start() { }
        public virtual PropertyCollection GetDeviceProperties() { return null; }
        public virtual ComponentDescription GetComponentDescription(ComponentBase component) { return null; }
        public virtual PropertyCollection[] GetDeviceDataItems() { return null; }
        public virtual ComponentDescriptor[] GetComponentDescriptors(ComponentBase parent) { return null; }
        public virtual PropertyCollection[] GetComponentDataItems(ComponentBase parent) { return null; }

        public Adapter()
        {
            m_instanceNumber = ++m_instanceCount;
            m_defaultDataMap.Add("SYSTEM.DOUBLE", "0.0");
            m_defaultDataMap.Add("SYSTEM.BOOLEAN", "false");
            m_defaultDataMap.Add("SYSTEM.STRING", string.Empty);
            m_defaultDataMap.Add("SYSTEM.INT64", "0");
            m_defaultDataMap.Add("AVAILABILITY", "AVAILABLE");
        }

        public Adapter(Device device)
            : this()
        {
            Device = device;
        }

        public virtual void PublishDefaultData() 
        {
            Debug.WriteLine("PublishDefaultData for " + this.Device.Name);

            PublishDefaultData(this.Device);
        }

        private void PublishDefaultData(ComponentBase component)
        {
            foreach (var di in component.DataItems)
            {
                PublishDefaultData(di);
            }

            foreach (var c in component.Components)
            {
                PublishDefaultData(c);
            }
        }

        private void PublishDefaultData(DataItem dataItem)
        {
            var typeName = dataItem.Type.ToUpper();
            if (m_defaultDataMap.ContainsKey(typeName))
            {
                this.Container.Agent.PublishData(dataItem.ID, m_defaultDataMap[typeName], DateTime.Now);
            }
        }

        public Device Device 
        {
            get { return m_device; }
            internal set
            {
                if (m_device != null)
                {
                    m_device.DataItemValueSet -= Device_DataItemValueSet;
                }

                m_device = value;
                if (value != null)
                {
                    m_device.DataItemValueSet += Device_DataItemValueSet;
                }
            }
        }

        private void Device_DataItemValueSet(object sender, DataItemValue e)
        {
            OnDataItemValueSet(sender, e);
        }

        public virtual void OnDataItemValueSet(object sender, DataItemValue value)
        {
            DataItemValueSetDelegate.Fire(sender ?? Device, value);
        }

        public void AddAlarm(string name, string id, string path)
        {
            DataItem dataItem = new DataItem(DataItemCategory.Condition, DataItemType.HARDWARE, name, id);

            dataItem.Writable = true;

            string[] paths = path.Split('.');

            if (paths.Length == 2)
            {
                Component component = Device.Components[paths[1]];
                component.AddDataItem(dataItem);
                return;
            }

            if (paths.Length == 3)
            {
                string componentPath = paths[0] + "." + paths[1];
                Component component = Device.Components[componentPath];
                Component subComponent = component.Components[path];
                subComponent.AddDataItem(dataItem);
                return;
            }

            Device.AddDataItem(dataItem);
        }

        public void RemoveAlarm(string name, string id, string path)
        {
            //travel path
            string[] paths = path.Split('.');

            if (paths.Length == 2)
            {
                Component component = Device.Components[path];
                var dataItems = (List<DataItem>)component.DataItems.Find(t => t.Name == name && t.ID == id);
                component.RemoveDataItem(dataItems[0]);
                return;
            }

            if (paths.Length == 3)
            {
                string componentPath = paths[0] + "." + paths[1];
                Component component = Device.Components[componentPath];
                Component subComponent = component.Components[path];
                var dataItems = (List<DataItem>)component.DataItems.Find(t => t.Name == name && t.ID == id);
                subComponent.RemoveDataItem(dataItems[0]);
                return;
            }

            var deviceDataItems = (List<DataItem>) Device.DataItems.Find(t => t.Name == name && t.ID == id);
            Device.RemoveDataItem(deviceDataItems[0]);
        }
    }
}
