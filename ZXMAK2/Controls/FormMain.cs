using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Win32;

using ZXMAK2.Engine;
using ZXMAK2.Interfaces;
using ZXMAK2.Controls;
using ZXMAK2.Controls.Debugger;
using ZXMAK2.MDX;
using ZXMAK2.Entities;


namespace ZXMAK2.Controls
{
    public unsafe partial class FormMain : Form
    {
        private VirtualMachine m_vm;
        private MdxHost m_host;

        private bool m_fullscreen = false;
        private Point m_location;
        private Size m_size;
        private FormBorderStyle m_style;

        private string m_startupImage;
        private bool m_firstShow = true;

        
        public FormMain(params string[] args)
        {
            SetStyle(ControlStyles.Opaque, true);
            InitializeComponent();
            this.Icon = Utils.GetAppIcon();
            loadClientSize();
            loadRenderSetting();
            if (args.Length > 0 && File.Exists(args[0]))
            {
                m_startupImage = Path.GetFullPath(args[0]);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            //LogAgent.Debug("MainForm.OnLoad");
            base.OnLoad(e);
            try
            {
                renderVideo.InitWnd();
                m_host = new MdxHost(this, renderVideo);
                m_vm = new VirtualMachine(m_host, new GuiData(this, menuTools));
                m_vm.Init();
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            //LogAgent.Debug("MainForm.OnFormClosed");
            try
            {
                if (m_vm != null)
                {
                    m_vm.Dispose();
                    m_vm = null;
                }
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
            }
            base.OnFormClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            m_allowSaveSize = false;
        }

        protected override void OnShown(EventArgs e)
        {
            //LogAgent.Debug("MainForm.OnShown");
            base.OnShown(e);
            if (m_firstShow)
            {
                m_firstShow = false;
                try
                {
                    var fileName = Path.Combine(
                        Utils.GetAppDataFolder(),
                        "ZXMAK2.vmz");
                    if (File.Exists(fileName))
                    {
                        m_vm.OpenConfig(fileName);
                    }
                    else
                    {
                        m_vm.SaveConfigAs(fileName);
                    }
                    if (m_startupImage != null)
                    {
                        string imageName = m_vm.Spectrum.Loader.OpenFileName(m_startupImage, true);
                        if (imageName != string.Empty)
                        {
                            setCaption(imageName);
                        }
                    }
                    m_vm.DoRun();
                }
                catch (Exception ex)
                {
                    LogAgent.Error(ex);
                    DialogService.ShowFatalError(ex);
                }
            }
            m_allowSaveSize = true;
        }

        private void setCaption(string imageName)
        {
            this.Text = imageName != string.Empty ?
                string.Format("[{0}] - ZXMAK2", imageName) :
                string.Format("ZXMAK2");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (m_vm != null && m_vm.CPU.RST)
            {
                m_vm.CPU.RST = false;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            //RESET
            if (e.Alt && e.Control && e.KeyCode == Keys.Insert)
            {
                m_vm.CPU.RST = false;
                if (!m_vm.IsRunning)
                    m_vm.DoReset();
                e.Handled = true;
                return;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
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
                    Fullscreen = !Fullscreen;
                }
                e.Handled = true;
                return;
            }

            //RESET
            if (e.Alt && e.Control && e.KeyCode == Keys.Insert)
            {
                m_vm.CPU.RST = true;
                e.Handled = true;
                return;
            }

            // STOP/RUN
            if (e.KeyCode == Keys.Pause)
            {
                if (m_vm.IsRunning)
                {
                    m_vm.DoStop();
                }
                else
                {
                    m_vm.DoRun();
                }
                e.Handled = true;
                return;
            }

            if (e.Alt && e.Control && e.KeyCode == Keys.F1)
            {
                QuickBoot();
                e.Handled = true;
                return;
            }

            if (e.Alt && e.Control && e.KeyCode == Keys.F8)
            {
                var tape = m_vm.Spectrum.BusManager.FindDevice<ITapeDevice>();
                if (tape != null)
                {
                    if (tape.IsPlay)
                    {
                        tape.Stop();
                    }
                    else
                    {
                        tape.Play();
                    }
                    e.Handled = true;
                    return;
                }
            }
        }

        private void QuickBoot()
        {
            string fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            fileName = Path.Combine(fileName, "boot.zip");
            if (!File.Exists(fileName))
            {
                MessageBox.Show("Quick snapshot boot.zip is missing!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.Loader.CheckCanOpenFileName(fileName))
                {
                    m_vm.Spectrum.Loader.OpenFileName(fileName, true);
                }
                else
                {
                    DialogService.Show(
                        "Cannot open quick snapshot boot.zip!",
                        "Error",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
                }
            }
            finally
            {
                if (running)
                    m_vm.DoRun();
            }
        }

        private void renderVideo_DeviceReset(object sender, EventArgs e)
        {
            if (m_host != null)
            {
                m_host.Video.UpdateVideo(m_vm);
            }
        }

        private void renderVideo_DoubleClick(object sender, EventArgs e)
        {
            if (renderVideo.Focused)
            {
                m_host.StartInputCapture();
            }
        }

        private void renderVideo_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Fullscreen)
                return;
            if (Menu != null && e.Y > 1)
                Menu = null;
            else if (e.Y <= SystemInformation.MenuHeight)
                Menu = menuMain;
        }

        private void OpenFile(string fileName, bool readOnly)
        {
            bool running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.Loader.CheckCanOpenFileName(fileName))
                {
                    string imageName = m_vm.Spectrum.Loader.OpenFileName(fileName, readOnly);
                    if (imageName != string.Empty)
                    {
                        setCaption(imageName);
                        m_vm.SaveConfig();
                    }
                }
                else
                {
                    DialogService.Show(
                        "Unrecognized file!",
                        "Error",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                DialogService.Show(ex.Message, "ERROR", DlgButtonSet.OK, DlgIcon.Error);
            }
            finally
            {
                if (running)
                    m_vm.DoRun();
            }
        }

        private void SaveFile(string fileName)
        {
            bool running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.Loader.CheckCanSaveFileName(fileName))
                {
                    setCaption(m_vm.Spectrum.Loader.SaveFileName(fileName));
                    m_vm.SaveConfig();
                }
                else
                {
                    DialogService.Show(
                        "Unrecognized file!",
                        "Error",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                DialogService.Show(ex.Message, "ERROR", DlgButtonSet.OK, DlgIcon.Error);
            }
            finally
            {
                if (running)
                    m_vm.DoRun();
            }
        }

        #region Menu Handlers

        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            using (var loadDialog = new OpenFileDialog())
            {
                loadDialog.SupportMultiDottedExtensions = true;
                loadDialog.Title = "Open...";
                loadDialog.Filter = m_vm.Spectrum.Loader.GetOpenExtFilter();
                loadDialog.DefaultExt = "";
                loadDialog.FileName = "";
                loadDialog.ShowReadOnly = true;
                loadDialog.ReadOnlyChecked = true;
                loadDialog.CheckFileExists = true;
                loadDialog.FileOk += loadDialog_FileOk;
                if (loadDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                OpenFile(loadDialog.FileName, loadDialog.ReadOnlyChecked);
            }
        }

        private void loadDialog_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                var loadDialog = sender as OpenFileDialog;
                if (loadDialog == null) return;
                e.Cancel = !m_vm.Spectrum.Loader.CheckCanOpenFileName(loadDialog.FileName);
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                e.Cancel = true;
                DialogService.Show(ex.Message, "ERROR", DlgButtonSet.OK, DlgIcon.Error);
            }
        }

