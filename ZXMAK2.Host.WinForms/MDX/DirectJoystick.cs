﻿// (c) 2013 Eltaron
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using Microsoft.DirectX.DirectInput;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZxmakKey = ZXMAK2.Host.Entities.Key;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public sealed class DirectJoystick : IHostJoystick, IDisposable
    {
        #region Fields

        private const string KeyboardNumpadId = "keyboard";

        private readonly Dictionary<string, IJoystickState> m_states = new Dictionary<string, IJoystickState>();
        private readonly Dictionary<string, Device> m_devices = new Dictionary<string, Device>();
        private readonly Dictionary<string, bool> m_acquired = new Dictionary<string, bool>();
        private Form m_form;
        private IntPtr m_hwnd;
        private IJoystickState m_numpadState;

        #endregion Fields


        #region Public

        public DirectJoystick(Form form)
        {
            m_form = form;
            m_hwnd = form.Handle;
            m_form.Activated += WndActivated;
            m_form.Deactivate += WndDeactivate;
        }

        public void Scan()
        {
            var guidList = m_devices.Keys;
            foreach (var guid in guidList)
            {
                ActivateDevice(guid);
                m_states[guid] = ScanDevice(guid);
            }
            if (IsKeyboardStateRequired)
            {
                var isUp = KeyboardState[ZxmakKey.NumPad8];
                var isDown = KeyboardState[ZxmakKey.NumPad2];
                var isLeft = KeyboardState[ZxmakKey.NumPad4];
                var isRight = KeyboardState[ZxmakKey.NumPad6];
                var isFire = KeyboardState[ZxmakKey.NumPad5] ||
                    KeyboardState[ZxmakKey.NumPad0];
                m_numpadState = new StateWrapper(
                    isLeft,
                    isRight,
                    isUp,
                    isDown,
                    isFire);
            }
        }

        public void Dispose()
        {
            if (m_form != null)
            {
                m_form.Activated -= WndActivated;
                m_form.Deactivate -= WndDeactivate;
            }
            foreach (var guid in m_devices.Keys)
            {
                ReleaseHostDevice(guid);
            }
        }

        public void CaptureHostDevice(string hostId)
        {
            try
            {
                if (hostId == string.Empty)
                {
                    return;
                }
                if (hostId == KeyboardNumpadId)
                {
                    IsKeyboardStateRequired = true;
                    return;
                }
                var list = Manager.GetDevices(
                    DeviceClass.GameControl,
                    EnumDevicesFlags.AttachedOnly);
                while (list.MoveNext())
                {
                    var deviceInstance = (DeviceInstance)list.Current;
                    if (string.Compare(
                        GetDeviceId(deviceInstance.InstanceGuid),
                        hostId,
                        true) != 0)
                    {
                        continue;
                    }
                    var joystick = new Device(deviceInstance.InstanceGuid);
                    try
                    {
                        joystick.SetCooperativeLevel(null, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                        joystick.SetDataFormat(DeviceDataFormat.Joystick);
                        joystick.Acquire();
                        m_devices.Add(hostId, joystick);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        joystick.Dispose();
                        continue;
                    }
                    ActivateDevice(hostId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void ReleaseHostDevice(string hostId)
        {
            try
            {
                if (hostId == KeyboardNumpadId)
                {
                    IsKeyboardStateRequired = false;
                    return;
                }
                if (!m_devices.ContainsKey(hostId))
                {
                    return;
                }
                var device = m_devices[hostId];
                DeactivateDevice(hostId);
                try
                {
                    device.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                m_devices.Remove(hostId);
                if (m_states.ContainsKey(hostId))
                {
                    m_states.Remove(hostId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public IJoystickState GetState(string hostId)
        {
            if (m_states.ContainsKey(hostId))
            {
                return m_states[hostId];
            }
            if (hostId == KeyboardNumpadId)
            {
                return m_numpadState;
            }
            return StateWrapper.Empty;
        }

        public IKeyboardState KeyboardState { get; set; }
        public bool IsKeyboardStateRequired { get; private set; }

        public IEnumerable<IHostDeviceInfo> GetAvailableJoysticks()
        {
            var list = new List<IHostDeviceInfo>();
            try
            {
                var devList = Manager.GetDevices(
                    DeviceClass.GameControl,
                    EnumDevicesFlags.AttachedOnly);
                while (devList.MoveNext())
                {
                    var deviceInstance = (DeviceInstance)devList.Current;
                    var hdi = new HostDeviceInfo(
                        deviceInstance.InstanceName,
                        GetDeviceId(deviceInstance.InstanceGuid));
                    list.Add(hdi);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            list.Sort();
            list.Insert(
                0,
                new HostDeviceInfo("Keyboard Numpad", KeyboardNumpadId));
            list.Insert(
                0,
                new HostDeviceInfo("None", string.Empty));
            return list;
        }

        #endregion Public


        #region Private

        private static string GetDeviceId(Guid guid)
        {
            return guid.ToString(null, CultureInfo.InvariantCulture);
        }

        private void WndActivated(object sender, EventArgs e)
        {
            var guidList = m_devices.Keys;
            foreach (var guid in guidList)
            {
                ActivateDevice(guid);
            }
        }

        private void WndDeactivate(object sender, EventArgs e)
        {
            var guidList = m_devices.Keys;
            foreach (var guid in guidList)
            {
                DeactivateDevice(guid);
            }
        }

        private void ActivateDevice(string guid)
        {
            try
            {
                var device = m_devices[guid];
                var acquired = m_acquired.ContainsKey(guid) &&
                    m_acquired[guid];
                if (!acquired)
                {
                    device.Acquire();
                }
                m_acquired[guid] = true;
            }
            catch
            {
                m_acquired[guid] = false;
            }
        }

        private void DeactivateDevice(string guid)
        {
            try
            {
                var device = m_devices[guid];
                device.Unacquire();
                m_acquired[guid] = false;
            }
            catch
            {
            }
        }


        private IJoystickState ScanDevice(string hostId)
        {
            try
            {
                if (!m_acquired.ContainsKey(hostId) || 
                    !m_acquired[hostId])
                {
                    return StateWrapper.Empty;
                }
                var device = m_devices[hostId];

                // axisTolerance check is needed because of little fluctuation of axis values even when nothing is pressed.
                int axisTolerance = 0x1000; // Should this be taken from joystick device somehow?
                ushort center = 0x7FFF;

                try
                {
                    device.Poll();
                    var diState = device.CurrentJoystickState;

                    var isDown = diState.Y > center && diState.Y - center > axisTolerance;
                    var isUp = diState.Y < center && center - diState.Y > axisTolerance;
                    var isRight = diState.X > center && diState.X - center > axisTolerance;
                    var isLeft = diState.X < center && center - diState.X > axisTolerance;
                    var isFire = false;

                    var buttons = diState.GetButtons();
                    foreach (var button in buttons)
                    {
                        // fire = any key pressed
                        isFire |= (button & 0x80) != 0;
                    }

                    return new StateWrapper(
                        isLeft,
                        isRight,
                        isUp,
                        isDown,
                        isFire);
                }
                catch (NotAcquiredException)
                {
                    m_acquired[hostId] = false;
                }
                catch (InputLostException)
                {
                    m_acquired[hostId] = false;
                    //ReleaseHostDevice(hostId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return StateWrapper.Empty;
        }

        private class StateWrapper : IJoystickState
        {
            #region Static

            private static readonly StateWrapper s_empty = new StateWrapper(false, false, false, false, false);

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
