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
    public class DataItemCollection : IEnumerable<DataItem>
    {
        internal event EventHandler<DataItemValue> DataItemValueSet;

        private Dictionary<string, DataItem> m_items = new Dictionary<string, DataItem>();
        private ComponentBase m_parent;

        internal DataItemCollection(DataItemCollection collection)
        {
            foreach (var di in collection)
            {
                this.Add(new DataItem(di));
            }
            m_parent = collection.Parent;
        }

        internal DataItemCollection(ComponentBase parent)
        {
            m_parent = parent;
        }

        internal Dictionary<string, DataItem>.ValueCollection Items
        {
            get { return m_items.Values; }
        }

        internal ComponentBase Parent
        {
            get { return m_parent; }
        }

        public int Count
        {
            get { return m_items.Count; }
        }

        internal void Add(DataItem item)
        {
            item.Component = m_parent;
            m_items.Add(item.ID, item);
            item.ValueSet += item_ValueSet;
        }

        internal void Remove(DataItem item)
        {
            item.ValueSet -= item_ValueSet;
            item.RemoveEvent();
            m_items.Remove(item.ID);
        }

        public void Clear()
        {
            m_items.Clear();
        }

        void item_ValueSet(object sender, DataItemValue e)
        {
            DataItemValueSet.Fire(sender, e);
        }

        internal void AddRange(IEnumerable<DataItem> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                m_items.Add(item.Name, item);
            }
        }

        public IEnumerator<DataItem> GetEnumerator()
        {
            return m_items.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public XElement AsXElement(XNamespace ns)
        {
            var element = new XElement(ns + NodeNames.DataItems);

            foreach (var di in this)
            {
                element.Add(di.AsXElement(ns));
            }

            return element;
        }

        internal void Validate()
        {
            foreach (var di in this)
            {
                di.Validate();
            }
        }

        public DataItem this[string id]
        {
            get 
            {
                if (m_items.ContainsKey(id))
                {
                    return m_items[id];
                }

                // traverse subcomponents looking for the item
                foreach (var subcomponent in m_parent.Components)
                {
                    var item = subcomponent.DataItems[id];
                    if (item != null) return item;
                }

                return null;
            }
        }
    }
}
