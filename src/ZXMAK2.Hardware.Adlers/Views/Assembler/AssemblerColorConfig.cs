using FastColoredTextBoxNS;
using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    public partial class AssemblerColorConfig : Form
    {
        private Assembler _assemblerForm;

        private bool _eventsDisabled = false;

        public AssemblerColorConfig(Assembler i_assemblerForm)
        {
            InitializeComponent();
            _assemblerForm = i_assemblerForm;

             //set preview assembler text
             fastColoredPreview.Text = "       org 40000 ; this is a comment\n" +
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
        }

        private void fastColoredPreview_TextChanged(object sender, TextChangedEventArgs e)
        {
            FastColoredTextBox fctbx = sender as FastColoredTextBox;
            AssemblerConfig.RefreshControlStyles(fctbx);
        }

        //Comments style modification
        public void ChangeSyntaxStyle(TextStyle i_textStyleDynamic = null, int i_styleId = -1)
        {
            //fastColoredPreview.ClearStylesBuffer();

            //Range range = fastColoredPreview.VisibleRange;
            if (i_styleId == 0)
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
            if (i_styleId == 1)
            {
                //compiler directive style
                _eventsDisabled = true;
                //dynamically changed(load config, ....)
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
            AssemblerConfig.styleComment = new TextStyle(new SolidBrush(colorPickerComments.SelectedValue), null, style);
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
            AssemblerConfig.styleCompilerDirectives = new TextStyle(new SolidBrush(colorPickerCompilerDirectives.SelectedValue), null, style);
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
            AssemblerConfig.styleJumpInstruction = new TextStyle(new SolidBrush(colorPickerJumps.SelectedValue), null, style);
            
            AssemblerConfig.RefreshControlStyles(this.fastColoredPreview);

            if (i_textStyleDynamic != null)
                _assemblerForm.RefreshAssemblerCode();
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
            _assemblerForm.RefreshAssemblerCode();
            this.Hide();
        }
    }
}
