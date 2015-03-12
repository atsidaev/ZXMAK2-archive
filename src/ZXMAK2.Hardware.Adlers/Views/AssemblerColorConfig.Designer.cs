﻿namespace ZXMAK2.Hardware.Adlers.Views
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
            this.checkBoxCommentsUnderline = new System.Windows.Forms.CheckBox();
            this.checkBoxCommentsStrikeOut = new System.Windows.Forms.CheckBox();
            this.checkBoxCommentsBold = new System.Windows.Forms.CheckBox();
            this.checkBoxCommentsItalic = new System.Windows.Forms.CheckBox();
            this.colorPickerComments = new ZXMAK2.Hardware.Adlers.Views.CustomControls.ColorPicker();
            this.label1 = new System.Windows.Forms.Label();
            this.fastColoredPreview = new FastColoredTextBoxNS.FastColoredTextBox();
            this.buttonDone = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fastColoredPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxCommentsUnderline);
            this.groupBox1.Controls.Add(this.checkBoxCommentsStrikeOut);
            this.groupBox1.Controls.Add(this.checkBoxCommentsBold);
            this.groupBox1.Controls.Add(this.checkBoxCommentsItalic);
            this.groupBox1.Controls.Add(this.colorPickerComments);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 250);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Colors";
            // 
            // checkBoxCommentsUnderline
            // 
            this.checkBoxCommentsUnderline.AutoSize = true;
            this.checkBoxCommentsUnderline.Location = new System.Drawing.Point(395, 27);
            this.checkBoxCommentsUnderline.Name = "checkBoxCommentsUnderline";
            this.checkBoxCommentsUnderline.Size = new System.Drawing.Size(71, 17);
            this.checkBoxCommentsUnderline.TabIndex = 5;
            this.checkBoxCommentsUnderline.Text = "Underline";
            this.checkBoxCommentsUnderline.UseVisualStyleBackColor = true;
            this.checkBoxCommentsUnderline.CheckedChanged += new System.EventHandler(this.checkBoxCommentsUnderline_CheckedChanged);
            // 
            // checkBoxCommentsStrikeOut
            // 
            this.checkBoxCommentsStrikeOut.AutoSize = true;
            this.checkBoxCommentsStrikeOut.Location = new System.Drawing.Point(325, 27);
            this.checkBoxCommentsStrikeOut.Name = "checkBoxCommentsStrikeOut";
            this.checkBoxCommentsStrikeOut.Size = new System.Drawing.Size(68, 17);
            this.checkBoxCommentsStrikeOut.TabIndex = 4;
            this.checkBoxCommentsStrikeOut.Text = "Strikeout";
            this.checkBoxCommentsStrikeOut.UseVisualStyleBackColor = true;
            this.checkBoxCommentsStrikeOut.CheckedChanged += new System.EventHandler(this.checkBoxCommentsStrikeOut_CheckedChanged);
            // 
            // checkBoxCommentsBold
            // 
            this.checkBoxCommentsBold.AutoSize = true;
            this.checkBoxCommentsBold.Location = new System.Drawing.Point(272, 26);
            this.checkBoxCommentsBold.Name = "checkBoxCommentsBold";
            this.checkBoxCommentsBold.Size = new System.Drawing.Size(47, 17);
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
            this.checkBoxCommentsItalic.Location = new System.Drawing.Point(217, 26);
            this.checkBoxCommentsItalic.Name = "checkBoxCommentsItalic";
            this.checkBoxCommentsItalic.Size = new System.Drawing.Size(48, 17);
            this.checkBoxCommentsItalic.TabIndex = 2;
            this.checkBoxCommentsItalic.Text = "Italic";
            this.checkBoxCommentsItalic.UseVisualStyleBackColor = true;
            this.checkBoxCommentsItalic.CheckedChanged += new System.EventHandler(this.checkBoxCommentsItalic_CheckedChanged);
            // 
            // colorPickerComments
            // 
            this.colorPickerComments.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorPickerComments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorPickerComments.FormattingEnabled = true;
            this.colorPickerComments.Location = new System.Drawing.Point(81, 23);
            this.colorPickerComments.Name = "colorPickerComments";
            this.colorPickerComments.SelectedValue = System.Drawing.Color.White;
            this.colorPickerComments.Size = new System.Drawing.Size(129, 21);
            this.colorPickerComments.TabIndex = 1;
            this.colorPickerComments.SelectedIndexChanged += new System.EventHandler(this.colorPickerComments_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Comments";
            // 
            // fastColoredPreview
            // 
            this.fastColoredPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fastColoredPreview.AutoCompleteBrackets = true;
            this.fastColoredPreview.AutoCompleteBracketsList = new char[] {
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
            this.fastColoredPreview.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+\\s*(?<range>,)\\s*(?<range>[^;]+);";
            this.fastColoredPreview.AutoScrollMinSize = new System.Drawing.Size(27, 17);
            this.fastColoredPreview.AutoSize = true;
            this.fastColoredPreview.BackBrush = null;
            this.fastColoredPreview.CharHeight = 17;
            this.fastColoredPreview.CharWidth = 8;
            this.fastColoredPreview.CommentPrefix = ";";
            this.fastColoredPreview.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.fastColoredPreview.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.fastColoredPreview.Font = new System.Drawing.Font("Consolas", 11F);
            this.fastColoredPreview.IsReplaceMode = false;
            this.fastColoredPreview.Location = new System.Drawing.Point(8, 280);
            this.fastColoredPreview.Name = "fastColoredPreview";
            this.fastColoredPreview.Paddings = new System.Windows.Forms.Padding(0);
            this.fastColoredPreview.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.fastColoredPreview.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("fastColoredPreview.ServiceColors")));
            this.fastColoredPreview.Size = new System.Drawing.Size(470, 205);
            this.fastColoredPreview.TabIndex = 1;
            this.fastColoredPreview.Zoom = 100;
            this.fastColoredPreview.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.fastColoredPreview_TextChanged);
            // 
            // buttonDone
            // 
            this.buttonDone.Location = new System.Drawing.Point(490, 23);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(56, 23);
            this.buttonDone.TabIndex = 2;
            this.buttonDone.Text = "Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // AssemblerColorConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 487);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.fastColoredPreview);
            this.Controls.Add(this.groupBox1);
            this.Name = "AssemblerColorConfig";
            this.ShowIcon = false;
            this.Text = "AssemblerColorConfig";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fastColoredPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private FastColoredTextBoxNS.FastColoredTextBox fastColoredPreview;
        private System.Windows.Forms.Label label1;
        private CustomControls.ColorPicker colorPickerComments;
        private System.Windows.Forms.CheckBox checkBoxCommentsItalic;
        private System.Windows.Forms.CheckBox checkBoxCommentsBold;
        private System.Windows.Forms.CheckBox checkBoxCommentsUnderline;
        private System.Windows.Forms.CheckBox checkBoxCommentsStrikeOut;
        private System.Windows.Forms.Button buttonDone;
    }
}