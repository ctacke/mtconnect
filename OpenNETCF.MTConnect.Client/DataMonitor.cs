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

namespace OpenNETCF.MTConnect
{
    public class DataMonitor : IDisposable
    {
        private EntityClient m_client;
        private int m_period;
        private AutoResetEvent m_stopEvent;
        private Dictionary<string, Sample> m_samples = new Dictionary<string,Sample>();
        private Dictionary<string, Event> m_events = new Dictionary<string,Event>();
        private Dictionary<string, Condition> m_conditions = new Dictionary<string,Condition>();

        public event EventHandler<GenericEventArgs<Sample>> NewSample;
        public event EventHandler<GenericEventArgs<Event>> NewEvent;
        public event EventHandler<GenericEventArgs<Condition>> NewCondition;

        public bool Running { get; private set; }
        public bool Connected { get; private set; }

        public DataMonitor(string agentAddress)
            : this(agentAddress, 1000)
        {
        }

        public DataMonitor(string agentAddress, int period)
        {
            m_client = new EntityClient(agentAddress);
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

            new Thread(MonitorThreadProc) { IsBackground = true }
            .Start();

            Connected = true;
        }

        public void Stop()
        {
            m_stopEvent.Set();
        }

        public Sample GetSample(string id)
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

        public Event GetEvent(string id)
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

        public Condition GetCondition(string id)
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
        
        private void MonitorThreadProc()
        {
            Running = true;

            try
            {
                m_stopEvent.Reset();

                var start = Environment.TickCount;

                var data = m_client.Sample();
                if (data != null)
                {
                    HandleNewData(data);
                }

                var et = Environment.TickCount - start;
                if (et < Period) Thread.Sleep(Period - et);

                while (!(m_stopEvent.WaitOne(0)))
                {
                    start = Environment.TickCount;

                    data = m_client.Sample();
                    if (data != null)
                    {
                        Connected = true;
                        HandleNewData(data);
                    }
                    else
                    {
                        Connected = false;
                    }

                    et = Environment.TickCount - start;
                    if (et < Period) Thread.Sleep(Period - et);
                }
            }
            finally
            {
                Running = false;
            }
        }

        private void HandleNewData(DataStream data)
        {
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
                        NewSample.Fire(this, new GenericEventArgs<Sample>(s));
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
                        NewEvent.Fire(this, new GenericEventArgs<Event>(e));
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
                        NewCondition.Fire(this, new GenericEventArgs<Condition>(c));
                    });
                }
            }
        }

    }
}
