using System;
using System.Xml;
using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Hardware.IC;

namespace ZXMAK2.Hardware.General
{
    public class FddController : BusDeviceBase, IConfigurable, IBetaDiskDevice, IGuiExtension
    {
        #region Fields

        private bool m_sandbox = false;
        private IconDescriptor m_iconRd = new IconDescriptor("FDDRD", Utils.GetIconStream("Fdd.png"));
        private IconDescriptor m_iconWr = new IconDescriptor("FDDWR", Utils.GetIconStream("FddWr.png"));
        protected Z80CPU m_cpu;
        protected IMemoryDevice m_memory;
        protected Wd1793 m_wd = new Wd1793();

        #endregion

        
        #region IBusDevice

        public override string Name { get { return "FDD WD1793"; } }
        public override string Description { get { return "FDD controller WD1793\r\nBDI-ports compatible\r\nPorts active when DOSEN=1 or SYSEN=1"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Disk; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_sandbox = bmgr.IsSandbox;
            m_cpu = bmgr.CPU;
            m_memory = bmgr.FindDevice<IMemoryDevice>();

            bmgr.RegisterIcon(m_iconRd);
            bmgr.RegisterIcon(m_iconWr);
            bmgr.SubscribeBeginFrame(BusBeginFrame);
            bmgr.SubscribeEndFrame(BusEndFrame);
            
            OnSubscribeIo(bmgr);

            foreach (var fs in m_wd.FDD[0].SerializeManager.GetSerializers())
            {
                bmgr.AddSerializer(fs);
            }
        }

        public override void BusConnect()
        {
            if (!m_sandbox)
            {
                foreach (var di in m_wd.FDD)
                {
                    di.Connect();
                }
            }
        }

        public override void BusDisconnect()
        {
            if (!m_sandbox)
            {
                foreach (var di in m_wd.FDD)
                {
                    di.Disconnect();
                }
            }
        }

        #endregion

        
        #region IConfigurable

        public void LoadConfig(XmlNode itemNode)
        {
            NoDelay = Utils.GetXmlAttributeAsBool(itemNode, "noDelay", false);
            LogIo = Utils.GetXmlAttributeAsBool(itemNode, "logIo", false);
            for (var i = 0; i < m_wd.FDD.Length; i++)
            {
                var inserted = false;
                var readOnly = true;
                var fileName = string.Empty;
                var node = itemNode.SelectSingleNode(string.Format("Drive[@index='{0}']", i));
                if (node != null)
                {
                    inserted = Utils.GetXmlAttributeAsBool(node, "inserted", inserted);
                    readOnly = Utils.GetXmlAttributeAsBool(node, "readOnly", readOnly);
                    fileName = Utils.GetXmlAttributeAsString(node, "fileName", fileName);
                }
                // will be opened on Connect
                m_wd.FDD[i].FileName = fileName;
                m_wd.FDD[i].IsWP = readOnly;
                m_wd.FDD[i].Present = inserted;
            }
        }

        public void SaveConfig(XmlNode itemNode)
        {
            Utils.SetXmlAttribute(itemNode, "noDelay", NoDelay);
            Utils.SetXmlAttribute(itemNode, "logIo", LogIo);
            for (var i = 0; i < m_wd.FDD.Length; i++)
            {
                if (m_wd.FDD[i].Present)
                {
                    XmlNode xn = itemNode.AppendChild(itemNode.OwnerDocument.CreateElement("Drive"));
                    Utils.SetXmlAttribute(xn, "index", i);
                    Utils.SetXmlAttribute(xn, "inserted", m_wd.FDD[i].Present);
                    Utils.SetXmlAttribute(xn, "readOnly", m_wd.FDD[i].IsWP);
                    if (!string.IsNullOrEmpty(m_wd.FDD[i].FileName))
                    {
                        Utils.SetXmlAttribute(xn, "fileName", m_wd.FDD[i].FileName);
                    }
                }
            }
        }

        #endregion


        #region IBetaDiskInterface

        public bool DOSEN
        {
            get { return m_memory.DOSEN; }
            set { m_memory.DOSEN = value; }
        }

        public DiskImage[] FDD
        {
            get { return m_wd.FDD; }
        }

        public bool NoDelay
        {
            get { return m_wd.NoDelay; }
            set { m_wd.NoDelay = value; }
        }

        public bool LogIo { get; set; }

        #endregion


        #region IGuiExtension Members

        private GuiData m_guiData;
        private System.Windows.Forms.MenuItem m_subMenuItem;
        private Controls.Debugger.dbgWD1793 m_form;

        public void AttachGui(GuiData guiData)
        {
            m_guiData = guiData;
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                System.Windows.Forms.MenuItem menuItem = guiData.MenuItem as System.Windows.Forms.MenuItem;
                if (menuItem != null)
                {
                    m_subMenuItem = new System.Windows.Forms.MenuItem("WD1793", menu_Click);
                    menuItem.MenuItems.Add(m_subMenuItem);
                }
            }
        }

