using System;
using ZXMAK2.Interfaces;


namespace ZXMAK2.Engine
{
	public abstract class BusDeviceBase
	{
		public abstract string Name { get; }
		public abstract string Description { get; }
		public abstract BusCategory Category { get; }
		public int BusOrder { get; set; }

		#region Comment
		/// <summary>
		/// Collect information about device. Add handlers & serializers here.
		/// </summary>
		#endregion
		public abstract void BusInit(IBusManager bmgr);

		#region Comment
		/// <summary>
		/// Called after Init, before device will be used. Add load files here
		/// </summary>
		#endregion
		public abstract void BusConnect();

		#region Comment
		/// <summary>
		/// Called when device using finished. Add flush & close files here
		/// </summary>
		#endregion
		public abstract void BusDisconnect();

		#region Comment
		/// <summary>
		/// Called to reset device to initial state (before load snapshot)
		/// </summary>
		#endregion
		public virtual void ResetState()
		{
		}
	}
}
