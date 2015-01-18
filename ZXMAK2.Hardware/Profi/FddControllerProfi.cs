using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.General;
using ZXMAK2.Hardware.Circuits.Fdd;


namespace ZXMAK2.Hardware.Profi
{
    public class FddControllerProfi : FddController
    {
        public FddControllerProfi()
        {
            Name = "FDD PROFI";
            Description = "FDD controller WD1793 with PROFI port activation";
        }

        
        #region Private

        protected virtual bool IsNormalMode
        {
            get 
            {
                var cpm = (m_memory.CMR1 & 0x20) != 0;
                var rom48 = (m_memory.CMR0 & 0x10) != 0;
                return ((cpm && !rom48) || (!cpm && m_memory.SYSEN)); 
            }
        }

        protected virtual bool IsExtendedMode
        {
            get 
            {
                var cpm = (m_memory.CMR1 & 0x20) != 0;
                var rom48 = (m_memory.CMR0 & 0x10) != 0;
                return cpm && rom48; 
            }
        }

        public override bool IsActive
        {
            get { return m_memory.DOSEN || IsNormalMode; }
        }

        protected override void OnSubscribeIo(IBusManager bmgr)
        {
            bmgr.SubscribeWrIo(0x9F, 0x1F & 0x9F, BusWriteFdc);
            bmgr.SubscribeRdIo(0x9F, 0x1F & 0x9F, BusReadFdc);
            bmgr.SubscribeWrIo(0x9F, 0xFF & 0x9F, BusWriteSys);
            bmgr.SubscribeRdIo(0x9F, 0xFF & 0x9F, BusReadSys);

            // ExtendedMode:
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
            if (!iorqge || !IsExtendedMode)
            {
                return;
            }
            iorqge = false;

            var fdcReg = (addr & 0x60) >> 5;
            m_wd.Write(m_cpu.Tact, (WD93REG)fdcReg, value);
        }

        protected void BusReadFdcEx(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !IsExtendedMode)
            {
                return;
            }
            iorqge = false;

            var fdcReg = (addr & 0x60) >> 5;
            value = m_wd.Read(m_cpu.Tact, (WD93REG)fdcReg);
        }

        protected void BusWriteSysEx(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge || !IsExtendedMode)
            {
                return;
            }
            iorqge = false;

            m_wd.Write(m_cpu.Tact, WD93REG.SYS, value);
        }

        protected void BusReadSysEx(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !IsExtendedMode)
            {
                return;
            }
            iorqge = false;

            value = m_wd.Read(m_cpu.Tact, WD93REG.SYS);
        }

        #endregion
    }
}
