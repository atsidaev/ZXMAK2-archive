﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.MDX;
using ZXMAK2.Entities;
using ZXMAK2.MVP.Interfaces;


namespace ZXMAK2.MVP.WinForms
{
    public partial class MainView : Form, IMainView, IHostUi
    {
        private MdxHost m_host;

        private bool m_fullScreen;
        private Point m_location;
        private Size m_size;
        private FormBorderStyle m_style;

        private bool m_firstShow = true;
        private string m_title;
        private bool m_allowSaveSize;

        
        public MainView()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            this.Icon = Utils.GetAppIcon();
            LoadClientSize();
            LoadRenderSetting();
        }


        #region Commands

        public ICommand CommandViewFullScreen { get; private set; }
        public ICommand CommandVmPause { get; private set; }
        public ICommand CommandVmWarmReset { get; private set; }
        public ICommand CommandTapePause { get; private set; }
        public ICommand CommandQuickLoad { get; private set; }
        public ICommand CommandOpenUri { get; private set; }

        #endregion Commands


        #region IMainView

        public IWin32Window Window 
        { 
            get { return this; } 
        }

        public string Title
        {
            get { return m_title; }
            set
            {
                if (m_title == value)
                {
                    return;
                }
                m_title = value;
                Text = string.IsNullOrEmpty(m_title) ?
                    "ZXMAK2" :
                    string.Format("[{0}] - ZXMAK2", m_title);
            }
        }

        public IHost Host 
        { 
            get { return m_host; } 
        }

        public Func<Size> GetVideoSize { get; set; }
        public Func<float> GetVideoRatio { get; set; }

        public event EventHandler ViewOpened;
        public event EventHandler ViewClosed;
        public event EventHandler ViewInvalidate;

        public void Run()
        {
            Application.Run(this);
        }

        public void Bind(MainPresenter presenter)
        {
            BindMenuCommand(menuFileOpen, presenter.CommandFileOpen);
            BindMenuCommand(menuFileSaveAs, presenter.CommandFileSave);
            BindMenuCommand(menuFileExit, presenter.CommandFileExit);
            BindMenuCommand(menuViewFullScreen, presenter.CommandViewFullScreen);
            BindMenuCommand(menuVmPause, presenter.CommandVmPause);
            BindMenuCommand(menuVmMaximumSpeed, presenter.CommandVmMaxSpeed);
            BindMenuCommand(menuVmWarmReset, presenter.CommandVmWarmReset);
            BindMenuCommand(menuVmColdReset, presenter.CommandVmColdReset);
            BindMenuCommand(menuVmNmi, presenter.CommandVmNmi);
            BindMenuCommand(menuVmSettings, presenter.CommandVmSettings);
            BindMenuCommand(menuHelpViewHelp, presenter.CommandHelpViewHelp);
            BindMenuCommand(menuHelpKeyboardHelp, presenter.CommandHelpKeyboardHelp);
            BindMenuCommand(menuHelpAbout, presenter.CommandHelpAbout);

            BindToolBarCommand(tbrButtonOpen, presenter.CommandFileOpen);
            BindToolBarCommand(tbrButtonSave, presenter.CommandFileSave);
            BindToolBarCommand(tbrButtonPause, presenter.CommandVmPause);
            BindToolBarCommand(tbrButtonMaxSpeed, presenter.CommandVmMaxSpeed);
            BindToolBarCommand(tbrButtonWarmReset, presenter.CommandVmWarmReset);
            BindToolBarCommand(tbrButtonColdReset, presenter.CommandVmColdReset);
            BindToolBarCommand(tbrButtonFullScreen, presenter.CommandViewFullScreen);
            BindToolBarCommand(tbrButtonSettings, presenter.CommandVmSettings);
            
            BindToolBarImageText(
                tbrButtonPause, 
                presenter.CommandVmPause, 
                "Pause",
                global::ZXMAK2.Properties.Resources.EmuPause_32x32,
                global::ZXMAK2.Properties.Resources.EmuResume_32x32);
            BindToolBarImageText(
                tbrButtonFullScreen,
                presenter.CommandViewFullScreen,
                "Windowed",
                global::ZXMAK2.Properties.Resources.WindowWindowed_32x32,
                global::ZXMAK2.Properties.Resources.WindowFullScreen_32x32);
            CommandViewFullScreen = presenter.CommandViewFullScreen;
            CommandVmPause = presenter.CommandVmPause;
            CommandVmWarmReset = presenter.CommandVmWarmReset;
            CommandTapePause = presenter.CommandTapePause;
            CommandQuickLoad = presenter.CommandQuickLoad;
            CommandOpenUri = presenter.CommandOpenUri;
        }

