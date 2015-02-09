using System;
using ZXMAK2.Host.Entities;


namespace ZXMAK2.Host.Interfaces
{
    public interface IHost : IDisposable
    {
        IHostKeyboard Keyboard { get; }
        IHostMouse Mouse { get; }
        IHostJoystick Joystick { get; }
        SyncSource SyncSource { get; set; }
        int SampleRate { get; }
        bool IsCaptured { get; }

        bool CheckSyncSourceSupported(SyncSource value);
        void PushFrame(IVideoFrame videoFrame, ISoundFrame soundFrame);
        void CancelPush();
        void Capture();
        void Uncapture();
    }
}
