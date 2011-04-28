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
using System.Net;
using System.Threading;

namespace OpenNETCF.MTConnect
{
    public abstract class MTConnectProvider : IMTConnectProvider
    {
        public event EventHandler<GenericEventArgs<StatusSummary>> StatusChanged;

        protected abstract Adapter[] GetAdapters();
        protected abstract IHost GetHost(Agent agent);

        public abstract string AgentAddress { get; }
        public abstract bool Running { get; }
        public abstract void Start();
        public abstract void Stop();
        public abstract void UpdateAdapter(string requestSource, string xml);
        public abstract void SetDataByDataItemID(string requestSource, string dataItemID, string data);

        private static string ConfigPath { get; set; }

        public Agent Agent { get; private set; }
        public IHost Host { get; private set; }

        protected MTConnectProvider(int bufferSize, int checkpointFrequency)
        {
            Agent = new Agent(bufferSize, checkpointFrequency);
            Host = GetHost(Agent);

            foreach (var adapter in GetAdapters())
            {
                Agent.Adapters.Add(adapter);
            }

            Host.Start();
        }

        protected void RaiseStatusUpdateRequested(string source, string description)
        {
            // put this on another thread to prevent it from affecting actual request throughput
            ThreadPool.QueueUserWorkItem(delegate
            {
                StatusChanged.Fire(this, new GenericEventArgs<StatusSummary>(new StatusSummary
                {
                    Source = source,
                    Description = description,
                    TimeStamp = DateTime.Now
                }));
            });
        }
    }
}
