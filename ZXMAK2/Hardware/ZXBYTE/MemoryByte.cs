using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Hardware.Spectrum;

namespace ZXMAK2.Hardware.ZXBYTE
{
    public class MemoryByte : MemorySpectrum128, IGuiExtension
    {
        #region IBusDevice

        public override string Name { get { return "BYTE 48K"; } }
        public override string Description { get { return "Memory Module \"Byte\" 48K\r\nVersion 1.2"; } }

        #endregion

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRdIo(0x75, 0x1F & 0x75, BusReadPort1F);
        }

        #region MemoryBase

        public override bool IsMap48 { get { return true; } }

        public override byte CMR0
        {
            get { return 0x30; }
            set { UpdateMapping(); }
        }

        public override byte CMR1
        {
            get { return 0x00; }
            set { UpdateMapping(); }
        }

        protected override void BusReset()
        {
            base.BusReset();
            m_rd1f = 0;
        }

        protected override void OnLoadRomPage(string pageName, byte[] data)
        {
            if (pageName == "DD66")
            {
                Array.Copy(data, m_dd66, data.Length);
            }
            else if (pageName == "DD71")
            {
                Array.Copy(data, m_dd71, data.Length);
            }
            else
            {
                base.OnLoadRomPage(pageName, data);
            }
        }

        protected override void ReadMem0000(ushort addr, ref byte value)
        {
            if (m_rd1f != 0 &&
                !DOSEN)
            {
                var adr66 = ((addr >> 7) & 0xFF) | ((m_sovmest << 8) & 0x100);
                var dat66 = m_dd66[adr66];
                if ((dat66 & 0x10) == 0)
                {
                    var adr71 = ((dat66 & 0x0F) << 7) | (addr & 0x7F);
                    value = m_dd71[adr71];
                    return;
                }
            }
            base.ReadMem0000(addr, ref value);
        }

        public override byte RDMEM_DBG(ushort addr)
        {
            if (addr < 0x4000 &&
                m_rd1f != 0 &&
                !DOSEN)
            {
                var adr66 = ((addr >> 7) & 0xFF) | ((m_sovmest << 8) & 0x100);
                var dat66 = m_dd66[adr66];
                if ((dat66 & 0x10) == 0)
                {
                    var adr71 = ((dat66 & 0x0F) << 7) | (addr & 0x7F);
                    return m_dd71[adr71];
                }
            }
            return base.RDMEM_DBG(addr);
        }

        #endregion

        #region Bus Handlers

        private void BusReadPort1F(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!DOSEN)
            {
                m_rd1f = 1;
            }
        }

        #endregion

        private int m_sovmest = 1; // COBMECT="OFF"
        private int m_rd1f = 0;
        private byte[] m_dd66 = new byte[512];
        private byte[] m_dd71 = new byte[2048];

        public MemoryByte()
            : base("ZXBYTE")
        {
        }

        #region IGuiExtension Members

        private GuiData m_guiData;
        private System.Windows.Forms.MenuItem m_subMenuItem;

        public override void AttachGui(GuiData guiData)
        {
            base.AttachGui(guiData);
            m_guiData = guiData;
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                System.Windows.Forms.MenuItem menuItem = guiData.MenuItem as System.Windows.Forms.MenuItem;
                if (menuItem != null)
                {
                    m_subMenuItem = new System.Windows.Forms.MenuItem("BYTE \"COBMECT.\"", menu_Click);
                    m_subMenuItem.Checked = m_sovmest == 0;
                    menuItem.MenuItems.Add(m_subMenuItem);
                }
            }
        }

        public override void DetachGui()
        {
            base.DetachGui();
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                if (m_subMenuItem != null)
                {
                    m_subMenuItem.Parent.MenuItems.Remove(m_subMenuItem);
                    m_subMenuItem.Dispose();
                    m_subMenuItem = null;
                }
            }
            m_guiData = null;
        }

        private void menu_Click(object sender, EventArgs e)
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                m_sovmest ^= 1;
                m_subMenuItem.Checked = m_sovmest == 0;
            }
        }
        #endregion IGuiExtension Members
    }
}
