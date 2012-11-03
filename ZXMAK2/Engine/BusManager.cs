﻿using System;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine.Serializers;
using System.IO;

namespace ZXMAK2.Engine.Bus
{
	public class BusManager : IBusManager
	{
		private bool m_connected = false;
		private bool m_sandBox = false;
		private String m_machineFile = null;
		private Z80CPU m_cpu;
		private LoadManager m_loadManager;
		private IUlaDevice m_ula;
		private IDebuggable m_debuggable;
		private List<BusDeviceBase> m_deviceList = new List<BusDeviceBase>();
		private BusReadProc[] m_mapReadMemoryM1;
		private BusReadProc[] m_mapReadMemory;
		private BusReadIoProc[] m_mapReadPort;
		private BusWriteProc[] m_mapWriteMemory;
		private BusWriteIoProc[] m_mapWritePort;
		private BusNoMreqProc[] m_mapReadNoMreq;
		private BusNoMreqProc[] m_mapWriteNoMreq;
		private BusCycleProc m_preCycle;
		private BusSignalProc m_reset;
		private BusSignalProc m_nmiAck;
		private BusSignalProc m_intAck;
		private BusFrameEventHandler m_beginFrame;
		private BusFrameEventHandler m_endFrame;
		private IconDescriptor[] m_iconDescList = new IconDescriptor[0];
		private IconDescriptor m_iconPause = new IconDescriptor("PAUSE", Utils.GetIconStream("pause_vm.png"));

		public event BusFrameEventHandler FrameReady;
		public event EventHandler BusConnected;
		public event EventHandler BusDisconnect;

		public RzxHandler RzxHandler { get; set; }
		public IconDescriptor[] IconDescriptorArray { get { return m_iconDescList; } }
		public IconDescriptor IconPause { get { return m_iconPause; } }

		public void Init(Z80CPU cpu, LoadManager loadManager, bool sandBox)
		{
			m_loadManager = loadManager;
			m_sandBox = sandBox;
			if (m_loadManager != null)
			{
				m_loadManager.Clear();
				m_loadManager.AddStandardSerializers();
			}
			m_iconDescList = new IconDescriptor[0];
			if (m_cpu != null)
			{
				m_cpu.RDMEM_M1 = null;
				m_cpu.INTACK_M1 = null;
				m_cpu.NMIACK_M1 = null;
				m_cpu.RDMEM = null;
				m_cpu.WRMEM = null;
				m_cpu.RDPORT = null;
				m_cpu.WRPORT = null;
				m_cpu.RDNOMREQ = null;
				m_cpu.WRNOMREQ = null;
				//m_cpu.OnCycle = null;
				m_cpu.RESET = null;
				m_cpu = null;
			}
			m_cpu = cpu;
			m_cpu.RDMEM_M1 = RDMEM_M1;
			m_cpu.INTACK_M1 = INTACK_M1;
			m_cpu.NMIACK_M1 = NMIACK_M1;
			m_cpu.RDMEM = RDMEM;
			m_cpu.WRMEM = WRMEM;
			m_cpu.RDPORT = RDPORT;
			m_cpu.WRPORT = WRPORT;
			m_cpu.RDNOMREQ = RDNOMREQ;
			m_cpu.WRNOMREQ = WRNOMREQ;
			//m_cpu.OnCycle = OnCpuCycle;
			m_cpu.RESET = RESET;
			RzxHandler = new RzxHandler(m_cpu, this);

			m_deviceList.Clear();
			m_mapReadMemoryM1 = null;
			m_mapReadMemory = null;
			m_mapReadPort = null;
			m_mapWriteMemory = null;
			m_mapWritePort = null;
			m_reset = null;
			m_nmiAck = null;
			m_intAck = null;
			m_preCycle = null;
			m_beginFrame = null;
			m_endFrame = null;
		}


		#region IBusManager

		void IBusManager.SubscribeRDMEM_M1(int addrMask, int maskedValue, BusReadProc proc)
		{
			for (int addr = 0; addr < 0x10000; addr++)
				if ((addr & addrMask) == maskedValue)
					m_mapReadMemoryM1[addr] += proc;
		}

		void IBusManager.SubscribeRDMEM(int addrMask, int maskedValue, BusReadProc proc)
		{
			for (int addr = 0; addr < 0x10000; addr++)
				if ((addr & addrMask) == maskedValue)
					m_mapReadMemory[addr] += proc;
		}

