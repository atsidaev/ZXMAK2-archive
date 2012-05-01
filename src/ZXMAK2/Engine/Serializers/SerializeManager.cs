using System;
using System.IO;
using System.Text;
using System.Collections.Generic;



namespace ZXMAK2.Engine.Serializers
{
	public abstract class SerializeManager
	{
		private Dictionary<string, FormatSerializer> _formats = new Dictionary<string, FormatSerializer>();

		
		public string GetOpenExtFilter()
		{
			string result = string.Empty;

			List<string> groupList = new List<string>();
			foreach (FormatSerializer fs in _formats.Values)
				if (!groupList.Contains(fs.FormatGroup))
					groupList.Add(fs.FormatGroup);
			string allExtList = string.Empty;
			foreach (string group in groupList)
			{
				string gext = string.Empty;
				foreach (FormatSerializer fs in _formats.Values)
					if (fs.FormatGroup == group && fs.CanDeserialize)
					{
						if (gext.Length > 0) gext += ";";
						if (allExtList.Length > 0) allExtList += ";";

						if (fs.FormatExtension == "$") // Hobeta specific
						{
							gext += "*.!*;.$*";
							allExtList += "*.!*;*.$*";
						}
						else
						{
							gext += "*." + fs.FormatExtension.ToLower();
							allExtList += "*." + fs.FormatExtension.ToLower();
						}
					}
				if (gext.Length > 0)
				{
					gext += ";*.zip";
					result += "|" + group + " (" + gext + ")|" + gext;
				}
			}
			if (allExtList.Length > 0) allExtList += ";*.zip";
			result = "All supported files|" + allExtList + result;
			return result;
		}

		public string GetSaveExtFilter()
		{
			string result = string.Empty;

			List<string> groupList = new List<string>();
			foreach (FormatSerializer fs in _formats.Values)
				if (!groupList.Contains(fs.FormatGroup))
					groupList.Add(fs.FormatGroup);
			foreach (string group in groupList)
			{
				foreach (FormatSerializer fs in _formats.Values)
					if (fs.FormatGroup == group && fs.CanSerialize)
					{
						string gext = string.Empty;
						//if (gext.Length > 0) gext += ";";

						if (fs.FormatExtension == "$") // Hobeta specific
							gext += "*.!*;.$*";
						else
							gext += "*." + fs.FormatExtension.ToLower();
						
						if (gext.Length > 0)
						{
							if (result.Length > 0) result += "|";
							result += fs.FormatName + " (" + gext + ")|" + gext;
						}
					}
			}
			return result;
		}

		public string SaveFileName(string fileName)
		{
			using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
				saveStream(stream, Path.GetExtension(fileName).ToUpper(), fileName);
			return Path.GetFileName(fileName);
		}

        public string OpenFileStream(string fileName, Stream fileStream)
        {
            string ext = Path.GetExtension(fileName).ToUpper();
            if (ext != ".ZIP")
            {
                openStream(fileStream, ext, string.Empty, true);
                return Path.GetFileName(fileName);
            }
            else
            {
                List<ZipLib.Zip.ZipEntry> list = new List<ZipLib.Zip.ZipEntry>();
                using (ZipLib.Zip.ZipFile zip = new ZipLib.Zip.ZipFile(fileStream))
                {
                    zip.IsStreamOwner = false;
                    foreach (ZipLib.Zip.ZipEntry entry in zip)
                    {
                        if (entry.IsFile && entry.CanDecompress &&
                           Path.GetExtension(entry.Name).ToUpper() != ".ZIP" &&
                           CheckCanOpenFileName(entry.Name))
                        {
                            //return openZipEntry(fileName, zip, entry);
                            list.Add(entry);
                        }
                    }
                    ZipLib.Zip.ZipEntry selEntry = null;
                    if (list.Count == 1)
                    {
                        selEntry = list[0];
                    }
                    else if (list.Count > 1)
                    {
                        selEntry = (ZipLib.Zip.ZipEntry)DialogProvider.ObjectSelector(
                            list.ToArray(),
                            Path.GetFileName(fileName));
                        if (selEntry == null)
                            return string.Empty;
                    }
                    if (selEntry != null)
                    {
                        string result = openZipEntry(fileName, zip, selEntry, string.Empty);
                        if (result != string.Empty)
                        {
                            return result;
                        }
                    }
                }
            }
            DialogProvider.Show(
                string.Format("Can't open {0}!\n\nSupported file not found!", fileName),
                "Error",
                DlgButtonSet.OK,
                DlgIcon.Error);
            return string.Empty;
        }

