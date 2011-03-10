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
    public abstract class Adapter : IAdapter
    {
        public event EventHandler<DataItemValue> DataItemValueSet;

        public abstract void Start();
        public abstract PropertyCollection GetDeviceProperties();
        public abstract ComponentDescription GetComponentDescription(ComponentBase component);
        public abstract PropertyCollection[] GetDeviceDataItems();
        public abstract ComponentDescriptor[] GetComponentDescriptors(ComponentBase parent);
        public abstract PropertyCollection[] GetComponentDataItems(ComponentBase parent);

        private Device m_device;

        public Device Device 
        {
            get { return m_device; }
            internal set
            {
                if (m_device != null)
                {
                    m_device.DataItemValueSet -= Device_DataItemValueSet;
                }

                m_device = value;
                m_device.DataItemValueSet += Device_DataItemValueSet;
            }
        }

        private void Device_DataItemValueSet(object sender, DataItemValue e)
        {
            OnDataItemValueSet(e);
        }

        public virtual void OnDataItemValueSet(DataItemValue value)
        {
            DataItemValueSet.Fire(Device, value);
        }

        public Adapter()
        {
        }

        internal Adapter(Device device)
        {
            Device = device;
        }
    }
}
