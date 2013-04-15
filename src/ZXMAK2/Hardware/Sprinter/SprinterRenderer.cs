using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using ZXMAK2.Interfaces;


namespace ZXMAK2.Hardware.Sprinter
{
    public class SprinterRenderer : IUlaRenderer
    {
        public Size VideoSize
        {
            get { return new Size(640, 256); }
        }

        public int FrameLength
        {
            get { return 0x11800 * 6; } //6 - 21MHz
        }

        public int IntLength
        {
            get { return 0x20; }
        }

        public float PixelHeightRatio
        {
            get { return 2F; }
        }

        public void UpdateBorder(int value)
        {
        }

        public void UpdatePalette(int index, uint value)
        {
        }

        public void ReadFreeBus(int frameTact, ref byte value)
        {
        }

        public unsafe void Render(uint* bufPtr, int startTact, int endTact)
        {
        }

        public void Frame()
        {
        }

        public void LoadScreenData(Stream stream)
        {
        }

        public void SaveScreenData(Stream stream)
        {
        }

        public IUlaRenderer Clone()
        {
            var renderer = new SprinterRenderer();
            return renderer;
        }
    }
}
