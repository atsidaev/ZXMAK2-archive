/// Description: Debugger Adlers Window
/// Author: Adlers
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Presentation.Interfaces;
using ZXMAK2.Host.WinForms.Views;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Engine.Cpu.Tools;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Hardware.Adlers.Core;

namespace ZXMAK2.Hardware.Adlers.Views
{
    public partial class FormCpu : FormView, IDebuggerAdlersView
    {
        private IDebuggable m_spectrum;
        private DasmTool m_dasmTool;
        private TimingTool m_timingTool;
        private bool m_isTracing;
        private CpuRegs m_cpuRegs;
        private DebuggerTrace m_debuggerTrace;

        private bool showStack = true; // show stack or breakpoint list on the form(panel listState)

        //debugger command line history
        private List<string> m_cmdLineHistory = new List<string>();
        private int m_cmdLineHistoryPos = 0;

        //find bytes in memory
        static string m_strBytesToFindSave = "#AFC3, 201";

        public FormCpu(IDebuggable debugTarget, IBusManager bmgr)
        {
            InitializeComponent();
            Init(debugTarget);
            bmgr.SubscribeWrMem(0, 0, CheckWriteMem);
            bmgr.SubscribeRdMem(0, 0, CheckReadMem);
            bmgr.SubscribePreCycle(Bus_OnBeforeCpuCycle);
            m_cpuRegs = bmgr.CPU.regs;
        }

        private void Init(IDebuggable debugTarget)
        {
            if (debugTarget == m_spectrum)
                return;
            if (m_spectrum != null)
            {
                m_spectrum.UpdateState -= spectrum_OnUpdateState;
                m_spectrum.Breakpoint -= spectrum_OnBreakpoint;
            }
            if (debugTarget != null)
            {
                m_spectrum = debugTarget;
                m_dasmTool = new DasmTool(debugTarget.ReadMemory);
                m_timingTool = new TimingTool(m_spectrum.CPU, debugTarget.ReadMemory);
                m_spectrum.UpdateState += spectrum_OnUpdateState;
                m_spectrum.Breakpoint += spectrum_OnBreakpoint;
            }

            //some GUI init
            this.btnStartTrace.Image = global::ZXMAK2.Resources.ImageResources.Play_16x16;
            this.btnStopTrace.Image = global::ZXMAK2.Resources.ImageResources.Record_16x16;

            m_debuggerTrace = new DebuggerTrace(m_spectrum);

            listViewAdressRanges.View = View.Details;
            listViewAdressRanges.Columns.Add("From", -2, HorizontalAlignment.Right);
            listViewAdressRanges.Columns.Add(" To ", -2, HorizontalAlignment.Right);
            listViewAdressRanges.Columns.Add("Active", -2, HorizontalAlignment.Center );

            ListViewItem item = new ListViewItem(new[] { "#4000", "#FFFF", "Yes"});
            listViewAdressRanges.Items.Add(item);
        }

        public static string GetAppRootDir()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private void FormCPU_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_spectrum.UpdateState -= spectrum_OnUpdateState;
            m_spectrum.Breakpoint -= spectrum_OnBreakpoint;

            //ToDo: save debugger config file(debugger_config.xml)
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

