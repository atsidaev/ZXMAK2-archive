using FastColoredTextBoxNS;

namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
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
            this.components = new System.ComponentModel.Container();
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
            this.chckbxMemory = new System.Windows.Forms.CheckBox();
            this.toolMenu = new System.Windows.Forms.ToolStrip();
            this.toolStripNewSource = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.compileToolStrip = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveFileStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStrip = new System.Windows.Forms.ToolStripButton();
            this.toolStripColors = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolCodeLibrary = new System.Windows.Forms.ToolStripButton();
            this.treeViewFiles = new System.Windows.Forms.TreeView();
            this.buttonClearAssemblerLog = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.ctxMenuAssemblerCommands = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnFormatCode = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.txtAsm)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.toolMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.ctxMenuAssemblerCommands.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtAsm
            // 
            this.txtAsm.AutoCompleteBrackets = true;
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
            this.txtAsm.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+\\s*(?<range>,)\\s*(?<range>[^;]+);";
            this.txtAsm.AutoScrollMinSize = new System.Drawing.Size(27, 17);
            this.txtAsm.AutoSize = true;
            this.txtAsm.BackBrush = null;
            this.txtAsm.CharHeight = 17;
            this.txtAsm.CharWidth = 8;
            this.txtAsm.CommentPrefix = ";";
            this.txtAsm.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtAsm.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtAsm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAsm.Font = new System.Drawing.Font("Consolas", 11F);
            this.txtAsm.IsReplaceMode = false;
            this.txtAsm.Location = new System.Drawing.Point(0, 0);
            this.txtAsm.Name = "txtAsm";
            this.txtAsm.Paddings = new System.Windows.Forms.Padding(0);
            this.txtAsm.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtAsm.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txtAsm.ServiceColors")));
            this.txtAsm.Size = new System.Drawing.Size(527, 579);
            this.txtAsm.TabIndex = 0;
            this.txtAsm.Zoom = 100;
            this.txtAsm.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.txtAsm_TextChanged);
            this.txtAsm.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtAsm_MouseClick);
            // 
            // btnCompile
            // 
            this.btnCompile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompile.Location = new System.Drawing.Point(691, 172);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(85, 23);
            this.btnCompile.TabIndex = 1;
            this.btnCompile.Text = "Compile";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(691, 201);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // richCompileMessages
            // 
            this.richCompileMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richCompileMessages.Location = new System.Drawing.Point(0, 0);
            this.richCompileMessages.Name = "richCompileMessages";
            this.richCompileMessages.Size = new System.Drawing.Size(527, 85);
            this.richCompileMessages.TabIndex = 5;
            this.richCompileMessages.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textSaveFileName);
            this.groupBox1.Controls.Add(this.textMemAdress);
            this.groupBox1.Controls.Add(this.checkFile);
            this.groupBox1.Controls.Add(this.chckbxMemory);
            this.groupBox1.Location = new System.Drawing.Point(691, 35);
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
            // chckbxMemory
            // 
            this.chckbxMemory.AutoSize = true;
            this.chckbxMemory.Checked = true;
            this.chckbxMemory.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chckbxMemory.Location = new System.Drawing.Point(7, 22);
            this.chckbxMemory.Name = "chckbxMemory";
            this.chckbxMemory.Size = new System.Drawing.Size(63, 17);
            this.chckbxMemory.TabIndex = 0;
            this.chckbxMemory.Text = "Memory";
            this.chckbxMemory.UseVisualStyleBackColor = true;
            this.chckbxMemory.CheckedChanged += new System.EventHandler(this.checkMemory_CheckedChanged);
            // 
            // toolMenu
            // 
            this.toolMenu.AllowItemReorder = true;
            this.toolMenu.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripNewSource,
            this.toolStripSeparator2,
            this.compileToolStrip,
            this.toolStripButtonRefresh,
            this.toolStripSeparator1,
            this.openFileStripButton,
            this.saveFileStripButton,
            this.toolStripSeparator3,
            this.settingsToolStrip,
            this.toolStripColors,
            this.toolStripSeparator5,
            this.toolCodeLibrary});
            this.toolMenu.Location = new System.Drawing.Point(0, 0);
            this.toolMenu.Name = "toolMenu";
            this.toolMenu.Size = new System.Drawing.Size(835, 35);
            this.toolMenu.TabIndex = 7;
            this.toolMenu.Text = "toolStrip1";
            // 
            // toolStripNewSource
            // 
            this.toolStripNewSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripNewSource.Image = ((System.Drawing.Image)(resources.GetObject("toolStripNewSource.Image")));
            this.toolStripNewSource.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripNewSource.Name = "toolStripNewSource";
            this.toolStripNewSource.Size = new System.Drawing.Size(32, 32);
            this.toolStripNewSource.ToolTipText = "New assembler source";
            this.toolStripNewSource.Click += new System.EventHandler(this.toolStripNewSource_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 35);
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
            // toolStripButtonRefresh
            // 
            this.toolStripButtonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRefresh.Image")));
            this.toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
            this.toolStripButtonRefresh.Size = new System.Drawing.Size(32, 32);
            this.toolStripButtonRefresh.Text = "Refresh";
            this.toolStripButtonRefresh.Click += new System.EventHandler(this.toolStripButtonRefresh_Click);
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
            // saveFileStripButton
            // 
            this.saveFileStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveFileStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveFileStripButton.Image")));
            this.saveFileStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveFileStripButton.Name = "saveFileStripButton";
            this.saveFileStripButton.Size = new System.Drawing.Size(32, 32);
            this.saveFileStripButton.Text = "Save File(Ctrl+S)";
            this.saveFileStripButton.Click += new System.EventHandler(this.saveFileStripButton_Click);
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
            // toolStripColors
            // 
            this.toolStripColors.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripColors.Image = ((System.Drawing.Image)(resources.GetObject("toolStripColors.Image")));
            this.toolStripColors.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripColors.Name = "toolStripColors";
            this.toolStripColors.Size = new System.Drawing.Size(32, 32);
            this.toolStripColors.Text = "Select text color";
            this.toolStripColors.ToolTipText = "Colors";
            this.toolStripColors.Click += new System.EventHandler(this.toolStripColors_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 35);
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
            this.toolCodeLibrary.Click += new System.EventHandler(this.toolCodeLibrary_Click);
            // 
            // treeViewFiles
            // 
            this.treeViewFiles.CheckBoxes = true;
            this.treeViewFiles.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeViewFiles.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.treeViewFiles.HideSelection = false;
            this.treeViewFiles.LabelEdit = true;
            this.treeViewFiles.Location = new System.Drawing.Point(0, 35);
            this.treeViewFiles.Name = "treeViewFiles";
            treeNode1.Name = "Node0";
            treeNode1.Tag = "0";
            treeNode1.Text = "noname.asm";
            treeNode1.ToolTipText = "not save assembler code";
            this.treeViewFiles.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewFiles.ShowNodeToolTips = true;
            this.treeViewFiles.Size = new System.Drawing.Size(151, 671);
            this.treeViewFiles.TabIndex = 8;
            this.treeViewFiles.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewFiles_AfterLabelEdit);
            this.treeViewFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewFiles_AfterSelect);
            this.treeViewFiles.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeViewFiles_KeyUp);
            // 
            // buttonClearAssemblerLog
            // 
            this.buttonClearAssemblerLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearAssemblerLog.Location = new System.Drawing.Point(691, 619);
            this.buttonClearAssemblerLog.Name = "buttonClearAssemblerLog";
            this.buttonClearAssemblerLog.Size = new System.Drawing.Size(75, 23);
            this.buttonClearAssemblerLog.TabIndex = 10;
            this.buttonClearAssemblerLog.Text = "Clear log";
            this.buttonClearAssemblerLog.UseVisualStyleBackColor = true;
            this.buttonClearAssemblerLog.Click += new System.EventHandler(this.buttonClearAssemblerLog_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(157, 38);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtAsm);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.richCompileMessages);
            this.splitContainer1.Size = new System.Drawing.Size(527, 668);
            this.splitContainer1.SplitterDistance = 579;
            this.splitContainer1.TabIndex = 11;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(151, 35);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 671);
            this.splitter1.TabIndex = 12;
            this.splitter1.TabStop = false;
            // 
            // ctxMenuAssemblerCommands
            // 
            this.ctxMenuAssemblerCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnFormatCode});
            this.ctxMenuAssemblerCommands.Name = "ctxMenuAssemblerCommands";
            this.ctxMenuAssemblerCommands.Size = new System.Drawing.Size(144, 26);
            // 
            // btnFormatCode
            // 
            this.btnFormatCode.Name = "btnFormatCode";
            this.btnFormatCode.Size = new System.Drawing.Size(143, 22);
            this.btnFormatCode.Text = "Format Code";
            this.btnFormatCode.Click += new System.EventHandler(this.btnFormatCode_Click);
            // 
            // Assembler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(835, 706);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.buttonClearAssemblerLog);
            this.Controls.Add(this.treeViewFiles);
            this.Controls.Add(this.toolMenu);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCompile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "Assembler";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Assembler";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Assembler_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.assemblerForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.txtAsm)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.toolMenu.ResumeLayout(false);
            this.toolMenu.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ctxMenuAssemblerCommands.ResumeLayout(false);
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
        private System.Windows.Forms.CheckBox chckbxMemory;
        private System.Windows.Forms.TextBox textSaveFileName;
        private System.Windows.Forms.TextBox textMemAdress;
        private System.Windows.Forms.ToolStrip toolMenu;
        private System.Windows.Forms.ToolStripButton compileToolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton settingsToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripColors;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton openFileStripButton;
        private System.Windows.Forms.ToolStripButton saveFileStripButton;
        private System.Windows.Forms.ToolStripButton toolCodeLibrary;
        private System.Windows.Forms.TreeView treeViewFiles;
        private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.Button buttonClearAssemblerLog;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ToolStripButton toolStripNewSource;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ContextMenuStrip ctxMenuAssemblerCommands;
        private System.Windows.Forms.ToolStripMenuItem btnFormatCode;
    }
}