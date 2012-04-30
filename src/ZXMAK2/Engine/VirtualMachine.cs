using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;

using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.MDX;

namespace ZXMAK2.Engine
{
    public class VirtualMachine : IDebuggable
    {
        private readonly object m_sync = new object();
        private Thread m_thread = null;

        private int[] m_blankScreen = new int[320 * 240];
        public Size ScreenSize 
        { 
            get 
            { 
                IUlaDevice ula = m_spectrum.BusManager.FindDevice(typeof(IUlaDevice)) as IUlaDevice;
                if(ula!=null)
                    return ula.VideoSize;
                return new Size(320, 240);
            } 
        }

        public float ScreenHeightScale
        {
            get
            {
                IUlaDevice ula = m_spectrum.BusManager.FindDevice(typeof(IUlaDevice)) as IUlaDevice;
                if (ula != null)
                    return ula.VideoHeightScale;
                return 1F;
            }
        }
        
        public int[] Screen 
        { 
            get 
            {
                IUlaDevice ula = m_spectrum.BusManager.FindDevice(typeof(IUlaDevice)) as IUlaDevice;
                if (ula != null)
                    return ula.VideoBuffer;
                return m_blankScreen;
            } 
        }

        public event EventHandler UpdateVideo;

        public unsafe VirtualMachine(DirectKeyboard keyboard, DirectMouse mouse, DirectSound sound)
        {
            this.m_keyboard = keyboard;
            this.m_mouse = mouse;
            this.m_sound = sound;
            m_spectrum = new SpectrumConcrete();
            m_spectrum.Init();
            m_spectrum.UpdateState += OnUpdateState;
            m_spectrum.Breakpoint += OnBreakpoint;
			m_spectrum.UpdateFrame += OnUpdateFrame;
			m_spectrum.DoReset();
            m_spectrum.BusManager.SetDebuggable(this);
        }

        private string m_name = "ZX Spectrum Clone";
        private string m_description = "N/A";
        
        public void Load(XmlNode parent)
        {
            
            XmlNode infoNode = parent.SelectSingleNode("Info");
            XmlNode busNode = parent.SelectSingleNode("Bus");
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
                    m_name = infoNode.Attributes["name"].InnerText;
                if (infoNode.Attributes["description"] != null)
                    m_description = infoNode.Attributes["description"].InnerText;
            }
            m_spectrum.Load(busNode);
        }

        public void Save(XmlNode parent)
        {
            XmlElement xeInfo = parent.OwnerDocument.CreateElement("Info");
            if(m_name!="ZX Spectrum Clone")
                xeInfo.SetAttribute("name", m_name);
            if (m_description != "N/A")
                xeInfo.SetAttribute("description", m_description);
            parent.AppendChild(xeInfo);
            XmlElement xeBus = parent.OwnerDocument.CreateElement("Bus");
            XmlNode busNode = parent.AppendChild(xeBus);
            m_spectrum.Save(busNode);
        }

        #region Open/Save Config

        private string m_configFileName = string.Empty;

        public void OpenConfig(string fileName)
        {
            m_configFileName = Path.GetFullPath(fileName);
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                OpenConfig(stream);
        }

        public void SaveConfig()
        {
            if(!string.IsNullOrEmpty(m_configFileName))
                using (Stream stream = new FileStream(m_configFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    SaveConfig(stream);
        }

        public void SaveConfigAs(string fileName)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                SaveConfig(stream);
            m_configFileName = fileName;
        }

        public void OpenConfig(Stream stream)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(stream);
            XmlNode root = xml.SelectSingleNode("/VirtualMachine");
            if (root==null)
            {
                LogAgent.Error("Invalid Machine Configuration File");
                throw new ArgumentException("Invalid Machine Configuration File");
            }
            Load(root);
        }

        public void SaveConfig(Stream stream)
        {
            XmlDocument xml = new XmlDocument();
            XmlNode root = xml.AppendChild(xml.CreateElement("VirtualMachine"));
            Save(root);

            xml.Save(stream);
        }

        #endregion

        private void OnUpdateVideo()
		{
			if (UpdateVideo != null)
				UpdateVideo(this, EventArgs.Empty);
		}

        private void OnUpdateFrame(object sender, EventArgs e)
        {
			byte[] sndbuf = m_sound.LockBuffer();
			while (m_spectrum.IsRunning && sndbuf == null)
			{
				Thread.Sleep(1);
				sndbuf = m_sound.LockBuffer();
			}
			if (sndbuf != null)
			{
				try
				{
					mixAudio(sndbuf);
				}
				finally
				{
					m_sound.UnlockBuffer(sndbuf);
				}
			}
			else
			{
				Thread.Sleep(1);
			}
			OnUpdateVideo();
        }

        /// <summary>
        /// Debugger Update State
        /// </summary>
        private void OnUpdateState(object sender, EventArgs e)
        {
            if (UpdateState != null)
                UpdateState(this, EventArgs.Empty);
            IUlaDevice ula = m_spectrum.BusManager.FindDevice(typeof(IUlaDevice)) as IUlaDevice;
            if (ula != null)
                ula.Flush();
            OnUpdateVideo();
        }

        private void OnBreakpoint(object sender, EventArgs e)
        {
            if (Breakpoint != null)
                Breakpoint(this, EventArgs.Empty);
        }
        
        
        #region spectrum

        public SpectrumBase Spectrum { get { return m_spectrum; } }

        private SpectrumBase m_spectrum;
        private DirectKeyboard m_keyboard;
        private DirectMouse m_mouse;
        private unsafe DirectSound m_sound;

        //[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        //private static extern short GetAsyncKeyState(int vkey);
        //private static int VK_F4 = 0x73;