        private void FormCpu_Activated(object sender, EventArgs e)
        {
            if (!m_spectrum.IsRunning)
                dasmPanel.UpdateLines();
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
                int localStack = m_spectrum.CPU.regs.SP;
                byte counter = 0;
                do
                {
                    //the stack pointer can be set too low(SP=65535), e.g. Dizzy1,
                    //so condition on stack top must be added
                    if (localStack + 1 > 0xFFFF)
                        break;

                    UInt16 stackAdressLo = m_spectrum.ReadMemory(Convert.ToUInt16(localStack++));
                    UInt16 stackAdressHi = m_spectrum.ReadMemory(Convert.ToUInt16(localStack++));

                    listState.Items.Add((localStack - 2).ToString("X4") + ":   " + (stackAdressLo + stackAdressHi * 256).ToString("X4"));

                    counter += 2;
                    if (counter >= 20)
                        break;

                } while (true);
            }
            else
            {
                if (GetExtBreakpointsList().Count <= 0)
                {
                    listState.Items.Add("No breakpoints entered!");
                }
                else
                {
                    // show conditional breakpoints list on listState panel
                    foreach (KeyValuePair<byte, BreakpointAdlers> item in GetExtBreakpointsList())
                    {
                        string brDesc = String.Empty;

                        brDesc += item.Key.ToString() + ":";
                        if (!item.Value.Info.IsOn)
                            brDesc += "(off)";
                        else
                            brDesc += " ";
                        if (item.Value.Info.AccessType == BreakPointConditionType.memoryRead)
                        {
                            //desc of memory read breakpoint type
                            brDesc += String.Format("mem read #{0:X2}", item.Value.Info.LeftValue);
                        }
                        else
                        {
                            brDesc += item.Value.Info.BreakpointString;
                        }

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
            var mnemonic = m_dasmTool.GetMnemonic(ADDR, out len);
            var timing = m_timingTool.GetTimingString(ADDR);

            DASM = string.Format("{0,-24} ; {1}", mnemonic, timing);
        }

        private void dasmPanel_GetData(object Sender, ushort ADDR, int len, out byte[] data)
        {
            data = new byte[len];
            for (int i = 0; i < len; i++)
                data[i] = m_spectrum.ReadMemory((ushort)(ADDR + i));
        }

        private bool dasmPanel_CheckBreakpoint(object sender, ushort addr)
        {
            foreach (Breakpoint bp in m_spectrum.GetBreakpointList())
                if (bp.Address.HasValue && bp.Address == addr)
                    return true;
            return false;
        }

        private void dasmPanel_SetBreakpoint(object sender, ushort addr)
        {
            bool found = false;
            foreach (Breakpoint bp in m_spectrum.GetBreakpointList())
            {
                if (bp.Address.HasValue && bp.Address == addr)
                {
                    m_spectrum.RemoveBreakpoint(bp);
                    found = true;
                }
            }
            if (!found)
            {
                Breakpoint bp = new Breakpoint(addr);
                m_spectrum.AddBreakpoint(bp);
            }
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
                        Logger.Error(ex);
                        Locator.Resolve<IUserMessage>().ErrorDetails(ex);
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
                        Logger.Error(ex);
                        Locator.Resolve<IUserMessage>().ErrorDetails(ex);
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
                    e.Handled = true;
                    break;
                case Keys.F12:  // toggle Stack/Breakpoints on the panel
                    showStack = !showStack;
                    UpdateREGS();
                    break;
                case Keys.F:
                    if( e.Control )
                    {
                        menuItemFindBytes_Click(new object(), null);
                    }
                    break;
                case Keys.N:
                    if (e.Control)
                    {
                        menuItemFindBytesNext_Click(null, null);
                    }
                    break;
                case Keys.Escape:
                    this.Hide();
                    if (this.Owner != null)
                        this.Owner.Focus();
                    break;
            }
        }

        private void menuItemDasmGotoADDR_Click(object sender, EventArgs e)
        {
            int ToAddr = 0;
            var service = Locator.Resolve<IUserQuery>();
            if (service == null)
            {
                return;
            }
            if (!service.QueryValue("Disassembly Address", "New Address:", "#{0:X4}", ref ToAddr, 0, 0xFFFF)) return;
            dasmPanel.TopAddress = (ushort)ToAddr;
            dasmPanel.Focus();
        }

        private void menuItemDasmGotoPC_Click(object sender, EventArgs e)
        {
            dasmPanel.ActiveAddress = m_spectrum.CPU.regs.PC;
            dasmPanel.UpdateLines();
            Refresh();
        }

        private void menuItemDumpMemoryAtCurrentAddress_Click(object sender, EventArgs e)
        {
            dataPanel.TopAddress = dasmPanel.ActiveAddress;
        }

        private void menuItemFollowInDisassembly_Click(object sender, EventArgs e)
        {
            //Locator.Resolve<IUserMessage>().Info(dataPanel.ActiveAddress.ToString());
            dasmPanel.TopAddress = dataPanel.ActiveAddress;
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
            var service = Locator.Resolve<IUserQuery>();
            if (!service.QueryValue("Change Register " + reg, "New value:", "#{0:X4}", ref val, 0, 0xFFFF)) return;
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
            var service = Locator.Resolve<IUserQuery>();
            if (!service.QueryValue("POKE #" + Addr.ToString("X4"), "Value:", "#{0:X2}", ref poked, 0, 0xFF)) return;
            m_spectrum.WriteMemory((ushort)Addr, (byte)poked);
            UpdateCPU(false);
        }

        private void menuItemDataGotoADDR_Click(object sender, EventArgs e)
        {
            int adr = dataPanel.TopAddress;
            var service = Locator.Resolve<IUserQuery>();
            if (!service.QueryValue("Data Panel Address", "New Address:", "#{0:X4}", ref adr, 0, 0xFFFF)) return;
            dataPanel.TopAddress = (ushort)adr;
            dataPanel.Focus();
        }

        private void menuItemDataRefresh_Click(object sender, EventArgs e)
        {
            dataPanel.UpdateLines();
            Refresh();
        }

        private void menuItemDataSetColumnCount_Click(object sender, EventArgs e)
        {
            int cols = dataPanel.ColCount;
            var service = Locator.Resolve<IUserQuery>();
            if (!service.QueryValue("Data Panel Columns", "Column Count:", "{0}", ref cols, 1, 32)) return;
            dataPanel.ColCount = cols;
        }

        //Save disassembly
        private void menuItemSaveDisassembly_Click(object sender, EventArgs e)
        {
            string dissassembly = String.Empty;
            int addressFrom = dasmPanel.ActiveAddress;
            int addressTo = 0;
            var service = Locator.Resolve<IUserQuery>();
            if (!service.QueryValue("Save disassembly", "Address from:", "#{0:X2}", ref addressFrom, 0, 0xFFFF)) return;
            if (!service.QueryValue("Save disassembly", "Address to:", "#{0:X2}", ref addressTo, 0, 0xFFFF)) return;

            for( int counter = 0; ; )
            {
                int actualAddress = addressFrom + counter;
                if( actualAddress > addressTo )
                    break;

                int len;
                dissassembly += m_dasmTool.GetMnemonic(actualAddress, out len) + System.Environment.NewLine;
                counter += len;
            }

            if (dissassembly != String.Empty)
            {
                File.WriteAllText("dis.asm", dissassembly);
                Locator.Resolve<IUserMessage>().Info("File dis.asm saved!");
            }
            else
            {
                Locator.Resolve<IUserMessage>().Error("Nothing to save...!");
            }
        }

        //Save memory block as bytes(DEFB)
        private void menuItemSaveAsBytes_Click(object sender, EventArgs e)
        {
            string memBytes = String.Empty;
            int addressFrom = dasmPanel.ActiveAddress;
            int addressTo = 0;
            var service = Locator.Resolve<IUserQuery>();
            if (!service.QueryValue("Save memory bytes(DEFB)", "Address from:", "#{0:X2}", ref addressFrom, 0, 0xFFFF)) return;
            if (!service.QueryValue("Save memory bytes(DEFB)", "Address to:", "#{0:X2}", ref addressTo, 0, 0xFFFF)) return;

            for (int counter = 0; ; )
            {
                int actualAddress = addressFrom + counter;
                if (actualAddress >= addressTo)
                    break;

                if (counter % 8 == 0 || counter == 0)
                {
                    memBytes += "DEFB ";
                }

                memBytes += String.Format("#{0:X2}", m_spectrum.ReadMemory((ushort)actualAddress));

                if ((counter + 1) % 8 != 0 || counter == 0)
                {
                    if (actualAddress+1 < addressTo)
                        memBytes += ", ";
                }
                else
                    memBytes += System.Environment.NewLine;

                counter++;
            }

            if (memBytes != String.Empty)
            {
                File.WriteAllText("membytes.asm", memBytes);
                Locator.Resolve<IUserMessage>().Info("File membytes.asm saved!");
            }
            else
            {
                Locator.Resolve<IUserMessage>().Error("Nothing to save...!");
            }
        }

        //find bytes in memory
        private void menuItemFindBytes_Click(object sender, EventArgs e)
        {
            List<UInt16> bytesToFindInput = new List<UInt16>();
            var service = Locator.Resolve<IUserQuery>();

            if (sender != null) //null => Find next
            {
                if (!service.QueryText("Find bytes in memory", "Bytes(comma delimited):", ref m_strBytesToFindSave))
                    return;
            }
            if (m_strBytesToFindSave.Trim() == String.Empty || m_strBytesToFindSave.Trim().Length == 0)
                return;

            bytesToFindInput.Clear();
            foreach (string byteCandidate in Regex.Split(m_strBytesToFindSave, ","))
            {
                if (!String.IsNullOrEmpty(byteCandidate) && byteCandidate.Trim() != String.Empty && byteCandidate != ",")
                {
                    try
                    {
                        bytesToFindInput.Add(DebuggerManager.convertNumberWithPrefix(byteCandidate));
                    }
                    catch
                    {
                        Locator.Resolve<IUserMessage>().Error("Error in parsing the entered values!");
                    }
                }
            }

            if (bytesToFindInput.Count == 0)
                return;

            //finding the memory
            List<byte> bytesToFind = new List<byte>();
            bytesToFind.Clear();
            foreach( UInt16 word in bytesToFindInput )
            {
                if (word > 0xFFFF)
                {
                    Locator.Resolve<IUserMessage>().Error("Input value " + word.ToString() + " exceeded.");
                    return;
                }

                if (word > 0xFF)
                {
                    bytesToFind.Add((byte)(word / 256));
                    bytesToFind.Add((byte)(word % 256));
                }
                else
                    bytesToFind.Add((byte)word);
            }

            //search from actual address(dataPanel.TopAddress) until memory top(0xFFFF)
            for (ushort counter = (ushort)(dataPanel.TopAddress + 1); counter != 0; counter++)
            {
                if (m_spectrum.ReadMemory(counter) == bytesToFind[0]) //check 1. byte
                {
                    //check next bytes
                    bool bFound = true;
                    ushort actualAdress = (ushort)(counter + 1);

                    for (ushort counterNextBytes = 1; counterNextBytes < bytesToFind.Count; counterNextBytes++, actualAdress++)
                    {
                        if (m_spectrum.ReadMemory(actualAdress) != bytesToFind[counterNextBytes])
                        {
                            bFound = false;
                            break;
                        }
                    }

                    if (bFound)
                    {
                        dataPanel.TopAddress = counter;
                        return;
                    }
                }
            }

            //search from address 0 until actual address(dataPanel.TopAddress)
            for (ushort counter = 0; counter < (ushort)(dataPanel.TopAddress + 1); counter++)
            {
                if (m_spectrum.ReadMemory(counter) == bytesToFind[0]) //check 1. byte
                {
                    //check next bytes
                    bool bFound = true;
                    ushort actualAdress = (ushort)(counter + 1);

                    for (ushort counterNextBytes = 1; counterNextBytes < bytesToFind.Count; counterNextBytes++, actualAdress++)
                    {
                        if (m_spectrum.ReadMemory(actualAdress) != bytesToFind[counterNextBytes])
                        {
                            bFound = false;
                            break;
                        }
                    }

                    if (bFound)
                    {
                        dataPanel.TopAddress = counter;
                        return;
                    }
                }
            }
        }

        private void menuItemFindBytesNext_Click(object sender, EventArgs e)
        {
            menuItemFindBytes_Click(null, null);
        }

        private void dasmPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.menuItemDumpMemory.MenuItems.Clear();
                this.menuItemDumpMemory.MenuItems.Add(menuItemDumpMemoryAtCurrentAddress);

                ushort[] numbers = dasmPanel.GetNumberFromCpuInstruction_ActiveLine();
                if(numbers != null )
                {
                    //ToDo: numbers[] - question is whether there can be more numbers in one cpu instruction.
                    MenuItem menuItemNewAdress = new MenuItem();

                    menuItemNewAdress.Index = 0;
                    menuItemNewAdress.Text = String.Format("#{0:X4}", numbers[0]);
                    menuItemNewAdress.Click += new EventHandler(menuItemNewAdress_Clicked);

                    this.menuItemDumpMemory.MenuItems.Add(menuItemNewAdress);
                }

                contextMenuDasm.Show(dasmPanel, e.Location);
            }
        }
        private void menuItemNewAdress_Clicked(object sender, EventArgs e)
        {
            if (sender is MenuItem) //must be a MenuItem
            {
                string hex = ((MenuItem)sender).Text;
                dataPanel.TopAddress = ushort.Parse(hex.Substring(1, hex.Length - 1), System.Globalization.NumberStyles.HexNumber);
            }
        }

