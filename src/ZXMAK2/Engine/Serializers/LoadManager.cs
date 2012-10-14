using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Serializers.ScreenshotSerializers;
using ZXMAK2.Engine.Serializers.SnapshotSerializers;
using ZXMAK2.Engine.Serializers.TapeSerializers;
using ZXMAK2.Engine.Serializers.DiskSerializers;


namespace ZXMAK2.Engine.Serializers
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
