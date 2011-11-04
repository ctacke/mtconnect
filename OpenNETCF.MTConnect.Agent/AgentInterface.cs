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
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace OpenNETCF.MTConnect
{
    public class AgentInterface : IAgentInterface
    {
        public event EventHandler<DataItemValue> DataItemValueChanged;

        private string m_versionNumber;
        private Agent m_agent;
        private Dictionary<string, string> m_publishedValueCache = new Dictionary<string, string>();
        private IHostedAdapterService AdapterService { get; set; }

        internal AgentInterface(Agent agent, IHostedAdapterService adapterService)
        {
            m_agent = agent;
            m_versionNumber = Assembly.GetCallingAssembly().GetName().Version.ToString(3);
            AdapterService = adapterService;
        }

        public void ClearCache()
        {
            m_publishedValueCache.Clear();
        }

        private bool IsChangedValue(string dataItemID, string value)
        {
            lock (m_publishedValueCache)
            {
                if (!m_publishedValueCache.ContainsKey(dataItemID))
                {
                    m_publishedValueCache.Add(dataItemID, value);
                    return true;
                }

                var previous = m_publishedValueCache[dataItemID];
                if (previous != value)
                {
                    m_publishedValueCache[dataItemID] = value;
                    return true;
                }
            }
            return false;
        }

        public void PublishData(string dataItemID, object value)
        {
            PublishData(dataItemID, value, true, this);
        }

        public void PublishData(string dataItemID, object value, bool ignoreDuplicates)
        {
            PublishData(dataItemID, value, ignoreDuplicates, this);
        }

        public void PublishData(string dataItemID, object value, bool ignoreDuplicates, object parameter)
        {
            if (value == null) return;
            if ((ignoreDuplicates) && (!IsChangedValue(dataItemID, value.ToString())))
            {
                return;
            }

            m_agent.PublishData(dataItemID, value.ToString(), DateTime.Now, parameter);
        }

        public string Version
        {
            get { return m_versionNumber; }
        }

        public long InstanceID
        {
            get { return m_agent.InstanceID; }
        }

        public int BufferSize
        {
            get { return m_agent.Data.BufferSize; }
        }

        public int BufferCount
        {
            get { return m_agent.Data.Count; }
        }

        public string GetDataItemCurrentValue(string dataItemID)
        {
            var et = Environment.TickCount;
            var value = m_agent.Data.GetCurrentValue(dataItemID);
            et = Environment.TickCount - et;

            if ((Debugger.IsAttached) && (et > 50))
            {
                Debug.WriteLine(string.Format("GetDataItemCurrentValue took {0}ms", et));
                //Debugger.Break();
            }

            return value;
        }

        public string CallMethod(string adapterID, string methodName, string[] parameters)
        {
            if (AdapterService == null) return null;
            return AdapterService.CallAdapterMethod(adapterID, methodName, parameters);
        }

        public DataItem GetDataItemByID(string dataItemID)
        {
            return m_agent.GetDataItemByID(dataItemID);
        }
    }
}
