using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;

namespace ZXMAK2.Hardware.General
{
    public class KempstonJoystick : BusDeviceBase, IJoystickDevice
    {
        #region Fields

        private IMemoryDevice m_memory;

        #endregion Fields


        #region IBusDevice

        public override string Name { get { return "JOYSTICK KEMPSTON"; } }
        public override string Description { get { return "Kempston Joystick"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice<IMemoryDevice>();
            bmgr.SubscribeRdIo(0x00e0, 0x0000, ReadPort1F);
        }

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        #endregion IBusDevice


        public IJoystickState JoystickState { get; set; }


        protected virtual void ReadPort1F(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || m_memory.DOSEN)
            {
                return;
            }
            iorqge = false;

            value = 0x00;
            if (JoystickState.IsLeft) value |= 0x01;
            if (JoystickState.IsRight) value |= 0x02;
            if (JoystickState.IsUp) value |= 0x04;
            if (JoystickState.IsDown) value |= 0x08;
            if (JoystickState.IsFire) value |= 0x10;
        }
    }
}
