using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Linq;

namespace OpenNETCF.MTConnect.Test
{
    [TestClass()]
    public class AdapterTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void GetDataItemByID()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();

            agent.Adapters.Add(a);

            // get the ZActual data item by ID
            string expected = "18";
            var di = a.Device.DataItems[expected];
            Assert.IsNotNull(di);
            Assert.AreEqual(expected, di.ID);
        }

        [TestMethod()]
        public void FindDataItems()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();

            agent.Adapters.Add(a);

            var events = a.Device.DataItems.Find(d => d.Category == DataItemCategory.Event);
            Assert.IsNotNull(events);
            Assert.AreEqual(7, events.Count());

            var axes = a.Device.Components[0];
            events = axes.DataItems.Find(d => d.Category == DataItemCategory.Event);
            Assert.IsNull(events);
        }

        [TestMethod()]
        public void DataItemEvent()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();

            agent.Adapters.Add(a);

            var are = new AutoResetEvent(false);

            DataItemValue eventItem = null;

            a.DataItemValueSet += new EventHandler<DataItemValue>(
                delegate(object sender, DataItemValue value)
                {
                    eventItem = value;
                    are.Set();
                }
                );

            // get the ZActual data item by ID
            var di = a.Device.DataItems["18"];
            var expected = "12";

            di.SetValue(expected);
            Assert.IsTrue(are.WaitOne(5000));
            Assert.IsNotNull(eventItem);
            Assert.AreEqual(di, eventItem.Item);
            Assert.AreEqual(expected, eventItem.Value);

        }
    }
}
