﻿// -------------------------------------------------------------------------------------------------------
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
using OpenNETCF.Web;
using System.IO;
using System.Xml.Linq;

namespace OpenNETCF.MTConnect
{
    public abstract class Client
    {
        protected string RootFolder { get; private set; }
        protected RestConnector RestConnector { get; private set; }
        public object SyncRoot { get; private set; }
 
        public Client(string agentAddress)
        {
            // TODO: support authentication
            
            agentAddress = agentAddress.Replace('\\', '/');
            if(!agentAddress.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                agentAddress = "http://" + agentAddress;
            }

            var uri = new Uri(agentAddress);
            var targetAddress = uri.Host;

            SyncRoot = new object();
            RestConnector = new RestConnector(targetAddress);
            RootFolder = uri.PathAndQuery;
        }

        protected string GetProbePath()
        {
            if (string.IsNullOrEmpty(RootFolder))
            {
                return "probe";
            }

            return Path.Combine(RootFolder, "probe");
        }

        protected string GetCurrentPath()
        {
            if (string.IsNullOrEmpty(RootFolder))
            {
                return "current";
            }

            return Path.Combine(RootFolder, "current");
        }

        protected string GetSamplePath(int from)
        {
            return GetSamplePath(from, 100);
        }

        protected string GetSamplePath(int from, int count)
        {
            string path;

            if (string.IsNullOrEmpty(RootFolder))
            {
                path = "sample";
            }
            else
            {
                path = Path.Combine(RootFolder, "sample");
            }

            path = string.Format("{0}?from={1}&count={2}",
                path,
                from,
                count);

            return path;
        }

        protected internal string GetProbeXml()
        {
            lock (SyncRoot)
            {
                var path = GetProbePath();
                var xml = RestConnector.Get(path);
                return xml;
            }
        }

        protected internal string GetCurrentXml()
        {
            lock (SyncRoot)
            {
                var path = GetCurrentPath();
                var xml = RestConnector.Get(path);
                return xml;
            }
        }

        protected internal string GetNextSampleXml(int from, int maxItems, out int next)
        {
            lock (SyncRoot)
            {
                next = 0;
                
                if (from == -1)
                {
                    var result = GetCurrentXml();
                    if (result.Length == 0) return result;
                    next = GetNextSequenceID(result);
                    return result;
                }

                var path = GetSamplePath(from, maxItems);
                var xml = RestConnector.Get(path);
                if (xml != string.Empty)
                {
                    next = GetNextSequenceID(xml);
                }

                return xml;
            }
        }

        protected int GetNextSequenceID(string xml)
        {
            var doc = XDocument.Parse(xml);

            var ns = doc.Root.GetDefaultNamespace();
            var header = doc.Element(ns + "MTConnectStreams").Element(ns + "Header");
            var next = int.Parse(header.Attribute("nextSequence").Value);

            return next;
        }
    }
}