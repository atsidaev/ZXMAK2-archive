using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.ComponentModel;
using Microsoft.Win32;

using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Devices.Memory;
using ZXMAK2.Controls;
using ZXMAK2.Controls.Debugger;
using ZXMAK2.MDX;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;


namespace ZXMAK2.Controls
{
    public partial class FormMain : Form
    {
        private VirtualMachine m_vm;
        private DirectKeyboard m_keyboard;
        private DirectMouse m_mouse;
        private DirectSound m_sound;

        private bool m_fullscreen = false;
        private Point m_location;
        private Size m_size;
        private FormBorderStyle m_style;
        private bool m_topMost;

        private string m_startupImage = string.Empty;


        public FormMain()
        {
            InitializeComponent();
            this.Icon = Utils.GetAppIcon(); 
            //ClientSize = new Size(640, 480);
            loadClientSize();
            loadRenderSetting();
        }

        public string StartupImage
        {
            get { return m_startupImage; }
            set { m_startupImage = value; }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                m_mouse = new DirectMouse(this);
                m_keyboard = new DirectKeyboard(this);
                m_sound = new DirectSound(this, -1, 44100, 16, 2, 882 * 2 * 2, 4);
                m_vm = new VirtualMachine(m_keyboard, m_mouse, m_sound);
                m_vm.UpdateVideo += vm_UpdateVideo;
                //m_vm.Breakpoint += vm_Breakpoint;
                //ClientSize = new Size(m_vm.Spectrum.Ula.VideoSize.Width * 2, m_vm.Spectrum.Ula.VideoSize.Height * 2);
                //m_vm.DoRun();
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                if (m_keyboard != null)
                    m_keyboard.Dispose();
                m_keyboard = null;
                if (m_mouse != null)
                    m_mouse.Dispose();
                m_mouse = null;
                if (m_sound != null)
                    m_sound.Dispose();
                m_sound = null;
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
            base.OnHandleDestroyed(e);
        }

        private bool m_firstShow = true;

        protected override void OnShown(EventArgs e)
        {
            //LogAgent.Debug("MainForm.OnShown");
            base.OnShown(e);
            if (m_firstShow)
            {
                m_firstShow = false;
                try
                {
                    //ClientSize = new Size(m_vm.Spectrum.Ula.VideoSize.Width * 2, m_vm.Spectrum.Ula.VideoSize.Height * 2);
                    string appName = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
                    string fileName = Path.ChangeExtension(appName, ".vmz");
                    if (File.Exists(fileName))
                        m_vm.OpenConfig(fileName);
                    else
                        m_vm.SaveConfigAs(fileName);
                    if (StartupImage != string.Empty)
                    {
                        string imageName = m_vm.Spectrum.Loader.OpenFileName(StartupImage, true);
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
                    DialogProvider.ShowFatalError(ex);
                }
            }
            m_allowSaveSize = true;
        }

        private void setCaption(string imageName)
        {
            this.Text = imageName!=string.Empty ? 
                string.Format("[{0}] - ZXMAK2", imageName) :
                string.Format("ZXMAK2");
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_allowSaveSize = false;
            //LogAgent.Debug("FormMain.FormMain_FormClosing {0}", e.CloseReason);
            m_vm.UpdateVideo -= new EventHandler(vm_UpdateVideo);
            m_vm.DoStop();
            m_vm.Spectrum.BusManager.Disconnect();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
		}

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Alt && e.Control && m_mouse != null)
                m_mouse.StopCapture();
            
            // FULLSCREEN
            if (e.Alt && e.KeyCode == Keys.Enter)
            {
                if (e.Alt)
                    Fullscreen = !Fullscreen;
                e.Handled = true;
                return;
            }

            //RESET
            if (e.Alt && e.Control && e.KeyCode == Keys.Insert)
            {
                m_vm.DoReset();
                e.Handled = true;
                return;
            }
            
