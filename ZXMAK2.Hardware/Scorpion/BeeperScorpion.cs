using System;
using ZXMAK2.Hardware.General;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.Scorpion
{
    public class BeeperScorpion : BeeperDevice
    {
        #region Fields

        private IMemoryDevice m_memory;

        #endregion Fields


        public BeeperScorpion()
        {
            Name = "BEEPER SCORPION";
            Description = "Scorpion ZS Beeper\r\nPort: #FE\r\nMask: #23";
        }

        
        #region IBusDevice

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
    }
}
