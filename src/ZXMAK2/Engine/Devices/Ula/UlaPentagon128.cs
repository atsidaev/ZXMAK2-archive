using System;

using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Memory;
using ZXMAK2.Configuration;
using System.Drawing;
using System.IO;


namespace ZXMAK2.Engine.Ula
{
    public class UlaPentagon128 : IUla, IBusListener
    {
        #region Constants

        // Pentagon 128K
        // Total Size:          448 x 320
        // Visible Size:        384 x 304 (72+256+56 x 64+192+48)
        // First Line Border:   16
        // First Line Paper:    80
        // Paper Lines:         192
        // Bottom Border Lines: 48

        private const int c_ulaLineTime = 224;
        private const int c_ulaFirstPaperLine = 80;
        private const int c_ulaFirstPaperTact = 68;      // 68 [32sync+36border+128scr+28border]
        private const int c_frameTactCount = 71680;

        private const int c_ulaBorderTop = 24;//64;
        private const int c_ulaBorderBottom = 24;//48;
        private const int c_ulaBorderLeftT = 16;//36;
        private const int c_ulaBorderRightT = 16;//28;

        private const int c_ulaIntLength = 32;

        private const int c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
        private const int c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);

        #endregion

        private IMemory m_memory;
        private int m_videoPage = 5;
        private int m_ramPage = 0;
        private bool m_lock = false;

        private int[] _ulaLineOffset;
        private int[] _ulaAddrBW;
        private int[] _ulaAddrAT;
        private int[] _ulaDo;
        private uint[] _ulaInk;
        private uint[] _ulaPaper;
        private int _ulaFetchBW = 0;
        private int _ulaFetchAT = 0;
        private uint _ulaFetchInk = 0;
        private uint _ulaFetchPaper = 0;
        
        private byte[] _ulaMemory;              // current video ram bank
        private int _lastFrameTact = 0;         // last processed tact
        private int _flashState = 0;            // flash attr state (0/256)
        private int _flashCounter = 0;          // flash attr counter
        private uint _borderColor = _zxpal[0];  // current border color
        private int[] _bitmapBufPtr = new int[640 * 480];


        public void Init(Config config)
        {
            fillUlaTables(c_frameTactCount);
        }

        public void BusInit(IBusManager bmgr)
        {
            bmgr.AddListenerWriteMemory(0xC000, 0x4000, writeMem4000);
            bmgr.AddListenerWriteMemory(0xC000, 0xC000, writeMemC000);

            bmgr.AddListenerWritePort(0x8002, 0x0000, writePort7FFD);
            bmgr.AddListenerWritePort(0x0001, 0x0000, writePortFE);
            bmgr.AddListenerReadPort(0x00FF, 0x00FF, readPortFF);
            
            bmgr.AddListenerReset(busReset);
        }

        private void busReset(long cpuTact)
        {
            m_lock = false;
            writePort7FFD(cpuTact, 0x7FFD, 0);
        }

        public void SetMemory(IMemory memory)
        {
            m_memory = memory;
            if (memory.GetRamImagePageCount() < 8)
                throw new ArgumentException(string.Format("Incompatible Memory Type: {0}", memory.GetType()));
            _ulaMemory = m_memory.RamPages[m_videoPage];
        }

        #region WRMEM

        public void writeMem4000(long cpuTact, ushort addr, byte value)
        {
            if (m_videoPage == 5)
                UpdateState((int)(cpuTact % FrameTactCount));
        }

        public void writeMemC000(long cpuTact, ushort addr, byte value)
        {
            if (m_videoPage == m_ramPage)
                UpdateState((int)(cpuTact % FrameTactCount));
        }

        #endregion

        #region WRPORT

        private void writePort7FFD(long cpuTact, ushort addr, byte value)
        {
            if (!m_lock)
            {
                UpdateState((int)(cpuTact % FrameTactCount));
                
                m_lock = (value & 0x20) != 0;
                m_ramPage = value & 7;
                m_videoPage = (value & 0x08) == 0 ? 5 : 7;
                _ulaMemory = m_memory.RamPages[m_videoPage];
            }
        }

        private void writePortFE(long cpuTact, ushort addr, byte value)
        {
            UpdateState((int)(cpuTact % FrameTactCount));
            _borderColor = _zxpal[value & 7];
        }

        #endregion

        #region RDPORT