		public string OpenFileName(string fileName, bool wp)
		{
			string ext = Path.GetExtension(fileName).ToUpper();
			if (ext != ".ZIP")
			{
				using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
					openStream(stream, ext, fileName, wp);
				return Path.GetFileName(fileName);
			}
			else
			{
                List<ZipLib.Zip.ZipEntry> list = new List<ZipLib.Zip.ZipEntry>();
                using (ZipLib.Zip.ZipFile zip = new ZipLib.Zip.ZipFile(fileName))
                {
                    foreach (ZipLib.Zip.ZipEntry entry in zip)
                    {
                        if (entry.IsFile && entry.CanDecompress &&
                           Path.GetExtension(entry.Name).ToUpper() != ".ZIP" &&
                           CheckCanOpenFileName(entry.Name))
                        {
                            //return openZipEntry(fileName, zip, entry);
                            list.Add(entry);
                        }
                    }
                    ZipLib.Zip.ZipEntry selEntry = null;
                    if (list.Count == 1)
                    {
                        selEntry = list[0];
                    }
                    else if (list.Count > 1)
                    {
                        selEntry = (ZipLib.Zip.ZipEntry)DialogProvider.ObjectSelector(
                            list.ToArray(),
                            Path.GetFileName(fileName));
                        if (selEntry==null)
                            return string.Empty;
                    }
                    if (selEntry != null)
                    {
                        string result = openZipEntry(fileName, zip, selEntry, fileName);
                        if (result != string.Empty)
                        {
                            return result;
                        }
                    }
                }
            }
			DialogProvider.Show(
                string.Format("Can't open {0}!\n\nSupported file not found!", fileName),
                "Error",
                DlgButtonSet.OK,
                DlgIcon.Error);
			return string.Empty;
		}

        private string openZipEntry(
            string fileName, 
            ZipLib.Zip.ZipFile zip, ZipLib.Zip.ZipEntry entry,
            string source)
        {
            using (Stream s = zip.GetInputStream(entry))
            {
                byte[] data = new byte[entry.Size];
                s.Read(data, 0, data.Length);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    if (intCheckCanOpenFileName(entry.Name))
                    {
                        openStream(ms, Path.GetExtension(entry.Name).ToUpper(), source, true);
                    }
                    else
                    {
                        DialogProvider.Show(
                            string.Format("Can't open {0}\\{1}!\n\nFile not supported!", fileName, entry.Name),
                            "Error",
                            DlgButtonSet.OK,
                            DlgIcon.Error);
                        return string.Empty;
                    }
                    return string.Format("{0}/{1}", Path.GetFileName(fileName), entry.Name);
                }
            }
        }


		public bool CheckCanOpenFileName(string fileName)
		{
            if (Path.GetExtension(fileName).ToUpper() != ".ZIP")
            {
                return intCheckCanOpenFileName(fileName);
            }
            else
            {
                using (ZipLib.Zip.ZipFile zip = new ZipLib.Zip.ZipFile(fileName))
                    foreach (ZipLib.Zip.ZipEntry entry in zip)
                        if (entry.IsFile && entry.CanDecompress && intCheckCanOpenFileName(entry.Name))
                            return true;
            }
            return false;
		}

        public bool CheckCanOpenFileStream(string fileName, Stream fileStream)
        {
            if (Path.GetExtension(fileName).ToUpper() != ".ZIP")
            {
                return intCheckCanOpenFileName(fileName);
            }
            else
            {
                using (ZipLib.Zip.ZipFile zip = new ZipLib.Zip.ZipFile(fileStream))
                {
                    zip.IsStreamOwner = false;
                    foreach (ZipLib.Zip.ZipEntry entry in zip)
                        if (entry.IsFile && entry.CanDecompress && intCheckCanOpenFileName(entry.Name))
                            return true;
                }
            }
            return false;
        }

