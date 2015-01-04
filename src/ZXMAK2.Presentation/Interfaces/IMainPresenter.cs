using System;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Presentation.Interfaces
{
    public interface IMainPresenter : IDisposable
    {
        void Run();

        ICommand CommandFileOpen { get; }
        ICommand CommandFileSave { get; }
        ICommand CommandFileExit { get; }
        ICommand CommandViewFullScreen { get; }
        ICommand CommandViewSyncVBlank { get; }
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
