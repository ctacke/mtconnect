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

        private static void FillElementDataItems(XElement componentElement, ComponentBase component, DataItemValue[] items, XNamespace ns)
        {
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
                    samplesElement.Add(sample.AsXml(ns));
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
                    conditionElement.Add(condition.AsXml(ns));
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
                    eventsElement.Add(evt.AsXml(ns));
                }
                componentElement.Add(eventsElement);
            }
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

                var dev = (from i in items
                                  where i.Item.Device.UUID == device.UUID
                                  && i.Item.Device.ID == i.Item.Component.ID
                                  select i.Item.Component).Distinct().FirstOrDefault();

                if (dev != null)
                {
                    FillElementDataItems(deviceElement, dev, items, ns);
                }

                var components = (from i in items
                                  where i.Item.Device.UUID == device.UUID
                                  && i.Item.Device.ID != i.Item.Component.ID
                                  select i.Item.Component).Distinct();

                foreach (var component in components)
                {
                    var componentElement = new XElement(ns + "ComponentStream")
                        .AddAttribute("component", component.XmlNodeName)
                        .AddAttribute("componentId", component.ID)
                        .AddAttributeIfHasValue("name", component.Name);

                    FillElementDataItems(componentElement, component, items, ns);

                    deviceElement.Add(componentElement);
                }

                streamsElement.Add(deviceElement);
            }

            return streamsElement;
        }

        public static XElement AsStreamsXml_old(this DataItemValue[] items, XNamespace ns)
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
                                 orderby i.Item.Device.ID
                                 select i.Item.Component).Distinct();

                foreach (var component in components)
                {
                    XElement componentElement;
                    if (component.ID == device.ID)
                    {
                        componentElement = deviceElement;
                    }
                    else
                    {
                        componentElement = new XElement(ns + "ComponentStream")
                            .AddAttribute("component", component.XmlNodeName)
                            .AddAttribute("componentId", component.ID)
                            .AddAttributeIfHasValue("name", component.Name);
                    }


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
                            samplesElement.Add(sample.AsXml(ns));
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
                            conditionElement.Add(condition.AsXml(ns));
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
                            eventsElement.Add(evt.AsXml(ns));
                        }
                        componentElement.Add(eventsElement);
                    }

                    if (component.ID != device.ID)
                    {
                        deviceElement.Add(componentElement);
                    }
                }

                streamsElement.Add(deviceElement);
            }

            return streamsElement;
        }
    }
}
