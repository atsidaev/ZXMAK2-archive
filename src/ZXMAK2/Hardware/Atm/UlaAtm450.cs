﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Hardware.Atm
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
                Renderer.UpdateBorder(m_borderAttr);
            }
        }

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            m_extBorderIndex = (addr & 8) ^ 8;
            base.WritePortFE(addr, value, ref iorqge);
        }

        protected override IMemoryDevice Memory
        {
            set
            {
                base.Memory = value;
                var pageAt = Memory.RamPages.Length > 3 ?
                    Memory.RamPages[m_videoPage == 5 ? 1 : 3] :
                    new byte[0x4000];
                var pageBw = Memory.RamPages.Length > m_videoPage ?
                    Memory.RamPages[m_videoPage] :
                    new byte[0x4000];
                Atm320Renderer.MemoryPage0 = pageAt;
                Atm320Renderer.MemoryPage1 = pageBw;
                Atm640Renderer.MemoryPageAt = pageAt;
                Atm640Renderer.MemoryPageBw = pageBw;
                AtmTxtRenderer.MemoryPageAt = pageAt;
                AtmTxtRenderer.MemoryPageBw = pageBw;
            }
        }

        #endregion

        #region Bus Handlers

        private void busReset()
        {
            var newPalette = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                m_atm_pal[i] = m_pal_startup[i];
                newPalette[i] = m_atm_pal_map[m_atm_pal[i]];
            }
            //TODO: remove palette substitution and replace with UpdatePalette
            SpectrumRenderer.Palette = newPalette;
            Atm320Renderer.Palette = newPalette;
            Atm640Renderer.Palette = newPalette;
            AtmTxtRenderer.Palette = newPalette;
        }

        #endregion

        private int m_extBorderIndex = 0;
        private int m_borderAttr = 0;
        private AtmVideoMode m_mode = AtmVideoMode.Std256x192;

        private readonly byte[] m_atm_pal = new byte[16];
        private readonly uint[] m_atm_pal_map = new uint[0x100];
        private readonly byte[] m_pal_startup = new byte[]
        {
            0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA, 0xF9, 0xF8, 
            0xFF, 0xF6, 0xED, 0xE4, 0xDB, 0xD2, 0xC9, 0xC0,
        };


        protected Atm320Renderer Atm320Renderer = new Atm320Renderer();
        protected Atm640Renderer Atm640Renderer = new Atm640Renderer();
        protected AtmTxtRenderer AtmTxtRenderer = new AtmTxtRenderer();


        public UlaAtm450()
        {
            InitStaticTables();
        }

        protected override void OnRendererInit()
        {
            SpectrumRenderer.Params = CreateSpectrumRendererParams();
            SpectrumRenderer.Palette = SpectrumRenderer.CreatePalette();
            Atm320Renderer.Params = Atm320Renderer.CreateParams();
            Atm320Renderer.Palette = Atm320Renderer.CreatePalette();
            Atm640Renderer.Params = Atm640Renderer.CreateParams();
            Atm640Renderer.Palette = Atm640Renderer.CreatePalette();
            AtmTxtRenderer.Params = AtmTxtRenderer.CreateParams();
            AtmTxtRenderer.Palette = AtmTxtRenderer.CreatePalette();
        }

        protected override SpectrumRendererParams CreateSpectrumRendererParams()
        {
            // ATM1 v4.50
            // Total Size:          448 x 312
            // Visible Size:        320 x 240 (32+256+32 x 24+192+24)
            var timing = SpectrumRenderer.CreateParams();
            timing.c_frameTactCount = 69888;
            timing.c_ulaLineTime = 224;
            timing.c_ulaFirstPaperLine = 80;      // proof???80
            timing.c_ulaFirstPaperTact = 65;// 68;      // proof???68 [32sync+36border+128scr+28border]

            timing.c_ulaBorderTop = 32;//64;
            timing.c_ulaBorderBottom = 32;// 40;
            timing.c_ulaBorderLeftT = 16;
            timing.c_ulaBorderRightT = 16;

            timing.c_ulaIntBegin = 0;
            timing.c_ulaIntLength = 32;

            timing.c_ulaWidth = (timing.c_ulaBorderLeftT + 128 + timing.c_ulaBorderRightT) * 2;
            timing.c_ulaHeight = (timing.c_ulaBorderTop + 192 + timing.c_ulaBorderBottom);
            return timing;
        }

        public override void SetPageMapping(
            int videoPage,
            int page0000,
            int page4000,
            int page8000,
            int pageC000)
        {
            base.SetPageMapping(videoPage, page0000, page4000, page8000, pageC000);
            m_mode = AtmVideoMode.Std256x192;
            Renderer = SpectrumRenderer;
        }

        public void SetPageMappingAtm(
            AtmVideoMode mode,
            int videoPage,
            int page0000,
            int page4000,
            int page8000,
            int pageC000)
        {
            base.SetPageMapping(videoPage, page0000, page4000, page8000, pageC000);
            m_mode = mode;
            switch (mode)
            {
                case AtmVideoMode.Ega320x200: Renderer = Atm320Renderer; break;
                case AtmVideoMode.Hwm640x200: Renderer = Atm640Renderer; break;
                case AtmVideoMode.Std256x192: Renderer = SpectrumRenderer; break;
                case AtmVideoMode.Txt080x025: Renderer = AtmTxtRenderer; break;
            }
            var pageAt = Memory.RamPages[videoPage == 5 ? 1 : 3];
            var pageBw = Memory.RamPages[videoPage];
            Atm320Renderer.MemoryPage0 = pageAt;
            Atm320Renderer.MemoryPage1 = pageBw;
            Atm640Renderer.MemoryPageAt = pageAt;
            Atm640Renderer.MemoryPageBw = pageBw;
            AtmTxtRenderer.MemoryPageAt = pageAt;
            AtmTxtRenderer.MemoryPageBw = pageBw;
        }

        public void SetPaletteAtm(byte value)
        {
            m_atm_pal[m_borderAttr] = value;
            Renderer.UpdatePalette(m_borderAttr, m_atm_pal_map[value]);
        }

        public void SetPaletteAtm2(byte value)
        {
            // grbG--RB => --grbGRB
            value = (byte)((value & 3) | ((value >> 2) & 0xFC));
            SetPaletteAtm(value);
        }

        private void InitStaticTables()
        {
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
                //dst = // ATM2: grbG--RB => Gg0Rr0Bb
                //      ((v & 0x80) >> 1) | // g
                //      ((v & 0x40) >> 3) | // r
                //      ((v & 0x20) >> 5) | // b
                //      ((v & 0x10) << 3) | // G
                //      ((v & 0x02) << 3) | // R
                //      ((v & 0x01) << 1);  // B
                int g = ((int)dst >> 6) & 3;
                int r = ((int)dst >> 3) & 3;
                int b = (int)dst & 3;
                r *= 85;
                g *= 85;
                b *= 85;
                m_atm_pal_map[i] = 0xFF000000 | (uint)(r << 16) | (uint)(g << 8) | (uint)b;
            }
        }
    }

    public enum AtmVideoMode
    {
        Ega320x200 = 0,
        Hwm640x200 = 2,
        Std256x192 = 3,
        Txt080x025 = 6,
    }
}
