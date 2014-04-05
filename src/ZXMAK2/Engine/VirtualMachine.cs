using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;

using ZXMAK2.Engine;
using ZXMAK2.Interfaces;
using ZXMAK2.MDX;
using ZXMAK2.Controls.Debugger;
using ZXMAK2.Entities;

namespace ZXMAK2.Engine
{
    public class VirtualMachine : IDebuggable, IDisposable
    {
        private readonly object m_sync = new object();
        private Thread m_thread = null;

        private int[] m_blankScreen = new int[320 * 240];
        public Size ScreenSize
        {
            get
            {
                var ula = m_spectrum.BusManager.FindDevice<IUlaDevice>();
                return ula != null ? ula.VideoSize : new Size(320, 240);
            }
        }

        public float ScreenHeightScale
        {
            get
            {
                var ula = m_spectrum.BusManager.FindDevice<IUlaDevice>();
                return ula != null ? ula.VideoHeightScale : 1F;
            }
        }

        public int[] Screen
        {
            get
            {
                var ula = m_spectrum.BusManager.FindDevice<IUlaDevice>();
                return ula != null ? ula.VideoBuffer : m_blankScreen;
            }
        }

        public unsafe VirtualMachine(IHost host)
        {
            m_host = host;
            m_spectrum = new SpectrumConcrete();
            m_spectrum.UpdateState += OnUpdateState;
            m_spectrum.Breakpoint += OnBreakpoint;
            m_spectrum.UpdateFrame += OnUpdateFrame;
            m_spectrum.BusManager.HostUi = host.HostUi;
            m_spectrum.BusManager.ConfigChanged += BusManager_OnConfigChanged;
        }

        public void Init()
        {
            m_spectrum.Init();
            m_spectrum.DoReset();
            m_spectrum.BusManager.SetDebuggable(this);
        }

        public void Dispose()
        {
            DoStop();
            Spectrum.BusManager.Disconnect();
        }

        private string m_name = "ZX Spectrum Clone";
        private string m_description = "N/A";
        private bool m_isConfigUpdate;

        public void LoadConfigXml(XmlNode parent)
        {
            var infoNode = parent.SelectSingleNode("Info");
            var busNode = parent.SelectSingleNode("Bus");
            if (busNode == null)
            {
                LogAgent.Error("Machine bus configuration not found!");
                throw new ArgumentException("Machine bus configuration not found!");
            }

            m_name = "ZX Spectrum Clone";
            m_description = "N/A";
            if (infoNode != null)
            {
                if (infoNode.Attributes["name"] != null)
                {
                    m_name = infoNode.Attributes["name"].InnerText;
                }
                if (infoNode.Attributes["description"] != null)
                {
                    m_description = infoNode.Attributes["description"].InnerText;
                }
            }
            m_isConfigUpdate = true;
            try
            {
                m_spectrum.BusManager.LoadConfigXml(busNode);
            }
            finally
            {
                m_isConfigUpdate = false;
            }
            DoReset();
        }

        public void SaveConfigXml(XmlNode parent)
        {
            var xeInfo = (XmlElement)parent.OwnerDocument.CreateElement("Info");
            if (m_name != "ZX Spectrum Clone")
            {
                xeInfo.SetAttribute("name", m_name);
            }
            if (m_description != "N/A")
            {
                xeInfo.SetAttribute("description", m_description);
            }
            parent.AppendChild(xeInfo);
            var xeBus = parent.OwnerDocument.CreateElement("Bus");
            var busNode = parent.AppendChild(xeBus);
            m_isConfigUpdate = true;
            try
            {
                m_spectrum.BusManager.SaveConfigXml(busNode);
            }
            finally
            {
                m_isConfigUpdate = false;
            }
        }

        private void BusManager_OnConfigChanged(object sender, EventArgs e)
        {
            if (!m_isConfigUpdate)
            {
                SaveConfig();
            }
        }

        
        #region Open/Save Config

        private string m_configFileName = string.Empty;

        public void OpenConfig(string fileName)
        {
            fileName = Path.GetFullPath(fileName);
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                m_configFileName = fileName;
                m_spectrum.BusManager.MachineFile = m_configFileName;
                OpenConfig(stream);
            }
        }

        public void SaveConfig()
        {
            if (!string.IsNullOrEmpty(m_configFileName))
            {
                using (var stream = new FileStream(m_configFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    SaveConfig(stream);
                }
            }
        }

        public void SaveConfigAs(string fileName)
        {
            fileName = Path.GetFullPath(fileName);
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                m_configFileName = fileName;
                m_spectrum.BusManager.MachineFile = m_configFileName;
                SaveConfig(stream);
            }
        }

