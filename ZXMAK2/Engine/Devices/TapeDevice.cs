/// Description: Tape emulator
/// Author: Alex Makeev
/// Date: 16.04.2007
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

using ZXMAK2.Engine.Serializers;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Serializers.TapeSerializers;
using ZXMAK2.Engine.Z80;
using System.Xml;


namespace ZXMAK2.Engine.Devices
{
    public class TapeDevice : BusDeviceBase, ISoundRenderer, IConfigurable, ITapeDevice
	{
        #region IBusDevice

        public override string Name { get { return "Tape Player"; } }
        public override string Description { get { return "Generic Tape Device"; } }
        public override BusCategory Category { get { return BusCategory.Tape; } }

        public override void BusInit(IBusManager bmgr)
        {
			m_cpu = bmgr.CPU;
			m_memory = bmgr.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
            IUlaDevice ula = (IUlaDevice)bmgr.FindDevice(typeof(IUlaDevice));
            m_frameTactCount = ula.FrameTactCount;
			
            bmgr.SubscribeRDIO(0x0001, 0x0000, readPortFE);
            
            bmgr.SubscribePreCycle(busPreCycle);
            bmgr.SubscribeBeginFrame(busBeginFrame);
            bmgr.SubscribeEndFrame(busEndFrame);
	
			bmgr.AddSerializer(new TapSerializer(this));
            bmgr.AddSerializer(new TzxSerializer(this));
            bmgr.AddSerializer(new CswSerializer(this));
        }

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        #endregion

        #region ITapeDevice

        public bool TrapsAllowed
        {
            get { return m_trapsAllowed; }
            set { m_trapsAllowed = value; }
        }
        
        #endregion

        #region ISoundRenderer Members

        public uint[] AudioBuffer 
        { 
            get { return m_audioBuffer; } 
        }

