using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Hardware.Lec
{
    public class MemoryLec48528 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "LEC 48/528K (beta)"; } }
        public override string Description { get { return "LEC Memory Extension by Jiri Lamac"; } }

        public override void BusInit(Interfaces.IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeWRIO(0x0002, 0x00FD & 0x0002, busWriteCMR1);
            bmgr.SubscribeRESET(busReset);
        }

        #endregion

        #region MemoryBase

        public override bool IsMap48 { get { return false; } }

        public override byte[][] RamPages { get { return m_ramPages; } }

        protected override void UpdateMapping()
        {
            bool allram = (CMR1 & 0x80) != 0;
            int ramPageLec = ((CMR1 >> 4) & 7) | (CMR1 & 8);
            int romPage = 1;
            int videoPage = 32;

            if (DOSEN)      // trdos or 48/128
                romPage = 2;

            m_ula.SetPageMapping(
                videoPage,
                allram ? ramPageLec * 2 : -1,
                allram ? ramPageLec * 2 + 1 : 32,
                15 * 2,
                15 * 2 + 1);
            MapRead0000 = allram ? RamPages[ramPageLec * 2] : RomPages[romPage];
            MapRead4000 = allram ? RamPages[ramPageLec * 2 + 1] : RamPages[32];
            MapRead8000 = RamPages[15 * 2];
            MapReadC000 = RamPages[15 * 2 + 1];

            MapWrite0000 = allram ? MapRead0000 : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }

        #endregion

        #region Bus Handlers

        private void busWriteCMR1(ushort addr, byte value, ref bool iorqge)
        {
            CMR1 = value;
        }

        private void busReset()
        {
            CMR1 = 0;
        }

        #endregion

        private byte[][] m_ramPages = new byte[32 + 1][];
        private byte[] m_trashPage = new byte[0x4000];

        public MemoryLec48528()
            : base("LEC")
        {
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }
    }
}
