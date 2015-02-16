using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZXMAK2.Dependency;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.Adlers.Views;
using ZXMAK2.Host.Interfaces;

namespace ZXMAK2.Hardware.Adlers.Core
{
    public class DebuggerTrace
    {
        #region members
        public static readonly byte[] ConditionalCalls = new byte[] {
                                                             //CALL`s
                                                             0xC4, //CALL NZ,nn
                                                             0xCC, //CALL Z,nn
                                                             0xD4, //CALL NC,nn
                                                             0xDC, //CALL C,nn
                                                             0xE4, //CALL PO,nn
                                                             0xEC, //CALL PE,nn
                                                             0xF4, //CALL P,nn
                                                             0xFC  //CALL M,nn*
                                                             };
        public static readonly byte[] ConditionalJumps = new byte[] {
                                                             //JR`s
                                                             0x20, //JR NZ,d
                                                             0x28, //JR Z,d
                                                             0x30, //JR NC,d
                                                             0x38, //JR C,d
                                                             //JP`s
                                                             0xC2, //JP NZ,nn
                                                             0xCA, //JP Z,nn
                                                             0xD2, //JP NC,nn
                                                             0xDA, //JP C,nn
                                                             0xE2, //JP PO,nn
                                                             0xEA, //JP PE,nn
                                                             0xF2, //JP P,nn
                                                             0xFA //JP M,nn
                                                           };
        public static readonly byte[] CommonJumps = new byte[] { 
                                                             0x18, //JR d
                                                             0xC9, //RET
                                                             0xC3, //JP nn
                                                             0xCD, //CALL nn
                                                             0xE9, //JP (HL)
                                                           };
        private IDebuggable m_spectrum;

        private readonly object m_sync = new object();

        private int[] _counters = null;

        private bool[] m_addrsFlags; //false => address is excluded from tracing
        private byte[] m_currentTraceOpcodes = null;

        //Trace filter
        private bool m_isTraceFilterDefined = false;
        private bool m_isTracingJumps = false;
        private bool m_isTraceAreaDefined = false;

        //Opcode tracing
        private bool m_isTracingOpcode = false;
        private byte m_tracedOpcode;

        private string _traceLogFilename;
        #endregion

        public DebuggerTrace(IDebuggable i_spectrum)
        {
            m_spectrum = i_spectrum;

            m_addrsFlags = new bool[65536];
            ResetAddrsFlags();
        }

        public bool StartTrace(FormCpu i_form)
        {
            lock (m_sync)
            {
                SetTraceOpcodes(i_form);
                SetTraceArea(i_form);

                if (!ValidateTrace(i_form))
                    return false;

                _counters = new int[65536];
                _traceLogFilename = i_form.textBoxTraceFileName.Text.Trim();

                return true;
            }
        }
        public void StopTrace()
        {
            lock (m_sync)
            {
                m_isTracingJumps = false;
                m_isTraceAreaDefined = false;
                m_isTracingOpcode = false;

                if (!m_isTraceFilterDefined) //without filtering each instruction is written
                    return;

                //save counters to file
                int[] countersOut = _counters.Select((s, index) => new { s, index })
                                             .Where(x => x.s > 0)
                                             .Select(x => x.index)
                                             .ToArray();
                int totalOccurences = 0;
                Array.Sort(countersOut);
                string traceCountersLog = String.Empty;
                foreach (int counterItem in countersOut)
                {
                    traceCountersLog += String.Format("Addr: #{0:X4}   Trace occurences: {1}\n", counterItem, _counters[counterItem]);
                    totalOccurences += _counters[counterItem];
                }

                string sumLine = String.Format("Total addresses: {0}   Total occurences: {1}", countersOut.Length, totalOccurences);
                traceCountersLog += new String('=', sumLine.Length) + "\n";
                traceCountersLog += String.Format("Total addresses: {0}   Total occurences: {1}", countersOut.Length, totalOccurences);

                File.WriteAllText(Path.Combine(Utils.GetAppFolder(), _traceLogFilename), traceCountersLog);

                _counters = null;
            }
        }

