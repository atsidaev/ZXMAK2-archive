using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace ZXMAK2.Engine
{
	public class IconDescriptor
	{
		private string m_iconName;
		private Size m_iconSize;
		private byte[] m_iconData;

		public string Name { get { return m_iconName; } }
		public Size Size { get { return m_iconSize; } }
		public bool Visible { get; set; }

		public IconDescriptor(string iconName, Stream iconStream)
		{
			m_iconName = iconName;
			if (iconStream==null)
				throw new FileNotFoundException(string.Format("Icon stream '{0}' not found", iconName));
			m_iconData = new byte[iconStream.Length];
			iconStream.Read(m_iconData, 0, m_iconData.Length);
			using (MemoryStream ms = GetIconStream())
			using (Bitmap bmp=new Bitmap(ms))
				m_iconSize = bmp.Size;
		}

		public MemoryStream GetIconStream()
		{
			return new MemoryStream(m_iconData);
		}
	}

	public delegate bool GetIconStateHandler();
}
