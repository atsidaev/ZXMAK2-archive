using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Drawing;
using System.Globalization;


namespace ZXMAK2
{
	public static class Utils
	{
		public static Stream GetIconStream(string fileName)
		{
			string intName = string.Format("ZXMAK2.Icons.{0}", fileName);
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(intName);
		}

		public static Icon GetAppIcon()
		{
			using (Stream stream = GetIconStream("ZXMAK2.ICO"))
				return new Icon(stream);
		}

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

		public static string GetFullPathFromRelativePath(string relFileName, string rootPath)
		{
			// TODO: rewrite with safe version http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
			string current = Directory.GetCurrentDirectory();
			try
			{
				Directory.SetCurrentDirectory(rootPath);
				return Path.GetFullPath(relFileName);
			}
			finally
			{
				Directory.SetCurrentDirectory(current);
			}
		}
	}

	//public class ContentDownloader
	//{
	//    public delegate void OnDownloadHandler(Stream stream, string type);
	//    public static void Download(string url, OnDownloadHandler proc)
	//    {
	//        Uri uri = new Uri(url);
	//        System.Net.WebRequest request = System.Net.HttpWebRequest.Create(url);
	//        System.Net.WebResponse response = request.GetResponse();
	//        try
	//        {
	//            using (Stream stream = response.GetResponseStream())
	//            {
	//                byte[] data = new byte[response.ContentLength];
	//                stream.Read(data, 0, data.Length);
	//                using (MemoryStream ms = new MemoryStream(data))
	//                    proc(ms, response.ContentType);
	//            }
	//        }
	//        finally
	//        {
	//            response.Close();
	//        }
	//    }
	//}

}
