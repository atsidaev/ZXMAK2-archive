using System;
using System.Windows.Forms;
using ZXMAK2.Hardware;

namespace ZXMAK2.Controls.Debugger
{
    public partial class FormMemoryMap : Form
    {
        private const string CS_UNKNOWN = "???";
        private MemoryBase m_memory = null;
        
        public FormMemoryMap(MemoryBase memory)
        {
            m_memory = memory;
            InitializeComponent();
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (m_memory == null)
            {
                lblCmrValue0.Text = CS_UNKNOWN;
                lblCmrValue0.Text = CS_UNKNOWN;
                lblWnd0000.Text = CS_UNKNOWN;
                lblWnd4000.Text = CS_UNKNOWN;
                lblWnd8000.Text = CS_UNKNOWN;
                lblWndC000.Text = CS_UNKNOWN;
                chkDosen.CheckState = CheckState.Indeterminate;
                chkSysen.CheckState = CheckState.Indeterminate;
                return;
            }
            lblCmrValue0.Text = string.Format("#{0:X2}", m_memory.CMR0);
            lblCmrValue1.Text = string.Format("#{0:X2}", m_memory.CMR1);
            lblWnd0000.Text = findPageName(m_memory.Window0000);
            lblWnd4000.Text = findPageName(m_memory.Window4000);
            lblWnd8000.Text = findPageName(m_memory.Window8000);
            lblWndC000.Text = findPageName(m_memory.WindowC000);
            chkDosen.Checked = m_memory.DOSEN;
            chkSysen.Checked = m_memory.SYSEN;
        }

        private string findPageName(byte[] wndRead)
        {
            for (int i = 0; i < m_memory.RomPages.Length; i++)
                if (m_memory.RomPages[i] == wndRead)
                    return string.Format("ROM {0}", m_memory.GetRomName(i));
            for (int i = 0; i < m_memory.RamPages.Length; i++)
                if (m_memory.RamPages[i] == wndRead)
                    return string.Format("RAM #{0:X2}", i);
            return CS_UNKNOWN;
        }
    }
}