		void IBusManager.SubscribeWRMEM(int addrMask, int maskedValue, BusWriteProc proc)
		{
			for (int addr = 0; addr < 0x10000; addr++)
				if ((addr & addrMask) == maskedValue)
					m_mapWriteMemory[addr] += proc;
		}

		void IBusManager.SubscribeRDIO(int addrMask, int maskedValue, BusReadIoProc proc)
		{
			for (int addr = 0; addr < 0x10000; addr++)
				if ((addr & addrMask) == maskedValue)
					m_mapReadPort[addr] += proc;
		}

		void IBusManager.SubscribeWRIO(int addrMask, int maskedValue, BusWriteIoProc proc)
		{
			for (int addr = 0; addr < 0x10000; addr++)
				if ((addr & addrMask) == maskedValue)
					m_mapWritePort[addr] += proc;
		}

		void IBusManager.SubscribeRDNOMREQ(int addrMask, int maskedValue, BusNoMreqProc proc)
		{
			for (int addr = 0; addr < 0x10000; addr++)
				if ((addr & addrMask) == maskedValue)
					m_mapReadNoMreq[addr] += proc;
		}

		void IBusManager.SubscribeWRNOMREQ(int addrMask, int maskedValue, BusNoMreqProc proc)
		{
			for (int addr = 0; addr < 0x10000; addr++)
				if ((addr & addrMask) == maskedValue)
					m_mapWriteNoMreq[addr] += proc;
		}

		void IBusManager.SubscribePreCycle(BusCycleProc proc)
		{
			m_preCycle += proc;
		}

		void IBusManager.SubscribeRESET(BusSignalProc proc)
		{
			m_reset += proc;
		}

		void IBusManager.SubscribeNMIACK(BusSignalProc proc)
		{
			m_nmiAck += proc;
		}

		void IBusManager.SubscribeINTACK(BusSignalProc proc)
		{
			m_intAck += proc;
		}

		void IBusManager.SubscribeBeginFrame(BusFrameEventHandler handler)
		{
			m_beginFrame += handler;
		}

		void IBusManager.SubscribeEndFrame(BusFrameEventHandler handler)
		{
			m_endFrame += handler;
		}

		void IBusManager.AddSerializer(FormatSerializer serializer)
		{
			if (m_loadManager != null)
				m_loadManager.AddSerializer(serializer);
		}

		void IBusManager.RegisterIcon(IconDescriptor iconDesc)
		{
			List<IconDescriptor> list = new List<IconDescriptor>(m_iconDescList);
			list.Add(iconDesc);
			m_iconDescList = list.ToArray();
		}

		Z80CPU IBusManager.CPU
		{
			get { return m_cpu; }
		}

		bool IBusManager.IsSandbox
		{
			get { return m_sandBox; }
		}

		String IBusManager.GetSatelliteFileName(string extension)
		{
			if (m_sandBox || string.IsNullOrEmpty(m_machineFile))
				return null;
			extension = extension.Trim();
			if (!extension.StartsWith("."))
				extension = "." + extension;
			return Path.ChangeExtension(m_machineFile, extension);
		}

		public BusDeviceBase FindDevice(Type type)
		{
			foreach (BusDeviceBase device in m_deviceList)
				if (type.IsAssignableFrom(device.GetType()))
					return device;
			return null;
		}

		public List<BusDeviceBase> FindDevices(Type type)
		{
			List<BusDeviceBase> list = new List<BusDeviceBase>();
			foreach (BusDeviceBase device in m_deviceList)
				if (type.IsAssignableFrom(device.GetType()))
					list.Add(device);
			return list;
		}

		#endregion

		#region CPU Handlers

		private byte RDMEM_M1(ushort addr)
		{
			BusReadProc proc = m_mapReadMemoryM1[addr];
			byte result = m_cpu.BUS;
			if (proc != null)
				proc(addr, ref result);
			return result;
		}

		private byte RDMEM(ushort addr)
		{
			BusReadProc proc = m_mapReadMemory[addr];
			byte result = m_cpu.BUS;
			if (proc != null)
				proc(addr, ref result);
			return result;
		}

