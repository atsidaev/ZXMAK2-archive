using System;
using System.IO;
using System.Xml;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;


namespace ZXMAK2.Hardware
{
    public abstract class MemoryBase : BusDeviceBase, IMemoryDevice, IGuiExtension
    {
        #region Fields

        private byte[][] m_romPages;
        private byte[][] m_ramPages;
        private int[] m_map48 = new int[4] { -1, -1, -1, -1 };
        private bool m_dosen = false;
        private bool m_sysen = false;
        private byte m_cmr0 = 0;
        private byte m_cmr1 = 0;
        protected UlaDeviceBase m_ula;
        protected String m_romSetName;
        protected byte[] m_trashPage = new byte[0x4000];

        #endregion


        #region IBusDevice Members

        public override BusDeviceCategory Category
        {
            get { return BusDeviceCategory.Memory; }
        }

        public override void BusInit(IBusManager bmgr)
        {
            m_ula = bmgr.FindDevice<UlaDeviceBase>();
            bmgr.SubscribeRdMem(0xC000, 0x0000, ReadMem0000);
            bmgr.SubscribeRdMem(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRdMem(0xC000, 0x8000, ReadMem8000);
            bmgr.SubscribeRdMem(0xC000, 0xC000, ReadMemC000);
            bmgr.SubscribeRdMemM1(0xC000, 0x0000, ReadMem0000);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, ReadMem8000);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, ReadMemC000);

            bmgr.SubscribeWrMem(0xC000, 0x0000, WriteMem0000);
            bmgr.SubscribeWrMem(0xC000, 0x4000, WriteMem4000);
            bmgr.SubscribeWrMem(0xC000, 0x8000, WriteMem8000);
            bmgr.SubscribeWrMem(0xC000, 0xC000, WriteMemC000);
        }

        public override void BusConnect()
        {
            LoadRomSet();
            UpdateMapping();
        }

        public override void BusDisconnect()
        {
        }

        protected override void OnConfigLoad(XmlNode itemNode)
        {
            base.OnConfigLoad(itemNode);
            RomSetName = Utils.GetXmlAttributeAsString(itemNode, "romSet", RomSetName);
        }

        protected override void OnConfigSave(XmlNode itemNode)
        {
            base.OnConfigSave(itemNode);
            Utils.SetXmlAttribute(itemNode, "romSet", RomSetName);
        }

        #endregion


        #region IMemory Members
        public static ushort get16bitValue(IntPtr pZ80Regs)
        {
            unsafe
            {
                return *((ushort*)pZ80Regs.ToPointer());
            }
        }

        public virtual ushort RDMEM_DBG_16bit(ushort addr)
        {
            if (addr == 0x3FFF)
            {
                switch (addr & 0xC000)
                {
                    case 0x0000:
                        return MapRead0000[addr];  // 48/128
                    case 0x4000:
                        return MapRead4000[addr & 0x3FFF];
                    case 0x8000:
                        return MapRead8000[addr & 0x3FFF];
                    default: // 0xC000:
                        return MapReadC000[addr & 0x3FFF];
                }
            }
            else
            {
                switch (addr & 0xC000)
                {
                    case 0x0000:
                        unsafe
                        {
                            fixed (byte* pRegs = &(MapRead0000[addr]))
                            {
                                return get16bitValue(new IntPtr(pRegs)); // 48/128
                            }
                        }
                    case 0x4000:
                        return MapRead4000[addr & 0x3FFF];
                    case 0x8000:
                        return MapRead8000[addr & 0x3FFF];
                    default: // 0xC000:
                        return MapReadC000[addr & 0x3FFF];
                }
            }
        }

        public virtual byte RDMEM_DBG(ushort addr)
        {
            switch (addr & 0xC000)
            {
                case 0x0000:
                    return MapRead0000[addr];  // 48/128
                case 0x4000:
                    return MapRead4000[addr & 0x3FFF];
                case 0x8000:
                    return MapRead8000[addr & 0x3FFF];
                default: // 0xC000:
                    return MapReadC000[addr & 0x3FFF];
            }
        }

