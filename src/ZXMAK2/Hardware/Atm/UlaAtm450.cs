using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ZXMAK2.Engine.Devices.Ula
{
    public class UlaAtm450 : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "ATM450"; } }

        public override void BusInit(Interfaces.IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRESET(busReset);
        }

        #endregion

        #region UlaDeviceBase

        public override byte PortFE
        {
            set 
            { 
                base.PortFE = value;
                m_borderAttr = (value & 7) | (m_extBorderIndex & 8);
                _borderColor = Palette[m_borderAttr];
            }
        }

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            m_extBorderIndex = (addr & 8) ^ 8;
            base.WritePortFE(addr, value, ref iorqge);
        }

        public override float VideoHeightScale
        {
            get 
            { 
                switch(m_mode)
                {
                    case 0: // 320x200
                        return 1F;
                    case 2: // 640x200
                    case 6: // 640x200 TEXT
                        return 2F;
                    case 1: // ???
                    case 3: // 256x192 (standard)
                    default: 
                        return base.VideoHeightScale;
                }
            }
        }

        #endregion

        #region Bus Handlers

        private void busReset()
        {
            for (int i = 0; i < 16; i++)
            {
                m_atm_pal[i] = m_pal_startup[i];
                Palette[i] = m_atm_pal_map[m_atm_pal[i]];
            }
            OnPaletteChanged();
        }

        #endregion

        private int m_extBorderIndex = 0;
        private int m_mode = 3;

        private int[] m_ulaInk640;
        private int[] m_ulaPaper640;
        private int[] m_ulaAddr640;
        private int[] m_ulaAddr320;
        private int[] m_ulaAddrTXT640BW;
        private int[] m_ulaAddrTXT640AT;
        private int[] m_ulaAddrTXT640CG;
        private byte[] m_ulaSGEN;

        private int m_borderAttr = 0;
        private byte[] m_atm_pal;
        private uint[] m_atm_pal_map;

        private byte[] m_pal_startup = new byte[]
        {
            0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA, 0xF9, 0xF8, 
            0xFF, 0xF6, 0xED, 0xE4, 0xDB, 0xD2, 0xC9, 0xC0,
        };

        
        public UlaAtm450()
        {
            // ATM1 v4.50
            // Total Size:          448 x 312
            // Visible Size:        384 x 296 (72+256+56 x 64+192+40)
            // First Line Border:   16
            // First Line Paper:    80
            // Paper Lines:         192
            // Bottom Border Lines: 40

            c_ulaLineTime = 224;
            c_ulaFirstPaperLine = 80;      // proof???80
            c_ulaFirstPaperTact = 68;      // proof???68 [32sync+36border+128scr+28border]
            c_frameTactCount = 69888;

            c_ulaBorderTop = 24;//64;
            c_ulaBorderBottom = 24;// 40;
            c_ulaBorderLeftT = 16;
            c_ulaBorderRightT = 16;

            c_ulaIntBegin = 0;
            c_ulaIntLength = 32;

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
        }

        public override void SetPageMapping(int videoPage, int page0000, int page4000, int page8000, int pageC000)
        {
            m_mode = 3;
            base.SetPageMapping(videoPage, page0000, page4000, page8000, pageC000);
        }
        
        public void SetPageMappingAtm(
            int mode,
            int videoPage, 
            int page0000, 
            int page4000, 
            int page8000, 
            int pageC000)
        {
            m_mode = mode;
            base.SetPageMapping(videoPage, page0000, page4000, page8000, pageC000);
        }

        public void SetPaletteAtm(byte value)
        {
            m_atm_pal[m_borderAttr] = value;
            Palette[m_borderAttr] = m_atm_pal_map[value];
            _borderColor = Palette[m_borderAttr];
            OnPaletteChanged();
        }

        protected override unsafe void fetchVideo(
            uint* bitmapBufPtr, 
            int startTact, 
            int endTact, 
            UlaStateBase ulaState)
        {
            switch (m_mode)
            {
                case 0: // 320x200
                case 2: // 640x200
                case 6: // 640x200 TEXT
                    return;
                case 1: // ???
                case 3: // 256x192 (standard)
                    break;
            }
            base.fetchVideo(bitmapBufPtr, startTact, endTact, ulaState);
        }

        public override System.Drawing.Size VideoSize
        {
            get
            {
                switch (m_mode)
                {
                    case 0: // 320x200
                        return new System.Drawing.Size(320, 240);
                    case 2: // 640x200
                    case 6: // 640x200 TEXT
                        return new System.Drawing.Size(640, 240);
                    case 1: // ???
                    case 3: // 256x192 (standard)
                        break;
                }
                return base.VideoSize;
            }
        }

        protected override void EndFrame()
        {
            switch (m_mode)
            {
                case 0: // 320x200
                    drawFrame320();
                    break;
                case 2: // 640x200
                    drawFrame640();
                    break;
                case 6: // 640x200 TEXT
                    drawFrame640TXT();
                    break;
                case 1: // ???
                case 3: // 256x192 (standard)
                    break;
            }
            base.EndFrame();
        }

        private void drawFrame640TXT()
        {
            int[] buffer = VideoBuffer;
            byte[] bwPage = Memory.RamPages[m_videoPage];
            byte[] atPage = Memory.RamPages[m_videoPage == 5 ? 1 : 3];

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 640; j++)
                    buffer[i * 640 + j] = (int)_borderColor;

            for (int i = 0; i < 200 * 80; i++)
            {
                int addrBw = m_ulaAddrTXT640BW[i];
                int addrAt = m_ulaAddrTXT640AT[i];
                int addrCg = m_ulaAddrTXT640CG[i];
                int bw = m_ulaSGEN[(bwPage[addrBw]<<3)+addrCg];
                int at = atPage[addrAt];

                int ink = (int)Palette[m_ulaInk640[at]];
                int paper = (int)Palette[m_ulaPaper640[at]];

                int mask = 0x80;
                for (int b = 0; b < 8; b++)
                {
                    buffer[(20 * 640) + i * 8 + b] = (bw & mask) != 0 ? ink : paper;
                    mask >>= 1;
                }
            }

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 640; j++)
                    buffer[(220 + i) * 640 + j] = (int)_borderColor;
        }

        private void drawFrame640()
        {
            int[] buffer = VideoBuffer;
            byte[] bwPage = Memory.RamPages[m_videoPage];
            byte[] atPage = Memory.RamPages[m_videoPage==5? 1:3];

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 640; j++)
                    buffer[i*640+j] = (int)_borderColor;

            //for (int offset = 0; offset < 200 * 80; offset++)
            for (int i = 0; i < 200 * 80; i++)
            {
                int addr = m_ulaAddr640[i];
                int bw = bwPage[addr];
                int at = atPage[addr];
                int ink = (int)Palette[m_ulaInk640[at]];
                int paper = (int)Palette[m_ulaPaper640[at]];

                int mask = 0x80;
                for (int b = 0; b < 8; b++)
                {
                    buffer[(20*640)+ i * 8 + b] = (bw & mask) != 0 ? ink : paper;
                    mask >>= 1;
                }
            }

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 640; j++)
                    buffer[(220+i) * 640 + j] = (int)_borderColor;
        }

        private void drawFrame320()
        {
            int[] buffer = VideoBuffer;
            byte[] page1 = Memory.RamPages[m_videoPage];
            byte[] page0 = Memory.RamPages[m_videoPage == 5 ? 1 : 3];

            int offset = 0;
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 320; j++)
                    buffer[offset++] = (int)_borderColor;
            
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 160; x++)
                {
                    int addr = m_ulaAddr320[y*160+x];
                    int bw = (x & 1) == 0 ? page0[addr] : page1[addr];

                    int ink0 = (int) Palette[m_ulaInk640[bw]];
                    int ink1 = (int) Palette[m_ulaPaper640[bw]];
                    
                    buffer[offset++] = ink0;
                    buffer[offset++] = ink1;
                }

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 320; j++)
                    buffer[offset++] = (int)_borderColor;
        }

        protected override void OnTimingChanged()
        {
            base.OnTimingChanged();
            m_ulaAddr320 = new int[200 * 160];
            int offset = 0;
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 160; x++)
                    switch(x&3)
                    {
                        case 0: m_ulaAddr320[y * 160 + x] = 0x0000 + y * 160/4 + x / 4; break;
                        case 1: m_ulaAddr320[y * 160 + x] = 0x0000 + y * 160/4 + x / 4; break;
                        case 2: m_ulaAddr320[y * 160 + x] = 0x2000 + y * 160/4 + x / 4; break;
                        case 3: m_ulaAddr320[y * 160 + x] = 0x2000 + y * 160/4 + x / 4; break;
                    }

            m_ulaAddr640 = new int[200 * 80];
            offset = 0;
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 80; x++)
                    m_ulaAddr640[offset++] = (y*80+x) / 2 + 0x2000 * ((y*80+x) & 1);
            
            m_ulaAddrTXT640BW = new int[200 * 80];
            m_ulaAddrTXT640AT = new int[200 * 80];
            m_ulaAddrTXT640CG = new int[200 * 80];
            offset = 0;
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 80; x++, offset++)
                {
                    int pageOffsetBw = (x&1)!=0 ? 0x21C0 : 0x01C0;
                    int pageOffsetAt = (x&1)==0 ? 0x21C0 : 0x01C0;
                    pageOffsetBw += x >> 1;
                    pageOffsetAt += x >> 1;
                    pageOffsetBw += (y>>3) * 64;
                    pageOffsetAt += (y>>3) * 64;
                    m_ulaAddrTXT640BW[offset] = pageOffsetBw;
                    m_ulaAddrTXT640AT[offset] = pageOffsetAt;
                    m_ulaAddrTXT640CG[offset] = y & 7;
                }
            m_ulaSGEN = new byte[256 * 8];
            try
            {
                using (Stream stream = ZXMAK2.Engine.Devices.Memory.MemoryBase.GetRomFileStream("atm_sgen.rom"))
                    stream.Read(m_ulaSGEN, 0, m_ulaSGEN.Length);
            }
            catch(Exception ex)
            {
                LogAgent.Error(ex);
            }
            
            m_ulaInk640 = new int[0x100];
            m_ulaPaper640 = new int[0x100];
            for (int at = 0; at < 256; at++)
            {
                m_ulaInk640[at] = (at & 7) | ((at & 0x40) >> 3);
                m_ulaPaper640[at] = ((at >> 3) & 7) | ((at & 0x80) >> 4);
            }

            m_atm_pal = new byte[16];
            m_atm_pal_map = new uint[0x100];
            
            // atm palette mapping (port out to palette index)
            for (uint i = 0; i < 0x100; i++)
            {
                uint v = i ^ 0xFF; 
                uint dst;
                //if (true)// conf.mem_model == MM_ATM450)
                    dst = // ATM1: --grbGRB => Gg0Rr0Bb
                          ((v & 0x20) << 1) | // g
                          ((v & 0x10) >> 1) | // r
                          ((v & 0x08) >> 3) | // b
                          ((v & 0x04) << 5) | // G
                          ((v & 0x02) << 3) | // R
                          ((v & 0x01) << 1);  // B
                //else
                //    dst = // ATM2: grbG--RB => Gg0Rr0Bb
                //          ((v & 0x80) >> 1) | // g
                //          ((v & 0x40) >> 3) | // r
                //          ((v & 0x20) >> 5) | // b
                //          ((v & 0x10) << 3) | // G
                //          ((v & 0x02) << 3) | // R
                //          ((v & 0x01) << 1);  // B
                int g = ((int)dst >> 6)&3;
                int r = ((int)dst >> 3)&3;
                int b = (int)dst & 3;
                r *= 85;
                g *= 85;
                b *= 85;
                m_atm_pal_map[i] = 0xFF000000 | (uint)(r << 16) | (uint)(g << 8) | (uint)b;
            }
        }
    }
}
