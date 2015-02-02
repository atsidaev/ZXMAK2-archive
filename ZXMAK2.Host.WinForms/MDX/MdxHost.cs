using System;
using System.Windows.Forms;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.Dependency;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Engine;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public sealed class MdxHost : IHost
    {
        #region Fields

        private SyncTime m_timeSync;
        private IHostVideo m_video;
        private IHostSound m_sound;
        private IHostKeyboard m_keyboard;
        private DirectMouse m_mouse;
        private DirectJoystick m_joystick;

        #endregion Fields


        #region .ctor

        public MdxHost(Form form, IHostVideo hostVideo)
        {
            m_video = hostVideo;
            m_timeSync = new SyncTime();
            var viewResolver = Locator.TryResolve<IResolver>("View");
            if (viewResolver != null)
            {
                m_sound = viewResolver.TryResolve<IHostSound>(new Argument("form", form)); //new DirectSound(form, 44100, 4)
                m_keyboard = viewResolver.TryResolve<IHostKeyboard>(new Argument("form", form));
            }
            SafeExecute(() => m_mouse = new DirectMouse(form));
            SafeExecute(() => m_joystick = new DirectJoystick(form));
        }

        public void Dispose()
        {
            var time = m_timeSync;
            m_timeSync = null;
            if (time != null)
            {
                time.Dispose();
            }
            var sound = m_sound;
            m_sound = null;
            if (sound != null)
            {
                sound.Dispose();
            }
            var keyboard = m_keyboard;
            m_keyboard = null;
            if (keyboard != null)
            {
                keyboard.Dispose();
                keyboard = null;
            }
            var mouse = m_mouse;
            m_mouse = null;
            if (mouse != null)
            {
                mouse.Dispose();
                mouse = null;
            }
            var joystick = m_joystick;
            m_joystick = null;
            if (joystick != null)
            {
                joystick.Dispose();
                joystick = null;
            }
        }

        #endregion .ctor


        #region IHost

        public IHostKeyboard Keyboard { get { return m_keyboard; } }
        public IHostMouse Mouse { get { return m_mouse; } }
        public IHostJoystick Joystick { get { return m_joystick; } }
        public SyncSource SyncSource { get; set; }


        public bool CheckSyncSourceSupported(SyncSource value)
        {
            switch (value)
            {
                case SyncSource.None:
                    return true;
                case SyncSource.Time:
                    var time = m_timeSync;
                    return time != null;
                case SyncSource.Sound:
                    var sound = m_sound;
                    return sound != null;
                case SyncSource.Video:
                    var video = m_video;
                    return video != null;
                default:
                    return false;
            }
        }

        public int GetSampleRate()
        {
            var sound = m_sound;
            return sound != null ? sound.SampleRate : 22050;
        }

        public void PushFrame(
            IVideoFrame videoFrame,
            ISoundFrame soundFrame,
            bool isRequested)
        {
            // frame sync
            var timeSync = m_timeSync;
            var sound = m_sound;
            var video = m_video;
            switch (SyncSource)
            {
                case SyncSource.Time:
                    if (timeSync != null)
                    {
                        timeSync.WaitFrame();
                    }
                    break;
                case SyncSource.Sound:
                    if (sound != null)
                    {
                        sound.WaitFrame();
                    }
                    break;
                case SyncSource.Video:
                    if (video != null)
                    {
                        video.WaitFrame();
                    }
                    break;
            }
            if (video != null)
            {
                video.PushFrame(videoFrame, isRequested);
            }
            if (sound != null)
            {
                sound.PushFrame(soundFrame);
            }
        }

        public void CancelPush()
        {
            var timeSync = m_timeSync;
            if (timeSync != null)
            {
                timeSync.CancelWait();
            }
            var video = m_video;
            if (video != null)
            {
                video.CancelWait();
            }
            var sound = m_sound;
            if (sound != null)
            {
                sound.CancelWait();
            }
        }

        #endregion IHost


        #region Public

        public bool IsInputCaptured
        {
            get { return m_mouse != null && m_mouse.IsCaptured; }
        }

        public void StopInputCapture()
        {
            if (m_mouse != null)
            {
                m_mouse.StopCapture();
            }
        }

        public void StartInputCapture()
        {
            if (m_mouse != null)
            {
                m_mouse.StartCapture();
            }
        }

        #endregion Public


        #region Private

        private void SafeExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion Private
    }
}
