using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OpenNETCF.MTConnect.Test
{
    [TestClass()]
    public class HostTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void ProbeAllTest()
        {
            var agent = new Agent();

            var host = new MockHost(agent);

            var before = host.ResponsesSent;

            // pretend we got a probe request
            host.HandleHttpGet("test", new Uri("http://my.server.com/mtc/probe"), null, null);

            Assert.AreEqual(before + 1, host.ResponsesSent);
        }

        [TestMethod()]
        public void HttpGetContextTest()
        {
            var agent = new Agent();

            var host = new MockHost(agent);
            object passedContext = null;

            host.ResponseSent += delegate(string responseData, object context)
            {
                passedContext = context;
            };

            var before = host.ResponsesSent;

            // pretend we got a probe request
            host.HandleHttpGet("test", new Uri("http://my.server.com/mtc/probe"), null, this);

            Assert.AreEqual(this, passedContext);
        }
    }
}
