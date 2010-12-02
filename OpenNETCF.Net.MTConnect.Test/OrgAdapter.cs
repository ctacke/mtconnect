using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenNETCF.Net.MTConnect.Test
{
    public class OrgAdapter : Adapter
    {
        private XElement m_deviceElement;

        public OrgAdapter()
        {
            var stream = TestManager.GetTestConfig(ConfigXmlNames.AgentXML);
            var doc = XDocument.Load(stream);
            m_deviceElement = ConfigParser.GetDeviceElement(doc);
        }

        public override void Start()
        {
        }

        public override PropertyCollection GetDeviceProperties()
        {
            return ConfigParser.GetDeviceProperties(m_deviceElement);
        }

        public override ComponentDescription GetComponentDescription(ComponentBase component)
        {
            XElement element;

            if (component is Device)
            {
                element = m_deviceElement;
            }
            else
            {
                // get the subcomponent node
                element = m_deviceElement
                    .Element(m_deviceElement.Name.Namespace + "Components")
                    .Element(m_deviceElement.Name.Namespace + component.XmlNodeName);
            }

            return ConfigParser.GetComponentDescription(element);
        }

        public override ComponentDescriptor[] GetComponentDescriptors(ComponentBase parent)
        {
            XElement element;

            if (parent is Device)
            {
                element = m_deviceElement;
            }
            else
            {
                // get the subcomponent node
                element = m_deviceElement
                    .Element(m_deviceElement.Name.Namespace + "Components")
                    .Element(m_deviceElement.Name.Namespace + parent.XmlNodeName);
            }

            return ConfigParser.GetComponentDescriptors(element);
        }

        public override PropertyCollection[] GetDeviceDataItems()
        {
            return ConfigParser.GetComponentDataItems(m_deviceElement);

            // TODO: enforce existence of availability?
        }

        public override PropertyCollection[] GetComponentDataItems(ComponentBase parent)
        {
            var componentNode = (from c in m_deviceElement.Descendants(m_deviceElement.Name.Namespace + "Components").Elements()
                                where (c.Name.LocalName == parent.XmlNodeName) && (c.Attribute("id").Value == parent.ID)
                                select c).FirstOrDefault();

            if (componentNode == null) return null;

            return ConfigParser.GetComponentDataItems(componentNode);
        }
    }
}
