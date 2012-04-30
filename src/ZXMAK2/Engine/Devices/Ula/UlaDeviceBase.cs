using System;
using System.IO;
using System.Xml;
using System.Drawing;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Serializers.ScreenshotSerializers;
using ZXMAK2.Engine.Z80;


namespace ZXMAK2.Engine.Devices.Ula
{
    public abstract class UlaDeviceBase : BusDeviceBase, IUlaDevice
    {
        #region IBusDevice

        public override string Description { get { return "ULA device based on UlaDeviceBase"; } }
        public override BusCategory Category { get { return BusCategory.ULA; } }

        public override void BusInit(IBusManager bmgr)
        {
			m_page0000 = -1;
			m_page4000 = 5;
			m_page8000 = 2;
			m_pageC000 = 0;
			m_videoPage = 5;

			CPU = bmgr.CPU;
			Memory = (IMemoryDevice)bmgr.FindDevice(typeof(IMemoryDevice));

			bmgr.SubscribeWRMEM(0xC000, 0x0000, WriteMem0000);
			bmgr.SubscribeWRMEM(0xC000, 0x4000, WriteMem4000);
			bmgr.SubscribeWRMEM(0xC000, 0x8000, WriteMem8000);
            bmgr.SubscribeWRMEM(0xC000, 0xC000, WriteMemC000);

            bmgr.SubscribeWRIO(0x0001, 0x0000, WritePortFE);

            bmgr.SubscribePreCycle(CheckInt);
            bmgr.SubscribeBeginFrame(BeginFrame);
            bmgr.SubscribeEndFrame(EndFrame);

            bmgr.AddSerializer(new ScrSerializer(this));
            bmgr.AddSerializer(new BmpSerializer(this));
            bmgr.AddSerializer(new JpgSerializer(this));
            bmgr.AddSerializer(new PngSerializer(this));
        }

        public override void BusConnect()
        {
            fillUlaTables(c_frameTactCount);
        }

        public override void BusDisconnect()
        {
        }

        #endregion


		protected Z80CPU CPU; 

        #region Constants

        public int c_ulaLineTime = 224;
        public int c_ulaFirstPaperLine = 80;
        public int c_ulaFirstPaperTact = 68;      // 68 [32sync+36border+128scr+28border]
        public int c_frameTactCount = 71680;

        public int c_ulaBorderTop = 24;//64;
        public int c_ulaBorderBottom = 24;//48;
        public int c_ulaBorderLeftT = 16;//36;
        public int c_ulaBorderRightT = 16;//28;

        public int c_ulaIntBegin = 0;
        public int c_ulaIntLength = 32;
        public int c_ulaFlashPeriod = 25;

        public int c_ulaWidth;
        public int c_ulaHeight;

        #endregion

        #region Private

        private IMemoryDevice m_memory;
        protected int m_videoPage = 5;
		protected int m_page0000 = -1;
		protected int m_page4000 = 5;
		protected int m_page8000 = 2;
		protected int m_pageC000 = 0;

        protected int[] _ulaLineOffset;
        protected int[] _ulaAddrBW;
        protected int[] _ulaAddrAT;
        protected int[] _ulaDo;
        protected uint[] _ulaInk;
        protected uint[] _ulaPaper;

        protected int _ulaFetchB1;
        protected int _ulaFetchA1;
        protected int _ulaFetchB2;
        protected int _ulaFetchA2;
        protected uint _ulaFetchInk = 0;
        protected uint _ulaFetchPaper = 0;

        protected byte[] _ulaMemory;              // current video ram bank
        private int _lastFrameTact = 0;         // last processed tact
        protected int _flashState = 0;            // flash attr state (0/256)
        protected int _flashCounter = 0;          // flash attr counter
        protected uint _borderColor = 0;          // current border color
        private int[] _bitmapBufPtr = new int[1024 * 768];

        #endregion

		public virtual void SetPageMapping(int videoPage, int page0000, int page4000, int page8000, int pageC000)
		{
            UpdateState((int)((CPU.Tact+3) % FrameTactCount));
			m_videoPage = videoPage;
			m_page0000 = page0000;
			m_page4000 = page4000;
			m_page8000 = page8000;
			m_pageC000 = pageC000;
			_ulaMemory = m_memory.RamPages[videoPage];
		}


