using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Engine.Serializers.ScreenshotSerializers
{
    public class JpgSerializer : ScreenshotSerializerBase
    {
		public JpgSerializer(IUlaDevice ulaDevice)
            : base (ulaDevice)
		{
		}

        public override string FormatExtension { get { return "JPG"; } }

        protected override void Save(Stream stream, Bitmap bmp)
        {
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters eps = new EncoderParameters(1);
            eps.Param[0] = new EncoderParameter(encoder, 500L);
            bmp.Save(stream, jgpEncoder, eps);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
                if (codec.FormatID == format.Guid)
                    return codec;
            return null;
        }
    }
}
