﻿// -------------------------------------------------------------------------------------------------------
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

namespace OpenNETCF.MTConnect
{
    public class Agent
    {
        public DeviceCollection Devices { get; private set; }
        public AdapterCollection Adapters { get; private set; }
        public AgentData Data { get; private set; }

        public long InstanceID { get; private set; }

        internal IHost Host { get; set; }

        private Dictionary<string, DataItem> m_dataItemMap = new Dictionary<string, DataItem>();

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

            Initialize(bufferSize);
        }

        public string Version
        {
            get { return "1.1"; }
        }

        private void Initialize(int bufferSize)
        {
            Adapters = new AdapterCollection(this);
            Adapters.DataItemValueSet += new EventHandler<DataItemValue>(Adapters_DataItemValueSet);
            Adapters.AdapterAdded += new EventHandler<GenericEventArgs<Adapter>>(Adapters_AdapterAdded);
            Adapters.Cleared += new EventHandler(Adapters_Cleared);

            Devices = new DeviceCollection();
            Data = new AgentData(this);
            InstanceID = DateTime.Now.ToUniversalTime().Ticks;
        }

        void Adapters_Cleared(object sender, EventArgs e)
        {
            Devices.Clear();
            Data.Clear();
        }

        void Adapters_AdapterAdded(object sender, GenericEventArgs<Adapter> e)
        {
            Devices.Add(e.Value.Device);
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

            // TODO raise an event
        }

        public void PublishData(string dataItemID, string value, DateTime time)
        {
            DataItem dataItem = GetDataItemByID(dataItemID);

            if (dataItem == null)
            {
                throw new InvalidIDException(dataItemID, string.Format("DataItem with ID {0} not found", dataItemID));
            }

            if (dataItem.Device == null)
            {
                if (Debugger.IsAttached) Debugger.Break();
            }

            dataItem.SetValue(value);
        }

        protected DataItem GetDataItemByID(string dataItemID)
        {
            DataItem dataItem = null;

            if (m_dataItemMap.ContainsKey(dataItemID))
            {
                dataItem = m_dataItemMap[dataItemID];
            }
            else
            {
                foreach (var device in Devices.ToArray())
                {
                    var items = device.DataItems.Find(d => d.ID == dataItemID);

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

                        m_dataItemMap.Add(dataItemID, dataItem);
                        break;
                    }
                }
            }

            return dataItem;
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
                    );

            XElement element = new XElement(ns + "Devices");

            if (deviceName != null)
            {
                var device = Devices[deviceName];
                if (device == null)
                {
                    throw new Exception("Device not found");
                    // error
                    // TODO: pass proper MTC error string
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
    }
}
