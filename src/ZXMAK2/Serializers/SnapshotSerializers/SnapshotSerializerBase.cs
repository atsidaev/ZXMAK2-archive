using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;

namespace ZXMAK2.Serializers.SnapshotSerializers
{
	public abstract class SnapshotSerializerBase : FormatSerializer
	{
		protected SpectrumBase _spec;


		public SnapshotSerializerBase(SpectrumBase spec)
		{
			_spec = spec;
		}

		public override string FormatGroup { get { return "Snapshots"; } }
		public override string FormatName { get { return string.Format("{0} snapshot", FormatExtension); } }


		protected void UpdateState()
		{
			IUlaDevice ula = (IUlaDevice)_spec.BusManager.FindDevice(typeof(IUlaDevice));
			ula.ForceRedrawFrame();
			_spec.RaiseUpdateState();
		}

		protected byte ReadMemory(ushort addr)
		{
			//IMemory memory = _spec.BusManager.FindDevice(typeof(IMemory)) as IMemory;
			//return memory.RDMEM_DBG(addr);
			return _spec.ReadMemory(addr);
		}

		public void WriteMemory(ushort addr, byte value)
		{
			//IMemory memory = _spec.BusManager.FindDevice(typeof(IMemory)) as IMemory;
			//memory.WRMEM_DBG(addr, value);
			_spec.WriteMemory(addr, value);
		}

		public int GetFrameTact()
		{
			IUlaDevice ula = _spec.BusManager.FindDevice(typeof(IUlaDevice)) as IUlaDevice;
			return (int)(_spec.CPU.Tact % ula.FrameTactCount);
		}

		public void SetFrameTact(int frameTact)
		{
			IUlaDevice ula = _spec.BusManager.FindDevice(typeof(IUlaDevice)) as IUlaDevice;

			if (frameTact < 0)
				frameTact = 0;
			frameTact %= ula.FrameTactCount;

			int delta = frameTact - GetFrameTact();
			if (delta < 0)
				delta += ula.FrameTactCount;
			_spec.CPU.Tact += delta;
		}

		public void InitStd128K()
		{
			foreach (BusDeviceBase device in _spec.BusManager.FindDevices(typeof(BusDeviceBase)))
				device.ResetState();
			_spec.DoReset();
			IMemoryDevice memory = _spec.BusManager.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
			memory.SYSEN = false;
			IBetaDiskDevice betaDisk = _spec.BusManager.FindDevice(typeof(IBetaDiskDevice)) as IBetaDiskDevice;
			if (betaDisk != null)
				betaDisk.DOSEN = false;
			SetFrameTact(0);
		}
	}
}
