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
    public class AdapterCollection : IEnumerable<Adapter>
    {
        internal event EventHandler<DataItemValue> DataItemValueSet;
        internal event EventHandler<GenericEventArgs<Adapter>> AdapterAdded;
        internal event EventHandler<GenericEventArgs<Adapter>> AdapterRemoved;
        internal event EventHandler Cleared;

        private List<Adapter> m_adapters = new List<Adapter>();

        internal Agent Agent { get; private set; }
        public object SyncRoot { get; private set; }

        internal AdapterCollection(Agent agent)
        {
            SyncRoot = new object();
            Agent = agent;
        }

        public IEnumerator<Adapter> GetEnumerator()
        {
            return m_adapters.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Adapter this[int index]
        {
            get 
            {
                lock (SyncRoot)
                {
                    return m_adapters[index];
                }
            }
        }

        public void AddRange(IEnumerable<Adapter> adapters)
        {
            lock (SyncRoot)
            {
                foreach (var adapter in adapters)
                {
                    Add(adapter);
                }
            }
        }

        public void Add(Adapter adapter)
        {
            if (adapter.Device == null)
            {
                // make sure it's valid before we add it
                var device = new Device(adapter.GetDeviceProperties());

                device.Validate();

                adapter.Device = device;

                device.Description = adapter.GetComponentDescription(device);

                var dataItems = adapter.GetDeviceDataItems();
                if (dataItems != null)
                {
                    foreach (var di in dataItems)
                    {
                        device.DataItems.Add(new DataItem(di));
                    }
                }

                foreach (var cd in adapter.GetComponentDescriptors(device))
                {
                    var component = new Component(cd, device);
                    component.Description = adapter.GetComponentDescription(component);

                    dataItems = adapter.GetComponentDataItems(component);
                    if (dataItems != null)
                    {
                        foreach (var di in dataItems)
                        {
                            component.DataItems.Add(new DataItem(di));
                        }
                    }

                    // get subcomponents
                    foreach (var subcomponentDescriptor in adapter.GetComponentDescriptors(component))
                    {
                        var subcomponent = new Component(subcomponentDescriptor, device);

                        dataItems = adapter.GetComponentDataItems(subcomponent);
                        if (dataItems != null)
                        {
                            foreach (var di in dataItems)
                            {
                                subcomponent.DataItems.Add(new DataItem(di));
                            }
                        }

                        component.Components.Add(subcomponent);
                    }

                    device.Components.Add(component);
                }
            }

            ValidateAdapter(adapter);

            lock (SyncRoot)
            {
                adapter.DataItemValueSet += adapter_DataItemValueSet;
                adapter.Container = this;
                m_adapters.Add(adapter);
            }

            AdapterAdded.Fire(this, new GenericEventArgs<Adapter>(adapter));
        }

        public void Remove(string deviceID)
        {
            lock (SyncRoot)
            {
                var adapter = m_adapters.Find(t => t.Device.ID == deviceID);
                if (adapter != null)
                {
                    adapter.DataItemValueSet -= adapter_DataItemValueSet;
                    adapter.Container = null;
                    m_adapters.Remove(adapter);
                    AdapterRemoved.Fire(this, new GenericEventArgs<Adapter>(adapter));
                    adapter = null;
                }
            }
        }

        void adapter_DataItemValueSet(object sender, DataItemValue e)
        {
            DataItemValueSet.Fire(sender, e);
        }

        public int Count
        {
            get { return m_adapters.Count; }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                for(int i = 0 ; i < m_adapters.Count ; i++)
                {
                    var adapter = m_adapters[0];
                    m_adapters.RemoveAt(0);
                    adapter.DataItemValueSet -= adapter_DataItemValueSet;
                    adapter.Container = null;
                    adapter.Device = null;
                    adapter = null;
                }

                m_adapters = new List<Adapter>();
            }
            Cleared.Fire(this, EventArgs.Empty);
        }

        private void ValidateAdapter(Adapter adapter)
        {
            Validate.Begin()
                .IsNotNull(adapter);

            if(adapter.Device == null)
            {
                throw new  InvalidAdapterException(adapter, "Adapter Device cannot be null");
            }

            foreach (var a in this)
            {
                if (a.Device.UUID == adapter.Device.UUID)
                {
                    throw new DeviceAlreadyExistsException("Duplicate device UUID");
                }
            }

            adapter.Device.Validate();

        }
    }
}
