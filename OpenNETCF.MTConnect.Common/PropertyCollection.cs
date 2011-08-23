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

namespace OpenNETCF.MTConnect
{
    public sealed class PropertyCollection : IEnumerable<KeyValuePair<string, string>>
    {
        private Dictionary<string, string> m_values = new Dictionary<string, string>();

        internal PropertyCollection()
        {
        }

        public PropertyCollection(PropertyCollection collection)
        {
            foreach (var p in collection)
            {
                this.Add(p.Key, p.Value);
            }
        }

        internal void Add(string name, string value)
        {
            m_values.Add(name, value);
        }

        public int Count
        {
            get { return m_values.Count; }
        }

        public static implicit operator PropertyCollection(Dictionary<string, string> d)
        {
            var c = new PropertyCollection();
            foreach (var item in d) { c.Add(item.Key, item.Value); }
            return c;
        }

        public string this[string propertyName]
        {
            get 
            {
                if (m_values.ContainsKey(propertyName))
                {
                    return m_values[propertyName];
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    if (m_values.ContainsKey(propertyName))
                    {
                        m_values.Remove(propertyName);
                    }
                    return;
                }

                if (!ContainsProperty(propertyName))
                {
                    m_values.Add(propertyName, value);
                }
                else
                {
                    m_values[propertyName] = value;
                }
            }
        }

        public static PropertyCollection FromAttributes(IEnumerable<XAttribute> attributes)
        {
            PropertyCollection pc = new PropertyCollection();

            foreach (var attr in attributes)
            {
                pc.Add(attr.Name.LocalName, attr.Value);
            }

            return pc;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return m_values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal bool ContainsProperty(string propertyName)
        {
            return m_values.ContainsKey(propertyName);
        }
    }

}
