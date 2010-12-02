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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenNETCF.Net.MTConnect
{
    internal static class DataItemValueExtensions
    {
        public static long FirstSequence(this DataItemValue[] items)
        {
            if (items.Count() == 0) return -1;

            return items.Select(i => i.Sequence).Min();
        }

        public static long LastSequence(this DataItemValue[] items)
        {
            if (items.Count() == 0) return -1;

            return items.Select(i => i.Sequence).Max();
        }

        public static XElement AsStreamsXml(this DataItemValue[] items, XNamespace ns)
        {
            var streamsElement = new XElement(ns + "Streams");

            var devices = (from d in items
                    select new 
                    {
                        Name = d.Item.Device.Name,
                        UUID = d.Item.Device.UUID,
                        ID = d.Item.Device.ID
                    }).Distinct();

            foreach (var device in devices)
            {
                // TODO: add device data items

                var deviceElement = new XElement(ns + "DeviceStream")
                    .AddAttribute("uuid", device.UUID)
                    .AddAttribute("id", device.ID)
                    .AddAttribute("name", device.Name);

                var components = (from i in items
                                 where i.Item.Device.UUID == device.UUID
                                 select i.Item.Component).Distinct();

                foreach (var component in components)
                {
                    var componentElement = new XElement(ns + "ComponentStream")
                        .AddAttribute("component", component.XmlNodeName)
                        .AddAttribute("componentId", component.ID)
                        .AddAttributeIfHasValue("name", component.Name);

                    // samples
                    var samplesElement = new XElement(ns + "Samples");
                    var samples = from i in items
                                  where i.Item.Component.ID == component.ID
                                  && i.Item.Category == DataItemCategory.Sample
                                  select i;

                    if (samples.Count() > 0)
                    {
                        foreach (var sample in samples)
                        {
                            samplesElement.Add(sample.AsXml());
                        }
                        componentElement.Add(samplesElement);
                    }

                    // condition
                    var conditionElement = new XElement(ns + "Condition");
                    var conditions = from i in items
                                  where i.Item.Component.ID == component.ID
                                  && i.Item.Category == DataItemCategory.Condition
                                  select i;

                    if (conditions.Count() > 0)
                    {
                        foreach (var condition in conditions)
                        {
                            conditionElement.Add(condition.AsXml());
                        }
                        componentElement.Add(conditionElement);
                    }

                    // events
                    var eventsElement = new XElement(ns + "Events");
                    var events = from i in items
                                  where i.Item.Component.ID == component.ID
                                  && i.Item.Category == DataItemCategory.Event
                                  select i;

                    if (events.Count() > 0)
                    {
                        foreach (var evt in events)
                        {
                            eventsElement.Add(evt.AsXml());
                        }
                        componentElement.Add(eventsElement);
                    }

                    deviceElement.Add(componentElement);
                }

                streamsElement.Add(deviceElement);
            }

            return streamsElement;
        }
    }

    public class AgentData
    {
        private CircularBuffer<DataItemValue> m_buffer;
        private long m_currentSequence = 1;
        private object m_syncRoot = new object();
        private Agent m_agent;

        internal AgentData(Agent agent)
        {
            m_buffer = new CircularBuffer<DataItemValue>(10000);
            m_agent = agent;
        }

        internal long AddValue(DataItem item, string value, DateTime time)
        {
            lock (m_syncRoot)
            {
                // See if the value is actually "new".
                // This slows things down, yes, but the spec requires that we only store changed data and
                // we can't rely on the Adapter implementer to ensure this
                var last = m_buffer.Last(i => i.Item.ID == item.ID);
                if (last != null)
                {
                    if (last.Value == value) return -1;
                }

                var sequence = IncrementSequenceNumber();
                var div = new DataItemValue(sequence, item, value, time);
                m_buffer.Enqueue(div);

                return sequence;
            }
        }

        internal int BufferSize
        {
            get { return m_buffer.MaxElements; }
        }

        private long IncrementSequenceNumber()
        {
            lock (m_syncRoot)
            {
                return m_currentSequence++;
            }
        }

        public DataItemValue GetFromBuffer(long sequenceNumber)
        {
            return m_buffer.First(v => v.Sequence == sequenceNumber);
        }

        public int Count
        {
            get { return m_buffer.Count; }
        }

        public DataItemValue[] Sample(long startSequence, int count)
        {
            return Sample(startSequence, count, d => true);
        }

        public DataItemValue[] Sample(long startSequence, int count, Func<DataItem, bool> filter)
        {
            int index = 0;

            List<DataItemValue> values = new List<DataItemValue>();

            while((m_buffer[index].Sequence < startSequence) && (index < Count - 1))
            {
                index++;
            }

            while ((index < Count - 1) && (values.Count < count))
            {
                var item = m_buffer[index];

                if (filter(item.Item))
                {
                    values.Add(m_buffer[index]);
                    index++;
                }
            }

            return values.ToArray();
        }

        public string SampleXml(long startSequence, int requestedCount, out int actualCount, Func<DataItem, bool> filter)
        {
            var data = Sample(startSequence, requestedCount, filter);
            actualCount = data.Length;

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "true"));

            XNamespace ns = "urn:mtconnect.com:MTConnectStreams:1.1";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var schemaLocation = "urn:mtconnect.com:MTConnectStreams:1.1 http://www.mtconnect.org/schemas/MTConnectStreams_1.1.xsd";

            var mtcStreamsElement = new XElement(ns + "MTConnectStreams",
                new XAttribute("xmlns", ns),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "schemaLocation", schemaLocation));

            doc.Add(mtcStreamsElement);

            var headerElement = new XElement(ns + "Header")
                .AddAttribute("creationTime", DateTime.Now.ToUniversalTime().ToString("s"))
                .AddAttribute("sender", m_agent.Host == null ? "[unconnected]" : m_agent.Host.HostName)
                .AddAttribute("instanceId", m_agent.InstanceID.ToString())
                .AddAttribute("bufferSize", m_agent.Data.BufferSize.ToString())
                .AddAttribute("version", m_agent.Version)
                .AddAttribute("nextSequence", (data.LastSequence() + 1).ToString())
                .AddAttribute("firstSequence", data.FirstSequence().ToString())
                .AddAttribute("lastSequence", data.LastSequence().ToString());

            mtcStreamsElement.Add(headerElement);

            mtcStreamsElement.Add(data.AsStreamsXml(ns));

            return doc.ToString();
        }

        public DataItemValue[] Current(out long nextSequence)
        {
            return Current(out nextSequence, d => true);
        }

        public DataItemValue[] Current(out long nextSequence, Func<DataItem, bool> filter)
        {
            lock (m_syncRoot)
            {
                nextSequence = NextSequenceNumber;
            }

            List<DataItemValue> values = new List<DataItemValue>();

            // This will return the last DatItemValue in the buffer for each known DataItem for every Device and Component.
            // It may well be slow if the tree is large.
            // We could markedly improve perf by caching the "last" for every data item, but at the cost of memory for the cache
            // We'll revisit that if perf is an issue when fielded

            foreach (var device in m_agent.Devices)
            {
                foreach (var item in device.DataItems)
                {
                    var last = m_buffer.Last(i => i.Item.ID == item.ID);
                    if ((last != null) && (filter(last.Item))) values.Add(last);
                }

                foreach (var component in device.Components)
                {
                    foreach (var item in component.DataItems)
                    {
                        var last = m_buffer.Last(i => i.Item.ID == item.ID);
                        if ((last != null) && (filter(last.Item))) values.Add(last);
                    }

                    foreach (var subcomponent in component.Components)
                    {
                        foreach (var item in subcomponent.DataItems)
                        {
                            var last = m_buffer.Last(i => i.Item.ID == item.ID);
                            if ((last != null) && (filter(last.Item))) values.Add(last);
                        }
                    }
                }
            }

            return values.ToArray();
        }

        public string CurrentXml()
        {
            return CurrentXml(f => true);
        }

        public string CurrentXml(Func<DataItem, bool> filter)
        {
            long nextSequence;

            var data = Current(out nextSequence, filter);

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "true"));

            XNamespace ns = "urn:mtconnect.com:MTConnectStreams:1.1";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var schemaLocation = "urn:mtconnect.com:MTConnectStreams:1.1 http://www.mtconnect.org/schemas/MTConnectStreams_1.1.xsd";
            
            var mtcStreamsElement = new XElement(ns + "MTConnectStreams",
                new XAttribute("xmlns", ns), 
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "schemaLocation", schemaLocation));

            doc.Add(mtcStreamsElement);

            var headerElement = new XElement(ns + "Header")
                .AddAttribute("creationTime", DateTime.Now.ToUniversalTime().ToString("s"))
                .AddAttribute("sender", m_agent.Host == null ? "[unconnected]" : m_agent.Host.HostName)
                .AddAttribute("instanceId", m_agent.InstanceID.ToString())
                .AddAttribute("bufferSize", m_agent.Data.BufferSize.ToString())
                .AddAttribute("version", m_agent.Version)
                .AddAttribute("nextSequence", nextSequence.ToString())
                .AddAttribute("firstSequence", data.FirstSequence().ToString())
                .AddAttribute("lastSequence", data.LastSequence().ToString());

            mtcStreamsElement.Add(headerElement);

            mtcStreamsElement.Add(data.AsStreamsXml(ns));

            return doc.ToString();
        }

        public long? FirstSequenceNumber
        {
            get 
            {
                if (Count == 0) return null;
                return m_buffer[0].Sequence; 
            }
        }

        public long? LastSequenceNumber
        {
            get 
            {
                if (Count == 0) return null;
                return m_buffer[Count - 1].Sequence; 
            }
        }

        public long NextSequenceNumber
        {
            get { return m_currentSequence; }
        }
    }
}
