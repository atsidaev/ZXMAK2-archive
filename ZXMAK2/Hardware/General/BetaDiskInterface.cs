using System;
using System.Xml;
using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.Circuits;


namespace ZXMAK2.Hardware.General
{
    public class BetaDiskInterface : FddController
    {
        #region IBusDevice

        public override string Name { get { return "BDI"; } }
        public override string Description { get { return "Beta Disk Interface\r\nDOSEN enabler + WD1793"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Disk; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);

            bmgr.SubscribeRdMemM1(0xFF00, 0x3D00, BusReadMem3D00_M1);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, BusReadMemRam);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, BusReadMemRam);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, BusReadMemRam);

            bmgr.SubscribeReset(BusReset);
            bmgr.SubscribeNmiRq(BusNmiRq);
            bmgr.SubscribeNmiAck(BusNmiAck);
        }

        #endregion


        #region Private

        protected virtual void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if (m_memory.IsRom48)
            {
                m_memory.DOSEN = true;
            }
        }

        protected virtual void BusReadMemRam(ushort addr, ref byte value)
        {
            if (m_memory.DOSEN)
            {
                m_memory.DOSEN = false;
            }
        }

        protected virtual void BusReset()
        {
            m_memory.DOSEN = false;
        }

        protected virtual void BusNmiRq(BusCancelArgs e)
        {
            e.Cancel = !m_memory.IsRom48;
        }

        protected virtual void BusNmiAck()
        {
            m_memory.DOSEN = true;
        }

        #endregion
    }
}
