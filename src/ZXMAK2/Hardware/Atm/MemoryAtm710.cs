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

			bmgr.SubscribeRDIO(0x0001, 0x0000, busReadPortFE);					// bit Z emulation
			bmgr.SubscribeWRIO(0x009F, 0x00FF & 0x009F, busWritePortXXFF_PAL);	// atm_writepal(val);
			bmgr.SubscribeRDIO(0x8202, 0x7FFD & 0x8202, busReadPort7FFD);					// bit Z emulation

			bmgr.SubscribeWRIO(0x00FF, 0xFF77 & 0x00FF, busWritePortFF77_SYS);
			bmgr.SubscribeWRIO(0x00FF, 0x3FF7 & 0x00FF, busWritePortXFF7_WND);	//ATM3 mask=0x3FFF
			bmgr.SubscribeWRIO(0x8202, 0x7FFD & 0x8202, busWritePort7FFD_128);

			bmgr.SubscribeRESET(busReset);
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

		protected override void LoadRom()
		{
			base.LoadRom();
			LoadRomPack("ATM710");
		}

		protected override void UpdateMapping()
		{
			m_lock = (CMR0 & 0x20) != 0;
			if (PEN)
			{
				int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;
				if (m_ulaAtm != null)
					m_ulaAtm.SetPageMappingAtm(RG, videoPage, -1, -1, -1, -1);
				else
					m_ula.SetPageMapping(videoPage, -1, -1, -1, -1);
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
					romMask = 0x3F;
				int ramMask = RamPages.Length - 1;
				if (ramMask > 0x3F)
					ramMask = 0x3F;

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
					return;
				base.DOSEN = value;
			}
		}

		#endregion

		#region Bus Handlers

		private void busReadPortFE(ushort addr, ref byte value, ref bool iorqge)
		{
			// bit Z emulation
			value &= 0xDF;
			int frameLength = m_ula.FrameTactCount;
			int frameTact = (int)(m_cpu.Tact % frameLength);
			if (atm710_z(frameTact, frameLength))
				value |= 0x20;
		}

		private void busWritePortXXFF_PAL(ushort addr, byte value, ref bool iorqge)
		{
			if ((DOSEN || SYSEN) && PEN2 && m_ulaAtm != null)
			{
				m_ulaAtm.SetPaletteAtm2(value);
			}
		}

		private void busReadPort7FFD(ushort addr, ref byte value, ref bool iorqge)
		{
			if (iorqge && (DOSEN || SYSEN))
			{
				// ADC ready emulation
				iorqge = false;
				value &= 0x7F;
			}
		}

		private void busWritePortFF77_SYS(ushort addr, byte value, ref bool iorqge) // ATM2
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

		private void busWritePortXFF7_WND(ushort addr, byte value, ref bool iorqge) // ATM2
		{
			if (DOSEN || SYSEN)
			{
				m_pXFF7[((CMR0 & 0x10) >> 2) | ((addr >> 14) & 3)] = value ^ 0x3F; //(((value & 0xC0) << 2) | (value & 0x3F)) ^ 0x33F;
				UpdateMapping();
			}
		}

		private void busWritePort7FFD_128(ushort addr, byte value, ref bool iorqge)
		{
			if (m_lock || DOSEN)
				return;
			CMR0 = value;
		}

		private void busReset()
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
			busReset();
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
		{
			Init();
		}

		protected virtual void Init()
		{
			m_ramPages = new byte[64][];
			for (int i = 0; i < m_ramPages.Length; i++)
				m_ramPages[i] = new byte[0x4000];
			m_romPages = new byte[8][];
			for (int i = 0; i < m_romPages.Length; i++)
				m_romPages[i] = new byte[0x4000];
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
			{ // NORMAL SPEED mode
				if ((uint)(frameTact - 7200) < 40 || (uint)(frameTact - 7284) < 40 || (uint)(frameTact - 7326) < 40) return false;
			}
			else
			{ // TURBO mode
				if ((uint)(frameTact - 21514) < 40 || (uint)(frameTact - 21703) < 80 || (uint)(frameTact - 21808) < 40) return false;
			}
			return true;
		}

		#endregion
	}




	#region TMP
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
                AtmVideoMode videoMode = (AtmVideoMode)(m_pFF77 & 7);
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
	*/
	#endregion
}
