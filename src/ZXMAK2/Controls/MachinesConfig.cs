using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;


namespace ZXMAK2.Controls
{
    public class MachinesConfig
    {
        private XmlDocument m_config = new XmlDocument();

        public void Load(string fileName)
        {
            m_config.Load(fileName);
        }

        public IEnumerable<string> GetNames()
        {
            var list = new List<string>();
            foreach (XmlNode node in m_config.DocumentElement.ChildNodes)
            {
                if (string.Compare(node.Name, "Bus", true) != 0 ||
                    node.Attributes["name"] == null)
                {
                    continue;
                }
                list.Add(node.Attributes["name"].InnerText);
            }
            return list;
        }

        public XmlNode GetConfig(string name)
        {
            foreach (XmlNode node in m_config.DocumentElement.ChildNodes)
            {
                if (string.Compare(node.Name, "Bus", true) != 0 ||
                    node.Attributes["name"] == null ||
                    node.Attributes["name"].InnerText != name)
                {
                    continue;
                }
                return node;
            }
            return null;
        }
    }
}
