﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Hardware;

namespace ZXMAK2.Controls.Configuration
{
    public partial class CtlSettingsMemory : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private IMemoryDevice m_device;

        public CtlSettingsMemory()
        {
            InitializeComponent();
            BindTypeList();
        }

        private void BindTypeList()
        {
            cbxType.Items.Clear();
            foreach (var bdd in DeviceEnumerator.SelectByType<IMemoryDevice>())
            {
                cbxType.Items.Add(bdd);
            }
            cbxType.Sorted = true;

            cbxRomSet.Items.Clear();
            foreach (var name in RomPack.GetRomSetNames())
            {
                cbxRomSet.Items.Add(name);
            }
            cbxRomSet.Sorted = true;
        }

        public void Init(BusManager bmgr, IMemoryDevice device)
        {
            m_bmgr = bmgr;
            m_device = device;

            var busDevice = (BusDeviceBase)device;
            cbxType.SelectedIndex = -1;
            if (busDevice != null)
            {
                for (var i = 0; i < cbxType.Items.Count; i++)
                {
                    var bdd = (BusDeviceDescriptor)cbxType.Items[i];
                    if (busDevice.GetType() == bdd.Type)
                    {
                        cbxType.SelectedIndex = i;
                        break;
                    }
                }
            }
            cbxType_SelectedIndexChanged(this, EventArgs.Empty);
        }

        public override void Apply()
        {
            var bdd = (BusDeviceDescriptor)cbxType.SelectedItem;

            var memory = m_bmgr.FindDevice<IMemoryDevice>();
            if (memory != null && 
                memory.GetType() != bdd.Type)
            {
                m_bmgr.Remove((BusDeviceBase)memory);
                memory = (IMemoryDevice)Activator.CreateInstance(bdd.Type);
                m_bmgr.Add((BusDeviceBase)memory);
            }
            var memoryBase = memory as MemoryBase;
            if (memoryBase != null && cbxRomSet.SelectedItem!=null)
            {
                memoryBase.RomSetName = (String)cbxRomSet.SelectedItem;
            }
            Init(m_bmgr, memory);
        }

        private void cbxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var bdd = (BusDeviceDescriptor)cbxType.SelectedItem;

            if (bdd==null || 
                !typeof(MemoryBase).IsAssignableFrom(bdd.Type))
            {
                lblRomSet.Visible = false;
                cbxRomSet.Visible = false;
                return;
            }
            MemoryBase memory = null;
            if (bdd.Type == m_device.GetType())
            {
                memory = (MemoryBase)m_device;
            }
            if (memory == null)
            {
                memory = (MemoryBase)Activator.CreateInstance(bdd.Type);
            }
            cbxRomSet.SelectedIndex = -1;
            for (var i = 0; i < cbxRomSet.Items.Count; i++)
            {
                if (memory.RomSetName == (String)cbxRomSet.Items[i])
                {
                    cbxRomSet.SelectedIndex = i;
                    break;
                }
            }
        }
    }
}