            // STOP/RUN
            if (e.KeyCode == Keys.Pause)
            {
                if (m_vm.IsRunning)
                    m_vm.DoStop();
                else
                    m_vm.DoRun();
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
                ITapeDevice tape = m_vm.Spectrum.BusManager.FindDevice(typeof(ITapeDevice)) as ITapeDevice;
                if (tape != null)
                {
                    if (tape.IsPlay)
                        tape.Stop();
                    else
                        tape.Play();
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
                    DialogProvider.Show(
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


        private void vm_UpdateVideo(object sender, EventArgs e)
        {
            if (m_vm != null)
            {
                IBetaDiskDevice betaDisk = m_vm.Spectrum.BusManager.FindDevice(typeof(IBetaDiskDevice)) as IBetaDiskDevice;
                renderVideo.IconDisk = betaDisk != null ? betaDisk.LedDiskIO : false;
                if (betaDisk != null)
                    betaDisk.LedDiskIO = false;
                renderVideo.DebugStartTact = m_vm.DebugFrameStartTact;
                renderVideo.UpdateSurface(m_vm.Screen, m_vm.ScreenSize, m_vm.ScreenHeightScale);
            }
        }

		private void renderVideo_DeviceReset(object sender, EventArgs e)
		{
            if (m_vm != null)
            {
                IBetaDiskDevice betaDisk = m_vm.Spectrum.BusManager.FindDevice(typeof(IBetaDiskDevice)) as IBetaDiskDevice;
                renderVideo.IconDisk = betaDisk != null ? betaDisk.LedDiskIO : false;
                if (betaDisk != null)
                    betaDisk.LedDiskIO = false;
                renderVideo.DebugStartTact = m_vm.DebugFrameStartTact;
                renderVideo.UpdateSurface(m_vm.Screen, m_vm.ScreenSize, m_vm.ScreenHeightScale);
            }
		}

        private void renderVideo_Click(object sender, EventArgs e)
        {
            if (renderVideo.Focused && m_mouse != null)
                m_mouse.StartCapture();
        }

        private void renderVideo_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Fullscreen)
                return;
            if (Menu != null && e.Y > 1)
                Menu = null;
            else if (e.Y <= SystemInformation.MenuHeight)
                Menu = mainMenu;
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
                    DialogProvider.Show(
                        "Unrecognized file!",
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
                    DialogProvider.Show(
                        "Unrecognized file!",
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

        #region Menu Handlers

        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog loadDialog = new OpenFileDialog())
            {
                loadDialog.InitialDirectory = ".";
                loadDialog.SupportMultiDottedExtensions = true;
                loadDialog.Title = "Open...";
                loadDialog.Filter = m_vm.Spectrum.Loader.GetOpenExtFilter();
                loadDialog.DefaultExt = "";
                loadDialog.FileName = "";
                loadDialog.ShowReadOnly = true;
                loadDialog.ReadOnlyChecked = true;
                loadDialog.CheckFileExists = true;
                loadDialog.FileOk += new CancelEventHandler(loadDialog_FileOk);
                if (loadDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) 
                    return;
                OpenFile(loadDialog.FileName, loadDialog.ReadOnlyChecked);
            }
        }

        private void loadDialog_FileOk(object sender, CancelEventArgs e)
        {
            OpenFileDialog loadDialog = sender as OpenFileDialog;
            if (loadDialog == null) return;
            e.Cancel = !m_vm.Spectrum.Loader.CheckCanOpenFileName(loadDialog.FileName);
        }

        private void menuFileSaveAs_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.InitialDirectory = ".";
                saveDialog.SupportMultiDottedExtensions = true;
                saveDialog.Title = "Save...";
                saveDialog.Filter = m_vm.Spectrum.Loader.GetSaveExtFilter();
                saveDialog.DefaultExt = m_vm.Spectrum.Loader.GetDefaultExtension();
                saveDialog.FileName = "";
                saveDialog.OverwritePrompt = true;
                if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                SaveFile(saveDialog.FileName);
            }
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuTools_Popup(object sender, EventArgs e)
        {
            menuToolsDebugger.Visible = m_vm.Spectrum.BusManager.FindDevice(typeof(IJtagDevice)) == null;
            menuToolsTape.Visible = m_vm.Spectrum.BusManager.FindDevice(typeof(ITapeDevice)) != null;
        }

        private void menuToolsDebugger_Click(object sender, EventArgs e)
        {
            if (m_vm.Spectrum.BusManager.FindDevice(typeof(IJtagDevice)) != null)
                return;
            FormCpu.Show(this, m_vm);
        }

        private void menuToolsTape_Click(object sender, EventArgs e)
        {
            ITapeDevice tape = m_vm.Spectrum.BusManager.FindDevice(typeof(ITapeDevice)) as ITapeDevice;
            if (tape == null)
                return;
            new TapeForm(tape).Show(this);
        }

        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            using (FormAbout form = new FormAbout())
                form.ShowDialog();
        }

