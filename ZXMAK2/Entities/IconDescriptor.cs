using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace ZXMAK2.Entities
{
	public class IconDescriptor
	{
		private readonly byte[] m_iconData;

        public string Name { get; private set; }
        public Size Size { get; private set; }
		public bool Visible { get; set; }

		public IconDescriptor(string iconName, Stream iconStream)
		{
            if (iconStream == null)
            {
                throw new FileNotFoundException(
                    string.Format(
                        "Icon stream '{0}' not found",
                        iconName));
            }
            Name = iconName;
			m_iconData = new byte[iconStream.Length];
			iconStream.Read(m_iconData, 0, m_iconData.Length);
			using (var stream = GetIconStream())
            using (var bitmap = new Bitmap(stream))
            {
                Size = bitmap.Size;
            }
		}

		public MemoryStream GetIconStream()
		{
			return new MemoryStream(m_iconData);
		}
	}
}
