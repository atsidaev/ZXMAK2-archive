using System;
using System.Xml;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.General
{
    public class KempstonJoystick : BusDeviceBase, IJoystickDevice
    {
        #region Fields

        private IMemoryDevice m_memory;
        private string m_hostId = string.Empty;

        #endregion Fields


        #region IBusDevice

        public override string Name { get { return "JOYSTICK KEMPSTON"; } }
        public override string Description { get { return "Kempston Joystick"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice<IMemoryDevice>();
            bmgr.SubscribeRdIo(0xE0, 0x00, ReadPort1F);
        }

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        #endregion IBusDevice


        public IJoystickState JoystickState { get; set; }
        
        public string HostId 
        {
            get { return m_hostId; }
            set { m_hostId = value; OnConfigChanged(); }
        }


        protected virtual void ReadPort1F(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || m_memory.DOSEN)
            {
                return;
            }
            iorqge = false;

            value = 0x00;
            if (JoystickState.IsRight) value |= 0x01;
            if (JoystickState.IsLeft) value |= 0x02;
            if (JoystickState.IsDown) value |= 0x04;
            if (JoystickState.IsUp) value |= 0x08;
            if (JoystickState.IsFire) value |= 0x10;
        }

        protected override void OnConfigSave(XmlNode itemNode)
        {
            base.OnConfigSave(itemNode);
            Utils.SetXmlAttribute(itemNode, "hostId", HostId);
        }

        protected override void OnConfigLoad(XmlNode itemNode)
        {
            base.OnConfigLoad(itemNode);
            HostId = Utils.GetXmlAttributeAsString(itemNode, "hostId", HostId);
        }
    }
}
