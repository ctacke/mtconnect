using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace OpenNETCF.MTConnect.Test
{
    public class XmlAdapter : XmlDefinedAdapter
    {
        public XmlAdapter(Stream configuration)
            : base(configuration)
        {
        }

        public XmlAdapter(XDocument configuration)
            : base(configuration)
        {
        }

        public override void Start()
        {
        }
    }
}
