﻿// -------------------------------------------------------------------------------------------------------
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
    public class Device : ComponentBase
    {
        internal Device(PropertyCollection props)
            : base(props)
        {
        }

        public string UUID
        {
            get { return Properties[CommonProperties.UUID]; }
        }

        public override XElement AsXElement(XNamespace ns)
        {
            if (ns == null) ns = string.Empty;
            var element = new XElement(ns + "Device");

            foreach(var prop in Properties)
            {
                element.AddAttributeIfHasValue(prop.Key, prop.Value);
            }

            element
                .AddChildElement(DataItems.AsXElement(ns))
                .AddChildElement(Components.AsXElement(ns));

            return element;
        }

        internal override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(UUID))
            {
                throw new InvalidComponentException(this, "Device must have a UUID");
            }
        }
    }
}