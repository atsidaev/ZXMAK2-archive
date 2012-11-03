using System;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Engine.Devices
{
	public class BeeperDevice : SoundRendererDeviceBase
	{
		#region IBusDevice

		public override string Name { get { return "Standard Beeper"; } }
		public override string Description { get { return "Simple Standard Beeper"; } }
		public override BusCategory Category { get { return BusCategory.Sound; } }

		public override void BusInit(IBusManager bmgr)
		{
			base.BusInit(bmgr);

			bmgr.SubscribeWRIO(0x0001, 0x0000, WritePortFE);
		}

		#endregion

		#region Bus Handlers

		protected virtual void WritePortFE(ushort addr, byte value, ref bool iorqge)
		{
			//if (!iorqge)
			//	return;
			//iorqge = false;
			//PortFE = value;
			value &= 0x10;
			if (value == _portFE)
				return;
			_portFE = value;
			ushort v = _portFE != 0 ? m_dacValue1 : m_dacValue0;
			UpdateDAC(v, v);
		}

		#endregion

		protected override void  OnVolumeChanged(int oldVolume, int newVolume)
		{
			m_dacValue0 = 0;
			m_dacValue1 = (ushort)((0x7FFF * newVolume) / 100);
		}

		private int _portFE = 0;
		private ushort m_dacValue0 = 0;
		private ushort m_dacValue1 = 0x1FFF;


		public BeeperDevice()
		{
			Volume = 30;
		}
	}
}
