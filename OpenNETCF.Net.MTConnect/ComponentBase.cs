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
using System.Xml.Linq;
using System.Diagnostics;

namespace OpenNETCF.Net.MTConnect
{
    public abstract class ComponentBase
    {
        internal event EventHandler<DataItemValue> DataItemValueSet;

        public abstract XElement AsXElement(XNamespace ns);

        public ComponentCollection Components { get; private set; }
        public PropertyCollection Properties { get; private set; }
        public DataItemCollection DataItems { get; private set; }
        public ComponentDescription Description { get; internal set; }

        internal string XmlNodeName { get; set; }

        internal ComponentBase()
        {
            Components = new ComponentCollection();
            Properties = new PropertyCollection();

            DataItems = new DataItemCollection(this);

            Components.DataItemValueSet += new EventHandler<DataItemValue>(DataItems_DataItemValueSet);
            DataItems.DataItemValueSet += new EventHandler<DataItemValue>(DataItems_DataItemValueSet);
        }

        internal ComponentBase(PropertyCollection props)
            : this()
        {
            Properties = props;
        }

        public string Name
        {
            get { return Properties[CommonProperties.Name]; }
        }

        public string ID
        {
            get { return Properties[CommonProperties.ID]; }
        }

        public override string ToString()
        {
            return string.Format("'{0}' Component", Name);
        }

        void DataItems_DataItemValueSet(object sender, DataItemValue e)
        {
            DataItemValueSet.Fire(sender, e);
        }

        internal static T FromXElement<T>(XNamespace ns, XElement element)
            where T : ComponentBase
        {
            PropertyCollection props = ConfigParser.GetAttributes(element);
            // the reason for this is to prevent exposing a public, parameterless constructor for Device and Component
            var ctor = typeof(T).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null , new Type[] { typeof(PropertyCollection) }, null);
            T comp = (T)ctor.Invoke(new object[] { props });

            comp.XmlNodeName = element.Name.LocalName;

            var subcomps =
                (from d in element.Descendants(ns + NodeNames.Components)
                 select d).FirstOrDefault();

            if (subcomps != null)
            {
                foreach (var c in subcomps.Elements())
                {
                    Debug.WriteLine(c.Name);

                    var subcomp = ComponentBase.FromXElement<Component>(ns, c);

                    if (subcomp != null)
                    {
                        comp.Components.Add(subcomp);
                    }
                }
            }

            var dataItems =
                (from d in element.Descendants(ns + NodeNames.DataItems)
                 select d).FirstOrDefault();

            if (dataItems != null)
            {
                foreach (var di in dataItems.Elements())
                {
                    var item = DataItem.FromXElement(ns, di);
                    if (item != null)
                    {
                        comp.DataItems.Add(item);
                    }
                }
            }

            return comp;
        }

        internal virtual void Validate()
        {
            if (string.IsNullOrEmpty(ID))
            {
                throw new InvalidComponentException(this, "Component must have an ID");
            }
            if (string.IsNullOrEmpty(Name))
            {
                throw new InvalidComponentException(this, "Component must have a Name");
            }

            DataItems.Validate();
        }
    }
}