        public void ShowHelp()
        {
            HelpService.ShowHelp(this);
        }

        #endregion IMainView

        
        #region IHostUi

        public void ClearCommandsUi()
        {
            menuTools.DropDownItems.Clear();
        }
        
        public void AddCommandUi(ICommand command)
        {
            var subMenu = menuTools.DropDownItems.Add(command.Text) as ToolStripMenuItem;
            if (subMenu == null)
            {
                return;
            }
            BindMenuCommand(subMenu, command, this);
            SortMenuTools();
        }

        #endregion IHostUi


        #region IMainView Events

        private void OnViewOpened()
        {
            var handler = ViewOpened;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnViewClosed()
        {
            var handler = ViewClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnViewInvalidate()
        {
            var handler = ViewInvalidate;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion IMainView Events

        
        #region Form Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                renderVideo.InitWnd();
                m_host = new MdxHost(this, renderVideo);
                OnViewOpened();
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            //LogAgent.Debug("MainForm.OnShown");
            base.OnShown(e);
            try
            {
                if (m_firstShow)
                {
                    m_firstShow = false;
                    //OnViewOpened();

                    //var mult = 2;
                    //var scale = 1D;
                    //var toolHeight = mnuStrip.Height + tbrStrip.Height;
                    //Size size = GetVideoSize();
                    //size = new System.Drawing.Size(
                    //    size.Width * mult,
                    //    (int)((float)size.Height * scale) * mult + toolHeight);
                    //ClientSize = size;
                }
                m_allowSaveSize = true;
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                DialogService.ShowFatalError(ex);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            m_allowSaveSize = false;
        }
        
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            try
            {
                OnViewClosed();
                if (m_host != null)
                {
                    m_host.Dispose();
                    m_host = null;
                }
                renderVideo.FreeWnd();
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                DialogService.ShowFatalError(ex);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout(true);
            if (m_allowSaveSize &&
                WindowState == FormWindowState.Normal &&
                !m_fullScreen)
            {
                SaveClientSize();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (m_host.IsInputCaptured)
            {
                e.SuppressKeyPress = true;
            }
            if (e.Alt && e.Control)
            {
                m_host.StopInputCapture();
            }
            // FULLSCREEN
            if (e.Alt && e.KeyCode == Keys.Enter)
            {
                if (e.Alt)
                {
                    OnCommand(CommandViewFullScreen);
                }
                e.Handled = true;
                return;
            }
            //RESET
            if (e.Alt && e.Control && e.KeyCode == Keys.Insert)
            {
                OnCommand(CommandVmWarmReset, true);
                e.Handled = true;
                return;
            }
            // STOP/RUN
            if (e.KeyCode == Keys.Pause)
            {
                OnCommand(CommandVmPause);
                e.Handled = true;
                return;
            }
            if (e.Alt && e.Control && e.KeyCode == Keys.F1)
            {
                OnCommand(CommandQuickLoad);
                e.Handled = true;
                return;
            }
            if (e.Alt && e.Control && e.KeyCode == Keys.F8)
            {
                OnCommand(CommandTapePause);
                e.Handled = true;
                return;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            //RESET
            if (e.Alt && e.Control && e.KeyCode == Keys.Insert)
            {
                OnCommand(CommandVmWarmReset, false);
                e.Handled = true;
                return;
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (!Created)
            {
                return;
            }
            OnCommand(CommandVmWarmReset, false);
        }


        #endregion Form Event Handlers


        #region Drag-n-Drop

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            try
            {
                if (!CanFocus)
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }
                var ddw = new DragDataWrapper(e.Data);
                var allowOpen = false;
                if (ddw.IsFileDrop)
                {
                    var uri = new Uri(Path.GetFullPath(ddw.GetFilePath()));
                    allowOpen = CommandOpenUri.CanExecute(uri);
                }
                else if (ddw.IsLinkDrop)
                {
                    var uri = new Uri(ddw.GetLinkUri());
                    allowOpen = CommandOpenUri.CanExecute(uri);
                }
                e.Effect = allowOpen ? DragDropEffects.Link : DragDropEffects.None;
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            try
            {
                if (!CanFocus)
                {
                    return;
                }
                var ddw = new DragDataWrapper(e.Data);
                if (ddw.IsFileDrop)
                {
                    var uri = new Uri(Path.GetFullPath(ddw.GetFilePath()));
                    this.Activate();
                    this.BeginInvoke(new Action(()=>OnCommand(CommandOpenUri, uri)));

                    //string fileName = ddw.GetFilePath();
                    //if (fileName != string.Empty)
                    //{
                    //    this.Activate();
                    //    this.BeginInvoke(new OpenFileHandler(OpenFile), fileName, true);
                    //}
                }
                else if (ddw.IsLinkDrop)
                {
                    var uri = new Uri(ddw.GetLinkUri());
                    this.Activate();
                    this.BeginInvoke(new Action(() => OnCommand(CommandOpenUri, uri)));

                    //string linkUrl = ddw.GetLinkUri();
                    //if (linkUrl != string.Empty)
                    //{
                    //    Uri fileUri = new Uri(linkUrl);
                    //    this.Activate();
                    //    this.BeginInvoke(new OpenUriHandler(OpenUri), fileUri);
                    //}
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        #endregion Drag-n-Drop


        #region Layout

        private void UpdateLayout(bool isResize = false)
        {
            var pos = isResize ? new Point(0, 0) : PointToClient(Cursor.Position);
            var menuEnabled = true || m_fullScreen;
            var toolEnabled = menuViewCustomizeShowToolBar.Checked;
            var statEnabled = menuViewCustomizeShowStatusBar.Checked;
            var menuHeight = (menuEnabled ? mnuStrip.Height : 0);
            var tbarHeight = (toolEnabled ? tbrStrip.Height : 0);
            var sbarHeight = (statEnabled ? sbrStrip.Height : 0);
            var toolHeight = menuHeight + tbarHeight;
            var toolArea = pos.X >= 0 && pos.X < ClientSize.Width &&
                pos.Y >= 0 && pos.Y < toolHeight + 2;
            toolArea |= !m_fullScreen;
            sbrStrip.SizingGrip = !m_fullScreen;
            mnuStrip.Visible = toolArea && menuEnabled;
            tbrStrip.Visible = toolArea && toolEnabled;
            sbrStrip.Visible = statEnabled;

            sbarHeight = m_fullScreen ? 0 : sbarHeight;

            var shift = m_fullScreen ? 0 : toolHeight;
            renderVideo.Location = new Point(0, shift);
            shift += sbarHeight;
            renderVideo.Size = new Size(ClientSize.Width, ClientSize.Height - shift);
        }

        private void SetRenderSize(Size size)
        {
            var menuEnabled = true || m_fullScreen;
            var toolEnabled = menuViewCustomizeShowToolBar.Checked;
            var statEnabled = menuViewCustomizeShowStatusBar.Checked;
            var menuHeight = (menuEnabled ? mnuStrip.Height : 0);
            var tbarHeight = (toolEnabled ? tbrStrip.Height : 0);
            var sbarHeight = (statEnabled ? sbrStrip.Height : 0);
            var toolHeight = menuHeight + tbarHeight;
            var shift = m_fullScreen ? 0 : toolHeight + sbarHeight;
            ClientSize = new Size(size.Width, size.Height + shift);
        }

        public bool IsFullScreen
        {
            get { return m_fullScreen; }
            set
            {
                if (value == m_fullScreen)
                {
                    return;
                }
                m_fullScreen = value;
                if (m_fullScreen)
                {
                    m_style = FormBorderStyle;
                    m_location = Location;
                    m_size = ClientSize;

                    FormBorderStyle = FormBorderStyle.None;
                    Location = new Point(0, 0);
                    Size = Screen.PrimaryScreen.Bounds.Size;

                    //m_host.StartInputCapture();

                    Focus();
                }
                else
                {
                    Location = m_location;
                    FormBorderStyle = m_style;
                    ClientSize = m_size;

                    m_host.StopInputCapture();
                }
            }
        }
        
        private void renderVideo_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateLayout();
        }

        private void renderVideo_DeviceReset(object sender, EventArgs e)
        {
            OnViewInvalidate();
        }

        private void renderVideo_DoubleClick(object sender, EventArgs e)
        {
            if (!renderVideo.Focused)
            {
                return;
            }
            m_host.StartInputCapture();
        }

        private void renderVideo_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                return;
            }
            WindowState = FormWindowState.Normal;
            OnCommand(CommandViewFullScreen, true);
        }

        #endregion Layout

        
        #region Save/Load Registry Settings

        private void SaveClientSize()
        {
            try
            {
                var rkey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ZXMAK2");
                rkey.SetValue("WindowWidth", renderVideo.Size.Width, RegistryValueKind.DWord);
                rkey.SetValue("WindowHeight", renderVideo.Size.Height, RegistryValueKind.DWord);
                rkey.SetValue("ViewShowToolBar", menuViewCustomizeShowToolBar.Checked ? 1 : 0, RegistryValueKind.DWord);
                rkey.SetValue("ViewShowStatusBar", menuViewCustomizeShowStatusBar.Checked ? 1 : 0, RegistryValueKind.DWord);

            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void LoadClientSize()
        {
            try
            {
                RegistryKey rkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\ZXMAK2");
                if (rkey != null)
                {
                    object objShowToolBar = rkey.GetValue("ViewShowToolBar");
                    object objShowStatusBar = rkey.GetValue("ViewShowStatusBar");
                    object objWidth = rkey.GetValue("WindowWidth");
                    object objHeight = rkey.GetValue("WindowHeight");
                    if (objShowToolBar != null && objShowToolBar is int &&
                        objShowStatusBar != null && objShowStatusBar is int)
                    {
                        menuViewCustomizeShowToolBar.Checked = (int)objShowToolBar != 0;
                        menuViewCustomizeShowStatusBar.Checked = (int)objShowStatusBar != 0;
                    }
                    if (objWidth != null && objWidth is int &&
                        objHeight != null && objHeight is int)
                    {
                        int width = (int)objWidth;
                        int height = (int)objHeight;
                        //if(width>0 && height >0)
                        SetRenderSize(new Size(width, height));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void SaveRenderSetting()
        {
            try
            {
                var scaleMode = menuViewScaleModeStretch.Checked ? ScaleMode.Stretch :
                    menuViewScaleModeKeepProportion.Checked ? ScaleMode.KeepProportion :
                    ScaleMode.FixedPixelSize;
                var rkey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ZXMAK2");
                rkey.SetValue("RenderSmoothing", menuViewSmoothing.Checked ? 1 : 0, RegistryValueKind.DWord);
                rkey.SetValue("RenderNoFlic", menuViewNoFlic.Checked ? 1 : 0, RegistryValueKind.DWord);
                rkey.SetValue("RenderScaleMode", (int)scaleMode, RegistryValueKind.DWord);
                rkey.SetValue("RenderVBlankSync", menuViewVBlankSync.Checked ? 1 : 0, RegistryValueKind.DWord);
                rkey.SetValue("RenderDisplayIcon", menuViewDisplayIcon.Checked ? 1 : 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void LoadRenderSetting()
        {
            try
            {
                RegistryKey rkey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ZXMAK2");
                if (rkey != null)
                {
                    object objSmooth = rkey.GetValue("RenderSmoothing");
                    object objNoFlic = rkey.GetValue("RenderNoFlic");
                    object objScale = rkey.GetValue("RenderScaleMode");
                    object objSync = rkey.GetValue("RenderVBlankSync");
                    object objIcon = rkey.GetValue("RenderDisplayIcon");
                    if (objSmooth != null && objSmooth is int)
                        menuViewSmoothing.Checked = (int)objSmooth != 0;
                    if (objNoFlic != null && objNoFlic is int)
                        menuViewNoFlic.Checked = (int)objNoFlic != 0;
                    if (objScale != null && objScale is int)
                    {
                        var scaleMode = (ScaleMode)objScale;
                        menuViewScaleModeStretch.Checked = scaleMode == ScaleMode.Stretch;
                        menuViewScaleModeKeepProportion.Checked = scaleMode == ScaleMode.KeepProportion;
                        menuViewScaleModeFixedPixelSize.Checked = scaleMode == ScaleMode.FixedPixelSize;
                    }
                    else
                    {
                        menuViewScaleModeStretch.Checked = false;
                        menuViewScaleModeKeepProportion.Checked = false;
                        menuViewScaleModeFixedPixelSize.Checked = true;
                    }
                    if (objSync != null && objSync is int)
                        menuViewVBlankSync.Checked = (int)objSync != 0;
                    if (objIcon != null && objIcon is int)
                    {
                        menuViewDisplayIcon.Checked = (int)objIcon != 0;
                    }
                    else
                    {
                        menuViewDisplayIcon.Checked = true;
                    }
                    ApplyRenderSetting();
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void ApplyRenderSetting()
        {
            renderVideo.Smoothing = menuViewSmoothing.Checked;
            renderVideo.NoFlic = menuViewNoFlic.Checked;
            renderVideo.ScaleMode = GetSelectedScaleMode();
            renderVideo.VBlankSync = menuViewVBlankSync.Checked && !menuVmMaximumSpeed.Checked;
            renderVideo.DisplayIcon = menuViewDisplayIcon.Checked;
            renderVideo.DebugInfo = menuViewDebugInfo.Checked;
            renderVideo.Invalidate();
        }

        private ScaleMode GetSelectedScaleMode()
        {
            return
                menuViewScaleModeStretch.Checked ? ScaleMode.Stretch :
                menuViewScaleModeKeepProportion.Checked ? ScaleMode.KeepProportion :
                menuViewScaleModeFixedPixelSize.Checked ? ScaleMode.FixedPixelSize :
                ScaleMode.FixedPixelSize;   // default value
        }

        #endregion


        #region Menu Handlers

        private void menuView_DropDownOpening(object sender, EventArgs e)
        {
            menuViewFullScreen.Checked = m_fullScreen;

            var videoSize = GetVideoSize();
            var ratio = GetVideoRatio(); 
            videoSize = new System.Drawing.Size(videoSize.Width, (int)((float)videoSize.Height * ratio));
            menuViewSizeX1.Enabled = m_fullScreen || renderVideo.Size != videoSize;
            menuViewSizeX1.Checked = !m_fullScreen && renderVideo.Size == videoSize;
            menuViewSizeX2.Enabled = m_fullScreen || renderVideo.Size != new Size(videoSize.Width * 2, videoSize.Height * 2);
            menuViewSizeX2.Checked = !m_fullScreen && renderVideo.Size == new Size(videoSize.Width * 2, videoSize.Height * 2);
            menuViewSizeX3.Enabled = m_fullScreen || renderVideo.Size != new Size(videoSize.Width * 3, videoSize.Height * 3);
            menuViewSizeX3.Checked = !m_fullScreen && renderVideo.Size == new Size(videoSize.Width * 3, videoSize.Height * 3);
            menuViewSizeX4.Enabled = m_fullScreen || renderVideo.Size != new Size(videoSize.Width * 4, videoSize.Height * 4);
            menuViewSizeX4.Checked = !m_fullScreen && renderVideo.Size == new Size(videoSize.Width * 4, videoSize.Height * 4);
        }

        private void menuViewCustomize_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            var tool = sender == menuViewCustomizeShowToolBar ? (ToolStrip)tbrStrip : 
                sender == menuViewCustomizeShowStatusBar ? sbrStrip :
                null;
            if (tool == null)
            {
                return;
            }
            var delta = menuItem.Checked ? tool.Height : -tool.Height;
            if (m_fullScreen)
            {
                m_size = new Size(m_size.Width, m_size.Height + delta);    
            }
            else
            {
                Height += delta;
            }
            //UpdateLayout(true);
        }

        private void menuViewX_Click(object sender, EventArgs e)
        {
            OnCommand(CommandViewFullScreen, false);
            int mult = 1;
            if (sender == menuViewSizeX1)
                mult = 1;
            if (sender == menuViewSizeX2)
                mult = 2;
            if (sender == menuViewSizeX3)
                mult = 3;
            if (sender == menuViewSizeX4)
                mult = 4;
            var size = GetVideoSize();
            var ratio = GetVideoRatio();
            size = new System.Drawing.Size(
                size.Width * mult,
                (int)((float)size.Height * ratio) * mult);
            SetRenderSize(size);
        }

        private void menuViewRender_Click(object sender, EventArgs e)
        {
            var scaleMode =
                sender == menuViewScaleModeStretch ? ScaleMode.Stretch :
                sender == menuViewScaleModeKeepProportion ? ScaleMode.KeepProportion :
                sender == menuViewScaleModeFixedPixelSize ? ScaleMode.FixedPixelSize :
                GetSelectedScaleMode();
            menuViewScaleModeStretch.Checked = scaleMode == ScaleMode.Stretch;
            menuViewScaleModeKeepProportion.Checked = scaleMode == ScaleMode.KeepProportion;
            menuViewScaleModeFixedPixelSize.Checked = scaleMode == ScaleMode.FixedPixelSize;
            ApplyRenderSetting();
            SaveRenderSetting();
        }

        private void SortMenuTools()
        {
            var list = new List<ToolStripItem>();
            foreach (ToolStripItem item in menuTools.DropDownItems)
            {
                list.Add(item);
            }
            list.Sort(SortMenuToolsComparison);
            menuTools.DropDownItems.Clear();
            foreach (var item in list)
            {
                menuTools.DropDownItems.Add(item);
            }
        }

        private static int SortMenuToolsComparison(ToolStripItem x, ToolStripItem y)
        {
            if (x.Text == y.Text) return 0;
            if (string.Compare(x.Text, "Debugger", true) == 0) return -1;
            if (string.Compare(y.Text, "Debugger", true) == 0) return 1;
            return x.Text.CompareTo(y.Text);
        }

        #endregion Menu Handlers

        
        #region Bind Helpers

        private static void OnCommand(ICommand command, object arg = null)
        {
            if (command == null || !command.CanExecute(arg))
            {
                return;
            }
            command.Execute(arg);
        }

        private void BindMenuCommand(
            ToolStripMenuItem menuItem,
            ICommand command,
            object arg=null)
        {
            if (command == null)
            {
                menuItem.Visible = false;
                return;
            }
            menuItem.Tag = command;
            menuItem.Click += (s, e) => OnCommand(command, arg);
            if (!string.IsNullOrEmpty(command.Text))
            {
                menuItem.Text = command.Text;
            }
            else
            {
                command.Text = menuItem.Text;
            }
            menuItem.Checked = command.Checked;
            command.CanExecuteChanged += (s, e) =>
            {
                var canExecute = command.CanExecute(arg);
                var action = new Action(() => menuItem.Enabled = canExecute);
                if (InvokeRequired)
                {
                    BeginInvoke(action);
                }
                else
                {
                    action();
                }
            };
            BindProperty<string>(command, "Text", (v) => menuItem.Text = v);
            BindProperty<bool>(command, "Checked", (v) => menuItem.Checked = v);
        }

        private void BindToolBarCommand(
            ToolStripButton toolItem,
            ICommand command,
            object arg=null)
        {
            if (command == null)
            {
                toolItem.Visible = false;
                return;
            }
            toolItem.Tag = command;
            toolItem.Click += (s, e) => OnCommand(command, arg);
            if (!string.IsNullOrEmpty(command.Text))
            {
                toolItem.Text = command.Text;
            }
            toolItem.Checked = command.Checked;
            command.CanExecuteChanged += (s, e) =>
            {
                var canExecute = command.CanExecute(arg);
                var action = new Action(() => toolItem.Enabled = canExecute);
                if (InvokeRequired)
                {
                    BeginInvoke(action);
                }
                else
                {
                    action();
                }
            };
            BindProperty<string>(command, "Text", (v) => toolItem.Text = v);
            BindProperty<bool>(command, "Checked", (v) => toolItem.Checked = v);
        }

        private void BindToolBarImageText(
            ToolStripButton toolItem,
            ICommand command,
            string text,
            Image image,
            Image imageDefault)
        {
            BindProperty<string>(command, "Text", (value) =>
            {
                toolItem.Image = value == text ? image : imageDefault;
            });
        }

        private void BindProperty<T>(
            INotifyPropertyChanged viewModel, 
            string name, 
            Action<T> setter)
        {
            var property = viewModel
                .GetType()
                .GetProperty(name);
            setter((T)property.GetValue(viewModel, null));
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == name)
                {
                    var value = (T)property.GetValue(viewModel, null);
                    if (InvokeRequired)
                    {
                        BeginInvoke(setter, value);
                    }
                    else
                    {
                        setter(value);
                    }
                }
            };
        }

        #endregion Bind Helpers
    }
}