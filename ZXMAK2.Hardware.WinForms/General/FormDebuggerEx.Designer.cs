namespace ZXMAK2.Hardware.WinForms.General
{
    partial class FormDebuggerEx
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDebuggerEx));
            WeifenLuo.WinFormsUI.Docking.DockPanelSkin dockPanelSkin2 = new WeifenLuo.WinFormsUI.Docking.DockPanelSkin();
            WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin autoHideStripSkin2 = new WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient4 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient8 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin dockPaneStripSkin2 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient dockPaneStripGradient2 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient9 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient5 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient10 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient dockPaneStripToolWindowGradient2 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient11 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient12 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient6 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient13 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient14 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSplitter = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindowDisassembly = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindowRegisters = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindowMemory = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindowState = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDebugContinue = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDebugBreak = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDebugSplitter = new System.Windows.Forms.ToolStripSeparator();
            this.menuDebugStepInto = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDebugStepOver = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStrip = new ZXMAK2.Host.WinForms.Controls.ToolStripEx();
            this.toolStripContinue = new System.Windows.Forms.ToolStripButton();
            this.toolStripBreak = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripStepInto = new System.Windows.Forms.ToolStripButton();
            this.toolStripStepOver = new System.Windows.Forms.ToolStripButton();
            this.toolStripStepOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripShowNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripBreakPoints = new System.Windows.Forms.ToolStripButton();
            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.themeVS2012Light = new WeifenLuo.WinFormsUI.Docking.VS2012LightTheme();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuWindow,
            this.menuDebug});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(694, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileLoad,
            this.menuFileSave,
            this.menuFileSplitter,
            this.menuFileClose});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(37, 20);
            this.menuFile.Text = "File";
            // 
            // menuFileLoad
            // 
            this.menuFileLoad.Name = "menuFileLoad";
            this.menuFileLoad.Size = new System.Drawing.Size(109, 22);
            this.menuFileLoad.Text = "Load...";
            // 
            // menuFileSave
            // 
            this.menuFileSave.Name = "menuFileSave";
            this.menuFileSave.Size = new System.Drawing.Size(109, 22);
            this.menuFileSave.Text = "Save...";
            // 
            // menuFileSplitter
            // 
            this.menuFileSplitter.Name = "menuFileSplitter";
            this.menuFileSplitter.Size = new System.Drawing.Size(106, 6);
            // 
            // menuFileClose
            // 
            this.menuFileClose.Name = "menuFileClose";
            this.menuFileClose.Size = new System.Drawing.Size(109, 22);
            this.menuFileClose.Text = "Close";
            // 
            // menuWindow
            // 
            this.menuWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuWindowDisassembly,
            this.menuWindowRegisters,
            this.menuWindowMemory,
            this.menuWindowState});
            this.menuWindow.Name = "menuWindow";
            this.menuWindow.Size = new System.Drawing.Size(63, 20);
            this.menuWindow.Text = "Window";
            // 
            // menuWindowDisassembly
            // 
            this.menuWindowDisassembly.Name = "menuWindowDisassembly";
            this.menuWindowDisassembly.Size = new System.Drawing.Size(139, 22);
            this.menuWindowDisassembly.Text = "Disassembly";
            // 
            // menuWindowRegisters
            // 
            this.menuWindowRegisters.Name = "menuWindowRegisters";
            this.menuWindowRegisters.Size = new System.Drawing.Size(139, 22);
            this.menuWindowRegisters.Text = "Registers";
            // 
            // menuWindowMemory
            // 
            this.menuWindowMemory.Name = "menuWindowMemory";
            this.menuWindowMemory.Size = new System.Drawing.Size(139, 22);
            this.menuWindowMemory.Text = "Memory";
            // 
            // menuWindowState
            // 
            this.menuWindowState.Name = "menuWindowState";
            this.menuWindowState.Size = new System.Drawing.Size(139, 22);
            this.menuWindowState.Text = "State";
            // 
            // menuDebug
            // 
            this.menuDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuDebugContinue,
            this.menuDebugBreak,
            this.menuDebugSplitter,
            this.menuDebugStepInto,
            this.menuDebugStepOver});
            this.menuDebug.Name = "menuDebug";
            this.menuDebug.Size = new System.Drawing.Size(54, 20);
            this.menuDebug.Text = "Debug";
            // 
            // menuDebugContinue
            // 
            this.menuDebugContinue.Name = "menuDebugContinue";
            this.menuDebugContinue.Size = new System.Drawing.Size(125, 22);
            this.menuDebugContinue.Text = "Continue";
            // 
            // menuDebugBreak
            // 
            this.menuDebugBreak.Name = "menuDebugBreak";
            this.menuDebugBreak.Size = new System.Drawing.Size(125, 22);
            this.menuDebugBreak.Text = "Break";
            // 
            // menuDebugSplitter
            // 
            this.menuDebugSplitter.Name = "menuDebugSplitter";
            this.menuDebugSplitter.Size = new System.Drawing.Size(122, 6);
            // 
            // menuDebugStepInto
            // 
            this.menuDebugStepInto.Name = "menuDebugStepInto";
            this.menuDebugStepInto.Size = new System.Drawing.Size(125, 22);
            this.menuDebugStepInto.Text = "Step Into";
            // 
            // menuDebugStepOver
            // 
            this.menuDebugStepOver.Name = "menuDebugStepOver";
            this.menuDebugStepOver.Size = new System.Drawing.Size(125, 22);
            this.menuDebugStepOver.Text = "Step Over";
            // 
            // statusStrip
            // 
            this.statusStrip.Location = new System.Drawing.Point(0, 411);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(694, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripContinue,
            this.toolStripBreak,
            this.toolStripSeparator1,
            this.toolStripStepInto,
            this.toolStripStepOver,
            this.toolStripStepOut,
            this.toolStripSeparator2,
            this.toolStripShowNext,
            this.toolStripSeparator3,
            this.toolStripBreakPoints});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(694, 25);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStripEx1";
            // 
            // toolStripContinue
            // 
            this.toolStripContinue.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripContinue.Image = ((System.Drawing.Image)(resources.GetObject("toolStripContinue.Image")));
            this.toolStripContinue.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripContinue.Name = "toolStripContinue";
            this.toolStripContinue.Size = new System.Drawing.Size(23, 22);
            this.toolStripContinue.Text = "Continue";
            // 
            // toolStripBreak
            // 
            this.toolStripBreak.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBreak.Enabled = false;
            this.toolStripBreak.Image = ((System.Drawing.Image)(resources.GetObject("toolStripBreak.Image")));
            this.toolStripBreak.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripBreak.Name = "toolStripBreak";
            this.toolStripBreak.Size = new System.Drawing.Size(23, 22);
            this.toolStripBreak.Text = "Break";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripStepInto
            // 
            this.toolStripStepInto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripStepInto.Image = ((System.Drawing.Image)(resources.GetObject("toolStripStepInto.Image")));
            this.toolStripStepInto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripStepInto.Name = "toolStripStepInto";
            this.toolStripStepInto.Size = new System.Drawing.Size(23, 22);
            this.toolStripStepInto.Text = "Step Into";
            // 
            // toolStripStepOver
            // 
            this.toolStripStepOver.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripStepOver.Image = ((System.Drawing.Image)(resources.GetObject("toolStripStepOver.Image")));
            this.toolStripStepOver.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripStepOver.Name = "toolStripStepOver";
            this.toolStripStepOver.Size = new System.Drawing.Size(23, 22);
            this.toolStripStepOver.Text = "Step Over";
            // 
            // toolStripStepOut
            // 
            this.toolStripStepOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripStepOut.Image = ((System.Drawing.Image)(resources.GetObject("toolStripStepOut.Image")));
            this.toolStripStepOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripStepOut.Name = "toolStripStepOut";
            this.toolStripStepOut.Size = new System.Drawing.Size(23, 22);
            this.toolStripStepOut.Text = "Step Out";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripShowNext
            // 
            this.toolStripShowNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripShowNext.Image = ((System.Drawing.Image)(resources.GetObject("toolStripShowNext.Image")));
            this.toolStripShowNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripShowNext.Name = "toolStripShowNext";
            this.toolStripShowNext.Size = new System.Drawing.Size(23, 22);
            this.toolStripShowNext.Text = "Show Next Instruction";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripBreakPoints
            // 
            this.toolStripBreakPoints.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBreakPoints.Image = ((System.Drawing.Image)(resources.GetObject("toolStripBreakPoints.Image")));
            this.toolStripBreakPoints.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripBreakPoints.Name = "toolStripBreakPoints";
            this.toolStripBreakPoints.Size = new System.Drawing.Size(23, 22);
            this.toolStripBreakPoints.Text = "toolStripButton1";
            // 
            // dockPanel
            // 
            this.dockPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel.Location = new System.Drawing.Point(0, 49);
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.Size = new System.Drawing.Size(694, 362);
            dockPanelGradient4.EndColor = System.Drawing.SystemColors.ControlLight;
            dockPanelGradient4.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            autoHideStripSkin2.DockStripGradient = dockPanelGradient4;
            tabGradient8.EndColor = System.Drawing.SystemColors.Control;
            tabGradient8.StartColor = System.Drawing.SystemColors.Control;
            tabGradient8.TextColor = System.Drawing.SystemColors.ControlDarkDark;
            autoHideStripSkin2.TabGradient = tabGradient8;
            autoHideStripSkin2.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            dockPanelSkin2.AutoHideStripSkin = autoHideStripSkin2;
            tabGradient9.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(206)))), ((int)(((byte)(219)))));
            tabGradient9.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            tabGradient9.TextColor = System.Drawing.Color.White;
            dockPaneStripGradient2.ActiveTabGradient = tabGradient9;
            dockPanelGradient5.EndColor = System.Drawing.SystemColors.Control;
            dockPanelGradient5.StartColor = System.Drawing.SystemColors.Control;
            dockPaneStripGradient2.DockStripGradient = dockPanelGradient5;
            tabGradient10.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            tabGradient10.StartColor = System.Drawing.SystemColors.Control;
            tabGradient10.TextColor = System.Drawing.Color.Black;
            dockPaneStripGradient2.InactiveTabGradient = tabGradient10;
            dockPaneStripSkin2.DocumentGradient = dockPaneStripGradient2;
            dockPaneStripSkin2.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            tabGradient11.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(170)))), ((int)(((byte)(220)))));
            tabGradient11.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient11.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            tabGradient11.TextColor = System.Drawing.Color.White;
            dockPaneStripToolWindowGradient2.ActiveCaptionGradient = tabGradient11;
            tabGradient12.EndColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient12.StartColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient12.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dockPaneStripToolWindowGradient2.ActiveTabGradient = tabGradient12;
            dockPanelGradient6.EndColor = System.Drawing.SystemColors.Control;
            dockPanelGradient6.StartColor = System.Drawing.SystemColors.Control;
            dockPaneStripToolWindowGradient2.DockStripGradient = dockPanelGradient6;
            tabGradient13.EndColor = System.Drawing.SystemColors.ControlDark;
            tabGradient13.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient13.StartColor = System.Drawing.SystemColors.Control;
            tabGradient13.TextColor = System.Drawing.SystemColors.GrayText;
            dockPaneStripToolWindowGradient2.InactiveCaptionGradient = tabGradient13;
            tabGradient14.EndColor = System.Drawing.SystemColors.Control;
            tabGradient14.StartColor = System.Drawing.SystemColors.Control;
            tabGradient14.TextColor = System.Drawing.SystemColors.GrayText;
            dockPaneStripToolWindowGradient2.InactiveTabGradient = tabGradient14;
            dockPaneStripSkin2.ToolWindowGradient = dockPaneStripToolWindowGradient2;
            dockPanelSkin2.DockPaneStripSkin = dockPaneStripSkin2;
            this.dockPanel.TabIndex = 4;
            this.dockPanel.Theme = this.themeVS2012Light;
            // 
            // FormDebuggerEx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 433);
            this.Controls.Add(this.dockPanel);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "FormDebuggerEx";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DebuggerEx";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem menuFileLoad;
        private System.Windows.Forms.ToolStripMenuItem menuFileSave;
        private System.Windows.Forms.ToolStripSeparator menuFileSplitter;
        private System.Windows.Forms.ToolStripMenuItem menuFileClose;
        private System.Windows.Forms.ToolStripMenuItem menuWindow;
        private System.Windows.Forms.ToolStripMenuItem menuWindowDisassembly;
        private System.Windows.Forms.ToolStripMenuItem menuWindowRegisters;
        private System.Windows.Forms.ToolStripMenuItem menuWindowMemory;
        private System.Windows.Forms.ToolStripMenuItem menuWindowState;
        private System.Windows.Forms.ToolStripMenuItem menuDebug;
        private System.Windows.Forms.ToolStripMenuItem menuDebugContinue;
        private System.Windows.Forms.ToolStripMenuItem menuDebugBreak;
        private System.Windows.Forms.ToolStripSeparator menuDebugSplitter;
        private System.Windows.Forms.ToolStripMenuItem menuDebugStepInto;
        private System.Windows.Forms.ToolStripMenuItem menuDebugStepOver;
        private Host.WinForms.Controls.ToolStripEx toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripContinue;
        private System.Windows.Forms.ToolStripButton toolStripBreak;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripStepInto;
        private System.Windows.Forms.ToolStripButton toolStripStepOver;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripShowNext;
        private System.Windows.Forms.ToolStripButton toolStripStepOut;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripBreakPoints;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
        private WeifenLuo.WinFormsUI.Docking.VS2012LightTheme themeVS2012Light;
    }
}