        public void DetachGui()
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
                    m_form = new Controls.Debugger.dbgWD1793(m_wd);
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


        #region Private

        protected virtual void OnSubscribeIo(IBusManager bmgr)
        {
            bmgr.SubscribeWrIo(0x83, 0x1F & 0x83, BusWriteFdc);
            bmgr.SubscribeRdIo(0x83, 0x1F & 0x83, BusReadFdc);
            bmgr.SubscribeWrIo(0x83, 0xFF & 0x83, BusWriteSys);
            bmgr.SubscribeRdIo(0x83, 0xFF & 0x83, BusReadSys);
        }

        protected virtual void BusBeginFrame()
        {
            m_wd.LedRd = false;
            m_wd.LedWr = false;
        }

        protected virtual void BusEndFrame()
        {
            m_iconWr.Visible = m_wd.LedWr;
            m_iconRd.Visible = !m_wd.LedWr && m_wd.LedRd;
        }

        protected virtual void BusWriteFdc(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            if (m_memory.DOSEN || m_memory.SYSEN)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                LogIoWrite(m_cpu.Tact, (WD93REG)fdcReg, value);
                m_wd.Write(m_cpu.Tact, (WD93REG)fdcReg, value);
            }
        }

        protected virtual void BusReadFdc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            if (m_memory.DOSEN || m_memory.SYSEN)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                value = m_wd.Read(m_cpu.Tact, (WD93REG)fdcReg);
                LogIoRead(m_cpu.Tact, (WD93REG)fdcReg, value);
            }
        }

        protected virtual void BusWriteSys(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            if (m_memory.DOSEN || m_memory.SYSEN)
            {
                iorqge = false;
                LogIoWrite(m_cpu.Tact, WD93REG.SYS, value);
                m_wd.Write(m_cpu.Tact, WD93REG.SYS, value);
            }
        }

        protected virtual void BusReadSys(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            if (m_memory.DOSEN || m_memory.SYSEN)
            {
                iorqge = false;
                value = m_wd.Read(m_cpu.Tact, WD93REG.SYS);
                LogIoRead(m_cpu.Tact, WD93REG.SYS, value);
            }
        }

        protected void LogIoWrite(long tact, WD93REG reg, byte value)
        {
            if (LogIo)
            {
                LogAgent.Debug(
                    "WD93 {0} <== #{1:X2} [PC=#{2:X4}, T={3}]",
                    reg,
                    value,
                    m_cpu.regs.PC,
                    tact);
            }
        }

        protected void LogIoRead(long tact, WD93REG reg, byte value)
        {
            if (LogIo)
            {
                LogAgent.Debug(
                    "WD93 {0} ==> #{1:X2} [PC=#{2:X4}, T={3}]",
                    reg,
                    value,
                    m_cpu.regs.PC,
                    tact);
            }
        }

        #endregion Private
    }
}
