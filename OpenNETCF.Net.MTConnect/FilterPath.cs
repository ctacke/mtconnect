using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenNETCF.Net.MTConnect
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
