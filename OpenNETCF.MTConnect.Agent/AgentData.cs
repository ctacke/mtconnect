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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace OpenNETCF.MTConnect
{
    public class AgentData
    {
        private CircularBuffer<DataItemValue> m_buffer;
        private Dictionary<string, DataItemValue> m_currentValues;

        private long m_currentSequence = 1;
        private object m_syncRoot = new object();
        private Agent m_agent;
        internal const int DefaultBufferSize = 10000;

        internal AgentData(Agent agent)
            : this(agent, DefaultBufferSize)
        {
        }

        internal AgentData(Agent agent, int bufferSize)
        {
            Validate.Begin()
                .IsPositive(bufferSize)
                .Check();

            m_buffer = new CircularBuffer<DataItemValue>(bufferSize);
            m_currentValues = new Dictionary<string, DataItemValue>();

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
                UpdateCurrentValuesList(div);

                return sequence;
            }
        }

        private void UpdateCurrentValuesList(DataItemValue value)
        {
            var id = value.Item.ID;

            if (m_currentValues.ContainsKey(id))
            {
                m_currentValues[id] = value;
            }
            else
            {
                m_currentValues.Add(id, value);
            }
        }

        internal void Clear()
        {
            lock (m_syncRoot)
            {
                m_buffer.Clear();
                m_currentValues.Clear();
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

            while (index <= Count - 1)
            {
                if (m_buffer[index].Sequence >= startSequence) break;
                index++;
            }

            while ((index <= Count - 1) && (values.Count < count))
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

                List<DataItemValue> values = new List<DataItemValue>();

                foreach (var item in m_currentValues)
                {
                    if (filter(item.Value.Item))
                    {
                        values.Add(item.Value);
                    }
                }

                return values.ToArray();
            }
        }

        internal DataItemValue[] Current(out long nextSequence, FilterPath filter)
        {
            if (filter == null) return Current(out nextSequence);

            lock (m_syncRoot)
            {
                nextSequence = NextSequenceNumber;

                List<DataItemValue> values = new List<DataItemValue>();

                foreach (var item in m_currentValues)
                {
                    if (!filter.ContainsDevice(item.Value.Item.Device.Name)) continue;
                    if (!filter.ContainsComponent(item.Value.Item.Component.Name)) continue;
                    if (!filter.ContainsComponent(item.Value.Item.Component.Name)) continue;
                    if (!filter.ContainsDataItem(item.Value.Item)) continue;

                    values.Add(item.Value);
                }

                return values.ToArray();
            }
        }

        public string CurrentXml()
        {
            return CurrentXml(f => true);
        }

        internal string CurrentXml(FilterPath filter)
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

        public string GetCurrentValue(string dataItemID)
        {
            DataItemValue item = null;

            if (m_currentValues.Keys.Contains(dataItemID))
            {
                item = m_currentValues[dataItemID];
            }
            else
            {
                item = m_buffer.Last(i => i.Item.ID == dataItemID);
            }

            if (item == null) return null;
            
            return item.Value;
        }
    }
}
