namespace ZXMAK2.Host.WinForms.HardwareViews.Adlers
{
    partial class GraphicsEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphicsEditor));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboDisplayTypeHeight = new System.Windows.Forms.ComboBox();
            this.comboDisplayTypeWidth = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.comboDisplayType = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDownActualAddress = new System.Windows.Forms.NumericUpDown();
            this.pictureZXDisplay = new System.Windows.Forms.PictureBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownActualAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureZXDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboDisplayTypeHeight);
            this.groupBox2.Controls.Add(this.comboDisplayTypeWidth);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.comboDisplayType);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.groupBox2.Location = new System.Drawing.Point(12, 346);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(120, 159);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Screen View Type:";
            // 
            // comboDisplayTypeHeight
            // 
            this.comboDisplayTypeHeight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDisplayTypeHeight.Enabled = false;
            this.comboDisplayTypeHeight.FormattingEnabled = true;
            this.comboDisplayTypeHeight.Items.AddRange(new object[] {
            "8",
            "16",
            "24",
            "32",
            "48",
            "192"});
            this.comboDisplayTypeHeight.Location = new System.Drawing.Point(7, 127);
            this.comboDisplayTypeHeight.Name = "comboDisplayTypeHeight";
            this.comboDisplayTypeHeight.Size = new System.Drawing.Size(104, 23);
            this.comboDisplayTypeHeight.TabIndex = 27;
            // 
            // comboDisplayTypeWidth
            // 
            this.comboDisplayTypeWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDisplayTypeWidth.Enabled = false;
            this.comboDisplayTypeWidth.FormattingEnabled = true;
            this.comboDisplayTypeWidth.Items.AddRange(new object[] {
            "8",
            "16",
            "24",
            "32",
            "40"});
            this.comboDisplayTypeWidth.Location = new System.Drawing.Point(7, 83);
            this.comboDisplayTypeWidth.Name = "comboDisplayTypeWidth";
            this.comboDisplayTypeWidth.Size = new System.Drawing.Size(104, 23);
            this.comboDisplayTypeWidth.TabIndex = 26;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label9.Location = new System.Drawing.Point(7, 109);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(86, 15);
            this.label9.TabIndex = 25;
            this.label9.Text = "Height(Pixels):";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label8.Location = new System.Drawing.Point(7, 65);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(85, 15);
            this.label8.TabIndex = 24;
            this.label8.Text = "Width(Pixels):";
            // 
            // comboDisplayType
            // 
            this.comboDisplayType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDisplayType.FormattingEnabled = true;
            this.comboDisplayType.Items.AddRange(new object[] {
            "Screen View",
            "Sprite View",
            "Linear",
            "Robocop style"});
            this.comboDisplayType.Location = new System.Drawing.Point(6, 39);
            this.comboDisplayType.Name = "comboDisplayType";
            this.comboDisplayType.Size = new System.Drawing.Size(107, 23);
            this.comboDisplayType.TabIndex = 23;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label7.Location = new System.Drawing.Point(4, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 15);
            this.label7.TabIndex = 22;
            this.label7.Text = "Display Type:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.label10.Location = new System.Drawing.Point(9, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(95, 15);
            this.label10.TabIndex = 26;
            this.label10.Text = "Memory adress:";
            // 
            // numericUpDownActualAddress
            // 
            this.numericUpDownActualAddress.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.numericUpDownActualAddress.Location = new System.Drawing.Point(12, 28);
            this.numericUpDownActualAddress.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDownActualAddress.Name = "numericUpDownActualAddress";
            this.numericUpDownActualAddress.Size = new System.Drawing.Size(120, 26);
            this.numericUpDownActualAddress.TabIndex = 27;
            this.numericUpDownActualAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDownActualAddress.Value = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.numericUpDownActualAddress.ValueChanged += new System.EventHandler(this.numericUpDownActualAddress_ValueChanged);
            // 
            // pictureZXDisplay
            // 
            this.pictureZXDisplay.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureZXDisplay.Image = ((System.Drawing.Image)(resources.GetObject("pictureZXDisplay.Image")));
            this.pictureZXDisplay.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureZXDisplay.InitialImage")));
            this.pictureZXDisplay.Location = new System.Drawing.Point(138, 28);
            this.pictureZXDisplay.Name = "pictureZXDisplay";
            this.pictureZXDisplay.Size = new System.Drawing.Size(512, 384);
            this.pictureZXDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureZXDisplay.TabIndex = 28;
            this.pictureZXDisplay.TabStop = false;
            // 
            // GraphicsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 517);
            this.Controls.Add(this.pictureZXDisplay);
            this.Controls.Add(this.numericUpDownActualAddress);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.groupBox2);
            this.Name = "GraphicsEditor";
            this.ShowIcon = false;
            this.Text = "GraphicsEditor";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownActualAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureZXDisplay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox comboDisplayTypeHeight;
        private System.Windows.Forms.ComboBox comboDisplayTypeWidth;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboDisplayType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDownActualAddress;
        private System.Windows.Forms.PictureBox pictureZXDisplay;
    }
}