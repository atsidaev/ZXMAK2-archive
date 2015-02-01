using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.Xna4.Xna
{
    public sealed class XnaMouse : IHostMouse, IMouseState
    {
        public void Update(MouseState state)
        {
            var buttons = 0;
            if (state.LeftButton == ButtonState.Pressed) buttons |= 1;
            if (state.RightButton == ButtonState.Pressed) buttons |= 2;
            if (state.MiddleButton == ButtonState.Pressed) buttons |= 4;
            Buttons = buttons;
            X = state.X;
            Y = state.Y;
        }

        public void Dispose()
        {
        }

        
        #region IHostMouse

        public void Scan()
        {
        }

        public IMouseState MouseState
        {
            get { return this; }
        }

        #endregion IHostMouse


        #region IMouseState

        public int X { get; private set; }

        public int Y { get; private set; }

        public int Buttons { get; private set; }

        #endregion IMouseState
    }
}
