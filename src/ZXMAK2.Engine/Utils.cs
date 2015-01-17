using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Globalization;


namespace ZXMAK2.Engine
{
    public static class Utils
    {
        #region Xml Helpers

        public static void SetXmlAttribute(XmlNode node, string name, Int32 value)
        {
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value.ToString(CultureInfo.InvariantCulture);
            node.Attributes.Append(attr);
        }

        public static void SetXmlAttribute(XmlNode node, string name, UInt32 value)
        {
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value.ToString(CultureInfo.InvariantCulture);
            node.Attributes.Append(attr);
        }

        public static void SetXmlAttribute(XmlNode node, string name, string value)
        {
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value;
            node.Attributes.Append(attr);
        }

        public static void SetXmlAttribute(XmlNode node, string name, bool value)
        {
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value.ToString();
            node.Attributes.Append(attr);
        }

        public static Int32 GetXmlAttributeAsInt32(XmlNode itemNode, string name, int defValue)
        {
            int result = defValue;
            if (itemNode.Attributes[name] != null)
                if (Int32.TryParse(itemNode.Attributes[name].InnerText, out result))
                    return result;
            return defValue;
        }

        public static UInt32 GetXmlAttributeAsUInt32(XmlNode itemNode, string name, uint defValue)
        {
            uint result = defValue;
            if (itemNode.Attributes[name] != null)
                if (UInt32.TryParse(itemNode.Attributes[name].InnerText, out result))
                    return result;
            return defValue;
        }

        public static bool GetXmlAttributeAsBool(XmlNode itemNode, string name, bool defValue)
        {
            bool result = defValue;
            if (itemNode.Attributes[name] != null)
                if (bool.TryParse(itemNode.Attributes[name].InnerText, out result))
                    return result;
            return defValue;
        }

        public static String GetXmlAttributeAsString(XmlNode itemNode, string name, string defValue)
        {
            if (itemNode.Attributes[name] != null)
                return itemNode.Attributes[name].InnerText;
            return defValue;
        }

        #endregion

        public static int ParseSpectrumInt(string strValue)
        {
            strValue = strValue.Trim().ToLower();
            int value;
            if ((strValue.Length > 0) && (strValue[0] == '#'))
            {
                strValue = strValue.Remove(0, 1);
                value = Convert.ToInt32(strValue, 16);
            }
            else if ((strValue.Length > 1) && ((strValue[1] == 'x') && (strValue[0] == '0')))
            {
                strValue = strValue.Remove(0, 2);
                value = Convert.ToInt32(strValue, 16);
            }
            else
            {
                value = Convert.ToInt32(strValue, 10);
            }
            return value;
        }

        public static String GetAppDataFolder()
        {
            var appName = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            var appFolder = Path.GetDirectoryName(appName);
            return appFolder;
        }

        public static string GetAppFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
