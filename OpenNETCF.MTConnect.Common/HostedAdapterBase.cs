// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://mtcagent.codeplex.com
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace OpenNETCF.MTConnect
{
    public abstract partial class HostedAdapterBase : IHostedAdapter, IDisposable
    {
        protected int m_refreshPeriod = 5000;
        protected const string m_separator = ".";
        protected IAgentInterface m_agentInterface;        
        protected AutoResetEvent m_shutdownEvent = new AutoResetEvent(false);

        public Device Device { get; protected set; }
        public bool Loaded { get; protected set; }
        public virtual string AdapterType { get; protected set; }

        public abstract IHostedDevice HostedDevice { get; }

        public virtual void OnConfigurationChange() { }
        public virtual void OnError(Exception exception) { }
        public virtual void OnNewAgentInterface() { }
        public virtual void BeforeLoad() { }
        public virtual void AfterLoad() { }

        public HostedAdapterBase()
        {
            AdapterType = "Model";
        }

        ~HostedAdapterBase()
        {
            Dispose();
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
            m_shutdownEvent.Set();
        }

        public virtual void PublishDefaultData() 
        {
            Debug.WriteLine("PublishDefaultData for " + this.Device.Name);
        }

        public virtual string ID { get; set; }
        //{
        //    get { return this.GetType().Assembly.GetName().Name; }
        //}

        public virtual string AssemblyName
        {
            get { return Path.GetFileNameWithoutExtension(this.GetType().Assembly.FullName); }
        }

        public IAgentInterface AgentInterface
        {
            get { return m_agentInterface; }
            set
            {
                m_agentInterface = value;
                m_agentInterface.DataItemValueChanged += new EventHandler<DataItemValue>(OnDataItemValueChanged);
                OnNewAgentInterface();
            }
        }

        public virtual void OnDataItemValueChanged(object sender, DataItemValue e)
        {
            if (!Loaded) return;
            if (sender == this) return;
            if (m_firstPublish) return;

            CheckForSetProperty(e);
        }

        public virtual void CreateDeviceAndComponents()
        {
            Device = new Device(HostedDevice.Name, HostedDevice.Name, HostedDevice.Name);

            if (HostedDevice.Components != null)
            {
                foreach (var component in HostedDevice.Components)
                {
                    Device.AddComponent(new Component("Library", component.Name, component.Name));
                }
            }
        }

        public void LoadPropertiesAndMethods()
        {
            LoadProperties();
            LoadMethods();
        }

        public void StartPublishing()
        {

            var threadName = string.Format("{0}[{1}]", this.GetType().Name, this.HostedDevice.Name);
            new Thread(PublisherProc)
            {
                IsBackground = true,
                Name = threadName
            }
           .Start();

            Loaded = true;
        }

        public virtual void AgentInitialized() 
        { 
            Device.SetAvailability(true);
        }

        protected virtual void OnPublish()
        {
            // derived classes will override this to provide publishing logic
        }

        protected void PublisherProc()
        {
            // publish default/startup property data
            UpdateProperties();

            // do periodic publishing work here
            while (!m_shutdownEvent.WaitOne(m_refreshPeriod, false))
            {
                OnPublish();
            }
        }
    }

}