        private void menuFileSaveAs_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.SupportMultiDottedExtensions = true;
                saveDialog.Title = "Save...";
                saveDialog.Filter = m_vm.Spectrum.Loader.GetSaveExtFilter();
                saveDialog.DefaultExt = m_vm.Spectrum.Loader.GetDefaultExtension();
                saveDialog.FileName = "";
                saveDialog.OverwritePrompt = true;
                if (saveDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                SaveFile(saveDialog.FileName);
            }
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            using (FormAbout form = new FormAbout())
                form.ShowDialog();
        }

        private void menuHelpViewHelp_Click(object sender, EventArgs e)
        {
            HelpService.ShowHelp(this);
        }

        private void menuHelpKeyboard_Click(object sender, EventArgs e)
        {
            FormKeyboardHelp form = (FormKeyboardHelp)menuHelpKeyboard.Tag;
            if (form == null)
            {
                form = new FormKeyboardHelp();
                form.FormClosed += new FormClosedEventHandler(delegate(object s1, FormClosedEventArgs e1)
                {
                    menuHelpKeyboard.Tag = null;
                });
                menuHelpKeyboard.Tag = form;
                form.Show(this);
            }
            else
            {
                form.Activate();
            }
        }

        private void menuVm_Popup(object sender, EventArgs e)
        {
            menuVmPause.Text = m_vm.IsRunning ? "Pause" : "Resume";
        }

