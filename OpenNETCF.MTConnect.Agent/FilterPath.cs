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
using System.Text.RegularExpressions;

namespace OpenNETCF.MTConnect
{
    internal class FilterPath
    {
        internal FilterPath(string resource, string path)
        {
            if (resource != "*")
            {
                Device = resource;
            }

            if (string.IsNullOrEmpty(path)) return;

            var paths = Regex.Split(path, "//");
            var diFilter = paths[paths.Length - 1];
            var cp = new List<string>();

            foreach (var element in paths)
            {

                if (element.StartsWith("DataItem", StringComparison.InvariantCultureIgnoreCase))
                {

                    ComponentPath = new string[paths.Length - 2];
                    Array.Copy(paths, ComponentPath, ComponentPath.Length);

                    var start = diFilter.IndexOf('[') + 1;
                    diFilter = diFilter.Substring(start, diFilter.IndexOf(']') - start);

                    DataItemFilter = diFilter;
                    break;
                }
                else if (!string.IsNullOrEmpty(element))
                {
                    cp.Add(element);
                }

                ComponentPath = cp.ToArray();
            }
        }

        public string Device { get; private set; }
        public string[] ComponentPath { get; private set; }
        public string DataItemFilter { get; private set; }

        public bool ContainsDevice(string deviceName)
        {
            if (string.IsNullOrEmpty(Device)) return true;

            return string.Compare(deviceName, Device, true) == 0;
        }

        public bool HasComponents
        {
            get { return (ComponentPath != null) && (ComponentPath.Length > 0); }
        }

        public bool HasSubcomponents
        {
            get { return (ComponentPath != null) && (ComponentPath.Length > 1); }
        }

        public bool ContainsComponent(string componentName)
        {
            if (!HasComponents) return true;

            return (string.Compare(ComponentPath[0], componentName, true) == 0);
        }

        public bool ContainsSubcomponent(string componentName)
        {
            if (!HasSubcomponents) return true;

            return (string.Compare(ComponentPath[1], componentName, true) == 0);
        }
    }
}
