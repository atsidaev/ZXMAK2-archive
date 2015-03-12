using FastColoredTextBoxNS;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ZXMAK2.Hardware.Adlers.Views
{
    public partial class AssemblerColorConfig : Form
    {
        #region Styles
            public Style CommentStyle;
            public Style CommonInstructionStyle;
            public Style JumpInstructionStyle;
            public Style StackInstructionStyle;
            public Style CompilerInstructionStyle;
            public Style RegistryStyle;
            public Style NumbersStyle;
        #endregion

        private Assembler _assemblerForm;

        private bool _eventsDisabled = false;

        public AssemblerColorConfig(Assembler i_assemblerForm)
        {
            InitializeComponent();
            _assemblerForm = i_assemblerForm;

            #region Style definitions
                CommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
                colorPickerComments.SelectedValue = Color.Green;
                CommonInstructionStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
                JumpInstructionStyle = new TextStyle(Brushes.DarkViolet, null, FontStyle.Regular);
                StackInstructionStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
                CompilerInstructionStyle = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Italic);
                RegistryStyle = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
                NumbersStyle = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
            #endregion

             //set preview assembler text
             fastColoredPreview.Text = "       org 40000 ; this is a comment\n" +
                                       "label: xor a\n" +
                                       "       push bc\n\n" +
                                       "defb DefinedByte 0\n" +
                                       "       ld ix, 0xAF05\n" +
                                       "       ld a, %11100010\n" +
                                       "       ret\n" +
                                       "macroBorderRandom MACRO var1\n      ld a, var1: out(#fe),a\nENDM\n";
        }

        private void fastColoredPreview_TextChanged(object sender, TextChangedEventArgs e)
        {
            //comments
            e.ChangedRange.ClearStyle(CommentStyle);
            e.ChangedRange.SetStyle(CommentStyle, @";.*$", RegexOptions.Multiline);

            e.ChangedRange.ClearStyle(CommonInstructionStyle);
            e.ChangedRange.ClearStyle(JumpInstructionStyle);
            e.ChangedRange.ClearStyle(StackInstructionStyle);
            e.ChangedRange.ClearStyle(RegistryStyle);
            e.ChangedRange.ClearStyle(CompilerInstructionStyle);
            e.ChangedRange.ClearStyle(NumbersStyle);

            e.ChangedRange.SetStyle(NumbersStyle, @"(?:\(|\n|,| )\d{1,5}\b|[^a-zA-Z](?:x|#|\$)[0-9A-Fa-f]{1,4}|%[0-1]{1,16}", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(CommonInstructionStyle, @"\bldir\b|\blddr\b|\bld\b|\bim\b|\badd\b|\bsub\b|\bdec\b|\bsbc\b|\bhalt\b|\bbit\b|" +
                @"\bset\b|xor|\binc(\n| )\b|\bcp\b|\bcpl\b|\bei\b|\bdi\b|\band\b|\bor\b|\band\b" +
                @"|\brr\b|\bscf\b|\bccf\b|\bneg\b|\bsrl\b|exx|\bex\b|\brla\b|\brra\b|\brr\b|\bout\b|\bin\b|\bsla\b|\brl\b",
                RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(CompilerInstructionStyle, @"\bdefb\b|\bdefw\b|\bdefl\b|\bdefm\b|\bdefs\b|\bequ\b|\bmacro\b|\bendm\b|include|incbin|" +
                @"\bif\b|\bendif\b|\belse\b",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(StackInstructionStyle, @"\bpush\b|\bpop\b|\bdec sp\b|\binc sp\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(JumpInstructionStyle, @"\borg\b|\breti\b|\bretn\b|\bret\b|\bjp\b|\bjr\b|\bcall\b|\bdjnz\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(RegistryStyle, @"\bhl\b|\bbc\b|\bix\b|\biy\b|\bde\b|\bpc\b|\baf\b|\bsp\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        //Comments style modification
        public void ChangeCommentsStyle(TextStyle i_textStyleDynamic = null)
        {
            fastColoredPreview.ClearStylesBuffer();

            Range range = fastColoredPreview.VisibleRange;
            FontStyle commentsStyle = new FontStyle();

            if (i_textStyleDynamic != null)
            {
                _eventsDisabled = true;
                //dynamically changed(load config, ....)
                this.colorPickerComments.SelectedValue = ((SolidBrush)i_textStyleDynamic.ForeBrush).Color;
                this.checkBoxCommentsItalic.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Italic);
                this.checkBoxCommentsBold.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Bold);
                this.checkBoxCommentsStrikeOut.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Strikeout);
                this.checkBoxCommentsUnderline.Checked = i_textStyleDynamic.FontStyle.HasFlag(FontStyle.Underline);
                _eventsDisabled = false;
            }

            if (this.checkBoxCommentsItalic.Checked)
                commentsStyle |= FontStyle.Italic;
            if (this.checkBoxCommentsBold.Checked)
                commentsStyle |= FontStyle.Bold;
            if (this.checkBoxCommentsStrikeOut.Checked)
                commentsStyle |= FontStyle.Strikeout;
            if (this.checkBoxCommentsUnderline.Checked)
                commentsStyle |= FontStyle.Underline;

            CommentStyle = new TextStyle(new SolidBrush(colorPickerComments.SelectedValue), null, commentsStyle);
            range.SetStyle(CommentStyle, @";.*$", RegexOptions.Multiline);

            if (i_textStyleDynamic != null)
                _assemblerForm.RefreshAssemblerCode();
        }
        private void colorPickerComments_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeCommentsStyle();
        }
        private void checkBoxCommentsItalic_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeCommentsStyle();
        }
        private void checkBoxCommentsBold_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeCommentsStyle();
        }
        private void checkBoxCommentsStrikeOut_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeCommentsStyle();
        }
        private void checkBoxCommentsUnderline_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_eventsDisabled)
                ChangeCommentsStyle();
        }

        //Done
        private void buttonDone_Click(object sender, System.EventArgs e)
        {
            _assemblerForm.RefreshAssemblerCode();
            this.Hide();
        }
    }
}
