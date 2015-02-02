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

        private SyncTime m_timeSync;
        private IHostVideo m_video;
        private IHostSound m_sound;

        #endregion Fields


        #region .ctor

        public XnaHost(IHostVideo hostVideo)
        {
            m_video = hostVideo;
            m_timeSync = new SyncTime();
            
            var viewResolver = Locator.Resolve<IResolver>("View");
            if (viewResolver != null)
            {
                m_sound = viewResolver.TryResolve<IHostSound>();
            }
            Keyboard = new XnaKeyboard();
            Mouse = new XnaMouse();
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
            var keyboard = Keyboard;
            Keyboard = null;
            if (keyboard != null)
            {
                keyboard.Dispose();
            }
            var mouse = Mouse;
            Mouse = null;
            if (mouse != null)
            {
                mouse.Dispose();
            }
            var joystick = Joystick;
            Joystick = null;
            if (joystick != null)
            {
                joystick.Dispose();
            }
            // temporary not supported (reentrance)
            //var video = Video;
            //Video = null;
            //if (video != null)
            //{
            //    video.Dispose();
            //}
        }

        #endregion .ctor


        #region IHost

        public IHostKeyboard Keyboard { get; private set; }
        public IHostMouse Mouse { get; private set; }
        public IHostJoystick Joystick { get; private set; }
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
    }
}