        public UlaDeviceBase()
        {
            // Pentagon 128K
            // Total Size:          448 x 320
            // Visible Size:        384 x 304 (72+256+56 x 64+192+48)
            // First Line Border:   16
            // First Line Paper:    80
            // Paper Lines:         192
            // Bottom Border Lines: 48

            c_ulaLineTime = 224;
            c_ulaFirstPaperLine = 80;
            c_ulaFirstPaperTact = 68;      // 68 [32sync+36border+128scr+28border]
            c_frameTactCount = 71680;

            c_ulaBorderTop = 24;//64;
            c_ulaBorderBottom = 24;//48;
            c_ulaBorderLeftT = 16;//36;
            c_ulaBorderRightT = 16;//28;

            c_ulaIntBegin = 0;
            c_ulaIntLength = 32;

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
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
            UpdateState((int)((CPU.Tact+3) % FrameTactCount));
            PortFE = value;
        }

        protected virtual void ReadPortFF(int frameTact, ref byte value)
        {
            int do0 = _ulaDo[frameTact];
            if (do0 == 2 || do0 == 4 || do0==9)
                value = _ulaMemory[_ulaAddrBW[frameTact]];
            else if (do0 == 3 || do0 == 5 || do0 == 10)
                value = _ulaMemory[_ulaAddrAT[frameTact]];
        }

        protected virtual void CheckInt(int frameTact)
        {
            CPU.INT = frameTact < c_ulaIntLength;
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
                _ulaMemory = Memory.RamPages[m_videoPage];
            }
        }

        public int FrameTactCount { get { return c_frameTactCount; } }

        public int[] VideoBuffer
        {
            get { return _bitmapBufPtr; }
            set { _bitmapBufPtr = value; }
        }

        public virtual float VideoHeightScale { get { return 1F; } }

        public virtual Size VideoSize { get { return new Size(c_ulaWidth, c_ulaHeight); } }

        public void LoadScreenData(Stream stream)
        {
            stream.Read(_ulaMemory, 0, 6912);
        }

        public void SaveScreenData(Stream stream)
        {
            stream.Write(_ulaMemory, 0, 6912);
        }

        #region Comment
        /// <summary>
        /// Callback to process Memory/Port changes
        /// </summary>
        /// <param name="frameTact">frameTact = _cpu.Tact % FrameTactCount</param>
        #endregion
        protected virtual unsafe void UpdateState(int frameTact)
        {
            if (frameTact < _lastFrameTact)
                frameTact = c_frameTactCount;
            fixed (int* ptr = _bitmapBufPtr)
                fetchVideo(
                    (uint*)ptr,
                    _lastFrameTact,
                    frameTact,
                    ref _ulaFetchB1,
                    ref _ulaFetchB2,
                    ref _ulaFetchA1,
                    ref _ulaFetchA2,
                    ref _ulaFetchInk,
                    ref _ulaFetchPaper);
            _lastFrameTact = frameTact;
        }

        protected virtual unsafe void BeginFrame()
        {
            _lastFrameTact = 0;
            UpdateState(0);
        }

        #region Comment
        /// <summary>
        /// Fill video frame buffer to end
        /// </summary>
        #endregion Comment
        protected virtual void EndFrame()
        {
            UpdateState(c_frameTactCount);
            _lastFrameTact = c_frameTactCount;

            _flashCounter++;
            if (_flashCounter >= c_ulaFlashPeriod)
            {
                _flashState ^= 256;
                _flashCounter = 0;
            }
        }


        public void Flush()
        {
            UpdateState((int)(CPU.Tact % FrameTactCount));            
        }

        public unsafe void ForceRedrawFrame()
        {
            int ulaFetchB1 = 0;
            int ulaFetchA1 = 0;
            int ulaFetchB2 = 0;
            int ulaFetchA2 = 0;
            uint ulaFetchInk = 0;
            uint ulaFetchPaper = 0;

            fixed (int* ptr = _bitmapBufPtr)
                fetchVideo(
                    (uint*)ptr,
                    0,
                    c_frameTactCount,
                    ref ulaFetchB1,
                    ref ulaFetchB2,
                    ref ulaFetchA1,
                    ref ulaFetchA2,
                    ref ulaFetchInk,
                    ref ulaFetchPaper);
        }

