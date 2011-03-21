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
using System.Diagnostics;

namespace OpenNETCF.MTConnect
{
    public class AgentInterface
    {
        private Agent m_agent;

        public event EventHandler<DataItemValue> DataItemValueChanged;

        private Dictionary<string, string> m_publishedValueCache = new Dictionary<string, string>();

        internal AgentInterface(Agent agent)
        {
            m_agent = agent;
            m_agent.Adapters.DataItemValueSet += new EventHandler<DataItemValue>(DataItemValueSet);
        }

        public void ClearCache()
        {
            m_publishedValueCache.Clear();
        }

        private void DataItemValueSet(object sender, DataItemValue e)
        {
            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    DataItemValueChanged.Fire(sender, e);
                }
            );
        }

        private bool IsChangedValue(string dataItemID, string value)
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

            return false;
        }

        public void PublishData(string dataItemID, string value)
        {
            PublishData(dataItemID, value, true);
        }

        public void PublishData(string dataItemID, string value, bool ignoreDuplicates)
        {
            if ((ignoreDuplicates) && (!IsChangedValue(dataItemID, value)))
            {
                return;
            }

            m_agent.PublishData(dataItemID, value, DateTime.Now);
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
            return m_agent.Data.GetCurrentValue(dataItemID);
        }
    }
}
