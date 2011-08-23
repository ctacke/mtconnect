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
    public class ComponentCollection : IEnumerable<Component>
    {
        internal event EventHandler<DataItemValue> DataItemValueSet;

        // this apparent duplication is necessary because the CF doesn't support the OrderedDictionary class
        private List<Component> m_components = new List<Component>();
        private Dictionary<string, Component> m_componentDictionary = new Dictionary<string, Component>();

        private object m_syncRoot = new object();

        public ComponentCollection()
        {
        }

        internal ComponentCollection(ComponentCollection collection)
        {
            foreach (var c in collection)
            {
                this.Add(new Component(c));
            }
        }

        public IEnumerator<Component> GetEnumerator()
        {
            return m_components.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Boolean Remove(Component component)
        {
            lock (m_syncRoot)
            {
                if (m_componentDictionary.ContainsKey(component.ID))
                {
                    m_componentDictionary.Remove(component.ID);
                    m_components.Remove(component);
                    return true;
                }
            }
            return false;
        }

        public Component Add(Component component)
        {
            OpenNETCF.Validate
                .Begin()
                .IsNotNull(component)
                .Check();

            lock (m_syncRoot)
            {
                m_components.Add(component);
                m_componentDictionary.Add(component.ID, component);
            }

            component.DataItemValueSet += new EventHandler<DataItemValue>(component_DataItemValueSet);

            return component;
        }

        void component_DataItemValueSet(object sender, DataItemValue e)
        {
            DataItemValueSet.Fire(sender, e);
        }

        public XElement AsXElement(XNamespace ns)
        {
            var element = new XElement(ns + NodeNames.Components);

            foreach (var c in this)
            {
                element.Add(c.AsXElement(ns));
            }

            return element;
        }

        public Boolean HasComponent(string id)
        {
            if (m_componentDictionary == null) return false;
            if (m_componentDictionary.Count == 0) return false;
            if (m_componentDictionary.Keys.Contains(id)) return true;
            return false;
        }

        public Component this[int index]
        {
            get { return m_components[index]; }
        }

        public Component this[string id]
        {
            get { 
                if(!m_componentDictionary.ContainsKey(id))
                {
                    return null;
                }
                return m_componentDictionary[id]; }
        }

        public int Count
        {
            get { return m_components.Count; }
        }
    }
}