        private byte m_portFE = 0;
        public virtual byte PortFE 
        {
            get { return m_portFE; }
            set
            {
                m_portFE = value; 
                _borderColor = Palette[value & 7];
            } 
        }


        #region Private

        protected virtual void fillUlaTables(int MaxTakt)
        {
            int pitchWidth = c_ulaWidth;
            _ulaLineOffset = new int[MaxTakt];
            _ulaAddrBW = new int[MaxTakt];
            _ulaAddrAT = new int[MaxTakt];
            _ulaDo = new int[MaxTakt];

            int takt = 0;
            for (int line = 0; line < c_frameTactCount / c_ulaLineTime; line++)
                for (int pix = 0; pix < c_ulaLineTime; pix++, takt++)
                {
                    if ((line >= (c_ulaFirstPaperLine - c_ulaBorderTop)) && (line < (c_ulaFirstPaperLine + 192 + c_ulaBorderBottom)) &&
                        (pix >= (c_ulaFirstPaperTact - c_ulaBorderLeftT)) && (pix < (c_ulaFirstPaperTact + 128 + c_ulaBorderRightT))) // visibleArea
                    {
                        if ((line >= c_ulaFirstPaperLine) && (line < (c_ulaFirstPaperLine + 192)) &&
                            (pix >= c_ulaFirstPaperTact) && (pix < (c_ulaFirstPaperTact + 128)))  // paper
                        {
                            int sx, sy, ap, vp;
                            int scrPix = pix - c_ulaFirstPaperTact;
                            switch (scrPix & 7)
                            {
                                case 0:
                                    _ulaDo[takt] = 4;   // shift 1 + fetch B2
                                    
                                    sx = pix + 4 - c_ulaFirstPaperTact;  // +4 = prefetch!
                                    sy = line - c_ulaFirstPaperLine;
                                    sx >>= 2;
                                    ap = sx | ((sy >> 3) << 5);
                                    vp = sx | (sy << 5);
                                    _ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                    //_ulaAddrAT[takt] = 6144 + ap;
                                    break;
                                case 1:
                                    _ulaDo[takt] = 5;   // shift 1 + fetch A2
                                    
                                    sx = pix + 3 - c_ulaFirstPaperTact;  // +3 = prefetch!
                                    sy = line - c_ulaFirstPaperLine;
                                    sx >>= 2;
                                    ap = sx | ((sy >> 3) << 5);
                                    vp = sx | (sy << 5);
                                    //_ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                    _ulaAddrAT[takt] = 6144 + ap;
                                    break;
                                case 2:
                                    _ulaDo[takt] = 6;   // shift 1
                                    break;
                                case 3:
                                    _ulaDo[takt] = 7;   // shift 1 (last)
                                    break;
                                case 4:
                                    _ulaDo[takt] = 8;   // shift 2
                                    break;
                                case 5:
                                    _ulaDo[takt] = 8;   // shift 2
                                    break;
                                case 6:
                                    _ulaDo[takt] = pix < (c_ulaFirstPaperTact + 128-2)? 
                                        9 :             // shift 2 + fetch B2
                                        8;              // shift 2

                                    sx = pix + 2 - c_ulaFirstPaperTact;  // +2 = prefetch!
                                    sy = line - c_ulaFirstPaperLine;
                                    sx >>= 2;
                                    ap = sx | ((sy >> 3) << 5);
                                    vp = sx | (sy << 5);
                                    _ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                    //_ulaAddrAT[takt] = 6144 + ap;
                                    break;
                                case 7:
                                    _ulaDo[takt] = pix < (c_ulaFirstPaperTact + 128 - 2) ?
                                        10 :             // shift 2 + fetch A2
                                        8;               // shift 2

                                    sx = pix + 1 - c_ulaFirstPaperTact;  // +1 = prefetch!
                                    sy = line - c_ulaFirstPaperLine;
                                    sx >>= 2;
                                    ap = sx | ((sy >> 3) << 5);
                                    vp = sx | (sy << 5);
                                    //_ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                    _ulaAddrAT[takt] = 6144 + ap;
                                    break;
                            }
                        }
                        else if ((line >= c_ulaFirstPaperLine) && (line < (c_ulaFirstPaperLine + 192)) &&
                                 (pix == (c_ulaFirstPaperTact - 2)))  // border & fetch B1
                        {
                            _ulaDo[takt] = 2; // border & fetch B1

                            int sx = pix + 2 - c_ulaFirstPaperTact;  // +2 = prefetch!
                            int sy = line - c_ulaFirstPaperLine;
                            sx >>= 2;
                            int ap = sx | ((sy >> 3) << 5);
                            int vp = sx | (sy << 5);
                            _ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                            //_ulaAddrAT[takt] = 6144 + ap;
                        }
                        else if ((line >= c_ulaFirstPaperLine) && (line < (c_ulaFirstPaperLine + 192)) &&
                                 (pix == (c_ulaFirstPaperTact - 1)))  // border & fetch A1
                        {
                            _ulaDo[takt] = 3; // border & fetch A1

                            int sx = pix + 1 - c_ulaFirstPaperTact;  // +1 = prefetch!
                            int sy = line - c_ulaFirstPaperLine;
                            sx >>= 2;
                            int ap = sx | ((sy >> 3) << 5);
                            int vp = sx | (sy << 5);
                            //_ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                            _ulaAddrAT[takt] = 6144 + ap;
                        }
                        else _ulaDo[takt] = 1; // border

                        int wy = line - (c_ulaFirstPaperLine - c_ulaBorderTop);
                        int wx = (pix - (c_ulaFirstPaperTact - c_ulaBorderLeftT)) * 2;
                        _ulaLineOffset[takt] = wy * pitchWidth + wx;
                        //(videoParams.LinePitch * (videoParams.Height - 240) / 2) + ((videoParams.Width - 320) / 2); // if texture size > 320x240 -> center image 
                    }
                    else _ulaDo[takt] = 0;
                }

            shiftTable(ref _ulaDo, c_ulaIntBegin);
            shiftTable(ref _ulaAddrBW, c_ulaIntBegin);
            shiftTable(ref _ulaAddrAT, c_ulaIntBegin);
            shiftTable(ref _ulaLineOffset, c_ulaIntBegin);

            //{
            //    XmlDocument xml = new XmlDocument();
            //    XmlNode root = xml.AppendChild(xml.CreateElement("ULA"));
            //    for (int i = 0; i < c_frameTactCount; i++)
            //    {
            //        XmlElement xe = xml.CreateElement("Item");
            //        xe.SetAttribute("tact", i.ToString());
            //        xe.SetAttribute("do", _ulaDo[i].ToString("D2"));
            //        xe.SetAttribute("offset", _ulaLineOffset[i].ToString("D6"));
            //        xe.SetAttribute("y", (_ulaLineOffset[i] / pitchWidth).ToString("D3"));
            //        xe.SetAttribute("x", (_ulaLineOffset[i] % pitchWidth).ToString("D3"));
            //        root.AppendChild(xe);
            //    }
            //    xml.Save("_ulaDo.xml");
            //    //byte[] tmp = new byte[c_frameTactCount];
            //    //for (int i = 0; i < tmp.Length; i++)
            //    //    tmp[i] = (byte)_ulaDo[i];
            //    //using (FileStream fs = new FileStream("_ulaDo.dat", FileMode.Create, FileAccess.Write, FileShare.Read))
            //    //    fs.Write(tmp, 0, tmp.Length);
            //}

            _ulaInk = new uint[256 * 2];
            _ulaPaper = new uint[256 * 2];
            OnPaletteChanged();
        }

