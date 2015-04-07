using FastColoredTextBoxNS;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ZXMAK2.Hardware.Adlers.Views.CustomControls;

namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    public partial class AssemblerColorConfig : Form
    {
        private static AssemblerColorConfig _instance = null; //this
        private static Assembler _assemblerForm;

        private bool _eventsDisabled = false;

        //set preview assembler text
        private static readonly string _TEST_TEXT = 
                          "       org 40000 ; this is a comment\n" +
                          "label: xor a\n" +
                          "       push bc\n" +
                          "       jp #4455\n\n" +
                          "defb DefinedByte 0\n" +
                          "       ld ix, 0xAF05\n" +
                          "       ld a, %11100010\n" +
                          "       ret\n" +
                          "macroBorderRandom MACRO var1\n" +
                          "ld a, var1: out(#fe),a\nENDM\n" +
                          "ld b, 10: djnz $43";

        private AssemblerColorConfig()
        {
            InitializeComponent();
            InitSyntaxHighlightningStyles();

            fctbxPreview.Text = _TEST_TEXT;
        }
        public static void ShowForm()
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new AssemblerColorConfig();
                //_instance.LoadConfig();
                _instance.ShowInTaskbar = false;
            }
            _instance.ShowDialog();
        }

        public static AssemblerColorConfig GetInstance()
        {
            if (_instance == null)
                _instance = new AssemblerColorConfig();
            return _instance;
        }

        public static void Init(Assembler i_assemblerForm)
        {
            _assemblerForm = i_assemblerForm;
        }

        #region Syntax highlightning styles
            //comments
            public static Style  CommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
            public static string regexComment = @";.*";
            //common instructions
            public static Style  styleCommonInstruction = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            public static string regexCommonInstruction = @"\bldir\b|\blddr\b|\bld\b|\bim[ ]+\b|\badd\b|\bsub\b|[ ]dec[ ]|\bsbc\b|\bhalt\b|\bbit\b|" +
                                                          @"\bset\b|xor|[ ]inc[ ]|\bcp\b|\bcpl\b|\bei\b|\bdi\b|\band\b|\bor\b|\band\b" +
                                                          @"|\brr\b|\bscf\b|\bccf\b|\bneg\b|\bsrl\b|exx|\bex\b|\brla\b|\brra\b|\brr\b|\bout\b|\bin\b|\bsla\b|\brl\b|\brrca\b" + 
                                                          @"|\brlca\b";
            //jumps
            public static Style  JumpInstructionStyle = new TextStyle(Brushes.DarkViolet, null, FontStyle.Regular);
            public static string regexJumpInstruction = @"\breti\b|\bretn\b|\bret\b|\bjp\b|\bjr\b|\bcall\b|\bdjnz\b";
            //stack
            public static Style  styleStackInstruction = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            public static string regexStackInstruction = @"\bpush\b|\bpop\b|\bdec sp\b|\binc sp\b";
            //registry
            public static Style  styleRegistry = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
            public static string regexRegistry = @"\bhl\b|(?<=[ ,(])BC(?=[ ,)\n;\r])|\bix\b|\biy\b|\bde\b|\bpc\b|\bsp\b|[\( ,]\b(IR|HL)\b|(?<=[ ,(])AF(?=[ ,)\n;\r])";
            //compiler directives
            public static Style  CompilerDirectivesStyle = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Italic);
            public static string regexCompilerDirectives = @"\borg\b|\bdefb\b|\bdefw\b|\bdefl\b|\bdefm\b|\bdefs\b|\bequ\b|\bmacro\b|\bendm\b|include|incbin|" +
                                                           @"\bif\b|\bendif\b|\belse\b";
            //numbers
            public static Style  styleNumbers = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
            public static string regexNumbers = @"(?:\(|\n|,| |\+|\-|\*|\/)\d{1,5}\b|[^a-zA-Z](?:x|#|\$)[0-9A-Fa-f]{1,4}|%[0-1]{1,16}";
        
        private void InitSyntaxHighlightningStyles()
        {
            CommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
            //styleCommonInstruction = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            JumpInstructionStyle = new TextStyle(Brushes.DarkViolet, null, FontStyle.Regular);
            //styleStackInstruction = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            CompilerDirectivesStyle = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Italic);
            //styleRegistry = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
            //styleNumbers = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
        }
        public static void RefreshControlStyles( object i_fctxbBox, TextChangedEventArgs e)
        {
            Range range;
            if (i_fctxbBox is SourceCodeEditorBox)
            {
                (i_fctxbBox as SourceCodeEditorBox).RefreshRtf();
                range = (e==null ? (i_fctxbBox as SourceCodeEditorBox).Range : e.ChangedRange);
            }
            else
                range = (e == null ? (i_fctxbBox as FastColoredTextBox).Range : e.ChangedRange);

            range.ClearStyle(CompilerDirectivesStyle);
            range.ClearStyle(CommentStyle);
            //range.ClearStyle(styleNumbers);
            //range.ClearStyle(styleCommonInstruction);
            range.ClearStyle(JumpInstructionStyle);
            //range.ClearStyle(styleRegistry);
            //range.ClearStyle(styleStackInstruction);
            //fctxbBox.ClearStylesBuffer();

            if (Settings.IsSyntaxHighlightningOn() && _instance != null)
            {
                //keep style order! highest priority has first style added.
                if (_instance.IsCommentsColorEnabled())
                    range.SetStyle(CommentStyle, regexComment);
                //range.SetStyle(styleRegistry, regexRegistry, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                //range.SetStyle(styleNumbers, regexNumbers, RegexOptions.Multiline);
                if (_instance.IsCompilerDirectivesEnabled())
                    range.SetStyle(CompilerDirectivesStyle, regexCompilerDirectives, RegexOptions.IgnoreCase);
                //range.SetStyle(styleCommonInstruction, regexCommonInstruction, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (_instance.IsJumpStyleEnabled())
                    range.SetStyle(JumpInstructionStyle, regexJumpInstruction, RegexOptions.IgnoreCase);
                //range.SetStyle(styleStackInstruction, regexStackInstruction, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            }
        }
        #endregion Syntax highlightning styles

        private void fastColoredPreview_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshControlStyles(sender, e);
        }

        //Comments style modification
        public void ChangeSyntaxStyle(bool i_isEnabled = false, TextStyle i_textStyleDynamic = null, int i_styleId = -1)
        {
            if (i_styleId == 0)
            {
                _eventsDisabled = true;
                //dynamically changed(load config, ....)
                this.chckbxCommentsColorEnabled.Checked = i_isEnabled;
                this.colorPickerComments.SelectedValue = ((SolidBrush)i_textStyleDynamic.ForeBrush).Color;
                this.checkBoxCommentsItalic.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Italic);
                this.checkBoxCommentsBold.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Bold);
                this.checkBoxCommentsStrikeOut.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Strikeout);
                this.checkBoxCommentsUnderline.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Underline);
                _eventsDisabled = false;
            }
            if (i_styleId == 1)
            {
                //compiler directive style
                _eventsDisabled = true;
                //dynamically changed(load config, ....)
                this.chcbxCompilerDirectivesEnabled.Checked = i_isEnabled;
                this.colorPickerCompilerDirectives.SelectedValue = ((SolidBrush)i_textStyleDynamic.ForeBrush).Color;
                this.checkBoxCompilerDirectivesItalic.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Italic);
                this.checkBoxCompilerDirectivesBold.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Bold);
                this.checkBoxCompilerDirectivesStrikeout.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Strikeout);
                this.checkBoxCompilerDirectivesUnderline.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Underline);
                _eventsDisabled = false;
            }
            if (i_styleId == 2)
            {
                //jumps style
                _eventsDisabled = true;
                //dynamically changed(load config, ....)
                this.chcbxJumpsEnabled.Checked = i_isEnabled;
                this.colorPickerJumps.SelectedValue = ((SolidBrush)i_textStyleDynamic.ForeBrush).Color;
                this.checkBoxJumpsItalic.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Italic);
                this.checkBoxJumpsBold.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Bold);
                this.checkBoxJumpsStrikeout.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Strikeout);
                this.checkBoxJumpsUnderline.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Underline);
                _eventsDisabled = false;
            }

            //comments
            FontStyle style = new FontStyle();
            if (this.checkBoxCommentsItalic.Checked)
                style |= FontStyle.Italic;
            if (this.checkBoxCommentsBold.Checked)
                style |= FontStyle.Bold;
            if (this.checkBoxCommentsStrikeOut.Checked)
                style |= FontStyle.Strikeout;
            if (this.checkBoxCommentsUnderline.Checked)
                style |= FontStyle.Underline;
            CommentStyle = new TextStyle(new SolidBrush(colorPickerComments.SelectedValue), null, style);
            //compiler directives
            style = new FontStyle();
            if (this.checkBoxCompilerDirectivesItalic.Checked)
                style |= FontStyle.Italic;
            if (this.checkBoxCompilerDirectivesBold.Checked)
                style |= FontStyle.Bold;
            if (this.checkBoxCompilerDirectivesStrikeout.Checked)
                style |= FontStyle.Strikeout;
            if (this.checkBoxCompilerDirectivesUnderline.Checked)
                style |= FontStyle.Underline;
            CompilerDirectivesStyle = new TextStyle(new SolidBrush(colorPickerCompilerDirectives.SelectedValue), null, style);
            //jumps
            style = new FontStyle();
            if (this.checkBoxJumpsItalic.Checked)
                style |= FontStyle.Italic;
            if (this.checkBoxJumpsBold.Checked)
                style |= FontStyle.Bold;
            if (this.checkBoxJumpsStrikeout.Checked)
                style |= FontStyle.Strikeout;
            if (this.checkBoxJumpsUnderline.Checked)
                style |= FontStyle.Underline;
            JumpInstructionStyle = new TextStyle(new SolidBrush(colorPickerJumps.SelectedValue), null, style);
            
            RefreshControlStyles(this.fctbxPreview, null);

            if (i_textStyleDynamic != null)
                _assemblerForm.RefreshAssemblerCodeSyntaxHighlightning();
        }

        //comments controls(combo and checkbox)
        private void colorPickerComments_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxCommentsItalic_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxCommentsBold_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxCommentsStrikeOut_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxCommentsUnderline_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        //Compiler directive controls(combo and checkbox)
        private void colorPickerCompilerDirectives_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxCompilerDirectivesItalic_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxCompilerDirectivesBold_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxCompilerDirectivesStrikeout_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxCompilerDirectivesUnderline_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }

        //Jumps controls(combo and checkbox)
        private void colorPickerJumps_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxJumpsItalic_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxJumpsBold_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxJumpsStrikeout_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        private void checkBoxJumpsUnderline_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }

        //Done
        private void buttonDone_Click(object sender, System.EventArgs e)
        {
            _assemblerForm.RefreshAssemblerCodeSyntaxHighlightning();
            this.Hide();
        }

        //comments enabled
        private void chckbxCommentsColorEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            colorPickerComments.Enabled = checkBoxCommentsItalic.Enabled = checkBoxCommentsBold.Enabled = checkBoxCommentsStrikeOut.Enabled =
                checkBoxCommentsUnderline.Enabled = chckbxCommentsColorEnabled.Checked;
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        public bool IsCommentsColorEnabled()
        {
            return chckbxCommentsColorEnabled.Checked;
        }

        //compiler directives enabled
        private void chcbxCompilerDirectivesEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            colorPickerCompilerDirectives.Enabled = checkBoxCompilerDirectivesItalic.Enabled = checkBoxCompilerDirectivesBold.Enabled = checkBoxCompilerDirectivesStrikeout.Enabled =
                checkBoxCompilerDirectivesUnderline.Enabled = chcbxCompilerDirectivesEnabled.Checked;
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        public bool IsCompilerDirectivesEnabled()
        {
            return chcbxCompilerDirectivesEnabled.Checked;
        }

        //jumps enabled
        private void chcbxJumpsEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            colorPickerJumps.Enabled = checkBoxJumpsItalic.Enabled = checkBoxJumpsBold.Enabled = checkBoxJumpsStrikeout.Enabled =
                checkBoxJumpsUnderline.Enabled = chcbxJumpsEnabled.Checked;
            if (!_eventsDisabled)
                ChangeSyntaxStyle();
        }
        public bool IsJumpStyleEnabled()
        {
            return chcbxJumpsEnabled.Checked;
        }
    }
}
