using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
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

        private string m_title;
        private bool m_allowSaveSize;

        static MainView()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        public MainView()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            this.Icon = Utils.GetAppIcon();

            menuViewCustomizeShowToolBar.Checked = true;
            SetRenderSize(new Size(640, 512));

            LoadClientSize();
            LoadRenderSetting();
        }


        #region Commands

        private ICommand CommandViewFullScreen { get; set; }
        private ICommand CommandVmPause { get; set; }
        private ICommand CommandVmMaxSpeed { get; set; }
        private ICommand CommandVmWarmReset { get; set; }
        private ICommand CommandTapePause { get; set; }
        private ICommand CommandQuickLoad { get; set; }
        private ICommand CommandOpenUri { get; set; }

        #endregion Commands


        #region IMainView

        public string Title
        {
            get { return m_title; }
            set
            {
                m_title = value;
                var tail = "ZXMAK2";
                if (CommandVmPause != null && CommandVmPause.Text != "Pause")
                {
                    tail += " (Paused)";
                }
                Text = string.IsNullOrEmpty(m_title) ?
                    tail :
                    string.Format("[{0}] - {1}", m_title, tail);
            }
        }

        public IHost Host
        {
            get { return m_host; }
        }

        public Func<IVideoData> GetVideoData { get; set; }

        public event EventHandler ViewOpened;
        public event EventHandler ViewClosed;
        public event EventHandler ViewInvalidate;

        public void Run()
        {
            Application.Run(this);
        }

        public void Bind(IMainPresenter presenter)
        {
            if (presenter.CommandViewSyncVBlank != null)
            {
                // set back to apply registry setting
                OnCommand(presenter.CommandViewSyncVBlank, menuViewVBlankSync.Checked);
            }

            BindMenuCommand(menuFileOpen, presenter.CommandFileOpen, this);
            BindMenuCommand(menuFileSaveAs, presenter.CommandFileSave, this);
            BindMenuCommand(menuFileExit, presenter.CommandFileExit);
            BindMenuCommand(menuViewFullScreen, presenter.CommandViewFullScreen);
            BindMenuCommand(menuViewVBlankSync, presenter.CommandViewSyncVBlank);
            BindMenuCommand(menuVmPause, presenter.CommandVmPause);
            BindMenuCommand(menuVmMaximumSpeed, presenter.CommandVmMaxSpeed);
            BindMenuCommand(menuVmWarmReset, presenter.CommandVmWarmReset);
            BindMenuCommand(menuVmColdReset, presenter.CommandVmColdReset);
            BindMenuCommand(menuVmNmi, presenter.CommandVmNmi);
            BindMenuCommand(menuVmSettings, presenter.CommandVmSettings, this);
            BindMenuCommand(menuHelpViewHelp, presenter.CommandHelpViewHelp, this);
            BindMenuCommand(menuHelpKeyboardHelp, presenter.CommandHelpKeyboardHelp, this);
            BindMenuCommand(menuHelpAbout, presenter.CommandHelpAbout, this);

            BindToolBarCommand(tbrButtonOpen, presenter.CommandFileOpen, this);
            BindToolBarCommand(tbrButtonSave, presenter.CommandFileSave, this);
            BindToolBarCommand(tbrButtonPause, presenter.CommandVmPause);
            BindToolBarCommand(tbrButtonMaxSpeed, presenter.CommandVmMaxSpeed);
            BindToolBarCommand(tbrButtonWarmReset, presenter.CommandVmWarmReset);
            BindToolBarCommand(tbrButtonColdReset, presenter.CommandVmColdReset);
            BindToolBarCommand(tbrButtonFullScreen, presenter.CommandViewFullScreen);
            BindToolBarCommand(tbrButtonQuickLoad, presenter.CommandQuickLoad);
            BindToolBarCommand(tbrButtonSettings, presenter.CommandVmSettings, this);

            BindProperty<string>(
                presenter.CommandVmPause,
                "Text",
                new Action<string>((value) =>
                {
                    if (value == "Pause")
                    {
                        tbrButtonPause.Image = global::ZXMAK2.WinForms.Properties.Resources.EmuPause_32x32;
                    }
                    else
                    {
                        tbrButtonPause.Image = global::ZXMAK2.WinForms.Properties.Resources.EmuResume_32x32;
                    }
                    Title = Title;
                }));
            BindProperty<string>(
                presenter.CommandViewFullScreen,
                "Text",
                new Action<string>((value) =>
                {
                    if (value == "Windowed")
                    {
                        tbrButtonFullScreen.Image = global::ZXMAK2.WinForms.Properties.Resources.EmuWindowed_32x32;
                    }
                    else
                    {
                        tbrButtonFullScreen.Image = global::ZXMAK2.WinForms.Properties.Resources.EmuFullScreen_32x32;
                    }
                }));

            CommandViewFullScreen = presenter.CommandViewFullScreen;
            CommandVmPause = presenter.CommandVmPause;
            CommandVmMaxSpeed = presenter.CommandVmMaxSpeed;
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
            base.OnShown(e);
            m_allowSaveSize = true;
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
                Locator.Resolve<IUserMessage>()
                    .ErrorDetails(ex);
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
            // Max Speed
            if (e.Alt && e.KeyCode == Keys.Scroll)
            {
                OnCommand(CommandVmMaxSpeed);
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
                    this.BeginInvoke(new Action(() => OnCommand(CommandOpenUri, uri)));

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
            //renderVideo.VBlankSync = menuViewVBlankSync.Checked && !menuVmMaximumSpeed.Checked;
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

            var videoData = GetVideoData();
            var videoSize = new Size(
                videoData.Size.Width, 
                (int)((float)videoData.Size.Height * videoData.Ratio));
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
            var videoData = GetVideoData();
            var size = new Size(
                videoData.Size.Width * mult,
                (int)((float)videoData.Size.Height * videoData.Ratio) * mult);
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
            ToolStripMenuItem toolItem,
            ICommand command,
            object arg = null)
        {
            if (command == null)
            {
                toolItem.Visible = false;
                return;
            }
            BindToolStripItemCommand(toolItem, command, arg);
            toolItem.Checked = command.Checked;
            BindProperty<bool>(command, "Checked", (v) => toolItem.Checked = v);
        }

        private void BindToolBarCommand(
            ToolStripButton toolItem,
            ICommand command,
            object arg = null)
        {
            if (command == null)
            {
                toolItem.Visible = false;
                return;
            }
            BindToolStripItemCommand(toolItem, command, arg);
            toolItem.Checked = command.Checked;
            BindProperty<bool>(command, "Checked", (v) => toolItem.Checked = v);
        }

        private void BindToolStripItemCommand(
            ToolStripItem toolItem,
            ICommand command,
            object arg = null)
        {
            if (command == null)
            {
                toolItem.Visible = false;
                return;
            }
            toolItem.Tag = command;
            toolItem.Click += (s, e) => OnCommand(command, arg);
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
            if (!string.IsNullOrEmpty(command.Text))
            {
                toolItem.Text = command.Text;
            }
            else
            {
                command.Text = toolItem.Text;
            }
            BindProperty<string>(command, "Text", (v) => toolItem.Text = v);
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
