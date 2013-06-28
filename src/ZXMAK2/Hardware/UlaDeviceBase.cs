using System;
using System.IO;
using System.Xml;
using System.Drawing;
using ZXMAK2.Interfaces;
using ZXMAK2.Serializers.ScreenshotSerializers;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine;
using ZXMAK2.Entities;


namespace ZXMAK2.Hardware
{
    public abstract class UlaDeviceBase : BusDeviceBase, IUlaDevice
    {
        #region Fields

        protected Z80CPU CPU;
        private int[] m_bitmapBuffer = new int[1024 * 768];
        private IMemoryDevice m_memory;
        private int m_lastFrameTact = 0;         // last processed tact
        private byte m_portFe = 0;

        protected int m_videoPage = 5;
        protected int m_page0000 = -1;
        protected int m_page4000 = 5;
        protected int m_page8000 = 2;
        protected int m_pageC000 = 0;
        protected SpectrumRenderer SpectrumRenderer = new SpectrumRenderer();
        protected IUlaRenderer Renderer { get; set; }

        #endregion Fields

        
        #region IBusDevice

        public override string Description { get { return "UlaDeviceBase based ULA"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.ULA; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_page0000 = -1;
            m_page4000 = 5;
            m_page8000 = 2;
            m_pageC000 = 0;
            m_videoPage = 5;

            CPU = bmgr.CPU;
            Memory = bmgr.FindDevice<IMemoryDevice>();

            bmgr.SubscribeWrMem(0xC000, 0x0000, WriteMem0000);
            bmgr.SubscribeWrMem(0xC000, 0x4000, WriteMem4000);
            bmgr.SubscribeWrMem(0xC000, 0x8000, WriteMem8000);
            bmgr.SubscribeWrMem(0xC000, 0xC000, WriteMemC000);

            bmgr.SubscribeWrIo(0x0001, 0x0000, WritePortFE);

            bmgr.SubscribeBeginFrame(BeginFrame);
            bmgr.SubscribeEndFrame(EndFrame);

            bmgr.AddSerializer(new ScrSerializer(this));
            bmgr.AddSerializer(new BmpSerializer(this));
            bmgr.AddSerializer(new JpgSerializer(this));
            bmgr.AddSerializer(new PngSerializer(this));
        }

        public override void BusConnect()
        {
            OnTimingChanged();
        }

        public override void BusDisconnect()
        {
        }

        #endregion



        public UlaDeviceBase()
        {
            OnRendererInit();
            Renderer = SpectrumRenderer;
        }

        protected virtual void OnRendererInit()
        {
            SpectrumRenderer.Params = CreateSpectrumRendererParams();
            SpectrumRenderer.Palette = SpectrumRenderer.CreatePalette();
        }

        protected abstract SpectrumRendererParams CreateSpectrumRendererParams();

        public virtual void SetPageMapping(
            int videoPage,
            int page0000,
            int page4000,
            int page8000,
            int pageC000)
        {
            UpdateState((int)(CPU.Tact % FrameTactCount));
            m_videoPage = videoPage;
            m_page0000 = page0000;
            m_page4000 = page4000;
            m_page8000 = page8000;
            m_pageC000 = pageC000;
            SpectrumRenderer.MemoryPage = m_memory.RamPages[videoPage];
        }


        #region Bus Handlers

        protected virtual void WriteMem0000(ushort addr, byte value)
        {
            if (m_videoPage == m_page0000)
                UpdateState((int)(CPU.Tact % FrameTactCount));
        }

        protected virtual void WriteMem4000(ushort addr, byte value)
        {
            if (m_videoPage == m_page4000)
                UpdateState((int)(CPU.Tact % FrameTactCount));
        }

        protected virtual void WriteMem8000(ushort addr, byte value)
        {
            if (m_videoPage == m_page8000)
                UpdateState((int)(CPU.Tact % FrameTactCount));
        }

        protected virtual void WriteMemC000(ushort addr, byte value)
        {
            if (m_videoPage == m_pageC000)
                UpdateState((int)(CPU.Tact % FrameTactCount));
        }


        protected virtual void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            UpdateState((int)(CPU.Tact % FrameTactCount));
            PortFE = value;
        }

        protected virtual void ReadPortFF(int frameTact, ref byte value)
        {
            Renderer.ReadFreeBus(frameTact, ref value);
        }

        public virtual bool CheckInt(int frameTact)
        {
            return frameTact < Renderer.IntLength;
        }

        #endregion

        protected virtual IMemoryDevice Memory
        {
            get { return m_memory; }
            set
            {
                if (value == null)
                    throw new ArgumentException(string.Format("Memory Device is missing!"));
                m_memory = value;
                if (Memory.RamPages.Length < 8)
                    throw new ArgumentException(string.Format("Incompatible Memory Type: {0}", Memory.GetType()));
                SpectrumRenderer.MemoryPage = Memory.RamPages[m_videoPage];
            }
        }

        public int FrameTactCount
        {
            get { return Renderer.FrameLength; }
        }

        public int[] VideoBuffer
        {
            get { return m_bitmapBuffer; }
            set { m_bitmapBuffer = value; }
        }

        public float VideoHeightScale
        {
            get { return Renderer.PixelHeightRatio; }
        }

        public Size VideoSize
        {
            get { return Renderer.VideoSize; }
        }

        public void LoadScreenData(Stream stream)
        {
            Renderer.LoadScreenData(stream);
        }

        public void SaveScreenData(Stream stream)
        {
            Renderer.SaveScreenData(stream);
        }

        #region Comment
        /// <summary>
        /// Callback to process Memory/Port changes
        /// </summary>
        /// <param name="frameTact">frameTact = _cpu.Tact % FrameTactCount</param>
        #endregion
        protected unsafe void UpdateState(int frameTact)
        {
            if (frameTact < m_lastFrameTact)
                frameTact = FrameTactCount;
            fixed (int* ptr = m_bitmapBuffer)
            {
                Renderer.Render(
                    (uint*)ptr,
                    m_lastFrameTact,
                    frameTact);
            }
            m_lastFrameTact = frameTact;
        }

        protected virtual void BeginFrame()
        {
            m_lastFrameTact = 0;
            UpdateState(0);
        }

        #region Comment
        /// <summary>
        /// Fill video frame buffer to end
        /// </summary>
        #endregion Comment
        protected virtual void EndFrame()
        {
            UpdateState(FrameTactCount);
            m_lastFrameTact = FrameTactCount;
            Renderer.Frame();
        }


        public void Flush()
        {
            UpdateState((int)(CPU.Tact % FrameTactCount));
        }

        public unsafe void ForceRedrawFrame()
        {
            //TODO: what about _ulaFetch??
            fixed (int* ptr = m_bitmapBuffer)
            {
                Renderer.Render(
                    (uint*)ptr,
                    0,
                    FrameTactCount);
            }
        }

        public virtual byte PortFE
        {
            get { return m_portFe; }
            set { m_portFe = value; Renderer.UpdateBorder(value & 7); }
        }

        protected virtual void OnTimingChanged()
        {
            //_renderer.Timing = _renderer.Timing;
            //OnPaletteChanged();
        }
    }
}