        public virtual void WRMEM_DBG(ushort addr, byte value)
        {
            switch (addr & 0xC000)
            {
                case 0x0000:
                    MapWrite0000[addr] = value;
                    break;
                case 0x4000:
                    MapWrite4000[addr & 0x3FFF] = value;
                    break;
                case 0x8000:
                    MapWrite8000[addr & 0x3FFF] = value;
                    break;
                default: // 0xC000:
                    MapWriteC000[addr & 0x3FFF] = value;
                    break;
            }
        }

        public virtual byte[][] RamPages
        {
            get { return m_ramPages; }
        }

        public virtual byte[][] RomPages
        {
            get { return m_romPages; }
        }

        public virtual byte CMR0
        {
            get { return m_cmr0; }
            set
            {
                if (m_cmr0 != value)
                {
                    m_cmr0 = value;
                    UpdateMapping();
                }
            }
        }

        public virtual byte CMR1
        {
            get { return m_cmr1; }
            set
            {
                if (m_cmr1 != value)
                {
                    m_cmr1 = value;
                    UpdateMapping();
                }
            }
        }

        public virtual bool DOSEN
        {
            get { return m_dosen; }
            set
            {
                if (m_dosen != value)
                {
                    m_dosen = value;
                    UpdateMapping();
                }
            }
        }

        public virtual bool SYSEN
        {
            get { return m_sysen; }
            set
            {
                if (m_sysen != value)
                {
                    m_sysen = value;
                    UpdateMapping();
                }
            }
        }

        public virtual bool IsMap48
        {
            get { return false; }
        }

        public int[] Map48
        {
            get { return m_map48; }
        }

        public virtual bool IsRom48
        {
            get { return MapRead0000 == RomPages[GetRomIndex(RomName.ROM_SOS)]; }
        }

        #endregion


        public MemoryBase(
            String romSetName,
            int romPageCount,
            int ramPageCount)
        {
            m_romSetName = romSetName;// "Default";
            Init(romPageCount, ramPageCount);
            OnPowerOn();
        }

        public String RomSetName
        {
            get { return m_romSetName; }
            set
            {
                m_romSetName = value;
                OnConfigChanged();
                LoadRomSet();
            }
        }

        public byte[] Window0000 { get { return MapRead0000; } }
        public byte[] Window4000 { get { return MapRead4000; } }
        public byte[] Window8000 { get { return MapRead8000; } }
        public byte[] WindowC000 { get { return MapReadC000; } }

        public override void ResetState()
        {
            m_map48 = new int[4] { -1, -1, -1, -1 };
            m_dosen = false;
            m_sysen = false;
            m_cmr0 = 0;
            m_cmr1 = 0;
            LoadRomSet();
            UpdateMapping();
            base.ResetState();
        }

        public virtual string GetRomName(int pageNo)
        {
            if (pageNo == GetRomIndex(RomName.ROM_128))
                return "128";
            if (pageNo == GetRomIndex(RomName.ROM_SOS))
                return "SOS";
            if (pageNo == GetRomIndex(RomName.ROM_DOS))
                return "DOS";
            if (pageNo == GetRomIndex(RomName.ROM_SYS))
                return "SYS";
            return string.Format("#{0:X2}", pageNo);
        }

        public abstract int GetRomIndex(RomName romId);

        
        #region RD/WR MEM

        protected byte[] MapRead0000 = null;
        protected byte[] MapRead4000 = null;
        protected byte[] MapRead8000 = null;
        protected byte[] MapReadC000 = null;
        protected byte[] MapWrite0000 = null;
        protected byte[] MapWrite4000 = null;
        protected byte[] MapWrite8000 = null;
        protected byte[] MapWriteC000 = null;

        protected virtual void ReadMem0000(ushort addr, ref byte value)
        {
            value = MapRead0000[addr];
        }

        protected virtual void ReadMem4000(ushort addr, ref byte value)
        {
            value = MapRead4000[addr & 0x3FFF];
        }

        protected virtual void ReadMem8000(ushort addr, ref byte value)
        {
            value = MapRead8000[addr & 0x3FFF];
        }

        protected virtual void ReadMemC000(ushort addr, ref byte value)
        {
            value = MapReadC000[addr & 0x3FFF];
        }

        protected virtual void WriteMem0000(ushort addr, byte value)
        {
            MapWrite0000[addr & 0x3FFF] = value;
        }

