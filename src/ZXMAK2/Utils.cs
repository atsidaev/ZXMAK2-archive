using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Drawing;
using System.Globalization;
using System.Security.Principal;
using System.Security.AccessControl;
using ZXMAK2.Resources;


namespace ZXMAK2
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

        public static bool IsFolderWritable(string fileName)
        {
            //if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) != 0)
            //    return false;

            // Get the access rules of the specified files (user groups and user names that have access to the file)
            var rules = Directory
                .GetAccessControl(fileName)
                .GetAccessRules(
                    true,
                    true,
                    typeof(System.Security.Principal.SecurityIdentifier));

            // Get the identity of the current user and the groups that the user is in.
            var groups = WindowsIdentity.GetCurrent().Groups;
            string sidCurrentUser = WindowsIdentity.GetCurrent().User.Value;

            // Check if writing to the file is explicitly denied for this user or a group the user is in.
            foreach (FileSystemAccessRule r in rules)
            {
                if ((groups.Contains(r.IdentityReference) ||
                    r.IdentityReference.Value == sidCurrentUser) &&
                    r.AccessControlType == AccessControlType.Deny &&
                    (r.FileSystemRights & FileSystemRights.WriteData) == System.Security.AccessControl.FileSystemRights.WriteData)
                {
                    return false;
                }

            }

            // Check if writing is allowed
            foreach (FileSystemAccessRule r in rules)
            {
                if ((groups.Contains(r.IdentityReference) ||
                    r.IdentityReference.Value == sidCurrentUser) &&
                    r.AccessControlType == AccessControlType.Allow &&
                    (r.FileSystemRights & FileSystemRights.WriteData) == System.Security.AccessControl.FileSystemRights.WriteData)
                {
                    return true;
                }
            }
            return false;
        }

        public static String GetAppDataFolder()
        {
            var appName = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            var appFolder = Path.GetDirectoryName(appName);
            try
            {
                if (!Utils.IsFolderWritable(appFolder))
                {
                    // Folder is not writable?
                    // Then use %Users%/<username>/AppData/Roaming/ZXMAK2/
                    appFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "ZXMAK2");
                    if (!Directory.Exists(appFolder))
                    {
                        Directory.CreateDirectory(appFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return appFolder;
        }
    }
}
