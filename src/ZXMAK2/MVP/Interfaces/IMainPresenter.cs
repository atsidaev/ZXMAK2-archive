using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;

namespace ZXMAK2.MVP.Interfaces
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
