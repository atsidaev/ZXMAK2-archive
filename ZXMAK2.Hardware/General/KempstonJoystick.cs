using System;
using System.Xml;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.General
{
    public class KempstonJoystick : BusDeviceBase, IJoystickDevice
    {
        #region Fields

        private IMemoryDevice m_memory;
        private string m_hostId = string.Empty;

        #endregion Fields


        public KempstonJoystick()
        {
            Category = BusDeviceCategory.Other;
            Name = "JOYSTICK KEMPSTON";
            Description = "Kempston Joystick (port #1F, mask #E0)";
        }


        #region IBusDevice

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice<IMemoryDevice>();
            bmgr.Events.SubscribeRdIo(0xE0, 0x00, ReadPort1F);
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


        protected virtual void ReadPort1F(ushort addr, ref byte value, ref bool handled)
        {
            if (handled || m_memory.DOSEN)
                return;
            handled = true;

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