        private void readPortFF(long cpuTact, ushort addr, ref byte value)
        {
            int frameTact = (int)(cpuTact % FrameTactCount);
            if (_ulaLineOffset[frameTact] >= 0 && _ulaAddrBW[frameTact] >= 0) // not border?
                value = _ulaMemory[_ulaAddrAT[frameTact]];
        }

        #endregion

        public int FrameTactCount { get { return c_frameTactCount; } }

        public int[] VideoBuffer
        {
            get { return _bitmapBufPtr; }
            set { _bitmapBufPtr = value; }
        }

        public Size VideoSize { get { return new Size(c_ulaWidth, c_ulaHeight); } }

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
        public unsafe void UpdateState(int frameTact)
        {
            if (frameTact < _lastFrameTact)
                frameTact = c_frameTactCount;
            fixed (int* ptr = _bitmapBufPtr)
                fetchVideo(
                    (uint*)ptr,
                    _lastFrameTact,
                    frameTact,
                    ref _ulaFetchBW,
                    ref _ulaFetchAT,
                    ref _ulaFetchInk,
                    ref _ulaFetchPaper);
            _lastFrameTact = frameTact;
        }

        public unsafe void BeginFrame(int frameTact)
        {
            //fixed (int* ptr = _bitmapBufPtr)
            //    fetchVideo(
            //        (uint*)ptr,
            //        0,
            //        frameTact,
            //        ref _ulaFetchBW,
            //        ref _ulaFetchAT,
            //        ref _ulaFetchInk,
            //        ref _ulaFetchPaper);
            //_lastFrameTact = frameTact;
            _lastFrameTact = 0;
        }

        #region Comment
        /// <summary>
        /// Fill video frame buffer to end
        /// </summary>
        #endregion Comment
        public void EndFrame()
        {
            UpdateState(c_frameTactCount);
            _lastFrameTact = 0;

            _flashCounter++;
            if (_flashCounter > 24)
            {
                _flashState ^= 256;
                _flashCounter = 0;
            }
        }

        public bool GetIntState(int frameTact)
        {
            return frameTact < c_ulaIntLength;
        }

        public unsafe void ForceRedrawFrame()
        {
            int ulaFetchBW = 0;
            int ulaFetchAT = 0;
            uint ulaFetchInk = 0;
            uint ulaFetchPaper = 0;

            fixed (int* ptr = _bitmapBufPtr)
                fetchVideo(
                    (uint*)ptr,
                    0,
                    c_frameTactCount,
                    ref ulaFetchBW,
                    ref ulaFetchAT,
                    ref ulaFetchInk,
                    ref ulaFetchPaper);
        }

        #region Comment
        /// <summary>
        /// draw screen on specified surface
        /// </summary>
        #endregion
        public unsafe void DrawIndependent(int* videoPtr)
        {
            if (videoPtr == (int*)0) return;

            int ulaFetchBW = 0;
            int ulaFetchAT = 0;
            uint ulaFetchInk = 0;
            uint ulaFetchPaper = 0;

            fetchVideo(
                (uint*)videoPtr,
                0,
                c_frameTactCount,
                ref ulaFetchBW,
                ref ulaFetchAT,
                ref ulaFetchInk,
                ref ulaFetchPaper);
        }

        #region Private

