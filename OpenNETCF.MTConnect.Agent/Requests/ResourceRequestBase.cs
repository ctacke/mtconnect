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
    public abstract class ResourceRequestBase : IResourceRequest
    {
        private char[] ParamDelimeter = new char[] { '&' };
        private char[] NameValueDelimeter = new char[] { '=' };

        private Dictionary<string, string> m_parameters = new Dictionary<string, string>(new CaselessComparer());

        public string ResourceName { get; set; }

        abstract protected string[] ParameterNames { get; }
        
        protected virtual void AfterParameterParse()
        {
        }

        private class CaselessComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Compare(x, y, true) == 0;
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }

        public Dictionary<string, string> Parameters
        {
            get { return m_parameters; }
        }

        public void SetParameters(string paramString)
        {
            if (string.IsNullOrEmpty(paramString))
            {
                return;
            }

            List<string> names = new List<string>(ParameterNames);

            foreach (var pair in paramString.Split(ParamDelimeter))
            {
                var nameVal = pair.Split(NameValueDelimeter);

                foreach (var name in names)
                {
                    var index = pair.IndexOf(name + "=", StringComparison.InvariantCultureIgnoreCase);
                    if (index >= 0)
                    {
                        var value = pair.Substring(index + name.Length + 1);
                        m_parameters.Add(name, value);
                        Debug.WriteLine(string.Format("{0}='{1}'", name, value));
                        names.Remove(name);
                        break;
                    }
                }
            }

            AfterParameterParse();
        }
    }
}
