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
        #region Fields

        private readonly object m_sync = new object();
        private Thread m_thread = null;
        private IVideoData m_blankData = new VideoData(320, 240, 1F);

        private string m_name = "ZX Spectrum Clone";
        private string m_description = "N/A";
        private bool m_isConfigUpdate;
        private string m_configFileName = string.Empty;


        private IHost m_host;

        public SpectrumBase Spectrum { get; private set; }
        
        public int DebugFrameStartTact 
        { 
            get { return Spectrum.FrameStartTact; } 
        }
        
        public bool MaxSpeed = false;

        #endregion Fields


        public IVideoData VideoData
        {
            get
            {
                var ula = Spectrum.BusManager.FindDevice<IUlaDevice>();
                return ula != null && ula.VideoData != null ? ula.VideoData : m_blankData;
            }
        }


        #region .ctor

        public unsafe VirtualMachine(IHost host)
        {
            m_host = host;
            Spectrum = new SpectrumConcrete();
            Spectrum.UpdateState += OnUpdateState;
            Spectrum.Breakpoint += OnBreakpoint;
            Spectrum.UpdateFrame += OnUpdateFrame;
            Spectrum.BusManager.HostUi = host.HostUi;
            Spectrum.BusManager.ConfigChanged += BusManager_OnConfigChanged;
        }

        public void Dispose()
        {
            DoStop();
            Spectrum.BusManager.Disconnect();
        }

        #endregion .ctor


        #region Config

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
                Spectrum.BusManager.LoadConfigXml(busNode);
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
                Spectrum.BusManager.SaveConfigXml(busNode);
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

        public void OpenConfig(string fileName)
        {
            fileName = Path.GetFullPath(fileName);
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                m_configFileName = fileName;
                Spectrum.BusManager.MachineFile = m_configFileName;
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
                Spectrum.BusManager.MachineFile = m_configFileName;
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

        #endregion Config

        private uint[][] m_soundBuffers;

        private void OnUpdateSound()
        {
            var host = m_host;
            var sound = host != null ? host.Sound : null;
            if (sound == null)
            {
                return;
            }
            sound.PushFrame(m_soundBuffers);
        }

        private void OnUpdateVideo()
        {
            var host = m_host;
            var video = host != null ? host.Video : null;
            if (video == null)
            {
                return;
            }
            m_host.Video.PushFrame(this);
        }

        private void OnUpdateFrame(object sender, EventArgs e)
        {
            if (m_host == null || m_host.Sound == null)
            {
                return;
            }
            OnUpdateSound();
            OnUpdateVideo();
        }

        /// <summary>
        /// Debugger Update State
        /// </summary>
        private void OnUpdateState(object sender, EventArgs e)
        {
            Spectrum.BusManager.IconPause.Visible = !Spectrum.IsRunning;
            var handler = UpdateState;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
            var ula = Spectrum.BusManager.FindDevice<IUlaDevice>();
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

        public void Init()
        {
            Spectrum.Init();
            Spectrum.DoReset();
            Spectrum.BusManager.SetDebuggable(this);
        }

        private unsafe void runThreadProc()
        {
            try
            {
                Spectrum.IsRunning = true;

                var bus = Spectrum.BusManager;
                var host = m_host;
                var sound = host != null ? host.Sound : null;
                var video = host != null ? host.Video : null;

                using (var input = new InputAggregator(
                    host,
                    bus.FindDevices<IKeyboardDevice>().ToArray(),
                    bus.FindDevices<IMouseDevice>().ToArray(),
                    bus.FindDevices<IJoystickDevice>().ToArray()))
                {
                    var list = new List<uint[]>();
                    foreach (var renderer in Spectrum.BusManager.FindDevices<ISoundRenderer>())
                    {
                        list.Add(renderer.AudioBuffer);
                    }
                    m_soundBuffers = list.ToArray();

                    // main emulation loop
                    while (Spectrum.IsRunning)
                    {
                        input.Scan();
                        Spectrum.ExecuteFrame();
                        
                        // frame sync
                        if (sound != null && !MaxSpeed)
                        {
                            sound.WaitFrame();
                        }
                    }

                    m_soundBuffers = null;
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
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
            lock (m_sync)
            {
                if (!IsRunning || m_thread == null)
                {
                    return;
                }
                Spectrum.IsRunning = false;
                var host = m_host;
                var sound = host != null ? host.Sound : null;
                if (sound != null)
                {
                    sound.CancelWait();
                }
                var video = host != null ? host.Video : null;
                if (video != null)
                {
                    video.CancelWait();
                }
                var thread = m_thread;
                m_thread = null;
                thread.Join();
            }
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