        private void menuVmPause_Click(object sender, EventArgs e)
        {
            if (m_vm.IsRunning)
                m_vm.DoStop();
            else
                m_vm.DoRun();
        }

        private void menuVmReset_Click(object sender, EventArgs e)
        {
            m_vm.DoReset();
        }

        private void menuVmNmi_Click(object sender, EventArgs e)
        {
            m_vm.DoNmi();
        }

        private void menuVmOptions_Click(object sender, EventArgs e)
        {
            try
            {
                using (FormMachineSettings form = new FormMachineSettings())
                {
                    form.Init(m_vm, renderVideo);
                    form.ShowDialog(this);
                    m_host.Video.UpdateVideo(m_vm);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                DialogService.Show(
                    ex.Message,
                    "ERROR",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
            }
        }

        private void menuViewX_Click(object sender, EventArgs e)
        {
            Fullscreen = false;
            int mult = 1;
            if (sender == menuViewSizeX1)
                mult = 1;
            if (sender == menuViewSizeX2)
                mult = 2;
            if (sender == menuViewSizeX3)
                mult = 3;
            if (sender == menuViewSizeX4)
                mult = 4;
            Size size = m_vm.ScreenSize;
            size = new System.Drawing.Size(
                size.Width * mult,
                (int)((float)size.Height * m_vm.ScreenHeightScale) * mult);
            ClientSize = size;
        }

        private void menuView_Popup(object sender, EventArgs e)
        {
            menuViewFullscreen.Enabled = !Fullscreen;
            menuViewFullscreen.Checked = Fullscreen;
            menuViewWindowed.Enabled = Fullscreen;
            menuViewWindowed.Checked = !Fullscreen;

            //menuViewSize.Enabled = !Fullscreen;

            Size videoSize = m_vm.ScreenSize;
            videoSize = new System.Drawing.Size(videoSize.Width, (int)((float)videoSize.Height * m_vm.ScreenHeightScale));
            menuViewSizeX1.Enabled = ClientSize != videoSize;
            menuViewSizeX1.Checked = ClientSize == videoSize;
            menuViewSizeX2.Enabled = ClientSize != new Size(videoSize.Width * 2, videoSize.Height * 2);
            menuViewSizeX2.Checked = ClientSize == new Size(videoSize.Width * 2, videoSize.Height * 2);
            menuViewSizeX3.Enabled = ClientSize != new Size(videoSize.Width * 3, videoSize.Height * 3);
            menuViewSizeX3.Checked = ClientSize == new Size(videoSize.Width * 3, videoSize.Height * 3);
            menuViewSizeX4.Enabled = ClientSize != new Size(videoSize.Width * 4, videoSize.Height * 4);
            menuViewSizeX4.Checked = ClientSize == new Size(videoSize.Width * 4, videoSize.Height * 4);
        }

        private void menuViewWindowed_Click(object sender, EventArgs e)
        {
            Fullscreen = false;
        }

        private void menuViewFullscreen_Click(object sender, EventArgs e)
        {
            Fullscreen = true;
        }

        private void menuViewRender_Click(object sender, EventArgs e)
        {
            menuViewSmoothing.Checked = sender == menuViewSmoothing ? !menuViewSmoothing.Checked : menuViewSmoothing.Checked;
            menuViewNoFlic.Checked = sender == menuViewNoFlic ? !menuViewNoFlic.Checked : menuViewNoFlic.Checked;
            menuViewVBlankSync.Checked = sender == menuViewVBlankSync ? !menuViewVBlankSync.Checked : menuViewVBlankSync.Checked;
            menuViewDisplayIcon.Checked = sender == menuViewDisplayIcon ? !menuViewDisplayIcon.Checked : menuViewDisplayIcon.Checked;
            menuViewDebugInfo.Checked = sender == menuViewDebugInfo ? !menuViewDebugInfo.Checked : menuViewDebugInfo.Checked;

            var scaleMode =
                sender == menuViewScaleModeStretch ? ScaleMode.Stretch :
                sender == menuViewScaleModeKeepProportion ? ScaleMode.KeepProportion :
                sender == menuViewScaleModeFixedPixelSize ? ScaleMode.FixedPixelSize :
                GetSelectedScaleMode();
            menuViewScaleModeStretch.Checked = scaleMode == ScaleMode.Stretch;
            menuViewScaleModeKeepProportion.Checked = scaleMode == ScaleMode.KeepProportion;
            menuViewScaleModeFixedPixelSize.Checked = scaleMode == ScaleMode.FixedPixelSize;

            applyRenderSetting();
            saveRenderSetting();
        }

        private void menuVmMaximumSpeed_Click(object sender, EventArgs e)
        {
            menuVmMaximumSpeed.Checked = !menuVmMaximumSpeed.Checked;
            m_vm.MaxSpeed = menuVmMaximumSpeed.Checked;
            applyRenderSetting();
        }

        private ScaleMode GetSelectedScaleMode()
        {
            return
                menuViewScaleModeStretch.Checked ? ScaleMode.Stretch :
                menuViewScaleModeKeepProportion.Checked ? ScaleMode.KeepProportion :
                menuViewScaleModeFixedPixelSize.Checked ? ScaleMode.FixedPixelSize :
                ScaleMode.FixedPixelSize;   // default value
        }

        private void applyRenderSetting()
        {
            renderVideo.Smoothing = menuViewSmoothing.Checked;
            renderVideo.NoFlic = menuViewNoFlic.Checked;
            renderVideo.ScaleMode = GetSelectedScaleMode();
            renderVideo.VBlankSync = menuViewVBlankSync.Checked && !menuVmMaximumSpeed.Checked;
            renderVideo.DisplayIcon = menuViewDisplayIcon.Checked;
            renderVideo.DebugInfo = menuViewDebugInfo.Checked;
            renderVideo.Invalidate();
        }

        #endregion

        #region Fullscreen

        public bool Fullscreen
        {
            get { return m_fullscreen; }
            set
            {
                if (value != m_fullscreen)
                {
                    m_fullscreen = value;
                    if (value)
                    {
                        m_style = FormBorderStyle;
                        m_location = Location;
                        m_size = ClientSize;

                        FormBorderStyle = FormBorderStyle.None;
                        Location = new Point(0, 0);

                        //m_host.StartInputCapture();
                        Menu = null;
                        Size = Screen.PrimaryScreen.Bounds.Size;
                        Focus();
                    }
                    else
                    {
                        Location = m_location;
                        FormBorderStyle = m_style;

                        m_host.StopInputCapture();
                        Menu = menuMain;
                        ClientSize = m_size;
                    }
                }
            }
        }

        #endregion

        private void renderVideo_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                Fullscreen = true;
                renderVideo.Location = new Point(0, 0);
                renderVideo.Size = Screen.PrimaryScreen.Bounds.Size;
            }
        }

