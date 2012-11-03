using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ZXMAK2.Engine.Interfaces;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ZXMAK2.Engine.Serializers.ScreenshotSerializers
{
    public class BmpSerializer : ScreenshotSerializerBase
    {
        public BmpSerializer(IUlaDevice ulaDevice)
            : base(ulaDevice)
        {
        }

        public override string FormatExtension { get { return "BMP"; } }

        protected override void Save(Stream stream, Bitmap bmp)
        {
            bmp.Save(stream, ImageFormat.Bmp);
        }
    }
}
