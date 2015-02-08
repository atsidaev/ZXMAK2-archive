using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Host.Services;


namespace ZXMAK2.Host.Xna4.Xna
{
    public class XnaHost : IHost
    {
        #region Fields

        private SyncSource m_syncSource;
        private TimeSync m_timeSync;
        private IHostVideo m_video;
        private IHostSound m_sound;

        private IHostKeyboard m_keyboard;
        private IHostMouse m_mouse;
        private IHostJoystick m_joystick;

        #endregion Fields


        #region .ctor

        public XnaHost(IHostVideo hostVideo)
        {
            m_video = hostVideo;

            m_timeSync = new TimeSync();
            var viewResolver = Locator.Resolve<IResolver>("View");
            if (viewResolver != null)
            {
                m_sound = viewResolver.TryResolve<IHostSound>();
            }
            m_keyboard = new XnaKeyboard();
            m_mouse = new XnaMouse();
            UpdateSyncSource();
        }

        public void Dispose()
        {
            Dispose(ref m_timeSync);
            Dispose(ref m_sound);
            Dispose(ref m_keyboard);
            Dispose(ref m_mouse);
            Dispose(ref m_joystick);
            // temporary not supported (reentrance)
            //Dispose(ref Video);
        }

        #endregion .ctor


        #region IHost

        public IHostKeyboard Keyboard { get { return m_keyboard; } }
        public IHostMouse Mouse { get { return m_mouse; } }
        public IHostJoystick Joystick { get { return m_joystick; } }

        public SyncSource SyncSource
        {
            get { return m_syncSource; }
            set
            {
                m_syncSource = value;
                UpdateSyncSource();
            }
        }

        private void UpdateSyncSource()
        {
            var video = m_video;
            var sound = m_sound;
            sound.IsSynchronized = m_syncSource == SyncSource.Sound;
            video.IsSynchronized = m_syncSource == SyncSource.Video;
        }


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
            ISoundFrame soundFrame)
        {
            // frame sync
            var timeSync = m_timeSync;
            var sound = m_sound;
            var video = m_video;
            if (videoFrame.IsRefresh)
            {
                // request from UI, so we don't need sound and sync
                if (video != null && videoFrame != null)
                {
                    video.PushFrame(videoFrame);
                }
                return;
            }
            if (SyncSource == SyncSource.Time && timeSync != null)
            {
                timeSync.WaitFrame();
            }
            if (video != null && videoFrame != null)
            {
                video.PushFrame(videoFrame);
            }
            if (sound != null && soundFrame != null)
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

        public void Update(KeyboardState kbdState, MouseState mouseState)
        {
            var keyboard = Keyboard as XnaKeyboard;
            if (keyboard != null)
            {
                keyboard.Update(kbdState);
            }
            var mouse = Mouse as XnaMouse;
            if (mouse != null)
            {
                mouse.Update(mouseState);
            }
        }

        #endregion Public


        #region Private

        private static void Dispose<T>(ref T disposable)
            where T : IDisposable
        {
            var value = disposable;
            disposable = default(T);
            if (value != null)
            {
                value.Dispose();
            }
        }

        #endregion Private
    }
}
