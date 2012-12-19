/// Description: CPU Debug Window
/// Author: Alex Makeev
/// Date: 18.03.2008
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine;
using System.Collections.Generic;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Controls.Debugger
{
	public partial class FormCpu : Form
	{
		private IDebuggable m_spectrum;
        private DasmUtils m_dasmUtils;

        private bool showStack = true; // show stack or breakpoint list on the form(panel listState)
		
        public FormCpu()
		{
			InitializeComponent();
		}

        public bool AllowClose { get; set; }


        public void Init(IDebuggable debugTarget)
        {
            if(debugTarget==m_spectrum)
                return;
            if (m_spectrum != null)
            {
                m_spectrum.UpdateState -= spectrum_OnUpdateState;
                m_spectrum.Breakpoint -= spectrum_OnBreakpoint;
            }
            if (debugTarget != null)
            {
                m_spectrum = debugTarget;
                m_dasmUtils = new DasmUtils(m_spectrum.CPU, debugTarget.ReadMemory);
                m_spectrum.UpdateState += spectrum_OnUpdateState;
                m_spectrum.Breakpoint += spectrum_OnBreakpoint;
            }
        }

        private void FormCPU_FormClosed(object sender, FormClosedEventArgs e)
		{
            m_spectrum.UpdateState -= spectrum_OnUpdateState;
            m_spectrum.Breakpoint -= spectrum_OnBreakpoint;
		}

        private void FormCPU_Load(object sender, EventArgs e)
		{
			UpdateCPU(true);
		}
		
        private void FormCPU_Shown(object sender, EventArgs e)
		{
			Show();
			UpdateCPU(false);
			//dasmPanel.Focus();
            dbgCmdLine.Focus();
			Select();
		}


		private void spectrum_OnUpdateState(object sender, EventArgs args)
		{
            if (!Created)
                return;
            if (InvokeRequired)
            {
                Invoke(new EventHandler(spectrum_OnUpdateState), sender, args);
                return;
            }
            else
            {
                UpdateCPU(true);
            }
		}
		private void spectrum_OnBreakpoint(object sender, EventArgs args)
		{
            //LogAgent.Info("spectrum_OnBreakpoint {0}", sender);
            if (!Created)
                return;
            if (InvokeRequired)
            {
                Invoke(new EventHandler(spectrum_OnBreakpoint), sender, args);
                return;
            }
            else
            {
                Show();
                UpdateCPU(true);
                dasmPanel.Focus();
                Select();
            }
		}

		private void UpdateCPU(bool updatePC)
		{
			if (m_spectrum.IsRunning)
				updatePC = false;
			dasmPanel.ForeColor = m_spectrum.IsRunning ? SystemColors.ControlDarkDark : SystemColors.ControlText;
			UpdateREGS();
			UpdateDASM(updatePC);
			UpdateDATA();
		}

		private void UpdateREGS()
		{
			listREGS.Items.Clear();
			listREGS.Items.Add(" PC = " + m_spectrum.CPU.regs.PC.ToString("X4"));
			listREGS.Items.Add(" IR = " + m_spectrum.CPU.regs.IR.ToString("X4"));
			listREGS.Items.Add(" SP = " + m_spectrum.CPU.regs.SP.ToString("X4"));
			listREGS.Items.Add(" AF = " + m_spectrum.CPU.regs.AF.ToString("X4"));
			listREGS.Items.Add(" HL = " + m_spectrum.CPU.regs.HL.ToString("X4"));
			listREGS.Items.Add(" DE = " + m_spectrum.CPU.regs.DE.ToString("X4"));
			listREGS.Items.Add(" BC = " + m_spectrum.CPU.regs.BC.ToString("X4"));
			listREGS.Items.Add(" IX = " + m_spectrum.CPU.regs.IX.ToString("X4"));
			listREGS.Items.Add(" IY = " + m_spectrum.CPU.regs.IY.ToString("X4"));
			listREGS.Items.Add(" AF'= " + m_spectrum.CPU.regs._AF.ToString("X4"));
			listREGS.Items.Add(" HL'= " + m_spectrum.CPU.regs._HL.ToString("X4"));
			listREGS.Items.Add(" DE'= " + m_spectrum.CPU.regs._DE.ToString("X4"));
			listREGS.Items.Add(" BC'= " + m_spectrum.CPU.regs._BC.ToString("X4"));
			listREGS.Items.Add(" MW = " + m_spectrum.CPU.regs.MW.ToString("X4"));
			listF.Items.Clear();
			listF.Items.Add("  S = " + (((m_spectrum.CPU.regs.F & 0x80) != 0) ? "1" : "0"));
			listF.Items.Add("  Z = " + (((m_spectrum.CPU.regs.F & 0x40) != 0) ? "1" : "0"));
			listF.Items.Add(" F5 = " + (((m_spectrum.CPU.regs.F & 0x20) != 0) ? "1" : "0"));
			listF.Items.Add("  H = " + (((m_spectrum.CPU.regs.F & 0x10) != 0) ? "1" : "0"));
			listF.Items.Add(" F3 = " + (((m_spectrum.CPU.regs.F & 0x08) != 0) ? "1" : "0"));
			listF.Items.Add("P/V = " + (((m_spectrum.CPU.regs.F & 0x04) != 0) ? "1" : "0"));
			listF.Items.Add("  N = " + (((m_spectrum.CPU.regs.F & 0x02) != 0) ? "1" : "0"));
			listF.Items.Add("  C = " + (((m_spectrum.CPU.regs.F & 0x01) != 0) ? "1" : "0"));
			listF.Items.Add("========");
            listF.Items.Add("IFF1=" + (m_spectrum.CPU.IFF1 ? "1" : "0"));
            listF.Items.Add("IFF2=" + (m_spectrum.CPU.IFF2 ? "1" : "0"));
            listF.Items.Add("HALT=" + (m_spectrum.CPU.HALTED ? "1" : "0"));
            listF.Items.Add("BINT=" + (m_spectrum.CPU.BINT ? "1" : "0"));
            listF.Items.Add("  IM=" + m_spectrum.CPU.IM.ToString());
            listF.Items.Add("  FX=" + m_spectrum.CPU.FX.ToString());
            listF.Items.Add(" XFX=" + m_spectrum.CPU.XFX.ToString());

			/*listState.Items.Clear();
			listState.Items.Add("IFF1=" + (m_spectrum.CPU.IFF1 ? "1" : "0") + " IFF2=" + (m_spectrum.CPU.IFF2 ? "1" : "0"));
			listState.Items.Add("HALT=" + (m_spectrum.CPU.HALTED ? "1" : "0"));
            listState.Items.Add("BINT=" + (m_spectrum.CPU.BINT ? "1" : "0"));
            listState.Items.Add("  IM=" + m_spectrum.CPU.IM.ToString());
			listState.Items.Add("  FX=" + m_spectrum.CPU.FX.ToString());
			listState.Items.Add(" XFX=" + m_spectrum.CPU.XFX.ToString());*/

            listState.Items.Clear();
            if (showStack) // toggle by F12 key
            {
                // show stack on listState panel
                int  localStack = m_spectrum.CPU.regs.SP;
                byte counter = 0;
                do
                {
                    //the stack pointer can be set too low(SP=65535), e.g. Dizzy1,
                    //so condition on stack top must be added
                    if (localStack + 1 > 0xFFFF)
                        break;

                    UInt16 stackAdressLo = m_spectrum.ReadMemory(Convert.ToUInt16(localStack++));
                    UInt16 stackAdressHi = m_spectrum.ReadMemory(Convert.ToUInt16(localStack++));

                    listState.Items.Add((localStack-2).ToString("X4") + ":   " + (stackAdressLo + stackAdressHi * 256).ToString("X4"));

                    counter += 2;
                    if (counter >= 20)
                        break;

                } while (true);
            }
            else
            {
                if (m_spectrum.GetExtBreakpointsList().Count <= 0)
                {
                    listState.Items.Add("No breakpoints entered!");
                }
                else
                {
                    // show conditional breakpoints list on listState panel
                    foreach (KeyValuePair<byte, breakpointInfo> item in m_spectrum.GetExtBreakpointsList())
                    {
                        string brDesc = String.Empty;

                        brDesc += item.Key.ToString() + ":";
                        if (!item.Value.isOn)
                            brDesc += "(off)";
                        else
                            brDesc += " ";
                        brDesc += item.Value.leftCondition.ToString();
                        brDesc += item.Value.conditionTypeSign.ToString();
                        brDesc += item.Value.rightCondition.ToString();

                        listState.Items.Add(brDesc);

                    }
                }
            }

            //Window text
            this.Text = "Z80 CPU(";
            this.Text += "Tact=" + m_spectrum.CPU.Tact.ToString() + " ";
            this.Text += "frmT=" + m_spectrum.GetFrameTact().ToString() + ")";

			/*listState.Items.Add("Tact=" + m_spectrum.CPU.Tact.ToString());
			listState.Items.Add("frmT=" + m_spectrum.GetFrameTact().ToString());*/
		}
		
        private void UpdateDASM(bool updatePC)
		{
			if (!m_spectrum.IsRunning && updatePC)
			{
				dasmPanel.ActiveAddress = m_spectrum.CPU.regs.PC;
			}
			else
			{
				dasmPanel.UpdateLines();
				dasmPanel.Refresh();
			}
		}
		
        private void UpdateDATA()
		{
			dataPanel.UpdateLines();
			dataPanel.Refresh();
		}
		
        private bool dasmPanel_CheckExecuting(object Sender, ushort ADDR)
		{
			if (m_spectrum.IsRunning) return false;
			if (ADDR == m_spectrum.CPU.regs.PC) return true;
			return false;
		}
		
        private void dasmPanel_GetDasm(object Sender, ushort ADDR, out string DASM, out int len)
		{
            string mnemonic = Z80CPU.GetMnemonic(m_spectrum.ReadMemory, ADDR, true, out len);
            string timing = m_dasmUtils.GetTimingInfo(ADDR);
            
            DASM = string.Format("{0,-24} ; {1}", mnemonic, timing);
		}
		
        private void dasmPanel_GetData(object Sender, ushort ADDR, int len, out byte[] data)
		{
			data = new byte[len];
			for (int i = 0; i < len; i++)
				data[i] = m_spectrum.ReadMemory((ushort)(ADDR + i));
		}
		
        private bool dasmPanel_CheckBreakpoint(object Sender, ushort ADDR)
		{
			return m_spectrum.CheckBreakpoint(ADDR);
		}
		
        private void dasmPanel_SetBreakpoint(object Sender, ushort Addr)
		{
			if (m_spectrum.CheckBreakpoint(Addr))
				m_spectrum.RemoveBreakpoint(Addr);  // reset
			else
				m_spectrum.AddBreakpoint(Addr);    // set
		}


		private void FormCPU_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.F3:              // reset
					if (m_spectrum.IsRunning)
						break;
					m_spectrum.DoReset();
					UpdateCPU(true);
					break;
				case Keys.F7:              // StepInto
					if (m_spectrum.IsRunning)
						break;
					try
					{
						m_spectrum.DoStepInto();
					}
					catch (Exception ex)
					{
						LogAgent.Error(ex);
                        DialogProvider.ShowFatalError(ex);
					}
					UpdateCPU(true);
					break;
				case Keys.F8:              // StepOver
					if (m_spectrum.IsRunning)
						break;
					try
					{
						m_spectrum.DoStepOver();
					}
					catch (Exception ex)
					{
						LogAgent.Error(ex);
                        DialogProvider.ShowFatalError(ex);
					}
					UpdateCPU(true);
					break;
				case Keys.F9:              // Run
					m_spectrum.DoRun();
					UpdateCPU(false);
					break;
				case Keys.F5:              // Stop
					m_spectrum.DoStop();
					UpdateCPU(true);
					break;
                case Keys.F12:  // toggle stack / breakpoints on the panel
                    showStack = !showStack;
                    UpdateREGS();
                    break;
			}
		}

		private void menuItemDasmGotoADDR_Click(object sender, EventArgs e)
		{
			int ToAddr = 0;
			if (!InputBox.InputValue("Disassembly Address", "New Address:", "#", "X4", ref ToAddr, 0, 0xFFFF)) return;
			dasmPanel.TopAddress = (ushort)ToAddr;
		}
		
        private void menuItemDasmGotoPC_Click(object sender, EventArgs e)
		{
			dasmPanel.ActiveAddress = m_spectrum.CPU.regs.PC;
			dasmPanel.UpdateLines();
			Refresh();
		}
		
        private void menuItemDasmClearBP_Click(object sender, EventArgs e)
		{
			m_spectrum.ClearBreakpoints();
			UpdateCPU(false);
		}
		
        private void menuItemDasmRefresh_Click(object sender, EventArgs e)
		{
			dasmPanel.UpdateLines();
			Refresh();
		}

		private void listF_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (listF.SelectedIndex < 0) return;
			if (m_spectrum.IsRunning) return;

            switch (listF.SelectedIndex)
            {
                case 9:     //iff
                    m_spectrum.CPU.IFF1 = m_spectrum.CPU.IFF2 = !m_spectrum.CPU.IFF1;
                    UpdateCPU(false);
                    return;
                case 11:     //halt
                    m_spectrum.CPU.HALTED = !m_spectrum.CPU.HALTED;
                    UpdateCPU(false);
                    return;
                case 13:     //im
                    m_spectrum.CPU.IM++;
                    if (m_spectrum.CPU.IM > 2)
                        m_spectrum.CPU.IM = 0;
                    UpdateCPU(false);
                    return;
                /*ToDo:
                case 16:     //frmT
                    int frameTact = m_spectrum.GetFrameTact();
                    if (InputBox.InputValue("Frame Tact", "New Frame Tact:", "", "D", ref frameTact, 0, m_spectrum.FrameTactCount))
                    {
                        int delta = frameTact - m_spectrum.GetFrameTact();
                        if (delta < 0)
                            delta += m_spectrum.FrameTactCount;
                        m_spectrum.CPU.Tact += delta;
                    }
                    UpdateCPU(false);
                    return;*/
            }

			m_spectrum.CPU.regs.F ^= (byte)(0x80 >> listF.SelectedIndex);
			UpdateREGS();
		}

		private void listREGS_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (listREGS.SelectedIndex < 0) return;
			if (m_spectrum.IsRunning) return;
			ChangeRegByIndex(listREGS.SelectedIndex);
		}

        #region registry setters/getters by name/index
        private void ChangeRegByIndex(int index)
		{
			switch (index)
			{
				case 0:
					ChangeReg(ref m_spectrum.CPU.regs.PC, "PC");
					break;
				case 1:
					ChangeReg(ref m_spectrum.CPU.regs.IR, "IR");
					break;
				case 2:
					ChangeReg(ref m_spectrum.CPU.regs.SP, "SP");
					break;
				case 3:
					ChangeReg(ref m_spectrum.CPU.regs.AF, "AF");
					break;
				case 4:
					ChangeReg(ref m_spectrum.CPU.regs.HL, "HL");
					break;
				case 5:
					ChangeReg(ref m_spectrum.CPU.regs.DE, "DE");
					break;
				case 6:
					ChangeReg(ref m_spectrum.CPU.regs.BC, "BC");
					break;
				case 7:
					ChangeReg(ref m_spectrum.CPU.regs.IX, "IX");
					break;
				case 8:
					ChangeReg(ref m_spectrum.CPU.regs.IY, "IY");
					break;
				case 9:
					ChangeReg(ref m_spectrum.CPU.regs._AF, "AF'");
					break;
				case 10:
					ChangeReg(ref m_spectrum.CPU.regs._HL, "HL'");
					break;
				case 11:
					ChangeReg(ref m_spectrum.CPU.regs._DE, "DE'");
					break;
				case 12:
					ChangeReg(ref m_spectrum.CPU.regs._BC, "BC'");
					break;
				case 13:
					ChangeReg(ref m_spectrum.CPU.regs.MW, "MW (Memptr Word)");
					break;
			}
		}
		
        private void ChangeReg(ref ushort p, string reg)
		{
			int val = p;
			if (!InputBox.InputValue("Change Register " + reg, "New value:", "#", "X4", ref val, 0, 0xFFFF)) return;
			p = (ushort)val;
			UpdateCPU(false);
		}

        private void ChangeRegByName(string i_registryName, ushort newRegistryValue)
        {
            string registryName = i_registryName.ToUpper();

            switch (registryName)
            {
                case "PC":
                    m_spectrum.CPU.regs.PC = newRegistryValue;
                    break;
                case "IR":
                    m_spectrum.CPU.regs.IR = newRegistryValue;
                    break;
                case "SP":
                    m_spectrum.CPU.regs.SP = newRegistryValue;
                    break;
                case "AF":
                    m_spectrum.CPU.regs.AF = newRegistryValue;
                    break;
                case "A":
                    m_spectrum.CPU.regs.AF = (ushort)((newRegistryValue * 256) + (m_spectrum.CPU.regs.AF & 0xFF));
                    break;
                case "HL":
                    m_spectrum.CPU.regs.HL = newRegistryValue;
                    break;
                case "DE":
                    m_spectrum.CPU.regs.DE = newRegistryValue;
                    break;
                case "BC":
                    m_spectrum.CPU.regs.BC = newRegistryValue;
                    break;
                case "IX":
                    m_spectrum.CPU.regs.IX = newRegistryValue;
                    break;
                case "IY":
                    m_spectrum.CPU.regs.IY = newRegistryValue;
                    break;
                case "AF'":
                    m_spectrum.CPU.regs._AF = newRegistryValue;
                    break;
                case "HL'":
                    m_spectrum.CPU.regs._HL = newRegistryValue;
                    break;
                case "DE'":
                    m_spectrum.CPU.regs._DE = newRegistryValue;
                    break;
                case "BC'":
                    m_spectrum.CPU.regs._BC = newRegistryValue;
                    break;
                case "MW (Memptr Word)":
                    m_spectrum.CPU.regs.MW = newRegistryValue;
                    break;
                default:
                    throw new Exception("Bad registry name: " + i_registryName);
            }
        }
        #endregion

        private void contextMenuDasm_Popup(object sender, EventArgs e)
		{
			//if (m_spectrum.IsRunning) menuItemDasmClearBreakpoints.Enabled = false;
			//else menuItemDasmClearBreakpoints.Enabled = true;
		}

		// dbg funs
		private void dataPanel_DataClick(object Sender, ushort Addr)
		{
			int poked;
			poked = m_spectrum.ReadMemory((ushort)Addr);
			if (!InputBox.InputValue("POKE #" + Addr.ToString("X4"), "Value:", "#", "X2", ref poked, 0, 0xFF)) return;
			m_spectrum.WriteMemory((ushort)Addr, (byte)poked);
			UpdateCPU(false);
		}
		
        private void menuItemDataGotoADDR_Click(object sender, EventArgs e)
		{
			int adr = dataPanel.TopAddress;
			if (!InputBox.InputValue("Data Panel Address", "New Address:", "#", "X4", ref adr, 0, 0xFFFF)) return;
			dataPanel.TopAddress = (ushort)adr;
		}
		
        private void menuItemDataRefresh_Click(object sender, EventArgs e)
		{
			dataPanel.UpdateLines();
			Refresh();
		}

		private void menuItemDataSetColumnCount_Click(object sender, EventArgs e)
		{
			int cols = dataPanel.ColCount;
			if (!InputBox.InputValue("Data Panel Columns", "Column Count:", "", "", ref cols, 1, 32)) return;
			dataPanel.ColCount = cols;
		}

		private void dasmPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				contextMenuDasm.Show(dasmPanel, e.Location);
		}

		private void dataPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				contextMenuData.Show(dataPanel, e.Location);
		}

		private void FormCPU_FormClosing(object sender, FormClosingEventArgs e)
		{
            //LogAgent.Debug("FormCpu.FormCPU_FormClosing {0}", e.CloseReason);
            if (e.CloseReason != CloseReason.FormOwnerClosing && !this.AllowClose)
            {
                //LogAgent.Debug("FormCpu.Hide");
                Hide();
                e.Cancel = true;
            }
		}

		private void listState_DoubleClick(object sender, EventArgs e)
		{
            if (!showStack) // if we are in breakpoint mode only
            {
                int selectedIndex = listState.SelectedIndex;
                if (selectedIndex < 0 || m_spectrum.GetExtBreakpointsList().Count == 0) return;
                if (selectedIndex + 1 > m_spectrum.GetExtBreakpointsList().Count) return;

                string strTemp = listState.Items[selectedIndex].ToString();
                int index = strTemp.IndexOf(':');
                string key = String.Empty;
                if(index > 0)
                    key = strTemp.Substring(0, index);

                bool isBreakpointIsOn = m_spectrum.GetExtBreakpointsList()[Convert.ToByte(key)].isOn;
                m_spectrum.EnableOrDisableBreakpointStatus(Convert.ToByte(key), !isBreakpointIsOn);
                UpdateREGS();
            }
			/*if (listState.SelectedIndex < 0) return;
			if (m_spectrum.IsRunning)
				return;
			switch (listState.SelectedIndex)
			{
				case 0:     //iff
					m_spectrum.CPU.IFF1 = m_spectrum.CPU.IFF2 = !m_spectrum.CPU.IFF1;
					break;
				case 1:     //halt
					m_spectrum.CPU.HALTED = !m_spectrum.CPU.HALTED;
					break;
				case 3:     //im
					m_spectrum.CPU.IM++;
					if (m_spectrum.CPU.IM > 2)
						m_spectrum.CPU.IM = 0;
					break;
				case 7:     //frmT
					int frameTact = m_spectrum.GetFrameTact();
					if (InputBox.InputValue("Frame Tact", "New Frame Tact:", "", "D", ref frameTact, 0, m_spectrum.FrameTactCount))
					{
						int delta = frameTact - m_spectrum.GetFrameTact();
						if(delta < 0)
							delta += m_spectrum.FrameTactCount;
						m_spectrum.CPU.Tact += delta;
					}
					break;
			}
			UpdateCPU(false);*/
		}

        private static int s_addr = 0x4000;
        private static int s_len = 6912;
        
        private void menuLoadBlock_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog loadDialog = new OpenFileDialog())
            {
                loadDialog.InitialDirectory = ".";
                loadDialog.SupportMultiDottedExtensions = true;
                loadDialog.Title = "Load Block...";
                loadDialog.Filter = "All files (*.*)|*.*";
                loadDialog.DefaultExt = "";
                loadDialog.FileName = "";
                loadDialog.ShowReadOnly = false;
                loadDialog.CheckFileExists = true;
                if (loadDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                FileInfo fileInfo = new FileInfo(loadDialog.FileName);
                s_len = (int)fileInfo.Length;

                if (s_len < 1)
                    return;
                if (!InputBox.InputValue("Load Block", "Memory Address:", "#", "X4", ref s_addr, 0, 0xFFFF))
                    return;
                if (!InputBox.InputValue("Load Block", "Block Length:", "#", "X4", ref s_len, 0, 0x10000))
                    return;

                byte[] data = new byte[s_len];
                using (FileStream fs = new FileStream(loadDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    fs.Read(data, 0, data.Length);
                m_spectrum.WriteMemory((ushort)s_addr, data, 0, s_len);
            }
        }

        private void menuSaveBlock_Click(object sender, EventArgs e)
        {
            if (!InputBox.InputValue("Save Block", "Memory Address:", "#", "X4", ref s_addr, 0, 0xFFFF))
                return;
            if (!InputBox.InputValue("Save Block", "Block Length:", "#", "X4", ref s_len, 0, 0x10000))
                return;

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.InitialDirectory = ".";
                saveDialog.SupportMultiDottedExtensions = true;
                saveDialog.Title = "Save Block...";
                saveDialog.Filter = "Binary Files (*.bin)|*.bin|All files (*.*)|*.*";
                saveDialog.DefaultExt = "";
                saveDialog.FileName = "";
                saveDialog.OverwritePrompt = true;
                if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                byte[] data = new byte[s_len];
                m_spectrum.ReadMemory((ushort)s_addr, data, 0, s_len);

                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    fs.Write(data, 0, data.Length);
            }
        }

        private void dbgCmdLine_KeyUp(object sender, KeyEventArgs e)
        {
            //Debug command entered ?
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    string actualCommand = dbgCmdLine.Text;

                    List<string> parsedCommand = ParseCommand(actualCommand);
                    if (parsedCommand == null)
                        throw new Exception("unknown debugger command");

                    CommandType commandType = getDbgCommandType(parsedCommand);

                    if (commandType == CommandType.Unidentified)
                        throw new Exception("unknown debugger command"); // unknown cmd line type

                    //breakpoint manipulation ?
                    if (getDbgCommandType(parsedCommand) == CommandType.breakpointManipulation)
                    {
                        // add new enhanced breakpoint
                        string left = parsedCommand[1];

                        //left side must be registry or memory reference
                        if (!isRegistry(left) && !isMemoryReference(left))
                            throw new Exception("bad condition !");

                        m_spectrum.AddExtBreakpoint(parsedCommand); // add breakpoint, send parsed command e.g.: br pc == #0000

                        showStack = false; // show breakpoint list on listState panel
                    }
                    else if (getDbgCommandType(parsedCommand) == CommandType.gotoAdress)
                    {
                        // goto adress to dissasembly
                        dasmPanel.TopAddress = convertNumberWithPrefix(parsedCommand[1]);
                    }
                    else if (getDbgCommandType(parsedCommand) == CommandType.removeBreakpoint)
                    {
                        // remove breakpoint
                        m_spectrum.RemoveExtBreakpoint(Convert.ToByte(convertNumberWithPrefix(parsedCommand[1])));
                    }
                    else if (getDbgCommandType(parsedCommand) == CommandType.enableBreakpoint)
                    {
                        //enable breakpoint
                        m_spectrum.EnableOrDisableBreakpointStatus(Convert.ToByte(convertNumberWithPrefix(parsedCommand[1])), true);
                    }
                    else if (getDbgCommandType(parsedCommand) == CommandType.disableBreakpoint)
                    {
                        //disable breakpoint
                        m_spectrum.EnableOrDisableBreakpointStatus(Convert.ToByte(convertNumberWithPrefix(parsedCommand[1])), false);
                    }
                    else if (getDbgCommandType(parsedCommand) == CommandType.loadBreakpointsListFromFile)
                    {
                        //load breakpoints list into debugger
                        m_spectrum.LoadBreakpointsListFromFile(parsedCommand[1]);

                        showStack = false;                        
                    }
                    else if (getDbgCommandType(parsedCommand) == CommandType.saveBreakpointsListToFile)
                    {
                        //save breakpoints list into debugger
                        m_spectrum.SaveBreakpointsListToFile(parsedCommand[1]);
                    }
                    else
                    {
                        // memory/registry manipulation(LD instruction)
                        string left = parsedCommand[1];
                        UInt16 leftNum = 0;
                        bool isLeftMemoryReference = false;
                        bool isLeftRegistry = false;

                        string right = parsedCommand[2];
                        UInt16 rightNum = 0;
                        bool isRightMemoryReference = false;
                        bool isRightRegistry = false;

                        //Reading values - left side of statement
                        if (isMemoryReference(left))
                        {
                            leftNum = getReferencedMemoryPointer(left);

                            isLeftMemoryReference = true;
                        }
                        else
                        {
                            // is it register ?
                            if (isRegistry(left))
                            {
                                leftNum = getRegistryValueByName(m_spectrum.CPU.regs, left);
                                isLeftRegistry = true;
                            }
                            else
                                leftNum = convertNumberWithPrefix(left);
                        }

                        //Reading values - right side of statement
                        if (isMemoryReference(right))
                        {
                            rightNum = getReferencedMemoryPointer(right);

                            isRightMemoryReference = true;
                        }
                        else
                        {
                            // is it register ?
                            if (isRegistry(right))
                            {
                                rightNum = getRegistryValueByName(m_spectrum.CPU.regs, right);
                                isRightRegistry = true;
                            }
                            else
                                rightNum = convertNumberWithPrefix(right);
                        }

                        //Writing Memory/Registry
                        if (isLeftMemoryReference && isRightMemoryReference)
                        {
                            // memcpy e.g.: ld (#4000), (#3000)
                            m_spectrum.WriteMemory(leftNum, m_spectrum.ReadMemory(rightNum));
                            m_spectrum.WriteMemory((ushort)(leftNum + 1), m_spectrum.ReadMemory((ushort)(rightNum + 1)));
                        }
                        else if (isLeftMemoryReference)
                        {
                            // write registry or memory ?
                            if (isRightRegistry)
                            {
                                // e.g.: ld (#9C40), hl
                                UInt16 regValue = getRegistryValueByName(m_spectrum.CPU.regs, right);
                                if (regValue <= Byte.MaxValue)
                                    m_spectrum.WriteMemory(leftNum, Convert.ToByte(regValue));
                                else
                                {
                                    //2 bytes will be written; ToDo: check on adress if it is not > 65535
                                    byte hiBits = Convert.ToByte(regValue / 256);
                                    byte loBits = Convert.ToByte(regValue - hiBits*256 );

                                    m_spectrum.WriteMemory(leftNum, loBits);
                                    leftNum++;
                                    m_spectrum.WriteMemory(leftNum, hiBits);
                                }
                            }
                            else
                            {
                                // e.g.: ld (#9C40), #21
                                if (rightNum <= Byte.MaxValue)
                                    m_spectrum.WriteMemory(leftNum, Convert.ToByte(rightNum));
                                else
                                {
                                    //2 bytes will be written; ToDo: check on adress if it is not > 65535
                                    byte hiBits = Convert.ToByte(rightNum / 256);
                                    byte loBits = Convert.ToByte(rightNum - hiBits * 256);

                                    m_spectrum.WriteMemory(leftNum, loBits);
                                    leftNum++;
                                    m_spectrum.WriteMemory(leftNum, hiBits);
                                }
                            }
                        }
                        else if (isRightMemoryReference)
                        {
                            // e.g.: ld hl, (#9C40)
                            if (isLeftRegistry)
                            {
                                byte LByte = m_spectrum.ReadMemory(rightNum);
                                byte HByte = m_spectrum.ReadMemory((ushort)(rightNum + 1));

                                ChangeRegByName(left, (ushort)(HByte * 256 + LByte));
                            }
                            else
                            {
                                m_spectrum.WriteMemory(m_spectrum.ReadMemory(leftNum), Convert.ToByte(rightNum));
                            }
                        }
                        else
                        {
                            // no, so registry change
                            ChangeRegByName(left, rightNum);
                        }
                    }

                    UpdateREGS();
                    UpdateCPU(false);

                    dbgCmdLine.SelectAll();
                    dbgCmdLine.Focus();
                }
                catch (Exception)
                {
                    dbgCmdLine.BackColor = Color.Red;
                    dbgCmdLine.ForeColor = Color.Red;
                    dbgCmdLine.Refresh();
                    System.Threading.Thread.Sleep(50);
                    dbgCmdLine.BackColor = Color.White;
                    dbgCmdLine.ForeColor = Color.Black;
                }
            }
        }
    }

	public class InputBox : Form
	{

		private InputBox(string Caption, string Text)
		{
			this.label = new System.Windows.Forms.Label();
			this.textValue = new System.Windows.Forms.TextBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();

			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Location = new System.Drawing.Point(9, 13);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(31, 13);
			this.label.TabIndex = 1;
			this.label.Text = Text;
			// 
			// textValue
			// 
			this.textValue.Location = new System.Drawing.Point(12, 31);
			this.textValue.Name = "textValue";
			this.textValue.Size = new System.Drawing.Size(245, 20);
			this.textValue.TabIndex = 2;
			this.textValue.WordWrap = false;
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(57, 67);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 3;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(138, 67);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// Form
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(270, 103);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textValue);
			this.Controls.Add(this.label);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InputBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = Caption;
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public static bool Query(string Caption, string Text, ref string s_val)
		{
            using (InputBox ib = new InputBox(Caption, Text))
            {
                ib.textValue.Text = s_val;
                if (ib.ShowDialog() != System.Windows.Forms.DialogResult.OK) 
                    return false;
                s_val = ib.textValue.Text;
            }
			return true;
		}
		public static bool InputValue(string Caption, string Text, string prefix, string format, ref int value, int min, int max)
		{
			int val = value;

			string s_val = prefix + value.ToString(format);
			bool OKVal;
			do
			{
				OKVal = true;
				if (!Query(Caption, Text, ref s_val)) return false;

				try
				{
					string sTr = s_val.Trim();

					if ((sTr.Length > 0) && (sTr[0] == '#'))
					{
						sTr = sTr.Remove(0, 1);
						val = Convert.ToInt32(sTr, 16);
						//                  s_val = "0x" + s_val;
					}
					else if ((sTr.Length > 1) && ((sTr[1] == 'x') && (sTr[0] == '0')))
					{
						sTr = sTr.Remove(0, 2);
						val = Convert.ToInt32(sTr, 16);
					}
					else
						val = Convert.ToInt32(sTr, 10);
				}
				catch { MessageBox.Show("Numeric value required!"); OKVal = false; }
				if ((val < min) || (val > max)) { MessageBox.Show("Numeric value should be int the following range: " + min.ToString() + "..." + max.ToString() + " !"); OKVal = false; }
			} while (!OKVal);
			value = val;
			return true;
		}

		private System.Windows.Forms.Label label;
		private System.Windows.Forms.TextBox textValue;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
    }
}