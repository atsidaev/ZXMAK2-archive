using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;
using ZXMAK2.Hardware.General;
using ZXMAK2.Hardware.IC;

namespace ZXMAK2.Hardware.Profi
{
    public class FddControllerProfi : FddController
    {
        #region IBusDevice

        public override string Name { get { return "FDD PROFI"; } }
        public override string Description { get { return "FDD controller WD1793 with PROFI port activation"; } }

        #endregion

        
        #region Private

        public override bool IsActive
        {
            get 
            {
                bool cpm = (m_memory.CMR1 & 0x20) != 0;
                bool rom48 = (m_memory.CMR0 & 0x10) != 0;
                bool csNormal = ((cpm && !rom48) || (!cpm && m_memory.SYSEN));
                //bool csExtend = cpm && rom48;
                return base.IsActive || csNormal; 
            }
        }

        protected override void OnSubscribeIo(IBusManager bmgr)
        {
            base.OnSubscribeIo(bmgr);
            // #83 - CMD
            // #A3 - TRK
            // #C3 - SEC
            // #E3 - DAT
            // #3F - SYS
            bmgr.SubscribeWrIo(0x9F, 0x83 & 0x9F, BusWriteFdcEx);
            bmgr.SubscribeRdIo(0x9F, 0x83 & 0x9F, BusReadFdcEx);
            bmgr.SubscribeWrIo(0xFF, 0x3F, BusWriteSysEx);
            bmgr.SubscribeRdIo(0xFF, 0x3F, BusReadSysEx);
        }

        protected void BusWriteFdcEx(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csExtend = cpm && rom48;
            if (csExtend)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                m_wd.Write(m_cpu.Tact, (WD93REG)fdcReg, value);
            }
        }

        protected void BusReadFdcEx(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csExtend = cpm && rom48;
            if (csExtend)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                value = m_wd.Read(m_cpu.Tact, (WD93REG)fdcReg);
            }
        }

        protected void BusWriteSysEx(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csExtend = cpm && rom48;
            if (csExtend)
            {
                iorqge = false;
                m_wd.Write(m_cpu.Tact, WD93REG.SYS, value);
            }
        }

        protected void BusReadSysEx(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool csExtend = cpm && rom48;
            if (csExtend)
            {
                iorqge = false;
                value = m_wd.Read(m_cpu.Tact, WD93REG.SYS);
            }
        }

        #endregion
    }
}
