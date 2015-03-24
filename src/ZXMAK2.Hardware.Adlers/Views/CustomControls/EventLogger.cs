using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ZXMAK2.Hardware.Adlers.Views.CustomControls
{
    public partial class EventLogger : RichTextBox
    {
        public EventLogger()
        {
            InitializeComponent();
            base.ReadOnly = true;
            base.BackColor = Color.Black;
            base.ForeColor = Color.Green;
            base.Font = new Font("consolas", 9);

            _logInfo = new List<LOG_INFO>();
        }

        public void Display()
        {
            //int messageLinesCount = 0;
            int realLinesCount = 1;

            base.Text = String.Empty;
            foreach( LOG_INFO info in _logInfo.OrderBy(p => p.Id) )
            {
                info.RangeStart = realLinesCount - 1;
                if( info.HasLogTime )
                    base.Text += info.LogTime.ToLongTimeString() + ": ";
                base.Text += info.GetDisplayMessage();
                base.Text += "\n";
                info.RangeEnd = realLinesCount;
                base.Text += "====================================\n";

                realLinesCount = base.Lines.Length;
            }
            base.SelectionStart = base.Text.Length;
            base.ScrollToCaret();
        }

        #region members

            List<LOG_INFO> _logInfo;

            //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            //private new string Text;
        #endregion members

        #region GUI
        [DllImport("user32.dll")]
        private static extern int HideCaret(IntPtr hwnd);
        
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            HideCaret(this.Handle);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            HideCaret(this.Handle);
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            HideCaret(this.Handle);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int firstcharindex = base.GetFirstCharIndexOfCurrentLine();
            string currentlinetext = base.Lines[GetCurrentLine()];
            base.Select(firstcharindex, currentlinetext.Length);
        }
        public int GetCurrentLine()
        {
            int firstcharindex = base.GetFirstCharIndexOfCurrentLine();
            return base.GetLineFromCharIndex(firstcharindex);
        }
        /*protected override void OnKeyDown(KeyEventArgs pe)
        {
            pe.Handled = true;
            //base.OnKeyDown(pe);
        }
        protected override void OnKeyUp(KeyEventArgs pe)
        {
            pe.Handled = true;
            //base.OnKeyUp(pe);
        }
        protected override void OnTextChanged(EventArgs e)
        {
            //base.Text;
            //base.OnTextChanged(e);
        }*/
        /*override protected CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }*/

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get { return base.Text; }
            set { /*base.Text = value;*/ } //dummy
        }
        #endregion GUI

        #region log methods
        public void AppendLog(string i_msg, LOG_LEVEL i_level, bool i_logTime = false)
        {
            LOG_INFO logInfo = new LOG_INFO();
            logInfo.Id = GetNextLogInfoId();
            logInfo.SetMessage( i_msg );
            logInfo.Level = i_level;
            logInfo.HasLogTime = i_logTime;
            if (i_logTime)
                logInfo.LogTime = DateTime.Now;

            AppendLog(logInfo);
        }
        public void AppendLog(LOG_INFO i_logInfo)
        {
            i_logInfo.Id = GetNextLogInfoId();
            _logInfo.Add(i_logInfo);
            Display();
        }
        public void AppendInfo(string i_infoMessage)
        {
            LOG_INFO logInfo = new LOG_INFO();
            logInfo.SetMessage("INFO:  " + i_infoMessage);
            logInfo.Id = GetNextLogInfoId();
            _logInfo.Add(logInfo);
            Display();
        }

        public LOG_INFO GetCurrentMessage(/*int i_lineNumber*/)
        {
            int lineNumber = GetCurrentLine();
            return _logInfo.Where(item => item.RangeStart <= lineNumber && item.RangeEnd >= lineNumber).FirstOrDefault();
        }

        public int GetNextLogInfoId()
        {
            if (_logInfo.Count == 0)
                return 0;

            return _logInfo.Max(p => p.Id) + 1;
        }

        public void ClearLog()
        {
            _logInfo.Clear();
            base.Text = String.Empty;
            Display();
        }
        #endregion log methods
    }

    public enum LOG_LEVEL { Warning, Error, Info, Unknown };
    public class LOG_INFO
    {
        public int Id = 0;
        public string _originalMessage = String.Empty;
        public string _displayMessage = String.Empty;
        public bool HasLogTime = false;
        public DateTime LogTime;
        public LOG_LEVEL Level;

        public int RangeStart = 0; //line number where starts the current log info
        public int RangeEnd = 0; //line number where ends the current log info
        
        public int ErrorLine = -1;

        public int GetMessageLines()
        {
            return _displayMessage.Split('\n').Length;
        }
        public void SetMessage(string i_message)
        {
            _originalMessage = i_message;
            _displayMessage = i_message.TrimEnd('\n');
        }
        public string GetDisplayMessage()
        {
            return _displayMessage;
        }
    };
}
