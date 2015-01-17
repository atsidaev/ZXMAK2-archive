using System;
using System.Linq;
using ZXMAK2.Presentation;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.Presentation.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        private IHost _host;
        
        public IHost Host
        {
            get { return _host; }
            set { PropertyChangeRef("Host", ref _host, value); }
        }

        private string _title;
        
        public string Title
        {
            get { return _title; }
            set { PropertyChangeRef("Title", ref _title, value); }
        }

        private bool _isFullScreen;

        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set { PropertyChangeVal("IsFullScreen", ref _isFullScreen, value); }
        }

        private ICommandManager _commandManager;

        public ICommandManager CommandManager
        {
            get { return _commandManager; }
            set { PropertyChangeRef("CommandManager", ref _commandManager, value); }
        }


        #endregion Properties


        #region Commands

        private ICommand _commandFileOpen;
        
        public ICommand CommandFileOpen
        {
            get { return _commandFileOpen; }
            set { PropertyChangeRef("CommandFileOpen", ref _commandFileOpen, value); }
        }

        private ICommand _commandFileSave;

        public ICommand CommandFileSave
        {
            get { return _commandFileSave; }
            set { PropertyChangeRef("CommandFileSave", ref _commandFileSave, value); }
        }

        private ICommand _commandFileExit;

        public ICommand CommandFileExit
        {
            get { return _commandFileSave; }
            set { PropertyChangeRef("CommandFileExit", ref _commandFileExit, value); }
        }

        private ICommand _commandViewFullScreen;

        public ICommand CommandViewFullScreen
        {
            get { return _commandViewFullScreen; }
            set { PropertyChangeRef("CommandViewFullScreen", ref _commandViewFullScreen, value); }
        }

        private ICommand _commandViewSyncVBlank;

        public ICommand CommandViewSyncVBlank
        {
            get { return _commandViewSyncVBlank; }
            set { PropertyChangeRef("CommandViewSyncVBlank", ref _commandViewSyncVBlank, value); }
        }

        private ICommand _commandVmPause;

        public ICommand CommandVmPause
        {
            get { return _commandVmPause; }
            set { PropertyChangeRef("CommandVmPause", ref _commandVmPause, value); }
        }

        private ICommand _commandVmMaxSpeed;

        public ICommand CommandVmMaxSpeed
        {
            get { return _commandVmMaxSpeed; }
            set { PropertyChangeRef("CommandVmMaxSpeed", ref _commandVmMaxSpeed, value); }
        }

        private ICommand _commandVmWarmReset;

        public ICommand CommandVmWarmReset
        {
            get { return _commandVmWarmReset; }
            set { PropertyChangeRef("CommandVmWarmReset", ref _commandVmWarmReset, value); }
        }

        private ICommand _commandVmColdReset;

        public ICommand CommandVmColdReset
        {
            get { return _commandVmColdReset; }
            set { PropertyChangeRef("CommandVmColdReset", ref _commandVmColdReset, value); }
        }

        private ICommand _commandVmNmi;

        public ICommand CommandVmNmi
        {
            get { return _commandVmNmi; }
            set { PropertyChangeRef("CommandVmNmi", ref _commandVmNmi, value); }
        }

        private ICommand _commandVmSettings;

        public ICommand CommandVmSettings
        {
            get { return _commandVmSettings; }
            set { PropertyChangeRef("CommandVmSettings", ref _commandVmSettings, value); }
        }

        private ICommand _commandHelpViewHelp;

        public ICommand CommandHelpViewHelp
        {
            get { return _commandHelpViewHelp; }
            set { PropertyChangeRef("CommandHelpViewHelp", ref _commandHelpViewHelp, value); }
        }

        private ICommand _commandHelpKeyboardHelp;

        public ICommand CommandHelpKeyboardHelp
        {
            get { return _commandHelpKeyboardHelp; }
            set { PropertyChangeRef("CommandHelpKeyboardHelp", ref _commandHelpKeyboardHelp, value); }
        }

        private ICommand _commandHelpAbout;

        public ICommand CommandHelpAbout
        {
            get { return _commandHelpAbout; }
            set { PropertyChangeRef("CommandHelpAbout", ref _commandHelpAbout, value); }
        }

        private ICommand _commandTapePause;

        public ICommand CommandTapePause
        {
            get { return _commandTapePause; }
            set { PropertyChangeRef("CommandTapePause", ref _commandTapePause, value); }
        }

        private ICommand _commandQuickLoad;

        public ICommand CommandQuickLoad
        {
            get { return _commandQuickLoad; }
            set { PropertyChangeRef("CommandQuickLoad", ref _commandQuickLoad, value); }
        }

        private ICommand _commandOpenUri;

        public ICommand CommandOpenUri
        {
            get { return _commandOpenUri; }
            set { PropertyChangeRef("CommandOpenUri", ref _commandOpenUri, value); }
        }

        #endregion Commands
    }
}
