using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Hardware.General;
using ZXMAK2.Hardware.IC;


namespace ZXMAK2.Hardware.Evo
{
    public class FddPentEvo : FddController
    {
        private MemoryPentEvo m_memoryPentEvo;
        
        public override string Name
        {
            get { return "FDD PentEvo"; }
        }

        public override void BusInit(IBusManager bmgr)
        {
            m_memoryPentEvo = bmgr.FindDevice<MemoryPentEvo>();
            base.BusInit(bmgr);
        }

        protected override void OnSubscribeIo(IBusManager bmgr)
        {
            bmgr.SubscribeWrIo(0x009F, 0x001F, BusWriteFdc);
            bmgr.SubscribeRdIo(0x009F, 0x001F, BusReadFdc);
            bmgr.SubscribeWrIo(0x00FF, 0x00FF, BusWriteSys);
            bmgr.SubscribeRdIo(0x00FF, 0x00FF, BusReadSys);
        }

        public override bool IsActive
        {
            get { return base.IsActive || (m_memoryPentEvo != null && m_memoryPentEvo.SHADOW); }
        }
    }
}
