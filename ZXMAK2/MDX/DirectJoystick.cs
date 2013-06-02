// (c) 2013 Eltaron
using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;
using ZXMAK2.Interfaces;


namespace ZXMAK2.MDX
{
    public class DirectJoystick : IHostJoystick, IDisposable
    {
        #region Fields

        private Device m_diJoystick;

        #endregion Fields


        #region Properties

        public IJoystickState State { get; private set; }

        #endregion Properties


        #region Public

        public DirectJoystick(Form form)
        {
            try
            {
                State = StateWrapper.Empty;
                var gameControllerList = Manager.GetDevices(
                    DeviceClass.GameControl,
                    EnumDevicesFlags.AttachedOnly);
                if (gameControllerList.Count > 0)
                {
                    gameControllerList.MoveNext();
                    var deviceInstance = (DeviceInstance)gameControllerList.Current;
                    var joystick = new Device(deviceInstance.InstanceGuid);
                    joystick.SetCooperativeLevel(form, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                    joystick.SetDataFormat(DeviceDataFormat.Joystick);
                    joystick.Acquire();
                    m_diJoystick = joystick;
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        public void Scan()
        {
            try
            {
                if (m_diJoystick == null)
                {
                    State = StateWrapper.Empty;
                    return;
                }

                // axisTolerance check is needed because of little fluctuation of axis values even when nothing is pressed.
                int axisTolerance = 0x1000; // Should this be taken from joystick device somehow?
                ushort center = 0x7FFF;

                try
                {
                    m_diJoystick.Poll();
                    var diState = m_diJoystick.CurrentJoystickState;

                    var isUp = diState.Y > center && diState.Y - center > axisTolerance;
                    var isDown = diState.Y < center && center - diState.Y > axisTolerance;
                    var isLeft = diState.X > center && diState.X - center > axisTolerance;
                    var isRight = diState.X < center && center - diState.X > axisTolerance;
                    var isFire = false;

                    var buttons = diState.GetButtons();
                    foreach (var button in buttons)
                    {
                        // fire = any key pressed
                        isFire |= (button & 0x80) != 0;
                    }

                    State = new StateWrapper(
                        isLeft,
                        isRight,
                        isUp,
                        isDown,
                        isFire);
                }
                catch (InputLostException)
                {
                    State = StateWrapper.Empty;
                    ReleaseJoystick();
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        public void Dispose()
        {
            if (m_diJoystick != null)
            {
                ReleaseJoystick();
            }
        }

        #endregion Public


        #region Private

        private void ReleaseJoystick()
        {
            try
            {
                m_diJoystick.Unacquire();
                m_diJoystick.Dispose();
                m_diJoystick = null;
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private class StateWrapper : IJoystickState
        {
            #region Static

            private static StateWrapper s_empty = new StateWrapper(false, false, false, false, false);

            public static StateWrapper Empty
            {
                get { return s_empty; }
            }

            #endregion


            #region Public

            public StateWrapper(
                bool isLeft,
                bool isRight,
                bool isUp,
                bool isDown,
                bool isFire)
            {
                IsLeft = isLeft;
                IsRight = isRight;
                IsUp = isUp;
                IsDown = isDown;
                IsFire = isFire;
            }

            public StateWrapper()
                : this(false, false, false, false, false)
            {
            }

            public bool IsLeft { get; private set; }
            public bool IsRight { get; private set; }
            public bool IsUp { get; private set; }
            public bool IsDown { get; private set; }
            public bool IsFire { get; private set; }

            #endregion
        }

        #endregion Private
    }
}