        private void fillUlaTables(int MaxTakt)
        {
            int pitchWidth = c_ulaWidth;
            _ulaLineOffset = new int[MaxTakt];
            _ulaAddrBW = new int[MaxTakt];
            _ulaAddrAT = new int[MaxTakt];
            _ulaDo = new int[MaxTakt];

            int takt = 0;
            for (int line = 0; line < 320; line++)
                for (int pix = 0; pix < 224; pix++, takt++)
                {
                    if ((line >= (c_ulaFirstPaperLine - c_ulaBorderTop)) && (line < (c_ulaFirstPaperLine + 192 + c_ulaBorderBottom)) &&
                        (pix >= (c_ulaFirstPaperTact - c_ulaBorderLeftT)) && (pix < (c_ulaFirstPaperTact + 128 + c_ulaBorderRightT))) // visibleArea
                    {
                        if ((line >= c_ulaFirstPaperLine) && (line < (c_ulaFirstPaperLine + 192)) &&
                            (pix >= c_ulaFirstPaperTact) && (pix < (c_ulaFirstPaperTact + 128)))  // paper
                        {
                            _ulaDo[takt] = 3;    // shift
                            if ((pix & 3) == 3)
                            {
                                _ulaDo[takt] = 4; // shift & fetch
                                int sx = pix + 1 - c_ulaFirstPaperTact;  // +1 = prefetch!
                                int sy = line - c_ulaFirstPaperLine;
                                sx >>= 2;
                                int ap = sx | ((sy >> 3) << 5);
                                int vp = sx | (sy << 5);
                                _ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
                                _ulaAddrAT[takt] = 6144 + ap;
                            }
                        }
                        else if ((line >= c_ulaFirstPaperLine) && (line < (c_ulaFirstPaperLine + 192)) &&
                                 (pix == (c_ulaFirstPaperTact - 1)))  // border & fetch
                        {
                            _ulaDo[takt] = 2; // border & fetch
                            int sx = pix + 1 - c_ulaFirstPaperTact;  // +1 = prefetch!
                            int sy = line - c_ulaFirstPaperLine;
                            sx >>= 2;
                            int ap = sx | ((sy >> 3) << 5);
                            int vp = sx | (sy << 5);
                            _ulaAddrBW[takt] = (vp & 0x181F) | ((vp & 0x0700) >> 3) | ((vp & 0x00E0) << 3);
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

            _ulaInk = new uint[256 * 2];
            _ulaPaper = new uint[256 * 2];
            for (int atd = 0; atd < 256; atd++)
            {
                _ulaInk[atd] = _zxpal[(atd & 7) + ((atd & 0x40) >> 3)];
                _ulaPaper[atd] = _zxpal[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                if ((atd & 0x80) != 0)
                {
                    _ulaInk[atd + 256] = _zxpal[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                    _ulaPaper[atd + 256] = _zxpal[(atd & 7) + ((atd & 0x40) >> 3)];
                }
                else
                {
                    _ulaInk[atd + 256] = _zxpal[(atd & 7) + ((atd & 0x40) >> 3)];
                    _ulaPaper[atd + 256] = _zxpal[((atd >> 3) & 7) + ((atd & 0x40) >> 3)];
                }
            }

        }

        private unsafe void fetchVideo(uint* bitmapBufPtr, int startTact, int endTact, ref int ulaFetchBW, ref int ulaFetchAT, ref uint ulaFetchInk, ref uint ulaFetchPaper)
        {
            if (bitmapBufPtr == null)
                return;

            if (_ulaDo == null)	// VideoParams not set!
                return;

            if (endTact > c_frameTactCount)
                endTact = c_frameTactCount;
            if (startTact > c_frameTactCount)
                startTact = c_frameTactCount;
            if (startTact < c_ulaLineTime * (c_ulaFirstPaperLine - c_ulaBorderTop))
                startTact = c_ulaLineTime * (c_ulaFirstPaperLine - c_ulaBorderTop);

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
                    case 2:     // border&fetch
                        bitmapBufPtr[_ulaLineOffset[takt]] = _borderColor;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = _borderColor;
                        ulaFetchBW = _ulaMemory[_ulaAddrBW[takt]];
                        ulaFetchAT = _ulaMemory[_ulaAddrAT[takt]];
                        ulaFetchInk = _ulaInk[ulaFetchAT + _flashState];
                        ulaFetchPaper = _ulaPaper[ulaFetchAT + _flashState];
                        break;
                    case 3:     // shift
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchBW & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchBW & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchBW <<= 2;
                        break;
                    case 4:     // shift & fetch
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaFetchBW & 0x80) != 0) ? ulaFetchInk : ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaFetchBW & 0x40) != 0) ? ulaFetchInk : ulaFetchPaper;
                        ulaFetchBW <<= 2;
                        ulaFetchBW = _ulaMemory[_ulaAddrBW[takt]];
                        ulaFetchAT = _ulaMemory[_ulaAddrAT[takt]];
                        ulaFetchInk = _ulaInk[ulaFetchAT + _flashState];
                        ulaFetchPaper = _ulaPaper[ulaFetchAT + _flashState];
                        break;
                }
            }
        }

        #endregion

        #region Private Static

        private static uint[] _zxpal = new uint[16] // zx spectrum pallette
		{ 
			0xFF000000, 0xFF0000C0, 0xFFC00000, 0xFFC000C0, 0xFF00C000, 0xFF00C0C0, 
			0xFFC0C000, 0xFFC0C0C0,
			0xFF000000, 0xFF0000FF, 0xFFFF0000, 0xFFFF00FF, 0xFF00FF00, 0xFF00FFFF, 
			0xFFFFFF00, 0xFFFFFFFF 
		};

        #endregion
    }
}
