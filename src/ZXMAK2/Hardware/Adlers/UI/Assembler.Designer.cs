using FastColoredTextBoxNS;

namespace ZXMAK2.Hardware.Adlers.UI
{
    partial class Assembler
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Assembler));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("noname.asm");
            this.txtAsm = new FastColoredTextBoxNS.FastColoredTextBox();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.richCompileMessages = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textSaveFileName = new System.Windows.Forms.TextBox();
            this.textMemAdress = new System.Windows.Forms.TextBox();
            this.checkFile = new System.Windows.Forms.CheckBox();
            this.checkMemory = new System.Windows.Forms.CheckBox();
            this.toolMenu = new System.Windows.Forms.ToolStrip();
            this.compileToolStrip = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveileStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStrip = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.fonttoolStrip = new System.Windows.Forms.ToolStripButton();
            this.colorToolStrip = new System.Windows.Forms.ToolStripButton();
            this.backColortoolStrip = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolCodeLibrary = new System.Windows.Forms.ToolStripButton();
            this.treeViewFiles = new System.Windows.Forms.TreeView();
            ((System.ComponentModel.ISupportInitialize)(this.txtAsm)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.toolMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtAsm
            // 
            this.txtAsm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAsm.AutoCompleteBracketsList = new char[] {
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
            this.txtAsm.AutoScrollMinSize = new System.Drawing.Size(27, 17);
            this.txtAsm.AutoSize = true;
            this.txtAsm.BackBrush = null;
            this.txtAsm.CharHeight = 17;
            this.txtAsm.CharWidth = 8;
            this.txtAsm.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtAsm.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtAsm.Font = new System.Drawing.Font("Consolas", 11F);
            this.txtAsm.IsReplaceMode = false;
            this.txtAsm.Location = new System.Drawing.Point(157, 35);
            this.txtAsm.Name = "txtAsm";
            this.txtAsm.Paddings = new System.Windows.Forms.Padding(0);
            this.txtAsm.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtAsm.Size = new System.Drawing.Size(381, 473);
            this.txtAsm.TabIndex = 0;
            this.txtAsm.Zoom = 100;
            this.txtAsm.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.txtAsm_TextChanged);
            // 
            // btnCompile
            // 
            this.btnCompile.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCompile.Location = new System.Drawing.Point(543, 172);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(85, 23);
            this.btnCompile.TabIndex = 1;
            this.btnCompile.Text = "Compile";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Location = new System.Drawing.Point(543, 201);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // richCompileMessages
            // 
            this.richCompileMessages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richCompileMessages.Location = new System.Drawing.Point(157, 521);
            this.richCompileMessages.Name = "richCompileMessages";
            this.richCompileMessages.Size = new System.Drawing.Size(381, 52);
            this.richCompileMessages.TabIndex = 5;
            this.richCompileMessages.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textSaveFileName);
            this.groupBox1.Controls.Add(this.textMemAdress);
            this.groupBox1.Controls.Add(this.checkFile);
            this.groupBox1.Controls.Add(this.checkMemory);
            this.groupBox1.Location = new System.Drawing.Point(543, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(132, 131);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Compile to:";
            // 
            // textSaveFileName
            // 
            this.textSaveFileName.Enabled = false;
            this.textSaveFileName.Location = new System.Drawing.Point(7, 95);
            this.textSaveFileName.Name = "textSaveFileName";
            this.textSaveFileName.Size = new System.Drawing.Size(119, 20);
            this.textSaveFileName.TabIndex = 3;
            // 
            // textMemAdress
            // 
            this.textMemAdress.Location = new System.Drawing.Point(7, 46);
            this.textMemAdress.Name = "textMemAdress";
            this.textMemAdress.Size = new System.Drawing.Size(119, 20);
            this.textMemAdress.TabIndex = 2;
            this.textMemAdress.Text = "#9C40";
            // 
            // checkFile
            // 
            this.checkFile.AutoSize = true;
            this.checkFile.Location = new System.Drawing.Point(7, 72);
            this.checkFile.Name = "checkFile";
            this.checkFile.Size = new System.Drawing.Size(42, 17);
            this.checkFile.TabIndex = 1;
            this.checkFile.Text = "File";
            this.checkFile.UseVisualStyleBackColor = true;
            // 
            // checkMemory
            // 
            this.checkMemory.AutoSize = true;
            this.checkMemory.Location = new System.Drawing.Point(7, 22);
            this.checkMemory.Name = "checkMemory";
            this.checkMemory.Size = new System.Drawing.Size(63, 17);
            this.checkMemory.TabIndex = 0;
            this.checkMemory.Text = "Memory";
            this.checkMemory.UseVisualStyleBackColor = true;
            this.checkMemory.CheckedChanged += new System.EventHandler(this.checkMemory_CheckedChanged);
            // 
            // toolMenu
            // 
            this.toolMenu.AllowItemReorder = true;
            this.toolMenu.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compileToolStrip,
            this.toolStripSeparator1,
            this.openFileStripButton,
            this.saveileStripButton,
            this.toolStripSeparator3,
            this.settingsToolStrip,
            this.toolStripSeparator2,
            this.fonttoolStrip,
            this.colorToolStrip,
            this.backColortoolStrip,
            this.toolStripSeparator4,
            this.toolCodeLibrary});
            this.toolMenu.Location = new System.Drawing.Point(0, 0);
            this.toolMenu.Name = "toolMenu";
            this.toolMenu.Size = new System.Drawing.Size(687, 35);
            this.toolMenu.TabIndex = 7;
            this.toolMenu.Text = "toolStrip1";
            // 
            // compileToolStrip
            // 
            this.compileToolStrip.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.compileToolStrip.Image = ((System.Drawing.Image)(resources.GetObject("compileToolStrip.Image")));
            this.compileToolStrip.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.compileToolStrip.Name = "compileToolStrip";
            this.compileToolStrip.Size = new System.Drawing.Size(32, 32);
            this.compileToolStrip.Text = "Compile(F5)";
            this.compileToolStrip.Click += new System.EventHandler(this.compileToolStrip_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 35);
            // 
            // openFileStripButton
            // 
            this.openFileStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openFileStripButton.Image = ((System.Drawing.Image)(resources.GetObject("openFileStripButton.Image")));
            this.openFileStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openFileStripButton.Name = "openFileStripButton";
            this.openFileStripButton.Size = new System.Drawing.Size(32, 32);
            this.openFileStripButton.Text = "Open File(Ctrl+O)";
            this.openFileStripButton.Click += new System.EventHandler(this.openFileStripButton_Click);
            // 
            // saveileStripButton
            // 
            this.saveileStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveileStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveileStripButton.Image")));
            this.saveileStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveileStripButton.Name = "saveileStripButton";
            this.saveileStripButton.Size = new System.Drawing.Size(32, 32);
            this.saveileStripButton.Text = "Save File";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 35);
            // 
            // settingsToolStrip
            // 
            this.settingsToolStrip.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.settingsToolStrip.Image = ((System.Drawing.Image)(resources.GetObject("settingsToolStrip.Image")));
            this.settingsToolStrip.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsToolStrip.Name = "settingsToolStrip";
            this.settingsToolStrip.Size = new System.Drawing.Size(32, 32);
            this.settingsToolStrip.Text = "Settings";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 35);
            // 
            // fonttoolStrip
            // 
            this.fonttoolStrip.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fonttoolStrip.Image = ((System.Drawing.Image)(resources.GetObject("fonttoolStrip.Image")));
            this.fonttoolStrip.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fonttoolStrip.Name = "fonttoolStrip";
            this.fonttoolStrip.Size = new System.Drawing.Size(32, 32);
            this.fonttoolStrip.Text = "Font select";
            this.fonttoolStrip.Click += new System.EventHandler(this.fonttoolStrip_Click);
            // 
            // colorToolStrip
            // 
            this.colorToolStrip.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.colorToolStrip.Image = ((System.Drawing.Image)(resources.GetObject("colorToolStrip.Image")));
            this.colorToolStrip.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.colorToolStrip.Name = "colorToolStrip";
            this.colorToolStrip.Size = new System.Drawing.Size(32, 32);
            this.colorToolStrip.Text = "Select text color";
            this.colorToolStrip.ToolTipText = "Select colors";
            this.colorToolStrip.Click += new System.EventHandler(this.colorToolStrip_Click);
            // 
            // backColortoolStrip
            // 
            this.backColortoolStrip.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.backColortoolStrip.Image = ((System.Drawing.Image)(resources.GetObject("backColortoolStrip.Image")));
            this.backColortoolStrip.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.backColortoolStrip.Name = "backColortoolStrip";
            this.backColortoolStrip.Size = new System.Drawing.Size(32, 32);
            this.backColortoolStrip.Text = "toolStripButton2";
            this.backColortoolStrip.ToolTipText = "Select BackGround Color";
            this.backColortoolStrip.Click += new System.EventHandler(this.backColortoolStrip_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 35);
            // 
            // toolCodeLibrary
            // 
            this.toolCodeLibrary.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolCodeLibrary.Image = ((System.Drawing.Image)(resources.GetObject("toolCodeLibrary.Image")));
            this.toolCodeLibrary.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolCodeLibrary.Name = "toolCodeLibrary";
            this.toolCodeLibrary.Size = new System.Drawing.Size(32, 32);
            this.toolCodeLibrary.Text = "toolStripButton1";
            this.toolCodeLibrary.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.toolCodeLibrary.ToolTipText = "Code Library(Includes)";
            // 
            // treeViewFiles
            // 
            this.treeViewFiles.CheckBoxes = true;
            this.treeViewFiles.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeViewFiles.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.treeViewFiles.Location = new System.Drawing.Point(0, 35);
            this.treeViewFiles.Name = "treeViewFiles";
            treeNode1.Name = "Node0";
            treeNode1.Text = "noname.asm";
            this.treeViewFiles.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewFiles.Size = new System.Drawing.Size(151, 538);
            this.treeViewFiles.TabIndex = 8;
            // 
            // Assembler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 573);
            this.Controls.Add(this.treeViewFiles);
            this.Controls.Add(this.toolMenu);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.richCompileMessages);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCompile);
            this.Controls.Add(this.txtAsm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Assembler";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Assembler";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.assemblerForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.txtAsm)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.toolMenu.ResumeLayout(false);
            this.toolMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FastColoredTextBoxNS.FastColoredTextBox txtAsm;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.RichTextBox richCompileMessages;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkFile;
        private System.Windows.Forms.CheckBox checkMemory;
        private System.Windows.Forms.TextBox textSaveFileName;
        private System.Windows.Forms.TextBox textMemAdress;
        private System.Windows.Forms.ToolStrip toolMenu;
        private System.Windows.Forms.ToolStripButton compileToolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton settingsToolStrip;
        private System.Windows.Forms.ToolStripButton fonttoolStrip;
        private System.Windows.Forms.ToolStripButton colorToolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton openFileStripButton;
        private System.Windows.Forms.ToolStripButton backColortoolStrip;
        private System.Windows.Forms.ToolStripButton saveileStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton toolCodeLibrary;
        private System.Windows.Forms.TreeView treeViewFiles;
    }
}