using OpenNETCF.Net.MTConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OpenNETCF.Net.MTConnect.Test
{
    [TestClass()]
    public class AdapterCollectionTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void AddOrgAdapterTest()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();

            var start = agent.Adapters.Count;

            agent.Adapters.Add(a);

            Assert.AreEqual(start + 1, agent.Adapters.Count);

        }

    }
}
