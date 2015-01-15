using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.WinForms.Mdx;
using ZXMAK2.Host.WinForms.Controls;
using ZXMAK2.Host.WinForms.Tools;
using ZXMAK2.Resources;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.Host.Presentation.Interfaces;
using ZXMAK2.Host.WinForms.Services;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Host.WinForms.Views
{
    public partial class MainView : Form, IMainView, ICommandManager
    {
        private readonly IResolver m_resolver;
        private readonly SettingService m_setting;

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

        public MainView(IResolver resolver)
        {
            m_resolver = resolver;
            m_setting = new SettingService();
            SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            Icon = ImageResources.ZXMAK2;

            LoadClientSize();
            LoadRenderSetting();
        }


        public string KeyboardMapFile { get; set; }


        #region Commands

        private ICommand CommandViewFullScreen { get; set; }
        private ICommand CommandViewSyncSource { get; set; }
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

        public ICommandManager CommandManager
        {
            get { return this; }
        }

        public event EventHandler ViewOpened;
        public event EventHandler ViewClosed;
        public event EventHandler RequestFrame;

        public void Run()
        {
            Application.Run(this);
        }

        public void Bind(IMainPresenter presenter)
        {
            if (presenter.CommandViewSyncSource != null)
            {
                // set back to apply registry setting
                OnCommand(presenter.CommandViewSyncSource, SelectedSyncSource);
            }

            BindMenuCommand(menuFileOpen, presenter.CommandFileOpen, this);
            BindMenuCommand(menuFileSaveAs, presenter.CommandFileSave, this);
            BindMenuCommand(menuFileExit, presenter.CommandFileExit);
            BindMenuCommand(menuViewFullScreen, presenter.CommandViewFullScreen);
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
                        tbrButtonPause.Image = global::ZXMAK2.Host.WinForms.Properties.Resources.EmuPause_32x32;
                        renderVideo.IsRunning = true;
                    }
                    else
                    {
                        tbrButtonPause.Image = global::ZXMAK2.Host.WinForms.Properties.Resources.EmuResume_32x32;
                        renderVideo.IsRunning = false;
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
                        tbrButtonFullScreen.Image = global::ZXMAK2.Host.WinForms.Properties.Resources.EmuWindowed_32x32;
                    }
                    else
                    {
                        tbrButtonFullScreen.Image = global::ZXMAK2.Host.WinForms.Properties.Resources.EmuFullScreen_32x32;
                    }
                }));

            CommandViewFullScreen = presenter.CommandViewFullScreen;
            CommandViewSyncSource = presenter.CommandViewSyncSource;
            CommandVmPause = presenter.CommandVmPause;
            CommandVmMaxSpeed = presenter.CommandVmMaxSpeed;
            CommandVmWarmReset = presenter.CommandVmWarmReset;
            CommandTapePause = presenter.CommandTapePause;
            CommandQuickLoad = presenter.CommandQuickLoad;
            CommandOpenUri = presenter.CommandOpenUri;
        }

        #endregion IMainView


        #region IHostUi

        public void Clear()
        {
            menuTools.DropDownItems.Clear();
        }

        public void Add(ICommand command)
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

        private void OnRequestFrame()
        {
            var handler = RequestFrame;
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
                LoadConfig();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void LoadConfig()
        {
            if (string.IsNullOrEmpty(KeyboardMapFile))
            {
                return;
            }
            try
            {
                m_host.Keyboard.LoadConfiguration(KeyboardMapFile);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            m_allowSaveSize = true;

            if (renderVideo.IsReadScanlineSupported)
            {
                menuViewFrameSyncVideo.ToolTipText = null;
                menuViewFrameSyncVideo.Enabled = true;
            }
            else
            {
                menuViewFrameSyncVideo.ToolTipText = "ReadScanLine capability is not supported by your videocard!";
                menuViewFrameSyncVideo.Enabled = false;
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
                Logger.Error(ex);
                m_resolver.Resolve<IUserMessage>().ErrorDetails(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
            var toolEnabled = m_setting.IsToolBarVisible;
            var statEnabled = m_setting.IsStatusBarVisible;
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
                    var screen = Screen.FromControl(this);
                    Location = screen.Bounds.Location;
                    Size = screen.Bounds.Size;

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
            OnRequestFrame();
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
            m_setting.WindowWidth = renderVideo.Size.Width;
            m_setting.WindowHeight = renderVideo.Size.Height;
            m_setting.IsToolBarVisible = menuViewCustomizeShowToolBar.Checked;
            m_setting.IsStatusBarVisible = menuViewCustomizeShowStatusBar.Checked;
        }

        private void LoadClientSize()
        {
            try
            {
                menuViewCustomizeShowToolBar.Checked = m_setting.IsToolBarVisible;
                menuViewCustomizeShowStatusBar.Checked = m_setting.IsStatusBarVisible;
                SetRenderSize(new Size(m_setting.WindowWidth, m_setting.WindowHeight));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SaveRenderSetting()
        {
            try
            {
                m_setting.SyncSource = SelectedSyncSource;
                m_setting.RenderScaleMode = SelectedScaleMode;
                m_setting.RenderVideoFilter = SelectedVideoFilter;
                m_setting.RenderSmooth = menuViewSmoothing.Checked;
                m_setting.RenderMimicTv = menuViewMimicTv.Checked;
                m_setting.RenderDisplayIcon = menuViewDisplayIcon.Checked;
                m_setting.RenderDebugInfo = menuViewDebugInfo.Checked;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void LoadRenderSetting()
        {
            try
            {
                SelectedSyncSource = m_setting.SyncSource;
                SelectedScaleMode = m_setting.RenderScaleMode;
                SelectedVideoFilter = m_setting.RenderVideoFilter;
                menuViewSmoothing.Checked = m_setting.RenderSmooth;
                menuViewMimicTv.Checked = m_setting.RenderMimicTv;
                menuViewDisplayIcon.Checked = m_setting.RenderDisplayIcon;
                menuViewDebugInfo.Checked = m_setting.RenderDebugInfo;
                
                ApplyRenderSetting();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void ApplyRenderSetting()
        {
            renderVideo.Smoothing = m_setting.RenderSmooth;
            renderVideo.MimicTv = m_setting.RenderMimicTv;
            renderVideo.NoFlic = m_setting.RenderVideoFilter == VideoFilter.NoFlick;
            renderVideo.ScaleMode = m_setting.RenderScaleMode;
            renderVideo.DisplayIcon = m_setting.RenderDisplayIcon;
            renderVideo.DebugInfo = m_setting.RenderDebugInfo;
            if (CommandViewSyncSource != null)
            {
                // set back
                OnCommand(CommandViewSyncSource, m_setting.SyncSource);
            }
            renderVideo.Invalidate();
        }

        private SyncSource SelectedSyncSource
        {
            get
            {
                //return menuViewVBlankSync.Checked ? SyncSource.Video : SyncSource.Sound;
                return menuViewFrameSyncTime.Checked ? SyncSource.Time :
                    menuViewFrameSyncSound.Checked ? SyncSource.Sound :
                    menuViewFrameSyncVideo.Checked ? SyncSource.Video :
                    default(SyncSource);
            }
            set
            {
                //menuViewVBlankSync.Checked = value == SyncSource.Video;
                menuViewFrameSyncTime.Checked = value == SyncSource.Time;
                menuViewFrameSyncSound.Checked = value == SyncSource.Sound;
                menuViewFrameSyncVideo.Checked = value == SyncSource.Video;
            }
        }

        private ScaleMode SelectedScaleMode
        {
            get
            {
                //return GetSelectedScaleMode();
                return menuViewScaleModeStretch.Checked ? ScaleMode.Stretch :
                    menuViewScaleModeKeepProportion.Checked ? ScaleMode.KeepProportion :
                    menuViewScaleModeFixedPixelSize.Checked ? ScaleMode.FixedPixelSize :
                    menuViewScaleModeSquarePixelSize.Checked ? ScaleMode.SquarePixelSize :
                    default(ScaleMode);
            }
            set
            {
                menuViewScaleModeStretch.Checked = value == ScaleMode.Stretch;
                menuViewScaleModeKeepProportion.Checked = value == ScaleMode.KeepProportion;
                menuViewScaleModeFixedPixelSize.Checked = value == ScaleMode.FixedPixelSize;
                menuViewScaleModeSquarePixelSize.Checked = value == ScaleMode.SquarePixelSize;
            }
        }

        private VideoFilter SelectedVideoFilter
        {
            get
            {
                return menuViewVideoFilterNoFlick.Checked ? VideoFilter.NoFlick : 
                    menuViewVideoFilterNone.Checked ? VideoFilter.None :
                    default(VideoFilter);
            }
            set
            {
                menuViewVideoFilterNoFlick.Checked = value == VideoFilter.NoFlick;
                menuViewVideoFilterNone.Checked = value == VideoFilter.None;
            }
        }

        #endregion


        #region Menu Handlers

        private void menuView_DropDownOpening(object sender, EventArgs e)
        {
            menuViewFullScreen.Checked = m_fullScreen;

            OnRequestFrame();
            var videoSize = renderVideo.FrameSize;
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
            OnRequestFrame();
            var videoSize = renderVideo.FrameSize;
            var size = new Size(
                videoSize.Width * mult,
                videoSize.Height * mult);
            SetRenderSize(size);
        }

        private void menuViewRender_Click(object sender, EventArgs e)
        {
            SelectedScaleMode =
                sender == menuViewScaleModeStretch ? ScaleMode.Stretch :
                sender == menuViewScaleModeKeepProportion ? ScaleMode.KeepProportion :
                sender == menuViewScaleModeFixedPixelSize ? ScaleMode.FixedPixelSize :
                sender == menuViewScaleModeSquarePixelSize ? ScaleMode.SquarePixelSize :
                SelectedScaleMode;
            SelectedSyncSource =
                sender == menuViewFrameSyncTime ? SyncSource.Time :
                sender == menuViewFrameSyncSound ? SyncSource.Sound :
                sender == menuViewFrameSyncVideo ? SyncSource.Video :
                SelectedSyncSource;
            SelectedVideoFilter =
                sender == menuViewVideoFilterNoFlick ? VideoFilter.NoFlick :
                sender == menuViewVideoFilterNone ? VideoFilter.None :
                SelectedVideoFilter;
            SaveRenderSetting();
            ApplyRenderSetting();
        }

        private void menuViewRender_CheckStateChanged(object sender, EventArgs e)
        {
            SaveRenderSetting();
            ApplyRenderSetting();
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
            toolItem.Enabled = command.CanExecute(arg);
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
