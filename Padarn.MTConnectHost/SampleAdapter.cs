using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNETCF.Net.MTConnect;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace Padarn.MTConnectHost
{
    public class SampleAdapter : XmlDefinedAdapterBase
    {
        public SampleAdapter(XDocument config)
            : base(config)
        {
        }

        public override void Start()
        {
            new Thread(GetDataProc) { IsBackground = true }
            .Start();
        }

        private void GetDataProc()
        {
            int i = 1;

            // alter data every 5 seconds
            while (true)
            {
                this.Device.DataItems["14"].SetValue((i++).ToString());
                Thread.Sleep(1000);
            }
        }
    }
}
