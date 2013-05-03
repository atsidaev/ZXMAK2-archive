﻿using System;
using System.IO;

using ZXMAK2.Interfaces;


namespace ZXMAK2.Hardware.Pentagon
{
    public class MemoryPentagon128 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "Pentagon 128K"; } }
        public override string Description { get { return "Pentagon 128K Memory Module"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeWRIO(0x8002, 0x0000, writePort7FFD);
            bmgr.SubscribeRESET(busReset);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

        public override bool IsMap48 { get { return m_lock; } }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            int ramPage = CMR0 & 7;
            int romPage = (CMR0 & 0x10) >> 4;
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
                romPage = 2;

            m_ula.SetPageMapping(videoPage, -1, 5, 2, ramPage);
            MapRead0000 = RomPages[romPage];
            MapRead4000 = RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;

            Map48[0] = romPage;
            Map48[1] = 5;
            Map48[2] = 2;
            Map48[3] = ramPage;
        }

        #endregion


        private byte[][] m_ramPages = new byte[8][];
        private byte[] m_trashPage = new byte[0x4000];
        private bool m_lock = false;

        public MemoryPentagon128()
            : base("Pentagon")
        {
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }

        #region Bus Handlers

        private void writePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!m_lock)
                CMR0 = value;
        }

        private void busReset()
        {
            CMR0 = 0;
        }

        #endregion
    }
}
