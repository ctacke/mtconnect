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

namespace OpenNETCF.MTConnect
{
    public static class ConfigParser
    {
        public static XElement[] GetDeviceElements(XDocument document)
        {
            return document.Descendants(document.Root.Name.Namespace + "Device").ToArray();
        }

        public static XElement GetDeviceElement(XDocument document, string deviceName)
        {
            XElement deviceElement;
            if (deviceName == null)
            {
                deviceElement = document.Descendants(document.Root.Name.Namespace + "Device").First();
            }
            else
            {
                deviceElement = (from d in document.Descendants(document.Root.Name.Namespace + "Device")
                                 where d.Attribute("name").Value == deviceName
                                 select d).FirstOrDefault();
            }
            return deviceElement;
        }

        public static PropertyCollection GetDeviceProperties(XElement deviceElement)
        {
            var pc = PropertyCollection.FromAttributes(deviceElement.Attributes());
            return pc;
        }

        public static ComponentDescription GetComponentDescription(XElement componentElement)
        {
            var descriptionNode = componentElement.Element(componentElement.Name.Namespace + "Description");
            if (descriptionNode == null) return null;

            ComponentDescription desc = new ComponentDescription();

            var attributes = PropertyCollection.FromAttributes(descriptionNode.Attributes());

            desc.SerialNumber = attributes["serialNumber"];
            desc.Manufacturer = attributes["manufacturer"];
            desc.Station = attributes["station"];

            desc.Description = descriptionNode.Value;

            return desc;
        }

        public static ComponentDescriptor[] GetComponentDescriptors(XElement componentElement)
        {
            List<ComponentDescriptor> descriptors = new List<ComponentDescriptor>();
            var componentsList = componentElement.Element(componentElement.Name.Namespace + "Components");

            if (componentsList != null)
            {
                foreach (var compNode in componentsList.Elements())
                {
                    var cd = new ComponentDescriptor(compNode.Name.LocalName, PropertyCollection.FromAttributes(compNode.Attributes()));
                    descriptors.Add(cd);
                }
            }

            return descriptors.ToArray();
        }

        public static PropertyCollection[] GetComponentDataItems(XElement componentElement)
        {
            var itemsNode = componentElement.Element(componentElement.Name.Namespace + "DataItems");
            if (itemsNode == null) return null;

            List<PropertyCollection> propsList = new List<PropertyCollection>();
            foreach (var item in itemsNode.Elements(componentElement.Name.Namespace + "DataItem"))
            {
                propsList.Add(PropertyCollection.FromAttributes(item.Attributes()));
            }

            return propsList.ToArray();

        }

        internal static DeviceCollection ParseConfigFile(Stream configFile)
        {
            string xml;

            // read the data
            using (var reader = new StreamReader(configFile))
            {
                xml = reader.ReadToEnd();
            }

            return ParseConfigFile(xml);
        }

        internal static DeviceCollection ParseConfigFile(string xml)
        {
            var deviceList = new DeviceCollection();


            var doc = XDocument.Parse(xml);
            var ns = doc.Root.Name.Namespace;

            var devices =
                from d in doc.Descendants(ns + NodeNames.Device)
                select d;


            foreach (var deviceNode in devices)
            {
                deviceList.Add(Device.FromXElement<Device>(ns, deviceNode));
            }

            return deviceList;
        }

        internal static Dictionary<string, string> GetAttributes(XElement element)
        {
            var attribs = new Dictionary<string, string>();

            foreach(var attrib in element.Attributes())
            {
                attribs.Add(attrib.Name.LocalName, attrib.Value);
            }

            return attribs;
        }
    }
}