        private void menuToolsMemory_Click(object sender, EventArgs e)
        {
            FormMemoryMap form = new FormMemoryMap();
            form.Memory = m_vm.Spectrum.BusManager.FindDevice(typeof(IMemoryDevice)) as MemoryBase;
            form.Show(this);
        }
        
        private void menuToolsVG93_Click(object sender, EventArgs e)
        {
            IBetaDiskDevice betaDisk = m_vm.Spectrum.BusManager.FindDevice(typeof(IBetaDiskDevice)) as IBetaDiskDevice;
            new dbgWD1793(betaDisk).Show(this);
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
                    vm_UpdateVideo(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                DialogProvider.Show(
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
            menuViewSmoothing.Checked = renderVideo.Smoothing;
            menuViewKeepProportion.Checked = renderVideo.KeepProportion;
            menuViewVBlankSync.Checked = renderVideo.VBlankSync;
            menuViewDisplayIcon.Checked = renderVideo.DisplayIcon;
            menuViewDebugInfo.Checked = renderVideo.DebugInfo;

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
            menuViewKeepProportion.Checked = sender == menuViewKeepProportion ? !menuViewKeepProportion.Checked : menuViewKeepProportion.Checked;
            menuViewVBlankSync.Checked = sender == menuViewVBlankSync ? !menuViewVBlankSync.Checked : menuViewVBlankSync.Checked;
            menuViewDisplayIcon.Checked = sender == menuViewDisplayIcon ? !menuViewDisplayIcon.Checked : menuViewDisplayIcon.Checked;
            menuViewDebugInfo.Checked = sender == menuViewDebugInfo ? !menuViewDebugInfo.Checked : menuViewDebugInfo.Checked;
            
            renderVideo.Smoothing = menuViewSmoothing.Checked;
            renderVideo.KeepProportion = menuViewKeepProportion.Checked;
            renderVideo.VBlankSync = menuViewVBlankSync.Checked;
            renderVideo.DisplayIcon = menuViewDisplayIcon.Checked;
            renderVideo.DebugInfo = menuViewDebugInfo.Checked;
            try
            {
                RegistryKey rkey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ZXMAK2");
                rkey.SetValue("RenderSmoothing", renderVideo.Smoothing? 1:0, RegistryValueKind.DWord);
                rkey.SetValue("RenderKeepProportion", renderVideo.KeepProportion? 1:0, RegistryValueKind.DWord);
                rkey.SetValue("RenderVBlankSync", renderVideo.VBlankSync ? 1 : 0, RegistryValueKind.DWord);
                rkey.SetValue("RenderDisplayIcon", renderVideo.DisplayIcon ? 1 : 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
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
                        m_topMost = TopMost;
                        m_location = Location;
                        m_size = ClientSize;

                        FormBorderStyle = FormBorderStyle.None;
                        Location = new Point(0, 0);

                        //m_mouse.StartCapture();
                        Menu = null;
                        Size = Screen.PrimaryScreen.Bounds.Size;
                        Focus();
                    }
                    else
                    {
                        Location = m_location;
                        FormBorderStyle = m_style;

                        m_mouse.StopCapture();
                        Menu = mainMenu;
                        ClientSize = m_size;
                    }

                    //vctl.RenderScene();
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
                renderVideo.Location = new Point(0,0);
                renderVideo.Size = Screen.PrimaryScreen.Bounds.Size;
            }
        }

        private bool m_allowSaveSize = false;
        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (m_allowSaveSize && WindowState == FormWindowState.Normal && !Fullscreen)
                saveClientSize();
        }

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
            ClientSize = new Size(640, 480);
        }

