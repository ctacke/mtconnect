using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.MTConnect.Test
{
    public delegate void ResponseHandler(string data, object context);

    public class MockHost : HostBase
    {
        public event ResponseHandler ResponseSent;

        public MockHost(Agent agent)
            : base(agent)
        {
        }

        public override string HostName
        {
            get { return "MockHost"; }
        }

        public override bool IsClientConnected(object context)
        {
            throw new NotImplementedException();
        }

        public override void SendResponse(string responseData, object context, bool flush)
        {
            ResponsesSent++;

            if(ResponseSent != null)
            {
                ResponseSent(responseData, context);
            }
        }

        public int ResponsesSent { get; set; }


        public override void SetResponseHeader(object context, string headerName, string value)
        {
        }

        public override void ClearResponseHeaders(object context)
        {
            throw new NotImplementedException();
        }
    }
}
