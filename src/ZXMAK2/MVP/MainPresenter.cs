﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Controls;
using ZXMAK2.Presentation.Entities;
using ZXMAK2.Host.Presentation;
using ZXMAK2.Host.Presentation.Interfaces;
using ZXMAK2.Host.Presentation.Tools;
using ZXMAK2.Engine.Interfaces;



namespace ZXMAK2.MVP
{
    public class MainPresenter : IMainPresenter
    {
        private readonly IResolver m_resolver;
        private readonly IUserMessage m_userMessage;
        private readonly IMainView m_view;
        private readonly string m_startupImage;
        private VirtualMachine m_vm;
        
        
        public MainPresenter(
            IResolver resolver,
            IUserMessage userMessage,
            IMainView view, 
            params string[] args)
        {
            m_resolver = resolver;
            m_userMessage = userMessage;
            m_view = view;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                m_startupImage = Path.GetFullPath(args[0]);
            }
            m_view.ViewOpened += MainView_OnViewOpened;
            m_view.ViewClosed += MainView_OnViewClosed;
            m_view.RequestFrame += MainView_OnRequestFrame;
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
                Logger.Warn("IMainView.ViewOpened event raised twice!");
                return;
            }
            m_vm = new VirtualMachine(m_view.Host, m_view.CommandManager);
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
                m_view.Title = m_vm.Spectrum.BusManager.LoadManager.OpenFileName(m_startupImage, true);
            }
            m_view.Bind(this);
            m_vm.DoRun();
        }

        private void MainView_OnViewClosed(object sender, EventArgs e)
        {
            if (m_vm == null)
            {
                Logger.Warn("IMainView.ViewClosed: object is not initialized!");
            }
            Dispose();
        }

        private void MainView_OnRequestFrame(object sender, EventArgs e)
        {
            if (m_view == null || m_view.Host == null)
            {
                return;
            }
            m_vm.RequestFrame();
        }

        #endregion MainView Event Handlers


        #region Command Implementation

        private void CreateCommands()
        {
            CommandFileOpen = new CommandDelegate(CommandFileOpen_OnExecute, CommandFileOpen_OnCanExecute);
            CommandFileSave = new CommandDelegate(CommandFileSave_OnExecute, CommandFileSave_OnCanExecute);
            CommandFileExit = new CommandDelegate(CommandFileExit_OnExecute);
            CommandViewFullScreen = new CommandDelegate(CommandViewFullScreen_OnExecute);
            CommandViewSyncVBlank = new CommandDelegate(CommandViewSyncVBlank_OnExecute);
            CommandVmPause = new CommandDelegate(CommandVmPause_OnExecute);
            CommandVmMaxSpeed = new CommandDelegate(CommandVmMaxSpeed_OnExecute);
            CommandVmWarmReset = new CommandDelegate(CommandVmWarmReset_OnExecute);
            CommandVmNmi = new CommandDelegate(CommandVmNmi_OnExecute);
            CommandVmSettings = new CommandDelegate(CommandVmSettings_OnExecute);
            CommandHelpViewHelp = new CommandDelegate(CommandHelpViewHelp_OnExecute, CommandHelpViewHelp_OnCanExecute);
            CommandHelpKeyboardHelp = CreateViewHolderCommand<IKeyboardView>();
            CommandHelpAbout = CreateViewHolderCommand<IAboutView>();
            CommandTapePause = new CommandDelegate(CommandTapePause_OnExecute, CommandTapePause_CanExecute);
            CommandQuickLoad = new CommandDelegate(CommandQuickLoad_OnExecute);
            CommandOpenUri = new CommandDelegate(CommandOpenUri_OnExecute, CommandOpenUri_OnCanExecute);
        }

        private ICommand CreateViewHolderCommand<T>()
            where T : IView
        {
            var viewHolder = new ViewHolder<T>(null);
            return viewHolder.CommandOpen;
        }

        private bool CommandFileOpen_OnCanExecute()
        {
            if (m_vm == null)
            {
                return false;
            }
            var dialog = GetViewService<IOpenFileDialog>();
            if (dialog != null)
            {
                dialog.Dispose();
            }
            return dialog != null;
        }

        private void CommandFileOpen_OnExecute()
        {
            if (!CommandFileOpen_OnCanExecute())
            {
                return;
            }
            using (var loadDialog = GetViewService<IOpenFileDialog>())
            {
                loadDialog.Title = "Open...";
                loadDialog.Filter = m_vm.Spectrum.BusManager.LoadManager.GetOpenExtFilter();
                loadDialog.FileName = "";
                loadDialog.ShowReadOnly = true;
                loadDialog.ReadOnlyChecked = true;
                loadDialog.CheckFileExists = true;
                loadDialog.FileOk += LoadDialog_FileOk;
                if (loadDialog.ShowDialog(m_view) != DlgResult.OK)
                {
                    return;
                }
                OpenFile(loadDialog.FileName, loadDialog.ReadOnlyChecked);
            }
        }

        private bool CommandFileSave_OnCanExecute()
        {
            if (m_vm == null)
            {
                return false;
            }
            var dialog = GetViewService<ISaveFileDialog>();
            if (dialog != null)
            {
                dialog.Dispose();
            }
            return dialog != null;
        }

        private void CommandFileSave_OnExecute()
        {
            if (!CommandFileSave_OnCanExecute())
            {
                return;
            }
            using (var saveDialog = GetViewService<ISaveFileDialog>())
            {
                saveDialog.Title = "Save...";
                saveDialog.Filter = m_vm.Spectrum.BusManager.LoadManager.GetSaveExtFilter();
                saveDialog.DefaultExt = m_vm.Spectrum.BusManager.LoadManager.GetDefaultExtension();
                saveDialog.FileName = string.Empty;
                saveDialog.OverwritePrompt = true;
                if (saveDialog.ShowDialog(m_view) != DlgResult.OK)
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

        private T GetViewService<T>()
        {
            var viewResolver = m_resolver.Resolve<IResolver>("View");
            return viewResolver.TryResolve<T>();
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
                    m_vm.RequestFrame();
                    
                    ((CommandDelegate)CommandTapePause).RaiseCanExecuteChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                m_userMessage.Error(ex);
            }
        }

        private bool CommandHelpViewHelp_OnCanExecute(object arg)
        {
            var service = GetViewService<IUserHelp>();
            return service != null && service.CanShow(arg);
        }

        private void CommandHelpViewHelp_OnExecute(object arg)
        {
            if (!CommandHelpViewHelp_OnCanExecute(arg))
            {
                return;
            }
            var service = GetViewService<IUserHelp>();
            service.ShowHelp(arg);
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
                m_userMessage.Error("Quick snapshot boot.zip is missing!");
                return;
            }
            var running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.BusManager.LoadManager.CheckCanOpenFileName(fileName))
                {
                    m_vm.Spectrum.BusManager.LoadManager.OpenFileName(fileName, true);
                }
                else
                {
                    m_userMessage.Error("Cannot open quick snapshot boot.zip!");
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
                    m_vm.Spectrum.BusManager.LoadManager.CheckCanOpenFileName(uri.LocalPath));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                Logger.Error(ex);
                m_userMessage.Error(ex);
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
                var loadDialog = sender as IOpenFileDialog;
                if (loadDialog == null) return;
                e.Cancel = !m_vm.Spectrum.BusManager.LoadManager.CheckCanOpenFileName(loadDialog.FileName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                e.Cancel = true;
                m_userMessage.Error(ex);
            }
        }

        private void OpenFile(string fileName, bool readOnly)
        {
            var running = m_vm.IsRunning;
            m_vm.DoStop();
            try
            {
                if (m_vm.Spectrum.BusManager.LoadManager.CheckCanOpenFileName(fileName))
                {
                    string imageName = m_vm.Spectrum.BusManager.LoadManager.OpenFileName(fileName, readOnly);
                    if (imageName != string.Empty)
                    {
                        m_view.Title = imageName;
                        m_vm.SaveConfig();
                    }
                }
                else
                {
                    m_userMessage.Error("Unrecognized file!");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                m_userMessage.Error(ex);
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
                if (m_vm.Spectrum.BusManager.LoadManager.CheckCanSaveFileName(fileName))
                {
                    m_view.Title = m_vm.Spectrum.BusManager.LoadManager.SaveFileName(fileName);
                    m_vm.SaveConfig();
                }
                else
                {
                    m_userMessage.Error("Unrecognized file!");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                m_userMessage.Error(ex);
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
                if (m_vm.Spectrum.BusManager.LoadManager.CheckCanOpenFileStream(fileName, fileStream))
                {
                    string imageName = m_vm.Spectrum.BusManager.LoadManager.OpenFileStream(fileName, fileStream);
                    if (imageName != string.Empty)
                    {
                        m_view.Title = imageName;
                        m_vm.SaveConfig();
                    }
                }
                else
                {
                    m_userMessage.Error(
                        "Unrecognized file!\n\n{0}", 
                        fileName);
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
