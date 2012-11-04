using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using ZXMAK2.Interfaces;
using ZXMAK2.Serializers.ScreenshotSerializers;
using ZXMAK2.Serializers.SnapshotSerializers;
using ZXMAK2.Serializers.TapeSerializers;
using ZXMAK2.Serializers.DiskSerializers;
using ZXMAK2.Engine;


namespace ZXMAK2.Serializers
{
	public class LoadManager : SerializeManager
	{
		private SpectrumBase _spec;

		public LoadManager(SpectrumBase spec)
		{
			_spec = spec;
		}

		public void AddStandardSerializers()
		{
			// Snapshots...
			AddSerializer(new SzxSerializer(_spec));
			AddSerializer(new Z80Serializer(_spec));
			AddSerializer(new SnaSerializer(_spec));
			AddSerializer(new SitSerializer(_spec));
			AddSerializer(new ZxSerializer(_spec));
			AddSerializer(new RzxSerializer(_spec));
		}
	}
}
