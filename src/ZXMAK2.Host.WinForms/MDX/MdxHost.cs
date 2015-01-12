﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public class MdxHost : IHost
    {
        private DirectKeyboard m_keyboard;
        private DirectMouse m_mouse;
        private DirectJoystick m_joystick;
        private DirectSound m_sound;
        

        public MdxHost(Form form, IHostVideo hostVideo)
        {
            HostUi = form as ICommandManager;
            Video = hostVideo;
            SafeExecute(() => m_sound = new DirectSound(form, -1, 44100, 16, 2, 882 * 2 * 2, 4));
            SafeExecute(() => m_keyboard = new DirectKeyboard(form));
            SafeExecute(() => m_mouse = new DirectMouse(form));
            SafeExecute(() => m_joystick = new DirectJoystick(form));
        }

        public void Dispose()
        {
            if (m_sound != null)
                m_sound.Dispose();
            m_sound = null;
            if (m_keyboard != null)
                m_keyboard.Dispose();
            m_keyboard = null;
            if (m_mouse != null)
                m_mouse.Dispose();
            m_mouse = null;
            if (m_joystick != null)
                m_joystick.Dispose();
            m_joystick = null;
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
