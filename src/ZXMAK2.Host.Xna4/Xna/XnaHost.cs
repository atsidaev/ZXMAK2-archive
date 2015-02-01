using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Dependency;


namespace ZXMAK2.Host.Xna4.Xna
{
    public class XnaHost : IHost
    {
        public XnaHost(IHostVideo hostVideo)
        {
            Video = hostVideo;
            
            var viewResolver = Locator.Resolve<IResolver>("View");
            if (viewResolver != null)
            {
                Sound = viewResolver.TryResolve<IHostSound>();
            }
            Keyboard = new XnaKeyboard();
            Mouse = new XnaMouse();
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
            var sound = Sound;
            Sound = null;
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
    }
}
