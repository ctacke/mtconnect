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
using System.IO;
using System.Xml;

namespace OpenNETCF.Net.MTConnect
{
    public abstract class XmlDefinedAdapterBase : Adapter
    {
        private XElement m_deviceElement;

        public XmlDefinedAdapterBase(Stream configuration)
            : this(XmlReader.Create(configuration))
        {
        }

        public XmlDefinedAdapterBase(XmlReader reader)
            : this(XDocument.Load(reader))
        {
        }

        public XmlDefinedAdapterBase(XDocument configuration)
        {
            m_deviceElement = ConfigParser.GetDeviceElement(configuration);
        }

        public override PropertyCollection GetDeviceProperties()
        {
            return ConfigParser.GetDeviceProperties(m_deviceElement);
        }

        public override ComponentDescription GetComponentDescription(ComponentBase component)
        {
            XElement element;

            if (component is Device)
            {
                element = m_deviceElement;
            }
            else
            {
                // get the subcomponent node
                element = m_deviceElement
                    .Element(m_deviceElement.Name.Namespace + "Components")
                    .Element(m_deviceElement.Name.Namespace + component.XmlNodeName);
            }

            return ConfigParser.GetComponentDescription(element);
        }

        public override ComponentDescriptor[] GetComponentDescriptors(ComponentBase parent)
        {
            XElement element;

            if (parent is Device)
            {
                element = m_deviceElement;
            }
            else
            {
                // get the subcomponent node
                element = m_deviceElement
                    .Element(m_deviceElement.Name.Namespace + "Components")
                    .Element(m_deviceElement.Name.Namespace + parent.XmlNodeName);
            }

            return ConfigParser.GetComponentDescriptors(element);
        }

        public override PropertyCollection[] GetDeviceDataItems()
        {
            return ConfigParser.GetComponentDataItems(m_deviceElement);

            // TODO: enforce existence of availability?
        }

        public override PropertyCollection[] GetComponentDataItems(ComponentBase parent)
        {
            var componentNode = (from c in m_deviceElement.Descendants(m_deviceElement.Name.Namespace + "Components").Elements()
                                 where (c.Name.LocalName == parent.XmlNodeName) && (c.Attribute("id").Value == parent.ID)
                                 select c).FirstOrDefault();

            if (componentNode == null) return null;

            return ConfigParser.GetComponentDataItems(componentNode);
        }
    }
}
