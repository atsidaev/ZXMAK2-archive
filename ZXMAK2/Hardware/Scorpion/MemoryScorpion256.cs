using System;

using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;


namespace ZXMAK2.Hardware.Scorpion
{
    public class MemoryScorpion256 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion 256K"; } }
        public override string Description { get { return "Scorpion 256K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            m_cpu = bmgr.CPU;
            bmgr.SubscribeWrIo(0xD027, 0x5025, busWritePort7FFD);
            bmgr.SubscribeWrIo(0xD027, 0x1025, busWritePort1FFD);
            bmgr.SubscribeNmiRq(BusNmiRq);
            bmgr.SubscribeNmiAck(BusNmiAck);
            bmgr.SubscribeReset(BusReset);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

        public override bool IsMap48 { get { return false; } }

        public override int GetRomIndex(RomName romId)
        {
            switch (romId)
            {
                case RomName.ROM_128: return 0;
                case RomName.ROM_SOS: return 1;
                case RomName.ROM_SYS: return 2;
                case RomName.ROM_DOS: return 3;
            }
            LogAgent.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
        }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;
            bool norom = (CMR1 & 0x01) != 0;

            int romPage = GetRomPage();
            int ramPage = GetRamPage();

            m_ula.SetPageMapping(videoPage, norom ? 0 : -1, 5, 2, ramPage);
            MapRead0000 = norom ? RamPages[0] : RomPages[romPage];
            MapRead4000 = RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = norom ? RamPages[0] : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }

        protected virtual int GetRamPage()
        {
            int ramPage = CMR0 & 7;
            int sega = (CMR1 & 0x10) >> 4;
            ramPage |= sega << 3;
            return ramPage;
        }

        protected virtual int GetRomPage()
        {
            int romPage = (CMR0 & 0x10) >> 4;
            if (DOSEN)      // trdos or 48/128
                romPage = 3;
            if (SYSEN)
                romPage = 2;
            return romPage;
        }

        public override bool SYSEN
        {
            get { return (CMR1 & 0x02) != 0; }
            set
            {
                if (value)
                    CMR1 |= 0x02;
                else
                    CMR1 &= 0x02 ^ 0xFF;
                UpdateMapping();
            }
        }


        #endregion

        #region Bus Handlers

        private void busWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            if (!m_lock)
                CMR0 = value;
        }

        private void busWritePort1FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            CMR1 = value;
        }

        private void BusReset()
        {
            CMR1 = 0;
            CMR0 = 0;
        }

        private void BusNmiRq(BusCancelArgs e)
        {
            // check DOSEN to avoid conflict with BDI
            e.Cancel = DOSEN || (m_cpu.regs.PC & 0xC000) == 0;
        }

        private void BusNmiAck()
        {
            // enable shadow rom
            CMR1 |= 0x02;
        }

        #endregion


        protected byte[][] m_ramPages = new byte[16][];
        private byte[] m_trashPage = new byte[0x4000];
        private bool m_lock = false;
        private Z80CPU m_cpu;


        public MemoryScorpion256(String romSetName)
            : base(romSetName)
        {
            InitRam();
        }

        public MemoryScorpion256()
            : this("Scorpion")
        {
        }


        protected virtual void InitRam()
        {
            m_ramPages = new byte[16][];
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }
    }

    public class MemoryScorpion1024 : MemoryScorpion256
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion 1024K"; } }
        public override string Description { get { return "Scorpion 1024K Memory Manager"; } }

        #endregion

        protected override void InitRam()
        {
            m_ramPages = new byte[64][];
            for (int i = 0; i < m_ramPages.Length; i++)
                m_ramPages[i] = new byte[0x4000];
        }

        protected override int GetRamPage()
        {
            int ramPage = CMR0 & 7;
            int sega = (CMR1 & 0x10) >> 4;
            sega |= (CMR1 & 0xC0) >> 5;		// 1024 extension
            ramPage |= sega << 3;
            return ramPage;
        }
    }

    public class MemoryScorpionProfRom256 : MemoryScorpion256
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion PROF-ROM 256K"; } }
        public override string Description { get { return "Scorpion PROF-ROM 256K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRdMemM1(0xFFF0, 0x0100, busProfRomGate);
            bmgr.SubscribeRdMem(0xFFF0, 0x0100, busProfRomGate);
        }

        #endregion

        #region Bus Handlers

        protected virtual void busProfRomGate(ushort addr, ref byte value)
        {
            if (!SYSEN)	// 2, 6, А, Е
                return;
            int newPlane = s_profPlaneMap[(addr & 0x0C) | (m_profPlane & 0x03)];
            if (m_profPlane != newPlane)
            {
                m_profPlane = newPlane;
                UpdateMapping();
            }
        }

        private static readonly int[] s_profPlaneMap = new int[]
		{
			0, 1, 2, 3,
			3, 3, 3, 2,
			2, 2, 0, 1,
			1, 0, 1, 0,
		};

        #endregion


        protected byte[][] m_romPages = new byte[16][];
        private int m_profPlane = 0;


        public MemoryScorpionProfRom256()
            : base("Scorpion-ProfRom")
        {
        }

        public override byte[][] RomPages { get { return m_romPages; } }

        // needs to allow enable DOS when m_profPlane!=0
        public override bool IsRom48
        {
            get { return !SYSEN && !DOSEN && (CMR0 & 0x10) != 0; }
        }

        protected override void InitRam()
        {
            base.InitRam();
            // init prof-rom
            m_romPages = new byte[16][];
            for (int i = 0; i < m_romPages.Length; i++)
                m_romPages[i] = new byte[0x4000];
        }

        public override void ResetState()
        {
            m_profPlane = 0;
            base.ResetState();
        }

        protected override int GetRomPage()
        {
            int romPage = base.GetRomPage();
            romPage |= m_profPlane << 2;
            return romPage;
        }
    }
}
