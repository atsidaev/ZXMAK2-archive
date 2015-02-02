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

        bool CheckSyncSourceSupported(SyncSource value);
        int GetSampleRate();
        void PushFrame(
            IVideoFrame videoFrame, 
            ISoundFrame soundFrame,
            bool isRequested);
        void CancelPush();
    }
}