		public bool CheckCanSaveFileName(string fileName)
		{
			return intCheckCanSaveFileName(fileName);
		}

		public string GetDefaultExtension()
		{
			foreach (FormatSerializer fs in _formats.Values)
				if (fs.CanSerialize && fs.FormatExtension!="$")
					return "." + fs.FormatExtension;
			return string.Empty;
		}

		#region private

        private void saveStream(Stream stream, string ext, string source)
		{
			FormatSerializer serializer = GetSerializer(ext);
			if (serializer == null)
			{
				DialogProvider.Show(
                    string.Format("Save {0} file format not implemented!", ext), 
                    "Error",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return;
			}
			serializer.Serialize(stream);
            serializer.SetSource(source);
		}

		private void openStream(Stream stream, string ext, string source, bool wp)
		{
			FormatSerializer serializer = GetSerializer(ext);
			if (serializer == null)
			{
				DialogProvider.Show(
                    string.Format("Open {0} file format not implemented!", ext), 
                    "Error",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return;
			}
			serializer.Deserialize(stream);
            serializer.SetSource(source);
            serializer.SetReadOnly(wp);
		}

		private bool intCheckCanOpenFileName(string fileName)
		{
			string ext = Path.GetExtension(fileName).ToUpper();
			foreach (string se in OpenFileExtensionList)
			{
				if (ext == se) return true;
				if (se.IndexOf('*') >= 0 && ext.Length >= 2 && (ext.Substring(0, 2) == ".!" || ext.Substring(0, 2) == ".$")) return true;
			}
			return false;
		}

		private bool intCheckCanSaveFileName(string fileName)
		{
			string ext = Path.GetExtension(fileName).ToUpper();
			foreach (string se in SaveFileExtensionList)
			{
				if (ext == se) return true;
				if (se.IndexOf('*') >= 0 && ext.Length >= 2 && (ext.Substring(0, 2) == ".!" || ext.Substring(0, 2) == ".$")) return true;
			}
			return false;
		}

		private string[] OpenFileExtensionList
		{
			get
			{
				List<string> list = new List<string>();
				foreach (FormatSerializer serializer in _formats.Values)
				{
					if (serializer.CanDeserialize)
					{
						if (serializer.FormatExtension == "$") // Hobeta is specific...
						{
							list.Add(".!*");
							list.Add(".$*");
						}
						else
							list.Add("." + serializer.FormatExtension.ToUpper());
					}
				}
				return list.ToArray();
			}
		}

		private string[] SaveFileExtensionList
		{
			get
			{
				List<string> list = new List<string>();
				foreach (FormatSerializer serializer in _formats.Values)
				{
					if (serializer.CanSerialize)
					{
						if (serializer.FormatExtension == "$") // Hobeta is specific...
						{
							list.Add(".!*");
							list.Add(".$*");
						}
						else
							list.Add("." + serializer.FormatExtension.ToUpper());
					}
				}
				return list.ToArray();
			}
		}

        public void Clear()
        {
            _formats.Clear();
        }

        public void AddSerializer(FormatSerializer serializer)
		{
            string key = serializer.FormatExtension.ToUpper();
            if (_formats.ContainsKey(key))
                throw new ArgumentException(string.Format("Cannot add serializer for extension \"{0}\", because there is another serializer exists for the same extension", serializer.FormatExtension));
		    _formats.Add(key , serializer);
		}

		public FormatSerializer GetSerializer(string ext)
		{
			ext = ext.ToUpper();
			FormatSerializer serializer = null;
			if (ext.Length >= 2 && ext.Substring(0, 2) == ".!" || ext.Substring(0, 2) == ".$")
			{
				serializer = _formats["$"];
			}
			else
			{
				if (ext.StartsWith("."))
					ext = ext.Substring(1, ext.Length - 1);
				if (_formats.ContainsKey(ext))
					serializer = _formats[ext];
			}
			return serializer;
		}

        public List<FormatSerializer> GetSerializers()
        {
            return new List<FormatSerializer>(_formats.Values);
        }
        #endregion

	}
}
