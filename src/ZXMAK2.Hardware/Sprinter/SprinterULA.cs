using System;

using ZXMAK2.Engine.Cpu;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.Sprinter
{
    public class SprinterULA : UlaDeviceBase
    {
        #region Fields

        private byte m_mode;
        private byte m_rgadr;
        private byte[][] m_vram;

        protected int _flashState = 0;            // flash attr state (0/256)
        protected int _flashCounter = 0;          // flash attr counter

        #endregion Fields


        public SprinterULA()
        {
            Name = "Sprinter Video";
            Description = string.Format("Sprinter Video Adapter{0}Version 0.1a", Environment.NewLine);
            Renderer = new SprinterRenderer();
        }


        protected override SpectrumRendererParams CreateSpectrumRendererParams()
        {
            var timing = SpectrumRenderer.CreateParams();
            timing.c_ulaLineTime = 0xe0;
            timing.c_ulaFirstPaperLine = 80;
            timing.c_ulaFirstPaperTact = 0x44;
            timing.c_frameTactCount = 0x11800 * 6;//6 - 21MHz
            
            timing.c_ulaBorderTop = 0x18;
            timing.c_ulaBorderBottom = 0x18;
            timing.c_ulaBorderLeftT = 0x10;
            timing.c_ulaBorderRightT = 0x10;
            
            timing.c_ulaIntBegin = 0;
            timing.c_ulaIntLength = 0x20;
            
            timing.c_ulaWidth = ((timing.c_ulaBorderLeftT + 0x80) + timing.c_ulaBorderRightT) * 2;
            timing.c_ulaHeight = (timing.c_ulaBorderTop + 0xc0) + timing.c_ulaBorderBottom;
            return timing;
        }

        #region  -- Bus IO Procs --
        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            //bmgr.SubscribeWRIO(0x00FF, 0x0089, new BusWriteIoProc(this.writePort89h));  //write 89h
            bmgr.SubscribeReset(busReset);

        }

        private int TXTPaletteToRGB(int attrib, int pix, int flash)
        {
            //flash = 1 = on
            //pix = 1 = on
            int _Red, _Green, _Blue;
            byte num = (byte)(flash * 2 + pix);
            _Red = m_vram[(attrib & 0xf0) >> 4][((attrib & 0x0f)*1024)+ 0x03f0 + num * 4];
            _Green = m_vram[(attrib & 0xf0) >> 4][((attrib & 0x0f) * 1024) + 0x03f1 + num * 4];
            _Blue = m_vram[(attrib & 0xf0) >> 4][((attrib & 0x0f) * 1024) + 0x03f2 + num * 4];
            //return _Red * 65536 + _Green * 256 + _Blue;
            return (_Red << 16) | (_Green << 8) | _Blue;
            //return System.Drawing.Color.FromArgb(255, _Red, _Green, _Blue).ToArgb();
        }

        /*private int ToColor(int color)
        {
            int _color = System.Drawing.Color.Black.ToArgb();
            switch (color & 7) {
                case 1: _color = System.Drawing.Color.Blue.ToArgb(); break;
                case 2: _color = System.Drawing.Color.Red.ToArgb(); break;
                case 3: _color = System.Drawing.Color.Magenta.ToArgb(); break;
                case 4: _color = System.Drawing.Color.Green.ToArgb(); break;
                case 5: _color = System.Drawing.Color.AliceBlue.ToArgb(); break;
                case 6: _color = System.Drawing.Color.Yellow.ToArgb(); break;
                case 7: _color = System.Drawing.Color.White.ToArgb(); break;

            }

            return _color;
        }
        */
        private int ColorPaletteToRGB(int color, int palette)
        {
            int red, green, blue;

            var index1 = (color & 0xf0) >> 4;
            var index2 = ((color & 0x0f) * 1024) + 0x03e0 + palette * 4;
            red = m_vram[index1][index2];
            green = m_vram[index1][index2+1];
            blue = m_vram[index1][index2+2];
            return (red << 16) | (green << 8) | blue;
            //return _Red * 65536 + _Green * 256 + _Blue;
            //return System.Drawing.Color.FromArgb(255, _Red, _Green, _Blue).ToArgb();
        }

        protected override void EndFrame()
        {
            var videoBuffer = base.VideoData.Buffer;
            int num = 0;
            byte linenum, linenum1;
            byte mode0, mode1, mode2, mode10, mode11, mode12, mode, scrbyte, attrbyte;//, scrbyte1, attrbyte1;
            for (int y = 0; y < 32; y++)
            {
                for (int y1 = 0; y1 < 8; y1++)
                {
                    for (int x = 0; x < 40; x++)
                    {
                        linenum = (byte)(1 + 2 * x + ((m_mode & 1) == 1 ? 128 : 0));
                        byte vpage = (byte)((linenum & 0xF0) >> 4);
                        mode0 = m_vram[vpage][((linenum & 0x0f) * 1024) + 0x0300 + 4 * y];
                        mode1 = m_vram[vpage][((linenum & 0x0f) * 1024) + 0x0301 + 4 * y];
                        mode2 = m_vram[vpage][((linenum & 0x0f) * 1024) + 0x0302 + 4 * y];

                        mode = (byte)((mode0 & 0x30) >> 4);
                        switch (mode)
                        {
                            //Графический 640*256
                            case 0:
                                {
                                    int bloknum = ((mode0 & 15) << 1) | ((mode1 & 4) >> 2);
                                    int palette = (mode0 & 192) >> 6;
                                    int ln = (mode1 & 0xf0) >> 4;
                                    int col = (((mode1 & 0x08) | y1) * 1024) + (bloknum * 32) + ((mode1 & 3) << 3);
                                    for (int i = 0; i < 8; i++)
                                    {
                                        videoBuffer[num++] = ColorPaletteToRGB(((m_vram[ln][col + i] & 0xf0)>>4), palette);
                                        videoBuffer[num++] = ColorPaletteToRGB((m_vram[ln][col + i] & 0x0f), palette);
                                        //                                        scrbyte = m_vram[ln][col];
                                    }

                                }
                                break;
                            //спектрумовский, 80симв
                            case 1:
                                {
                                    linenum1 = (byte)(linenum + 1);
                                    vpage = (byte)((linenum1 & 0xF0) >> 4);
                                    mode10 = m_vram[vpage][((linenum1 & 0x0f) * 1024) + 0x0300 + 4 * y];
                                    mode11 = m_vram[vpage][((linenum1 & 0x0f) * 1024) + 0x0301 + 4 * y];
                                    mode12 = m_vram[vpage][((linenum1 & 0x0f) * 1024) + 0x0302 + 4 * y];

                                    int bloknum = ((mode0 & 15) << 1) | ((this.Memory.CMR0 & 8) == 0 ? 0 : 1);
                                    //ushort addr = (ushort)(((mode0 & 192) << 5) | (mode1) | (y1 << 8));
                                    scrbyte = m_vram[((mode1 & 0xf0) >> 4)][((mode1 & 0x0f) * 1024) + (bloknum * 32) + ((mode0 & 0xC0) >> 3) + y1];
                                    attrbyte = m_vram[((mode2 & 0xf0) >> 4)][((mode2 & 0x0f) * 1024) + (bloknum * 32) + ((mode0 & 0xC0) >> 5) + 24];
                                    for (int i = 0, msk = 128; i < 8; i++)
                                    {
                                        /*if ((scrbyte & msk) == 0)
                                        {
                                            videoBuffer[num++] = ToColor((attrbyte & 0x38) >> 3);
                                        }
                                        else
                                        {
                                            videoBuffer[num++] = ToColor(attrbyte & 0x07);
                                        }*/
                                        videoBuffer[num++] = TXTPaletteToRGB(attrbyte, ((scrbyte & msk) == 0) ? 0 : 1, (this._flashState == 0) ? 0 : 1);
                                        msk = msk >> 1;
                                    }
                                    bloknum = ((mode10 & 15) << 1) | ((this.Memory.CMR0 & 8) == 0 ? 0 : 1);
                                    scrbyte = m_vram[((mode11 & 0xf0) >> 4)][((mode11 & 0x0f) * 1024) + (bloknum * 32) + ((mode10 & 0xC0) >> 3) + y1];
                                    attrbyte = m_vram[((mode12 & 0xf0) >> 4)][((mode12 & 0x0f) * 1024) + (bloknum * 32) + ((mode10 & 0xC0) >> 5) + 24];
                                    for (int i = 0, msk = 128; i < 8; i++)
                                    {
/*                                        if ((scrbyte & msk) == 0)
                                        {
                                            videoBuffer[num++] = ToColor((attrbyte & 0x38) >> 3);
                                        }
                                        else
                                        {
                                            videoBuffer[num++] = ToColor(attrbyte & 0x07);
                                        }*/

                                        videoBuffer[num++] = TXTPaletteToRGB(attrbyte, ((scrbyte & msk) == 0) ? 0 : 1, (this._flashState==0)?0:1);
                                        msk = msk >> 1;
                                    }
                                }
                                break;
                            //Графический 320*256
                            case 2:
                                {
                                    int bloknum = ((mode0 & 15) << 1) | ((mode1 & 4) >> 2);
                                    int palette = (mode0 & 192) >> 6;
                                    int ln = (mode1&0xf0)>>4;
                                    int col = (((mode1 & 0x08) | y1) * 1024) + (bloknum * 32) + ((mode1 & 3) << 3);
                                    for (int i = 0; i < 8; i++)
                                    {
                                        videoBuffer[num++] = ColorPaletteToRGB(m_vram[ln][col + i], palette);
                                        videoBuffer[num++] = ColorPaletteToRGB(m_vram[ln][col + i], palette);
//                                        scrbyte = m_vram[ln][col];
                                    }
                                }
                                break;
                            //спектрумовский, 40симв
                            case 3:
                                {
                                    int bloknum = ((mode0 & 15) << 1) | ((this.Memory.CMR0 & 8)==0?0:1);
                                    //ushort addr = (ushort)(((mode0 & 192) << 5) | (mode1) | (y1 << 8));
                                    scrbyte = m_vram[((mode1 & 0xf0) >> 4)][((mode1 & 0x0f) * 1024) + (bloknum * 32) + ((mode0 & 0xC0) >> 3) + y1];
                                    attrbyte = m_vram[((mode2 & 0xf0) >> 4)][((mode2 & 0x0f) * 1024) + (bloknum * 32) + ((mode0 & 0xC0) >> 5) + 24];
                                    for (int i = 0, msk=128; i < 8; i++)
                                    {
                                        //надо проверять, возможно не правильно формируется цвет, надо 16 цветов или целый байт атрибута учавствует в выборке из палитры?
                                        videoBuffer[num++] = TXTPaletteToRGB(attrbyte, ((scrbyte & msk) == 0) ? 0 : 1, (this._flashState == 0) ? 0 : 1);
/*                                        if ((scrbyte & msk) == 0)
                                        {
                                            videoBuffer[num++] = ToColor( (attrbyte & 0x38) >> 3);
                                            videoBuffer[num++] = ToColor((attrbyte & 0x38) >> 3);
                                        }
                                        else
                                        {
                                            videoBuffer[num++] = ToColor(attrbyte & 0x07);
                                            videoBuffer[num++] = ToColor(attrbyte & 0x07);
                                        }*/
                                        msk = msk >> 1;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            base.EndFrame();
            // flash emulation, because moved to renderer
            _flashCounter++;
            if (_flashCounter >= 25)
            {
                _flashState ^= 256;
                _flashCounter = 0;
            }
        }

        private void busReset()
        {
            m_rgadr = 0;
            m_mode = 0;
        }

        #endregion

        public virtual byte RGADR
        {
            get
            {
                return this.m_rgadr;
            }
            set
            {
                this.m_rgadr = value;
            }
        }

        public virtual byte RGMOD
        {
            get
            {
                return this.m_mode;
            }
            set
            {
                this.m_mode = value;
            }
        }

        public virtual byte[][]VRAM 
        {
            get
            {
                return this.m_vram;
            }
            set
            {
                this.m_vram = value;
            }
        }

        public void UpdateFrame()
        {
            this.EndFrame();
        }
    }
}
