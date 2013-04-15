using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Hardware;
using ZXMAK2.Entities;


namespace ZXMAK2.Controls.Configuration
{
    public partial class CtlSettingsUla : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private UlaDeviceBase m_device;

        public CtlSettingsUla()
        {
            InitializeComponent();
            initTypeList();
        }

        private void initTypeList()
        {
            cbxType.Items.Clear();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        if (type.IsClass && !type.IsAbstract && typeof(IUlaDevice).IsAssignableFrom(type))
                        {
                            var dev = (BusDeviceBase)Activator.CreateInstance(type);
                            cbxType.Items.Add(dev.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogAgent.Error(ex);
                    DialogProvider.Show(
                        string.Format("Bad plugin assembly: {0}", asm.Location),
                        "ERROR",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
                }
            }
            cbxType.Sorted = true;
        }

        public void Init(BusManager bmgr, UlaDeviceBase device)
        {
            m_bmgr = bmgr;
            m_device = device;

            cbxType.SelectedIndex = -1;
            if (m_device != null)
                for (int i = 0; i < cbxType.Items.Count; i++)
                    if (m_device.Name == (string)cbxType.Items[i])
                    {
                        cbxType.SelectedIndex = i;
                        break;
                    }
        }

        public override void Apply()
        {
            var type = getType(cbxType.SelectedItem.ToString(), typeof(IUlaDevice));

            var ula = (IUlaDevice)Activator.CreateInstance(type);
            var oldUla = m_bmgr.FindDevice<IUlaDevice>();
            if (oldUla != null && oldUla.GetType() != ula.GetType())
            {
                var busOldUla = (BusDeviceBase)oldUla;
                var busNewUla = (BusDeviceBase)ula;
                if (busOldUla != null)
                {
                    m_bmgr.Remove(busOldUla);
                    ula.PortFE = oldUla.PortFE;
                }
                m_bmgr.Add(busNewUla);
            }
            Init(m_bmgr, (UlaDeviceBase)ula);
        }

        private Type getType(string typeName, Type iface)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && iface.IsAssignableFrom(type))
                    {
                        var dev = (BusDeviceBase)Activator.CreateInstance(type);
                        if (dev.Name == typeName)
                            return type;
                    }
                }
            }
            return null;
        }
    }
}
