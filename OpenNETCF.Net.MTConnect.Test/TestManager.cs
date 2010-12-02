using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace OpenNETCF.Net.MTConnect.Test
{
    internal static class ConfigXmlNames
    {
        public const string AgentXML = "OpenNETCF.Net.MTConnect.Test.inputs.agent.mtconnect.org.xml";
    }

    internal static class TestManager
    {
        public static Stream GetTestConfig(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            return assembly.GetManifestResourceStream(name);
        }
    }
}
