namespace ZXMAK2.MVP.WinForms
{
    partial class MainView
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
            this.mnuStrip = new ZXMAK2.Controls.MenuStripEx();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuView = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewCustomize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewCustomizeShowToolBar = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewCustomizeShowStatusBar = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuViewSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuViewSize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewSizeX1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewSizeX2 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewSizeX3 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewSizeX4 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewFullScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuViewScaleMode = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewScaleModeStretch = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewScaleModeKeepProportion = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewScaleModeFixedPixelSize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuViewSmoothing = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewNoFlic = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewVBlankSync = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewDisplayIcon = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewDebugInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVm = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVmPause = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVmMaximumSpeed = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVmSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuVmWarmReset = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVmColdReset = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVmNmi = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVmSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuVmSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpViewHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpKeyboardHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.tbrStrip = new ZXMAK2.Controls.ToolStripEx();
            this.tbrButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.tbrButtonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tbrButtonPause = new System.Windows.Forms.ToolStripButton();
            this.tbrButtonMaxSpeed = new System.Windows.Forms.ToolStripButton();
            this.tbrButtonWarmReset = new System.Windows.Forms.ToolStripButton();
            this.tbrButtonColdReset = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tbrButtonFullScreen = new System.Windows.Forms.ToolStripButton();
            this.tbrButtonQuickLoad = new System.Windows.Forms.ToolStripButton();
            this.tbrButtonSettings = new System.Windows.Forms.ToolStripButton();
            this.sbrStrip = new System.Windows.Forms.StatusStrip();
            this.renderVideo = new ZXMAK2.MVP.WinForms.RenderVideo();
            this.mnuStrip.SuspendLayout();
            this.tbrStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuStrip
            // 
            this.mnuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuView,
            this.menuVm,
            this.menuTools,
            this.menuHelp});
            this.mnuStrip.Location = new System.Drawing.Point(0, 0);
            this.mnuStrip.Name = "mnuStrip";
            this.mnuStrip.Size = new System.Drawing.Size(704, 24);
            this.mnuStrip.TabIndex = 0;
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileOpen,
            this.menuFileSaveAs,
            this.menuFileSeparator,
            this.menuFileExit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(37, 20);
            this.menuFile.Text = "File";
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.Name = "menuFileOpen";
            this.menuFileOpen.Size = new System.Drawing.Size(123, 22);
            this.menuFileOpen.Text = "Open...";
            // 
            // menuFileSaveAs
            // 
            this.menuFileSaveAs.Name = "menuFileSaveAs";
            this.menuFileSaveAs.Size = new System.Drawing.Size(123, 22);
            this.menuFileSaveAs.Text = "Save As...";
            // 
            // menuFileSeparator
            // 
            this.menuFileSeparator.Name = "menuFileSeparator";
            this.menuFileSeparator.Size = new System.Drawing.Size(120, 6);
            // 
            // menuFileExit
            // 
            this.menuFileExit.Name = "menuFileExit";
            this.menuFileExit.Size = new System.Drawing.Size(123, 22);
            this.menuFileExit.Text = "Exit";
            // 
            // menuView
            // 
            this.menuView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.menuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewCustomize,
            this.MenuViewSeparator1,
            this.menuViewSize,
            this.menuViewFullScreen,
            this.menuViewSeparator2,
            this.menuViewScaleMode,
            this.menuViewSeparator3,
            this.menuViewSmoothing,
            this.menuViewNoFlic,
            this.menuViewVBlankSync,
            this.menuViewDisplayIcon,
            this.menuViewDebugInfo});
            this.menuView.Name = "menuView";
            this.menuView.Size = new System.Drawing.Size(44, 20);
            this.menuView.Text = "View";
            this.menuView.DropDownOpening += new System.EventHandler(this.menuView_DropDownOpening);
            // 
            // menuViewCustomize
            // 
            this.menuViewCustomize.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewCustomizeShowToolBar,
            this.menuViewCustomizeShowStatusBar});
            this.menuViewCustomize.Name = "menuViewCustomize";
            this.menuViewCustomize.Size = new System.Drawing.Size(188, 22);
            this.menuViewCustomize.Text = "Customize";
            // 
            // menuViewCustomizeShowToolBar
            // 
            this.menuViewCustomizeShowToolBar.CheckOnClick = true;
            this.menuViewCustomizeShowToolBar.Name = "menuViewCustomizeShowToolBar";
            this.menuViewCustomizeShowToolBar.Size = new System.Drawing.Size(126, 22);
            this.menuViewCustomizeShowToolBar.Text = "Tool Bar";
            this.menuViewCustomizeShowToolBar.Click += new System.EventHandler(this.menuViewCustomize_Click);
            // 
            // menuViewCustomizeShowStatusBar
            // 
            this.menuViewCustomizeShowStatusBar.CheckOnClick = true;
            this.menuViewCustomizeShowStatusBar.Name = "menuViewCustomizeShowStatusBar";
            this.menuViewCustomizeShowStatusBar.Size = new System.Drawing.Size(126, 22);
            this.menuViewCustomizeShowStatusBar.Text = "Status Bar";
            this.menuViewCustomizeShowStatusBar.Click += new System.EventHandler(this.menuViewCustomize_Click);
            // 
            // MenuViewSeparator1
            // 
            this.MenuViewSeparator1.Name = "MenuViewSeparator1";
            this.MenuViewSeparator1.Size = new System.Drawing.Size(185, 6);
            // 
            // menuViewSize
            // 
            this.menuViewSize.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewSizeX1,
            this.menuViewSizeX2,
            this.menuViewSizeX3,
            this.menuViewSizeX4});
            this.menuViewSize.Name = "menuViewSize";
            this.menuViewSize.Size = new System.Drawing.Size(188, 22);
            this.menuViewSize.Text = "Size";
            // 
            // menuViewSizeX1
            // 
            this.menuViewSizeX1.Name = "menuViewSizeX1";
            this.menuViewSizeX1.Size = new System.Drawing.Size(102, 22);
            this.menuViewSizeX1.Text = "100%";
            this.menuViewSizeX1.Click += new System.EventHandler(this.menuViewX_Click);
            // 
            // menuViewSizeX2
            // 
            this.menuViewSizeX2.Name = "menuViewSizeX2";
            this.menuViewSizeX2.Size = new System.Drawing.Size(102, 22);
            this.menuViewSizeX2.Text = "200%";
            this.menuViewSizeX2.Click += new System.EventHandler(this.menuViewX_Click);
            // 
            // menuViewSizeX3
            // 
            this.menuViewSizeX3.Name = "menuViewSizeX3";
            this.menuViewSizeX3.Size = new System.Drawing.Size(102, 22);
            this.menuViewSizeX3.Text = "300%";
            this.menuViewSizeX3.Click += new System.EventHandler(this.menuViewX_Click);
            // 
            // menuViewSizeX4
            // 
            this.menuViewSizeX4.Name = "menuViewSizeX4";
            this.menuViewSizeX4.Size = new System.Drawing.Size(102, 22);
            this.menuViewSizeX4.Text = "400%";
            this.menuViewSizeX4.Click += new System.EventHandler(this.menuViewX_Click);
            // 
            // menuViewFullScreen
            // 
            this.menuViewFullScreen.Name = "menuViewFullScreen";
            this.menuViewFullScreen.ShortcutKeyDisplayString = "Alt+Enter";
            this.menuViewFullScreen.Size = new System.Drawing.Size(188, 22);
            this.menuViewFullScreen.Text = "Full Screen";
            // 
            // menuViewSeparator2
            // 
            this.menuViewSeparator2.Name = "menuViewSeparator2";
            this.menuViewSeparator2.Size = new System.Drawing.Size(185, 6);
            // 
            // menuViewScaleMode
            // 
            this.menuViewScaleMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewScaleModeStretch,
            this.menuViewScaleModeKeepProportion,
            this.menuViewScaleModeFixedPixelSize});
            this.menuViewScaleMode.Name = "menuViewScaleMode";
            this.menuViewScaleMode.Size = new System.Drawing.Size(188, 22);
            this.menuViewScaleMode.Text = "Scale Mode";
            // 
            // menuViewScaleModeStretch
            // 
            this.menuViewScaleModeStretch.Name = "menuViewScaleModeStretch";
            this.menuViewScaleModeStretch.Size = new System.Drawing.Size(160, 22);
            this.menuViewScaleModeStretch.Text = "Stretch";
            this.menuViewScaleModeStretch.Click += new System.EventHandler(this.menuViewRender_Click);
            // 
            // menuViewScaleModeKeepProportion
            // 
            this.menuViewScaleModeKeepProportion.Name = "menuViewScaleModeKeepProportion";
            this.menuViewScaleModeKeepProportion.Size = new System.Drawing.Size(160, 22);
            this.menuViewScaleModeKeepProportion.Text = "Keep Proportion";
            this.menuViewScaleModeKeepProportion.Click += new System.EventHandler(this.menuViewRender_Click);
            // 
            // menuViewScaleModeFixedPixelSize
            // 
            this.menuViewScaleModeFixedPixelSize.Name = "menuViewScaleModeFixedPixelSize";
            this.menuViewScaleModeFixedPixelSize.Size = new System.Drawing.Size(160, 22);
            this.menuViewScaleModeFixedPixelSize.Text = "Fixed Pixel Size";
            this.menuViewScaleModeFixedPixelSize.Click += new System.EventHandler(this.menuViewRender_Click);
            // 
            // menuViewSeparator3
            // 
            this.menuViewSeparator3.Name = "menuViewSeparator3";
            this.menuViewSeparator3.Size = new System.Drawing.Size(185, 6);
            // 
            // menuViewSmoothing
            // 
            this.menuViewSmoothing.CheckOnClick = true;
            this.menuViewSmoothing.Name = "menuViewSmoothing";
            this.menuViewSmoothing.Size = new System.Drawing.Size(188, 22);
            this.menuViewSmoothing.Text = "Smoothing";
            this.menuViewSmoothing.Click += new System.EventHandler(this.menuViewRender_Click);
            // 
            // menuViewNoFlic
            // 
            this.menuViewNoFlic.CheckOnClick = true;
            this.menuViewNoFlic.Name = "menuViewNoFlic";
            this.menuViewNoFlic.Size = new System.Drawing.Size(188, 22);
            this.menuViewNoFlic.Text = "No Flic";
            this.menuViewNoFlic.Click += new System.EventHandler(this.menuViewRender_Click);
            // 
            // menuViewVBlankSync
            // 
            this.menuViewVBlankSync.Name = "menuViewVBlankSync";
            this.menuViewVBlankSync.Size = new System.Drawing.Size(188, 22);
            this.menuViewVBlankSync.Text = "VBlank Sync";
            this.menuViewVBlankSync.Click += new System.EventHandler(this.menuViewRender_Click);
            // 
            // menuViewDisplayIcon
            // 
            this.menuViewDisplayIcon.CheckOnClick = true;
            this.menuViewDisplayIcon.Name = "menuViewDisplayIcon";
            this.menuViewDisplayIcon.Size = new System.Drawing.Size(188, 22);
            this.menuViewDisplayIcon.Text = "Display Icons";
            this.menuViewDisplayIcon.Click += new System.EventHandler(this.menuViewRender_Click);
            // 
            // menuViewDebugInfo
            // 
            this.menuViewDebugInfo.CheckOnClick = true;
            this.menuViewDebugInfo.Name = "menuViewDebugInfo";
            this.menuViewDebugInfo.Size = new System.Drawing.Size(188, 22);
            this.menuViewDebugInfo.Text = "Debug Info";
            this.menuViewDebugInfo.Click += new System.EventHandler(this.menuViewRender_Click);
            // 
            // menuVm
            // 
            this.menuVm.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuVmPause,
            this.menuVmMaximumSpeed,
            this.menuVmSeparator1,
            this.menuVmWarmReset,
            this.menuVmColdReset,
            this.menuVmNmi,
            this.menuVmSeparator2,
            this.menuVmSettings});
            this.menuVm.Name = "menuVm";
            this.menuVm.Size = new System.Drawing.Size(37, 20);
            this.menuVm.Text = "VM";
            // 
            // menuVmPause
            // 
            this.menuVmPause.Name = "menuVmPause";
            this.menuVmPause.ShortcutKeyDisplayString = "Pause";
            this.menuVmPause.Size = new System.Drawing.Size(226, 22);
            this.menuVmPause.Text = "Pause";
            // 
            // menuVmMaximumSpeed
            // 
            this.menuVmMaximumSpeed.Name = "menuVmMaximumSpeed";
            this.menuVmMaximumSpeed.ShortcutKeyDisplayString = "Ctrl+Scroll";
            this.menuVmMaximumSpeed.Size = new System.Drawing.Size(226, 22);
            this.menuVmMaximumSpeed.Text = "Maximum Speed";
            // 
            // menuVmSeparator1
            // 
            this.menuVmSeparator1.Name = "menuVmSeparator1";
            this.menuVmSeparator1.Size = new System.Drawing.Size(223, 6);
            // 
            // menuVmWarmReset
            // 
            this.menuVmWarmReset.Name = "menuVmWarmReset";
            this.menuVmWarmReset.ShortcutKeyDisplayString = "Alt+Ctrl+Insert";
            this.menuVmWarmReset.Size = new System.Drawing.Size(226, 22);
            this.menuVmWarmReset.Text = "Warm Reset";
            // 
            // menuVmColdReset
            // 
            this.menuVmColdReset.Name = "menuVmColdReset";
            this.menuVmColdReset.Size = new System.Drawing.Size(226, 22);
            this.menuVmColdReset.Text = "Cold Reset";
            // 
            // menuVmNmi
            // 
            this.menuVmNmi.Name = "menuVmNmi";
            this.menuVmNmi.Size = new System.Drawing.Size(226, 22);
            this.menuVmNmi.Text = "NMI";
            // 
            // menuVmSeparator2
            // 
            this.menuVmSeparator2.Name = "menuVmSeparator2";
            this.menuVmSeparator2.Size = new System.Drawing.Size(223, 6);
            // 
            // menuVmSettings
            // 
            this.menuVmSettings.Name = "menuVmSettings";
            this.menuVmSettings.Size = new System.Drawing.Size(226, 22);
            this.menuVmSettings.Text = "Settings";
            // 
            // menuTools
            // 
            this.menuTools.Name = "menuTools";
            this.menuTools.Size = new System.Drawing.Size(48, 20);
            this.menuTools.Text = "Tools";
            // 
            // menuHelp
            // 
            this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHelpViewHelp,
            this.menuHelpKeyboardHelp,
            this.menuHelpSeparator,
            this.menuHelpAbout});
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Size = new System.Drawing.Size(44, 20);
            this.menuHelp.Text = "Help";
            // 
            // menuHelpViewHelp
            // 
            this.menuHelpViewHelp.Name = "menuHelpViewHelp";
            this.menuHelpViewHelp.Size = new System.Drawing.Size(152, 22);
            this.menuHelpViewHelp.Text = "View Help";
            // 
            // menuHelpKeyboardHelp
            // 
            this.menuHelpKeyboardHelp.Name = "menuHelpKeyboardHelp";
            this.menuHelpKeyboardHelp.Size = new System.Drawing.Size(152, 22);
            this.menuHelpKeyboardHelp.Text = "Keyboard Help";
            // 
            // menuHelpSeparator
            // 
            this.menuHelpSeparator.Name = "menuHelpSeparator";
            this.menuHelpSeparator.Size = new System.Drawing.Size(149, 6);
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Name = "menuHelpAbout";
            this.menuHelpAbout.Size = new System.Drawing.Size(152, 22);
            this.menuHelpAbout.Text = "About";
            // 
            // tbrStrip
            // 
            this.tbrStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tbrStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.tbrStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbrButtonOpen,
            this.tbrButtonSave,
            this.toolStripSeparator2,
            this.tbrButtonPause,
            this.tbrButtonMaxSpeed,
            this.tbrButtonWarmReset,
            this.tbrButtonColdReset,
            this.toolStripSeparator1,
            this.tbrButtonFullScreen,
            this.tbrButtonQuickLoad,
            this.tbrButtonSettings});
            this.tbrStrip.Location = new System.Drawing.Point(0, 24);
            this.tbrStrip.Name = "tbrStrip";
            this.tbrStrip.Padding = new System.Windows.Forms.Padding(0);
            this.tbrStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tbrStrip.Size = new System.Drawing.Size(704, 39);
            this.tbrStrip.TabIndex = 2;
            this.tbrStrip.MouseMove += new System.Windows.Forms.MouseEventHandler(this.renderVideo_MouseMove);
            // 
            // tbrButtonOpen
            // 
            this.tbrButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonOpen.Image = global::ZXMAK2.Properties.Resources.EmuFileOpen_32x32;
            this.tbrButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonOpen.Name = "tbrButtonOpen";
            this.tbrButtonOpen.Size = new System.Drawing.Size(36, 36);
            // 
            // tbrButtonSave
            // 
            this.tbrButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonSave.Image = global::ZXMAK2.Properties.Resources.EmuFileSave_32x32;
            this.tbrButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonSave.Name = "tbrButtonSave";
            this.tbrButtonSave.Size = new System.Drawing.Size(36, 36);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // tbrButtonPause
            // 
            this.tbrButtonPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonPause.Image = global::ZXMAK2.Properties.Resources.EmuResume_32x32;
            this.tbrButtonPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonPause.Name = "tbrButtonPause";
            this.tbrButtonPause.Size = new System.Drawing.Size(36, 36);
            // 
            // tbrButtonMaxSpeed
            // 
            this.tbrButtonMaxSpeed.CheckOnClick = true;
            this.tbrButtonMaxSpeed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonMaxSpeed.Image = global::ZXMAK2.Properties.Resources.EmuMaxSpeed_32x32;
            this.tbrButtonMaxSpeed.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonMaxSpeed.Name = "tbrButtonMaxSpeed";
            this.tbrButtonMaxSpeed.Size = new System.Drawing.Size(36, 36);
            // 
            // tbrButtonWarmReset
            // 
            this.tbrButtonWarmReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonWarmReset.Image = global::ZXMAK2.Properties.Resources.EmuWarmReset_32x32;
            this.tbrButtonWarmReset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonWarmReset.Name = "tbrButtonWarmReset";
            this.tbrButtonWarmReset.Size = new System.Drawing.Size(36, 36);
            // 
            // tbrButtonColdReset
            // 
            this.tbrButtonColdReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonColdReset.Image = global::ZXMAK2.Properties.Resources.EmuColdReset_32x32;
            this.tbrButtonColdReset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonColdReset.Name = "tbrButtonColdReset";
            this.tbrButtonColdReset.Size = new System.Drawing.Size(36, 36);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // tbrButtonFullScreen
            // 
            this.tbrButtonFullScreen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonFullScreen.Image = global::ZXMAK2.Properties.Resources.WindowFullScreen_32x32;
            this.tbrButtonFullScreen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonFullScreen.Name = "tbrButtonFullScreen";
            this.tbrButtonFullScreen.Size = new System.Drawing.Size(36, 36);
            // 
            // tbrButtonQuickLoad
            // 
            this.tbrButtonQuickLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonQuickLoad.Image = global::ZXMAK2.Properties.Resources.EmuQuickLoad_32x32;
            this.tbrButtonQuickLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonQuickLoad.Name = "tbrButtonQuickLoad";
            this.tbrButtonQuickLoad.Size = new System.Drawing.Size(36, 36);
            this.tbrButtonQuickLoad.Text = "Quick Boot";
            // 
            // tbrButtonSettings
            // 
            this.tbrButtonSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrButtonSettings.Image = global::ZXMAK2.Properties.Resources.EmuSettings_32x32;
            this.tbrButtonSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrButtonSettings.Name = "tbrButtonSettings";
            this.tbrButtonSettings.Size = new System.Drawing.Size(36, 36);
            // 
            // sbrStrip
            // 
            this.sbrStrip.Location = new System.Drawing.Point(0, 527);
            this.sbrStrip.Name = "sbrStrip";
            this.sbrStrip.Size = new System.Drawing.Size(704, 22);
            this.sbrStrip.TabIndex = 4;
            // 
            // renderVideo
            // 
            this.renderVideo.DebugInfo = false;
            this.renderVideo.DisplayIcon = true;
            this.renderVideo.IconDisk = false;
            this.renderVideo.Location = new System.Drawing.Point(0, 63);
            this.renderVideo.Name = "renderVideo";
            this.renderVideo.NoFlic = false;
            this.renderVideo.ScaleMode = ZXMAK2.MVP.WinForms.ScaleMode.FixedPixelSize;
            this.renderVideo.Size = new System.Drawing.Size(526, 385);
            this.renderVideo.Smoothing = false;
            this.renderVideo.TabIndex = 3;
            this.renderVideo.Text = "renderVideo";
            this.renderVideo.DeviceReset += new System.EventHandler(this.renderVideo_DeviceReset);
            this.renderVideo.DoubleClick += new System.EventHandler(this.renderVideo_DoubleClick);
            this.renderVideo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.renderVideo_MouseMove);
            this.renderVideo.Resize += new System.EventHandler(this.renderVideo_Resize);
            // 
            // MainView
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 549);
            this.Controls.Add(this.sbrStrip);
            this.Controls.Add(this.tbrStrip);
            this.Controls.Add(this.mnuStrip);
            this.Controls.Add(this.renderVideo);
            this.KeyPreview = true;
            this.MainMenuStrip = this.mnuStrip;
            this.Name = "MainView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ZXMAK2";
            this.mnuStrip.ResumeLayout(false);
            this.mnuStrip.PerformLayout();
            this.tbrStrip.ResumeLayout(false);
            this.tbrStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuView;
        private System.Windows.Forms.ToolStripMenuItem menuVm;
        private System.Windows.Forms.ToolStripMenuItem menuTools;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripButton tbrButtonOpen;
        private System.Windows.Forms.ToolStripButton tbrButtonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tbrButtonPause;
        private System.Windows.Forms.ToolStripButton tbrButtonWarmReset;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tbrButtonFullScreen;
        private System.Windows.Forms.ToolStripButton tbrButtonSettings;
        private RenderVideo renderVideo;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpen;
        private System.Windows.Forms.ToolStripMenuItem menuFileSaveAs;
        private System.Windows.Forms.ToolStripSeparator menuFileSeparator;
        private System.Windows.Forms.ToolStripMenuItem menuFileExit;
        private System.Windows.Forms.ToolStripMenuItem menuViewSize;
        private System.Windows.Forms.ToolStripMenuItem menuViewSizeX1;
        private System.Windows.Forms.ToolStripMenuItem menuViewSizeX2;
        private System.Windows.Forms.ToolStripMenuItem menuViewSizeX3;
        private System.Windows.Forms.ToolStripMenuItem menuViewSizeX4;
        private System.Windows.Forms.ToolStripMenuItem menuViewFullScreen;
        private System.Windows.Forms.ToolStripSeparator menuViewSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuViewScaleMode;
        private System.Windows.Forms.ToolStripMenuItem menuViewScaleModeStretch;
        private System.Windows.Forms.ToolStripMenuItem menuViewScaleModeKeepProportion;
        private System.Windows.Forms.ToolStripMenuItem menuViewScaleModeFixedPixelSize;
        private System.Windows.Forms.ToolStripSeparator menuViewSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuViewSmoothing;
        private System.Windows.Forms.ToolStripMenuItem menuViewNoFlic;
        private System.Windows.Forms.ToolStripMenuItem menuViewVBlankSync;
        private System.Windows.Forms.ToolStripMenuItem menuViewDisplayIcon;
        private System.Windows.Forms.ToolStripMenuItem menuViewDebugInfo;
        private System.Windows.Forms.ToolStripMenuItem menuVmPause;
        private System.Windows.Forms.ToolStripMenuItem menuVmMaximumSpeed;
        private System.Windows.Forms.ToolStripSeparator menuVmSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuVmWarmReset;
        private System.Windows.Forms.ToolStripMenuItem menuVmNmi;
        private System.Windows.Forms.ToolStripSeparator menuVmSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuVmSettings;
        private System.Windows.Forms.ToolStripMenuItem menuHelpViewHelp;
        private System.Windows.Forms.ToolStripMenuItem menuHelpKeyboardHelp;
        private System.Windows.Forms.ToolStripSeparator menuHelpSeparator;
        private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;
        private System.Windows.Forms.ToolStripMenuItem menuViewCustomize;
        private System.Windows.Forms.ToolStripSeparator MenuViewSeparator1;
        private System.Windows.Forms.ToolStripButton tbrButtonMaxSpeed;
        private System.Windows.Forms.ToolStripMenuItem menuViewCustomizeShowToolBar;
        private System.Windows.Forms.ToolStripMenuItem menuViewCustomizeShowStatusBar;
        private System.Windows.Forms.StatusStrip sbrStrip;
        private System.Windows.Forms.ToolStripButton tbrButtonColdReset;
        private System.Windows.Forms.ToolStripMenuItem menuVmColdReset;
        private Controls.MenuStripEx mnuStrip;
        private Controls.ToolStripEx tbrStrip;
        private System.Windows.Forms.ToolStripButton tbrButtonQuickLoad;
    }
}