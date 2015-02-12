using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.Adlers.Views;

namespace ZXMAK2.Hardware.Adlers.Core
{
    public class DebuggerTrace
    {
        #region members
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
                                                             0xFA, //JP M,nn
                                                             //CALL`s
                                                             0xC4, //CALL NZ,nn
                                                             0xCC, //CALL Z,nn
                                                             0xD4, //CALL NC,nn
                                                             0xDC, //CALL C,nn
                                                             0xE4, //CALL PO,nn
                                                             0xEC, //CALL PE,nn
                                                             0xF4, //CALL P,nn
                                                             0xFC  //CALL M,nn
                                                           };
        public static readonly byte[] CommonJumps = new byte[] { 
                                                             0x18, //JR d
                                                             0xC9, //RET
                                                             0xC3, //JP nn
                                                             0xCD, //CALL nn
                                                             0xE9, //JP (HL)
                                                           };
        private IDebuggable m_spectrum;
        
        private bool[] m_addrsFlags; //false => address is excluded from tracing
        private byte[] m_currentTraceOpcodes = null;

        private bool m_isTracingJumps = false;
        private bool m_isTraceAreaDefined = false;
        #endregion

        public DebuggerTrace(IDebuggable i_spectrum)
        {
            m_spectrum = i_spectrum;

            m_addrsFlags = new bool[65536];
            ResetAddrsFlags();
        }

        private void ResetAddrsFlags()
        {
            Array.Clear(m_addrsFlags, 0, m_addrsFlags.Length);
        }

        public void StartTrace(FormCpu i_form)
        {
            SetTraceOpcodes(i_form);
            SetTraceArea(i_form);
        }
        public void StopTrace(FormCpu i_form)
        {
            m_isTracingJumps = false;
        }

        private void SetTraceOpcodes(FormCpu i_form)
        {
            m_currentTraceOpcodes = null;

            if (i_form.checkBoxAllJumps.Checked)
            {
                m_currentTraceOpcodes = new byte[CommonJumps.Length + ConditionalJumps.Length];
                m_currentTraceOpcodes = CommonJumps.Union(ConditionalJumps).ToArray();

                m_isTracingJumps = true;
            }
            else if (i_form.checkBoxConditionalJumps.Checked)
            {
                m_currentTraceOpcodes = ConditionalJumps;
                m_isTracingJumps = true;
            }
            else
                m_isTracingJumps = false;
        }
        private void SetTraceArea(FormCpu i_form)
        {
            if (i_form.listViewAdressRanges.Items.Count == 0 || i_form.checkBoxTraceArea.Checked == false)
            {
                m_isTraceAreaDefined = false;
                return;
            }
            
            foreach( ListViewItem item in i_form.listViewAdressRanges.Items )
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
        public bool IsTracingJumps()
        {
            return m_isTracingJumps;
        }
        public bool IsTraceAreaDefined()
        {
            return m_isTraceAreaDefined;
        }
    }
}
