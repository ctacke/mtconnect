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
using System.Diagnostics;

namespace OpenNETCF.Net.MTConnect
{
    public class CurrentRequest : ResourceRequestBase
    {
        protected override string[] ParameterNames
        {
            get
            {
                return new string[] 
                {
                    "path",
                    "frequency",
                    "at"
                };
            }
        }

        public string Path 
        {
            get
            {
                if (!Parameters.ContainsKey("path"))
                {
                    return null;
                }

                return Parameters["path"];
            }
        }

        public int? Frequency
        {
            get
            {
                if (!Parameters.ContainsKey("frequency"))
                {
                    return null;
                }

                int value;

                try
                {
                    value = int.Parse(Parameters["frequency"]);
                }
                catch
                {
                    throw new RequestParameterException(string.Format("Parameter 'frequency' value of '{0}' is invalid", Parameters["frequency"]));
                }

                return value;
            }
        }

        public int? At
        {
            get
            {
                if (!Parameters.ContainsKey("at"))
                {
                    return null;
                }

                int value;

                try
                {
                    value = int.Parse(Parameters["at"]);
                }
                catch
                {
                    throw new RequestParameterException(string.Format("Parameter 'at' value of '{0}' is invalid", Parameters["at"]));
                }

                return value;
            }
        }

        protected override void AfterParameterParse()
        {
            if (this.Frequency.HasValue && this.At.HasValue)
            {
                throw new RequestParameterException("Parameters 'frequency' and 'at' are mutually exclusive"); 
            }

            if (this.At.HasValue && At.Value < 0)
            {
                throw new RequestParameterException("Parameter 'at' must be greater than or equal to zero.");
            }

            if (this.Frequency.HasValue && Frequency.Value <= 0)
            {
                throw new RequestParameterException("Parameter 'frequency' must be a positive integer.");
            }

            // check for device filter
            //var index = this.Path.IndexOf("//Device[", StringComparison.InvariantCultureIgnoreCase);
            //if(index >= 0)
            //{
            //    var resourceName = this.Path.Substring(index);
            //    index = resourceName.IndexOf
            //}
        }

    }
}
