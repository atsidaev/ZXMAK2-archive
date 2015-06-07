/* 
 *  Copyright 2008, 2015 Alex Makeev
 * 
 *  This file is part of ZXMAK2 (ZX Spectrum virtual machine).
 *
 *  ZXMAK2 is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ZXMAK2 is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with ZXMAK2.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.DirectX.Direct3D;
using ZXMAK2.Host.WinForms.Tools;
using System.Threading;
using NativeWindow=System.Windows.Forms.NativeWindow;
using Message = System.Windows.Forms.Message;
using Size=System.Drawing.Size;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public sealed class AllocatorPresenter
    {
        #region Fields

        private readonly object _syncRoot = new object();
        private readonly List<IRenderer> _renderers = new List<IRenderer>();
        private readonly HashSet<IRenderer> _loaded = new HashSet<IRenderer>();
        private SubclassWindow _window;
        private IntPtr _monitorId;
        private Thread _thread;
        private Device _device;
        private PresentParameters _d3dpp;
        private SwapChain _swapChain;
        private Surface _renderTarget;
        private Size _size;

        #endregion Fields


        #region .ctor

        public AllocatorPresenter()
        {
            _thread = new Thread(RenderThreadProc);
            _thread.IsBackground = false;
        }

        public void Dispose()
        {
            Detach();
        }

        #endregion .ctor


        #region IDeviceAllocator

        public event EventHandler PresentCompleted;

        public void Attach(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                throw new ArgumentNullException("hwnd");
            }
            lock (_syncRoot)
            {
                if (_window != null)
                {
                    throw new InvalidOperationException("Already attached!");
                }
                _window = new SubclassWindow(this);
                _window.AssignHandle(hwnd);
                _thread.Start();
            }
        }

        public void Detach()
        {
            lock (_syncRoot)
            {
                if (_window != null)
                {
                    _window.ReleaseHandle();
                    _window = null;
                }
                _thread.Abort();
            }
            // wait outside lock to avoid deadlock
            _thread.Join();
        }

        private bool Render()
        {
            lock (_syncRoot)
            {
                try
                {
                    if (_monitorId != _window.MonitorId)
                    {
                        OnLost();
                        OnDeviceDestroy();
                    }
                    OnDeviceCheck();
                    if (_device == null)
                    {
                        Thread.Sleep(1000);
                        return false;
                    }
                    OnAcquireCheck();
                    OnLoadCheck();
                    if (_renderTarget == null)
                    {
                        return true;
                    }
                    var wndSize = _window.Size;
                    if (wndSize.Width < 1 ||
                        wndSize.Height < 1 ||
                        !_window.Visible)
                    {
                        return false;
                    }
                    if (_renderTarget == null ||
                        wndSize != _size)
                    {
                        OnLost();
                        return true;
                    }
                    
                    var hr = TestCooperativeLevel();
                    switch (hr)
                    {
                        case ResultCode.Success:
                            OnRender();
                            if (_swapChain != null)
                            {
                                _swapChain.Present(); // Present.DoNotWait // Present.None
                            }
                            else
                            {
                                _device.Present();
                            }
                            OnPresentCompleted();
                            return true;
                        case ResultCode.DeviceNotReset:
                            OnLost();
                            OnDeviceDestroy();
                            break;
                        case ResultCode.DeviceLost:
                            OnLost();
                            break;
                        default:
                            Logger.Warn("TestCooperativeLevel() = {0}", hr);
                            break;
                    }
                    return false;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (DeviceLostException ex)
                {
                    Logger.Debug(ex);
                    return false;
                }
                catch (GraphicsException ex)
                {
                    Logger.Error(ex);
                    Thread.Sleep(1000);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    Thread.Sleep(1000);
                    return false;
                }
            }
        }

        public void Register(IRenderer renderer)
        {
            ExecuteSynchronized(() => _renderers.Add(renderer));
        }

        public void Unregister(IRenderer renderer)
        {
            ExecuteSynchronized(() =>
            {
                if (_loaded.Contains(renderer))
                {
                    _loaded.Remove(renderer);
                    renderer.Unload();
                }
                _renderers.Remove(renderer);
            });
        }

        #endregion IDeviceAllocator


        #region Internal

        internal Device Device
        {
            get { return _device; }
        }

        internal void ExecuteSynchronized(Action action)
        {
            lock (_syncRoot)
            {
                action();
            }
        }

        internal T ExecuteSynchronized<T>(Func<T> action)
        {
            lock (_syncRoot)
            {
                return action();
            }
        }

        #endregion Internal


        #region Private

        private void RenderThreadProc()
        {
            try
            {
                while (true)
                {
                    var success = Render();
                    if (!success)
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }
            finally
            {
                OnLost();
                OnDeviceDestroy();
                Logger.Debug("Direct3D: RenderThreadProc finished");
            }
        }

        private void OnPresentCompleted()
        {
            var handler = PresentCompleted;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnDeviceCheck()
        {
            if (_device != null)
            {
                return;
            }
            _size = _window.Size;
            _d3dpp = CreatePresentParams(_window.Handle);

            _monitorId = _window.MonitorId;
            var adapterId = _window.AdapterId;
            var caps = Manager.GetDeviceCaps(adapterId, DeviceType.Hardware);
            var flags = CreateFlags.NoWindowChanges | CreateFlags.FpuPreserve;// CreateFlags.MultiThreaded;
            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                flags |= CreateFlags.HardwareVertexProcessing;
            else
                flags |= CreateFlags.SoftwareVertexProcessing;

            Logger.Debug("Direct3D: create Device, threadId={0}", Thread.CurrentThread.ManagedThreadId);
            _device = new Device(
                adapterId,
                DeviceType.Hardware,
                _window.Handle,
                flags,
                _d3dpp);
            _device.DeviceResizing += (s, e) => e.Cancel = true;
            //_device.DeviceReset += Device_OnDeviceReset;

            // Set default sampler/renderer state
            _device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Linear);
            _device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Linear);
            _device.SetRenderState(RenderStates.Lighting, false);
            _device.SetRenderState(RenderStates.ZEnable, true);
            _device.SetRenderState(RenderStates.DitherEnable, true);
            _device.SetRenderState(RenderStates.CullMode, 1);

            _device.SetRenderState(RenderStates.AlphaBlendEnable, true);
            _device.SetRenderState(RenderStates.SourceBlend, (int)Blend.SourceAlpha);
            _device.SetRenderState(RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha);
            //_device.SetRenderState(RenderStates.BlendOperation, (int)BlendOperation.Add);
        }

        private void OnDeviceDestroy()
        {
            if (_device != null)
            {
                Logger.Debug("Direct3D: dispose Device, threadId={0}", Thread.CurrentThread.ManagedThreadId);
                _device.Dispose();
                _device = null;
            }
        }

        private void OnLoadCheck()
        {
            if (_renderTarget == null ||
                _loaded.Count == _renderers.Count)
            {
                return;
            }
            if (_loaded.Count > _renderers.Count)
            {
                _loaded
                    .Except(_renderers)
                    .ToList()
                    .ForEach(renderer =>
                        {
                            _loaded.Remove(renderer);
                            renderer.Unload();
                        });
            }
            _renderers
                .Except(_loaded)
                .ToList()
                .ForEach(renderer =>
                    {
                        _loaded.Add(renderer);
                        renderer.Load();
                    });
        }

        private void OnUnload()
        {
            var renderers = _loaded.ToArray();
            foreach (var renderer in renderers)
            {
                _loaded.Remove(renderer);
                renderer.Unload();
            }
        }

        private void OnAcquireCheck()
        {
            if (_device == null || 
                _renderTarget != null || 
                _swapChain != null)
            {
                return;
            }
            var d3dpp = (PresentParameters)_d3dpp.Clone();
            _size = _window.Size;
            d3dpp.DeviceWindowHandle = _window.Handle;
            d3dpp.BackBufferWidth = _size.Width;
            d3dpp.BackBufferHeight = _size.Height;


            if (_size.Width <= 0 || _size.Height <= 0)
            {
                return;
            }
            try
            {
                _swapChain = new SwapChain(_device, d3dpp);
                _renderTarget = _swapChain.GetBackBuffer(0, BackBufferType.Mono);
                //_renderTarget = _device.GetRenderTarget(0);
            }
            catch
            {
                if (_swapChain != null)
                {
                    _swapChain.Dispose();
                    _swapChain = null;
                }
            }
        }

        private void OnLost()
        {
            OnUnload();
            if (_renderTarget != null)
            {
                _renderTarget.Dispose();
                _renderTarget = null;
            }
            if (_swapChain != null)
            {
                _swapChain.Dispose();
                _swapChain = null;
            }
        }

        private void OnRender()
        {
            _device.SetRenderTarget(0, _renderTarget);
            _device.Clear(ClearFlags.Target, 0, 1.0f, 0);
            _device.BeginScene();
            try
            {
                foreach (var renderLayer in _loaded)
                {
                    renderLayer.Render(_size.Width, _size.Height);
                }
            }
            finally
            {
                _device.EndScene();
            }
        }

        private ResultCode TestCooperativeLevel()
        {
            if (_device == null)
            {
                return ResultCode.NotAvailable;
            }
            int hr;
            _device.CheckCooperativeLevel(out hr);
            return (ResultCode)hr;
        }

        private PresentParameters CreatePresentParams(IntPtr hwnd)
        {
            var format = Format.A8R8G8B8;   // mode.Format;

            var d3dpp = new PresentParameters();
            d3dpp.DeviceWindowHandle = hwnd;
            d3dpp.Windowed = true;
            d3dpp.PresentationInterval = PresentInterval.One;// PresentInterval.One;//vbSync ? PresentInterval.One : PresentInterval.Immediate;//PresentInterval.Default;
            //make sure you are NOT using flipping if you are in windowed mode. 
            //In windowed mode, you share the current video mode of the applications running. 
            //Unfortunately, you have to use the slower blitting process.
            d3dpp.SwapEffect = SwapEffect.Discard;//vbSync ? SwapEffect.Flip : SwapEffect.Discard;//SwapEffect.Discard;
            d3dpp.BackBufferCount = 1;
            d3dpp.BackBufferFormat = format;
            d3dpp.BackBufferWidth = _size.Width > 0 ? _size.Width : 1;
            d3dpp.BackBufferHeight = _size.Height > 0 ? _size.Height : 1;
            d3dpp.EnableAutoDepthStencil = false;
            //d3dpp.MultiSample = MultiSampleType.NonMaskable;
            d3dpp.PresentFlag = PresentFlag.Video; // PresentFlag.DeviceClip == single display mode
            return d3dpp;
        }

        #endregion Private


        #region Helpers

        internal static int GetPotSize(int size)
        {
            // Create POT texture (e.g. 512x512) to render NPOT image (e.g. 320x240),
            // because NPOT textures is not supported on some videocards
            var potSize = 0;
            for (var power = 1; potSize < size; power++)
            {
                potSize = Pow(2, power);
            }
            return potSize;
        }

        private static int Pow(int value, int power)
        {
            var result = value;
            for (var i = 0; i < power; i++)
            {
                result *= value;
            }
            return result;
        }

        #endregion Helpers


        #region SubclassWindow

        private class SubclassWindow : NativeWindow
        {
            private const int WM_WINDOWPOSCHANGING = 0x0046;
            private const int WM_WINDOWPOSCHANGED = 0x0047;
            //private const int WM_MOVING = 0x0216;
            private const int WM_PAINT = 0x000F;
            private const int WM_SIZE = 0x0005;
            private const int MONITOR_DEFAULTTONEAREST = 2;
            private const int GA_ROOT = 2;

            private readonly AllocatorPresenter _allocator;
            private readonly SubclassWindowRoot _windowRoot;

            public SubclassWindow(AllocatorPresenter allocator)
            {
                _allocator = allocator;
                _windowRoot = new SubclassWindowRoot(this);
            }

            public IntPtr MonitorId { get; private set; }
            public int AdapterId { get; private set; }
            public string AdapterName { get; private set; }
            public string AdapterDriver { get; private set; }

            protected override void OnHandleChange()
            {
                base.OnHandleChange();
                if (Handle != IntPtr.Zero)
                {
                    var root = NativeMethods.GetAncestor(Handle, GA_ROOT);
                    _windowRoot.AssignHandle(root);
                }
                else
                {
                    _windowRoot.ReleaseHandle();
                    AdapterId = 0;
                    MonitorId = IntPtr.Zero;
                    return;
                }
                CheckAdapter();
            }

            public AdapterInformation GetAdapterId(IntPtr hwnd)
            {
                var screen = System.Windows.Forms.Screen.FromHandle(hwnd);
                if (screen == null)
                {
                    Logger.Warn("SubclassWindow.GetAdapterId: Screen.FromHandle({0}) == null", Handle);
                    return Manager.Adapters.Default;
                }
                var adapterInfo = Manager.Adapters
                    .OfType<AdapterInformation>()
                    .FirstOrDefault(ai => ai.Information.DeviceName == screen.DeviceName);
                if (adapterInfo == null)
                {
                    // happens when display just connected on the fly
                    // so it is missing from Manager.Adapters
                    Logger.Warn("SubclassWindow.GetAdapterId: adapter not found!");
                    return Manager.Adapters.Default;
                }
                adapterInfo = adapterInfo ?? Manager.Adapters.Default;
                return adapterInfo;
            }

            private void CheckAdapter()
            {
                var hMonitor = NativeMethods.MonitorFromWindow(Handle, MONITOR_DEFAULTTONEAREST);
                if (MonitorId == hMonitor)
                {
                    return;
                }
                MonitorId = hMonitor;
                Logger.Debug("MonitorFromWindow: 0x{0}", MonitorId.ToString("X"));

                var adapter = GetAdapterId(Handle);
                if (adapter == null)
                {
                    AdapterId = 0;
                    Logger.Debug("AdapterId: {0}", AdapterId);
                }
                else
                {
                    AdapterId = adapter.Adapter;
                    Logger.Debug(
                        "AdapterId: {0} [\"{1}\", \"{2}\"]", 
                        AdapterId, 
                        adapter.Information.DeviceName,
                        adapter.Information.Description);
                }
            }

            public System.Drawing.Size Size
            {
                get { return NativeMethods.GetWindowRect(Handle).Size; }
            }

            public bool Visible
            {
                get { return NativeMethods.IsWindowVisible(Handle); }
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_WINDOWPOSCHANGING:
                        lock (_allocator._syncRoot)
                        {
                            base.WndProc(ref m);
                        }
                        CheckAdapter();
                        return;
                    //case WM_PAINT:
                    //    _allocator.Render();
                    //    NativeMethods.InvalidateRect(Handle, IntPtr.Zero, false);
                    //    return;
                }
                base.WndProc(ref m);
            }

            private class SubclassWindowRoot : NativeWindow
            {
                private readonly SubclassWindow _child;

                public SubclassWindowRoot(SubclassWindow child)
                {
                    _child = child;
                }

                protected override void WndProc(ref Message m)
                {
                    switch (m.Msg)
                    {
                        case WM_WINDOWPOSCHANGED:
                            _child.CheckAdapter();
                            break;
                    }
                    base.WndProc(ref m);
                }
            }
        }

        #endregion SubclassWindow
    }
}
