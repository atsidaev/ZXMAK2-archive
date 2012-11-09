using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Hardware.Profi
{
	public class UlaProfi3XX : UlaDeviceBase
	{
		#region IBusDevice

		public override string Name { get { return "PROFI 3.xx"; } }

		public override void BusInit(IBusManager bmgr)
		{
			base.BusInit(bmgr);
			bmgr.SubscribeRDIO(0x0000, 0x0000, ReadPortAll);
		}

		#endregion

		#region UlaDeviceBase

		public override byte PortFE
		{
			set
			{
				base.PortFE = value;
				//if (Memory != null && (Memory.CMR1 & 0x80) != 0)
				//    m_borderAttr = value & 0x0F;
				//else
				//    m_borderAttr = value & 7;
				_borderColor = Palette[value & 7];
			}
		}

		protected override void OnPaletteChanged()
		{
			base.OnPaletteChanged();
			for (int i = 0; i < 0x100; i++)
			{
				_ulaProfiInk[i] = Palette[(i & 7) | ((i >> 3) & 0x8)];
				_ulaProfiPaper[i] = Palette[((i >> 3) & 7) | ((i >> 4) & 0x8)];
			}
		}

		public override float VideoHeightScale
		{
			get { return m_profiMode ? 2F : base.VideoHeightScale; }
		}

		#endregion

		#region Bus Handlers

		protected override void WriteMem0000(ushort addr, byte value)
		{
			if (m_profiMode && (m_pageBw == m_page0000 || m_pageClr == m_page0000))
				UpdateState((int)(CPU.Tact % FrameTactCount));
			else
				base.WriteMem0000(addr, value);
		}

		protected override void WriteMem4000(ushort addr, byte value)
		{
			if (m_profiMode && (m_pageBw == m_page4000 || m_pageClr == m_page4000))
				UpdateState((int)(CPU.Tact % FrameTactCount));
			else
				base.WriteMem4000(addr, value);
		}

		protected override void WriteMem8000(ushort addr, byte value)
		{
			if (m_profiMode && (m_pageBw == m_page8000 || m_pageClr == m_page8000))
				UpdateState((int)(CPU.Tact % FrameTactCount));
			else
				base.WriteMem8000(addr, value);
		}

		protected override void WriteMemC000(ushort addr, byte value)
		{
			if (m_profiMode && (m_pageBw == m_pageC000 || m_pageClr == m_pageC000))
				UpdateState((int)(CPU.Tact % FrameTactCount));
			else
				base.WriteMemC000(addr, value);
		}

		protected virtual void ReadPortAll(ushort addr, ref byte value, ref bool iorqge)
		{
			if ((addr & 0xFF) == 0xFF)
			{
				// Port #FF emulation
				int frameTact = (int)((CPU.Tact - 1) % FrameTactCount);
				base.ReadPortFF(frameTact, ref value);
			}
		}

		#endregion

		#region Constants

		private int _ulaProfiTactsPerLine;		// tacts per line
		private int _ulaProfiScreenBeginTact;	// tact for left top pixel
		private int _ulaProfiLineBeginTact;		// tact for left pixel in line
		public int c_ulaProfiBorderLeftT;
		public int c_ulaProfiBorderRightT;
		public bool c_ulaProfiColor = false;

		private int _ulaProfiWidth;
		private int _ulaProfiHeight;

		#endregion

		private bool m_profiMode = false;
		private int m_pageBw = 4;
		private int m_pageClr = 0x38;
		private byte[] _memoryCpmUlaBw;				// current b/w video ram for CPM mode
		private byte[] _memoryCpmUlaClr;			// current color video ram for CPM mode


		public UlaProfi3XX()
		{
			// PROFI 3.2
			// Total Size:          768 x 312
			// Visible Size:        640 x 240 (64+512+64 x 0+240+0)
			// SYNCGEN: SAMX6 (original)

			c_ulaLineTime = 224;
			c_ulaFirstPaperLine = 56;
			c_ulaFirstPaperTact = 42;
			c_frameTactCount = 69888;	// 59904 for profi mode (312x192)

			c_ulaBorderTop = 24;
			c_ulaBorderBottom = 24;
			c_ulaBorderLeftT = 16;
			c_ulaBorderRightT = 16;

			c_ulaIntBegin = 0;
			c_ulaIntLength = 32 + 7;	//  needs approve

			// profi mode timings...
			_ulaProfiTactsPerLine = 192;			// tacts per line
			_ulaProfiScreenBeginTact = 72 * 192;	// tact for left top pixel
			_ulaProfiLineBeginTact = 44;			// tact for left pixel in line
			c_ulaProfiBorderLeftT = 16;			// real 3.xx=6
			c_ulaProfiBorderRightT = 16;		// real 3.xx=10
			c_ulaProfiColor = false;

			c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
			c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);

			_ulaProfiWidth = (c_ulaProfiBorderLeftT + 128 + c_ulaProfiBorderRightT) * 4;
			_ulaProfiHeight = 240;

			_ulaProfiInk = new uint[0x100];
			_ulaProfiPaper = new uint[0x100];
		}

		public override void SetPageMapping(int videoPage, int page0000, int page4000, int page8000, int pageC000)
		{
			base.SetPageMapping(videoPage, page0000, page4000, page8000, pageC000);
			m_profiMode = false;
		}

		public void SetPageMappingProfi(int videoPage, int page0000, int page4000, int page8000, int pageC000, bool ds80)
		{
			SetPageMapping(videoPage, page0000, page4000, page8000, pageC000);
			if (Memory.RamPages.Length < 0x40)
				return;

			m_profiMode = ds80;
			bool polek = videoPage == 7;
			m_pageBw = polek ? 0x06 : 0x04;
			m_pageClr = polek ? 0x3A : 0x38;
			_memoryCpmUlaBw = Memory.RamPages[m_pageBw];
			_memoryCpmUlaClr = Memory.RamPages[m_pageClr];
			//_ulaMemory =        polek ? Memory.RamPages[7]    : Memory.RamPages[5];
		}

		public override System.Drawing.Size VideoSize
		{
			get
			{
				if (!m_profiMode)
					return base.VideoSize;
				return new System.Drawing.Size(_ulaProfiWidth, _ulaProfiHeight);
			}
		}

		protected override unsafe void fetchVideo(
			uint* bitmapBufPtr,
			int startTact,
			int endTact,
			UlaStateBase ulaState)
		{
			if (m_profiMode)
				fetchVideoProfi(bitmapBufPtr, startTact, endTact, _ulaStateProfi);
			else
				base.fetchVideo(
					bitmapBufPtr,
					startTact,
					endTact,
					ulaState);
		}


		#region Profi Update

		private class UlaStateProfi : UlaStateBase
		{
			//public byte ulaFetchBW = 0;
			//public byte ulaFetchAT = 0;
			//public uint ulaFetchInk = 0;
			//public uint ulaFetchPaper = 0;
		}

		private UlaStateProfi _ulaStateProfi = new UlaStateProfi();

		private unsafe delegate void UlaDoDelegate(uint* buffer, int tact, UlaStateProfi state);
		private UlaDoDelegate[] _ulaDoTable;
		private int[] _ulaBwOffset;
		private int[] _ulaVideoOffset;
		private uint[] _ulaProfiInk;
		private uint[] _ulaProfiPaper;

		protected unsafe override void OnTimingChanged()
		{
			base.OnTimingChanged();
			// rebuild tables...
			_ulaDoTable = new UlaDoDelegate[c_frameTactCount];
			_ulaBwOffset = new int[c_frameTactCount];
			_ulaVideoOffset = new int[c_frameTactCount];

			for (int tact = 0; tact < c_frameTactCount; tact++)
			{
				int scrtact = tact - (_ulaProfiScreenBeginTact + _ulaProfiLineBeginTact - c_ulaProfiBorderLeftT);

				int line = scrtact / _ulaProfiTactsPerLine;
				if (line < 0 || line >= _ulaProfiHeight)
				{
					_ulaDoTable[tact] = null;
					continue;
				}

				int lineTact = scrtact % _ulaProfiTactsPerLine;
				if (lineTact < 0 || lineTact >= (c_ulaProfiBorderLeftT + 128 + c_ulaProfiBorderRightT))
				{
					_ulaDoTable[tact] = null;
					continue;
				}

				if (lineTact < c_ulaProfiBorderLeftT || lineTact >= (c_ulaProfiBorderLeftT + 128))
				{
					_ulaVideoOffset[tact] = line * _ulaProfiWidth + lineTact * 4;
					if (_ulaVideoOffset[tact] < 0 || _ulaVideoOffset[tact] >= (1024 * 768 - 4))
						throw new IndexOutOfRangeException();
					_ulaDoTable[tact] = ulaDoProfi32_0;
					continue;
				}
				lineTact -= c_ulaProfiBorderLeftT;


				int x4 = lineTact;// -_ulaProfiLineBeginTact;
				if (x4 < 0 || x4 >= 512 / 4)
				{
					_ulaDoTable[tact] = null;
					continue;
				}

				_ulaVideoOffset[tact] = line * _ulaProfiWidth + c_ulaProfiBorderLeftT * 4 + x4 * 4;
				if (_ulaVideoOffset[tact] < 0 || _ulaVideoOffset[tact] >= (1024 * 768 - 4))
					throw new IndexOutOfRangeException();

				int __PixCOFF = 2048 * (line >> 6) + 256 * (line & 0x07) + ((line & 0x38) << 2);

				if ((x4 & 2) == 0)
					_ulaBwOffset[tact] = __PixCOFF + x4 / 4 + 8192;
				else
					_ulaBwOffset[tact] = __PixCOFF + x4 / 4;


				if ((x4 & 1) == 0)
					_ulaDoTable[tact] = c_ulaProfiColor ? (UlaDoDelegate)ulaDoProfi32_1_CLR : (UlaDoDelegate)ulaDoProfi32_1_BNW;
				else
					_ulaDoTable[tact] = c_ulaProfiColor ? (UlaDoDelegate)ulaDoProfi32_2_CLR : (UlaDoDelegate)ulaDoProfi32_2_BNW;
			}
		}

		private unsafe void ulaDoProfi32_0(uint* buffer, int tact, UlaStateProfi state)
		{
			int bufOffset = _ulaVideoOffset[tact];
			uint ink = Palette[(~PortFE) & 7];
			buffer[bufOffset + 0] = ink;
			buffer[bufOffset + 1] = ink;
			buffer[bufOffset + 2] = ink;
			buffer[bufOffset + 3] = ink;
		}

		private unsafe void ulaDoProfi32_1_CLR(uint* buffer, int tact, UlaStateProfi state)
		{
			int offset = _ulaBwOffset[tact];
			int shr = _memoryCpmUlaBw[offset];
			int attr = _memoryCpmUlaClr[offset];
			uint ink = _ulaProfiInk[attr]; //Palette[(attr & 7) | ((attr >> 3)&0x8)]; // attr];
			uint paper = _ulaProfiPaper[attr]; //Palette[((attr >> 3) & 7) | ((attr >> 4) & 0x8)]; // attr | ((attr & 0x80) >> 1)];

			int bufOffset = _ulaVideoOffset[tact];
			buffer[bufOffset + 0] = ((shr & 0x80) != 0) ? ink : paper;
			buffer[bufOffset + 1] = ((shr & 0x40) != 0) ? ink : paper;
			buffer[bufOffset + 2] = ((shr & 0x20) != 0) ? ink : paper;
			buffer[bufOffset + 3] = ((shr & 0x10) != 0) ? ink : paper;
		}

		private unsafe void ulaDoProfi32_2_CLR(uint* buffer, int tact, UlaStateProfi state)
		{
			int offset = _ulaBwOffset[tact];
			int shr = _memoryCpmUlaBw[offset];
			int attr = _memoryCpmUlaClr[offset];
			uint ink = _ulaProfiInk[attr]; //Palette[(attr & 7) | ((attr >> 3)&0x8)]; // attr];
			uint paper = _ulaProfiPaper[attr]; //Palette[((attr >> 3) & 7) | ((attr >> 4) & 0x8)]; // attr | ((attr & 0x80) >> 1)];

			int bufOffset = _ulaVideoOffset[tact];
			buffer[bufOffset + 0] = ((shr & 0x08) != 0) ? ink : paper;
			buffer[bufOffset + 1] = ((shr & 0x04) != 0) ? ink : paper;
			buffer[bufOffset + 2] = ((shr & 0x02) != 0) ? ink : paper;
			buffer[bufOffset + 3] = ((shr & 0x01) != 0) ? ink : paper;
		}

		private unsafe void ulaDoProfi32_1_BNW(uint* buffer, int tact, UlaStateProfi state)
		{
			int offset = _ulaBwOffset[tact];
			int shr = _memoryCpmUlaBw[offset];
			uint ink = Palette[PortFE & 0x7];
			uint paper = Palette[(~PortFE) & 0x7];

			int bufOffset = _ulaVideoOffset[tact];
			buffer[bufOffset + 0] = ((shr & 0x80) != 0) ? ink : paper;
			buffer[bufOffset + 1] = ((shr & 0x40) != 0) ? ink : paper;
			buffer[bufOffset + 2] = ((shr & 0x20) != 0) ? ink : paper;
			buffer[bufOffset + 3] = ((shr & 0x10) != 0) ? ink : paper;
		}

		private unsafe void ulaDoProfi32_2_BNW(uint* buffer, int tact, UlaStateProfi state)
		{
			int offset = _ulaBwOffset[tact];
			int shr = _memoryCpmUlaBw[offset];
			uint ink = Palette[PortFE & 0x7];
			uint paper = Palette[(~PortFE) & 0x7];

			int bufOffset = _ulaVideoOffset[tact];
			buffer[bufOffset + 0] = ((shr & 0x08) != 0) ? ink : paper;
			buffer[bufOffset + 1] = ((shr & 0x04) != 0) ? ink : paper;
			buffer[bufOffset + 2] = ((shr & 0x02) != 0) ? ink : paper;
			buffer[bufOffset + 3] = ((shr & 0x01) != 0) ? ink : paper;
		}

		private unsafe void fetchVideoProfi(uint* bitmapBufPtr, int startTact, int endTact, UlaStateProfi ulaState)
		{
			if (bitmapBufPtr == null || _ulaDoTable == null)
				return;

			if (endTact > FrameTactCount)
				endTact = FrameTactCount;
			if (startTact > FrameTactCount)
				startTact = FrameTactCount;

			for (int i = startTact; i < endTact; i++)
				if (_ulaDoTable[i] != null)
					_ulaDoTable[i](bitmapBufPtr, i, ulaState);
		}

		#endregion

		//private int m_lastChange = 0;
		//private int m_frame = 0;
		//protected override void EndFrame()
		//{
		//    m_frame++;
		//    if (m_frame < m_lastChange+50)
		//        return;
		//    base.EndFrame();
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F1))
		//    {
		//        m_lastChange = m_frame;
		//        _ulaProfiTactsPerLine--;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F2))
		//    {
		//        m_lastChange = m_frame;
		//        _ulaProfiTactsPerLine++;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F3))
		//    {
		//        m_lastChange = m_frame;
		//        _ulaProfiLineBeginTact--;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F4))
		//    {
		//        m_lastChange = m_frame;
		//        _ulaProfiLineBeginTact++;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F5))
		//    {
		//        m_lastChange = m_frame;
		//        _ulaProfiScreenBeginTact -= _ulaProfiTactsPerLine;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F6))
		//    {
		//        m_lastChange = m_frame;
		//        _ulaProfiScreenBeginTact += _ulaProfiTactsPerLine;
		//        OnTimingChanged();
		//    }
		//}
		//private static bool IsKeyPressed(System.Windows.Forms.Keys key)
		//{
		//    return (GetKeyState((int)key) & 0xFF00) != 0;
		//}
		//[System.Runtime.InteropServices.DllImport("user32")]
		//private static extern short GetKeyState(int vKey);
	}
}
