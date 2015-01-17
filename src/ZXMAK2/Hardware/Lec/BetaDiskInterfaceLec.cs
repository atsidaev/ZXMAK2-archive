using ZXMAK2.Hardware.General;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.Lec
{
    public class BetaDiskInterfaceLec : BetaDiskInterface
    {
        #region Fields

        private bool m_betaHack = false;

        #endregion


        #region IBusDevice

        public override string Name { get { return "BDI LEC (beta)"; } }
        public override string Description { get { return "Beta Disk Interface + LEC extension hack"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeWrIo(0x8002, 0x00FD & 0x8002, BusWriteBdiHack);
        }

        #endregion

        
        #region Private

        protected void BusWriteBdiHack(ushort addr, byte value, ref bool iorqge)
        {
            m_betaHack = (value & 0x10) != 0;
        }

        protected override void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if (!m_betaHack && m_memory.IsRom48)
            {
                DOSEN = true;
            }
        }

        protected override void BusReadMemRam(ushort addr, ref byte value)
        {
            if (!m_betaHack && m_memory.DOSEN)
            {
                DOSEN = false;
            }
        }

        protected override void BusReset()
        {
            m_betaHack = false;
            base.BusReset();
        }

        #endregion
    }
}
