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
using System.Diagnostics;

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

        private readonly XNamespace m_namespace = "urn:mtconnect.com:MTConnectStreams:1.1";
        private readonly XNamespace m_xsi = "http://www.w3.org/2001/XMLSchema-instance";
        private readonly string m_schemaLocation = "urn:mtconnect.com:MTConnectStreams:1.1 http://www.mtconnect.org/schemas/MTConnectStreams_1.1.xsd";

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

        public long AddValue(DataItem item, object value, DateTime time)
        {
            lock (m_syncRoot)
            {
                if (m_currentValues.ContainsKey(item.ID))
                {
                    var currentValue = m_currentValues[item.ID].Value;
                    if (currentValue is Condition)
                    {
                    }
                    else
                    {
                        if (currentValue == value) return -1;
                    }
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
            return m_currentSequence++;
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

            List<DataItemValue> values = new List<DataItemValue>(count);

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

        // these items are used a lot - to help minimize crating string garbage, we reuse as much as we can
        private XAttribute m_senderAttribute = null;
        private XAttribute m_instanceAttribute = null;
        private XAttribute m_bufferAttribute = null;
        private XAttribute m_versionAttribute = null;

        private XElement GetBaseStreamsElement()
        {
            return new XElement(m_namespace + "MTConnectStreams",
                new XAttribute("xmlns", m_namespace),
                new XAttribute(XNamespace.Xmlns + "xsi", m_xsi),
                new XAttribute(m_xsi + "schemaLocation", m_schemaLocation));
        }

        private XElement GetBaseHeaderElement()
        {
            if (m_senderAttribute == null)
            {
                m_senderAttribute = new XAttribute("sender", m_agent.Host == null ? "[unconnected]" : m_agent.Host.HostName);
                m_instanceAttribute = new XAttribute("instanceId", m_agent.InstanceID.ToString());
                m_bufferAttribute = new XAttribute("bufferSize", m_agent.Data.BufferSize.ToString());
                m_versionAttribute = new XAttribute("version", m_agent.Version);                
            }

            var @base = new XElement(m_namespace + "Header")
                .AddAttribute("creationTime", DateTime.Now.ToUniversalTime().ToString("s"));
            @base.Add(m_senderAttribute);
            @base.Add(m_instanceAttribute);
            @base.Add(m_bufferAttribute);
            @base.Add(m_versionAttribute);

            return @base;
        }

        public string SampleXml(long startSequence, int requestedCount, out int actualCount, Func<DataItem, bool> filter)
        {
            var data = Sample(startSequence, requestedCount, filter);
            
            actualCount = data.Length;

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "true"));

            var mtcStreamsElement = GetBaseStreamsElement();

            doc.Add(mtcStreamsElement);

            var headerElement = GetBaseHeaderElement()
                .AddAttribute("nextSequence", (data.LastSequence() + 1).ToString())
                .AddAttribute("firstSequence", data.FirstSequence().ToString())
                .AddAttribute("lastSequence", data.LastSequence().ToString());

            mtcStreamsElement.Add(headerElement);

            mtcStreamsElement.Add(data.AsStreamsXml(m_namespace));

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

            var mtcStreamsElement = GetBaseStreamsElement();

            doc.Add(mtcStreamsElement);

            var headerElement = GetBaseHeaderElement()
                .AddAttribute("nextSequence", nextSequence.ToString())
                .AddAttribute("firstSequence", data.FirstSequence().ToString())
                .AddAttribute("lastSequence", data.LastSequence().ToString());

            mtcStreamsElement.Add(headerElement);

            mtcStreamsElement.Add(data.AsStreamsXml(m_namespace));

            return doc.ToString();
        }

        public string CurrentXml(Func<DataItem, bool> filter)
        {
            long nextSequence;

            var data = Current(out nextSequence, filter);

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "true"));

            var mtcStreamsElement = GetBaseStreamsElement();

            doc.Add(mtcStreamsElement);

            var headerElement = GetBaseHeaderElement()
                .AddAttribute("nextSequence", nextSequence.ToString())
                .AddAttribute("firstSequence", data.FirstSequence().ToString())
                .AddAttribute("lastSequence", data.LastSequence().ToString());

            mtcStreamsElement.Add(headerElement);

            mtcStreamsElement.Add(data.AsStreamsXml(m_namespace));

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

        public object GetCurrentValue(string dataItemID)
        {
            DataItemValue item = null;

            if (dataItemID  == null) return null;
            if (m_currentValues.Count == 0) return null;
            if (m_currentValues.Keys == null) return null;

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
