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
    public class DataItemValue : EventArgs
    {
        public DataItemValue(long sequence, DataItem item, string value)
            : this(sequence, item, value, DateTime.Now)
        {
        }

        public DataItemValue(long sequence, DataItem item, string value, DateTime time)
        {
            Sequence = sequence;
            Item = item;
            Value = value;
            Time = time;
        }

        public long Sequence { get; internal set; }
        public DataItem Item { get; private set; }
        public string Value { get; set; }
        public DateTime Time { get; private set; }

        internal XElement AsXml(XNamespace ns)
        {
            XElement element;

            switch (Item.Category)
            {
                case DataItemCategory.Condition:
                    element = new XElement(ns + this.Value)
                        .AddAttribute(AttributeNames.DataItemId, this.Item.ID)
                        .AddAttribute(AttributeNames.Sequence, this.Sequence.ToString())
                        .AddAttribute(AttributeNames.Timestamp, this.Time.ToString("s"))
                        .AddAttribute(AttributeNames.Type, this.Item.Type.ToString())
                        .AddAttributeIfHasValue(AttributeNames.Name, this.Item.Name)
                        .AddAttributeIfHasValue(AttributeNames.Qualifier, this.Item.Properties[CommonProperties.Qualifier])
                        .AddAttributeIfHasValue(AttributeNames.NativeCode, this.Item.Properties[CommonProperties.NativeCode])
                        .AddAttributeIfHasValue(AttributeNames.NativeSeverity, this.Item.Properties[CommonProperties.NativeSeverity]);

                    // TODO: xs:lang

                    break;
                case DataItemCategory.Event:
                case DataItemCategory.Sample:
                    element = new XElement(ns + TypeToCamel(this.Item.Type))
                        .AddAttribute(AttributeNames.DataItemId, this.Item.ID)
                        .AddAttribute(AttributeNames.Sequence, this.Sequence.ToString())
                        .AddAttribute(AttributeNames.Timestamp, this.Time.ToString("s"))
                        .AddAttributeIfHasValue(AttributeNames.Name, this.Item.Name);
                    
                    element.Value = Value;

                    break;
                default:
                    throw new NotSupportedException("Invalid Category");
            }
            return element;
        }

        private static string TypeToCamel(string typeName)
        {
            // the type name of a data item, when used as an XML node name needs to be camel-cased
            // with the underscores removed.  This method does that.  Some examples:
            // DOOR_STATE --> DoorState
            // ANGLE      --> Angle
            // 

            typeName = typeName.ToUpper();

            string src = typeName.ToString();
            List<char> dest = new List<char>();

            for (int i = 0; i < src.Length; i++)
            {
                if (i == 0)
                {
                    dest.Add(src[i]);
                    continue;
                }

                if (src[i] == '_')
                {
                    var c = src[++i];
                    dest.Add(c);
                }
                else
                {
                    dest.Add((char)(src[i] + 32));
                }
            }

            return new string(dest.ToArray(), 0, dest.Count);
        }
    }
}
