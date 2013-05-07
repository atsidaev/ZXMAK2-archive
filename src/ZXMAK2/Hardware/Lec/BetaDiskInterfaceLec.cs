using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.General;

namespace ZXMAK2.Hardware.Lec
{
    public class BetaDiskInterfaceLec : BetaDiskInterface
    {
        #region IBusDevice

        public override string Name { get { return "BDI LEC (beta)"; } }
        public override string Description { get { return "Beta Disk Interface + LEC extension hack"; } }

        public override void BusInit(Interfaces.IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeWrIo(0x8002, 0x00FD & 0x8002, busWriteBdiHack);
        }

        #endregion

        #region BetaDiskInterface

        protected override void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if (!m_betaHack && !DOSEN && m_memory.IsRom48)
                DOSEN = true;
        }

        protected override void BusReadMemRam(ushort addr, ref byte value)
        {
            if (!m_betaHack && DOSEN)
                DOSEN = false;
        }

        protected override void BusReset()
        {
            m_betaHack = false;
            base.BusReset();
        }

        #endregion

        #region Bus Handlers

        private void busWriteBdiHack(ushort addr, byte value, ref bool iorqge)
        {
            m_betaHack = (value & 0x10) != 0;
        }
        
        #endregion

        private bool m_betaHack = false;

        public bool IsBetaHack { get { return m_betaHack; } }
    }
}
