/// Description: ZX Spectrum keyboard emulator
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using Microsoft.DirectX.DirectInput;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities.Tools;
using ZXMAK2.Host.WinForms.Tools;
using ZxmakKey = ZXMAK2.Host.Entities.Key;
using MdxKey = Microsoft.DirectX.DirectInput.Key;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public sealed class DirectKeyboard : IHostKeyboard, IKeyboardState
    {
        private readonly Form _form;
        private readonly IntPtr _hWnd;
        private Device _device;
        private KeyboardStateMapper<MdxKey> _mapper = new KeyboardStateMapper<MdxKey>();
        private readonly Dictionary<ZxmakKey, bool> _state = new Dictionary<ZxmakKey, bool>();
        private bool _isAcquired;



        public DirectKeyboard(Form form)
        {
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }
            _form = form;
            _hWnd = form.Handle;
            _device = new Device(SystemGuid.Keyboard);
            _device.SetCooperativeLevel(form, CooperativeLevelFlags.NonExclusive | CooperativeLevelFlags.Foreground);
            form.Deactivate += WndDeactivate;
            TryAcquire();
            _mapper.LoadMapFromString(
                global::ZXMAK2.Host.WinForms.Properties.Resources.Keyboard_Mdx);
        }

        public void Dispose()
        {
            if (_device == null)
            {
                return;
            }
            // TODO: sync needed
            var device = _device;
            _device = null;
            try
            {
                _form.Deactivate -= WndDeactivate;
                if (_isAcquired)
                {
                    _isAcquired = false;
                    device.Unacquire();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                device.Dispose();
            }
        }


        #region IKeyboardState

        public bool this[ZxmakKey key]
        {
            get { return _state.ContainsKey(key) && _state[key]; }
        }

        #endregion IKeyboardState


        #region IHostKeyboard

        public IKeyboardState State
        {
            get { return this; }
        }

        //public void LoadConfiguration(string fileName)
        //{
        //    using (var reader = (TextReader)new StreamReader(fileName))
        //    {
        //        var xml = reader.ReadToEnd();
        //        var mapper = new KeyboardStateMapper<MdxKey>();
        //        mapper.LoadMapFromString(xml);
        //        _mapper = mapper;
        //    }
        //}

        public void Scan()
        {
            if (_device == null || (!_isAcquired && !TryAcquire()))
            {
                foreach (var key in _mapper.Keys)
                {
                    _state[key] = false;
                }
                return;
            }
            try
            {
                var state = _device.GetCurrentKeyboardState();
                foreach (var key in _mapper.Keys)
                {
                    _state[key] = state[_mapper[key]];
                }
            }
            catch (NotAcquiredException)
            {
                // TODO: sync needed
            }
            catch (InputLostException)
            {
                WndDeactivate(null, null);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                WndDeactivate(null, null);
            }
        }

        #endregion IHostKeyboard


        #region Private

        private bool TryAcquire()
        {
            if (_device == null || 
                _hWnd != NativeMethods.GetForegroundWindow())
            {
                return false;
            }
            try
            {
                _device.Acquire();
                _isAcquired = true;
                return true;
            }
            catch (OtherApplicationHasPriorityException)
            {
            }
            catch (InputLostException)
            {
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return false;
        }

        private void WndDeactivate(object sender, EventArgs e)
        {
            if (_device == null || !_isAcquired)
            {
                return;
            }
            try
            {
                _isAcquired = false;
                _device.Unacquire();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion Private
    }
}
