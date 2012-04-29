using System;

using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Engine.Devices.Memory
{
    public class MemoryScorpion256 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion 256K"; } }
        public override string Description { get { return "Scorpion 256K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeWRIO(0xD027, 0x5025, busWritePort7FFD); // +
            bmgr.SubscribeWRIO(0xD027, 0x1025, busWritePort1FFD); // +
            bmgr.SubscribeNMIACK(busNmi);
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
            int romPage = (CMR0 & 0x10) >> 4;
			int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;
            
            bool norom = (CMR1 & 0x01) != 0;

            if (DOSEN)      // trdos or 48/128
                romPage = 2;
            if (SYSEN)
                romPage = 3;

            int sega = (CMR1 & 0x10) >> 4;
            if (RamPages.Length == 64)               //Scorp1024?
                sega |= (CMR1 & 0xC0) >> 5; 

            ramPage |= sega << 3;

			m_ula.SetPageMapping(videoPage, norom ? 0:-1, 5, 2, ramPage);
			MapRead0000 = norom ? RamPages[0] : RomPages[romPage];
            MapRead4000 = RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = norom ? RamPages[0] : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }

        protected override void LoadRom()
        {
            base.LoadRom();
			LoadRomPack("Scorpion");
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

        private void busReset()
        {
            CMR1 = 0;
            CMR0 = 0;
        }

        private void busNmi()
        {
            // enable shadow rom
            CMR1 |= 0x02;
        }

        #endregion


        protected byte[][] m_ramPages = new byte[16][];
        private byte[] m_trashPage = new byte[0x4000];
        private bool m_lock = false;


        public MemoryScorpion256()
        {
            InitRam();
        }



        protected virtual void InitRam()
        {
            m_ramPages = new byte[16][];
            for (int i = 0; i < m_ramPages.Length; i++)
                m_ramPages[i] = new byte[0x4000];
        }
    }
}
