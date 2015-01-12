using ZXMAK2.Entities;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.Scorpion
{
    public class CovoxScorpion : SoundDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "COVOX SCORPION"; } }
        public override string Description { get { return "COVOX SCORPION \r\nPort #DD - covox"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Sound; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            m_memory = bmgr.FindDevice<IMemoryDevice>();

            bmgr.SubscribeWrIo(0x00FF, 0x00DD, WritePort);
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
