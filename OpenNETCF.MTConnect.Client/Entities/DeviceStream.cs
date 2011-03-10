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
    public class DeviceStream
    {
        public string Name { get; private set; }
        public string UUID { get; private set; }

        public Sample[] Samples { get; private set; }
        public Event[] Events { get; private set; }
        public Condition[] Conditions { get; private set; }
        public ComponentStream[] ComponentStreams { get; set; }

        internal DeviceStream()
        {
        }

        internal static DeviceStream FromXml(XNamespace ns, XElement element)
        {
            var ds = new DeviceStream();

            var attr = element.Attribute("name");
            if (attr == null)
            {
                throw new Exception("Device Stream missing 'name' attribute"); 
            }
            ds.Name = attr.Value;

            attr = element.Attribute("uuid");
            if (attr == null)
            {
                throw new Exception("Device Stream missing 'uuid' attribute");
            }
            ds.UUID = attr.Value;

            // <samples>
            var samples = new List<Sample>();
            var samplesElement = element.Element(ns + "Samples");
            if (samplesElement != null)
            {
                foreach (var s in samplesElement.Elements())
                {
                    samples.Add(Sample.FromXml(ns, s));
                }
            }
            ds.Samples = samples.ToArray();

            // <events>
            var events = new List<Event>();
            var eventsElement = element.Element(ns + "Events");
            if (eventsElement != null)
            {
                foreach (var s in eventsElement.Elements())
                {
                    events.Add(Event.FromXml(ns, s));
                }
            }
            ds.Events = events.ToArray();

            // <condition>
            var conditions = new List<Condition>();
            foreach (var ce in element.Elements(ns + "Condition"))
            {
                foreach (var c in ce.Elements())
                {
                    conditions.Add(Condition.FromXml(ns, c));
                }
            }
            ds.Conditions = conditions.ToArray();

            List<ComponentStream> streams = new List<ComponentStream>();

            foreach (var componentStream in element.Elements(ns + "ComponentStream"))
            {
                streams.Add(ComponentStream.FromXml(ns, componentStream));
            }

            ds.ComponentStreams = streams.ToArray();

            return ds;
        }
    }
}