		private void WRMEM(ushort addr, byte value)
		{
			BusWriteProc proc = m_mapWriteMemory[addr];
			if (proc != null)
				proc(addr, value);
		}

		private byte RDPORT(ushort addr)
		{
			BusReadIoProc proc = m_mapReadPort[addr];
			byte result = m_cpu.BUS;
			bool iorqge = true;
			if (proc != null)
				proc(addr, ref result, ref iorqge);
			if (RzxHandler.IsPlayback)
				return RzxHandler.GetInput();
			return result;
		}

		private void WRPORT(ushort addr, byte value)
		{
			if (RzxHandler.IsRecording)
				RzxHandler.SetInput(value);
			BusWriteIoProc proc = m_mapWritePort[addr];
			bool iorqge = true;
			if (proc != null)
				proc(addr, value, ref iorqge);
		}

		private void RDNOMREQ(ushort addr)
		{
			BusNoMreqProc proc = m_mapReadNoMreq[addr];
			if (proc != null)
				proc(addr);
		}

		private void WRNOMREQ(ushort addr)
		{
			BusNoMreqProc proc = m_mapWriteNoMreq[addr];
			if (proc != null)
				proc(addr);
		}

		private void RESET()
		{
			RzxHandler.Reset();
			if (m_reset != null)
				m_reset();
		}

		private void INTACK_M1()
		{
			if (m_intAck != null)
				m_intAck();
		}

		private void NMIACK_M1()
		{
			if (m_nmiAck != null)
				m_nmiAck();
		}

		#endregion

		#region Device Add/Remove

		public void Add(BusDeviceBase device)
		{
			if (m_connected)
				throw new InvalidOperationException("Cannot add device into connected bus!");
			foreach (BusDeviceBase device2 in m_deviceList)
				if (device2.GetType() == device.GetType())
					throw new InvalidOperationException(string.Format("Cannot add device {0}, because this device already exist!", device.Name));
			if (device is IJtagDevice && FindDevice(typeof(IJtagDevice)) != null)
				throw new InvalidOperationException(string.Format("Cannot add second instance of JtagDevice! '{0}'", device.Name));
			IUlaDevice ula = device as IUlaDevice;
			if (ula != null)
			{
				if (m_ula != null)
					throw new InvalidOperationException("Cannot add second ULA device!");
				else
					m_ula = ula;
			}
			device.BusOrder = m_deviceList.Count;
			m_deviceList.Add(device);
		}

		public void Remove(BusDeviceBase device)
		{
			if (m_connected)
				throw new InvalidOperationException("Cannot remove device from connected bus!");
			if (device is IUlaDevice)
			{
				if (m_ula != device)
					throw new InvalidOperationException("Cannot remove non-existing ULA device!");
				else
					m_ula = null;
			}
			m_deviceList.Remove(device);
		}

		public void Clear()
		{
			if (m_connected)
				throw new InvalidOperationException("Cannot remove device from connected bus!");
			m_deviceList.Clear();
			m_ula = null;

			m_mapReadMemoryM1 = null;
			m_mapReadMemory = null;
			m_mapReadPort = null;
			m_mapWriteMemory = null;
			m_mapWritePort = null;
			m_mapReadNoMreq = null;
			m_mapWriteNoMreq = null;
			m_preCycle = null;
			m_reset = null;
			m_nmiAck = null;
			m_intAck = null;
			m_beginFrame = null;
			m_endFrame = null;
		}

		#endregion

