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
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using System.Reflection;

namespace OpenNETCF.MTConnect
{
    public class Agent
    {
        public DeviceCollection Devices { get; private set; }
        public AdapterCollection Adapters { get; private set; }
        public AgentData Data { get; private set; }

        public long InstanceID { get; private set; }
        public string AgentTypeName { get; private set; }

        private string m_versionNumber;
        internal IHost Host { get; set; }

        private Dictionary<string, DataItem> m_dataItemMap = new Dictionary<string, DataItem>(StringComparer.InvariantCultureIgnoreCase);

        internal Agent()
            : this(1000)
        {
        }

        internal Agent(int bufferSize)
            : this(bufferSize, 1000)
        {
        }

        internal Agent(int bufferSize, int checkpointFrequency)
        {
            Validate.Begin()
                .IsPositive(bufferSize)
                .IsPositive(checkpointFrequency)
                .Check();

            // TODO: implement checkpoints

            AgentTypeName = "OpenNETCF VirtualAgent";
            m_versionNumber = Assembly.GetCallingAssembly().GetName().Version.ToString(3);

            Initialize(bufferSize);
        }

        public string Version
        {
            get { return m_versionNumber; }
        }

        private void Initialize(int bufferSize)
        {
            Adapters = new AdapterCollection(this);
            Adapters.DataItemValueSet += new EventHandler<DataItemValue>(Adapters_DataItemValueSet);
            Adapters.AdapterAdded += new EventHandler<GenericEventArgs<Adapter>>(Adapters_AdapterAdded);
            Adapters.AdapterRemoved += new EventHandler<GenericEventArgs<Adapter>>(Adapters_AdapterRemoved);
            Adapters.Cleared += new EventHandler(Adapters_Cleared);

            Devices = new DeviceCollection();
            Data = new AgentData(this);
            InstanceID = DateTime.Now.ToUniversalTime().Ticks;
        }

        void Adapters_Cleared(object sender, EventArgs e)
        {
            m_dataItemMap.Clear();
            Devices.Clear();
            //Data.Clear();
        }

