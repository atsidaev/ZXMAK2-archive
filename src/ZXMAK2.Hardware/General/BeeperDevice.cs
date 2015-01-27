﻿using System;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;

namespace ZXMAK2.Hardware.General
{
	public class BeeperDevice : SoundDeviceBase
	{
        public BeeperDevice()
        {
            Category = BusDeviceCategory.Sound;
            Name = "BEEPER";
            Description = "Standard Beeper";
        }

        
        #region IBusDevice

		public override void BusInit(IBusManager bmgr)
		{
			base.BusInit(bmgr);

			bmgr.SubscribeWrIo(0x0001, 0x0000, WritePortFE);
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
			UpdateDac(v, v);
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
	}
}