        private bool m_allowSaveSize = false;
        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (m_allowSaveSize && WindowState == FormWindowState.Normal && !Fullscreen)
                saveClientSize();
        }

        #region Save/Load Registry Settings

        private void saveClientSize()
        {
            try
            {
                RegistryKey rkey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ZXMAK2");
                rkey.SetValue("WindowWidth", ClientSize.Width, RegistryValueKind.DWord);
                rkey.SetValue("WindowHeight", ClientSize.Height, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void loadClientSize()
        {
            try
            {
                RegistryKey rkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\ZXMAK2");
                if (rkey != null)
                {
                    object objWidth = rkey.GetValue("WindowWidth");
                    object objHeight = rkey.GetValue("WindowHeight");
                    if (objWidth != null && objWidth is int &&
                        objHeight != null && objHeight is int)
                    {
                        int width = (int)objWidth;
                        int height = (int)objHeight;
                        //if(width>0 && height >0)
                        ClientSize = new Size(width, height);

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void saveRenderSetting()
        {
            try
            {
                var scaleMode = menuViewScaleModeStretch.Checked ? ScaleMode.Stretch :
                    menuViewScaleModeKeepProportion.Checked ? ScaleMode.KeepProportion :
                    ScaleMode.FixedPixelSize;
                RegistryKey rkey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ZXMAK2");
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

        private void loadRenderSetting()
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
                    applyRenderSetting();
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        #endregion

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (!CanFocus)
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }
                DragDataWrapper ddw = new DragDataWrapper(e.Data);
                bool allowOpen = false;
                if (ddw.IsFileDrop)
                {
                    string fileName = ddw.GetFilePath();
                    if (fileName != string.Empty &&
                        m_vm.Spectrum.Loader.CheckCanOpenFileName(fileName))
                    {
                        allowOpen = true;
                    }
                }
                else if (ddw.IsLinkDrop)
                {
                    allowOpen = true;
                }
                e.Effect = allowOpen ? DragDropEffects.Link : DragDropEffects.None;
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (!CanFocus)
                    return;
                DragDataWrapper ddw = new DragDataWrapper(e.Data);
                if (ddw.IsFileDrop)
                {
                    string fileName = ddw.GetFilePath();
                    if (fileName != string.Empty)
                    {
                        this.Activate();
                        this.BeginInvoke(new OpenFileHandler(OpenFile), fileName, true);
                    }
                }
                else if (ddw.IsLinkDrop)
                {
                    string linkUrl = ddw.GetLinkUri();
                    if (linkUrl != string.Empty)
                    {
                        Uri fileUri = new Uri(linkUrl);
                        this.Activate();
                        this.BeginInvoke(new OpenUriHandler(OpenUri), fileUri);
                    }
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void OpenUri(Uri uri)
        {
            try
            {
                var downloader = new WebDownloader();
                var webFile = downloader.Download(uri);
                using (MemoryStream ms = new MemoryStream(webFile.Content))
                {
                    OpenStream(webFile.FileName, ms);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                DialogService.Show(ex.Message, "ERROR", DlgButtonSet.OK, DlgIcon.Error);
            }
        }

        private void OpenStream(string fileName, Stream fileStream)
        {
            bool running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.Loader.CheckCanOpenFileStream(fileName, fileStream))
                {
                    string imageName = m_vm.Spectrum.Loader.OpenFileStream(fileName, fileStream);
                    if (imageName != string.Empty)
                    {
                        setCaption(imageName);
                        m_vm.SaveConfig();
                    }
                }
                else
                {
                    DialogService.Show(
                        string.Format("Unrecognized file!\n\n{0}", fileName),
                        "Error",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
                }
            }
            finally
            {
                if (running)
                    m_vm.DoRun();
            }
        }

        private delegate void OpenFileHandler(string fileName, bool readOnly);
        private delegate void OpenUriHandler(Uri fileUri);
    }
}
