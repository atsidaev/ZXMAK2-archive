using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Devices.Ula;
using ZXMAK2.Engine.Z80;

namespace ZXMAK2.Engine.Devices.Memory
{
    /*
    
    public class MemoryAtm710 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "ATM710 512K"; } }
        public override string Description { get { return "ATM710 512K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);

            m_cpu = bmgr.CPU;
            m_ulaAtm = base.m_ula as UlaAtm450;

            bmgr.SubscribeRDIO(0x0001, 0x0000, busReadPortFE);      // bit Z emulation
            //bmgr.SubscribeWRIO(0x0001, 0x0000, busWritePortFE);
            //bmgr.SubscribeRDIO(0x0004, 0x00FB & 0x0004, busReadPortFB);   // CPSYS [(addr & 0x7F)==0x7B]

            bmgr.SubscribeWRIO(0x00FF, 0xFF77 & 0x00FF, busWritePortFF77);
            bmgr.SubscribeWRIO(0x00FF, 0x3FF7 & 0x00FF, busWritePortXFF7);  //ATM3 mask=0x3FFF


            bmgr.SubscribeWRIO(0x8202, 0x7FFD & 0x8202, busWritePort7FFD);
            bmgr.SubscribeWRIO(0x8202, 0xFDFD & 0x8202, busWritePortFDFD);

            bmgr.SubscribeWRIO(0x8202, 0x7DFD & 0x8202, busWritePort7DFD); // atm_writepal(val);

            bmgr.SubscribeRESET(busReset);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

		public override bool IsMap48 { get { return false; } }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            int ramPage = CMR0 & 7;
            int romPage = ((CMR0 & 0x10) >> 4) != 0 ? GetRomIndex(RomName.ROM_SOS) : GetRomIndex(RomName.ROM_SOS);
			int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
            {
                if (romPage == GetRomIndex(RomName.ROM_SOS))
                    romPage = GetRomIndex(RomName.ROM_DOS);    // dos
                else
                    romPage = GetRomIndex(RomName.ROM_SYS);    // sys
            }
            if (SYSEN)
            {
                romPage = GetRomIndex(RomName.ROM_SYS);    // sys
            }

            int sega = CMR1 & 3;   // & 7  for 1024K
            ramPage |= sega << 3;
            bool norom = false;

            if (m_ulaAtm != null)
            {
                int videoMode = m_pFF77 & 7;
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
            get { return true; }
            set
            {
                UpdateMapping();
            }
        }

        public override int GetRomIndex(RomName romId)
        {
            switch (romId)
            {
                case RomName.ROM_SOS: return 0;
                case RomName.ROM_DOS: return 1;
                case RomName.ROM_128: return 2;
                case RomName.ROM_SYS: return 3;
            }
            throw new NotImplementedException();
        }

        protected override void LoadRom()
        {
            base.LoadRom();
			LoadRomPack("ATM710");
        }

        #endregion

        #region Bus Handlers

        private void busReadPortFE(ushort addr, ref byte value, ref bool iorqge)
        {
            // bit Z emulation
            value &= 0xDF;
            if (atm710_z((int)(m_cpu.Tact % m_ula.FrameTactCount)))
                value |= 0x20;
        }

        private void busWritePortFF77(ushort addr, byte value, ref bool iorqge) // ATM2
        {
            //set_atm_FF77(addr, value);
            
            if (((m_pFF77 ^ value) & 1)!=0)
                atm_memswap();

            //if (((m_pFF77 & 7) ^ (value & 7))!=0)
            //{
            //    // change video mode...
            //    AtmApplySideEffectsWhenChangeVideomode(val);
            //}

            m_pFF77 = value;
            m_aFF77 = addr;
            UpdateMapping();
            //cpu.int_gate = (comp.pFF77 & 0x20) != false;
            //set_banks();
        }

        private void busWritePortXFF7(ushort addr, byte value, ref bool iorqge) // ATM2
        {
            m_pXFF7[((CMR0 & 0x10) >> 2) | ((addr >> 14) & 3)] = (((value & 0xC0) << 2) | (value & 0x3F)) ^ 0x33F;
            UpdateMapping();
        }

        private void busWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (DOSEN)
                return;
            if (!m_lock)
                CMR0 = value;
        }

        private void busWritePortFDFD(ushort addr, byte value, ref bool iorqge)
        {
            if (DOSEN)
                return;
            CMR1 = value;
        }

        private void busWritePort7DFD(ushort addr, byte value, ref bool iorqge)
        {
            if (DOSEN)
                return;
            if (m_ulaAtm != null && (m_aFF77 & 0x4000)==0)
                m_ulaAtm.SetPaletteAtm(value);
        }

        private void busReset()
        {
            m_aFF77 = 0;
            m_pFF77 = 0;

            m_pFF77 |= 3; // set video mode

            CMR0 = 0;
            CMR1 = 0;
        }

        #endregion

        private byte[][] m_ramPages = new byte[32][];
		private byte[] m_trashPage = new byte[0x4000];
		private bool m_lock = false;
        private Z80CPU m_cpu;
        private UlaAtm450 m_ulaAtm;
        
        private int[] m_pXFF7 = new int[8]; // ATM 7.10 / ATM3(4Mb) memory map
        private int m_aFF77;
        private byte m_pFF77;
        

        private bool m_cfg_mem_swap = true;  // ATM 7.10 hi-res video modes swap RAM/CPU address bus A5-A7<=>A8-A10


        public MemoryAtm710()
        {
            for (int i = 0; i < m_ramPages.Length; i++)
                m_ramPages[i] = new byte[0x4000];
        }


        #region Private

        private void atm_memswap()
        {
            if (!m_cfg_mem_swap) return;
            byte[] buffer = new byte[2048];
            for (int subPage = 0; subPage < m_ramPages.Length*2; subPage++)
            {
                byte[] bankPage = m_ramPages[subPage / 2];
                int bankIndex = (subPage % 2)*2048;
                for (int addr=0; addr < 2048; addr++)
                    buffer[addr] = bankPage[bankIndex+(addr & 0x1F) + ((addr >> 3) & 0xE0) + ((addr << 3) & 0x700)];
                for (int addr=0; addr < 2048; addr++)
                    bankPage[bankIndex+addr] = buffer[addr];
            }
        }

        private bool atm710_z(int t)
        {
            // PAL hardware gives 3 zeros in secret short time intervals
            if (m_ula.FrameTactCount < 80000) { // NORMAL SPEED mode
                if ((uint)(t-7200) < 40 || (uint)(t-7284) < 40 || (uint)(t-7326) < 40) return false;
            } else { // TURBO mode
                if ((uint)(t-21514) < 40 || (uint)(t-21703) < 80 || (uint)(t-21808) < 40) return false;
            }
            return true;
        }

        #endregion
    }

    #region TMP

    public class MemoryAtm710TMP : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "ATM710 512K"; } }
        public override string Description { get { return "ATM710 512K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);

            m_cpu = bmgr.CPU;
            m_ulaAtm = base.m_ula as UlaAtm450;

            bmgr.SubscribeRDIO(0x0001, 0x0000, busReadPortFE);      // bit Z emulation
            //bmgr.SubscribeWRIO(0x0001, 0x0000, busWritePortFE);
            //bmgr.SubscribeRDIO(0x0004, 0x00FB & 0x0004, busReadPortFB);   // CPSYS [(addr & 0x7F)==0x7B]

            bmgr.SubscribeWRIO(0x00FF, 0xFF77 & 0x00FF, busWritePortFF77);
            bmgr.SubscribeWRIO(0x00FF, 0x3FF7 & 0x00FF, busWritePortXFF7);  //ATM3 mask=0x3FFF


            bmgr.SubscribeWRIO(0x8202, 0x7FFD & 0x8202, busWritePort7FFD);
            bmgr.SubscribeWRIO(0x8202, 0xFDFD & 0x8202, busWritePortFDFD);

            bmgr.SubscribeWRIO(0x8202, 0x7DFD & 0x8202, busWritePort7DFD); // atm_writepal(val);

            bmgr.SubscribeRESET(busReset);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

        public override bool IsMap48 { get { return false; } }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            int ramPage = CMR0 & 7;
            int romPage = ((CMR0 & 0x10) >> 4) != 0 ? GetRomIndex(RomName.ROM_SOS) : GetRomIndex(RomName.ROM_SOS);
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
            {
                if (romPage == GetRomIndex(RomName.ROM_SOS))
                    romPage = GetRomIndex(RomName.ROM_DOS);    // dos
                else
                    romPage = GetRomIndex(RomName.ROM_SYS);    // sys
            }
            if (SYSEN)
            {
                romPage = GetRomIndex(RomName.ROM_SYS);    // sys
            }

            int sega = CMR1 & 3;   // & 7  for 1024K
            ramPage |= sega << 3;
            bool norom = false;

            if (m_ulaAtm != null)
            {
                int videoMode = m_pFF77 & 7;
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
            get { return true; }
            set
            {
                UpdateMapping();
            }
        }

        public override int GetRomIndex(RomName romId)
        {
            switch (romId)
            {
                case RomName.ROM_SOS: return 0;
                case RomName.ROM_DOS: return 1;
                case RomName.ROM_128: return 2;
                case RomName.ROM_SYS: return 3;
            }
            throw new NotImplementedException();
        }

        protected override void LoadRom()
        {
            base.LoadRom();
            LoadRomPack("ATM710");
        }

        #endregion

        #region Bus Handlers

        private void busReadPortFE(ushort addr, ref byte value, ref bool iorqge)
        {
            // bit Z emulation
            value &= 0xDF;
            if (atm710_z((int)(m_cpu.Tact % m_ula.FrameTactCount)))
                value |= 0x20;
        }

        private void busWritePortFF77(ushort addr, byte value, ref bool iorqge) // ATM2
        {
            //set_atm_FF77(addr, value);

            if (((m_pFF77 ^ value) & 1) != 0)
                atm_memswap();

            //if (((m_pFF77 & 7) ^ (value & 7))!=0)
            //{
            //    // change video mode...
            //    AtmApplySideEffectsWhenChangeVideomode(val);
            //}

            m_pFF77 = value;
            m_aFF77 = addr;
            UpdateMapping();
            //cpu.int_gate = (comp.pFF77 & 0x20) != false;
            //set_banks();
        }

        private void busWritePortXFF7(ushort addr, byte value, ref bool iorqge) // ATM2
        {
            m_pXFF7[((CMR0 & 0x10) >> 2) | ((addr >> 14) & 3)] = (((value & 0xC0) << 2) | (value & 0x3F)) ^ 0x33F;
            UpdateMapping();
        }

        private void busWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (DOSEN)
                return;
            if (!m_lock)
                CMR0 = value;
        }

        private void busWritePortFDFD(ushort addr, byte value, ref bool iorqge)
        {
            if (DOSEN)
                return;
            CMR1 = value;
        }

        private void busWritePort7DFD(ushort addr, byte value, ref bool iorqge)
        {
            if (DOSEN)
                return;
            if (m_ulaAtm != null && (m_aFF77 & 0x4000) == 0)
                m_ulaAtm.SetPaletteAtm(value);
        }

        private void busReset()
        {
            m_aFF77 = 0;
            m_pFF77 = 0;

            m_pFF77 |= 3; // set video mode

            CMR0 = 0;
            CMR1 = 0;
        }

        #endregion

        private byte[][] m_ramPages = new byte[32][];
        private byte[] m_trashPage = new byte[0x4000];
        private bool m_lock = false;
        private Z80CPU m_cpu;
        private UlaAtm450 m_ulaAtm;

        private int[] m_pXFF7 = new int[8]; // ATM 7.10 / ATM3(4Mb) memory map
        private int m_aFF77;
        private byte m_pFF77;


        private bool m_cfg_mem_swap = true;  // ATM 7.10 hi-res video modes swap RAM/CPU address bus A5-A7<=>A8-A10


        public MemoryAtm710TMP()
        {
            for (int i = 0; i < m_ramPages.Length; i++)
                m_ramPages[i] = new byte[0x4000];
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

        private bool atm710_z(int t)
        {
            // PAL hardware gives 3 zeros in secret short time intervals
            if (m_ula.FrameTactCount < 80000)
            { // NORMAL SPEED mode
                if ((uint)(t - 7200) < 40 || (uint)(t - 7284) < 40 || (uint)(t - 7326) < 40) return false;
            }
            else
            { // TURBO mode
                if ((uint)(t - 21514) < 40 || (uint)(t - 21703) < 80 || (uint)(t - 21808) < 40) return false;
            }
            return true;
        }

        #endregion
    }

    #endregion
    */ 
}