		public bool Connect()
		{
			bool success = true;
			if (m_connected)
				throw new InvalidOperationException("Cannot connect the bus which is already connected!");
			if (m_ula == null)
				throw new InvalidOperationException("ULA device is missing!");
			m_connected = true;
			m_mapReadMemoryM1 = new BusReadProc[0x10000];
			m_mapReadMemory = new BusReadProc[0x10000];
			m_mapReadPort = new BusReadIoProc[0x10000];
			m_mapWriteMemory = new BusWriteProc[0x10000];
			m_mapWritePort = new BusWriteIoProc[0x10000];
			m_mapReadNoMreq = new BusNoMreqProc[0x10000];
			m_mapWriteNoMreq = new BusNoMreqProc[0x10000];
			m_preCycle = null;
			m_reset = null;
			m_nmiAck = null;
			m_intAck = null;
			m_beginFrame = null;
			m_endFrame = null;
			m_deviceList.Sort(new BusDevicePriorityComparison());
			for (int i = 0; i < m_deviceList.Count; i++)
				m_deviceList[i].BusOrder = i;
			if (m_loadManager != null)
			{
				m_loadManager.Clear();
				m_loadManager.AddStandardSerializers();
			}
			m_iconDescList = new IconDescriptor[] { m_iconPause };
			foreach (BusDeviceBase device in m_deviceList)
			{
				try { device.BusInit(this); }
				catch (Exception ex)
				{ success = false; LogAgent.Error(ex); }
			}
			m_frameTactCount = m_ula.FrameTactCount;
			foreach (BusDeviceBase device in m_deviceList)
			{
				try { device.BusConnect(); }
				catch (Exception ex)
				{ success = false; LogAgent.Error(ex); }
			}
			OnBeginFrame();
			if (m_debuggable != null)
			{
				IJtagDevice jtag = FindDevice(typeof(IJtagDevice)) as IJtagDevice;
				if (jtag != null)
					jtag.Attach(m_debuggable);
			}
			OnBusConnected();
			return success;
		}

		public void Disconnect()
		{
			if (!m_connected)
				return;
			m_connected = false;
			OnEndFrame();
			OnBusDisconnect();
			if (m_loadManager != null)
				m_loadManager.Clear();
			m_iconDescList = new IconDescriptor[0];
			if (m_debuggable != null)
			{
				IJtagDevice jtag = FindDevice(typeof(IJtagDevice)) as IJtagDevice;
				if (jtag != null)
					jtag.Detach();
			}
			foreach (BusDeviceBase device in m_deviceList)
			{
				try
				{
					device.BusDisconnect();
				}
				catch (Exception ex)
				{
					LogAgent.Error(ex);
				}
			}
		}

		protected virtual void OnBusConnected()
		{
			if (BusConnected != null)
				BusConnected(this, EventArgs.Empty);
		}

		protected virtual void OnBusDisconnect()
		{
			if (BusDisconnect != null)
				BusDisconnect(this, EventArgs.Empty);
		}

		internal void SetDebuggable(IDebuggable dbg)
		{
			if (m_debuggable != null || dbg == null)
				throw new NotSupportedException("Assign for IDebuggable allowed only once!");
			m_debuggable = dbg;
			if (m_connected)
			{
				IJtagDevice jtag = FindDevice(typeof(IJtagDevice)) as IJtagDevice;
				if (jtag != null)
					jtag.Attach(m_debuggable);
			}
		}

		private int m_frameTactCount;
		public int GetFrameTact()
		{
			return (int)(m_cpu.Tact % m_frameTactCount);
		}

		private bool m_frameOpened = false;

		private void OnBeginFrame()
		{
			if (m_frameOpened)
			{
				LogAgent.Warn("Trying to begin frame twice");
				return;
			}
			m_frameOpened = true;
			if (m_beginFrame != null)
				m_beginFrame();
		}

		private void OnEndFrame()
		{
			if (!m_frameOpened)
			{
				LogAgent.Warn("Trying to end frame twice");
				return;
			}
			m_frameOpened = false;
			if (m_endFrame != null)
				m_endFrame();
			if (FrameReady != null)
				FrameReady();
		}

		private int m_lastFrameTact = int.MaxValue;

		public void ExecCycle()
		{
			int frameTact = GetFrameTact();
			if (frameTact < m_lastFrameTact)
			{
				OnEndFrame();
				OnBeginFrame();
			}
			m_lastFrameTact = frameTact;

			m_cpu.INT = RzxHandler.IsPlayback ?
				RzxHandler.CheckInt(frameTact) :
				m_ula.CheckInt(frameTact);

			if (m_preCycle != null)
				m_preCycle(frameTact);
			m_cpu.ExecCycle();
		}

		internal String MachineFile
		{
			get { return m_machineFile; }
			set { m_machineFile = value; }
		}

		public int FrameTactCount { get { return m_frameTactCount;/*m_ula.FrameTactCount*/; } }

