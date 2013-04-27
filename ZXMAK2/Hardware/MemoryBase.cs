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
        #region IBusDevice Members

        public override BusDeviceCategory Category { get { return BusDeviceCategory.Memory; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_ula = bmgr.FindDevice<UlaDeviceBase>();
            bmgr.SubscribeRDMEM(0xC000, 0x0000, ReadMem0000);
            bmgr.SubscribeRDMEM(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM(0xC000, 0x8000, ReadMem8000);
            bmgr.SubscribeRDMEM(0xC000, 0xC000, ReadMemC000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x0000, ReadMem0000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x8000, ReadMem8000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0xC000, ReadMemC000);

            bmgr.SubscribeWRMEM(0xC000, 0x0000, WriteMem0000);
            bmgr.SubscribeWRMEM(0xC000, 0x4000, WriteMem4000);
            bmgr.SubscribeWRMEM(0xC000, 0x8000, WriteMem8000);
            bmgr.SubscribeWRMEM(0xC000, 0xC000, WriteMemC000);
        }

        public override void BusConnect()
        {
            LoadRom();
            UpdateMapping();
        }

        public override void BusDisconnect()
        {
        }

        #endregion

        #region IMemory Members

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

        public abstract byte[][] RamPages { get; }
        public virtual byte[][] RomPages { get { return m_romImages; } }

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

        public virtual bool IsMap48 { get { return false; } }
        public int[] Map48 { get { return m_map48; } }

        public virtual bool IsRom48 { get { return MapRead0000 == RomPages[GetRomIndex(RomName.ROM_SOS)]; } }

        #endregion

        private byte[][] m_romImages = new byte[4][];
        private int[] m_map48 = new int[4] { -1, -1, -1, -1 };
        private bool m_dosen = false;
        private bool m_sysen = false;
        private byte m_cmr0 = 0;
        private byte m_cmr1 = 0;
        protected UlaDeviceBase m_ula;


        public MemoryBase()
        {
            for (var i = 0; i < m_romImages.Length; i++)
            {
                m_romImages[i] = new byte[0x4000];
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
            LoadRom();
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

        public virtual int GetRomIndex(RomName romId)
        {
            switch (romId)
            {
                case RomName.ROM_128: return 0;
                case RomName.ROM_SOS: return 1;
                case RomName.ROM_DOS: return 2;
                case RomName.ROM_SYS: return 3;
            }
            LogAgent.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
        }

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

        protected abstract void UpdateMapping();

        #region Rom Loader

        protected virtual void LoadRom()
        {
            for (var i = 0; i < RomPages.Length; i++)
            {
                for (var j = 0; j < RomPages[i].Length; j++)
                {
                    m_romImages[i][j] = 0xFF;
                }
            }
            LoadRomPack("Default");
        }

        protected void LoadRomPack(string modelName)
        {
            try
            {
                XmlDocument mapping = new XmlDocument();
                using (Stream stream = GetRomFileStream("~mapping.xml"))
                    mapping.Load(stream);
                foreach (XmlNode modelNode in mapping.SelectNodes("/Mapping/Model"))
                    if (modelNode.Attributes["name"] != null && string.Compare(modelName, modelNode.Attributes["name"].InnerText) == 0)
                    {
                        loadRomModelSection(modelNode);
                        break;
                    }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                return;
            }
        }


        private void loadRomModelSection(XmlNode modelNode)
        {
            foreach (XmlNode pageNode in modelNode.SelectNodes("Page"))
            {
                if (pageNode.Attributes["name"] == null || pageNode.Attributes["image"] == null)
                {
                    LogAgent.Warn("ROM mapping contains invalid Page node attribute \"name\" or \"image\" is missing");
                    continue;
                }
                string pageName = pageNode.Attributes["name"].InnerText;
                string pageImage = pageNode.Attributes["image"].InnerText;
                try
                {
                    int fileOffset = 0;
                    int fileLength = (int)GetRomFileLength(pageImage);
                    if (pageNode.Attributes["offset"] != null)
                    {
                        fileOffset = Utils.ParseSpectrumInt(pageNode.Attributes["offset"].InnerText);
                        fileLength -= fileOffset;
                    }
                    if (pageNode.Attributes["length"] != null)
                        fileLength = Utils.ParseSpectrumInt(pageNode.Attributes["length"].InnerText);

                    byte[] data = new byte[fileLength];
                    using (Stream stream = GetRomFileStream(pageImage))
                    {
                        stream.Seek(fileOffset, SeekOrigin.Begin);
                        stream.Read(data, 0, data.Length);
                    }
                    OnLoadRomPage(pageName, data);
                }
                catch (Exception ex)
                {
                    LogAgent.Error(ex);
                    LogAgent.Error(
                        "ROM load failed, model=\"{0}\", page=\"{1}\", image=\"{2}\"",
                        modelNode.Attributes["name"].InnerText,
                        pageName,
                        pageImage);
                }
            }
        }

        protected virtual void OnLoadRomPage(string pageName, byte[] data)
        {
            int pageNo = -1;
            switch (pageName.ToUpper())
            {
                case "128": pageNo = GetRomIndex(RomName.ROM_128); break;
                case "SOS": pageNo = GetRomIndex(RomName.ROM_SOS); break;
                case "DOS": pageNo = GetRomIndex(RomName.ROM_DOS); break;
                case "SYS": pageNo = GetRomIndex(RomName.ROM_SYS); break;
                case "RAW":
                    {
                        int capLen = (data.Length / 0x4000) * 0x4000;
                        if ((data.Length % 0x4000) != 0)
                            capLen += 0x4000;
                        byte[] capRom = new byte[capLen];
                        for (int i = 0; i < capRom.Length; i++)
                            capRom[i] = 0xFF;	// non flashed area
                        Array.Copy(data, 0, capRom, 0, data.Length);
                        for (int i = 0; i < RomPages.Length; i++)
                        {
                            Array.Copy(capRom, (i * 0x4000) % capLen, RomPages[i], 0, 0x4000);
                        }
                    }
                    return;
                default:
                    return;
            }
            if (pageNo >= 0)
            {
                int length = 0x4000;
                if (data.Length < length)
                    length = data.Length;
                Array.Copy(data, 0, RomPages[pageNo], 0, length);
            }
        }

        public static long GetRomFileLength(string fileName)
        {
            string folderName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // override
            string romsFolderName = Path.Combine(folderName, "roms");
            if (Directory.Exists(romsFolderName))
            {
                string romsFileName = Path.Combine(romsFolderName, fileName);
                if (File.Exists(romsFileName))
                    return new FileInfo(romsFileName).Length;
            }

            string pakFileName = Path.Combine(folderName, "Roms.PAK");

            using (ZipLib.Zip.ZipFile zip = new ZipLib.Zip.ZipFile(pakFileName))
                foreach (ZipLib.Zip.ZipEntry entry in zip)
                    if (entry.IsFile &&
                        entry.CanDecompress &&
                        string.Compare(entry.Name, fileName, true) == 0)
                    {
                        return entry.Size;
                    }
            throw new FileNotFoundException(string.Format("ROM file not found: {0}", fileName));
        }

        public static Stream GetRomFileStream(string fileName)
        {
            string folderName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // override
            string romsFolderName = Path.Combine(folderName, "roms");
            if (Directory.Exists(romsFolderName))
            {
                string romsFileName = Path.Combine(romsFolderName, fileName);
                if (File.Exists(romsFileName))
                    using (FileStream fs = new FileStream(romsFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        byte[] fileData = new byte[fs.Length];
                        fs.Read(fileData, 0, fileData.Length);
                        return new MemoryStream(fileData);
                    }
            }

            string pakFileName = Path.Combine(folderName, "Roms.PAK");

            using (ZipLib.Zip.ZipFile zip = new ZipLib.Zip.ZipFile(pakFileName))
                foreach (ZipLib.Zip.ZipEntry entry in zip)
                    if (entry.IsFile &&
                        entry.CanDecompress &&
                        string.Compare(entry.Name, fileName, true) == 0)
                    {
                        byte[] fileData = new byte[entry.Size];
                        using (Stream s = zip.GetInputStream(entry))
                            s.Read(fileData, 0, fileData.Length);
                        return new MemoryStream(fileData);
                    }
            throw new FileNotFoundException(string.Format("ROM file not found: {0}", fileName));
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
