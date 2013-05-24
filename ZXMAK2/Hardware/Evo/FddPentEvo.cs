using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Hardware.General;
using ZXMAK2.Hardware.IC;


namespace ZXMAK2.Hardware.Evo
{
    public class FddPentEvo : FddController
    {
        private byte m_p2F, m_p4F, m_p6F, m_p8F;

        
        public override string Name
        {
            get { return "FDD PentEvo"; }
        }

        protected override void OnSubscribeIo(IBusManager bmgr)
        {
            bmgr.SubscribeWrIo(0x009F, 0x001F, BusWriteFdc);
            bmgr.SubscribeRdIo(0x009F, 0x001F, BusReadFdc);
            bmgr.SubscribeWrIo(0x00FF, 0x00FF, BusWriteSys);
            bmgr.SubscribeRdIo(0x00FF, 0x00FF, BusReadSys);

            bmgr.SubscribeWrIo(0x00FF, 0x002F, WritePortEmu);
            bmgr.SubscribeWrIo(0x00FF, 0x004F, WritePortEmu);
            bmgr.SubscribeWrIo(0x00FF, 0x006F, WritePortEmu);
            bmgr.SubscribeWrIo(0x00FF, 0x008F, WritePortEmu);
            bmgr.SubscribeRdIo(0x00FF, 0x002F, ReadPortEmu);
            bmgr.SubscribeRdIo(0x00FF, 0x004F, ReadPortEmu);
            bmgr.SubscribeRdIo(0x00FF, 0x006F, ReadPortEmu);
            bmgr.SubscribeRdIo(0x00FF, 0x008F, ReadPortEmu);
        }

        protected virtual void WritePortEmu(ushort addr, byte val, ref bool iorqge)
        {
            if (IsActive)
            {
                switch ((addr & 0x00FF) >> 4)
                {
                    case 0x02:
                        m_p2F = val;
                        break;
                    case 0x04:
                        m_p4F = val;
                        break;
                    case 0x06:
                        m_p6F = val;
                        break;
                    case 0x08:
                        m_p8F = val;
                        break;
                }
            }
        }

        protected virtual void ReadPortEmu(ushort addr, ref byte val, ref bool iorqge)
        {
            if (IsActive)
            {
                switch ((addr & 0x00FF) >> 4)
                {
                    case 0x02:
                        val = m_p2F;
                        break;
                    case 0x04:
                        val = m_p4F;
                        break;
                    case 0x06:
                        val = m_p6F;
                        break;
                    case 0x08:
                        val = m_p8F;
                        break;
                }
            }
        }
    }
}
