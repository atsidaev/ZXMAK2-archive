using System;

using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Engine.Z80;


namespace ZXMAK2.Hardware.Profi
{
    public class MemoryProfi1024 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "PROFI+ 1024K"; } }
        public override string Description { get { return "PROFI+ 1024K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            m_cpu = bmgr.CPU;
            m_ulaProfi = bmgr.FindDevice<UlaProfi3XX>();

            bmgr.SubscribeWrIo(0x8002, 0x0000, writePort7FFD);
            bmgr.SubscribeWrIo(0xFFFF, 0xDFFD, writePortDFFD);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, readRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, readRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, readRamM1);

            bmgr.SubscribeReset(busReset);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

        public override bool IsMap48 { get { return false; } }

        protected override void UpdateMapping()
        {
            m_lock = ((CMR0 & 0x20) != 0);
            int ramPage = CMR0 & 7;
            int romPage = (CMR0 & 0x10) >> 4;
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
                romPage = 2;
            if (SYSEN)
                romPage = 3;

            int sega = CMR1 & 7;
            bool norom = (CMR1 & 0x10) != 0;
            bool sco = (CMR1 & 0x08) != 0;   // selectors RAM gates
            bool scr = (CMR1 & 0x40) != 0;   // !??CMR0.D3=1??!
            //bool cpm = (CMR1 & 0x20) != 0;
            bool ds80 = (CMR1 & 0x80) != 0;

            if (norom)
                m_lock = false;

            ramPage |= sega << 3;

            if (m_ulaProfi != null)
                m_ulaProfi.SetPageMappingProfi(videoPage, norom ? 0 : -1, sco ? ramPage : 5, scr ? 6 : 2, sco ? 7 : ramPage, ds80);
            else
                m_ula.SetPageMapping(videoPage, norom ? 0 : -1, sco ? ramPage : 5, scr ? 6 : 2, sco ? 7 : ramPage);
            MapRead0000 = norom ? RamPages[0] : RomPages[romPage];
            MapRead4000 = sco ? RamPages[ramPage] : RamPages[5];
            MapRead8000 = scr ? RamPages[6] : RamPages[2];
            MapReadC000 = sco ? RamPages[7] : RamPages[ramPage];

            MapWrite0000 = norom ? RamPages[0] : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }

        #endregion

        #region Bus Handlers

        private void writePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            if (!m_lock)
                CMR0 = value;
        }

        private void writePortDFFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            CMR1 = value;
        }

        private void readRamM1(ushort addr, ref byte value)
        {
            if (SYSEN)
                SYSEN = false;
        }

        private void busReset()
        {
            CMR1 = 0;
            CMR0 = 0;
            SYSEN = true;
        }

        #endregion


        private byte[][] m_ramPages = new byte[64][];
        private byte[] m_trashPage = new byte[0x4000];
        private Z80CPU m_cpu;
        private UlaProfi3XX m_ulaProfi;
        private bool m_lock = false;


        public MemoryProfi1024(String romSetName)
            : base(romSetName)
        {
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }

        public MemoryProfi1024()
            : this("Profi")
        {
        }
    }
}
