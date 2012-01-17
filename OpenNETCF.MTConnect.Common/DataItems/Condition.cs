// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://mtconnect.codeplex.com
//
// Copyright (c) 2010-2012 OpenNETCF Consulting
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

namespace OpenNETCF.MTConnect
{
    public enum ConditionValue
    {
        Unavailable,
        Normal,
        Warning,
        Fault
    }

    public class ConditionCollection : IEnumerable<Condition>
    {
        private List<Condition> m_conditions = new List<Condition>();

        public ConditionCollection()
        {
        }

        public IEnumerator<Condition> GetEnumerator()
        {
            return m_conditions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Condition condition)
        {
            m_conditions.Add(condition);
        }

        /// <summary>
        /// Performs a deep copy
        /// </summary>
        /// <returns></returns>
        public ConditionCollection Copy()
        {
            var copy = new ConditionCollection();

            foreach (var c in this)
            {
                copy.Add(c.Copy());
            }

            return copy;
        }

        public Condition this[int index]
        {
            get { return m_conditions[index]; }
        }

        public Condition this[string id]
        {
            get { return m_conditions.FirstOrDefault(c => c.ID == id); }
        }

        public XElement AsXElement(XNamespace ns)
        {
            var element = new XElement(ns + NodeNames.Conditions);

            foreach (var c in this)
            {
                element.Add(c.StructureAsXElement(ns));
            }

            return element;
        }
    }

    public class ConditionState
    {
        public event EventHandler StateChanged;

        private ConditionValue m_value;

        public string Text { get; set; }
        public string Qualifier { get; set; }
        public string NativeCode { get; set; }
        public string NativeSeverity { get; set; }

        public ConditionState()
        {
            Value = ConditionValue.Normal;
        }

        public ConditionValue Value 
        {
            get { return m_value; }
            set
            {
                if (m_value == value) return;

                m_value = value;
                StateChanged.Fire(this, EventArgs.Empty);
            }
        }
    }

    public class Condition : DataItem
    {
        private ConditionValue m_value;

        public Condition(string id)
            : this()
        {
            this.ID = id;
        }

        internal Condition()
        {
            this.Category = DataItemCategory.Condition;
            this.Type = DataItemType.OTHER.ToString();
            this.SubType = DataItemSubtype.NONE.ToString();
        }

        public override void SetValue(object value, DateTime time)
        {
            var source = value as Condition;
            if (source != null)
            {
                Qualifier = source.Qualifier;
                NativeCode = source.NativeCode;
                NativeSeverity = source.NativeSeverity;
                Text = source.Text;
                this.Value = source.Value;
            }
            else
            {
                throw new Exception();
            }
        }

        public ConditionValue Value
        {
            get { return m_value; }
            set
            {
                if (m_value == value) return;

                m_value = value;

                RaiseValueSet(this, DateTime.Now);
            }
        }

        public string Text { get; set; }

        public string Qualifier
        {
            get { return Properties[CommonProperties.Qualifier]; }
            set { SetProperty(CommonProperties.Qualifier, value); }
        }

        public string NativeCode
        {
            get { return Properties[CommonProperties.NativeCode]; }
            set { SetProperty(CommonProperties.NativeCode, value); }
        }

        public string NativeSeverity
        {
            get { return Properties[CommonProperties.NativeSeverity]; }
            set { SetProperty(CommonProperties.NativeSeverity, value); }
        }

        /// <summary>
        /// Performs a deep copy
        /// </summary>
        /// <returns></returns>
        public Condition Copy()
        {
            return new Condition()
            {
                ID = this.ID,
                Name = this.Name,
                Type = this.Type,
                SubType = this.SubType
            };
        }

        public XElement StructureAsXElement(XNamespace ns)
        {
            var element = new XElement(ns + NodeNames.DataItem,
                new XAttribute(XmlAttributeName.Category, "CONDITION"));

            element
                .AddAttribute(XmlAttributeName.DataItemID, this.ID)
                .AddAttribute(XmlAttributeName.Type, this.Type.ToString())
                .AddAttribute(XmlAttributeName.SubType, this.SubType.ToString())
                .AddAttributeIfHasValue(XmlAttributeName.Name, this.Name);

            return element;
        }

        public override XElement  AsXElement(XNamespace ns)
        {
            var element = new XElement(ns + this.Value.ToString().ToUpper());

            foreach (var prop in Properties)
            {
                element.AddAttributeIfHasValue(prop.Key, prop.Value);
            }

            element
                .AddAttributeIfHasValue(XmlAttributeName.NativeCode, this.NativeCode)
                .AddAttributeIfHasValue(XmlAttributeName.NativeSeverity, this.NativeSeverity)
                .AddAttributeIfHasValue(XmlAttributeName.Qualifier, this.Qualifier);

            if (!this.Text.IsNullOrEmpty())
            {
                element.Value = this.Text;
            }

            return element;
        }
    }
}
