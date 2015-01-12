using System;
using ZXMAK2.Hardware.General;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.Scorpion
{
    public class BeeperScorpion : BeeperDevice
    {
        #region IBusDevice

        public override string Name { get { return "BEEPER SCORPION"; } }
        public override string Description { get { return "Scorpion ZS Beeper\r\nPort: #FE\r\nMask: #23"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            m_memory = bmgr.FindDevice<IMemoryDevice>();
        }

        #endregion

        #region BeeperDevice

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            if (!m_memory.DOSEN && (addr & 0x23) == (0xFE & 0x23))
            {
                base.WritePortFE(addr, value, ref iorqge);
            }
        }

        #endregion

        private IMemoryDevice m_memory;
    }
}
