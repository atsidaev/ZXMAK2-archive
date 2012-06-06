using System;
using System.IO;
using System.Collections.Generic;

using ZXMAK2.Engine.Devices.Disk;
using ZXMAK2.Engine.Serializers.DiskSerializers;


namespace ZXMAK2.Engine.Serializers
{
	public class DiskLoadManager : SerializeManager
	{
		public DiskLoadManager(DiskImage diskImage)
		{
			AddSerializer(new UdiSerializer(diskImage));
			AddSerializer(new FdiSerializer(diskImage));
			AddSerializer(new Td0Serializer(diskImage));
			AddSerializer(new TrdSerializer(diskImage));
			AddSerializer(new SclSerializer(diskImage));
			AddSerializer(new HobetaSerializer(diskImage));

			diskImage.SaveDisk += new SaveDiskDelegate(saveDisk);
		}

		private void saveDisk(DiskImage sender)
		{
			if (sender.IsWP)
			{
				LogAgent.Error("Write protected disk was changed! Autosave canceled");
				return;
			}

			string fileName = sender.FileName;
			if (fileName != string.Empty)
			{
				FormatSerializer serializer = GetSerializer(Path.GetExtension(fileName));
				if (serializer == null || !serializer.CanSerialize)
					fileName = string.Empty;
			}
			if (fileName == string.Empty)
			{
				string folderName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				folderName = Path.Combine(folderName, "Images");
				for (int i = 0; i < 10001; i++)
				{
					fileName = string.Format("zxmak2image{0:D3}{1}", i, GetDefaultExtension());
					fileName = Path.Combine(folderName, fileName);
					if (!File.Exists(fileName))
						break;
					fileName = string.Empty;
				}
			}

			string msg = string.Format(
				"Do you want to save disk changes to {0}",
				Path.GetFileName(fileName));

			DlgResult qr = DialogProvider.Show(
				msg,
				"Attention!",
				DlgButtonSet.YesNo,
				DlgIcon.Question);
			if (qr == DlgResult.Yes)
			{
				//if (Path.GetExtension(_fileName).ToUpper() == ".SCL")
				//{
				//   _fileName = Path.ChangeExtension(_fileName, ".TRD");
				//   if (File.Exists(_fileName))
				//      _fileName = string.Empty;
				//}

				if (fileName == string.Empty)
				{
					DialogProvider.Show(
						"Can't save disk image!\nNo space on HDD!",
						"Warning",
						DlgButtonSet.OK,
						DlgIcon.Warning);
				}
				else
				{
					string folderName = Path.GetDirectoryName(fileName);
					if (!Directory.Exists(folderName))
						Directory.CreateDirectory(folderName);
					sender.FileName = fileName;
					SaveFileName(sender.FileName);
					DialogProvider.Show(
						string.Format("Disk image successfuly saved!\n{0}", sender.FileName),
						"Notification",
						DlgButtonSet.OK,
						DlgIcon.Information);
				}
			}
		}
	}
}
