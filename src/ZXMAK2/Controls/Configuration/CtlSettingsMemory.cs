using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Bus;
using ZXMAK2.Engine.Devices.Memory;

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
                foreach (Type type in asm.GetTypes())
                    if (type.IsClass && !type.IsAbstract && typeof(IMemoryDevice).IsAssignableFrom(type))
                    {
                        IBusDevice dev = (IBusDevice)Activator.CreateInstance(type);
                        cbxType.Items.Add(dev.Name);
                    }
            cbxType.Sorted = true;
        }

        public void Init(BusManager bmgr, IMemoryDevice device)
        {
            m_bmgr = bmgr;
            m_device = device;

            cbxType.SelectedIndex = -1;
            if(m_device!=null)
                for (int i = 0; i < cbxType.Items.Count; i++)
                    if (m_device.Name == (string)cbxType.Items[i])
                    {
                        cbxType.SelectedIndex = i;
                        break;
                    }
        }

        public override void Apply()
        {
            Type type = getType(cbxType.SelectedItem.ToString(), typeof(IMemoryDevice));

            IMemoryDevice memory = (IMemoryDevice)Activator.CreateInstance(type);
            IMemoryDevice oldMemory = (IMemoryDevice)m_bmgr.FindDevice(typeof(IMemoryDevice));
            if (oldMemory != null && oldMemory.GetType() != memory.GetType())
            {
                if (oldMemory != null)
                {
                    m_bmgr.Remove(oldMemory);
                }
                m_bmgr.Add(memory);
            }
            Init(m_bmgr, (IMemoryDevice)memory);
        }

        private Type getType(string typeName, Type iface)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in asm.GetTypes())
                    if (type.IsClass && !type.IsAbstract && iface.IsAssignableFrom(type))
                    {
                        IBusDevice dev = (IBusDevice)Activator.CreateInstance(type);
                        if (dev.Name == typeName)
                            return type;
                    }
            return null;
        }
    }
}
