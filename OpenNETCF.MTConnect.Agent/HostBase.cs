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
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;

namespace OpenNETCF.MTConnect
{
    public abstract class HostBase : IHost
    {
        public event EventHandler<GenericEventArgs<IResourceRequest>> RequestReceived;

        public abstract void SendResponse(string responseData, object context, bool flush);
        public abstract bool IsClientConnected(object context);
        public abstract void SetResponseHeader(object context, string headerName, string value);
        public abstract void ClearResponseHeaders(object context);

        public Agent Agent { get; private set; }

        public HostBase(Agent agent)
        {
            Validate.Begin()
                .IsNotNull(agent)
                .Check();

            Agent = agent;
            Agent.Host = this;
        }

        public virtual string Root 
        { 
            get { return "/mtc"; } 
        }

        public virtual void Start()
        {
            Agent.Start();
        }

        public virtual string HostName
        {
            get { return Dns.GetHostName(); }
        }

        public void HandleHttpGet(string source, Uri path, string requestData, object context)
        {
            Validate.Begin()
                .IsNotNull(path)
                .Check();

            var request = RequestParser.ParseRequest(source, path, Root);

            RequestReceived.Fire(this, new GenericEventArgs<IResourceRequest>(request));
            
            if (request is ProbeRequest)
            {
                SetResponseHeader(context, "Content-Type", "text/xml");
                var response = Agent.Probe();
                SendResponse(response, context, true);
            }
            else if (request is CurrentRequest)
            {
                var current = request as CurrentRequest;

                SetResponseHeader(context, "Content-Type", "text/xml");

                string xml = string.Empty;

                if ((!string.IsNullOrEmpty(current.Path)) || (request.ResourceName != "*"))
                {
                    var fp = new FilterPath(request.ResourceName, current.Path);
                    xml = Agent.Data.CurrentXml(fp);
                }
                else
                {
                    xml = Agent.Data.CurrentXml();
                }

                SendResponse(xml, context, true);
            }
            else if (request is SampleRequest)
            {
                var sample = request as SampleRequest;

                // frequency handling
                if (sample.Frequency > 0)
                {
                    var boundary = "==== MTConnect Stream Boundary ====";

                    ClearResponseHeaders(context);
                    SetResponseHeader(context, "Date", DateTime.Now.ToUniversalTime().ToString("s"));
                    SetResponseHeader(context, "Expires", "-1");
                    SetResponseHeader(context, "Connection", "close");
                    SetResponseHeader(context, "Content-Type", "multipart/x-mixed-replace;boundary=" + boundary);

                    // TODO add filtering
                    // TODO: spin this in a separate thread so we can process multiple clients?
                    int resultCount = 0;

                    var from = sample.From;

                    while (true)
                    {
                        if (!IsClientConnected(context))
                        {
                            System.Diagnostics.Debug.WriteLine("Client Disconnected");
                            break;
                        }

                        var xml = Agent.Data.SampleXml(from, sample.Count, out resultCount, t => true);

                        StringBuilder b = new StringBuilder();
                        b.Append("\r\n");
                        b.Append("--" + boundary + "\r\n");
                        b.Append("Content-type: text/xml\r\n");
                        b.Append("Content-length: " + xml.Length + "\r\n");
                        b.Append("\r\n");

                        SendResponse(b.ToString(), context, true);

                        SendResponse(xml, context, true);

                        b.Append("\r\n");
                        b.Append("\r\n");

                        Thread.Sleep(sample.Frequency);
                        from += resultCount;
                    }
                }
                else
                {
                    // TODO add filtering
                    SetResponseHeader(context, "Content-Type", "text/xml");
                    int resultCount = 0;
                    var xml = Agent.Data.SampleXml(sample.From, sample.Count, out resultCount, t => true);
                    SendResponse(xml, context, true);
                }
            }
            else if (request is AssetsRequest)
            {
                SetResponseHeader(context, "Content-Type", "text/xml");
                var response = Agent.Assets();
                SendResponse(response, context, true);
            }
            else
            {
                // TODO return valid MTConnect error string
                throw new NotSupportedException();
            }
        }
    }
}