        protected virtual void WriteMem4000(ushort addr, byte value)
        {
            MapWrite4000[addr & 0x3FFF] = value;
        }

        protected virtual void WriteMem8000(ushort addr, byte value)
        {
            MapWrite8000[addr & 0x3FFF] = value;
        }

        protected virtual void WriteMemC000(ushort addr, byte value)
        {
            MapWriteC000[addr & 0x3FFF] = value;
        }

        #endregion

        
        protected virtual void Init(
            int romPageCount,
            int ramPageCount)
        {
            m_romPages = new byte[romPageCount][];
            for (var i = 0; i < m_romPages.Length; i++)
            {
                m_romPages[i] = new byte[0x4000];
            }
            m_ramPages = new byte[ramPageCount][];
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }

        protected virtual void OnPowerOn()
        {
        }

        protected abstract void UpdateMapping();

        
        #region Rom Loader

        private void LoadRomSet()
        {
            for (var i = 0; i < RomPages.Length; i++)
            {
                for (var j = 0; j < RomPages[i].Length; j++)
                {
                    RomPages[i][j] = 0xFF;
                }
            }
            foreach (var page in RomPack.GetRomSet(m_romSetName))
            {
                OnLoadRomPage(page.Name, page.Content);
            }
        }

        protected virtual void OnLoadRomPage(string pageName, byte[] data)
        {
            var pageNo = -1;
            switch (pageName.ToUpper())
            {
                case "128": pageNo = GetRomIndex(RomName.ROM_128); break;
                case "SOS": pageNo = GetRomIndex(RomName.ROM_SOS); break;
                case "DOS": pageNo = GetRomIndex(RomName.ROM_DOS); break;
                case "SYS": pageNo = GetRomIndex(RomName.ROM_SYS); break;
                case "RAW":
                    {
                        var capLen = (data.Length / 0x4000) * 0x4000;
                        if ((data.Length % 0x4000) != 0)
                        {
                            capLen += 0x4000;
                        }
                        var capRom = new byte[capLen];
                        for (var i = 0; i < capRom.Length; i++)
                        {
                            capRom[i] = 0xFF;	// non flashed area
                        }
                        Array.Copy(data, 0, capRom, 0, data.Length);
                        for (var i = 0; i < RomPages.Length; i++)
                        {
                            Array.Copy(
                                capRom,
                                (i * 0x4000) % capLen,
                                RomPages[i],
                                0,
                                0x4000);
                        }
                    }
                    return;
                default:
                    return;
            }
            if (pageNo >= 0)
            {
                var length = 0x4000;
                if (data.Length < length)
                {
                    length = data.Length;
                }
                Array.Copy(data, 0, RomPages[pageNo], 0, length);
            }
        }

        #endregion

        #region IGuiExtension Members

        private GuiData m_guiData;
        private System.Windows.Forms.MenuItem m_subMenuItem;
        private Controls.Debugger.FormMemoryMap m_form;

        public virtual void AttachGui(GuiData guiData)
        {
            m_guiData = guiData;
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                System.Windows.Forms.MenuItem menuItem = guiData.MenuItem as System.Windows.Forms.MenuItem;
                if (menuItem != null)
                {
                    m_subMenuItem = new System.Windows.Forms.MenuItem("Memory Map", menu_Click);
                    menuItem.MenuItems.Add(m_subMenuItem);
                }
            }
        }

        public virtual void DetachGui()
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                if (m_subMenuItem != null)
                {
                    m_subMenuItem.Parent.MenuItems.Remove(m_subMenuItem);
                    m_subMenuItem.Dispose();
                    m_subMenuItem = null;
                }
                if (m_form != null)
                {
                    m_form.Close();
                    m_form = null;
                }
            }
            m_guiData = null;
        }

        private void menu_Click(object sender, EventArgs e)
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                if (m_form == null)
                {
                    m_form = new Controls.Debugger.FormMemoryMap(this);
                    m_form.FormClosed += delegate(object obj, System.Windows.Forms.FormClosedEventArgs arg) { m_form = null; };
                    m_form.Show((System.Windows.Forms.Form)m_guiData.MainWindow);
                }
                else
                {
                    m_form.Show();
                    m_form.Activate();
                }
            }
        }

        #endregion
    }
}