        private void loadRenderSetting()
        {
            try
            {
                RegistryKey rkey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ZXMAK2");
                if (rkey != null)
                {
                    object objSmooth = rkey.GetValue("RenderSmoothing");
                    object objKeep = rkey.GetValue("RenderKeepProportion");
                    object objSync = rkey.GetValue("RenderVBlankSync");
                    object objIcon = rkey.GetValue("RenderDisplayIcon");
                    if (objSmooth != null && objSmooth is int)
                        renderVideo.Smoothing = (int)objSmooth != 0;
                    if (objKeep != null && objKeep is int)
                        renderVideo.KeepProportion = (int)objKeep != 0;
                    if (objSync != null && objSync is int)
                        renderVideo.VBlankSync = (int)objSync != 0;
                    if (objIcon != null && objIcon is int)
                        renderVideo.DisplayIcon = (int)objIcon != 0;
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
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
                DragDataWrapper ddw = new DragDataWrapper(e.Data);
                if (ddw.IsFileDrop)
                {
                    string fileName = ddw.GetFilePath();
                    if (fileName != string.Empty)
                    {
                        this.BeginInvoke(new OpenFileHandler(OpenFile), fileName, true);
                        this.Activate();
                    }
                }
                else if (ddw.IsLinkDrop)
                {
                    string linkUrl = ddw.GetLinkUri();
                    if (linkUrl != string.Empty)
                    {
                        Uri fileUri = new Uri(linkUrl);
                        this.BeginInvoke(new OpenUriHandler(OpenUri), fileUri);
                        this.Activate();
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
                string fileName = string.Empty;
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] data = downloadUri(uri, out fileName);
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    OpenStream(fileName, ms);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                DialogProvider.Show(ex.Message, "ERROR", DlgButtonSet.OK, DlgIcon.Error);
            }
        }

        private byte[] downloadUri(Uri uri, out string fileName)
        {
            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Timeout = 15000;
            //webRequest.Credentials = new NetworkCredential("anonymous", "User@");
            WebResponse webResponse = webRequest.GetResponse();
            try
            {
                fileName = Path.GetFileName(webResponse.ResponseUri.LocalPath);
                if (webResponse.Headers["Content-Disposition"] != null)
                {
                    ContentDisposition contDisp = new ContentDisposition(
                        webResponse.Headers["Content-Disposition"]);
                    if (!string.IsNullOrEmpty(contDisp.FileName))
                        fileName = contDisp.FileName;
                }
                using (Stream stream = webResponse.GetResponseStream())
                {
                    byte[] data = webResponse.ContentLength >=0 ?
                        downloadStream(stream, webResponse.ContentLength, webRequest.Timeout) :
                        downloadStreamNoLength(stream, webRequest.Timeout);
                    return data;
                }
            }
            finally
            {
                webResponse.Close();
            }
        }

        private byte[] downloadStream(Stream stream, long length, int timeOut)
        {
            byte[] data = new byte[length];
            long read = 0;
            int tickCount = Environment.TickCount;
            while (read < length)
            {
                read += stream.Read(data, (int)read, (int)(length - read));
                if ((Environment.TickCount - tickCount) > timeOut)
                    throw new TimeoutException("Download timeout error!");
            }
            return data;
        }

        private byte[] downloadStreamNoLength(Stream stream, int timeOut)
        {
            List<byte> list = new List<byte>();
            byte[] readBuffer = new byte[0x10000];
            int tickCount = Environment.TickCount;
            while (true)
            {
                if ((Environment.TickCount - tickCount) > timeOut)
                    throw new TimeoutException("Download timeout error!");
                int len = stream.Read(readBuffer, 0, readBuffer.Length);
                if (len==0)
                {
                    break;
                }
                for(int i=0; i < len; i++)
                    list.Add(readBuffer[i]);
            }
            return list.ToArray();
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
                    DialogProvider.Show(
                        "Unrecognized file!",
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

    internal class DragDataWrapper
    {
        private IDataObject m_dataObject;
        public DragDataWrapper(IDataObject dataObject)
        {
            m_dataObject = dataObject;
        }

        public bool IsFileDrop { get { return m_dataObject.GetDataPresent(DataFormatEx.FileDrop); } }
        public bool IsLinkDrop { get { return m_dataObject.GetDataPresent(DataFormatEx.Uri); } }

        public string GetFilePath()
        {
            object objData = m_dataObject.GetData(DataFormatEx.FileDrop);
            string[] fileArray = getStringArray(objData as Array);
            return fileArray.Length == 1 ? fileArray[0] : string.Empty;
        }

        public string GetLinkUri()
        {
            object objData = m_dataObject.GetData(DataFormatEx.Uri);
            string fileUri = string.Empty;
            using (MemoryStream ms = objData as MemoryStream)
            {
                byte[] data = new byte[ms.Length];
                ms.Read(data, 0, data.Length);
                int length;
                for (length = 0; length < data.Length; length++)
                    if (data[length] == 0)
                        break;
                fileUri = Encoding.ASCII.GetString(data, 0, length);
            }
            return fileUri.Trim();
        }

        private static string[] getStringArray(Array dataArray)
        {
            List<String> list = new List<string>();
            if (dataArray != null)
            {
                foreach (string value in dataArray)
                    list.Add(value);
            }
            return list.ToArray();
        }

        private static class DataFormatEx
        {
            public static string FileDrop = DataFormats.FileDrop;
            public static string Uri = "UniformResourceLocator";
        }
    }
}