        private int m_debugFrameStartTact = 0;

        public int DebugFrameStartTact { get { return m_debugFrameStartTact; } }

        private unsafe void runThreadProc()
        {
            m_spectrum.IsRunning = true;

            List<BusDeviceBase> keyboards = m_spectrum.BusManager.FindDevices(typeof(IKeyboardDevice));
            List<BusDeviceBase> mouses = m_spectrum.BusManager.FindDevices(typeof(IMouseDevice));
            
            while (m_spectrum.IsRunning)
            {
                //if ((GetAsyncKeyState(VK_F4) & 1) != 0)
                //{
                //    Pentagon128 p128 = Spectrum as Pentagon128;
                //    for(int i=0; i < 50*60; i++)
                //        p128.ExecuteFrameFast();
                //    long frame = p128.CPU.Tact/(50*71680);
                //    LogAgent.Info(frame.ToString());
                //    OnUpdateFrame(); 
                //    continue;
                //}

                if (keyboards.Count>0)
                {
                    m_keyboard.Scan();
					foreach (BusDeviceBase dev in keyboards)
					{
						IKeyboardDevice keyboard = dev as IKeyboardDevice;
						keyboard.KeyboardState = m_keyboard.State;
					}
                }

                if (mouses.Count > 0)
                {
                    m_mouse.Scan();
                    foreach (BusDeviceBase dev in mouses)
                    {
                        IMouseDevice mouse = dev as IMouseDevice;
						mouse.MouseState = m_mouse.MouseState;
                    }
                }

                //lock(m_sync)
					m_spectrum.ExecuteFrame();
                m_debugFrameStartTact = (int)(m_spectrum.CPU.Tact % Spectrum.BusManager.FrameTactCount);
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

            List<BusDeviceBase> renderers = m_spectrum.BusManager.FindDevices(typeof(ISoundRenderer));
            List<uint[]> buffers = new List<uint[]>();
            foreach (BusDeviceBase device in renderers)
            {
                ISoundRenderer renderer = device as ISoundRenderer;
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

        public void DoReset()
        {
            lock (m_sync)
            {
                bool run = IsRunning;
                DoStop();
                Spectrum.DoReset();
                if (run)
                    DoRun();
            }
            OnUpdateVideo();
        }

        public void DoNmi()
        {
            lock (m_sync)
            {
                bool run = IsRunning;
                DoStop();
                Spectrum.DoNmi();
                if (run)
                    DoRun();
            }
            OnUpdateVideo();
        }

        public void DoStepInto()
        {
            lock(m_sync)
                Spectrum.DoStepInto();
            OnUpdateVideo();
        }

        public void DoStepOver()
        {
            lock (m_sync)
                Spectrum.DoStepOver();
            OnUpdateVideo();
        }

        public void DoRun()
        {
            lock (m_sync)
            {
                if (IsRunning)
                    return;
				m_thread = null;
                m_thread = new Thread(new ThreadStart(runThreadProc));
                m_thread.Name = "VirtualMachine.runThreadProc";
                m_thread.Priority = ThreadPriority.AboveNormal;
                m_thread.Start();
                while (!IsRunning)
                    Thread.Sleep(1);
            }
			OnUpdateVideo();
		}

        public void DoStop()
        {
			Thread thread = null;
			lock (m_sync)
            {
				if (!IsRunning)
					return;
				if (m_thread == null)
                    return;
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
            byte[] data = new byte[1];
            ReadMemory(addr, data, 0, 1);
            return data[0];
        }

        public void WriteMemory(ushort addr, byte value)
        {
            byte[] data = new byte[1];
            data[0] = value;
            WriteMemory(addr, data, 0, 1);
        }

        public void ReadMemory(ushort addr, byte[] data, int offset, int length)
        {
            lock (m_sync)
            {
                IMemoryDevice memory = Spectrum.BusManager.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
                ushort ptr = addr;
                for (int i = 0; i < length; i++, ptr++)
                    data[offset + i] = memory.RDMEM_DBG(ptr);
            }
        }

        public void WriteMemory(ushort addr, byte[] data, int offset, int length)
        {
            lock (m_sync)
            {
                IMemoryDevice memory = Spectrum.BusManager.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
                ushort ptr = addr;
                for (int i = 0; i < length; i++, ptr++)
                    memory.WRMEM_DBG(ptr, data[offset + i]);
            }
            OnUpdateVideo();
        }

        public void AddBreakpoint(ushort addr)
        {
            lock (m_sync)
                Spectrum.AddBreakpoint(addr);
        }

        public void RemoveBreakpoint(ushort addr)
        {
            lock (m_sync)
                Spectrum.RemoveBreakpoint(addr);
        }

        public ushort[] GetBreakpointList()
        {
            lock (m_sync)
                return Spectrum.GetBreakpointList();
        }

        public bool CheckBreakpoint(ushort addr)
        {
            lock (m_sync)
                return Spectrum.CheckBreakpoint(addr);
        }

        public void ClearBreakpoints()
        {
            lock (m_sync)
                Spectrum.ClearBreakpoints();
        }

        public event EventHandler UpdateState;
        public event EventHandler Breakpoint;
        
        public bool IsRunning
        {
            get 
            {
                lock (m_sync)
                    return Spectrum.IsRunning;
            }
        }

        public Engine.Z80.Z80CPU CPU
        {
            get 
            {
                lock (m_sync)
                    return Spectrum.CPU;
            }
        }

        public int GetFrameTact()
        {
            lock (m_sync)
                return Spectrum.BusManager.GetFrameTact();
        }

		public int FrameTactCount
		{
			get
			{
				lock (m_sync)
					return Spectrum.BusManager.FrameTactCount;
			}
		}

        #endregion
    }
}
