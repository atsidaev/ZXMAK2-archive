using System;
using System.IO;
using System.Text;

using ZXMAK2.Engine.Devices.Disk;


namespace ZXMAK2.Engine.Serializers.DiskSerializers
{
	public class SclSerializer : HobetaSerializer
	{
		public SclSerializer(DiskImage diskImage)
			: base(diskImage)
		{
		}

		
		#region FormatSerializer
		
		public override string FormatName { get { return "SCL disk image"; } }
		public override string FormatExtension { get { return "SCL"; } }
        public override bool CanDeserialize { get { return true; } }

		public override void Deserialize(Stream stream)
		{
			loadFromStream(stream);
            _diskImage.ModifyFlag = ModifyFlag.None;
            _diskImage.Present = true;
        }

        public override void SetSource(string fileName)
        {
            _diskImage.FileName = fileName;
        }

        public override void SetReadOnly(bool readOnly)
        {
            _diskImage.IsWP = readOnly;
        }

		#endregion


		private void loadFromStream(Stream stream)
		{
			if (stream.Length < 9 || stream.Length > 2544 * 256 + 9)
			{
				DialogProvider.Show(
                    "Invalid SCL file size", 
                    "SCL loader",
                    DlgButtonSet.OK,
                    DlgIcon.Warning);
                return;
			}

			byte[] fbuf = new byte[stream.Length];
			stream.Seek(0, SeekOrigin.Begin);
			stream.Read(fbuf, 0, (int)stream.Length);

			// TODO:check first 8 bytes "SINCLAIR"
			if (Encoding.ASCII.GetString(fbuf, 0, 8) != "SINCLAIR")
			{
				DialogProvider.Show(
                    "Corrupted SCL file", 
                    "SCL loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return;
			}

			//            if (fbuf.Length >= (9 + (0x100 + 14) * fbuf[8]))  
			//               throw new InvalidDataException("Corrupt *.SCL file!");

			//string oldExt = Path.GetExtension(_diskImage.FileName).ToUpper();
			//if (oldExt == string.Empty)
			_diskImage.Format();

			int size;
			for (int i = size = 0; i < fbuf[8]; i++)
				size += fbuf[9 + 14 * i + 13];
			if (size > 2544)
			{
				byte[] s9 = new byte[256];
				_diskImage.readLogicalSector(0, 0, 9, s9);
				s9[0xE5] = (byte)size;              // free sec
				s9[0xE6] = (byte)(size >> 8);
				_diskImage.writeLogicalSector(0, 0, 9, s9);
			}
			int dataIndex = 9 + 14 * fbuf[8];
			for (int i = 0; i < fbuf[8]; i++)
			{
				if (!addFile(fbuf, 9 + 14 * i, dataIndex))
					return;
				dataIndex += fbuf[9 + 14 * i + 13] * 0x100;
			}
		}
	}
}
