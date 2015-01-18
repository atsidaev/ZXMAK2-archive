using System;
using ZXMAK2.Hardware.General;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Hardware.ZXBYTE
{
    public class BeeperByte : BeeperDevice
    {
        public BeeperByte()
        {
            Name = "BEEPER BYTE";
            Description = "BYTE Beeper\r\nPort: #FE\r\nMask: #35";
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
            if (!m_memory.DOSEN && (addr & 0x35) == (0xFE & 0x35))
            {
                base.WritePortFE(addr, value, ref iorqge);
            }
        }

        #endregion

        private IMemoryDevice m_memory;
    }
}
