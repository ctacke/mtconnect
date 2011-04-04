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

namespace OpenNETCF.MTConnect
{
    public class DataStream
    {
        public string Sender { get; private set; }
        public DateTime CreateTime { get; private set; }
        public int FirstSequence { get; private set; }
        public int LastSequence { get; private set; }
        public int NextSequence { get; private set; }
        public DeviceStream[] DeviceStreams { get; private set; }

        internal DataStream()
        {
        }

        public ISample[] AllSamples()
        {
            List<ISample> samples = new List<ISample>();

            foreach (var device in DeviceStreams)
            {
                samples.AddRange(device.Samples);

                foreach (var component in device.ComponentStreams)
                {
                    samples.AddRange(component.Samples);
                }
            }

            return samples.ToArray();
        }

        public IEvent[] AllEvents()
        {
            List<IEvent> events = new List<IEvent>();

            foreach (var device in DeviceStreams)
            {
                events.AddRange(device.Events);

                foreach (var component in device.ComponentStreams)
                {
                    events.AddRange(component.Events);
                }
            }

            return events.ToArray();
        }

        public ICondition[] AllConditions()
        {
            List<ICondition> conditions = new List<ICondition>();

            foreach (var device in DeviceStreams)
            {
                conditions.AddRange(device.Conditions);

                foreach (var component in device.ComponentStreams)
                {
                    conditions.AddRange(component.Conditions);
                }
            }

            return conditions.ToArray();
        }

        public ISample GetSample(string sampleID)
        {
            foreach (var device in DeviceStreams)
            {
                foreach (var sample in device.Samples)
                {
                    if (sample.DataItemID == sampleID)
                    {
                        return sample;
                    }
                }

                foreach (var component in device.ComponentStreams)
                {
                    foreach (var sample in component.Samples)
                    {
                        if (sample.DataItemID == sampleID)
                        {
                            return sample;
                        }
                    }
                }
            }

            return null;
        }

        internal static DataStream FromXml(string xml)
        {
            var doc = XDocument.Parse(xml);
            var ns = doc.Root.GetDefaultNamespace();
            var root = doc.Element(ns + "MTConnectStreams");

            var dataStream = new DataStream();

            var streams = new List<DeviceStream>();
            var header = root.Element(ns + "Header");

            var attr = header.Attribute("creationTime");
            if (attr != null)
            {
                try
                {
                    dataStream.CreateTime = DateTime.Parse(attr.Value);
                }
                catch { }
            }

            attr = header.Attribute("sender");
            if (attr != null)
            {
                try
                {
                    dataStream.Sender = attr.Value;
                }
                catch { }
            }

            attr = header.Attribute("nextSequence");
            if (attr != null)
            {
                try
                {
                    dataStream.NextSequence = int.Parse(attr.Value);
                }
                catch { }
            }

            attr = header.Attribute("firstSequence");
            if (attr != null)
            {
                try
                {
                    dataStream.FirstSequence = int.Parse(attr.Value);
                }
                catch { }
            }

            attr = header.Attribute("lastSequence");
            if (attr != null)
            {
                try
                {
                    dataStream.LastSequence = int.Parse(attr.Value);
                }
                catch { }
            }
            
            foreach (var deviceStream in root.Element(ns + "Streams").Elements(ns + "DeviceStream"))
            {
                var ds = DeviceStream.FromXml(ns, deviceStream);
                streams.Add(ds);
            }

            dataStream.DeviceStreams = streams.ToArray();

            return dataStream;
        }
    }
}
