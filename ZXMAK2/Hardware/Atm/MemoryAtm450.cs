using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;

namespace ZXMAK2.Hardware.Atm
{
    public class MemoryAtm450 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "ATM450 512K"; } }
        public override string Description { get { return "ATM450 512K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);

            m_cpu = bmgr.CPU;
            m_ulaAtm = base.m_ula as UlaAtm450;

            bmgr.SubscribeRdIo(0x0001, 0x0000, BusReadPortFE);      // bit Z emulation
            bmgr.SubscribeWrIo(0x0001, 0x0000, BusWritePortFE);
            bmgr.SubscribeRdIo(0x0004, 0x00FB & 0x0004, BusReadPortFB);   // CPSYS [(addr & 0x7F)==0x7B]

            bmgr.SubscribeWrIo(0x8202, 0x7FFD & 0x8202, BusWritePort7FFD);
            bmgr.SubscribeWrIo(0x8202, 0xFDFD & 0x8202, BusWritePortFDFD);

            bmgr.SubscribeWrIo(0x8202, 0x7DFD & 0x8202, BusWritePort7DFD); // atm_writepal(val);

            bmgr.SubscribeReset(BusReset);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

        public override bool IsMap48 { get { return false; } }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            int ramPage = CMR0 & 7;
            int romPage = (CMR0 & 0x10) != 0 ?
                GetRomIndex(RomName.ROM_SOS) :
                GetRomIndex(RomName.ROM_128);
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
            {
                if (romPage == 1)
                    romPage = GetRomIndex(RomName.ROM_DOS);
                else
                    romPage = GetRomIndex(RomName.ROM_SYS);
            }

            int sega = CMR1 & 3;   // & 7  for 1024K
            ramPage |= sega << 3;

            bool norom = (m_aFE & 0x80) == 0;                 // CPUS
            if (!norom)
            {
                if (m_lock)
                    m_aFB = (byte)(m_aFB & ~0x80);
                if (DOSEN && (CMR1 & 8) != 0)   //CPNET?
                    m_aFB |= 0x80; // more priority, then 7FFD

                bool cpsys = (m_aFB & 0x80) != 0;                     // CPSYS
                if (cpsys)
                    romPage = GetRomIndex(RomName.ROM_SYS);
                else if (DOSEN)
                    romPage = GetRomIndex(RomName.ROM_DOS);
            }

            if (m_ulaAtm != null)
            {
                AtmVideoMode videoMode = (AtmVideoMode)(((m_aFE >> 6) & 1) | ((m_aFE >> 4) & 2)); // (m_aFE >> 5) & 3
                m_ulaAtm.SetPageMappingAtm(
                    videoMode,
                    videoPage,
                    norom ? 0 : -1,
                    norom ? 4 : 5,
                    2,
                    ramPage);
            }
            else
            {
                m_ula.SetPageMapping(
                    videoPage,
                    norom ? 0 : -1,
                    norom ? 4 : 5,
                    2,
                    ramPage);
            }

            MapRead0000 = norom ? RamPages[0] : RomPages[romPage];
            MapRead4000 = norom ? RamPages[4] : RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = norom ? RamPages[0] : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }

        public override bool SYSEN
        {
            get { return (m_aFB & 0x80) != 0 && (m_aFE & 0x80) != 0; }
            set
            {
                byte old_aFE = m_aFE;
                if (value)
                {
                    m_aFE = 0x80;
                    m_aFB = 0x80;
                }
                else
                {
                    m_aFE = 0x80 | 0x60;
                    m_aFB = 0x7F;
                }
                if (((m_aFE ^ old_aFE) & 0x40) != 0)
                {
                    atm_memswap();
                }
                UpdateMapping();
            }
        }

        public override int GetRomIndex(RomName romId)
        {
            switch (romId)
            {
                case RomName.ROM_128: return 2;
                case RomName.ROM_SOS: return 3;
                case RomName.ROM_DOS: return 1;
                case RomName.ROM_SYS: return 0;
            }
            LogAgent.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
        }

        #endregion

        #region Bus Handlers

        private void BusReadPortFE(ushort addr, ref byte value, ref bool iorqge)
        {
            value &= 0x7F;
            value |= atm450_z((int)(m_cpu.Tact % m_ula.FrameTactCount));
        }

        private void BusReadPortFB(ushort addr, ref byte value, ref bool iorqge)
        {
            m_aFB = (byte)addr;
            UpdateMapping();
        }

        private void BusWritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            addr &= 0x00FF;
            byte old_aFE = m_aFE;
            m_aFE = (byte)addr;
            if (((addr ^ old_aFE) & 0x40) != 0)
            {
                atm_memswap();
            }
            if (((addr ^ old_aFE) & 0x80) != 0)
            {
                UpdateMapping();
            }
        }

        private void BusWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (m_lock)
            {
                return;
            }
            CMR0 = value;
        }

        private void BusWritePortFDFD(ushort addr, byte value, ref bool iorqge)
        {
            CMR1 = value;
        }

        private void BusWritePort7DFD(ushort addr, byte value, ref bool iorqge)
        {
            if (m_ulaAtm != null)
            {
                m_ulaAtm.SetPaletteAtm(value);
            }
        }

        private void BusReset()
        {
            CMR0 = 0;
            CMR1 = 0;

            byte old_aFE = m_aFE;
            //RM_DOS
            //m_aFE = 0x80 | 0x60;
            //m_aFB = 0;

            //DEFAULT
            m_aFE = 0x80;
            m_aFB = 0x80;

            m_aFE |= 0x60;  // set mode 3 (standard spectrum 256x192)

            if (((m_aFE ^ old_aFE) & 0x40) != 0) atm_memswap();
            UpdateMapping();
        }

        #endregion

        private byte[][] m_ramPages = new byte[32][];
        private byte[] m_trashPage = new byte[0x4000];
        private bool m_lock = false;
        private Z80CPU m_cpu;
        private UlaAtm450 m_ulaAtm;
        private byte m_aFE = 0x80;
        private byte m_aFB = 0x80;


        private bool m_cfg_mem_swap = true;  // ATM 7.10 hi-res video modes swap RAM/CPU address bus A5-A7<=>A8-A10


        public MemoryAtm450()
            : base("ATM450")
        {
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }


        #region Private

        private void atm_memswap()
        {
            if (!m_cfg_mem_swap) return;
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

        private byte atm450_z(int t)
        {
            // PAL hardware gives 3 zeros in secret short time intervals
            if (m_ula.FrameTactCount < 80000)
            { 
                // NORMAL SPEED mode
                if ((uint)(t - 7200) < 40 || (uint)(t - 7284) < 40 || (uint)(t - 7326) < 40) return 0;
            }
            else
            { 
                // TURBO mode
                if ((uint)(t - 21514) < 40 || (uint)(t - 21703) < 80 || (uint)(t - 21808) < 40) return 0;
            }
            return 0x80;
        }

        #endregion
    }
}
