using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Hardware.General;
using ZXMAK2.Hardware.IC;

namespace ZXMAK2.Hardware.Quorum
{
    public class FddControllerQuorum : FddController
    {
        #region Fields

        private static readonly int[] s_drvDecode = new int[] { 3, 0, 1, 3 };

        #endregion

        
        #region IBusDevice

        public override string Name { get { return "FDD QUORUM"; } }
        public override string Description { get { return "FDD controller WD1793 with QUORUM port activation"; } }

        #endregion

        
        #region BetaDiskInterface

        protected override void OnSubscribeIo(IBusManager bmgr)
        {
            // mask - #9F
            // #80 - CMD
            // #81 - TRK
            // #82 - SEC
            // #83 - DAT
            // #85 - SYS
            bmgr.SubscribeWrIo(0x9C, 0x80 & 0x9C, BusWriteFdc);
            bmgr.SubscribeRdIo(0x9C, 0x80 & 0x9C, BusReadFdc);
            bmgr.SubscribeWrIo(0x9F, 0x85 & 0x9F, BusWriteSys);
            bmgr.SubscribeRdIo(0x9F, 0x85 & 0x9F, BusReadSys);
        }

        public override bool IsActive
        {
            get 
            {
                return true;//m_memory.CMR1 & 0x80; // Q_TR_DOS
            }
        }

        protected override void BusWriteFdc(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            if (IsActive)
            {
                iorqge = false;
                int fdcReg = addr & 0x03;
                m_wd.Write(m_cpu.Tact, (WD93REG)fdcReg, value);
            }
        }

        protected override void BusReadFdc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            if (IsActive)
            {
                iorqge = false;
                int fdcReg = addr & 0x03;
                value = m_wd.Read(m_cpu.Tact, (WD93REG)fdcReg);
            }
        }

        protected override void BusWriteSys(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            if (IsActive)
            {
                iorqge = false;
                int drv = s_drvDecode[value & 3];
                drv = ((value & ~3) ^ 0x10) | drv;
                m_wd.Write(m_cpu.Tact, WD93REG.SYS, (byte)drv);
            }
        }

        protected override void BusReadSys(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            if (IsActive)
            {
                iorqge = false;
                value = m_wd.Read(m_cpu.Tact, WD93REG.SYS);
            }
        }

        #endregion
    }
}
