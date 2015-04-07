namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    partial class AssemblerColorConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssemblerColorConfig));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chcbxCompilerDirectivesEnabled = new System.Windows.Forms.CheckBox();
            this.chckbxCommentsColorEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxJumpsUnderline = new System.Windows.Forms.CheckBox();
            this.checkBoxJumpsStrikeout = new System.Windows.Forms.CheckBox();
            this.checkBoxJumpsBold = new System.Windows.Forms.CheckBox();
            this.checkBoxJumpsItalic = new System.Windows.Forms.CheckBox();
            this.colorPickerJumps = new ZXMAK2.Hardware.Adlers.Views.CustomControls.ColorPicker();
            this.checkBoxCompilerDirectivesUnderline = new System.Windows.Forms.CheckBox();
            this.checkBoxCompilerDirectivesStrikeout = new System.Windows.Forms.CheckBox();
            this.checkBoxCompilerDirectivesBold = new System.Windows.Forms.CheckBox();
            this.checkBoxCompilerDirectivesItalic = new System.Windows.Forms.CheckBox();
            this.colorPickerCompilerDirectives = new ZXMAK2.Hardware.Adlers.Views.CustomControls.ColorPicker();
            this.checkBoxCommentsUnderline = new System.Windows.Forms.CheckBox();
            this.checkBoxCommentsStrikeOut = new System.Windows.Forms.CheckBox();
            this.checkBoxCommentsBold = new System.Windows.Forms.CheckBox();
            this.checkBoxCommentsItalic = new System.Windows.Forms.CheckBox();
            this.colorPickerComments = new ZXMAK2.Hardware.Adlers.Views.CustomControls.ColorPicker();
            this.fctbxPreview = new ZXMAK2.Hardware.Adlers.Views.CustomControls.SourceCodeEditorBox();
            this.buttonDone = new System.Windows.Forms.Button();
            this.btnUndo = new System.Windows.Forms.Button();
            this.chcbxJumpsEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fctbxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chcbxJumpsEnabled);
            this.groupBox1.Controls.Add(this.chcbxCompilerDirectivesEnabled);
            this.groupBox1.Controls.Add(this.chckbxCommentsColorEnabled);
            this.groupBox1.Controls.Add(this.checkBoxJumpsUnderline);
            this.groupBox1.Controls.Add(this.checkBoxJumpsStrikeout);
            this.groupBox1.Controls.Add(this.checkBoxJumpsBold);
            this.groupBox1.Controls.Add(this.checkBoxJumpsItalic);
            this.groupBox1.Controls.Add(this.colorPickerJumps);
            this.groupBox1.Controls.Add(this.checkBoxCompilerDirectivesUnderline);
            this.groupBox1.Controls.Add(this.checkBoxCompilerDirectivesStrikeout);
            this.groupBox1.Controls.Add(this.checkBoxCompilerDirectivesBold);
            this.groupBox1.Controls.Add(this.checkBoxCompilerDirectivesItalic);
            this.groupBox1.Controls.Add(this.colorPickerCompilerDirectives);
            this.groupBox1.Controls.Add(this.checkBoxCommentsUnderline);
            this.groupBox1.Controls.Add(this.checkBoxCommentsStrikeOut);
            this.groupBox1.Controls.Add(this.checkBoxCommentsBold);
            this.groupBox1.Controls.Add(this.checkBoxCommentsItalic);
            this.groupBox1.Controls.Add(this.colorPickerComments);
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(13, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(493, 250);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Syntax highlightning colors";
            // 
            // chcbxCompilerDirectivesEnabled
            // 
            this.chcbxCompilerDirectivesEnabled.AutoSize = true;
            this.chcbxCompilerDirectivesEnabled.Location = new System.Drawing.Point(9, 79);
            this.chcbxCompilerDirectivesEnabled.Name = "chcbxCompilerDirectivesEnabled";
            this.chcbxCompilerDirectivesEnabled.Size = new System.Drawing.Size(132, 19);
            this.chcbxCompilerDirectivesEnabled.TabIndex = 18;
            this.chcbxCompilerDirectivesEnabled.Text = "Compiler directives";
            this.chcbxCompilerDirectivesEnabled.UseVisualStyleBackColor = true;
            this.chcbxCompilerDirectivesEnabled.CheckedChanged += new System.EventHandler(this.chcbxCompilerDirectivesEnabled_CheckedChanged);
            // 
            // chckbxCommentsColorEnabled
            // 
            this.chckbxCommentsColorEnabled.AutoSize = true;
            this.chckbxCommentsColorEnabled.Checked = true;
            this.chckbxCommentsColorEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chckbxCommentsColorEnabled.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chckbxCommentsColorEnabled.Location = new System.Drawing.Point(9, 23);
            this.chckbxCommentsColorEnabled.Name = "chckbxCommentsColorEnabled";
            this.chckbxCommentsColorEnabled.Size = new System.Drawing.Size(115, 19);
            this.chckbxCommentsColorEnabled.TabIndex = 3;
            this.chckbxCommentsColorEnabled.Text = "Comments color";
            this.chckbxCommentsColorEnabled.UseVisualStyleBackColor = true;
            this.chckbxCommentsColorEnabled.CheckedChanged += new System.EventHandler(this.chckbxCommentsColorEnabled_CheckedChanged);
            // 
            // checkBoxJumpsUnderline
            // 
            this.checkBoxJumpsUnderline.AutoSize = true;
            this.checkBoxJumpsUnderline.Location = new System.Drawing.Point(363, 154);
            this.checkBoxJumpsUnderline.Name = "checkBoxJumpsUnderline";
            this.checkBoxJumpsUnderline.Size = new System.Drawing.Size(80, 19);
            this.checkBoxJumpsUnderline.TabIndex = 17;
            this.checkBoxJumpsUnderline.Text = "Underline";
            this.checkBoxJumpsUnderline.UseVisualStyleBackColor = true;
            this.checkBoxJumpsUnderline.CheckedChanged += new System.EventHandler(this.checkBoxJumpsUnderline_CheckedChanged);
            // 
            // checkBoxJumpsStrikeout
            // 
            this.checkBoxJumpsStrikeout.AutoSize = true;
            this.checkBoxJumpsStrikeout.Location = new System.Drawing.Point(284, 154);
            this.checkBoxJumpsStrikeout.Name = "checkBoxJumpsStrikeout";
            this.checkBoxJumpsStrikeout.Size = new System.Drawing.Size(75, 19);
            this.checkBoxJumpsStrikeout.TabIndex = 16;
            this.checkBoxJumpsStrikeout.Text = "Strikeout";
            this.checkBoxJumpsStrikeout.UseVisualStyleBackColor = true;
            this.checkBoxJumpsStrikeout.CheckedChanged += new System.EventHandler(this.checkBoxJumpsStrikeout_CheckedChanged);
            // 
            // checkBoxJumpsBold
            // 
            this.checkBoxJumpsBold.AutoSize = true;
            this.checkBoxJumpsBold.Location = new System.Drawing.Point(227, 154);
            this.checkBoxJumpsBold.Name = "checkBoxJumpsBold";
            this.checkBoxJumpsBold.Size = new System.Drawing.Size(51, 19);
            this.checkBoxJumpsBold.TabIndex = 15;
            this.checkBoxJumpsBold.Text = "Bold";
            this.checkBoxJumpsBold.UseVisualStyleBackColor = true;
            this.checkBoxJumpsBold.CheckedChanged += new System.EventHandler(this.checkBoxJumpsBold_CheckedChanged);
            // 
            // checkBoxJumpsItalic
            // 
            this.checkBoxJumpsItalic.AutoSize = true;
            this.checkBoxJumpsItalic.Checked = true;
            this.checkBoxJumpsItalic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxJumpsItalic.Location = new System.Drawing.Point(164, 154);
            this.checkBoxJumpsItalic.Name = "checkBoxJumpsItalic";
            this.checkBoxJumpsItalic.Size = new System.Drawing.Size(55, 19);
            this.checkBoxJumpsItalic.TabIndex = 14;
            this.checkBoxJumpsItalic.Text = "Italic";
            this.checkBoxJumpsItalic.UseVisualStyleBackColor = true;
            this.checkBoxJumpsItalic.CheckedChanged += new System.EventHandler(this.checkBoxJumpsItalic_CheckedChanged);
            // 
            // colorPickerJumps
            // 
            this.colorPickerJumps.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorPickerJumps.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorPickerJumps.FormattingEnabled = true;
            this.colorPickerJumps.Location = new System.Drawing.Point(29, 152);
            this.colorPickerJumps.Name = "colorPickerJumps";
            this.colorPickerJumps.SelectedValue = System.Drawing.Color.White;
            this.colorPickerJumps.Size = new System.Drawing.Size(129, 24);
            this.colorPickerJumps.TabIndex = 13;
            this.colorPickerJumps.SelectedIndexChanged += new System.EventHandler(this.colorPickerJumps_SelectedIndexChanged);
            // 
            // checkBoxCompilerDirectivesUnderline
            // 
            this.checkBoxCompilerDirectivesUnderline.AutoSize = true;
            this.checkBoxCompilerDirectivesUnderline.Location = new System.Drawing.Point(363, 107);
            this.checkBoxCompilerDirectivesUnderline.Name = "checkBoxCompilerDirectivesUnderline";
            this.checkBoxCompilerDirectivesUnderline.Size = new System.Drawing.Size(80, 19);
            this.checkBoxCompilerDirectivesUnderline.TabIndex = 11;
            this.checkBoxCompilerDirectivesUnderline.Text = "Underline";
            this.checkBoxCompilerDirectivesUnderline.UseVisualStyleBackColor = true;
            this.checkBoxCompilerDirectivesUnderline.CheckedChanged += new System.EventHandler(this.checkBoxCompilerDirectivesUnderline_CheckedChanged);
            // 
            // checkBoxCompilerDirectivesStrikeout
            // 
            this.checkBoxCompilerDirectivesStrikeout.AutoSize = true;
            this.checkBoxCompilerDirectivesStrikeout.Location = new System.Drawing.Point(284, 107);
            this.checkBoxCompilerDirectivesStrikeout.Name = "checkBoxCompilerDirectivesStrikeout";
            this.checkBoxCompilerDirectivesStrikeout.Size = new System.Drawing.Size(75, 19);
            this.checkBoxCompilerDirectivesStrikeout.TabIndex = 10;
            this.checkBoxCompilerDirectivesStrikeout.Text = "Strikeout";
            this.checkBoxCompilerDirectivesStrikeout.UseVisualStyleBackColor = true;
            this.checkBoxCompilerDirectivesStrikeout.CheckedChanged += new System.EventHandler(this.checkBoxCompilerDirectivesStrikeout_CheckedChanged);
            // 
            // checkBoxCompilerDirectivesBold
            // 
            this.checkBoxCompilerDirectivesBold.AutoSize = true;
            this.checkBoxCompilerDirectivesBold.Location = new System.Drawing.Point(227, 107);
            this.checkBoxCompilerDirectivesBold.Name = "checkBoxCompilerDirectivesBold";
            this.checkBoxCompilerDirectivesBold.Size = new System.Drawing.Size(51, 19);
            this.checkBoxCompilerDirectivesBold.TabIndex = 9;
            this.checkBoxCompilerDirectivesBold.Text = "Bold";
            this.checkBoxCompilerDirectivesBold.UseVisualStyleBackColor = true;
            this.checkBoxCompilerDirectivesBold.CheckedChanged += new System.EventHandler(this.checkBoxCompilerDirectivesBold_CheckedChanged);
            // 
            // checkBoxCompilerDirectivesItalic
            // 
            this.checkBoxCompilerDirectivesItalic.AutoSize = true;
            this.checkBoxCompilerDirectivesItalic.Checked = true;
            this.checkBoxCompilerDirectivesItalic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCompilerDirectivesItalic.Location = new System.Drawing.Point(166, 107);
            this.checkBoxCompilerDirectivesItalic.Name = "checkBoxCompilerDirectivesItalic";
            this.checkBoxCompilerDirectivesItalic.Size = new System.Drawing.Size(55, 19);
            this.checkBoxCompilerDirectivesItalic.TabIndex = 8;
            this.checkBoxCompilerDirectivesItalic.Text = "Italic";
            this.checkBoxCompilerDirectivesItalic.UseVisualStyleBackColor = true;
            this.checkBoxCompilerDirectivesItalic.CheckedChanged += new System.EventHandler(this.checkBoxCompilerDirectivesItalic_CheckedChanged);
            // 
            // colorPickerCompilerDirectives
            // 
            this.colorPickerCompilerDirectives.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorPickerCompilerDirectives.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorPickerCompilerDirectives.FormattingEnabled = true;
            this.colorPickerCompilerDirectives.Location = new System.Drawing.Point(29, 102);
            this.colorPickerCompilerDirectives.Name = "colorPickerCompilerDirectives";
            this.colorPickerCompilerDirectives.SelectedValue = System.Drawing.Color.White;
            this.colorPickerCompilerDirectives.Size = new System.Drawing.Size(129, 24);
            this.colorPickerCompilerDirectives.TabIndex = 7;
            this.colorPickerCompilerDirectives.SelectedIndexChanged += new System.EventHandler(this.colorPickerCompilerDirectives_SelectedIndexChanged);
            // 
            // checkBoxCommentsUnderline
            // 
            this.checkBoxCommentsUnderline.AutoSize = true;
            this.checkBoxCommentsUnderline.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxCommentsUnderline.Location = new System.Drawing.Point(363, 47);
            this.checkBoxCommentsUnderline.Name = "checkBoxCommentsUnderline";
            this.checkBoxCommentsUnderline.Size = new System.Drawing.Size(80, 19);
            this.checkBoxCommentsUnderline.TabIndex = 5;
            this.checkBoxCommentsUnderline.Text = "Underline";
            this.checkBoxCommentsUnderline.UseVisualStyleBackColor = true;
            this.checkBoxCommentsUnderline.CheckedChanged += new System.EventHandler(this.checkBoxCommentsUnderline_CheckedChanged);
            // 
            // checkBoxCommentsStrikeOut
            // 
            this.checkBoxCommentsStrikeOut.AutoSize = true;
            this.checkBoxCommentsStrikeOut.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxCommentsStrikeOut.Location = new System.Drawing.Point(284, 47);
            this.checkBoxCommentsStrikeOut.Name = "checkBoxCommentsStrikeOut";
            this.checkBoxCommentsStrikeOut.Size = new System.Drawing.Size(75, 19);
            this.checkBoxCommentsStrikeOut.TabIndex = 4;
            this.checkBoxCommentsStrikeOut.Text = "Strikeout";
            this.checkBoxCommentsStrikeOut.UseVisualStyleBackColor = true;
            this.checkBoxCommentsStrikeOut.CheckedChanged += new System.EventHandler(this.checkBoxCommentsStrikeOut_CheckedChanged);
            // 
            // checkBoxCommentsBold
            // 
            this.checkBoxCommentsBold.AutoSize = true;
            this.checkBoxCommentsBold.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxCommentsBold.Location = new System.Drawing.Point(227, 47);
            this.checkBoxCommentsBold.Name = "checkBoxCommentsBold";
            this.checkBoxCommentsBold.Size = new System.Drawing.Size(51, 19);
            this.checkBoxCommentsBold.TabIndex = 3;
            this.checkBoxCommentsBold.Text = "Bold";
            this.checkBoxCommentsBold.UseVisualStyleBackColor = true;
            this.checkBoxCommentsBold.CheckedChanged += new System.EventHandler(this.checkBoxCommentsBold_CheckedChanged);
            // 
            // checkBoxCommentsItalic
            // 
            this.checkBoxCommentsItalic.AutoSize = true;
            this.checkBoxCommentsItalic.Checked = true;
            this.checkBoxCommentsItalic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCommentsItalic.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxCommentsItalic.Location = new System.Drawing.Point(166, 47);
            this.checkBoxCommentsItalic.Name = "checkBoxCommentsItalic";
            this.checkBoxCommentsItalic.Size = new System.Drawing.Size(55, 19);
            this.checkBoxCommentsItalic.TabIndex = 2;
            this.checkBoxCommentsItalic.Text = "Italic";
            this.checkBoxCommentsItalic.UseVisualStyleBackColor = true;
            this.checkBoxCommentsItalic.CheckedChanged += new System.EventHandler(this.checkBoxCommentsItalic_CheckedChanged);
            // 
            // colorPickerComments
            // 
            this.colorPickerComments.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorPickerComments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorPickerComments.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.colorPickerComments.FormattingEnabled = true;
            this.colorPickerComments.Location = new System.Drawing.Point(29, 45);
            this.colorPickerComments.Name = "colorPickerComments";
            this.colorPickerComments.SelectedValue = System.Drawing.Color.White;
            this.colorPickerComments.Size = new System.Drawing.Size(129, 24);
            this.colorPickerComments.TabIndex = 1;
            this.colorPickerComments.SelectedIndexChanged += new System.EventHandler(this.colorPickerComments_SelectedIndexChanged);
            // 
            // fctbxPreview
            // 
            this.fctbxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fctbxPreview.AutoCompleteBrackets = true;
            this.fctbxPreview.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.fctbxPreview.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+\\s*(?<range>,)\\s*(?<range>[^;]+);";
            this.fctbxPreview.AutoScrollMinSize = new System.Drawing.Size(27, 17);
            this.fctbxPreview.AutoSize = true;
            this.fctbxPreview.BackBrush = null;
            this.fctbxPreview.CharHeight = 17;
            this.fctbxPreview.CharWidth = 8;
            this.fctbxPreview.CommentPrefix = "";
            this.fctbxPreview.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.fctbxPreview.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.fctbxPreview.Font = new System.Drawing.Font("Consolas", 11F);
            this.fctbxPreview.IsReplaceMode = false;
            this.fctbxPreview.Location = new System.Drawing.Point(8, 280);
            this.fctbxPreview.Name = "fctbxPreview";
            this.fctbxPreview.Paddings = new System.Windows.Forms.Padding(0);
            this.fctbxPreview.ReadOnly = true;
            this.fctbxPreview.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.fctbxPreview.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("fctbxPreview.ServiceColors")));
            this.fctbxPreview.Size = new System.Drawing.Size(493, 225);
            this.fctbxPreview.TabIndex = 1;
            this.fctbxPreview.Zoom = 100;
            this.fctbxPreview.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.fastColoredPreview_TextChanged);
            // 
            // buttonDone
            // 
            this.buttonDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDone.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDone.Location = new System.Drawing.Point(512, 29);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(56, 23);
            this.buttonDone.TabIndex = 2;
            this.buttonDone.Text = "Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // btnUndo
            // 
            this.btnUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUndo.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUndo.Location = new System.Drawing.Point(512, 58);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(56, 23);
            this.btnUndo.TabIndex = 3;
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = true;
            // 
            // chcbxJumpsEnabled
            // 
            this.chcbxJumpsEnabled.AutoSize = true;
            this.chcbxJumpsEnabled.Location = new System.Drawing.Point(9, 132);
            this.chcbxJumpsEnabled.Name = "chcbxJumpsEnabled";
            this.chcbxJumpsEnabled.Size = new System.Drawing.Size(60, 19);
            this.chcbxJumpsEnabled.TabIndex = 19;
            this.chcbxJumpsEnabled.Text = "Jumps";
            this.chcbxJumpsEnabled.UseVisualStyleBackColor = true;
            this.chcbxJumpsEnabled.CheckedChanged += new System.EventHandler(this.chcbxJumpsEnabled_CheckedChanged);
            // 
            // AssemblerColorConfig
            // 
            this.AcceptButton = this.buttonDone;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 507);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.fctbxPreview);
            this.Controls.Add(this.groupBox1);
            this.KeyPreview = true;
            this.Name = "AssemblerColorConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "AssemblerColorConfig";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fctbxPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private ZXMAK2.Hardware.Adlers.Views.CustomControls.SourceCodeEditorBox fctbxPreview;
        private CustomControls.ColorPicker colorPickerComments;
        private System.Windows.Forms.CheckBox checkBoxCommentsBold;
        private System.Windows.Forms.CheckBox checkBoxCommentsUnderline;
        private System.Windows.Forms.CheckBox checkBoxCommentsStrikeOut;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.CheckBox checkBoxCompilerDirectivesUnderline;
        private System.Windows.Forms.CheckBox checkBoxCompilerDirectivesStrikeout;
        private System.Windows.Forms.CheckBox checkBoxCompilerDirectivesBold;
        private System.Windows.Forms.CheckBox checkBoxCompilerDirectivesItalic;
        private CustomControls.ColorPicker colorPickerCompilerDirectives;
        private System.Windows.Forms.CheckBox checkBoxJumpsUnderline;
        private System.Windows.Forms.CheckBox checkBoxJumpsStrikeout;
        private System.Windows.Forms.CheckBox checkBoxJumpsBold;
        private System.Windows.Forms.CheckBox checkBoxJumpsItalic;
        private CustomControls.ColorPicker colorPickerJumps;
        private System.Windows.Forms.CheckBox chckbxCommentsColorEnabled;
        private System.Windows.Forms.CheckBox checkBoxCommentsItalic;
        private System.Windows.Forms.CheckBox chcbxCompilerDirectivesEnabled;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.CheckBox chcbxJumpsEnabled;
    }
}