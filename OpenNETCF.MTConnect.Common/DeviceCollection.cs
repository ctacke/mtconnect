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
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.MTConnect
{
    public class DeviceCollection : IEnumerable<Device>
    {
        private Dictionary<string, Device> m_devices = new Dictionary<string, Device>();

        public DateTime CreateTime { get; internal set; }
        public AgentInformation AgentInformation { get; internal set; }

        public DeviceCollection()
        {
        }

        public DeviceCollection(Device device)
        {
            Add(device);
        }

        public DeviceCollection(IEnumerable<Device> devices)
        {
            AddRange(devices);
        }

        public void Add(Device device)
        {
            m_devices.Add(device.Name, device);
        }

        public void AddRange(IEnumerable<Device> devices)
        {
            foreach (var d in devices) { Add(d); }
        }

        public void Remove(Device device)
        {
            m_devices.Remove(device.Name);
        }

        public Device this[string name]
        {
            get 
            {
                if (!m_devices.ContainsKey(name)) return null;

                return m_devices[name]; 
            }
        }

        public int Count
        {
            get { return m_devices.Count; }
        }

        public IEnumerator<Device> GetEnumerator()
        {
            return m_devices.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Clear()
        {
            m_devices.Clear();
        }
    }
}
