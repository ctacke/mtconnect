using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNETCF.MTConnect;
using System.IO;
using System.Xml.Linq;

namespace Padarn.MTConnectHost
{
    public class MTConnectService : MTConnectProvider
    {
        public static MTConnectService Instance { get; private set; }

        public MTConnectService()
            : base(10000, 1000)
        {
            if (Instance != null)
            {
                throw new Exception("Service can only be created once");
            }

            Instance = this;
        }

        protected override Adapter[] GetAdapters()
        {
            var configDoc = XDocument.Load("agent.mtconnect.org.xml");
            var sampleAdapter = new SampleAdapter(configDoc);

            return new Adapter[]
            {
                sampleAdapter
            };
        }

        protected override IHost GetHost(Agent agent)
        {
            return new PadarnHost(Agent);
        }
    }
}