        public void OpenConfig(Stream stream)
        {
            var xml = new XmlDocument();
            xml.Load(stream);
            var root = xml.SelectSingleNode("/VirtualMachine");
            if (root == null)
            {
                LogAgent.Error("Invalid Machine Configuration File");
                throw new ArgumentException("Invalid Machine Configuration File");
            }
            LoadConfigXml(root);
        }

        public void SaveConfig(Stream stream)
        {
            var xml = new XmlDocument();
            var root = xml.AppendChild(xml.CreateElement("VirtualMachine"));
            SaveConfigXml(root);
            xml.Save(stream);
        }

        #endregion

        private void OnUpdateVideo()
        {
            if (m_host == null || m_host.Video == null)
            {
                return;
            }
            m_host.Video.UpdateVideo(this);
        }

        private void OnUpdateFrame(object sender, EventArgs e)
        {
            if (m_host == null || m_host.Sound == null)
            {
                return;
            }
            if (!MaxSpeed)
            {
                var sndbuf = m_host.Sound.LockBuffer();
                while (m_spectrum.IsRunning && sndbuf == null)
                {
                    Thread.Sleep(1);
                    sndbuf = m_host.Sound.LockBuffer();
                }
                if (sndbuf != null)
                {
                    try
                    {
                        mixAudio(sndbuf);
                    }
                    finally
                    {
                        m_host.Sound.UnlockBuffer(sndbuf);
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
            OnUpdateVideo();
        }

        /// <summary>
        /// Debugger Update State
        /// </summary>
        private void OnUpdateState(object sender, EventArgs e)
        {
            m_spectrum.BusManager.IconPause.Visible = !m_spectrum.IsRunning;
            var handler = UpdateState;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
            var ula = m_spectrum.BusManager.FindDevice<IUlaDevice>();
            if (ula != null)
            {
                ula.Flush();
            }
            OnUpdateVideo();
        }

        private void OnBreakpoint(object sender, EventArgs e)
        {
            m_bpTriggered = true;
            var handler = Breakpoint;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }


        #region spectrum

        public SpectrumBase Spectrum { get { return m_spectrum; } }

        private SpectrumBase m_spectrum;
        private IHost m_host;

        public int DebugFrameStartTact { get { return Spectrum.FrameStartTact; } }
        public bool MaxSpeed = false;

        private unsafe void runThreadProc()
        {
            try
            {
                m_spectrum.IsRunning = true;

                using (var input = new InputAggregator(
                    m_host,
                    m_spectrum.BusManager.FindDevices<IKeyboardDevice>().ToArray(),
                    m_spectrum.BusManager.FindDevices<IMouseDevice>().ToArray(),
                    m_spectrum.BusManager.FindDevices<IJoystickDevice>().ToArray()))
                {
                    while (m_spectrum.IsRunning)
                    {
                        input.Scan();
                        m_spectrum.ExecuteFrame();
                    }
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        private unsafe void mixAudio(byte[] sndbuf)
        {
            if (sndbuf == null)
                return;

            int len = 44100 / 50;//50 fps

            if (!m_spectrum.IsRunning)
            {
                fixed (byte* soundPtr = sndbuf)
                    for (int i = 0; i < len * 4; i++)
                        soundPtr[i] = 0;
                return;
            }

            var renderers = m_spectrum.BusManager.FindDevices<ISoundRenderer>();
            var buffers = new List<uint[]>();
            foreach (var renderer in renderers)
            {
                buffers.Add(renderer.AudioBuffer);
            }
            mixBuffers(sndbuf, buffers.ToArray());
        }

        private unsafe void mixBuffers(byte[] dst, uint[][] bufferArray)
        {
            fixed (byte* bptr = dst)
            {
                uint* uiptr = (uint*)bptr;

                for (int i = 0; i < dst.Length / 4; i++)    // clean buffer
                {
                    uint value1 = 0;
                    uint value2 = 0;
                    if (bufferArray.Length > 0)
                    {
                        for (int j = 0; j < bufferArray.Length; j++)
                        {
                            value1 += bufferArray[j][i] >> 16;
                            value2 += bufferArray[j][i] & 0xFFFF;
                        }
                        value1 /= (uint)bufferArray.Length;
                        value2 /= (uint)bufferArray.Length;
                    }
                    uiptr[i] = (value1 << 16) | value2;
                }

                //for (int i = 0; i < dst.Length / 4; i++)    // clean buffer
                //    uiptr[i] = 0;
                //foreach (uint[] buffer in bufferArray)       // mix sound sources
                //    fixed (uint* uibuffer = buffer)
                //        for (int i = 0; i < dst.Length/4; i++)
                //        {
                //            uint s1 = uiptr[i];
                //            uint s2 = uibuffer[i];
                //            uiptr[i] = ((((s1 >> 16) + (s2 >> 16)) / 2) << 16) | (((s1 & 0xFFFF) + (s2 & 0xFFFF)) / 2);
                //        }
            }
        }

        #endregion

        #region IDebuggable

        private bool m_bpTriggered;

        public void DoReset()
        {
            lock (m_sync)
            {
                var run = IsRunning;
                DoStop();
                m_bpTriggered = false;
                Spectrum.DoReset();
                if (run && !m_bpTriggered)
                {
                    DoRun();
                }
            }
            OnUpdateVideo();
        }

        public void DoNmi()
        {
            lock (m_sync)
            {
                var run = IsRunning;
                DoStop();
                m_bpTriggered = false;
                Spectrum.DoNmi();
                if (run && !m_bpTriggered)
                {
                    DoRun();
                }
            }
            OnUpdateVideo();
        }

        public void DoStepInto()
        {
            lock (m_sync)
            {
                Spectrum.DoStepInto();
            }
            OnUpdateVideo();
        }

        public void DoStepOver()
        {
            lock (m_sync)
            {
                Spectrum.DoStepOver();
            }
            OnUpdateVideo();
        }

        public void DoRun()
        {
            lock (m_sync)
            {
                if (IsRunning)
                {
                    return;
                }
                m_thread = null;
                m_thread = new Thread(new ThreadStart(runThreadProc));
                m_thread.Name = "VirtualMachine.runThreadProc";
                m_thread.Priority = ThreadPriority.AboveNormal;
                m_thread.Start();
                while (!IsRunning)
                {
                    Thread.Sleep(1);
                }
            }
            OnUpdateVideo();
        }

        public void DoStop()
        {
            Thread thread = null;
            lock (m_sync)
            {
                if (!IsRunning || m_thread == null)
                {
                    return;
                }
                Spectrum.IsRunning = false;
                thread = m_thread;
                m_thread = null;
            }
            thread.Join();
            thread = null;
            OnUpdateVideo();
        }

        public byte ReadMemory(ushort addr)
        {
            var data = new byte[1];
            ReadMemory(addr, data, 0, 1);
            return data[0];
        }

        public void WriteMemory(ushort addr, byte value)
        {
            var data = new byte[1];
            data[0] = value;
            WriteMemory(addr, data, 0, 1);
        }

        public void ReadMemory(ushort addr, byte[] data, int offset, int length)
        {
            lock (m_sync)
            {
                var memory = Spectrum.BusManager.FindDevice<IMemoryDevice>();
                ushort ptr = addr;
                for (int i = 0; i < length; i++, ptr++)
                {
                    data[offset + i] = memory.RDMEM_DBG(ptr);
                }
            }
        }

        public void WriteMemory(ushort addr, byte[] data, int offset, int length)
        {
            lock (m_sync)
            {
                var memory = Spectrum.BusManager.FindDevice<IMemoryDevice>();
                ushort ptr = addr;
                for (int i = 0; i < length; i++, ptr++)
                {
                    memory.WRMEM_DBG(ptr, data[offset + i]);
                }
            }
            OnUpdateVideo();
        }

        public void AddBreakpoint(Breakpoint bp)
        {
            lock (m_sync)
            {
                Spectrum.AddBreakpoint(bp);
            }
        }

        public void RemoveBreakpoint(Breakpoint bp)
        {
            lock (m_sync)
            {
                Spectrum.RemoveBreakpoint(bp);
            }
        }

        public Breakpoint[] GetBreakpointList()
        {
            lock (m_sync)
            {
                return Spectrum.GetBreakpointList();
            }
        }

        public void ClearBreakpoints()
        {
            lock (m_sync)
            {
                Spectrum.ClearBreakpoints();
            }
        }

        public event EventHandler UpdateState;
        public event EventHandler Breakpoint;

        public bool IsRunning
        {
            get
            {
                lock (m_sync)
                {
                    return Spectrum.IsRunning;
                }
            }
        }

        public Engine.Z80.Z80CPU CPU
        {
            get
            {
                lock (m_sync)
                {
                    return Spectrum.CPU;
                }
            }
        }

        public int GetFrameTact()
        {
            lock (m_sync)
            {
                return Spectrum.BusManager.GetFrameTact();
            }
        }

        public int FrameTactCount
        {
            get
            {
                lock (m_sync)
                {
                    return Spectrum.BusManager.FrameTactCount;
                }
            }
        }

        public IRzxState RzxState
        {
            get
            {
                lock (m_sync)
                {
                    return Spectrum.BusManager.RzxHandler;
                }
            }
        }

        #endregion
    }
}
