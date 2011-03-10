using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Linq;

namespace OpenNETCF.MTConnect.Test
{
    [TestClass()]
    public class AgentDataTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void SampleTest()
        {

            var data = new AgentData(null);

            data.AddValue(new DataItem(DataItemCategory.Sample, "TypeA", "DataA", "1"), "A", DateTime.Now);
            data.AddValue(new DataItem(DataItemCategory.Sample, "TypeA", "DataA", "1"), "B", DateTime.Now);
            data.AddValue(new DataItem(DataItemCategory.Sample, "TypeA", "DataA", "1"), "C", DateTime.Now);

            var result = data.Sample(1, 10);
            Assert.AreEqual(3, result.Length);
            result = data.Sample(2, 10);
            Assert.AreEqual(2, result.Length);
            result = data.Sample(3, 10);
            Assert.AreEqual(1, result.Length);
            result = data.Sample(4, 10);
            Assert.AreEqual(0, result.Length);
        }

        
    }
}
