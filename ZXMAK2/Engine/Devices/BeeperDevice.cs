using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Z80;

namespace ZXMAK2.Engine.Devices
{
	public class BeeperDevice : BusDeviceBase, ISoundRenderer, IConfigurable
	{
		#region IBusDevice

		public override string Name { get { return "Standard Beeper"; } }
		public override string Description { get { return "Simple Standard Beeper"; } }
		public override BusCategory Category { get { return BusCategory.Sound; } }

		public override void BusInit(IBusManager bmgr)
		{
			m_cpu = bmgr.CPU;
			IUlaDevice ula = (IUlaDevice)bmgr.FindDevice(typeof(IUlaDevice));
			FrameTactCount = ula.FrameTactCount;
			//LogAgent.Debug("Beeper.FrameTactCount = {0}", FrameTactCount);

			bmgr.SubscribeWRIO(0x0001, 0x0000, WritePortFE);

			bmgr.SubscribeBeginFrame(BeginFrame);
			bmgr.SubscribeEndFrame(EndFrame);
		}

		public override void BusConnect()
		{
		}

		public override void BusDisconnect()
		{
		}

		#endregion

		#region ISoundRenderer

		public uint[] AudioBuffer
		{
			get
			{
				//LogAgent.Debug("GetBuffer");
				return _beeperSamples;
			}
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
				m_dacValue1 = (uint)((0x7FFF * m_volume) / 100);
			}
		}

		#endregion

		#region IConfigurable Members

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

		protected virtual void WritePortFE(ushort addr, byte value, ref bool iorqge)
		{
			//if (!iorqge)
			//	return;
			//iorqge = false;
			PortFE = value;
		}

		protected virtual void BeginFrame()
		{
			//int frameTact = (int)((m_cpu.Tact + 1) % FrameTactCount);
			//LogAgent.Debug("BeginFrame @ {0}T", frameTact);
			_beeperSamplePos = 0;
		}

		protected virtual void EndFrame()
		{
			UpdateState(_frameTactCount);
			//int frameTact = (int)((m_cpu.Tact + 1) % FrameTactCount);
			//LogAgent.Debug("EndFrame @ {0}T", frameTact);
		}

		#endregion


		private Z80CPU m_cpu;

		private int m_volume = 100;
		private int _frameTactCount = 0;
		private int _beeperSamplePos = 0;
		private uint[] _beeperSamples = new uint[882];    // beeper frame sound samples
		//private bool _tapeOutSoundEnable = true;
		//private bool _tapeInSoundEnable = true;
		private int _portFE = 0;
		private uint m_dacValue0 = 0;
		private uint m_dacValue1 = 0x1FFF;


		public BeeperDevice()
		{
			Volume = 30;
		}

		public int PortFE
		{
			get { return _portFE; }
			set
			{
				if (value != _portFE)
				{
					int frameTact = (int)((m_cpu.Tact + 1) % FrameTactCount);
					UpdateState(frameTact);
					_portFE = value;
				}
			}
		}

		protected int FrameTactCount
		{
			get { return _frameTactCount; }
			set { _frameTactCount = value; }
		}


		public unsafe void UpdateState(int frameTact)
		{
			//LogAgent.Debug("UpdateState @ {0}T", frameTact);

			int tp = (_beeperSamples.Length * frameTact / _frameTactCount);
			if (tp > _beeperSamples.Length) tp = _beeperSamples.Length;
			if (tp > _beeperSamplePos)
			{
				uint val = m_dacValue0;
				if ((_portFE & 0x10) != 0)    // beeper output
					val = m_dacValue1;
				val = val | (val << 16);

				fixed (uint* pSamples = _beeperSamples)
					for (; _beeperSamplePos < tp; _beeperSamplePos++)
						pSamples[_beeperSamplePos] = val;
			}
		}
	}
}
