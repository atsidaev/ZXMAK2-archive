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
            this.groupBoxScreenViewType = new System.Windows.Forms.GroupBox();
            this.comboSpriteHeight = new System.Windows.Forms.ComboBox();
            this.comboSpriteWidth = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.comboDisplayType = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.labelMemoryAddress = new System.Windows.Forms.Label();
            this.numericUpDownActualAddress = new System.Windows.Forms.NumericUpDown();
            this.pictureZXDisplay = new System.Windows.Forms.PictureBox();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numericIncDecDelta = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownZoomFactor = new System.Windows.Forms.NumericUpDown();
            this.groupBoxScreenInfo = new System.Windows.Forms.GroupBox();
            this.textBoxBytesAtAdress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxXCoorYCoor = new System.Windows.Forms.TextBox();
            this.textBoxScreenAddress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureZoomedArea = new System.Windows.Forms.PictureBox();
            this.checkBoxMirror = new System.Windows.Forms.CheckBox();
            this.groupBoxSpriteDetails = new System.Windows.Forms.GroupBox();
            this.textBoxSpriteBytes = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxSpriteAddress = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hexNumbersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bitmapGridSpriteView = new ZXMAK2.Hardware.Adlers.Views.BitmapGrid();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBoxScreenViewType.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownActualAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureZXDisplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericIncDecDelta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZoomFactor)).BeginInit();
            this.groupBoxScreenInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureZoomedArea)).BeginInit();
            this.groupBoxSpriteDetails.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bitmapGridSpriteView)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxScreenViewType
            // 
            this.groupBoxScreenViewType.Controls.Add(this.comboSpriteHeight);
            this.groupBoxScreenViewType.Controls.Add(this.comboSpriteWidth);
            this.groupBoxScreenViewType.Controls.Add(this.label9);
            this.groupBoxScreenViewType.Controls.Add(this.label8);
            this.groupBoxScreenViewType.Controls.Add(this.comboDisplayType);
            this.groupBoxScreenViewType.Controls.Add(this.label7);
            this.groupBoxScreenViewType.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.groupBoxScreenViewType.Location = new System.Drawing.Point(12, 346);
            this.groupBoxScreenViewType.Name = "groupBoxScreenViewType";
            this.groupBoxScreenViewType.Size = new System.Drawing.Size(120, 159);
            this.groupBoxScreenViewType.TabIndex = 25;
            this.groupBoxScreenViewType.TabStop = false;
            this.groupBoxScreenViewType.Text = "Screen View Type:";
            // 
            // comboSpriteHeight
            // 
            this.comboSpriteHeight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSpriteHeight.Enabled = false;
            this.comboSpriteHeight.FormattingEnabled = true;
            this.comboSpriteHeight.Items.AddRange(new object[] {
            "-",
            "8",
            "16",
            "24",
            "32",
            "48",
            "192"});
            this.comboSpriteHeight.Location = new System.Drawing.Point(7, 127);
            this.comboSpriteHeight.Name = "comboSpriteHeight";
            this.comboSpriteHeight.Size = new System.Drawing.Size(104, 23);
            this.comboSpriteHeight.TabIndex = 27;
            // 
            // comboSpriteWidth
            // 
            this.comboSpriteWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSpriteWidth.Enabled = false;
            this.comboSpriteWidth.FormattingEnabled = true;
            this.comboSpriteWidth.Items.AddRange(new object[] {
            "8",
            "16",
            "24",
            "32",
            "40"});
            this.comboSpriteWidth.Location = new System.Drawing.Point(7, 83);
            this.comboSpriteWidth.Name = "comboSpriteWidth";
            this.comboSpriteWidth.Size = new System.Drawing.Size(104, 23);
            this.comboSpriteWidth.TabIndex = 26;
            this.comboSpriteWidth.SelectedIndexChanged += new System.EventHandler(this.comboSpriteWidth_SelectedIndexChanged);
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
            "Robocop style",
            "JetPac style"});
            this.comboDisplayType.Location = new System.Drawing.Point(6, 39);
            this.comboDisplayType.Name = "comboDisplayType";
            this.comboDisplayType.Size = new System.Drawing.Size(107, 23);
            this.comboDisplayType.TabIndex = 23;
            this.comboDisplayType.SelectedIndexChanged += new System.EventHandler(this.comboDisplayType_SelectedIndexChanged);
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
            // labelMemoryAddress
            // 
            this.labelMemoryAddress.AutoSize = true;
            this.labelMemoryAddress.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.labelMemoryAddress.Location = new System.Drawing.Point(9, 28);
            this.labelMemoryAddress.Name = "labelMemoryAddress";
            this.labelMemoryAddress.Size = new System.Drawing.Size(129, 15);
            this.labelMemoryAddress.TabIndex = 26;
            this.labelMemoryAddress.Text = "Memory address(hex):";
            // 
            // numericUpDownActualAddress
            // 
            this.numericUpDownActualAddress.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.numericUpDownActualAddress.Hexadecimal = true;
            this.numericUpDownActualAddress.Increment = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numericUpDownActualAddress.Location = new System.Drawing.Point(12, 47);
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
            this.pictureZXDisplay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureZXDisplay.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureZXDisplay.Image = ((System.Drawing.Image)(resources.GetObject("pictureZXDisplay.Image")));
            this.pictureZXDisplay.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureZXDisplay.InitialImage")));
            this.pictureZXDisplay.Location = new System.Drawing.Point(138, 28);
            this.pictureZXDisplay.Name = "pictureZXDisplay";
            this.pictureZXDisplay.Size = new System.Drawing.Size(512, 384);
            this.pictureZXDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureZXDisplay.TabIndex = 28;
            this.pictureZXDisplay.TabStop = false;
            this.pictureZXDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureZXDisplay_MouseMove);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.buttonRefresh.Location = new System.Drawing.Point(670, 28);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 29;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.buttonClose.Location = new System.Drawing.Point(670, 58);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 30;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(9, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 15);
            this.label1.TabIndex = 31;
            this.label1.Text = "Increase/Decrease by:";
            // 
            // numericIncDecDelta
            // 
            this.numericIncDecDelta.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.numericIncDecDelta.Location = new System.Drawing.Point(84, 103);
            this.numericIncDecDelta.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numericIncDecDelta.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericIncDecDelta.Name = "numericIncDecDelta";
            this.numericIncDecDelta.Size = new System.Drawing.Size(48, 23);
            this.numericIncDecDelta.TabIndex = 32;
            this.numericIncDecDelta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericIncDecDelta.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numericIncDecDelta.ValueChanged += new System.EventHandler(this.numericIncDecDelta_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(12, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 15);
            this.label2.TabIndex = 33;
            this.label2.Text = "Zoom factor:";
            // 
            // numericUpDownZoomFactor
            // 
            this.numericUpDownZoomFactor.Enabled = false;
            this.numericUpDownZoomFactor.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.numericUpDownZoomFactor.Location = new System.Drawing.Point(84, 149);
            this.numericUpDownZoomFactor.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownZoomFactor.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownZoomFactor.Name = "numericUpDownZoomFactor";
            this.numericUpDownZoomFactor.Size = new System.Drawing.Size(48, 23);
            this.numericUpDownZoomFactor.TabIndex = 34;
            this.numericUpDownZoomFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDownZoomFactor.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownZoomFactor.ValueChanged += new System.EventHandler(this.numericUpDownZoomFactor_ValueChanged);
            // 
            // groupBoxScreenInfo
            // 
            this.groupBoxScreenInfo.Controls.Add(this.textBoxBytesAtAdress);
            this.groupBoxScreenInfo.Controls.Add(this.label5);
            this.groupBoxScreenInfo.Controls.Add(this.label4);
            this.groupBoxScreenInfo.Controls.Add(this.textBoxXCoorYCoor);
            this.groupBoxScreenInfo.Controls.Add(this.textBoxScreenAddress);
            this.groupBoxScreenInfo.Controls.Add(this.label3);
            this.groupBoxScreenInfo.Location = new System.Drawing.Point(138, 429);
            this.groupBoxScreenInfo.Name = "groupBoxScreenInfo";
            this.groupBoxScreenInfo.Size = new System.Drawing.Size(512, 76);
            this.groupBoxScreenInfo.TabIndex = 35;
            this.groupBoxScreenInfo.TabStop = false;
            this.groupBoxScreenInfo.Text = "Data under cursor";
            // 
            // textBoxBytesAtAdress
            // 
            this.textBoxBytesAtAdress.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxBytesAtAdress.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBytesAtAdress.Location = new System.Drawing.Point(159, 17);
            this.textBoxBytesAtAdress.Name = "textBoxBytesAtAdress";
            this.textBoxBytesAtAdress.Size = new System.Drawing.Size(136, 23);
            this.textBoxBytesAtAdress.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(119, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "Bytes:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(26, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "[X;Y]";
            // 
            // textBoxXCoorYCoor
            // 
            this.textBoxXCoorYCoor.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxXCoorYCoor.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxXCoorYCoor.Location = new System.Drawing.Point(60, 47);
            this.textBoxXCoorYCoor.Name = "textBoxXCoorYCoor";
            this.textBoxXCoorYCoor.Size = new System.Drawing.Size(53, 23);
            this.textBoxXCoorYCoor.TabIndex = 2;
            // 
            // textBoxScreenAddress
            // 
            this.textBoxScreenAddress.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxScreenAddress.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxScreenAddress.Location = new System.Drawing.Point(60, 17);
            this.textBoxScreenAddress.Name = "textBoxScreenAddress";
            this.textBoxScreenAddress.Size = new System.Drawing.Size(53, 23);
            this.textBoxScreenAddress.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(7, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "Address:";
            // 
            // pictureZoomedArea
            // 
            this.pictureZoomedArea.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureZoomedArea.Location = new System.Drawing.Point(12, 248);
            this.pictureZoomedArea.Name = "pictureZoomedArea";
            this.pictureZoomedArea.Size = new System.Drawing.Size(120, 96);
            this.pictureZoomedArea.TabIndex = 36;
            this.pictureZoomedArea.TabStop = false;
            // 
            // checkBoxMirror
            // 
            this.checkBoxMirror.AutoSize = true;
            this.checkBoxMirror.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxMirror.Location = new System.Drawing.Point(15, 179);
            this.checkBoxMirror.Name = "checkBoxMirror";
            this.checkBoxMirror.Size = new System.Drawing.Size(99, 19);
            this.checkBoxMirror.TabIndex = 37;
            this.checkBoxMirror.Text = "Mirror image";
            this.checkBoxMirror.UseVisualStyleBackColor = true;
            this.checkBoxMirror.CheckedChanged += new System.EventHandler(this.checkBoxMirror_CheckedChanged);
            // 
            // groupBoxSpriteDetails
            // 
            this.groupBoxSpriteDetails.Controls.Add(this.bitmapGridSpriteView);
            this.groupBoxSpriteDetails.Controls.Add(this.textBoxSpriteBytes);
            this.groupBoxSpriteDetails.Controls.Add(this.label11);
            this.groupBoxSpriteDetails.Controls.Add(this.label6);
            this.groupBoxSpriteDetails.Controls.Add(this.textBoxSpriteAddress);
            this.groupBoxSpriteDetails.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxSpriteDetails.Location = new System.Drawing.Point(400, 28);
            this.groupBoxSpriteDetails.Name = "groupBoxSpriteDetails";
            this.groupBoxSpriteDetails.Size = new System.Drawing.Size(250, 300);
            this.groupBoxSpriteDetails.TabIndex = 38;
            this.groupBoxSpriteDetails.TabStop = false;
            this.groupBoxSpriteDetails.Text = "Sprite details:";
            this.groupBoxSpriteDetails.Visible = false;
            // 
            // textBoxSpriteBytes
            // 
            this.textBoxSpriteBytes.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxSpriteBytes.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSpriteBytes.Location = new System.Drawing.Point(105, 46);
            this.textBoxSpriteBytes.Name = "textBoxSpriteBytes";
            this.textBoxSpriteBytes.Size = new System.Drawing.Size(136, 23);
            this.textBoxSpriteBytes.TabIndex = 9;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(45, 25);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(54, 15);
            this.label11.TabIndex = 6;
            this.label11.Text = "Address:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(6, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 15);
            this.label6.TabIndex = 8;
            this.label6.Text = "Bytes at address:";
            // 
            // textBoxSpriteAddress
            // 
            this.textBoxSpriteAddress.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxSpriteAddress.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSpriteAddress.Location = new System.Drawing.Point(105, 17);
            this.textBoxSpriteAddress.Name = "textBoxSpriteAddress";
            this.textBoxSpriteAddress.Size = new System.Drawing.Size(136, 23);
            this.textBoxSpriteAddress.TabIndex = 7;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(750, 24);
            this.menuStrip1.TabIndex = 39;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hexNumbersToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // hexNumbersToolStripMenuItem
            // 
            this.hexNumbersToolStripMenuItem.Checked = true;
            this.hexNumbersToolStripMenuItem.CheckOnClick = true;
            this.hexNumbersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.hexNumbersToolStripMenuItem.Name = "hexNumbersToolStripMenuItem";
            this.hexNumbersToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.hexNumbersToolStripMenuItem.Text = "Hex numbers";
            this.hexNumbersToolStripMenuItem.Click += new System.EventHandler(this.hexNumbersToolStripMenuItem_Click);
            // 
            // bitmapGridSpriteView
            // 
            this.bitmapGridSpriteView.AllowUserToAddRows = false;
            this.bitmapGridSpriteView.AllowUserToDeleteRows = false;
            this.bitmapGridSpriteView.AllowUserToResizeColumns = false;
            this.bitmapGridSpriteView.AllowUserToResizeRows = false;
            this.bitmapGridSpriteView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.bitmapGridSpriteView.ColumnHeadersVisible = false;
            this.bitmapGridSpriteView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.dataGridViewTextBoxColumn8,
            this.dataGridViewTextBoxColumn9,
            this.dataGridViewTextBoxColumn10,
            this.dataGridViewTextBoxColumn11,
            this.dataGridViewTextBoxColumn12,
            this.dataGridViewTextBoxColumn13,
            this.dataGridViewTextBoxColumn14,
            this.dataGridViewTextBoxColumn15,
            this.dataGridViewTextBoxColumn16});
            this.bitmapGridSpriteView.Location = new System.Drawing.Point(5, 72);
            this.bitmapGridSpriteView.Name = "bitmapGridSpriteView";
            this.bitmapGridSpriteView.RowHeadersVisible = false;
            this.bitmapGridSpriteView.RowTemplate.Height = 8;
            this.bitmapGridSpriteView.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.bitmapGridSpriteView.Size = new System.Drawing.Size(240, 222);
            this.bitmapGridSpriteView.TabIndex = 10;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 15;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Width = 15;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.Width = 14;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.Width = 15;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.Width = 15;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.Width = 15;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.Width = 15;
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.Width = 15;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.Width = 14;
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            this.dataGridViewTextBoxColumn10.Width = 15;
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            this.dataGridViewTextBoxColumn11.Width = 15;
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            this.dataGridViewTextBoxColumn12.Width = 15;
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            this.dataGridViewTextBoxColumn13.Width = 15;
            // 
            // dataGridViewTextBoxColumn14
            // 
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            this.dataGridViewTextBoxColumn14.Width = 14;
            // 
            // dataGridViewTextBoxColumn15
            // 
            this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            this.dataGridViewTextBoxColumn15.Width = 15;
            // 
            // dataGridViewTextBoxColumn16
            // 
            this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
            this.dataGridViewTextBoxColumn16.Width = 15;
            // 
            // GraphicsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 517);
            this.Controls.Add(this.groupBoxSpriteDetails);
            this.Controls.Add(this.checkBoxMirror);
            this.Controls.Add(this.pictureZoomedArea);
            this.Controls.Add(this.groupBoxScreenInfo);
            this.Controls.Add(this.numericUpDownZoomFactor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericIncDecDelta);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.pictureZXDisplay);
            this.Controls.Add(this.numericUpDownActualAddress);
            this.Controls.Add(this.labelMemoryAddress);
            this.Controls.Add(this.groupBoxScreenViewType);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "GraphicsEditor";
            this.Text = "GraphicsEditor";
            this.groupBoxScreenViewType.ResumeLayout(false);
            this.groupBoxScreenViewType.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownActualAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureZXDisplay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericIncDecDelta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZoomFactor)).EndInit();
            this.groupBoxScreenInfo.ResumeLayout(false);
            this.groupBoxScreenInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureZoomedArea)).EndInit();
            this.groupBoxSpriteDetails.ResumeLayout(false);
            this.groupBoxSpriteDetails.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bitmapGridSpriteView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxScreenViewType;
        private System.Windows.Forms.ComboBox comboSpriteHeight;
        private System.Windows.Forms.ComboBox comboSpriteWidth;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboDisplayType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelMemoryAddress;
        private System.Windows.Forms.NumericUpDown numericUpDownActualAddress;
        private System.Windows.Forms.PictureBox pictureZXDisplay;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericIncDecDelta;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownZoomFactor;
        private System.Windows.Forms.GroupBox groupBoxScreenInfo;
        private System.Windows.Forms.TextBox textBoxScreenAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxXCoorYCoor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxBytesAtAdress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureZoomedArea;
        private System.Windows.Forms.CheckBox checkBoxMirror;
        private System.Windows.Forms.GroupBox groupBoxSpriteDetails;
        private System.Windows.Forms.TextBox textBoxSpriteBytes;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxSpriteAddress;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hexNumbersToolStripMenuItem;
        private Hardware.Adlers.Views.BitmapGrid bitmapGridSpriteView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
    }
}