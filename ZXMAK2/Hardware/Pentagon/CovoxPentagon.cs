using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.Pentagon
{
    public class CovoxPentagon : SoundDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "COVOX PENTAGON"; } }
        public override string Description { get { return "COVOX PENTAGON \r\nPort #FB - covox"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Sound; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            m_memory = bmgr.FindDevice<IMemoryDevice>();

            //bmgr.SubscribeWrIo(0x0004, 0x00FB & 0x0004, WritePort);
            bmgr.SubscribeWrIo(0x00FF, 0x00FB, WritePort);
        }

        #endregion

        private IMemoryDevice m_memory;
        private int m_mult = 0;


        private void WritePort(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;

            var dac = (ushort)(value * m_mult);
            UpdateDac(dac, dac);
        }


        protected override void OnVolumeChanged(int oldVolume, int newVolume)
        {
            m_mult = (0xFFFF * newVolume) / (100 * 0xFF);
        }
    }
}