        public bool ValidateTrace(FormCpu i_form) //returns false when trace failed
        {
            //opcode filter
            if(i_form.checkBoxOpcode.Checked)
            {
                //bool error = false;
                if (i_form.textBoxOpcode.Text.Trim() == String.Empty)
                {
                    Locator.Resolve<IUserMessage>().Error("Filtering by opcode, but opcode not defined.");
                    i_form.textBoxOpcode.Focus();
                    return false;
                }
                if( ParseTracedOpcode(i_form, true ) == false )
                {
                    Locator.Resolve<IUserMessage>().Error("Opcode has incorrect number.");
                    i_form.textBoxOpcode.Focus();
                    return false;
                }
            }


            //trace area
            if ((!m_isTraceAreaDefined && !m_isTracingJumps && !m_isTracingOpcode)
                &&
                (!m_addrsFlags.Contains(true) && i_form.checkBoxTraceArea.Checked) //no valid addr to trace
               )
            {
                m_isTraceFilterDefined = false;

                //Trace everything, are you sure ?
                var service = Locator.Resolve<IUserQuery>();
                if (service == null)
                    return true;

                if( service.Show( "Because no trace filter/s is defined the\nemulation will be very slow.\n\nConsole output is allowed only when\nno trace filter is defined.\n\nAre you sure?",
                                  "Trace: Performance warning",
                                  ZXMAK2.Host.Entities.DlgButtonSet.YesNo,
                                  ZXMAK2.Host.Entities.DlgIcon.Warning )
                               != ZXMAK2.Host.Entities.DlgResult.Yes )
                    return false;
            }
            else
                m_isTraceFilterDefined = true;

            return true; //ok; if false => do not start tracing
        }

        private void SetTraceOpcodes(FormCpu i_form)
        {
            m_currentTraceOpcodes = null;

            if (i_form.checkBoxAllJumps.Checked && i_form.checkBoxAllJumps.Enabled)
            {
                m_currentTraceOpcodes = new byte[CommonJumps.Length + ConditionalJumps.Length];
                m_currentTraceOpcodes = CommonJumps.Union(ConditionalJumps).ToArray();
                m_currentTraceOpcodes = m_currentTraceOpcodes.Union(ConditionalCalls).ToArray();

                m_isTracingJumps = true;
            }
            else if (i_form.checkBoxConditionalJumps.Checked && i_form.checkBoxConditionalJumps.Enabled)
            {
                m_currentTraceOpcodes = ConditionalJumps;
                m_isTracingJumps = true;
            }
            else if (i_form.checkBoxConditionalCalls.Checked && i_form.checkBoxConditionalCalls.Enabled)
            {
                m_currentTraceOpcodes = ConditionalCalls;
                m_isTracingJumps = true;
            }
            else
                m_isTracingJumps = false;

            if (i_form.checkBoxOpcode.Checked)
            {
                m_isTracingOpcode = true;
            }
            else
                m_isTracingOpcode = false;
        }
        private void SetTraceArea(FormCpu i_form)
        {
            if (i_form.listViewAdressRanges.Items.Count == 0 || i_form.checkBoxTraceArea.Checked == false)
            {
                m_isTraceAreaDefined = false;
                return;
            }

            Array.Clear(m_addrsFlags, 0, m_addrsFlags.Length);

            foreach (ListViewItem item in i_form.listViewAdressRanges.Items)
            {
                string[] tags = ((string)item.Tag).Split(new char[] { ';' });
                if (tags.Length != 3)
                    continue;

                //setting
                int addrFrom = int.Parse(tags[0], System.Globalization.NumberStyles.HexNumber);
                int addrTo = int.Parse(tags[1], System.Globalization.NumberStyles.HexNumber);
                for (; addrFrom <= addrTo; addrFrom++)
                    m_addrsFlags[addrFrom] = (tags[2] == "Yes");
            }

            m_isTraceAreaDefined = true;
        }

