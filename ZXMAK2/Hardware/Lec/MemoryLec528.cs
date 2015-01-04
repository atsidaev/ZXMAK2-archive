using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;
using ZXMAK2.Entities;

namespace ZXMAK2.Hardware.Lec
{
    public class MemoryLec48528 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "LEC 48/528K (beta)"; } }
        public override string Description { get { return "LEC Memory Extension by Jiri Lamac"; } }

        public override void BusInit(Interfaces.IBusManager bmgr)
        {
            bmgr.SubscribeWrIo(0x0002, 0x00FD & 0x0002, BusWriteCmr1);
            bmgr.SubscribeReset(BusReset);

            // Subscribe before MemoryBase.BusInit 
            // to handle memory switches before read
            base.BusInit(bmgr);
        }

        #endregion

        #region MemoryBase

        public override bool IsMap48 { get { return false; } }

        protected override void UpdateMapping()
        {
            bool allram = (CMR1 & 0x80) != 0;
            int ramPageLec = ((CMR1 >> 4) & 7) | (CMR1 & 8);
            int romPage = GetRomIndex(RomId.ROM_SOS);
            int videoPage = 32;

            if (DOSEN)      // trdos or 48/128
                romPage = GetRomIndex(RomId.ROM_DOS);

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

        public override int GetRomIndex(RomId romId)
        {
            switch (romId)
            {
                case RomId.ROM_128: return 0;
                case RomId.ROM_SOS: return 1;
                case RomId.ROM_DOS: return 2;
                case RomId.ROM_SYS: return 3;
            }
            Logger.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
        }

        #endregion

        #region Bus Handlers

        protected virtual void BusWriteCmr1(ushort addr, byte value, ref bool iorqge)
        {
            CMR1 = value;
        }

        protected virtual void BusReset()
        {
            CMR1 = 0;
        }

        #endregion

        public MemoryLec48528()
            : base("LEC", 4, 32 + 1)
        {
        }
    }
}
