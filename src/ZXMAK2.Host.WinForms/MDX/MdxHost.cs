using System;
using System.Windows.Forms;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.Dependency;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public sealed class MdxHost : IHost
    {
        private IHostSound m_sound;
        private IHostKeyboard m_keyboard;
        private DirectMouse m_mouse;
        private DirectJoystick m_joystick;


        public MdxHost(Form form, IHostVideo hostVideo)
        {
            HostUi = form as ICommandManager;
            Video = hostVideo;
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

        public ICommandManager HostUi { get; private set; }
        public IHostVideo Video { get; private set; }
        public IHostSound Sound { get { return m_sound; } }
        public IHostKeyboard Keyboard { get { return m_keyboard; } }
        public IHostMouse Mouse { get { return m_mouse; } }
        public IHostJoystick Joystick { get { return m_joystick; } }


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
    }
}
