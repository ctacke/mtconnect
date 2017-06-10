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
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace OpenNETCF.MTConnect
{
    public class EntityClient : Client
    {
        private int m_nextSequence = -1;

        public EntityClient(string clientAddress)
            : base(clientAddress)
        {
        }

        public EntityClient(Uri clientAddress)
            : base(clientAddress)
        {
        }

        public EntityClient(RestConnector connector, string rootFolder)
            : base(connector, rootFolder)
        {
        }

        public void ResetSequence()
        {
            m_nextSequence = -1;
        }

        public long InstanceID { get; private set; }

        public DeviceCollection Probe()
        {
            var xml = GetProbeXml();

#if IPHONE
			DeviceCollection devices = ConfigParser.ParseConfigFile(xml);
            return devices;
#else
            var devices = ConfigParser.ParseConfigFile(xml);
            return devices;
#endif
        }

        public DataStream Current()
        {
            var xml = GetCurrentXml();
            if (xml.IsNullOrEmpty()) return null;
            var stream = DataStream.FromXml(xml);
            InstanceID = stream.InstanceID;
            return stream;
        }

        public DataStream Current(string deviceName)
        {
            var xml = GetCurrentXml(deviceName);
            var stream = DataStream.FromXml(xml);
            InstanceID = stream.InstanceID;
            return stream;
        }

        public IDataElement GetDataItemById(string id)
        {
            try
            {
                string path = string.Format("//DataItem[@id=\"{0}\"]", id);
                var xml = GetPathFilteredCurrentXml(path);
                if (xml == string.Empty) return null;
                var stream = DataStream.FromXml(xml);
                if (stream == null) return null;
                if (stream.DeviceStreams.Length == 0) return null;
                if (stream.DeviceStreams[0].AllDataItems.Length > 0)
                {
                    return stream.DeviceStreams[0].AllDataItems.FirstOrDefault();
                }
                if (stream.DeviceStreams[0].ComponentStreams.Length > 0)
                {
                    return stream.DeviceStreams[0].ComponentStreams[0].AllDataItems.FirstOrDefault();
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EntityClient.GetDataItemId exception: " + ex.Message);
                return null;
            }
        }

        public void SetDataByDataItemID(string dataItemID, string data)
        {
            var request =
                new XElement(XmlNodeName.DataItems,
                    new XElement(XmlNodeName.DataItem,
                        new XAttribute(XmlAttributeName.DataItemID, dataItemID),
                        new XElement(XmlNodeName.Value,
                            data)
                    )
                );

            var path = "/agent/data";
            lock (SyncRoot)
            {
                RestConnector.Post(path, request.ToString(), RequestTimeout);
            }
        }

        public DataStream Sample()
        {
            return Sample(100);
        }

        public DataStream Sample(int maxItems)
        {
            int next;
            var xml = GetNextSampleXml(m_nextSequence, maxItems, out next);
            if (next > 0)
            {
                m_nextSequence = next;
            }
            else if (next == 0)
            {
                m_nextSequence = -1;
            }
            if (xml == string.Empty) return null;
            var stream = DataStream.FromXml(xml);
            InstanceID = stream.InstanceID;
            return stream;
        }
    }
}
