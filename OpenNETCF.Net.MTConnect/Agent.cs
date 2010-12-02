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

namespace OpenNETCF.Net.MTConnect
{
    public class Agent
    {
        public DeviceCollection Devices { get; private set; }
        public AdapterCollection Adapters { get; private set; }
        public AgentData Data { get; private set; }

        public long InstanceID { get; private set; }

        internal IHost Host { get; set; }

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
            
            Devices = new DeviceCollection();
            Data = new AgentData(this);
            InstanceID = DateTime.Now.ToUniversalTime().Ticks;
        }

        void Adapters_AdapterAdded(object sender, GenericEventArgs<Adapter> e)
        {
            Devices.Add(e.Value.Device);
        }

        void Adapters_DataItemValueSet(object sender, DataItemValue e)
        {
            e.Sequence = Data.AddValue(e.Item, e.Value, e.Time);
            if (e.Sequence < 0)
            {
                // this was a duplicate value.  It wasn't stored (per the spec) and we don't want to raise an event for it
                return;
            }

            // TODO raise an event

        }

        public void Start()
        {
            foreach (var adapter in Adapters) { adapter.Start(); }
        }

        public string Probe()
        {
            return Probe(null);
        }

        public string Probe(string deviceName)
        {
            return Probe(deviceName, true).ToString();
        }

        private XElement Probe(string deviceName, bool includeHeader)
        {
            XNamespace ns = "urn:mtconnect.com:MTConnectDevices:1.1";
            XElement element = new XElement(ns + "Device");
            XElement root = null;

            if (includeHeader)
            {
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
            }

            if (root == null) root = element;

            if (string.IsNullOrEmpty(deviceName))
            {
                // probe all
                foreach (var d in Devices)
                {
                    root.Add(Probe(d.Name, false));
                }
            }
            else
            {
                var device = Devices[deviceName];
                if (device == null)
                {
                    throw new Exception("Device not found");
                    // error
                    // TODO: pass proper MTC error string
                }

                root.Add(device.AsXElement(ns));
            }

            return root;
        }
    }
}
