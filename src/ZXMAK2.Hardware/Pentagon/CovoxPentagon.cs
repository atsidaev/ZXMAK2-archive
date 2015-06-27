using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.Pentagon
{
    public class CovoxPentagon : SoundDeviceBase
    {
        public CovoxPentagon()
        {
            Category = BusDeviceCategory.Sound;
            Name = "COVOX PENTAGON";
            Description = "COVOX PENTAGON \r\nPort #FB - covox";
        }
        

        #region IBusDevice

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            //bmgr.SubscribeWrIo(0x0004, 0x00FB & 0x0004, WritePort);
            bmgr.Events.SubscribeWrIo(0x00FF, 0x00FB, WritePort);
        }

        #endregion

        private int m_mult = 0;


        private void WritePort(ushort addr, byte value, ref bool handled)
        {
            if (handled)
                return;
            handled = true;

            var dac = (ushort)(value * m_mult);
            UpdateDac(dac, dac);
        }


        protected override void OnVolumeChanged(int oldVolume, int newVolume)
        {
            m_mult = (ushort.MaxValue * newVolume) / (100 * 0xFF);
        }
    }
}
