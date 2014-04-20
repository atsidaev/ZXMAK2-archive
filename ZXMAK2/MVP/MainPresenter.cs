using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Controls;
using ZXMAK2.Entities;
using ZXMAK2.MVP.Interfaces;
using ZXMAK2.MVP.WinForms;


namespace ZXMAK2.MVP
{
    public class MainPresenter : IMainPresenter
    {
        private readonly IViewResolver m_viewResolver;
        private readonly IMainView m_view;
        private readonly string m_startupImage;
        private VirtualMachine m_vm;
        
        
        public MainPresenter(
            IViewResolver viewResolver, 
            IMainView view, 
            params string[] args)
        {
            m_viewResolver = viewResolver;
            m_view = view;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                m_startupImage = Path.GetFullPath(args[0]);
            }
            m_view.GetVideoData = () => m_vm.VideoData;
            m_view.ViewOpened += MainView_OnViewOpened;
            m_view.ViewClosed += MainView_OnViewClosed;
            m_view.ViewInvalidate += MainView_OnViewInvalidate;
            CreateCommands();
        }

        public void Dispose()
        {
            if (m_vm != null)
            {
                m_vm.Dispose();
                m_vm = null;
            }
        }

        public void Run()
        {
            m_view.Run();
        }

        
        #region Commands

        public ICommand CommandFileOpen { get; private set; }
        public ICommand CommandFileSave { get; private set; }
        public ICommand CommandFileExit { get; private set; }
        public ICommand CommandViewFullScreen { get; private set; }
        public ICommand CommandViewSyncVBlank { get; private set; }
        public ICommand CommandVmPause { get; private set; }
        public ICommand CommandVmMaxSpeed { get; private set; }
        public ICommand CommandVmWarmReset { get; private set; }
        public ICommand CommandVmColdReset { get; private set; }
        public ICommand CommandVmNmi { get; private set; }
        public ICommand CommandVmSettings { get; private set; }
        public ICommand CommandHelpViewHelp { get; private set; }
        public ICommand CommandHelpKeyboardHelp { get; private set; }
        public ICommand CommandHelpAbout { get; private set; }
        public ICommand CommandTapePause { get; private set; }
        public ICommand CommandQuickLoad { get; private set; }
        public ICommand CommandOpenUri { get; private set; }

        #endregion Commands


        #region MainView Event Handlers