        protected virtual void OnPaletteChanged()
        {
            for (int atd = 0; atd < 256; atd++)
            {
                _ulaInk[atd] = Palette[(atd & 7) + ((atd & 0x40) >> 3)];
                _ulaPaper[atd] = Palette[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                if ((atd & 0x80) != 0)
                {
                    _ulaInk[atd + 256] = Palette[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                    _ulaPaper[atd + 256] = Palette[(atd & 7) + ((atd & 0x40) >> 3)];
                }
                else
                {
                    _ulaInk[atd + 256] = Palette[(atd & 7) + ((atd & 0x40) >> 3)];
                    _ulaPaper[atd + 256] = Palette[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                }
            }
        }

		private void shiftTable(ref int[] table, int shift)
		{
			int[] shiftedTable = new int[table.Length];
			for (int i = 0; i < table.Length; i++)
			{
				int shiftedIndex = i - shift;
				if (shiftedIndex < 0)
					shiftedIndex += table.Length;
				shiftedTable[shiftedIndex] = table[i];
			}
			table = shiftedTable;
		}

		protected virtual unsafe void fetchVideo(
            uint* bitmapBufPtr, 
            int startTact, 
            int endTact, 
            ref int ulaFetchB1,
            ref int ulaFetchB2,
            ref int ulaFetchA1,
            ref int ulaFetchA2,
            ref uint ulaFetchInk, 
            ref uint ulaFetchPaper)
        {
			if (bitmapBufPtr == null || _ulaDo == null)
                return;

            if (endTact > c_frameTactCount)
                endTact = c_frameTactCount;
            if (startTact > c_frameTactCount)
                startTact = c_frameTactCount;

            for (int takt = startTact; takt < endTact; takt++)
            {
                switch (_ulaDo[takt])
                {
                    case 0:     // no action
                        continue;
                    
                    case 1:     // border
                        bitmapBufPtr[_ulaLineOffset[takt]] = _borderColor;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = _borderColor;
                        continue;
                    
                    case 2:     // border & fetch B1
                        bitmapBufPtr[_ulaLineOffset[takt]] = _borderColor;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = _borderColor;

                        ulaFetchB1 = _ulaMemory[_ulaAddrBW[takt]];
                        break;
                    
                    case 3:     // border & fetch A1
                        bitmapBufPtr[_ulaLineOffset[takt]] = _borderColor;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = _borderColor;

                        ulaFetchA1 = _ulaMemory[_ulaAddrAT[takt]];
                        ulaFetchInk = _ulaInk[ulaFetchA1 + _flashState];
                        ulaFetchPaper = _ulaPaper[ulaFetchA1 + _flashState];
                        break;
                    
                    case 4:     // shift 1 & fetch B2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchB1 & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchB1 & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchB1 <<= 2;

                        ulaFetchB2 = _ulaMemory[_ulaAddrBW[takt]];
                        break;

                    case 5:     // shift 1 & fetch A2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchB1 & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchB1 & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchB1 <<= 2;

                        ulaFetchA2 = _ulaMemory[_ulaAddrAT[takt]];
                        break;

                    case 6:     // shift 1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchB1 & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchB1 & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchB1 <<= 2;
                        break;

                    case 7:     // shift 1 (last)
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchB1 & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchB1 & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchB1 <<= 2;

                        ulaFetchInk = _ulaInk[ulaFetchA2 + _flashState];
                        ulaFetchPaper = _ulaPaper[ulaFetchA2 + _flashState];
                        break;

                    case 8:     // shift 2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchB2 & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchB2 & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchB2 <<= 2;
                        break;

                    case 9:     // shift 2 & fetch B1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchB2 & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchB2 & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchB2 <<= 2;

                        ulaFetchB1 = _ulaMemory[_ulaAddrBW[takt]];
                        break;

                    case 10:     // shift 2 & fetch A1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchB2 & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchB2 & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchB2 <<= 2;

                        ulaFetchA1 = _ulaMemory[_ulaAddrAT[takt]];
                        ulaFetchInk = _ulaInk[ulaFetchA1 + _flashState];
                        ulaFetchPaper = _ulaPaper[ulaFetchA1 + _flashState];
                        break;
                }
            }
        }

        #endregion

        #region Palette

        public readonly uint[] Palette = new uint[16] // zx spectrum pallette
		{ 
            0xFF000000, 0xFF0000AA, 0xFFAA0000, 0xFFAA00AA, 
            0xFF00AA00, 0xFF00AAAA, 0xFFAAAA00, 0xFFAAAAAA,
            0xFF000000, 0xFF0000FF, 0xFFFF0000, 0xFFFF00FF, 
            0xFF00FF00, 0xFF00FFFF, 0xFFFFFF00, 0xFFFFFFFF,
		};

        #endregion
    }
}
