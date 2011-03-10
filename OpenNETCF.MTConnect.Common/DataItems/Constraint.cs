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
    public class Constraint
    {
        private string m_value;
        private double? m_minimum;
        private double? m_maximum;

        public Constraint(string value)
        {
            Value = value;
        }

        public Constraint(double minimum, double maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public string Value 
        {
            get { return m_value; }
            set
            {
                if (value != null)
                {
                    if (Minimum.HasValue || Maximum.HasValue)
                    {
                        throw new InvalidOperationException("Value cannot be set if a Minimum or Maximum already exist");
                    }
                }
                m_value = value;
            }
        }

        public double? Minimum
        {
            get { return m_minimum; }
            set 
            {
                if (value != null)
                {
                    if (value != null)
                    {
                        throw new InvalidOperationException("Minimum cannot be set if a Value exists");
                    }
                }
                m_minimum = value; 
            } 
        }

        public double? Maximum
        {
            get { return m_maximum; }
            set 
            {
                if (value != null)
                {
                    if (value != null)
                    {
                        throw new InvalidOperationException("Maximum cannot be set if a Value exists");
                    }
                }
                m_maximum = value; 
            } 
        }
    }
}
