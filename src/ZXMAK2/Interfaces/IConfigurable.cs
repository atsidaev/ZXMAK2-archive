using System;
using System.Xml;


namespace ZXMAK2.Interfaces
{
    #region Comment
    /// <summary>
    /// Provide way to store configuration settings
    /// </summary>
    #endregion
    public interface IConfigurable
    {
        void LoadConfig(XmlNode itemNode);
        void SaveConfig(XmlNode itemNode);
    }
}
