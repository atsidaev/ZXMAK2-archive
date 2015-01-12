using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Serializers;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Resources;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Engine
{
    public class BusManager : IBusManager
    {
        private bool m_connected = false;
        private bool m_sandBox = false;
        private String m_machineFile = null;
        private CpuUnit m_cpu;
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
        private BusRqProc m_nmiRq;
        private BusSignalProc m_nmiAck;
        private BusSignalProc m_intAck;
        private BusFrameEventHandler m_beginFrame;
        private BusFrameEventHandler m_endFrame;
        private IIconDescriptor[] m_iconDescList = new IIconDescriptor[0];
        private IconDescriptor m_iconPause = new IconDescriptor(
            "PAUSE",
            ImageResources.Pause_32x32);

        public event BusFrameEventHandler FrameReady;
        public event EventHandler BusConnected;
        public event EventHandler BusDisconnect;
        public event EventHandler ConfigChanged;

        private int m_pendingNmi;
        private long m_pendingNmiLastTact;

        public CpuUnit Cpu { get { return m_cpu; } }
        public ISerializeManager LoadManager { get; private set; }
        public ICommandManager CommandManager { get; set; }
        public RzxHandler RzxHandler { get; set; }
        public IIconDescriptor[] IconDescriptorArray { get { return m_iconDescList; } }
        public IconDescriptor IconPause { get { return m_iconPause; } }
        
        public ModelId ModelId { get; set; }
        public string Name { get; set; }

        
        public BusManager()
        {
            m_cpu = new CpuUnit();
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
        }


        public void Init(SpectrumBase spectrum, bool sandBox)
        {
            m_sandBox = sandBox;
            LoadManager = new LoadManager(spectrum);
            m_iconDescList = new IconDescriptor[0];
            if (CommandManager != null)
            {
                CommandManager.Clear();
            }
            RzxHandler = new RzxHandler(m_cpu, this);

            m_deviceList.Clear();
            m_mapReadMemoryM1 = null;
            m_mapReadMemory = null;
            m_mapReadPort = null;
            m_mapWriteMemory = null;
            m_mapWritePort = null;
            m_reset = null;
            m_nmiRq = null;
            m_nmiAck = null;
            m_intAck = null;
            m_preCycle = null;
            m_beginFrame = null;
            m_endFrame = null;

            var config = new MachinesConfig();
            config.Load();
            LoadConfigXml(config.GetDefaultConfig());
        }

        #region IBusManager

        void IBusManager.SubscribeRdMemM1(int addrMask, int maskedValue, BusReadProc proc)
        {
            for (int addr = 0; addr < 0x10000; addr++)
                if ((addr & addrMask) == maskedValue)
                    m_mapReadMemoryM1[addr] += proc;
        }

        void IBusManager.SubscribeRdMem(int addrMask, int maskedValue, BusReadProc proc)
        {
            for (int addr = 0; addr < 0x10000; addr++)
                if ((addr & addrMask) == maskedValue)
                    m_mapReadMemory[addr] += proc;
        }

        void IBusManager.SubscribeWrMem(int addrMask, int maskedValue, BusWriteProc proc)
        {
            for (int addr = 0; addr < 0x10000; addr++)
                if ((addr & addrMask) == maskedValue)
                    m_mapWriteMemory[addr] += proc;
        }

        void IBusManager.SubscribeRdIo(int addrMask, int maskedValue, BusReadIoProc proc)
        {
            for (int addr = 0; addr < 0x10000; addr++)
                if ((addr & addrMask) == maskedValue)
                    m_mapReadPort[addr] += proc;
        }

        void IBusManager.SubscribeWrIo(int addrMask, int maskedValue, BusWriteIoProc proc)
        {
            for (int addr = 0; addr < 0x10000; addr++)
                if ((addr & addrMask) == maskedValue)
                    m_mapWritePort[addr] += proc;
        }

        void IBusManager.SubscribeRdNoMreq(int addrMask, int maskedValue, BusNoMreqProc proc)
        {
            for (int addr = 0; addr < 0x10000; addr++)
                if ((addr & addrMask) == maskedValue)
                    m_mapReadNoMreq[addr] += proc;
        }

        void IBusManager.SubscribeWrNoMreq(int addrMask, int maskedValue, BusNoMreqProc proc)
        {
            for (int addr = 0; addr < 0x10000; addr++)
                if ((addr & addrMask) == maskedValue)
                    m_mapWriteNoMreq[addr] += proc;
        }

        void IBusManager.SubscribePreCycle(BusCycleProc proc)
        {
            m_preCycle += proc;
        }

        void IBusManager.SubscribeReset(BusSignalProc proc)
        {
            m_reset += proc;
        }

        void IBusManager.SubscribeNmiRq(BusRqProc proc)
        {
            m_nmiRq += proc;
        }

        void IBusManager.SubscribeNmiAck(BusSignalProc proc)
        {
            m_nmiAck += proc;
        }

        void IBusManager.SubscribeIntAck(BusSignalProc proc)
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

        void IBusManager.AddSerializer(IFormatSerializer serializer)
        {
            if (LoadManager != null)
                LoadManager.AddSerializer(serializer);
        }

        void IBusManager.RegisterIcon(IIconDescriptor iconDesc)
        {
            var list = new List<IIconDescriptor>(m_iconDescList);
            list.Add(iconDesc);
            m_iconDescList = list.ToArray();
        }

        void IBusManager.AddCommandUi(ICommand command)
        {
            if (CommandManager != null)
            {
                CommandManager.Add(command);
            }
        }

        CpuUnit IBusManager.CPU
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

        public T FindDevice<T>()
            where T : class
        {
            var type = typeof(T);
            foreach (object device in m_deviceList)
            {
                if (type.IsAssignableFrom(device.GetType()))
                    return (T)device;
            }
            return null;
        }

        public List<T> FindDevices<T>()
            where T : class
        {
            var type = typeof(T);
            var list = new List<T>();
            foreach (object device in m_deviceList)
            {
                if (type.IsAssignableFrom(device.GetType()))
                    list.Add((T)device);
            }
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
            //LogAgent.Info(
            //    "{0:D3}-{1:D6}: #{2:X4} = #{3:X2}",
            //    m_cpu.Tact / m_ula.FrameTactCount,
            //    m_cpu.Tact % m_ula.FrameTactCount,
            //    m_cpu.regs.PC,
            //    result);
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
            var proc = m_mapReadPort[addr];
            var result = m_cpu.BUS;
            if (proc != null)
            {
                var iorqge = true;
                proc(addr, ref result, ref iorqge);
            }
            if (RzxHandler.IsPlayback)
            {
                return RzxHandler.GetInput();
            }
            else if (RzxHandler.IsRecording)
            {
                RzxHandler.SetInput(result);
            }
            return result;
        }

        private void WRPORT(ushort addr, byte value)
        {
            var proc = m_mapWritePort[addr];
            if (proc != null)
            {
                var iorqge = true;
                proc(addr, value, ref iorqge);
            }
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
            m_pendingNmi = 0;
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
            {
                throw new InvalidOperationException("Cannot add device into connected bus!");
            }
            foreach (var device2 in m_deviceList)
            {
                if (device2.GetType() == device.GetType())
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Cannot add device {0}, because this device already exist!",
                            device.Name));
                }
            }
            if (device is IJtagDevice && FindDevice<IJtagDevice>() != null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot add second instance of JtagDevice! '{0}'",
                        device.Name));
            }
            var ula = device as IUlaDevice;
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
            m_nmiRq = null;
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
            m_nmiRq = null;
            m_nmiAck = null;
            m_intAck = null;
            m_beginFrame = null;
            m_endFrame = null;
            m_deviceList.Sort(DevicePriorityComparison);
            for (int i = 0; i < m_deviceList.Count; i++)
            {
                m_deviceList[i].BusOrder = i;
            }
            if (LoadManager != null)
            {
                LoadManager.Clear();
            }
            m_iconDescList = new IconDescriptor[] { m_iconPause };
            if (CommandManager != null)
            {
                CommandManager.Clear();
            }
            foreach (var device in m_deviceList)
            {
                try { device.BusInit(this); }
                catch (Exception ex)
                { success = false; Logger.Error(ex); }
            }
            m_frameTactCount = m_ula.FrameTactCount;
            foreach (var device in m_deviceList)
            {
                device.ConfigChanged += Device_OnConfigChanged;
                try { device.BusConnect(); }
                catch (Exception ex)
                { success = false; Logger.Error(ex); }
            }
            OnBeginFrame();
            if (m_debuggable != null)
            {
                var jtag = FindDevice<IJtagDevice>();
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
            if (LoadManager != null)
            {
                LoadManager.Clear();
            }
            m_iconDescList = new IconDescriptor[0];
            if (CommandManager != null)
            {
                CommandManager.Clear();
            }
            if (m_debuggable != null)
            {
                var jtag = FindDevice<IJtagDevice>();
                if (jtag != null)
                    jtag.Detach();
            }
            foreach (var device in m_deviceList)
            {
                device.ConfigChanged -= Device_OnConfigChanged;
                try
                {
                    device.BusDisconnect();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        protected virtual void OnBusConnected()
        {
            var handler = BusConnected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnBusDisconnect()
        {
            var handler = BusDisconnect;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void Device_OnConfigChanged(object sender, EventArgs e)
        {
            var handler = ConfigChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        internal void SetDebuggable(IDebuggable dbg)
        {
            if (m_debuggable != null || dbg == null)
                throw new NotSupportedException("Assign for IDebuggable allowed only once!");
            m_debuggable = dbg;
            if (m_connected)
            {
                var jtag = FindDevice<IJtagDevice>();
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
                Logger.Warn("Trying to begin frame twice");
                return;
            }
            m_frameOpened = true;

            var beginHandler = m_beginFrame;
            if (beginHandler != null)
            {
                beginHandler();
            }
        }

        private void OnEndFrame()
        {
            if (!m_frameOpened)
            {
                Logger.Warn("Trying to end frame twice");
                return;
            }
            m_frameOpened = false;

            var endHandler = m_endFrame;
            if (endHandler != null)
            {
                endHandler();
            }
            var readyHandler = FrameReady;
            if (readyHandler != null)
            {
                readyHandler();
            }
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
            if (m_pendingNmi > 0)
            {
                var delta = (int)(m_cpu.Tact - m_pendingNmiLastTact);
                m_pendingNmiLastTact = m_cpu.Tact;
                m_pendingNmi -= delta;
                var e = new BusCancelArgs();
                if (m_nmiRq != null)
                {
                    m_nmiRq(e);
                }
                if (!e.Cancel)
                {
                    m_cpu.NMI = true;
                    m_pendingNmi = 0;
                }
            }
            else
            {
                m_cpu.NMI = false;
            }

            if (m_preCycle != null)
            {
                m_preCycle(frameTact);
            }
            m_cpu.ExecCycle();
        }

        internal String MachineFile
        {
            get { return m_machineFile; }
            set { m_machineFile = value; }
        }

        public int FrameTactCount { get { return m_frameTactCount;/*m_ula.FrameTactCount*/; } }

        public void LoadConfigXml(XmlNode busNode)
        {
            //LogAgent.Debug("time begin BusManager.LoadConfig");
            Disconnect();

            Name = null;
            if (busNode.Attributes["name"] != null)
            {
                Name = busNode.Attributes["name"].InnerText;
            }
            ModelId = ModelId.None;
            if (busNode.Attributes["modelId"] != null)
            {
                var value = busNode.Attributes["modelId"].InnerText;
                var modelId = ModelId.None;
                if (!Enum.TryParse<ModelId>(value, out modelId))
                {
                    Logger.Warn("Unknown modelId: {0}", value);
                }
                else
                {
                    ModelId = modelId;
                }
            }

            // store old devices to allow reuse & save state
            var oldDevices = new Dictionary<string, BusDeviceBase>();
            foreach (BusDeviceBase device in m_deviceList)
            {
                oldDevices.Add(getDeviceKey(device.GetType()), device);
            }
            Clear();
            var orderCounter = 0;
            // "Device"
            var deviceNodes = busNode.ChildNodes
                .OfType<XmlNode>()
                .Where(node => string.Compare(node.Name, "Device", true) == 0)
                .Where(node => !string.IsNullOrEmpty(GetAttrString(node, "type")))
                .Where(node => GetAttrString(node, "type").Trim() != string.Empty);
            foreach (XmlNode node in deviceNodes)
            {
                try
                {
                    var fullTypeName = GetAttrString(node, "type");
                    var type = GetTypeByName(fullTypeName, GetAttrString(node, "assembly"));
                    if (type == null)
                    {
                        Logger.Error("Type not found: {0}", fullTypeName);
                        continue;
                    }
                    if (!typeof(BusDeviceBase).IsAssignableFrom(type))
                    {
                        Logger.Error("Invalid Device: {0}", type.FullName);
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
                    device.BusOrder = orderCounter++;

                    device.LoadConfigXml(node);
                    Add(device);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    Locator.Resolve<IUserMessage>()
                        .Error("Load device failed: {0}", ex.Message);
                }
            }
            Sort();
            Connect();
            //LogAgent.Debug("time end BusManager.LoadConfig");
        }

        private static string GetNameByType(Type type)
        {
            return string.Format(
                "{0}, {1}",
                type.FullName,
                type.Assembly.GetName().Name);
        }

        private static bool CheckIsLocalAssembly(Assembly asm)
        {
            var asmPath = Path.GetDirectoryName(Path.GetFullPath(asm.Location));
            var localPath = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));
            return string.Compare(asmPath, localPath, true) == 0;
        }

        private static Type GetTypeByName(string fullTypeName, string oldAsmName)
        {
            var asmName = (string)null;
            var typeName = fullTypeName;
            if (fullTypeName.Contains(','))
            {
                var nameParts = fullTypeName.Split(',')
                    .Select(namePart => namePart.Trim());
                typeName = nameParts.First();
                asmName = nameParts.Skip(1).First();
            }
            asmName = GetTrimmedString(asmName);
            if (asmName == null)
            {
                asmName = "ZXMAK2";
            }
            var asm = asmName != null ?
                Assembly.Load(asmName) :
                oldAsmName != null ? Assembly.LoadFrom(oldAsmName) : null;
            if (asm != null)
            {
                return asm.GetType(typeName);
            }
            return null;
        }

        private static string GetTrimmedString(string value)
        {
            if (value == null)
            {
                return null;
            }
            value = value.Trim();
            if (value == string.Empty)
            {
                return null;
            }
            return value;
        }

        private static string GetAttrString(XmlNode node, string name)
        {
            var attr = node.Attributes[name];
            if (attr == null)
            {
                return null;
            }
            return attr.InnerText;
        }

        private string getDeviceKey(Type type)
        {
            string asm = string.Empty;
            if (type.Assembly != Assembly.GetExecutingAssembly())
                asm = type.Assembly.Location;
            return string.Format("{0}|{1}", asm, type.FullName);
        }

        public void SaveConfigXml(XmlNode busNode)
        {
            //LogAgent.Debug("time begin BusManager.SaveConfig");
            if (!string.IsNullOrEmpty(Name))
            {
                var el = (XmlElement)busNode;
                el.SetAttribute("name", Name);
            }
            if (ModelId != ModelId.None)
            {
                var el = (XmlElement)busNode;
                el.SetAttribute("modelId", ModelId.ToString());
            }
            foreach (var device in m_deviceList)
            {
                try
                {
                    var type = device.GetType();
                    var fullTypeName = GetNameByType(type);
                    var xe = busNode.OwnerDocument.CreateElement("Device");
                    xe.SetAttribute("type", fullTypeName);
                    if (!CheckIsLocalAssembly(type.Assembly))
                    {
                        // non local assembly
                        xe.SetAttribute("assembly", type.Assembly.Location);
                    }
                    var node = busNode.AppendChild(xe);
                    device.SaveConfigXml(node);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            //LogAgent.Debug("time end BusManager.SaveConfig");
        }

        public void Sort()
        {
            m_deviceList.Sort(DevicePriorityComparison);
            for (int i = 0; i < m_deviceList.Count; i++)
                m_deviceList[i].BusOrder = i;
        }

        private static int DevicePriorityComparison(
            BusDeviceBase x,
            BusDeviceBase y)
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
            return x.BusOrder < y.BusOrder ?
                -1 :
                x.BusOrder > y.BusOrder ? 1 : 0;
        }

        public void RequestNmi(int timeOut)
        {
            m_pendingNmiLastTact = m_cpu.Tact;
            m_pendingNmi = timeOut;
        }
    }
}
