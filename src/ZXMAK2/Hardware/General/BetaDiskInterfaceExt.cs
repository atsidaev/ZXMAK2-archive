using System;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Hardware.General
{
    public class BetaDiskInterfaceExt : BetaDiskInterface
    {
        #region IBusDevice

        public override string Name { get { return "BDI EXT"; } }
        public override string Description { get { return "Beta Disk Interface + WD93 port activator for shadow ROM"; } }

        #endregion

        #region BetaDiskInterface

        protected override void BusWritePortFdc(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if (DOSEN || m_memory.SYSEN)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                SetReg((WD93REG)fdcReg, value);
            }
        }

        protected override void BusReadPortFdc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if (DOSEN || m_memory.SYSEN)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                value = GetReg((WD93REG)fdcReg);
            }
        }

        protected override void BusWritePortSys(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if (DOSEN || m_memory.SYSEN)
            {
                iorqge = false;
                SetReg(WD93REG.SYS, value);
            }
        }

        protected override void BusReadPortSys(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if (DOSEN || m_memory.SYSEN)
            {
                iorqge = false;
                value = GetReg(WD93REG.SYS);
            }
        }

        protected override void BusNmi()
        {
        }

        #endregion
    }
}
