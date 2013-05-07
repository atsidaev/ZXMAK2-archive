using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;
using ZXMAK2.Hardware.General;

namespace ZXMAK2.Hardware.Profi
{
    public class BetaDiskInterfaceProfi : BetaDiskInterface
    {
        #region IBusDevice

        public override string Name { get { return "BDI PROFI"; } }
        public override string Description { get { return "Beta Disk Interface + PROFI mod"; } }

        #endregion

        #region BetaDiskInterface

        protected override void BusSubscribeWD93IO(IBusManager bmgr)
        {
            base.BusSubscribeWD93IO(bmgr);
            // #83 - CMD
            // #A3 - TRK
            // #C3 - SEC
            // #E3 - DAT
            // #3F - SYS
            bmgr.SubscribeWrIo(0x9F, 0x83 & 0x9F, BusWritePortFdcEx);
            bmgr.SubscribeRdIo(0x9F, 0x83 & 0x9F, BusReadPortFdcEx);
            bmgr.SubscribeWrIo(0xFF, 0x3F, BusWritePortSysEx);
            bmgr.SubscribeRdIo(0xFF, 0x3F, BusReadPortSysEx);
        }

        protected override void BusWritePortFdc(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csNormal = ((cpm && !rom48) || (!cpm && m_memory.SYSEN));
            bool csExtend = cpm && rom48;
            if (DOSEN || csNormal)
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
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool csNormal = ((cpm && (m_memory.CMR0 & 0x10) == 0) || (!cpm && m_memory.SYSEN));
            if (DOSEN || csNormal)
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
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool csNormal = ((cpm && (m_memory.CMR0 & 0x10) == 0) || (!cpm && m_memory.SYSEN));
            if (DOSEN || csNormal)
            {
                iorqge = false;
                SetReg(WD93REG.SYS, value);
            }
        }

        protected override void BusReadPortSys(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool csNormal = ((cpm && (m_memory.CMR0 & 0x10) == 0) || (!cpm && m_memory.SYSEN));
            if (DOSEN || csNormal)
            {
                iorqge = false;
                value = GetReg(WD93REG.SYS);
            }
        }

        #endregion

        #region Bus Handlers

        protected void BusWritePortFdcEx(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csExtend = cpm && rom48;
            if (csExtend)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                SetReg((WD93REG)fdcReg, value);
            }
        }

        protected void BusReadPortFdcEx(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csExtend = cpm && rom48;
            if (csExtend)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                value = GetReg((WD93REG)fdcReg);
            }
        }

        protected void BusWritePortSysEx(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csExtend = cpm && rom48;
            if (csExtend)
            {
                iorqge = false;
                SetReg(WD93REG.SYS, value);
            }
        }

        protected void BusReadPortSysEx(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csExtend = cpm && rom48;
            if (csExtend)
            {
                iorqge = false;
                value = GetReg(WD93REG.SYS);
            }
        }

        #endregion
    }
}
