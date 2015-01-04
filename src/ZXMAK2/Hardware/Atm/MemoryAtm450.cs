using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Engine.Attributes;


namespace ZXMAK2.Hardware.Atm
{
    public class MemoryAtm450 : MemoryBase
    {
        #region Fields

        protected CpuUnit m_cpu;
        private bool m_lock = false;
        private UlaAtm450 m_ulaAtm;
        private byte m_aFE = 0x80;
        private byte m_aFB = 0x80;

        private bool m_cfg_mem_swap = true;  // ATM 7.10 hi-res video modes swap RAM/CPU address bus A5-A7<=>A8-A10
        
        #endregion Fields


        #region IBusDevice

        public override string Name { get { return "ATM450 512K"; } }
        public override string Description { get { return "ATM450 512K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_cpu = bmgr.CPU;
            m_ulaAtm = bmgr.FindDevice<UlaAtm450>();

            bmgr.SubscribeRdIo(0x0001, 0x0000, BusReadPortFE);      // bit Z emulation
            bmgr.SubscribeWrIo(0x0001, 0x0000, BusWritePortFE);
            bmgr.SubscribeRdIo(0x0004, 0x00FB & 0x0004, BusReadPortFB);   // CPSYS [(addr & 0x7F)==0x7B]

            bmgr.SubscribeWrIo(0x8202, 0x7FFD & 0x8202, BusWritePort7FFD);
            bmgr.SubscribeWrIo(0x8202, 0xFDFD & 0x8202, BusWritePortFDFD);

            bmgr.SubscribeWrIo(0x8202, 0x7DFD & 0x8202, BusWritePort7DFD); // atm_writepal(val);

            bmgr.SubscribeRdMemM1(0xFF00, 0x3D00, BusReadMem3D00_M1);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, BusReadMemRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, BusReadMemRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, BusReadMemRamM1);
            bmgr.SubscribeReset(BusReset);

            // Subscribe before MemoryBase.BusInit 
            // to handle memory switches before read
            base.BusInit(bmgr);
        }

        #endregion

        #region MemoryBase

        public override bool IsMap48 
        { 
            get { return false; } 
        }

        public override bool IsRom48
        {
            get 
            { 
                return MapRead0000 == RomPages[GetRomIndex(RomId.ROM_SOS)] ||
                    MapRead0000 == RomPages[GetRomIndex(RomId.ROM_SOS)+4]; 
            }
        }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            int ramPage = CMR0 & 7;
            int romPage = (CMR0 & 0x10) != 0 ?
                GetRomIndex(RomId.ROM_SOS) :
                GetRomIndex(RomId.ROM_128);
            
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
            {
                if (romPage == 1)
                    romPage = GetRomIndex(RomId.ROM_DOS);
                else
                    romPage = GetRomIndex(RomId.ROM_SYS);
            }

            int sega = CMR1 & 3;   // & 7  for 1024K
            ramPage |= sega << 3;

            bool norom = CPUS;                 // CPUS
            if (!norom)
            {
                if (m_lock)
                    m_aFB = (byte)(m_aFB & ~0x80);
                if (DOSEN && CPNET)   //CPNET?
                    m_aFB |= 0x80; // more priority, then 7FFD

                bool cpsys = CPSYS;                     // CPSYS
                if (cpsys)
                    romPage = GetRomIndex(RomId.ROM_SYS);
                else if (DOSEN)
                    romPage = GetRomIndex(RomId.ROM_DOS);
            }
            romPage |= CMR1 & 4;    // extended 64K rom (if exists)

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

        [HardwareValue("AFE", Description="High address byte of the last port #FE output")]
        public byte AFE
        {
            get { return m_aFE; }
            set { m_aFE = value; }
        }

        [HardwareValue("AFB", Description="High address byte of the last port #FB output")]
        public byte AFB
        {
            get { return m_aFB; }
            set { m_aFB = value; }
        }

        [HardwareValue("CPUS", Description="Enable RAM cache")]
        public bool CPUS
        {
            get { return (m_aFE & 0x80) == 0; }
            set { m_aFE = (byte)((m_aFE & ~0x80) | (value ? 0x80:0)); }
        }

        [HardwareValue("CPSYS", Description="Select system ROM")]
        public bool CPSYS
        {
            get { return (m_aFB & 0x80) != 0; }
            set { m_aFB = (byte)((m_aFB & ~0x80) | (value ? 0x80:0)); }
        }

        [HardwareValue("CPNET", Description="")]
        public bool CPNET
        {
            get { return (CMR1 & 8) != 0; }
            set { CMR1 = (byte)((CMR1 & ~8) | (value ? 8:0)); }
        }
        


        [HardwareValue("SYSEN", Description="")]
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

        public override int GetRomIndex(RomId romId)
        {
            switch (romId)
            {
                case RomId.ROM_128: return 2;
                case RomId.ROM_SOS: return 3;
                case RomId.ROM_DOS: return 1;
                case RomId.ROM_SYS: return 0;
            }
            Logger.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
        }

        public override string GetRomName(int pageNo)
        {
            var name = base.GetRomName(pageNo & 3);
            return string.Format("{0}{1}", name, pageNo>>2);
        }

        #endregion

        #region Bus Handlers

        protected virtual void BusReadPortFE(ushort addr, ref byte value, ref bool iorqge)
        {
            value &= 0x7F;
            value |= atm450_z((int)(m_cpu.Tact % m_ula.FrameTactCount));
        }

        protected virtual void BusReadPortFB(ushort addr, ref byte value, ref bool iorqge)
        {
            m_aFB = (byte)addr;
            UpdateMapping();
        }

        protected virtual void BusWritePortFE(ushort addr, byte value, ref bool iorqge)
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

        protected virtual void BusWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (m_lock)
            {
                return;
            }
            CMR0 = value;
        }

        protected virtual void BusWritePortFDFD(ushort addr, byte value, ref bool iorqge)
        {
            CMR1 = value;
        }

        protected virtual void BusWritePort7DFD(ushort addr, byte value, ref bool iorqge)
        {
            if (m_ulaAtm != null)
            {
                m_ulaAtm.SetPaletteAtm(value);
            }
        }

        protected virtual void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if (!DOSEN && IsRom48)
            {
                DOSEN = true;
            }
        }

        protected virtual void BusReadMemRamM1(ushort addr, ref byte value)
        {
            if (DOSEN)
            {
                DOSEN = false;
            }
        }

        protected virtual void BusReset()
        {
            CMR0 = 0;
            CMR1 = 0;
            DOSEN = false;

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


        public MemoryAtm450()
            : base("ATM450", 8, 32)
        {
        }


        #region Private

        private void atm_memswap()
        {
            if (!m_cfg_mem_swap) return;
            byte[] buffer = new byte[2048];
            for (int subPage = 0; subPage < RamPages.Length * 2; subPage++)
            {
                byte[] bankPage = RamPages[subPage / 2];
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
