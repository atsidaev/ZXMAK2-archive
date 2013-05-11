using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;

namespace ZXMAK2.Hardware.Atm
{
    public class MemoryAtm710 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "ATM710 1024K"; } }
        public override string Description { get { return "ATM710 1024K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);

            m_cpu = bmgr.CPU;
            m_ulaAtm = base.m_ula as UlaAtm450;

            bmgr.SubscribeRdIo(0x0001, 0x0000, BusReadPortFE);					// bit Z emulation
            bmgr.SubscribeWrIo(0x009F, 0x00FF & 0x009F, BusWritePortXXFF_PAL);	// atm_writepal(val);
            bmgr.SubscribeRdIo(0x8202, 0x7FFD & 0x8202, BusReadPort7FFD);					// bit Z emulation

            bmgr.SubscribeWrIo(0x00FF, 0xFF77 & 0x00FF, BusWritePortFF77_SYS);
            bmgr.SubscribeWrIo(0x00FF, 0x3FF7 & 0x00FF, BusWritePortXFF7_WND);	//ATM3 mask=0x3FFF
            
            // fix for #7FFD (original ATM710 mask is #8202)
            // http://www.nedopc.com/ATMZAK/atm710re.htm#re11
            //bmgr.SubscribeWrIo(0x8202, 0x7FFD & 0x8202, BusWritePort7FFD_128);
            bmgr.SubscribeWrIo(0x8002, 0x7FFD & 0x8002, BusWritePort7FFD_128);

            bmgr.SubscribeReset(BusReset);
        }


        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }
        public override byte[][] RomPages { get { return m_romPages; } }

        public override bool IsMap48 { get { return false; } }
        public override bool IsRom48 { get { return !DOSEN && (CMR0 & 0x10) != 0; } }

        public override int GetRomIndex(RomName romId)
        {
            switch (romId)
            {
                case RomName.ROM_SOS: return 4;
                case RomName.ROM_DOS: return 5;
                case RomName.ROM_128: return 6;
                case RomName.ROM_SYS: return 7;
            }
            throw new NotImplementedException();
        }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            if (PEN)
            {
                int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;
                if (m_ulaAtm != null)
                {
                    m_ulaAtm.SetPageMappingAtm(
                        RG, videoPage, -1, -1, -1, -1);
                }
                else
                {
                    m_ula.SetPageMapping(
                        videoPage, -1, -1, -1, -1);
                }
                int romPage = RomPages.Length - 1;
                MapRead0000 = RomPages[romPage];
                MapRead4000 = RomPages[romPage];
                MapRead8000 = RomPages[romPage];
                MapReadC000 = RomPages[romPage];

                MapWrite0000 = m_trashPage;
                MapWrite4000 = m_trashPage;
                MapWrite8000 = m_trashPage;
                MapWriteC000 = m_trashPage;

                Map48[0] = -1;
                Map48[1] = -1;
                Map48[2] = -1;
                Map48[3] = -1;
            }
            else
            {
                int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;
                int romMask = RomPages.Length - 1;
                if (romMask > 0x3F)
                {
                    romMask = 0x3F;
                }
                int ramMask = RamPages.Length - 1;
                if (ramMask > 0x3F)
                {
                    ramMask = 0x3F;
                }

                int index = ((CMR0 & 0x10) >> 2);
                int w0 = m_pXFF7[index + 0];
                int w1 = m_pXFF7[index + 1];
                int w2 = m_pXFF7[index + 2];
                int w3 = m_pXFF7[index + 3];
                int romPage0 = (w0 & 0x80) != 0 ? (w0 & romMask & 0xFE) | (DOSEN | SYSEN ? 1 : 0) : w0 & romMask;
                int romPage1 = (w1 & 0x80) != 0 ? (w1 & romMask & 0xFE) | (DOSEN | SYSEN ? 1 : 0) : w1 & romMask;
                int romPage2 = (w2 & 0x80) != 0 ? (w2 & romMask & 0xFE) | (DOSEN | SYSEN ? 1 : 0) : w2 & romMask;
                int romPage3 = (w3 & 0x80) != 0 ? (w3 & romMask & 0xFE) | (DOSEN | SYSEN ? 1 : 0) : w3 & romMask;
                int ramPage0 = (w0 & 0x80) != 0 ? (w0 & ramMask & 0xF8) | (CMR0 & 7) : w0 & ramMask;
                int ramPage1 = (w1 & 0x80) != 0 ? (w1 & ramMask & 0xF8) | (CMR0 & 7) : w1 & ramMask;
                int ramPage2 = (w2 & 0x80) != 0 ? (w2 & ramMask & 0xF8) | (CMR0 & 7) : w2 & ramMask;
                int ramPage3 = (w3 & 0x80) != 0 ? (w3 & ramMask & 0xF8) | (CMR0 & 7) : w3 & ramMask;
                bool isRam0 = (w0 & 0x40) != 0;
                bool isRam1 = (w1 & 0x40) != 0;
                bool isRam2 = (w2 & 0x40) != 0;
                bool isRam3 = (w3 & 0x40) != 0;

                if (m_ulaAtm != null)
                {
                    m_ulaAtm.SetPageMappingAtm(
                        RG,
                        videoPage,
                        isRam0 ? ramPage0 : -1,
                        isRam1 ? ramPage1 : -1,
                        isRam2 ? ramPage2 : -1,
                        isRam3 ? ramPage3 : -1);
                }
                else
                {
                    m_ula.SetPageMapping(
                        videoPage,
                        isRam0 ? ramPage0 : -1,
                        isRam1 ? ramPage1 : -1,
                        isRam2 ? ramPage2 : -1,
                        isRam3 ? ramPage3 : -1);
                }

                MapRead0000 = isRam0 ? RamPages[ramPage0] : RomPages[romPage0];
                MapRead4000 = isRam1 ? RamPages[ramPage1] : RomPages[romPage1];
                MapRead8000 = isRam2 ? RamPages[ramPage2] : RomPages[romPage2];
                MapReadC000 = isRam3 ? RamPages[ramPage3] : RomPages[romPage3];

                MapWrite0000 = isRam0 ? MapRead0000 : m_trashPage;
                MapWrite4000 = isRam1 ? MapRead4000 : m_trashPage;
                MapWrite8000 = isRam2 ? MapRead8000 : m_trashPage;
                MapWriteC000 = isRam3 ? MapReadC000 : m_trashPage;

                Map48[0] = isRam0 ? -1 : romPage0;
                Map48[1] = isRam1 ? ramPage1 : -1;
                Map48[2] = isRam2 ? ramPage2 : -1;
                Map48[3] = isRam3 ? ramPage3 : -1;
            }
        }

        #endregion

        #region Hardware Values

        [HardwareValue("PEN", Description = "Disable memory manager")]
        public bool PEN
        {
            get { return (m_aFF77 & 0x100) == 0; }
            set { m_aFF77 = (m_aFF77 & ~0x100) | (value ? 0x0000 : 0x0100); UpdateMapping(); }
        }

        [HardwareValue("CPM", Description = "Enable continous access for extended ports and TRDOS ROM")]
        public override bool SYSEN
        {
            get { return (m_aFF77 & 0x200) == 0; }
            set { m_aFF77 = (m_aFF77 & ~0x200) | (value ? 0x0000 : 0x0200); UpdateMapping(); }
        }

        [HardwareValue("PEN2", Description = "Enable palette change through port #FF")]
        public bool PEN2
        {
            get { return (m_aFF77 & 0x4000) == 0; }
            set { m_aFF77 = (m_aFF77 & ~0x4000) | (value ? 0x0000 : 0x4000); UpdateMapping(); }
        }

        [HardwareValue("RG", Description = "Video mode")]
        public AtmVideoMode RG
        {
            get { return (AtmVideoMode)(m_pFF77 & 7); }
            set { m_pFF77 = (m_pFF77 & 0xF8) | ((int)value & 7); UpdateMapping(); }
        }

        [HardwareValue("Z_I", Description = "Enable HSYNC interrupts")]
        public bool Z_I
        {
            get { return (m_pFF77 & 0x20) == 0; }
            set { m_pFF77 = (m_pFF77 & ~0x20) | (value ? 0x20 : 0x00); UpdateMapping(); }
        }

        public override bool DOSEN
        {
            set
            {
                if ((m_pXFF7[(CMR0 & 0x10) >> 2] & 0x80) == 0)
                {
                    return;
                }
                // TODO: check on real ATM, if true then optimize all checks DOSEN | SYSEN with simple DOSEN
                base.DOSEN = value | SYSEN;
            }
        }

        #endregion

        #region Bus Handlers

        private void BusReadPortFE(ushort addr, ref byte value, ref bool iorqge)
        {
            // bit Z emulation
            value &= 0xDF;
            int frameLength = m_ula.FrameTactCount;
            int frameTact = (int)(m_cpu.Tact % frameLength);
            if (atm710_z(frameTact, frameLength))
            {
                value |= 0x20;
            }
        }

        private void BusWritePortXXFF_PAL(ushort addr, byte value, ref bool iorqge)
        {
            if ((DOSEN || SYSEN) && PEN2 && m_ulaAtm != null)
            {
                m_ulaAtm.SetPaletteAtm2(value);
            }
        }

        private void BusReadPort7FFD(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge && (DOSEN || SYSEN))
            {
                // ADC ready emulation
                iorqge = false;
                value &= 0x7F;
            }
        }

        private void BusWritePortFF77_SYS(ushort addr, byte value, ref bool iorqge) // ATM2
        {
            if (DOSEN || SYSEN)
            {
                m_pFF77 = value;
                m_aFF77 = addr;
                UpdateMapping();
                //cpu.int_gate = (comp.pFF77 & 0x20) != false;
                //set_banks();
            }
        }

        private void BusWritePortXFF7_WND(ushort addr, byte value, ref bool iorqge) // ATM2
        {
            if (DOSEN || SYSEN)
            {
                m_pXFF7[((CMR0 & 0x10) >> 2) | ((addr >> 14) & 3)] = value ^ 0x3F; //(((value & 0xC0) << 2) | (value & 0x3F)) ^ 0x33F;
                UpdateMapping();
            }
        }

        private void BusWritePort7FFD_128(ushort addr, byte value, ref bool iorqge)
        {
            if (m_lock)
            {
                return;
            }
            CMR0 = value;
        }

        private void BusReset()
        {
            m_aFF77 = 0;
            m_pFF77 = 0;

            //m_pFF77 = (m_pFF77 & 0xF8) | 3; // set video mode

            CMR0 = 0;
            CMR1 = 0;
            UpdateMapping();
        }

        public override void ResetState()
        {
            base.ResetState();
            BusReset();
        }

        #endregion

        private byte[][] m_ramPages;
        protected byte[][] m_romPages;
        private byte[] m_trashPage = new byte[0x4000];
        private bool m_lock = false;
        private Z80CPU m_cpu;
        private UlaAtm450 m_ulaAtm;

        private int m_aFF77;
        private int m_pFF77;
        private int[] m_pXFF7 = new int[8]; // ATM 7.10 / ATM3(4Mb) memory map


        public MemoryAtm710()
            : base("ATM710")
        {
            Init();
        }

        protected virtual void Init()
        {
            m_ramPages = new byte[64][];
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
            m_romPages = new byte[8][];
            for (int i = 0; i < m_romPages.Length; i++)
            {
                m_romPages[i] = new byte[0x4000];
            }
        }

        #region Private methods

        private void atm_memswap()
        {
            //if (!m_cfg_mem_swap) return;
            byte[] buffer = new byte[2048];
            for (int subPage = 0; subPage < m_ramPages.Length * 2; subPage++)
            {
                byte[] bankPage = m_ramPages[subPage / 2];
                int bankIndex = (subPage % 2) * 2048;
                for (int addr = 0; addr < 2048; addr++)
                    buffer[addr] = bankPage[bankIndex + (addr & 0x1F) + ((addr >> 3) & 0xE0) + ((addr << 3) & 0x700)];
                for (int addr = 0; addr < 2048; addr++)
                    bankPage[bankIndex + addr] = buffer[addr];
            }
        }

        private bool atm710_z(int frameTact, int frameLength)
        {
            // PAL hardware gives 3 zeros in secret short time intervals
            if (frameLength < 80000)
            { 
                // NORMAL SPEED mode
                if ((uint)(frameTact - 7200) < 40 || (uint)(frameTact - 7284) < 40 || (uint)(frameTact - 7326) < 40) return false;
            }
            else
            { 
                // TURBO mode
                if ((uint)(frameTact - 21514) < 40 || (uint)(frameTact - 21703) < 80 || (uint)(frameTact - 21808) < 40) return false;
            }
            return true;
        }

        #endregion
    }
}
