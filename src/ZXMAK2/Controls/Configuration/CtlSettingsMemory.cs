using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;

namespace ZXMAK2.Controls.Configuration
{
    public partial class CtlSettingsMemory : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private IMemoryDevice m_device;

        public CtlSettingsMemory()
        {
            InitializeComponent();
            initTypeList();
        }

        private void initTypeList()
        {
            cbxType.Items.Clear();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && typeof(IMemoryDevice).IsAssignableFrom(type))
                    {
                        var dev = (BusDeviceBase)Activator.CreateInstance(type);
                        cbxType.Items.Add(dev.Name);
                    }
                }
            }
            cbxType.Sorted = true;
        }

        public void Init(BusManager bmgr, IMemoryDevice device)
        {
            m_bmgr = bmgr;
            m_device = device;

            BusDeviceBase busDevice = (BusDeviceBase)device;
            cbxType.SelectedIndex = -1;
            if (m_device != null)
                for (int i = 0; i < cbxType.Items.Count; i++)
                    if (busDevice.Name == (string)cbxType.Items[i])
                    {
                        cbxType.SelectedIndex = i;
                        break;
                    }
        }

        public override void Apply()
        {
            var type = getType(cbxType.SelectedItem.ToString(), typeof(IMemoryDevice));

            var memory = (IMemoryDevice)Activator.CreateInstance(type);
            var oldMemory = m_bmgr.FindDevice<IMemoryDevice>();
            if (oldMemory != null && oldMemory.GetType() != memory.GetType())
            {
                var busOldMemory = (BusDeviceBase)oldMemory;
                var busNewMemory = (BusDeviceBase)memory;
                if (busOldMemory != null)
                {
                    m_bmgr.Remove(busOldMemory);
                }
                m_bmgr.Add(busNewMemory);
            }
            Init(m_bmgr, memory);
        }

        private Type getType(string typeName, Type iface)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in asm.GetTypes())
                    if (type.IsClass && !type.IsAbstract && iface.IsAssignableFrom(type))
                    {
                        BusDeviceBase dev = (BusDeviceBase)Activator.CreateInstance(type);
                        if (dev.Name == typeName)
                            return type;
                    }
            return null;
        }
    }
}
