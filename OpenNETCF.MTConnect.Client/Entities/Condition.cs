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
    internal static class DataElementFactory
    {
        internal static ICondition ConditionFromXml(XNamespace ns, XElement element)
        {
            return new Condition(ns, element);
        }

        internal static IEvent EventFromXml(XNamespace ns, XElement element)
        {
            string valueType = "string";
            var attr = element.Attribute("valueType");
            if (attr != null)
            {
                valueType = attr.Value;
            }

            IEvent @event;

            switch (valueType.ToLower())
            {
                case "int":
                    @event = new Event<int?>(ns, element);
                    @event.Value = int.Parse(element.Value);
                    break;
                case "boolean":
                    @event = new Event<bool?>(ns, element);
                    @event.Value = bool.Parse(element.Value);
                    break;
                case "string":
                    @event = new Event<string>(ns, element);
                    @event.Value = element.Value;
                    break;
                case "double":
                    @event = new Event<double?>(ns, element);
                    @event.Value = double.Parse(element.Value);
                    break;
                case "datetime":
                    @event = new Event<DateTime?>(ns, element);
                    @event.Value = DateTime.Parse(element.Value);
                    break;
                default:
                    throw new NotSupportedException(string.Format("Event with value type of '{0}' not supported", valueType));
            }

            return @event;
        }

        internal static ISample SampleFromXml(XNamespace ns, XElement element)
        {
            return new Sample(ns, element);
        }
    }

    public class Condition : DataElementBase<string>, ICondition
    {
        public string ConditionType { get; internal protected set; }

        internal Condition(XNamespace ns, XElement element)
            : base(ns, element)
        {
            this.ConditionType = element.Name.LocalName;

            var attr = element.Attribute("type");
            if (attr != null)
            {
                Type = attr.Value;
            }
        }
    }
}
