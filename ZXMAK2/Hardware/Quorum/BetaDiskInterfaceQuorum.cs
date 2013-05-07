using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Hardware.General;

namespace ZXMAK2.Hardware.Quorum
{
    public class BetaDiskInterfaceQuorum : BetaDiskInterface
    {
        #region IBusDevice

        public override string Name { get { return "BDI QUORUM"; } }
        public override string Description { get { return "Beta Disk Interface + QUORUM mod"; } }

        #endregion

        #region BetaDiskInterface

        protected override void BusSubscribeWD93IO(IBusManager bmgr)
        {
            // mask - #9F
            // #80 - CMD
            // #81 - TRK
            // #82 - SEC
            // #83 - DAT
            // #85 - SYS
            bmgr.SubscribeWRIO(0x9C, 0x80 & 0x9C, BusWritePortFdc);
            bmgr.SubscribeRDIO(0x9C, 0x80 & 0x9C, BusReadPortFdc);
            bmgr.SubscribeWRIO(0x9F, 0x85 & 0x9F, BusWritePortSys);
            bmgr.SubscribeRDIO(0x9F, 0x85 & 0x9F, BusReadPortSys);
        }

        protected override void BusWritePortFdc(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool dos = true;//m_memory.CMR1 & 0x80; // Q_TR_DOS
            if (dos)
            {
                iorqge = false;
                int fdcReg = addr & 0x03;
                SetReg((WD93REG)fdcReg, value);
            }
        }

        protected override void BusReadPortFdc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool dos = true;//m_memory.CMR1 & 0x80; // Q_TR_DOS
            if (dos)
            {
                iorqge = false;
                int fdcReg = addr & 0x03;
                value = GetReg((WD93REG)fdcReg);
            }
        }

        protected override void BusWritePortSys(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool dos = true;//m_memory.CMR1 & 0x80; // Q_TR_DOS
            if (dos)
            {
                iorqge = false;
                int drv = s_drvDecode[value & 3];
                drv = ((value & ~3) ^ 0x10) | drv;
                SetReg(WD93REG.SYS, (byte)drv);
            }
        }

        protected override void BusReadPortSys(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            bool dos = true;//m_memory.CMR1 & 0x80; // Q_TR_DOS
            if (dos)
            {
                iorqge = false;
                value = GetReg(WD93REG.SYS);
            }
        }

        protected override void BusNmiRq(BusCancelArgs e)
        {
        }

        protected override void BusNmiAck()
        {
        }

        #endregion

        private static readonly int[] s_drvDecode = new int[] { 3, 0, 1, 3 };
    }
}
