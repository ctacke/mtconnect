using System;
using System.Net;
using System.Collections.Generic;

namespace OpenNETCF.MTConnect
{
    public static class ComponentExtensions
    {
        public static Component FirstRecursive(this IEnumerable<Component> list, Func<Component, bool> func)
        {
            foreach (var component in list)
            {
                if (func(component)) return component;
            }

            foreach (var component in list)
            {
                var comp = component.Components.FirstRecursive(func);
                if (comp != null) return comp;
            }

            return null;
        }
    }
}