        public int Volume
        {
            get { return m_volume; }
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 100)
                    value = 100;
                m_volume = value;
                m_dacValue0 = 0;
                m_dacValue1 = (uint)((0x1FFF * m_volume) / 100);
            }
        }

        #endregion

        #region IConfigurable

        public void LoadConfig(XmlNode itemNode)
        {
            Volume = Utils.GetXmlAttributeAsInt32(itemNode, "volume", Volume);
        }

        public void SaveConfig(XmlNode itemNode)
        {
            Utils.SetXmlAttribute(itemNode, "volume", Volume);
        }

        #endregion

        #region Bus Handlers

        private void readPortFE(ushort addr, ref byte value, ref bool iorqge)
		{
			//if (!iorqge)
			//    return;
			//iorqge = false;
			value &= 0xBF;
            value |= (byte)(GetTapeBit(m_cpu.Tact) ? 0x40 : 0x00);
		}

		private void tapeTrap(ushort addr, ref byte value)
		{
            if (!IsPlay || !TrapsAllowed || !m_memory.IsRom48)
				return;

			TapeBlock tb = Blocks[CurrentBlock];
			if (tb.TapData == null)
				return;

			ushort rIX = m_cpu.regs.IX;
			ushort rDE = m_cpu.regs.DE;
			if (rDE != (tb.TapData.Length - 2)) 
				return;

			int offset = 0;
			byte crc = 0;
			crc = tb.TapData[offset++];
			for (int i = 0; i < rDE; i++)
			{
				crc ^= tb.TapData[offset];
				m_cpu.WRMEM(rIX++, tb.TapData[offset++]);
			}
			crc ^= tb.TapData[offset];

			m_cpu.regs.PC = 0x05DF-1;
			m_cpu.regs.IX = rIX;
			m_cpu.regs.DE = 0;
			m_cpu.regs.H = crc;
			m_cpu.regs.L = tb.TapData[offset];
			m_cpu.regs.BC = 0xB001;

			int newBlock = CurrentBlock+1;
            if (newBlock < Blocks.Count)
            {
                CurrentBlock = newBlock;
            }
            else
            {
                Stop();
                Rewind();
            }

            

			//tMask=0x80;
			//byteptr = 0;
			//cntr = 0;

			//NextBlock();
			//BlockEnd = true;
		}

        private void busPreCycle(int frameTact)
        {
            if (!IsPlay)
                return;
            //bmgr.SubscribeRDMEM_M1(0xFFFF, 0x056B, tapeTrap);
            //bmgr.SubscribeRDMEM_M1(0xFFFF, 0x059E, tapeTrap);

            UpdateState(frameTact);

            ushort addr = m_cpu.regs.PC;
            if (!TrapsAllowed || !m_memory.IsRom48 || !(addr==0x056B || addr==0x059E))
                return;

            TapeBlock tb = Blocks[CurrentBlock];
            if (tb.TapData == null)
                return;

            ushort rIX = m_cpu.regs.IX;
            ushort rDE = m_cpu.regs.DE;
            if (rDE != (tb.TapData.Length - 2))
                return;

            int offset = 0;
            byte crc = 0;
            crc = tb.TapData[offset++];
            for (int i = 0; i < rDE; i++)
            {
                crc ^= tb.TapData[offset];
                m_cpu.WRMEM(rIX++, tb.TapData[offset++]);
            }
            crc ^= tb.TapData[offset];

            m_cpu.regs.PC = 0x05DF;//0x05DF - 1;
            m_cpu.regs.IX = rIX;
            m_cpu.regs.DE = 0;
            m_cpu.regs.H = crc;
            m_cpu.regs.L = tb.TapData[offset];
            m_cpu.regs.BC = 0xB001;

            int newBlock = CurrentBlock + 1;
            if (newBlock < Blocks.Count)
            {
                CurrentBlock = newBlock;
            }
            else
            {
                Stop();
                Rewind();
            }
        }
        
        private void busBeginFrame()
        {
            m_samplePos = 0;
        }

        private void busEndFrame()
        {
            UpdateState(m_frameTactCount);
        }

		#endregion

        protected void UpdateState(int frameTact)
        {
            int tp = (m_audioBuffer.Length * frameTact / m_frameTactCount);
            if (tp > m_audioBuffer.Length) tp = m_audioBuffer.Length;
            if (tp > m_samplePos)
            {
                uint val = m_dacValue0;
                if (GetTapeBit(m_cpu.Tact))
                    val += m_dacValue1;
                val = val | (val << 16);

                for (; m_samplePos < tp; m_samplePos++)
                    m_audioBuffer[m_samplePos] = val;
            }
        }



		private Z80CPU m_cpu;
		private IMemoryDevice m_memory;
        private bool m_trapsAllowed = true;
        
        private uint[] m_audioBuffer = new uint[882];
        private int m_volume = 100;
        private uint m_dacValue0 = 0;
        private uint m_dacValue1 = 0x1FFF;
        private int m_frameTactCount;
        private int m_samplePos = 0;


        #region private data
		private long _Z80FQ = 3500000;

		private List<TapeBlock> _blocks = new List<TapeBlock>();
		private int _index = 0;
		private int _playPosition = 0;

		private bool _play = false;

		private long _lastTact = 0;
		private int _waitEdge = 0;
		private int _state = 0;
		#endregion


		public TapeDevice()
		{
			_Z80FQ = 3500000;
            Volume = 5;
		}

		public long Z80FQ { get { return _Z80FQ; } }
		public List<TapeBlock> Blocks
		{
			get { return _blocks; }
			set { _blocks = value; }
		}
		
        public int CurrentBlock
		{
			get
			{
				if (_blocks.Count > 0)
					return _index;
				else
					return -1;
			}
			set
			{
				if (value == _playPosition)
					return;
				if (value >= 0 && value < _blocks.Count)
				{
					_index = value;
					_playPosition = 0;
					raiseTapeStateChanged();
					//if(Play)
					//   _currentBlock = _blocks[_index] as TapeBlock;
				}
			}
		}

		public event EventHandler TapeStateChanged;
		private void raiseTapeStateChanged()
		{
			if (TapeStateChanged != null)
				TapeStateChanged(this, new EventArgs());
		}

		public int Position
		{
			get
			{
				if (_playPosition >= _blocks[_index].Periods.Count)
					return 0;
				return _playPosition;
			}
		}

        public int TactsPerSecond { get { return (int)Z80FQ; } }



		#region private methods
		private int tape_bit(long globalTact)
		{
			int delta = (int)(globalTact - _lastTact);

			if (!_play)
			{
				_lastTact = globalTact;
				return 0;
			}
			if (_index < 0) //???
			{
				_play = false;
				raiseTapeStateChanged();
				return _state;
			}

			while (delta >= _waitEdge)
			{
				delta -= _waitEdge;
				_state ^= -1;

				_playPosition++;
				if (_playPosition >= _blocks[_index].Periods.Count) // endof block?
				{
					while (_playPosition >= _blocks[_index].Periods.Count)   // skip empty blocks
					{
						_playPosition = 0;
						_index++;
						if (_index >= _blocks.Count) break;
					}
					if (_index >= _blocks.Count)  // end of tape -> rewind & stop
					{
						_lastTact = globalTact;
						_index = 0;
						_play = false;
						raiseTapeStateChanged();
						return _state;
					}
					raiseTapeStateChanged();
				}
				_waitEdge = _blocks[_index].Periods[_playPosition];
			}
			_lastTact = globalTact - (long)delta;
			return _state;
		}
		#endregion

		#region public methods
		
        protected bool GetTapeBit(long globalTact)
		{
			return tape_bit(globalTact)!=0;
		}

		public bool IsPlay { get { return _play; } }
		//public long LastProcessedTact { get { return _lastTact; } }

		public void Reset()  // loaded new image
		{
			_waitEdge = 0;
			_index = -1;
			if (_blocks.Count > 0)
				_index = 0;
			_playPosition = 0;
			_play = false;
			raiseTapeStateChanged();
		}

		public void Rewind()
		{
			_lastTact = m_cpu.Tact;
			_waitEdge = 0;
			_index = -1;
			if (_blocks.Count > 0)
				_index = 0;
			_playPosition = 0;
			_play = false;
			raiseTapeStateChanged();
		}
		
        public void Play()
		{
            _lastTact = m_cpu.Tact;
			if (_blocks.Count > 0 && _index >= 0)
			{
				while (_playPosition >= _blocks[_index].Periods.Count)
				{
					_playPosition = 0;
					_index++;
					if (_index >= _blocks.Count) break;
				}
				if (_index >= _blocks.Count)  // end of tape -> rewind & stop
				{
					_index = -1;
					return;
				}
				//if (_playPosition >= _blocks[_index].Periods.Count)
				//   _playPosition = 0;

				_state ^= -1;
				_waitEdge = _blocks[_index].Periods[_playPosition];
				_play = true;
				raiseTapeStateChanged();
			}
		}
		public void Stop()
		{
            _lastTact = m_cpu.Tact;
			_play = false;
			raiseTapeStateChanged();
		}
		#endregion
    }
}
