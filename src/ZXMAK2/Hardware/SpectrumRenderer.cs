using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Hardware
{
    public class SpectrumRendererParams
    {
        public int c_ulaLineTime;
        public int c_ulaFirstPaperLine;
        public int c_ulaFirstPaperTact;
        public int c_frameTactCount;

        public int c_ulaBorderTop;
        public int c_ulaBorderBottom;
        public int c_ulaBorderLeftT;
        public int c_ulaBorderRightT;

        public int c_ulaIntBegin;
        public int c_ulaIntLength;

        public bool c_ulaBorder4T;
        public int c_ulaBorder4Tstage;
        public int c_ulaFlashPeriod;

        public int c_ulaWidth;
        public int c_ulaHeight;
    }

    public class SpectrumRenderer : IUlaRenderer
    {
        private SpectrumRendererParams m_params;
        private uint[] m_palette;

        protected int[] m_ulaLineOffset;
        protected int[] m_ulaAddrBw;
        protected int[] m_ulaAddrAt;
        protected UlaAction[] m_ulaAction;
        protected readonly uint[] m_ulaInk = new uint[256 * 2];
        protected readonly uint[] m_ulaPaper = new uint[256 * 2];

        protected byte[] m_ulaMemory;              // current video ram bank
        protected int m_flashState = 0;            // flash attr state (0/256)
        protected int m_flashCounter = 0;          // flash attr counter
        protected int m_borderIndex = 0;            // current border value
        protected uint m_borderColor = 0;           // current border color

        protected int m_fetchB1;
        protected int m_fetchA1;
        protected int m_fetchB2;
        protected int m_fetchA2;
        protected uint m_fetchInk;
        protected uint m_fetchPaper;
        protected uint m_fetchBorder;


        #region IUlaRenderer

        public Size VideoSize
        {
            get { return new Size(Params.c_ulaWidth, Params.c_ulaHeight); }
        }

        public int FrameLength
        {
            get { return Params.c_frameTactCount; }
        }

        public int IntLength
        {
            get { return Params.c_ulaIntLength; }
        }

        public float PixelHeightRatio
        {
            get { return 1F; }
        }

        public virtual void UpdateBorder(int value)
        {
            m_borderIndex = value;
            m_borderColor = Palette[m_borderIndex & 7];
        }

        public virtual void UpdatePalette(int index, uint value)
        {
            Palette[index] = value;
            UpdateBorder(m_borderIndex);
            // TODO: remove palette index substitution to remove OnPaletteChanged
            OnPaletteChanged();
        }

        public virtual void ReadFreeBus(int frameTact, ref byte value)
        {
            switch (m_ulaAction[frameTact])
            {
                case UlaAction.BorderAndFetchB1:
                case UlaAction.Shift1AndFetchB2:
                case UlaAction.Shift2AndFetchB1:
                    value = m_ulaMemory[m_ulaAddrBw[frameTact]];
                    break;
                case UlaAction.BorderAndFetchA1:
                case UlaAction.Shift1AndFetchA2:
                case UlaAction.Shift2AndFetchA1:
                    value = m_ulaMemory[m_ulaAddrAt[frameTact]];
                    break;
            }
        }

        public virtual unsafe void Render(
            uint* bufPtr,
            int startTact,
            int endTact)
        {
            if (bufPtr == null || m_ulaAction == null)
                return;
            if (endTact > Params.c_frameTactCount)
                endTact = Params.c_frameTactCount;
            if (startTact > Params.c_frameTactCount)
                startTact = Params.c_frameTactCount;
            // cache params...
            var c_ulaBorder4T = Params.c_ulaBorder4T;
            var c_ulaBorder4Tstage = Params.c_ulaBorder4Tstage;

            for (int takt = startTact; takt < endTact; takt++)
            {
                if (!c_ulaBorder4T || (takt & 3) == c_ulaBorder4Tstage)
                {
                    m_fetchBorder = m_borderColor;
                }
                switch (m_ulaAction[takt])
                {
                    case UlaAction.None:
                        break;
                    case UlaAction.Border:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = m_fetchBorder;
                            bufPtr[offset + 1] = m_fetchBorder;
                        }
                        break;
                    case UlaAction.BorderAndFetchB1:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = m_fetchBorder;
                            bufPtr[offset + 1] = m_fetchBorder;
                            m_fetchB1 = m_ulaMemory[m_ulaAddrBw[takt]];
                        }
                        break;
                    case UlaAction.BorderAndFetchA1:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = m_fetchBorder;
                            bufPtr[offset + 1] = m_fetchBorder;
                            m_fetchA1 = m_ulaMemory[m_ulaAddrAt[takt]];
                            m_fetchInk = m_ulaInk[m_fetchA1 + m_flashState];
                            m_fetchPaper = m_ulaPaper[m_fetchA1 + m_flashState];
                        }
                        break;
                    case UlaAction.Shift1AndFetchB2:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = ((m_fetchB1 & 0x80) != 0) ? m_fetchInk : m_fetchPaper;
                            bufPtr[offset + 1] = ((m_fetchB1 & 0x40) != 0) ? m_fetchInk : m_fetchPaper;
                            m_fetchB1 <<= 2;
                            m_fetchB2 = m_ulaMemory[m_ulaAddrBw[takt]];
                        }
                        break;
                    case UlaAction.Shift1AndFetchA2:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = ((m_fetchB1 & 0x80) != 0) ? m_fetchInk : m_fetchPaper;
                            bufPtr[offset + 1] = ((m_fetchB1 & 0x40) != 0) ? m_fetchInk : m_fetchPaper;
                            m_fetchB1 <<= 2;
                            m_fetchA2 = m_ulaMemory[m_ulaAddrAt[takt]];
                        }
                        break;
                    case UlaAction.Shift1:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = ((m_fetchB1 & 0x80) != 0) ? m_fetchInk : m_fetchPaper;
                            bufPtr[offset + 1] = ((m_fetchB1 & 0x40) != 0) ? m_fetchInk : m_fetchPaper;
                            m_fetchB1 <<= 2;
                        }
                        break;
                    case UlaAction.Shift1Last:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = ((m_fetchB1 & 0x80) != 0) ? m_fetchInk : m_fetchPaper;
                            bufPtr[offset + 1] = ((m_fetchB1 & 0x40) != 0) ? m_fetchInk : m_fetchPaper;
                            m_fetchB1 <<= 2;
                            m_fetchInk = m_ulaInk[m_fetchA2 + m_flashState];
                            m_fetchPaper = m_ulaPaper[m_fetchA2 + m_flashState];
                        }
                        break;
                    case UlaAction.Shift2:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = ((m_fetchB2 & 0x80) != 0) ? m_fetchInk : m_fetchPaper;
                            bufPtr[offset + 1] = ((m_fetchB2 & 0x40) != 0) ? m_fetchInk : m_fetchPaper;
                            m_fetchB2 <<= 2;
                        }
                        break;
                    case UlaAction.Shift2AndFetchB1:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = ((m_fetchB2 & 0x80) != 0) ? m_fetchInk : m_fetchPaper;
                            bufPtr[offset + 1] = ((m_fetchB2 & 0x40) != 0) ? m_fetchInk : m_fetchPaper;
                            m_fetchB2 <<= 2;
                            m_fetchB1 = m_ulaMemory[m_ulaAddrBw[takt]];
                        }
                        break;
                    case UlaAction.Shift2AndFetchA1:
                        {
                            var offset = m_ulaLineOffset[takt];
                            bufPtr[offset] = ((m_fetchB2 & 0x80) != 0) ? m_fetchInk : m_fetchPaper;
                            bufPtr[offset + 1] = ((m_fetchB2 & 0x40) != 0) ? m_fetchInk : m_fetchPaper;
                            m_fetchB2 <<= 2;
                            m_fetchA1 = m_ulaMemory[m_ulaAddrAt[takt]];
                            m_fetchInk = m_ulaInk[m_fetchA1 + m_flashState];
                            m_fetchPaper = m_ulaPaper[m_fetchA1 + m_flashState];
                        }
                        break;
                }
            }
        }

        public virtual void Frame()
        {
            m_flashCounter++;
            if (m_flashCounter >= Params.c_ulaFlashPeriod)
            {
                m_flashState ^= 256;
                m_flashCounter = 0;
            }
        }

        public virtual void LoadScreenData(Stream stream)
        {
            stream.Read(UlaMemory, 0, 6912);
        }

        public virtual void SaveScreenData(Stream stream)
        {
            stream.Write(UlaMemory, 0, 6912);
        }

        public virtual IUlaRenderer Clone()
        {
            var renderer = new SpectrumRenderer();
            renderer.Params = this.Params;
            renderer.Palette = this.Palette;
            renderer.UlaMemory = this.UlaMemory;
            renderer.m_flashState = this.m_flashState;
            renderer.m_flashCounter = this.m_flashCounter;
            renderer.UpdateBorder(this.m_borderIndex);
            renderer.m_fetchB1 = this.m_fetchB1;
            renderer.m_fetchA1 = this.m_fetchA1;
            renderer.m_fetchB2 = this.m_fetchB2;
            renderer.m_fetchA2 = this.m_fetchA2;
            renderer.m_fetchInk = this.m_fetchInk;
            renderer.m_fetchPaper = this.m_fetchPaper;
            renderer.m_fetchBorder = m_fetchBorder;
            return renderer;
        }

        #endregion IUlaRenderer


        #region Public

        public SpectrumRendererParams Params
        {
            get { return m_params; }
            set { ValidateParams(value); m_params = value; OnParamsChanged(); }
        }

        public uint[] Palette
        {
            get { return m_palette; }
            set { m_palette = value; UpdateBorder(m_borderIndex); OnPaletteChanged(); }
        }

        public byte[] UlaMemory
        {
            get { return m_ulaMemory; }
            set { m_ulaMemory = value; }
        }

        #endregion


        public SpectrumRenderer()
        {
            Params = CreateParams();
            Palette = CreatePalette();
        }

        /// <summary>
        /// Create default renderer params (Pentagon 128K)
        /// </summary>
        public static SpectrumRendererParams CreateParams()
        {
            // Pentagon 128K
            // Total Size:          448 x 320
            // Visible Size:        320 x 240 (32+256+32 x 24+192+24)
            var timing = new SpectrumRendererParams();
            timing.c_ulaLineTime = 224;
            timing.c_ulaFirstPaperLine = 80;
            timing.c_ulaFirstPaperTact = 68;      // 68 [32sync+36border+128scr+28border]
            timing.c_frameTactCount = 71680;
            timing.c_ulaBorder4T = false;
            timing.c_ulaBorder4Tstage = 1;

            timing.c_ulaBorderTop = 24;//64;
            timing.c_ulaBorderBottom = 24;//48;
            timing.c_ulaBorderLeftT = 16;//36;
            timing.c_ulaBorderRightT = 16;//28;

            timing.c_ulaIntBegin = 0;
            timing.c_ulaIntLength = 32;
            timing.c_ulaFlashPeriod = 25;

            timing.c_ulaWidth = (timing.c_ulaBorderLeftT + 128 + timing.c_ulaBorderRightT) * 2;
            timing.c_ulaHeight = timing.c_ulaBorderTop + 192 + timing.c_ulaBorderBottom;
            return timing;
        }

        /// <summary>
        /// Create default palette
        /// </summary>
        public static uint[] CreatePalette()
        {
            return new uint[16]
            { 
                0xFF000000, 0xFF0000AA, 0xFFAA0000, 0xFFAA00AA, 
                0xFF00AA00, 0xFF00AAAA, 0xFFAAAA00, 0xFFAAAAAA,
                0xFF000000, 0xFF0000FF, 0xFFFF0000, 0xFFFF00FF, 
                0xFF00FF00, 0xFF00FFFF, 0xFFFFFF00, 0xFFFFFFFF,
            };
        }

        public static void ValidateParams(SpectrumRendererParams timing)
        {
            if (timing.c_ulaWidth != (timing.c_ulaBorderLeftT + 128 + timing.c_ulaBorderRightT) * 2 ||
                timing.c_ulaHeight != (timing.c_ulaBorderTop + 192 + timing.c_ulaBorderBottom))
            {
                throw new ArgumentException("width/height");
            }
            if (timing.c_ulaLineTime < 128)
            {
                throw new ArgumentException("ulaLineTime");
            }
            if (timing.c_frameTactCount < timing.c_ulaLineTime * 192)
            {
                throw new ArgumentException("frameTactCount");
            }
            //...
        }

        protected virtual void OnParamsChanged()
        {
            // rebuild tables...
            int pitchWidth = Params.c_ulaWidth;
            m_ulaLineOffset = new int[Params.c_frameTactCount];
            m_ulaAddrBw = new int[Params.c_frameTactCount];
            m_ulaAddrAt = new int[Params.c_frameTactCount];
            m_ulaAction = new UlaAction[Params.c_frameTactCount];

            int takt = 0;
            for (int line = 0; line < Params.c_frameTactCount / Params.c_ulaLineTime; line++)
                for (int pix = 0; pix < Params.c_ulaLineTime; pix++, takt++)
                {
                    if ((line >= (Params.c_ulaFirstPaperLine - Params.c_ulaBorderTop)) && (line < (Params.c_ulaFirstPaperLine + 192 + Params.c_ulaBorderBottom)) &&
                        (pix >= (Params.c_ulaFirstPaperTact - Params.c_ulaBorderLeftT)) && (pix < (Params.c_ulaFirstPaperTact + 128 + Params.c_ulaBorderRightT)))
                    {
                        // visibleArea (vertical)
                        if ((line >= Params.c_ulaFirstPaperLine) && (line < (Params.c_ulaFirstPaperLine + 192)) &&
                            (pix >= Params.c_ulaFirstPaperTact) && (pix < (Params.c_ulaFirstPaperTact + 128)))
                        {
                            // paper
                            int sx, sy, ap, vp;
                            int scrPix = pix - Params.c_ulaFirstPaperTact;
                            switch (scrPix & 7)
                            {
                                case 0:
                                    m_ulaAction[takt] = UlaAction.Shift1AndFetchB2;   // shift 1 + fetch B2

                                    sx = pix + 4 - Params.c_ulaFirstPaperTact;  // +4 = prefetch!
                                    sy = line - Params.c_ulaFirstPaperLine;
                                    sx >>= 2;
                                    ap = sx | ((sy >> 3) << 5);
                                    vp = sx | (sy << 5);
                                    m_ulaAddrBw[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                    //_ulaAddrAT[takt] = 6144 + ap;
                                    break;
                                case 1:
                                    m_ulaAction[takt] = UlaAction.Shift1AndFetchA2;   // shift 1 + fetch A2

                                    sx = pix + 3 - Params.c_ulaFirstPaperTact;  // +3 = prefetch!
                                    sy = line - Params.c_ulaFirstPaperLine;
                                    sx >>= 2;
                                    ap = sx | ((sy >> 3) << 5);
                                    vp = sx | (sy << 5);
                                    //_ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                    m_ulaAddrAt[takt] = 6144 + ap;
                                    break;
                                case 2:
                                    m_ulaAction[takt] = UlaAction.Shift1;   // shift 1
                                    break;
                                case 3:
                                    m_ulaAction[takt] = UlaAction.Shift1Last;   // shift 1 (last)
                                    break;
                                case 4:
                                    m_ulaAction[takt] = UlaAction.Shift2;   // shift 2
                                    break;
                                case 5:
                                    m_ulaAction[takt] = UlaAction.Shift2;   // shift 2
                                    break;
                                case 6:
                                    if (pix < (Params.c_ulaFirstPaperTact + 128 - 2))
                                    {
                                        m_ulaAction[takt] = UlaAction.Shift2AndFetchB1;   // shift 2 + fetch B2
                                    }
                                    else
                                    {
                                        m_ulaAction[takt] = UlaAction.Shift2;             // shift 2
                                    }

                                    sx = pix + 2 - Params.c_ulaFirstPaperTact;  // +2 = prefetch!
                                    sy = line - Params.c_ulaFirstPaperLine;
                                    sx >>= 2;
                                    ap = sx | ((sy >> 3) << 5);
                                    vp = sx | (sy << 5);
                                    m_ulaAddrBw[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                    //_ulaAddrAT[takt] = 6144 + ap;
                                    break;
                                case 7:
                                    if (pix < (Params.c_ulaFirstPaperTact + 128 - 2))
                                    {
                                        //???
                                        m_ulaAction[takt] = UlaAction.Shift2AndFetchA1;   // shift 2 + fetch A2
                                    }
                                    else
                                    {
                                        m_ulaAction[takt] = UlaAction.Shift2;             // shift 2
                                    }

                                    sx = pix + 1 - Params.c_ulaFirstPaperTact;  // +1 = prefetch!
                                    sy = line - Params.c_ulaFirstPaperLine;
                                    sx >>= 2;
                                    ap = sx | ((sy >> 3) << 5);
                                    vp = sx | (sy << 5);
                                    //_ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                    m_ulaAddrAt[takt] = 6144 + ap;
                                    break;
                            }
                        }
                        else if ((line >= Params.c_ulaFirstPaperLine) && (line < (Params.c_ulaFirstPaperLine + 192)) &&
                                 (pix == (Params.c_ulaFirstPaperTact - 2)))  // border & fetch B1
                        {
                            m_ulaAction[takt] = UlaAction.BorderAndFetchB1; // border & fetch B1

                            int sx = pix + 2 - Params.c_ulaFirstPaperTact;  // +2 = prefetch!
                            int sy = line - Params.c_ulaFirstPaperLine;
                            sx >>= 2;
                            int ap = sx | ((sy >> 3) << 5);
                            int vp = sx | (sy << 5);
                            m_ulaAddrBw[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                            //_ulaAddrAT[takt] = 6144 + ap;
                        }
                        else if ((line >= Params.c_ulaFirstPaperLine) && (line < (Params.c_ulaFirstPaperLine + 192)) &&
                                 (pix == (Params.c_ulaFirstPaperTact - 1)))  // border & fetch A1
                        {
                            m_ulaAction[takt] = UlaAction.BorderAndFetchA1; // border & fetch A1

                            int sx = pix + 1 - Params.c_ulaFirstPaperTact;  // +1 = prefetch!
                            int sy = line - Params.c_ulaFirstPaperLine;
                            sx >>= 2;
                            int ap = sx | ((sy >> 3) << 5);
                            int vp = sx | (sy << 5);
                            //_ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                            m_ulaAddrAt[takt] = 6144 + ap;
                        }
                        else
                        {
                            m_ulaAction[takt] = UlaAction.Border; // border
                        }

                        int wy = line - (Params.c_ulaFirstPaperLine - Params.c_ulaBorderTop);
                        int wx = (pix - (Params.c_ulaFirstPaperTact - Params.c_ulaBorderLeftT)) * 2;
                        m_ulaLineOffset[takt] = wy * pitchWidth + wx;
                        //(videoParams.LinePitch * (videoParams.Height - 240) / 2) + ((videoParams.Width - 320) / 2); // if texture size > 320x240 -> center image 
                    }
                    else
                    {
                        m_ulaAction[takt] = UlaAction.None;
                    }
                }

            ShiftTable(ref m_ulaAction, Params.c_ulaIntBegin);
            ShiftTable(ref m_ulaAddrBw, Params.c_ulaIntBegin);
            ShiftTable(ref m_ulaAddrAt, Params.c_ulaIntBegin);
            ShiftTable(ref m_ulaLineOffset, Params.c_ulaIntBegin);

            //{
            //    XmlDocument xml = new XmlDocument();
            //    XmlNode root = xml.AppendChild(xml.CreateElement("ULA"));
            //    for (int i = 0; i < c_frameTactCount; i++)
            //    {
            //        XmlElement xe = xml.CreateElement("Item");
            //        xe.SetAttribute("tact", i.ToString());
            //        xe.SetAttribute("do", _ulaDo[i].ToString("D2"));
            //        xe.SetAttribute("offset", _ulaLineOffset[i].ToString("D6"));
            //        xe.SetAttribute("y", (_ulaLineOffset[i] / pitchWidth).ToString("D3"));
            //        xe.SetAttribute("x", (_ulaLineOffset[i] % pitchWidth).ToString("D3"));
            //        root.AppendChild(xe);
            //    }
            //    xml.Save("_ulaDo.xml");
            //    //byte[] tmp = new byte[c_frameTactCount];
            //    //for (int i = 0; i < tmp.Length; i++)
            //    //    tmp[i] = (byte)_ulaDo[i];
            //    //using (FileStream fs = new FileStream("_ulaDo.dat", FileMode.Create, FileAccess.Write, FileShare.Read))
            //    //    fs.Write(tmp, 0, tmp.Length);
            //}
        }

        protected void ShiftTable<T>(ref T[] table, int shift)
        {
            var shiftedTable = new T[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                int shiftedIndex = i - shift;
                if (shiftedIndex < 0)
                    shiftedIndex += table.Length;
                shiftedIndex %= table.Length;
                shiftedTable[shiftedIndex] = table[i];
            }
            table = shiftedTable;
        }

        protected virtual void OnPaletteChanged()
        {
            for (int atd = 0; atd < 256; atd++)
            {
                m_ulaInk[atd] = Palette[(atd & 7) + ((atd & 0x40) >> 3)];
                m_ulaPaper[atd] = Palette[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                if ((atd & 0x80) != 0)
                {
                    m_ulaInk[atd + 256] = Palette[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                    m_ulaPaper[atd + 256] = Palette[(atd & 7) + ((atd & 0x40) >> 3)];
                }
                else
                {
                    m_ulaInk[atd + 256] = Palette[(atd & 7) + ((atd & 0x40) >> 3)];
                    m_ulaPaper[atd + 256] = Palette[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                }
            }
        }


        protected enum UlaAction
        {
            None = 0,
            Border,
            BorderAndFetchB1,
            BorderAndFetchA1,
            Shift1AndFetchB2,
            Shift1AndFetchA2,
            Shift1,
            Shift1Last,
            Shift2,
            Shift2AndFetchB1,
            Shift2AndFetchA1,
        }
    }
}
