using System;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Hardware.Scorpion
{
    public class MemoryScorpionProfRom256 : MemoryScorpion256
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion PROF-ROM 256K"; } }
        public override string Description { get { return "Scorpion PROF-ROM 256K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeRdMemM1(0xFFF0, 0x0100, BusProfRomGate);
            bmgr.SubscribeRdMem(0xFFF0, 0x0100, BusProfRomGate);

            // Subscribe before MemoryBase.BusInit 
            // to handle memory switches before read
            base.BusInit(bmgr);
        }

        #endregion

        #region Bus Handlers

        protected virtual void BusProfRomGate(ushort addr, ref byte value)
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
            InitRom();
        }

        public override byte[][] RomPages { get { return m_romPages; } }

        // needs to allow enable DOS when m_profPlane!=0
        public override bool IsRom48
        {
            get { return !SYSEN && !DOSEN && (CMR0 & 0x10) != 0; }
        }

        protected virtual void InitRom()
        {
            // init prof-rom
            m_romPages = new byte[16][];
            for (int i = 0; i < m_romPages.Length; i++)
            {
                m_romPages[i] = new byte[0x4000];
            }
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

    public class MemoryScorpionProfRom1024 : MemoryScorpionProfRom256
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion PROF-ROM 1024K"; } }
        public override string Description { get { return "Scorpion PROF-ROM 1024K Memory Manager"; } }

        #endregion

        protected override void InitRam()
        {
            m_ramPages = new byte[64][];
            for (int i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
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
}
