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
    public class ComponentStream
    {
        public string ComponentType { get; private set; }
        public string Name { get; private set; }
        public string ID { get; private set; }
        public string UUID { get; private set; }
        public string NativeName { get; private set; }

        public Sample[] Samples { get; private set; }
        public Event[] Events { get; private set; }
        public Condition[] Conditions { get; private set; }

        internal ComponentStream()
        {
        }

        internal static ComponentStream FromXml(XNamespace ns, XElement element)
        {
            var cs = new ComponentStream();

            var attr = element.Attribute("component");
            if (attr != null)
            {
                cs.ComponentType = attr.Value;
            }

            attr = element.Attribute("name");
            if (attr != null)
            {
                cs.Name = attr.Value;
            }

            attr = element.Attribute("nativeName");
            if (attr != null)
            {
                cs.NativeName = attr.Value;
            }

            attr = element.Attribute("componentId");
            if (attr != null)
            {
                cs.ID = attr.Value;
            }

            attr = element.Attribute("uuid");
            if (attr != null)
            {
                cs.UUID = attr.Value;
            }

            // <samples>
            var samples = new List<Sample>();
            var samplesElement = element.Element(ns + "Samples");
            if(samplesElement != null)
            {
                foreach (var s in samplesElement.Elements())
                {
                    samples.Add(Sample.FromXml(ns, s));
                }
            }
            cs.Samples = samples.ToArray();

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
            cs.Events = events.ToArray();

            // <condition>
            var conditions = new List<Condition>();
            foreach( var ce in element.Elements(ns + "Condition"))
            {
                foreach (var c in ce.Elements())
                {
                    conditions.Add(Condition.FromXml(ns, c));
                }
            }
            cs.Conditions = conditions.ToArray();

            return cs;
        }
    }
}