        static bool TryParse(string s, out int value)
        {
            try
            {
                value = int.Parse(s);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        void Adapters_AdapterAdded(object sender, GenericEventArgs<Adapter> e)
        {
            Devices.Add(e.Value.Device);
        }

        void Adapters_AdapterRemoved(object sender, GenericEventArgs<Adapter> e)
        {
            Devices.Remove(e.Value.Device);
        }

        void Adapters_DataItemValueSet(object sender, DataItemValue value)
        {
            if (value.Item.Device == null)
            {
                if (Debugger.IsAttached) Debugger.Break();
            }

            var sequence = Data.AddValue(value.Item, value.Value, value.Time);

            if (sequence < 0)
            {
                // this was a duplicate value.  It wasn't stored (per the spec) and we don't want to raise an event for it
                return;
            }
        }

        public void PublishData(string dataItemID, object value, DateTime time)
        {
            DataItem dataItem = GetDataItemByID(dataItemID);

            if (dataItem == null || dataItem.Device == null)
            {
                //if (Debugger.IsAttached) Debugger.Break();
                return;
            }

            dataItem.SetValue(value, DateTime.Now);
        }

        public DataItem GetDataItemByID(string dataItemID)
        {
            DataItem dataItem = null;

            lock (m_dataItemMap)
            {
                if (m_dataItemMap.ContainsKey(dataItemID))
                {
                    dataItem = m_dataItemMap[dataItemID];
                }
                else
                {
                    var collection = Devices.ToArray();
                    foreach (var device in collection)
                    {
                        var items = device.DataItems.Find(d => d.ID == dataItemID);

                        if (items == null)
                        {
                            foreach (var component in device.Components)
                            {
                                items = component.DataItems.Find(e => e.ID == dataItemID);
                                if (items != null)
                                {
                                }
                            }
                        }

                        if (items != null)
                        {
                            dataItem = items.FirstOrDefault();
                        }

                        if (dataItem != null)
                        {
                            if (dataItem.Device == null)
                            {
                                dataItem.Device = device;
                            }

                            dataItem.Removed += dataItem_Removed;
                            m_dataItemMap.Add(dataItemID, dataItem);
                            break;
                        }
                    }
                }

                if (dataItem == null)
                {
                    var collection = Devices.ToArray();
                }
            }

            return dataItem;
        }

        void dataItem_Removed(object sender, DataItemValue e)
        {
            e.Item.Removed -= dataItem_Removed;
            m_dataItemMap.Remove(e.Item.ID);
        }

        public void Start()
        {
            foreach (var adapter in Adapters) { adapter.Start(); }
        }

        public string Probe()
        {
            return Probe(null);
        }

        public XDocument GenerateErrorDocument(MTConnectError error)
        {
            return GenerateErrorDocument(new MTConnectError[] { error });
        }

        public XDocument GenerateErrorDocument(IEnumerable<MTConnectError> errors)
        {
            XNamespace ns = "urn:mtconnect.com:MTConnectError:1.1";
            XElement element = new XElement(ns + "MTConnectError");
            XElement root = null;

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var schemaLocation = "urn:mtconnect.org:MTConnectError:1.1 http://www.mtconnect.org/schemas/MTConnectError_1.1.xsd";

            root = new XElement(ns + "MTConnectError",
                new XAttribute("xmlns", ns),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "schemaLocation", schemaLocation));

            root.Add(
                new XElement(ns + "Header")
                    .AddAttribute("creationTime", DateTime.Now.ToUniversalTime().ToString("s"))
                    .AddAttribute("sender", Host == null ? "[unconnected]" : Host.HostName)
                    .AddAttribute("version", Version)
                    .AddAttribute("bufferSize", this.Data.BufferSize.ToString())
                    .AddAttribute("instanceId", this.InstanceID.ToString())
                    .AddAttribute("agentType", AgentTypeName)
                    );

            var errorsNode = new XElement(ns + "Errors");

            foreach (var e in errors)
            {
                errorsNode.Add(
                    new XElement(
                        ns + "Error",
                        e.ErrorCode.ToXmlAttribute(),
                        e.Description
                        )
                    );
            }

            root.Add(errorsNode);

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            return doc;
        }

        private string Probe(string deviceName)
        {
            XNamespace ns = "urn:mtconnect.com:MTConnectDevices:1.1";
            XElement root = null;

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var schemaLocation = "urn:mtconnect.com:MTConnectDevices:1.1 MTConnectDevices.xsd";

            root = new XElement(ns + "MTConnectDevices",
                new XAttribute("xmlns", ns),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "schemaLocation", schemaLocation));

            root.Add(
                new XElement(ns + "Header")
                    .AddAttribute("creationTime", DateTime.Now.ToUniversalTime().ToString("s"))
                    .AddAttribute("sender", Host == null ? "[unconnected]" : Host.HostName)
                    .AddAttribute("version", Version)
                    .AddAttribute("bufferSize", this.Data.BufferSize.ToString())
                    .AddAttribute("instanceId", this.InstanceID.ToString())
                    .AddAttribute("agentType", AgentTypeName)
                    );

            XElement element = new XElement(ns + "Devices");

            if (deviceName != null)
            {
                var device = Devices[deviceName];
                if (device == null)
                {
                    throw new Exception("Device not found");
                }

                element.Add(device.AsXElement(ns));
            }
            else // get all devices
            {
                foreach (var d in Devices)
                {

                    element.Add(d.AsXElement(ns));
                }
            }

            root.Add(element);

            return root.ToString();
        }

        public string Assets()
        {
            XNamespace ns = "urn:mtconnect.org:MTConnectAssets:1.2";
            XElement root = null;

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var schemaLocation = "urn:mtconnect.org:MTConnectAssets:1.2 http://www.mtconnect.org/schemas/MTConnectAssets_1.2.xsd";

            root = new XElement(ns + "MTConnectAssets",
                new XAttribute("xmlns", ns),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "schemaLocation", schemaLocation));

            root.Add(
                new XElement(ns + "Header")
                    .AddAttribute("creationTime", DateTime.Now.ToUniversalTime().ToString("s"))
                    .AddAttribute("sender", Host == null ? "[unconnected]" : Host.HostName)
                    .AddAttribute("version", Version)
                    .AddAttribute("bufferSize", this.Data.BufferSize.ToString())
                    .AddAttribute("instanceId", this.InstanceID.ToString())
                    .AddAttribute("agentType", AgentTypeName)
                    );

            XElement element = new XElement(ns + "Assets");

            root.Add(element);

            return root.ToString(); 
        }
    }
}
