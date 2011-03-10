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

namespace OpenNETCF.Net.MTConnect
{
    public class SampleRequest : ResourceRequestBase
    {
        private const long DefaultFrom = 0;
        private const int DefaultCount = 100;
        private const int DefaultFrequency = 0;

        protected override string[] ParameterNames
        {
            get
            {
                return new string[] 
                {
                    "path",
                    "from",
                    "count",
                    "frequency",
                    "nextSequence",
                };
            }
        }

        public string Path
        {
            get
            {
                return Parameters["path"];
            }
        }

        public long From
        {
            get 
            {
                if (Parameters.ContainsKey("from"))
                {
                    try
                    {
                        return long.Parse(Parameters["from"]);
                    }
                    catch
                    {
                        return DefaultFrom;
                    }
                }
                else
                {
                    return DefaultFrom;
                }
            }
        }

        public int Count
        {
            get 
            {
                if (Parameters.ContainsKey("count"))
                {
                    try
                    {
                        return int.Parse(Parameters["count"]);
                    }
                    catch
                    {
                        return DefaultCount;
                    }
                }
                else
                {
                    return DefaultCount;
                }
            }
        }

        public int Frequency
        {
            get
            {
                if (Parameters.ContainsKey("frequency"))
                {
                    try
                    {
                        return int.Parse(Parameters["frequency"]);
                    }
                    catch
                    {
                        return DefaultFrequency;
                    }
                }
                else
                {
                    return DefaultFrequency;
                }
            }
        }
    }
}
