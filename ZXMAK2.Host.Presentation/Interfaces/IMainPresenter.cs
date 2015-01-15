using System;
using ZXMAK2.Presentation.Interfaces;


namespace ZXMAK2.Host.Presentation.Interfaces
{
    public interface IMainPresenter : IDisposable
    {
        void Run();

        ICommand CommandFileOpen { get; }
        ICommand CommandFileSave { get; }
        ICommand CommandFileExit { get; }
        ICommand CommandViewFullScreen { get; }
        ICommand CommandViewSyncSource { get; }
        ICommand CommandVmPause { get; }
        ICommand CommandVmMaxSpeed { get; }
        ICommand CommandVmWarmReset { get; }
        ICommand CommandVmColdReset { get; }
        ICommand CommandVmNmi { get; }
        ICommand CommandVmSettings { get; }
        ICommand CommandHelpViewHelp { get; }
        ICommand CommandHelpKeyboardHelp { get; }
        ICommand CommandHelpAbout { get; }
        ICommand CommandTapePause { get; }
        ICommand CommandQuickLoad { get; }
        ICommand CommandOpenUri { get; }
    }
}
