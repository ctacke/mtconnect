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
using System.Diagnostics;
using System.Xml.Linq;

namespace OpenNETCF.MTConnect
{
    public class Component : ComponentBase
    {
        private Device m_device;

        internal Component(PropertyCollection props, Device device)
            : base(props)
        {
            m_device = device;
        }

        internal Component(ComponentDescriptor descriptor, Device device)
            : base(descriptor.Properties)
        {
            XmlNodeName = descriptor.Type;
            m_device = device;
        }

        public Component(ComponentType componentType, string name, string id)
            : this(componentType.ToString(), name, id)
        {
        }

        public Component(string componentType, string name, string id)
        {
            OpenNETCF.Validate
                .Begin()
                .IsNotNullOrEmpty(componentType)
                .IsNotNullOrEmpty(name)
                .IsNotNullOrEmpty(id)
                .Check();

            this.Name = name;
            this.ID = id;
            this.XmlNodeName = componentType.ToString();
        }

        public Component(ComponentType componentType, string name, string id, params Component[] subcomponents)
            : this(componentType.ToString(), name, id)
        {
            if (subcomponents != null)
            {
                foreach (var subcomponent in subcomponents)
                {
                    this.AddComponent(subcomponent);
                }
            }
        }

        public Component(string componentType, string name, string id, params Component[] subcomponents)
            : this(componentType.ToString(), name, id)
        {
            if (subcomponents != null)
            {
                foreach (var subcomponent in subcomponents)
                {
                    this.AddComponent(subcomponent);
                }
            }
        }

        public Component(ComponentType componentType, string name, string id, params DataItem[] dataItems)
            : this(componentType.ToString(), name, id)
        {
            if (dataItems != null)
            {
                foreach (var dataitem in dataItems)
                {
                    this.AddDataItem(dataitem);
                }
            }
        }

        public Component(string componentType, string name, string id, params DataItem[] dataItems)
            : this(componentType.ToString(), name, id)
        {
            if (dataItems != null)
            {
                foreach (var dataitem in dataItems)
                {
                    this.AddDataItem(dataitem);
                }
            }
        }

        public Device Device
        {
            get { return m_device; }
            internal set 
            { 
                m_device = value;
                foreach (var di in this.DataItems)
                {
                    di.Device = m_device;
                }
            }
        }

        public override XElement AsXElement(XNamespace ns)
        {
            if (ns == null) ns = string.Empty;
            var element = new XElement(ns + XmlNodeName);

            foreach (var prop in Properties)
            {
                element.AddAttributeIfHasValue(prop.Key, prop.Value);
            }

            element
                .AddChildElement(DataItems.AsXElement(ns))
                .AddChildElement(Components.AsXElement(ns));

            return element;
        }

        public string SampleRate
        {
            get { return Properties[CommonProperties.SampleRate]; }
            set { SetProperty(CommonProperties.SampleRate, value); }
        }

        public string NativeName
        {
            get { return Properties[CommonProperties.NativeName]; }
            set { SetProperty(CommonProperties.NativeName, value); }
        }

        public string ComponentType
        {
            get { return Properties[CommonProperties.Type]; }
            set { SetProperty(CommonProperties.Type, value); }
        }
    }
}