        public byte[] GetTraceOpcodes()
        {
            return m_currentTraceOpcodes;
        }
        public bool[] GetAddressFlags()
        {
            return m_addrsFlags;
        }
        public bool IsTraceFilterDefined()
        {
            return m_isTraceFilterDefined;
        }
        public bool IsTracingJumps()
        {
            return m_isTracingJumps;
        }
        public bool IsTraceAreaDefined()
        {
            return m_isTraceAreaDefined;
        }
        public bool IsTracingOpcode()
        {
            return m_isTracingOpcode;
        }
        public byte GetTracedOpcode()
        {
            return m_tracedOpcode;
        }
        public void SetTracedOpcode(byte i_opcode)
        {
            m_tracedOpcode = i_opcode;
        }
        public void IncCounter(int i_memPointer)
        {
            _counters[i_memPointer]++;
        }
        public bool ParseTracedOpcode(FormCpu i_form, bool i_justParse = false)
        {
            try
            {
                UInt16 opcode = DebuggerManager.convertNumberWithPrefix(i_form.textBoxOpcode.Text.Trim());
                if (opcode > 0xFF) //ToDo: only one byte for traced opcode
                    SetTracedOpcode((byte)(opcode % 256));
                else
                    SetTracedOpcode((byte)opcode);
            }
            catch (CommandParseException)
            {
                if (!i_justParse)
                {
                    Locator.Resolve<IUserMessage>().Error("Incorrect opcode number...");
                    i_form.textBoxOpcode.Focus();
                }
                return false;
            }

            return true;
        }

        #region GUI handlers/methods
        public void AddNewAddrArea(FormCpu i_form)
        {
            int FromAddr = 0;
            int ToAddr = 0;
            var service = Locator.Resolve<IUserQuery>();
            if (service == null)
            {
                return;
            }
            if (!service.QueryValue("Address area", "From:", "#{0:X4}", ref FromAddr, 0, 0xFFFF)) return;
            ToAddr = FromAddr;
            if (!service.QueryValue("Address area", "To:", "#{0:X4}", ref ToAddr, FromAddr, 0xFFFF)) return;

            ListViewItem item = new ListViewItem(new[] { String.Format("#{0:X4}", FromAddr), String.Format("#{0:X4}", ToAddr), "No" });
            item.Tag = String.Format("{0:X4};{1:X4};No", FromAddr, ToAddr);
            i_form.listViewAdressRanges.Items.Add(item);

            i_form.listViewAdressRanges.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            i_form.listViewAdressRanges.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            i_form.listViewAdressRanges.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        public void UpdateNewAddrArea(FormCpu i_form)
        {
            //this will only toggle Yes/No
            if (i_form.listViewAdressRanges.FocusedItem.Index < 0)
                return;

            ListViewItem itemToUpdate = i_form.listViewAdressRanges.Items[i_form.listViewAdressRanges.FocusedItem.Index];
            string strNewTraceStatus;
            string[] tags = ((string)itemToUpdate.Tag).Split(new char[] { ';' });
            if (tags.Length != 3)
                return;

            strNewTraceStatus = (tags[2] == "Yes" ? "No" : "Yes");
            ListViewItem item = new ListViewItem(new[] { "#" + tags[0], "#" + tags[1], strNewTraceStatus });
            item.Tag = tags[0] + ";" + tags[1] + ";" + strNewTraceStatus;
            i_form.listViewAdressRanges.Items[i_form.listViewAdressRanges.FocusedItem.Index] = item;
        }
        #endregion

        private void ResetAddrsFlags()
        {
            Array.Clear(m_addrsFlags, 0, m_addrsFlags.Length);
        }
    }
}
