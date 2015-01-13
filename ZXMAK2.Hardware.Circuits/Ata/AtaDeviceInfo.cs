using System;
using System.Xml;
using System.IO;


namespace ZXMAK2.Hardware.Circuits.Ata
{
    public class AtaDeviceInfo
    {
        public string image = string.Empty;
        public uint c = 20, h = 16, s = 63, lba = 20160;
        public bool readOnly = true;
        public bool cd = false;

        
        public void Save(string fileName)
        {
            XmlDocument xml = new XmlDocument();
            XmlNode root = xml.AppendChild(xml.CreateElement("IdeDiskDescriptor"));
            XmlNode imageNode = root.AppendChild(xml.CreateElement("Image"));
            string imageFile = image ?? string.Empty;
            if (imageFile != string.Empty &&
                Path.GetDirectoryName(imageFile) == Path.GetDirectoryName(fileName))
            {
                imageFile = Path.GetFileName(imageFile);
            }
            Utils.SetXmlAttribute(imageNode, "fileName", imageFile);
            Utils.SetXmlAttribute(imageNode, "isCdrom", cd);
            Utils.SetXmlAttribute(imageNode, "isReadOnly", readOnly);
            XmlNode geometryNode = root.AppendChild(xml.CreateElement("Geometry"));
            Utils.SetXmlAttribute(geometryNode, "cylinders", c);
            Utils.SetXmlAttribute(geometryNode, "heads", h);
            Utils.SetXmlAttribute(geometryNode, "sectors", s);
            Utils.SetXmlAttribute(geometryNode, "lba", lba);
            xml.Save(fileName);
        }

        public void Load(string fileName)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(fileName);
            XmlNode root = xml["IdeDiskDescriptor"];
            XmlNode imageNode = root["Image"];
            XmlNode geometryNode = root["Geometry"];
            image = Utils.GetXmlAttributeAsString(imageNode, "fileName", image ?? string.Empty);
            if (image != string.Empty && !Path.IsPathRooted(image))
                image = Utils.GetFullPathFromRelativePath(image, Path.GetDirectoryName(fileName));
            cd = Utils.GetXmlAttributeAsBool(imageNode, "isCdrom", false);
            readOnly = Utils.GetXmlAttributeAsBool(imageNode, "isReadOnly", true);
            c = Utils.GetXmlAttributeAsUInt32(geometryNode, "cylinders", c);
            h = Utils.GetXmlAttributeAsUInt32(geometryNode, "heads", h);
            s = Utils.GetXmlAttributeAsUInt32(geometryNode, "sectors", s);
            lba = Utils.GetXmlAttributeAsUInt32(geometryNode, "lba", lba);
        }
    }
}