        private void dasmPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.X > 30/*fGutterWidth*/)
                dbgCmdLine.Text += "#" + dasmPanel.ActiveAddress.ToString("X4");
        }

        private void dataPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuData.Show(dataPanel, e.Location);
        }

        private void listState_DoubleClick(object sender, EventArgs e)
        {
            if (!showStack) // if we are in breakpoint mode only
            {
                int selectedIndex = listState.SelectedIndex;
                if (selectedIndex < 0 || GetExtBreakpointsList().Count == 0) return;
                if (selectedIndex + 1 > GetExtBreakpointsList().Count) return;

                string strTemp = listState.Items[selectedIndex].ToString();
                int index = strTemp.IndexOf(':');
                string key = String.Empty;
                if (index > 0)
                    key = strTemp.Substring(0, index);

                bool isBreakpointIsOn = GetExtBreakpointsList()[Convert.ToByte(key)].Info.IsOn;
                EnableOrDisableBreakpointStatus(Convert.ToByte(key), !isBreakpointIsOn);
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
                var service = Locator.Resolve<IUserQuery>();
                if (service == null)
                {
                    return;
                }
                if (!service.QueryValue("Load Block", "Memory Address:", "#{0:X4}", ref s_addr, 0, 0xFFFF))
                    return;
                if (!service.QueryValue("Load Block", "Block Length:", "#{0:X4}", ref s_len, 0, 0x10000))
                    return;

                byte[] data = new byte[s_len];
                using (FileStream fs = new FileStream(loadDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    fs.Read(data, 0, data.Length);
                m_spectrum.WriteMemory((ushort)s_addr, data, 0, s_len);
            }
        }

        private void menuSaveBlock_Click(object sender, EventArgs e)
        {
            var service = Locator.Resolve<IUserQuery>();
            if (!service.QueryValue("Save Block", "Memory Address:", "#{0:X4}", ref s_addr, 0, 0xFFFF))
                return;
            if (!service.QueryValue("Save Block", "Block Length:", "#{0:X4}", ref s_len, 0, 0x10000))
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

        private void Bus_OnBeforeCpuCycle(int frameTact)
        {
            if (!m_isTracing || m_cpuRegs.PC < 0x4000) //no need to trace ROM
                return;
            
            //byte byteOnAddr = m_spectrum.ReadMemory(m_cpuRegs.PC);
            if (Array.Exists(DebuggerTrace.ConditionalJumps, p => p == m_spectrum.ReadMemory(m_cpuRegs.PC)))
            {
                //is addr allowed?
                //if( m_cpuRegs.PC!= 0xCE14 )
                {
                    var len = 0;
                    var mnemonic = m_dasmTool.GetMnemonic(m_cpuRegs.PC, out len);
                    Logger.Debug("#{0:X4}   {1}", m_cpuRegs.PC, mnemonic);
                }
            }
        }

        private void dbgCmdLine_KeyUp(object sender, KeyEventArgs e)
        {
            //ToDo: process always lands here after changing TopAdress(menuItemDasmGotoADDR_Click()) in dasm panel(pressing enter)....must be resolved.
            //Debug command entered ?
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    string actualCommand = dbgCmdLine.Text;

                    List<string> parsedCommand = DebuggerManager.ParseCommand(actualCommand);
                    if (parsedCommand == null || parsedCommand.Count == 0)
                        return;

                    DebuggerManager.CommandType commandType = DebuggerManager.getDbgCommandType(parsedCommand);

                    if (commandType == DebuggerManager.CommandType.Unidentified)
                        throw new Exception("unknown debugger command"); // unknown cmd line type

                    //breakpoint manipulation ?
                    if (commandType == DebuggerManager.CommandType.breakpointManipulation)
                    {
                        // add new enhanced breakpoint
                        string left = parsedCommand[1];

                        //left side must be registry or memory reference
                        if (   !DebuggerManager.isRegistry(left)
                            && !DebuggerManager.isFlag(left) 
                            && !DebuggerManager.isMemoryReference(left) 
                            && left != "memread"
                           )
                            throw new CommandParseException("bad condition !");

                        AddExtBreakpoint(parsedCommand); // add breakpoint, send parsed command e.g.: br pc == #0000

                        showStack = false; // show breakpoint list on listState panel
                    }
                    else if (commandType == DebuggerManager.CommandType.gotoAdress)
                    {
                        // goto adress to dissasembly
                        dasmPanel.TopAddress = DebuggerManager.convertNumberWithPrefix(parsedCommand[1]);
                    }
                    else if (commandType == DebuggerManager.CommandType.removeBreakpoint)
                    {
                        // remove breakpoint
                        if (parsedCommand.Count > 1)
                            RemoveExtBreakpoint(parsedCommand[1]);
                    }
                    else if (commandType == DebuggerManager.CommandType.enableBreakpoint)
                    {
                        //enable breakpoint
                        EnableOrDisableBreakpointStatus(Convert.ToByte(DebuggerManager.convertNumberWithPrefix(parsedCommand[1])), true);
                    }
                    else if (commandType == DebuggerManager.CommandType.disableBreakpoint)
                    {
                        //disable breakpoint
                        EnableOrDisableBreakpointStatus(Convert.ToByte(DebuggerManager.convertNumberWithPrefix(parsedCommand[1])), false);
                    }
                    else if (commandType == DebuggerManager.CommandType.loadBreakpointsListFromFile)
                    {
                        //load breakpoints list into debugger
                        LoadBreakpointsListFromFile(parsedCommand[1]);

                        showStack = false;
                    }
                    else if (commandType == DebuggerManager.CommandType.saveBreakpointsListToFile)
                    {
                        //save breakpoints list into debugger
                        SaveBreakpointsListToFile(parsedCommand[1]);
                    }
                    else if (commandType == DebuggerManager.CommandType.showAssembler)
                    {
                        m_spectrum.DoStop();
                        UpdateCPU(true);

                        Assembler.Show(ref m_spectrum);
                        Assembler.ActiveForm.Focus();

                        return;
                    }
                    else if (commandType == DebuggerManager.CommandType.showGraphicsEditor)
                    {
                        m_spectrum.DoStop();
                        UpdateCPU(true);

                        GraphicsEditor.Show(ref m_spectrum);
                        GraphicsEditor.ActiveForm.Focus();

                        return;
                    }
                    else if (commandType == DebuggerManager.CommandType.traceLog)
                    {
                        if (parsedCommand.Count < 2)
                            throw new Exception("Incorrect trace command syntax! Missing On or Off.");

                        if (parsedCommand[1].ToUpper() == "ON")
                        {
                            m_isTracing = true;
                            //Logger.Start();
                        }
                        else
                        {
                            m_isTracing = false;
                            //Logger.Finish();
                        }
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
                        if (DebuggerManager.isMemoryReference(left))
                        {
                            leftNum = DebuggerManager.getReferencedMemoryPointer(left);

                            isLeftMemoryReference = true;
                        }
                        else
                        {
                            // is it register ?
                            if (DebuggerManager.isRegistry(left))
                            {
                                leftNum = DebuggerManager.getRegistryValueByName(m_spectrum.CPU.regs, left);
                                isLeftRegistry = true;
                            }
                            else
                                leftNum = DebuggerManager.convertNumberWithPrefix(left);
                        }

                        //Reading values - right side of statement
                        if (DebuggerManager.isMemoryReference(right))
                        {
                            rightNum = DebuggerManager.getReferencedMemoryPointer(right);

                            isRightMemoryReference = true;
                        }
                        else
                        {
                            // is it register ?
                            if (DebuggerManager.isRegistry(right))
                            {
                                rightNum = DebuggerManager.getRegistryValueByName(m_spectrum.CPU.regs, right);
                                isRightRegistry = true;
                            }
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
                                UInt16 regValue = DebuggerManager.getRegistryValueByName(m_spectrum.CPU.regs, right);
                                if (regValue <= Byte.MaxValue)
                                    m_spectrum.WriteMemory(leftNum, Convert.ToByte(regValue));
                                else
                                {
                                    //2 bytes will be written; ToDo: check on adress if it is not > 65535
                                    byte hiBits = Convert.ToByte(regValue / 256);
                                    byte loBits = Convert.ToByte(regValue - hiBits * 256);

                                    m_spectrum.WriteMemory(leftNum, loBits);
                                    leftNum++;
                                    m_spectrum.WriteMemory(leftNum, hiBits);
                                }
                            }
                            else
                            {
                                bool isWritingCharacters = false;
                                for (int counter = 2; parsedCommand.Count > counter; counter++)
                                {
                                    string currExpr = parsedCommand[counter];
                                    if (currExpr[0] == '"')
                                        isWritingCharacters = true;
                                    if (isWritingCharacters)
                                    {
                                        // e.g.: ld (<memAddr>), "something to write to mem"
                                        byte[] arrToWriteToMem = DebuggerManager.convertASCIIStringToBytes(currExpr);
                                        m_spectrum.WriteMemory(leftNum, arrToWriteToMem, 0, arrToWriteToMem.Length);
                                        leftNum += (ushort)arrToWriteToMem.Length;
                                        if (currExpr.EndsWith("\""))
                                            isWritingCharacters = false;
                                        else
                                        {
                                            m_spectrum.WriteMemory(leftNum, 0x20 /*space*/);
                                            leftNum++;
                                        }
                                    }
                                    else
                                    {
                                        // e.g.: ld (#9C40), #21 #33 3344 .. .. .. -> x
                                        rightNum = DebuggerManager.convertNumberWithPrefix(currExpr);
                                        if (rightNum <= Byte.MaxValue)
                                        {
                                            m_spectrum.WriteMemory(leftNum, Convert.ToByte(rightNum));
                                            leftNum++;
                                        }
                                        else
                                        {
                                            //2 bytes will be written; ToDo: check on adress if it is not > 65535
                                            byte hiBits = Convert.ToByte(rightNum / 256);
                                            byte loBits = Convert.ToByte(rightNum % 256);

                                            m_spectrum.WriteMemory(leftNum, hiBits);
                                            leftNum++;
                                            m_spectrum.WriteMemory(leftNum, loBits);
                                            leftNum++;
                                        }
                                    }
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

                    //command line history
                    m_cmdLineHistory.Add(actualCommand);
                    this.m_cmdLineHistoryPos++;

                    UpdateREGS();
                    UpdateCPU(false);

                    dbgCmdLine.SelectAll();
                    dbgCmdLine.Focus();
                }
                catch (Exception exc)
                {
                    //Logger.Error(exc);
                    string saveCmdLineString = dbgCmdLine.Text;
                    dbgCmdLine.BackColor = Color.Red;
                    dbgCmdLine.ForeColor = Color.Black;
                    dbgCmdLine.Text = exc.Message;
                    dbgCmdLine.Refresh();
                    System.Threading.Thread.Sleep(140);
                    dbgCmdLine.BackColor = Color.White;
                    dbgCmdLine.ForeColor = Color.Black;
                    dbgCmdLine.Text = saveCmdLineString;
                }
            }
            else if (e.KeyCode == Keys.Up && this.m_cmdLineHistory.Count != 0) //arrow up - history of command line
            {
                if (this.m_cmdLineHistoryPos < (this.m_cmdLineHistory.Count - 1))
                {
                    this.dbgCmdLine.Text = this.m_cmdLineHistory[++m_cmdLineHistoryPos];
                }
                else
                {
                    this.m_cmdLineHistoryPos = 0;
                    this.dbgCmdLine.Text = this.m_cmdLineHistory[this.m_cmdLineHistoryPos];
                }
                dbgCmdLine.Select(this.dbgCmdLine.Text.Length, 0);
                dbgCmdLine.Focus();
                e.Handled = true;
                return;
            }
            else if (e.KeyCode == Keys.Down && this.m_cmdLineHistory.Count != 0) //arrow down - history of command line
            {
                if (this.m_cmdLineHistoryPos != 0)
                {
                    this.dbgCmdLine.Text = this.m_cmdLineHistory[--m_cmdLineHistoryPos];
                }
                else
                {
                    this.m_cmdLineHistoryPos = this.m_cmdLineHistory.Count - 1;
                    this.dbgCmdLine.Text = this.m_cmdLineHistory[this.m_cmdLineHistoryPos];
                }
                dbgCmdLine.Select(this.dbgCmdLine.Text.Length, 0);
                dbgCmdLine.Focus();
                e.Handled = true;
                return;
            }
            else if (this.dbgCmdLine.Text == "lo") //shortcut
            {
                this.dbgCmdLine.Text = "loadbrs ";
                dbgCmdLine.Select(8, 0);
                dbgCmdLine.Focus();
                return;
            }
            else if (this.dbgCmdLine.Text == "sa") //shortcut
            {
                this.dbgCmdLine.Text = "savebrs ";
                dbgCmdLine.Select(8, 0);
                dbgCmdLine.Focus();
                return;
            }
        }

        #region 2.) Extended breakpoints(conditional on memory change, write, registry change, ...)

        //conditional breakpoints
        private DictionarySafe<byte, BreakpointAdlers> _breakpointsExt = null;

        public bool CheckIsBrkMulticonditional(List<string> newBreakpointDesc)
        {
            if (newBreakpointDesc.Count != 8) //multicondional breakpoint; example: br pc == <number> && a == 5
                return false;

            return newBreakpointDesc[4] == "&&";
        }

        public void FillBreakpointConditionData(List<string> i_newBreakpointDesc, int i_brkIndex, ref BreakpointInfo o_breakpointInfo)
        {
            //1.LEFT condition
            bool leftIsMemoryReference = false;
            string leftExpr = i_newBreakpointDesc[i_brkIndex];
            if (DebuggerManager.isMemoryReference(leftExpr))
            {
                o_breakpointInfo.LeftCondition = leftExpr.ToUpper();

                // it can be memory reference by registry value, e.g.: (PC), (DE), ...
                if (DebuggerManager.isRegistryMemoryReference(leftExpr))
                    o_breakpointInfo.LeftRegistryArrayIndex = DebuggerManager.getRegistryArrayIndex(DebuggerManager.getRegistryFromReference(leftExpr));
                else
                    o_breakpointInfo.LeftValue = DebuggerManager.getReferencedMemoryPointer(leftExpr);

                leftIsMemoryReference = true;
            }
            else
            {
                //must be a registry or flag
                if (DebuggerManager.isRegistry(leftExpr))
                {
                    o_breakpointInfo.LeftCondition = leftExpr.ToUpper();
                    o_breakpointInfo.LeftRegistryArrayIndex = DebuggerManager.getRegistryArrayIndex(o_breakpointInfo.LeftCondition);
                    if (leftExpr.Length == 1) //8 bit registry
                        o_breakpointInfo.Is8Bit = true;
                    else
                        o_breakpointInfo.Is8Bit = false;
                }
                else if( DebuggerManager.isFlag(leftExpr) )
                {
                    o_breakpointInfo.LeftCondition = leftExpr.ToUpper();
                    o_breakpointInfo.LeftIsFlag = true;
                }
                else
                {
                    throw new CommandParseException("Incorrect syntax(left expression)!");
                }
            }

            //2.CONDITION type
            o_breakpointInfo.ConditionTypeSign = i_newBreakpointDesc[i_brkIndex+1]; // ==, !=, <, >, ...
            if (o_breakpointInfo.ConditionTypeSign == "==")
                o_breakpointInfo.IsConditionEquals = true;

            //3.RIGHT condition
            byte rightType = 0xFF; // 0 - memory reference, 1 - registry value, 2 - common value

            string rightExpr = i_newBreakpointDesc[i_brkIndex+2];
            if (DebuggerManager.isMemoryReference(rightExpr))
            {
                o_breakpointInfo.RightCondition = rightExpr.ToUpper(); // because of breakpoint panel
                o_breakpointInfo.RightValue = m_spectrum.ReadMemory(DebuggerManager.getReferencedMemoryPointer(rightExpr));

                rightType = 0;
            }
            else
            {
                if (DebuggerManager.isRegistry(rightExpr))
                {
                    o_breakpointInfo.RightCondition = rightExpr;

                    rightType = 1;
                }
                else
                {
                    //it has to be a common value, e.g.: #4000, %111010101, ...
                    o_breakpointInfo.RightCondition = rightExpr.ToUpper(); // because of breakpoint panel
                    o_breakpointInfo.RightValue = DebuggerManager.convertNumberWithPrefix(rightExpr); // last chance

                    rightType = 2;
                }
            }

            if (rightType == 0xFF)
                throw new CommandParseException("Incorrect right expression!");
            if (o_breakpointInfo.LeftIsFlag && rightType != 2)
                throw new CommandParseException("Flags allows only true/false right expression...");

            //4. finish
            if (leftIsMemoryReference)
            {
                if (DebuggerManager.isRegistryMemoryReference(o_breakpointInfo.LeftCondition)) // left condition is e.g.: (PC), (HL), (DE), ...
                {
                    if (rightType == 2) // right is number
                        o_breakpointInfo.AccessType = BreakPointConditionType.registryMemoryReferenceVsValue;
                }
            }
            else
            {
                if (rightType == 2)
                {
                    o_breakpointInfo.AccessType = o_breakpointInfo.LeftIsFlag ?  BreakPointConditionType.flagVsValue : BreakPointConditionType.registryVsValue;
                }
            }
        } //end FillBreakpointData()

        public void AddExtBreakpoint(List<string> newBreakpointDesc)
        {
            if (_breakpointsExt == null)
                _breakpointsExt = new DictionarySafe<byte, BreakpointAdlers>();

            BreakpointInfo breakpointInfo = new BreakpointInfo();
            breakpointInfo.IsMulticonditional = CheckIsBrkMulticonditional(newBreakpointDesc);

            //0. memory read breakpoint ?
            if (newBreakpointDesc[1].ToUpper() == "MEMREAD")
            {
                if (newBreakpointDesc.Count == 3) //e.g.: "br memread #4000"
                {
                    breakpointInfo.IsOn = true;
                    breakpointInfo.AccessType = BreakPointConditionType.memoryRead;
                    breakpointInfo.LeftValue = DebuggerManager.convertNumberWithPrefix(newBreakpointDesc[2]); // last chance
                    InsertNewBreakpoint(breakpointInfo);
                }
                else
                    throw new CommandParseException("Incorrect memory check breakpoint syntax...");

                return;
            }

            //fill first condition
            FillBreakpointConditionData(newBreakpointDesc, 1, ref breakpointInfo);

            //let emit CIL code to check the breakpoint
            Func<bool> checkBreakpoint = ILProcessor.EmitCondition(ref m_spectrum, breakpointInfo);
            if (checkBreakpoint != null)
                breakpointInfo.SetBreakpointCheckMethod(checkBreakpoint);

            //multiconditional breakpoint ?
            if (breakpointInfo.IsMulticonditional)
            {
                //second condition
                FillBreakpointConditionData(newBreakpointDesc, 5, ref breakpointInfo);
                checkBreakpoint = ILProcessor.EmitCondition(ref m_spectrum, breakpointInfo);
                breakpointInfo.SetCheckSecondCondition(checkBreakpoint);
            }

            breakpointInfo.IsOn = true; // activate the breakpoint

            //save breakpoint command line string
            breakpointInfo.BreakpointString = String.Empty;
            for (byte counter = 1; counter < newBreakpointDesc.Count; counter++)
            {
                breakpointInfo.BreakpointString += newBreakpointDesc[counter];
            }

            InsertNewBreakpoint(breakpointInfo);
        }
        public void RemoveExtBreakpoint(string brkIndex)
        {
            if (_breakpointsExt == null || _breakpointsExt.Count == 0)
                throw new Exception("No breakpoints...!");

            if (brkIndex.ToUpper() == "ALL")
            {
                _breakpointsExt.Clear();
                m_spectrum.ClearBreakpoints();
                UpdateREGS();
            }
            else
            {
                byte index = Convert.ToByte(brkIndex);
                if (_breakpointsExt.ContainsKey(Convert.ToByte(index)))
                {
                    Breakpoint bp = _breakpointsExt[index];
                    _breakpointsExt.Remove(index);
                    m_spectrum.RemoveBreakpoint(bp);
                }
                else
                    throw new Exception(String.Format("No breakpoint with index {0} !", index));
            }
        }
        public DictionarySafe<byte, BreakpointAdlers> GetExtBreakpointsList()
        {
            if (_breakpointsExt != null)
                return _breakpointsExt;

            _breakpointsExt = new DictionarySafe<byte, BreakpointAdlers>();

            return _breakpointsExt;
        }
        private void InsertNewBreakpoint(BreakpointInfo info)
        {
            // ADD breakpoint into list
            // Here will be the breakpoint key assigned by searching keys starting with key 0
            // Maximum 255 breakpoints is allowed
            if (_breakpointsExt.Count < 255)
            {
                var bp = new BreakpointAdlers(info);
                _breakpointsExt.Add(GetNewBreakpointPosition(), bp);
                m_spectrum.AddBreakpoint(bp);
            }
            else
                throw new CommandParseException("Maximum breakpoints count(255) exceeded...");
        }
        private byte GetNewBreakpointPosition()
        {
            if( _breakpointsExt.Count == 0 )
                return 0;

            for (byte positionCounter = 0; positionCounter < _breakpointsExt.Count; positionCounter++)
                if (!_breakpointsExt.ContainsKey(positionCounter))
                    return positionCounter;

            return Convert.ToByte(_breakpointsExt.Count);
        }

        #region Read and Write Mem check methods
        public void CheckWriteMem(ushort addr, byte value)
        {
            if (_breakpointsExt == null)
                return;

            foreach (BreakpointAdlers brk in _breakpointsExt.Values)
            {
                //here would be nice to use select x from _breakpointsExt where ..., but cannot use Linq(.Net Framework 2.0 is used)
                if (brk.Info.IsOn &&
                    (brk.Info.AccessType == BreakPointConditionType.memoryVsValue || brk.Info.AccessType == BreakPointConditionType.registryMemoryReferenceVsValue))
                {
                    brk.IsNeedWriteMemoryCheck = true;
                }
            }

            return;
        }
        public void CheckReadMem(ushort addr, ref byte value)
        {
            if (_breakpointsExt == null)
                return;

            foreach (BreakpointAdlers brk in _breakpointsExt.Values)
            {
                //here would be nice to use select x from _breakpointsExt where ..., but cannot use Linq(.Net Framework 2.0 is used)
                if (brk.Info.IsOn && brk.Info.AccessType == BreakPointConditionType.memoryRead)
                {
                    if (brk.Info.LeftValue == addr)
                    {
                        // raise force stop at the end of the currect CPU cycle
                        // (this flag will be checked from BreakpointAdlers.Check at the end of CPU cycle)
                        brk.IsForceStop = true;
                    }
                }
            }

            return;
        }
        #endregion

        public void EnableOrDisableBreakpointStatus(byte whichBpToEnableOrDisable, bool setOn) //enables/disables breakpoint, command "on" or "off"
        {
            if (_breakpointsExt == null || _breakpointsExt.Count == 0)
                return;

            if (!_breakpointsExt.ContainsKey(whichBpToEnableOrDisable))
                return;

            BreakpointAdlers temp = _breakpointsExt[whichBpToEnableOrDisable];
            temp.Info.IsOn = setOn;
            return;
        }

        public void LoadBreakpointsListFromFile(string fileName)
        {
            System.IO.StreamReader file = null;
            try
            {
                if (!File.Exists(Path.Combine(GetAppRootDir(), fileName)))
                    throw new Exception("file " + fileName + " does not exists...");

                string dbgCommandFromFile = String.Empty;
                file = new System.IO.StreamReader(Path.Combine(GetAppRootDir(), fileName));
                while ((dbgCommandFromFile = file.ReadLine()) != null)
                {
                    if (dbgCommandFromFile.Trim() == String.Empty || dbgCommandFromFile[0] == ';')
                        continue;

                    List<string> parsedCommand = DebuggerManager.ParseCommand("br " + dbgCommandFromFile);
                    if (parsedCommand == null)
                        throw new Exception("unknown debugger command");

                    AddExtBreakpoint(parsedCommand);
                }
            }
            finally
            {
                if (file != null)
                    file.Close();
            }
        }

        public void SaveBreakpointsListToFile(string fileName)
        {
            DictionarySafe<byte, BreakpointAdlers> localBreakpointsList = GetExtBreakpointsList();
            if (localBreakpointsList.Count == 0)
                return;

            System.IO.StreamWriter file = null;
            try
            {
                file = new System.IO.StreamWriter(Path.Combine(GetAppRootDir(), fileName));

                foreach (KeyValuePair<byte, BreakpointAdlers> breakpoint in localBreakpointsList)
                {
                    file.WriteLine(breakpoint.Value.Info.BreakpointString);
                }
            }
            finally
            {
                file.Close();
            }
        }

        #endregion

        #region Trace GUI methods
        private void checkBoxOpcode_CheckedChanged(object sender, EventArgs e)
        {
            textBoxOpcode.Enabled = checkBoxOpcode.Checked;
        }
        private void checkBoxConditionalJumps_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConditionalJumps.Checked)
                checkBoxAllJumps.Checked = false;
        }
        private void checkBoxTraceFileOut_CheckedChanged(object sender, EventArgs e)
        {
            buttonSetTraceFileName.Enabled = textBoxTraceFileName.Enabled = checkBoxTraceFileOut.Checked;
        }
        private void btnStartTrace_Click(object sender, EventArgs e)
        {
            //Trace start
            btnStartTrace.Enabled = false;
            btnStopTrace.Enabled = true;

            m_debuggerTrace.StartTrace(this);

            m_isTracing = true;
        }
        private void btnStopTrace_Click(object sender, EventArgs e)
        {
            //Trace stop
            btnStartTrace.Enabled = true;
            btnStopTrace.Enabled = false;

            m_isTracing = false;
        }
        #endregion
    }
}