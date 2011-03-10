using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Linq;

namespace OpenNETCF.MTConnect.Test
{
    [TestClass()]
    public class AgentTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void AgentDefaultConstructorTest()
        {
            var agent = new Agent();
            Assert.IsNotNull(agent);
        }

        [TestMethod()]
        [ExpectedException(typeof(ValidationException))]
        public void AgentBadBufferSizeConstructorTest()
        {
            var agent = new Agent(-1);
            Assert.IsNotNull(agent);
        }

        [TestMethod()]
        [ExpectedException(typeof(ValidationException))]
        public void AgentBadCheckpointFrequencyConstructorTest()
        {
            var agent = new Agent(1000, -1);
            Assert.IsNotNull(agent);
        }

        [TestMethod()]
        public void NoDevicesProbeTest()
        {
            var agent = new Agent();
            var xml = agent.Probe();
            Assert.IsFalse(string.IsNullOrEmpty(xml));

            var doc = XDocument.Parse(xml);

            var devicesNode = doc.Element(doc.Root.Name.Namespace + "MTConnectDevices");
            Assert.IsNotNull(devicesNode);

            var header = devicesNode.Element(doc.Root.Name.Namespace + "Header");
            Assert.IsNotNull(header);

            var time = header.Attribute("creationTime");
            Assert.IsNotNull(time);
            DateTime dt;
            Assert.IsTrue(DateTime.TryParse(time.Value,  out dt));

            var sender = header.Attribute("sender");
            Assert.IsNotNull(sender);

            var version = header.Attribute("version");
            Assert.IsNotNull(version);

            var bufferSize = header.Attribute("bufferSize");
            Assert.IsNotNull(bufferSize);
            long l;
            Assert.IsTrue(long.TryParse(bufferSize.Value,  out l));

            var instanceId = header.Attribute("instanceId");
            Assert.IsNotNull(instanceId);
            Assert.IsTrue(long.TryParse(instanceId.Value,  out l));
        }

        [TestMethod()]
        public void ProbeTest()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();

            agent.Adapters.Add(a);
            var probe = agent.Probe();
        }

        [TestMethod()]
        public void DataItemStorageTest()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();

            agent.Adapters.Add(a);
            var di = a.Device.DataItems["18"];
            var expected = "12";

            var before = agent.Data.Count;
            Assert.IsNull(agent.Data.FirstSequenceNumber);
            Assert.IsNull(agent.Data.LastSequenceNumber);

            di.SetValue(expected);

            var firstSeq = agent.Data.FirstSequenceNumber;
            Assert.IsNotNull(firstSeq);
            var lastSeq = agent.Data.LastSequenceNumber;
            Assert.IsNotNull(lastSeq);
            Assert.AreEqual(lastSeq, firstSeq);

            var after = agent.Data.Count;
            Assert.AreEqual(before + 1, after);
        }

        [TestMethod()]
        public void CurrentDataTest()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();
            agent.Adapters.Add(a);
            var di1 = a.Device.DataItems["18"];
            var di2 = a.Device.DataItems["6"];

            var firstValue = "12";
            di1.SetValue(firstValue);

            long sequence;
            var current = agent.Data.Current(out sequence);
            Assert.AreEqual(1, current.Count());

            var secondValue = "6";
            di2.SetValue(secondValue);

            current = agent.Data.Current(out sequence);
            Assert.AreEqual(2, current.Count());

            // duplicate value - this should get filtered (i.e. the sequence should not change)
            di1.SetValue(firstValue);
            long newSequence;
            current = agent.Data.Current(out newSequence);
            Assert.AreEqual(2, current.Count());
            Assert.AreEqual(sequence, newSequence);
            di1.SetValue(firstValue);

            var thirdValue = "20";
            di1.SetValue(thirdValue);
            current = agent.Data.Current(out newSequence);
            Assert.AreEqual(2, current.Count());
            Assert.AreNotEqual(sequence, newSequence);
        }

        [TestMethod()]
        public void PathFilteredCurrentDataTest()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();
            agent.Adapters.Add(a);
            var di1 = a.Device.DataItems["18"];
            var di2 = a.Device.DataItems["6"];

            var firstValue = "12";
            di1.SetValue(firstValue);
            var secondValue = "6";
            di2.SetValue(secondValue);

            var path = new FilterPath("*", null);
            long seq;
            var result = agent.Data.Current(out seq, path);
            Assert.AreEqual(2, result.Length);

            path = new FilterPath("*", "//axes");
            result = agent.Data.Current(out seq, path);
            Assert.AreEqual(2, result.Length);

            path = new FilterPath("*", "//axes//Z");
            result = agent.Data.Current(out seq, path);
            Assert.AreEqual(1, result.Length);

            path = new FilterPath("*", "//nodevice");
            result = agent.Data.Current(out seq, path);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod()]
        public void CurrentDataXmlTest()
        {
            Agent agent = new Agent();
            Adapter a = new OrgAdapter();
            agent.Adapters.Add(a);
            var di1 = a.Device.DataItems["18"];
            var di2 = a.Device.DataItems["6"];

            var firstValue = "12";
            di1.SetValue(firstValue);
            var secondValue = "6";
            di2.SetValue(secondValue);
            var thirdValue = "20";
            di1.SetValue(thirdValue);

            var xml = agent.Data.CurrentXml();
            Assert.IsFalse(string.IsNullOrEmpty(xml));

            var doc = XDocument.Parse(xml);

            var streamsNode = doc.Element(doc.Root.Name.Namespace + "MTConnectStreams");
            Assert.IsNotNull(streamsNode);

            var header = streamsNode.Element(doc.Root.Name.Namespace + "Header");
            Assert.IsNotNull(header);

            var time = header.Attribute("creationTime");
            Assert.IsNotNull(time);
            DateTime dt;
            Assert.IsTrue(DateTime.TryParse(time.Value, out dt));

            var sender = header.Attribute("sender");
            Assert.IsNotNull(sender);

            var version = header.Attribute("version");
            Assert.IsNotNull(version);

            var bufferSize = header.Attribute("bufferSize");
            Assert.IsNotNull(bufferSize);
            long l;
            Assert.IsTrue(long.TryParse(bufferSize.Value, out l));

            var instanceId = header.Attribute("instanceId");
            Assert.IsNotNull(instanceId);
            Assert.IsTrue(long.TryParse(instanceId.Value, out l));

            var nextSequence = header.Attribute("nextSequence");
            Assert.IsNotNull(nextSequence);
            Assert.IsTrue(long.TryParse(nextSequence.Value, out l));

            var firstSequence = header.Attribute("firstSequence");
            Assert.IsNotNull(firstSequence);
            Assert.IsTrue(long.TryParse(firstSequence.Value, out l));

            var lastSequence = header.Attribute("lastSequence");
            Assert.IsNotNull(lastSequence);
            Assert.IsTrue(long.TryParse(lastSequence.Value, out l));
        }

        [TestMethod()]
        public void GenerateErrorDocumentTest()
        {
            var agent = new Agent();

            var error = new MTConnectError(ErrorCode.InvalidUri, "Bad URI");

            var err = agent.GenerateErrorDocument(error);
        }
        
    }
}
