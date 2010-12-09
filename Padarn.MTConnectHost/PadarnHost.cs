using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNETCF.Net.MTConnect;
using OpenNETCF.Web.Server;
using OpenNETCF.Web;
using System.Net;

namespace Padarn.MTConnectHost
{
    public class PadarnHost : HostBase
    {
        private WebServer m_server = new WebServer();

        internal PadarnHost(Agent agent)
            : base(agent)
        {
        }

        public override void Start()
        {
            m_server.Start();

            base.Start();
        }

        public override bool IsClientConnected(object context)
        {
            var response = (HttpResponse)context;
            return response.IsClientConnected;
        }

        public override void SetResponseHeader(object context, string headerName, string value)
        {
            var response = (HttpResponse)context;
            if (string.Compare(headerName, "Content-Type", true) == 0)
            {
                response.ContentType = value;
            }
            else if (string.Compare(headerName, "Content-Length", true) == 0)
            {
                response.ContentLength = long.Parse(value);
            }
            else
            {
                response.AppendHeader(headerName, value);
            }
        }

        public override void SendResponse(string responseData, object context, bool flush)
        {
            var response = (HttpResponse)context;

//            response.ContentType = "text/xml";
            response.Write(responseData);
            if (flush)
            {
                response.Flush();
            }
        }
    }
}