		public void LoadConfig(XmlNode busNode)
		{
			//LogAgent.Debug("time begin BusManager.LoadConfig");
			Disconnect();
			// store old devices to allow reuse & save state
			Dictionary<string, BusDeviceBase> oldDevices = new Dictionary<string, BusDeviceBase>();
			foreach (BusDeviceBase device in m_deviceList)
				oldDevices.Add(getDeviceKey(device.GetType()), device);
			Clear();
			int orderCounter = 0;
			foreach (XmlNode node in busNode.SelectNodes("Device"))
			{
				try
				{
					Assembly asm = Assembly.GetExecutingAssembly();
					if (node.Attributes["assembly"] != null)
					{
						string assemblyFile = node.Attributes["assembly"].InnerText;
						asm = Assembly.LoadFrom(assemblyFile);
					}
					if (node.Attributes["type"] == null)
					{
						LogAgent.Error("Device type not specified!");
						continue;
					}
					string typeName = node.Attributes["type"].InnerText;
					Type type = asm.GetType(typeName);
					if (type == null)
					{
						LogAgent.Error("Device not found: {0}", typeName);
						continue;
					}
					if (!typeof(BusDeviceBase).IsAssignableFrom(type))
					{
						LogAgent.Error("Invalid Device: {0}", type.FullName);
						continue;
					}
					BusDeviceBase device = null;
					string key = getDeviceKey(type);
					if (oldDevices.ContainsKey(key))
					{
						//reuse
						device = oldDevices[key];
					}
					else
					{
						//create new
						device = (BusDeviceBase)Activator.CreateInstance(type);
					}
					int busOrder;
					if (node.Attributes["busOrder"] == null || int.TryParse(node.Attributes["busOrder"].InnerText, out busOrder))
						busOrder = orderCounter++;
					device.BusOrder = busOrder;

					IConfigurable configurable = device as IConfigurable;
					if (configurable != null)
						configurable.LoadConfig(node);
					Add(device);
				}
				catch (Exception ex)
				{
					LogAgent.Error(ex);
					DialogProvider.Show(
						string.Format("Load device failed: {0}", ex.Message),
						"ERROR",
						DlgButtonSet.OK,
						DlgIcon.Error);
				}
			}
			Connect();
			//LogAgent.Debug("time end BusManager.LoadConfig");
		}

		private string getDeviceKey(Type type)
		{
			string asm = string.Empty;
			if (type.Assembly != Assembly.GetExecutingAssembly())
				asm = type.Assembly.Location;
			return string.Format("{0}|{1}", asm, type.FullName);
		}

		public void SaveConfig(XmlNode busNode)
		{
			//LogAgent.Debug("time begin BusManager.SaveConfig");
			foreach (BusDeviceBase device in m_deviceList)
			{
				try
				{
					XmlElement xe = busNode.OwnerDocument.CreateElement("Device");
					if (device.GetType().Assembly != Assembly.GetExecutingAssembly())
						xe.SetAttribute("assembly", device.GetType().Assembly.Location);
					xe.SetAttribute("type", device.GetType().FullName);
					xe.SetAttribute("busOrder", device.BusOrder.ToString());
					XmlNode node = busNode.AppendChild(xe);
					IConfigurable configurable = device as IConfigurable;
					if (configurable != null)
						configurable.SaveConfig(node);
				}
				catch (Exception ex)
				{
					LogAgent.Error(ex);
				}
			}
			//LogAgent.Debug("time end BusManager.SaveConfig");
		}

		public void Sort()
		{
			m_deviceList.Sort(new BusDevicePriorityComparison());
			for (int i = 0; i < m_deviceList.Count; i++)
				m_deviceList[i].BusOrder = i;
		}
	}

	public class BusDevicePriorityComparison : IComparer<BusDeviceBase>, System.Collections.IComparer
	{
		public int Compare(BusDeviceBase x, BusDeviceBase y)
		{
			if (x == y)
				return 0;
			if (x is IUlaDevice)      // should be first
				return -1;
			if (x is IMemoryDevice)   // should be last
				return 1;
			if (y is IUlaDevice)      // should be first
				return 1;
			if (y is IMemoryDevice)   // should be last
				return -1;
			// priority for other devices is not implemented yet
			return x.BusOrder < y.BusOrder ? -1 : x.BusOrder > y.BusOrder ? 1 : 0;
		}

		public int Compare(object x, object y)
		{
			return Compare(x as BusDeviceBase, y as BusDeviceBase);
		}
	}
}