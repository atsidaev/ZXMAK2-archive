using System;
using System.Linq;
using ZXMAK2.Interfaces;
using Microsoft.Xna.Framework.Input;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.Xna4.Xna
{
    public class XnaHost : IHost
    {
        public XnaHost(IHostVideo hostVideo)
        {
            Video = hostVideo;
            
            Keyboard = new XnaKeyboard();
            Mouse = new XnaMouse();
            var sound = new XnaSound(44100, 2);
            sound.Start();
            Sound = sound;
        }

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


        public IHostVideo Video { get; private set; }
        public IHostSound Sound { get; private set; }
        public IHostKeyboard Keyboard { get; private set; }
        public IHostMouse Mouse { get; private set; }
        public IHostJoystick Joystick { get; private set; }
        
        public void Dispose()
        {
            if (Sound != null)
            {
                var sound = (XnaSound)Sound;
                Sound = null;
                sound.Stop();
            }
        }
    }
}
