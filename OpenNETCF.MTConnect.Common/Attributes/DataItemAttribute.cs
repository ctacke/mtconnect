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

namespace OpenNETCF.MTConnect
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DataItemAttribute : Attribute
    {
        public string ID { get; set; }
        public DataItemType ItemType { get; set; }
        public DataItemSubtype ItemSubType { get; set; }
        public string Units { get; set; }
        public string CoordianteSystem { get; set; }
        public string UUID { get; set; }
        public string Source { get; set; }
        public string SignificantDigits { get; set; }
        public string NativeUnits { get; set; }
        public string NativeScale { get; set; }

        public DataItemAttribute()
        {
            ItemSubType = DataItemSubtype.NONE;
            ItemType = DataItemType.OTHER;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EventDataItemAttribute : DataItemAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SampleDataItemAttribute : DataItemAttribute
    {
        public SampleDataItemAttribute(string units)
        {
            Units = units;
        }
    }
}