        private void MainView_OnViewOpened(object sender, EventArgs e)
        {
            if (m_vm != null)
            {
                LogAgent.Warn("IMainView.ViewOpened event raised twice!");
                return;
            }
            m_vm = new VirtualMachine(m_view.Host);
            m_vm.Init();
            m_vm.UpdateState += VirtualMachine_OnUpdateState;

            m_view.Title = string.Empty;
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
                m_view.Title = m_vm.Spectrum.Loader.OpenFileName(m_startupImage, true);
            }
            m_view.Bind(this);
            m_vm.DoRun();
        }

        private void MainView_OnViewClosed(object sender, EventArgs e)
        {
            if (m_vm == null)
            {
                LogAgent.Warn("IMainView.ViewClosed: object is not initialized!");
            }
            Dispose();
        }

        private void MainView_OnViewInvalidate(object sender, EventArgs e)
        {
            if (m_view == null || m_view.Host == null)
            {
                return;
            }
            m_view.Host.Video.PushFrame(m_vm);
        }

        #endregion MainView Event Handlers


        #region Command Implementation

        private void CreateCommands()
        {
            CommandFileOpen = new CommandDelegate(CommandFileOpen_OnExecute);
            CommandFileSave = new CommandDelegate(CommandFileSave_OnExecute);
            CommandFileExit = new CommandDelegate(CommandFileExit_OnExecute);
            CommandViewFullScreen = new CommandDelegate(CommandViewFullScreen_OnExecute);
            CommandViewSyncVBlank = new CommandDelegate(CommandViewSyncVBlank_OnExecute);
            CommandVmPause = new CommandDelegate(CommandVmPause_OnExecute);
            CommandVmMaxSpeed = new CommandDelegate(CommandVmMaxSpeed_OnExecute);
            CommandVmWarmReset = new CommandDelegate(CommandVmWarmReset_OnExecute);
            CommandVmNmi = new CommandDelegate(CommandVmNmi_OnExecute);
            CommandVmSettings = new CommandDelegate(CommandVmSettings_OnExecute);
            CommandHelpViewHelp = new CommandDelegate((obj)=>m_view.ShowHelp(obj));
            CommandHelpKeyboardHelp = CreateViewHolderCommand<IKeyboardView>();
            CommandHelpAbout = CreateViewHolderCommand<IAboutView>();
            CommandTapePause = new CommandDelegate(CommandTapePause_OnExecute, CommandTapePause_CanExecute);
            CommandQuickLoad = new CommandDelegate(CommandQuickLoad_OnExecute);
            CommandOpenUri = new CommandDelegate(CommandOpenUri_OnExecute, CommandOpenUri_OnCanExecute);
        }

        private ICommand CreateViewHolderCommand<T>()
            where T : IView
        {
            var viewHolder = new ViewHolder<T>(m_viewResolver, null);
            return viewHolder.CommandOpen;
        }

        private void CommandFileOpen_OnExecute(Object objArg)
        {
            if (m_vm == null)
            {
                return;
            }
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
                loadDialog.FileOk += LoadDialog_FileOk;
                if (loadDialog.ShowDialog(objArg as IWin32Window) != DialogResult.OK)
                {
                    return;
                }
                OpenFile(loadDialog.FileName, loadDialog.ReadOnlyChecked);
            }
        }

        private void CommandFileSave_OnExecute(Object objArg)
        {
            if (m_vm == null)
            {
                return;
            }
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.SupportMultiDottedExtensions = true;
                saveDialog.Title = "Save...";
                saveDialog.Filter = m_vm.Spectrum.Loader.GetSaveExtFilter();
                saveDialog.DefaultExt = m_vm.Spectrum.Loader.GetDefaultExtension();
                saveDialog.FileName = "";
                saveDialog.OverwritePrompt = true;
                if (saveDialog.ShowDialog(objArg as IWin32Window) != DialogResult.OK)
                {
                    return;
                }
                SaveFile(saveDialog.FileName);
            }
        }

        private void CommandFileExit_OnExecute()
        {
            m_view.Close();
        }

        private void CommandViewFullScreen_OnExecute(object objState)
        {
            var state = objState as bool?;
            var value = m_view.IsFullScreen;
            value = state.HasValue ? state.Value : !value;
            m_view.IsFullScreen = value;
            CommandViewFullScreen.Text = value ? "Windowed" : "Full Screen";
        }

        private void CommandViewSyncVBlank_OnExecute(object objState)
        {
            var state = objState as bool?;
            var value = CommandViewSyncVBlank.Checked;
            value = state.HasValue ? state.Value : !value;
            CommandViewSyncVBlank.Checked = value;
            m_vm.SyncSource = CommandVmMaxSpeed.Checked ? SyncSource.None :
                CommandViewSyncVBlank.Checked ? SyncSource.Video :
                SyncSource.Sound;
        }

        private void CommandVmPause_OnExecute()
        {
            if (m_vm == null)
            {
                return;
            }
            if (m_vm.IsRunning)
            {
                m_vm.DoStop();
            }
            else
            {
                m_vm.DoRun();
            }
        }

        private void CommandVmMaxSpeed_OnExecute(Object objState)
        {
            var state = objState as bool?;
            var value = CommandVmMaxSpeed.Checked;
            value = state.HasValue ? state.Value : !value;
            CommandVmMaxSpeed.Checked = value;
            if (m_vm == null)
            {
                return;
            }
            m_vm.SyncSource = CommandVmMaxSpeed.Checked ? SyncSource.None :
                CommandViewSyncVBlank.Checked ? SyncSource.Video :
                SyncSource.Sound;
            //applyRenderSetting();  ???
        }

        private void CommandVmWarmReset_OnExecute(object objState)
        {
            if (m_vm == null)
            {
                return;
            }
            var state = objState as bool?;
            if (state.HasValue)
            {
                if (m_vm.IsRunning)
                {
                    // state-change command
                    if (state.Value != m_vm.CPU.RST)
                    {
                        m_vm.CPU.RST = (bool)objState;
                    }
                    CommandVmWarmReset.Checked = state.Value;
                }
                else if (!state.Value && state.Value != m_vm.CPU.RST)
                {
                    // if stopped then trigger reset on back front only once
                    // because false may be set on wnd.deactivate for breakpoint
                    m_vm.DoReset();
                    CommandVmWarmReset.Checked = false;
                }
            }
            else
            {
                // event command
                m_vm.DoReset();
                CommandVmWarmReset.Checked = false;
            }
        }

        private void CommandVmNmi_OnExecute()
        {
            if (m_vm == null)
            {
                return;
            }
            m_vm.DoNmi();
        }

        private void CommandVmSettings_OnExecute(Object objArg)
        {
            try
            {
                if (m_vm == null)
                {
                    return;
                }
                using (var form = new FormMachineSettings(m_view.Host))
                {
                    form.Init(m_vm);
                    form.ShowDialog(objArg as IWin32Window);
                    m_view.Host.Video.PushFrame(m_vm);
                    
                    ((CommandDelegate)CommandTapePause).RaiseCanExecuteChanged();
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                Locator.Resolve<IUserMessage>().Error(ex);
            }
        }

        private bool CommandTapePause_CanExecute()
        {
            if (m_vm == null)
            {
                return false;
            }
            var tape = m_vm.Spectrum.BusManager.FindDevice<ITapeDevice>();
            return tape != null;
        }

        private void CommandTapePause_OnExecute()
        {
            if (m_vm == null)
            {
                return;
            }
            var tape = m_vm.Spectrum.BusManager.FindDevice<ITapeDevice>();
            if (tape == null)
            {
                return;
            }
            if (tape.IsPlay)
            {
                tape.Stop();
            }
            else
            {
                tape.Play();
            }
        }

        private void CommandQuickLoad_OnExecute()
        {
            if (m_vm == null)
            {
                return;
            }
            var fileName = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            fileName = Path.Combine(fileName, "boot.zip");
            if (!File.Exists(fileName))
            {
                Locator.Resolve<IUserMessage>()
                    .Error("Quick snapshot boot.zip is missing!");
                return;
            }
            var running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.Loader.CheckCanOpenFileName(fileName))
                {
                    m_vm.Spectrum.Loader.OpenFileName(fileName, true);
                }
                else
                {
                    Locator.Resolve<IUserMessage>()
                        .Error("Cannot open quick snapshot boot.zip!");
                }
            }
            finally
            {
                if (running)
                    m_vm.DoRun();
            }
        }

        private bool CommandOpenUri_OnCanExecute(object objUri)
        {
            try
            {
                if (m_vm == null)
                {
                    return false;
                }
                var uri = (Uri)objUri;
                return uri != null && 
                    (!uri.IsLoopback ||
                    m_vm.Spectrum.Loader.CheckCanOpenFileName(uri.LocalPath));
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                return false;
            }
        }

        private void CommandOpenUri_OnExecute(object objUri)
        {
            try
            {
                if (m_vm == null)
                {
                    return;
                }
                var uri = (Uri)objUri;
                if (uri.IsLoopback)
                {
                    OpenFile(uri.LocalPath, true);
                }
                else
                {
                    var downloader = new WebDownloader();
                    var webFile = downloader.Download(uri);
                    using (var ms = new MemoryStream(webFile.Content))
                    {
                        OpenStream(webFile.FileName, ms);
                    }
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                Locator.Resolve<IUserMessage>().Error(ex);
            }
        }

        #endregion Command Implementation


        #region Private

        private void VirtualMachine_OnUpdateState(object sender, EventArgs e)
        {
            var text = m_vm.IsRunning ? "Pause" : "Resume";
            CommandVmPause.Text = text;
            RaiseCommandCanExecuteChanged(CommandTapePause);
        }

        private void LoadDialog_FileOk(object sender, CancelEventArgs e)
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
                Locator.Resolve<IUserMessage>().Error(ex);
            }
        }

        private void OpenFile(string fileName, bool readOnly)
        {
            var running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.Loader.CheckCanOpenFileName(fileName))
                {
                    string imageName = m_vm.Spectrum.Loader.OpenFileName(fileName, readOnly);
                    if (imageName != string.Empty)
                    {
                        m_view.Title = imageName;
                        m_vm.SaveConfig();
                    }
                }
                else
                {
                    Locator.Resolve<IUserMessage>()
                        .Error("Unrecognized file!");
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                Locator.Resolve<IUserMessage>().Error(ex);
            }
            finally
            {
                if (running)
                    m_vm.DoRun();
            }
        }

        private void SaveFile(string fileName)
        {
            var running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.Loader.CheckCanSaveFileName(fileName))
                {
                    m_view.Title = m_vm.Spectrum.Loader.SaveFileName(fileName);
                    m_vm.SaveConfig();
                }
                else
                {
                    Locator.Resolve<IUserMessage>()
                        .Error("Unrecognized file!");
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                Locator.Resolve<IUserMessage>().Error(ex);
            }
            finally
            {
                if (running)
                    m_vm.DoRun();
            }
        }

        private void OpenStream(string fileName, Stream fileStream)
        {
            var running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.Loader.CheckCanOpenFileStream(fileName, fileStream))
                {
                    string imageName = m_vm.Spectrum.Loader.OpenFileStream(fileName, fileStream);
                    if (imageName != string.Empty)
                    {
                        m_view.Title = imageName;
                        m_vm.SaveConfig();
                    }
                }
                else
                {
                    Locator.Resolve<IUserMessage>()
                        .Error("Unrecognized file!\n\n{0}", fileName);
                }
            }
            finally
            {
                if (running)
                    m_vm.DoRun();
            }
        }

        private void RaiseCommandCanExecuteChanged(ICommand command)
        {
            var commandDelegate = command as CommandDelegate;
            if (commandDelegate != null)
            {
                commandDelegate.RaiseCanExecuteChanged();
            }
        }

        #endregion Private
    }
}
