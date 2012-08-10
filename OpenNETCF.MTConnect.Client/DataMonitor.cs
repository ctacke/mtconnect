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
using System.Threading;
using OpenNETCF.Web;
using System.Diagnostics;

namespace OpenNETCF.MTConnect
{
    public class DataMonitor : IDisposable
    {
        private readonly bool Profiling = false;
        private int m_lastET;
        private ulong m_totalTicks;
        private ulong m_sumET;
        private bool m_connected;
        private const int MaxSamplesPerQuery = 100;
        private const int ConnectedTries = 5;
        private EntityClient m_client;
        private string m_agentAddress;
        private int m_period;
        private AutoResetEvent m_stopEvent;
        private Dictionary<string, ISample> m_samples = new Dictionary<string, ISample>();
        private Dictionary<string, IEvent> m_events = new Dictionary<string, IEvent>();
        private Dictionary<string, ICondition> m_conditions = new Dictionary<string, ICondition>();
        private static Random m_random = new Random();

        public event EventHandler<GenericEventArgs<ISample>> NewSample;
        public event EventHandler<GenericEventArgs<IEvent>> NewEvent;
        public event EventHandler<GenericEventArgs<ICondition>> NewCondition;

        public event EventHandler<GenericEventArgs<Boolean>> OnConnected;

        public bool Running { get; private set; }
        public bool Connected
        {
            get{ return m_connected;}
            private set
            {
                if (m_connected != value)
                {
                    m_connected = value;
                    if (OnConnected != null)
                    {
                        OnConnected(this, new GenericEventArgs<bool>(value));
                    }
                }
            }
        }
        public int MeanExecutionTime { get; private set; }

        public RestConnector RestConnector { get; private set; }
        public long InstanceID { get { return m_client.InstanceID; } }

        public DataMonitor(EntityClient entityClient)
            : this(entityClient, 1000)
        {
        }

        public DataMonitor(EntityClient entityClient, int period)
        {
#if DEBUG
            Profiling = true;
#endif
            m_agentAddress = entityClient.RestConnector.DeviceAddress;
            m_client = entityClient;
            RestConnector = m_client.RestConnector;
            Period = period;
            m_stopEvent = new AutoResetEvent(false);
            Running = false;
        }

        ~DataMonitor()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Stop();
        }

        public int Period
        {
            get { return m_period; }
            set
            {
                Validate
                    .Begin()
                    .IsGreaterThanOrEqualTo(value, 100)
                    .Check();

                m_period = value;
            }
        }

        public void Start()
        {
            if (Running) return;

            new Thread(MonitorThreadProc) 
            { 
                IsBackground = true,
                Name = string.Format("DataMonitor[{0}]", m_client.AgentAddress)
            }
            .Start();

            Connected = false;
        }

        public void Stop()
        {
#if !WindowsCE
            if (!m_stopEvent.SafeWaitHandle.IsClosed && !m_stopEvent.SafeWaitHandle.IsInvalid)
            {
#endif
                m_stopEvent.Set();

#if !WindowsCE
            }
#endif
        }

        public ISample GetSample(string id)
        {
            lock (m_samples)
            {
                if (m_samples.ContainsKey(id))
                {
                    return m_samples[id];
                }
                return null;
            }
        }

        public IEvent GetEvent(string id)
        {
            lock (m_events)
            {
                if (m_events.ContainsKey(id))
                {
                    return m_events[id];
                }
                return null;
            }
        }

        public ICondition GetCondition(string id)
        {
            lock (m_conditions)
            {
                if (m_conditions.ContainsKey(id))
                {
                    return m_conditions[id];
                }
                return null;
            }
        }

        public ICondition[] Conditions()
        {
            return m_conditions.Values.ToArray();
        }

        private void MonitorThreadProc()
        {
            int reTries = 0;
            int currentWaitPeriod = Period;
            Running = true;

            try
            {
                m_stopEvent.Reset();

                Thread.Sleep(Period);

                while (!(m_stopEvent.WaitOne(0)))
                {
                    var start = Environment.TickCount;

                    var data = m_client.Sample(MaxSamplesPerQuery);
                    if (data != null)
                    {
                        Connected = true;
                        reTries = 0;
                        HandleNewData(data);

                        // make sure we pulled *all* data
                        var remain = data.LastSequence - data.NextSequence + 1;
                        while (remain > 0)
                        {
                            data = m_client.Sample(MaxSamplesPerQuery);
                            if (data == null)
                            {
                                continue;
                            }

                            HandleNewData(data);
                            remain = data.LastSequence - data.NextSequence + 1;
                        }
                    }
                    else
                    {
                        reTries++;
                        if (reTries > ConnectedTries)
                        {
                            Connected = false;
                        }
                    }

                    var et = Environment.TickCount - start;
                    var wait = GetWaitPeriod(et, Connected);

                    Thread.Sleep(wait);
                }
            }
            finally
            {
                Running = false;
            }
        }

        private int GetWaitPeriod(int et, bool connected)
        {
            int actualwait;

            if (connected)
            {
                m_totalTicks++;
                m_sumET += (ulong)et;

                // if execution time took less than the desired check frequency, just wait that amount (minus the execute time)
                if (et < Period) return Period - et;

                // we're slow - this is common on poor (i.e. wireless) networks.  Let's throttle back
                MeanExecutionTime = (int)(m_sumET / m_totalTicks);
                m_lastET = et;

                // Let the last value have a larger effect on the overall mean
                actualwait = (m_lastET + MeanExecutionTime) / 2;
            }
            else
            {
                // we're not connected, so slow way down (5-8 seconds)
                // use some randomization in case we have multiple disconnected Engines we don't flood the system trying to connect to all at once
                actualwait = 5000 + m_random.Next(3000);
            }

            if (Profiling)
            {
#if !MONOTOUCH
                Trace.WriteLine(string.Format("DataMonitor period on {0} slowed to {1}ms", this.m_agentAddress, actualwait));
#endif
            }
            
            return actualwait;
        }

        private void HandleNewData(DataStream data)
        {
            if (data == null) return;

            var samples = data.AllSamples();
            
            lock (m_samples)
            {
                foreach (var s in samples)
                {
                    if (m_samples.ContainsKey(s.DataItemID))
                    {
                        m_samples[s.DataItemID] = s;
                    }
                    else
                    {
                        m_samples.Add(s.DataItemID, s);
                    }

                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        // raise the event on a thread so we don't slow down processing
                        NewSample.Fire(this, new GenericEventArgs<ISample>(s));
                    });
                }
            }

            lock (m_events)
            {
                var events = data.AllEvents();
                foreach (var e in events)
                {
                    if (m_events.ContainsKey(e.DataItemID))
                    {
                        m_events[e.DataItemID] = e;
                    }
                    else
                    {
                        m_events.Add(e.DataItemID, e);
                    }

                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        // raise the event on a thread so we don't slow down processing
                        NewEvent.Fire(this, new GenericEventArgs<IEvent>(e));
                    });
                }
            }

            lock (m_conditions)
            {
                var conditions = data.AllConditions();
                foreach (var c in conditions)
                {
                    if (m_conditions.ContainsKey(c.DataItemID))
                    {
                        m_conditions[c.DataItemID] = c;
                    }
                    else
                    {
                        m_conditions.Add(c.DataItemID, c);
                    }

                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        // raise the event on a thread so we don't slow down processing
                        NewCondition.Fire(this, new GenericEventArgs<ICondition>(c));
                    });
                }
            }
        }

    }
}
