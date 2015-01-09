using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZXMAK2.Interfaces
{
    public interface ISerializeManager
    {
        string GetOpenExtFilter();
        string GetSaveExtFilter();
        string SaveFileName(string fileName);
        string OpenFileStream(string fileName, Stream fileStream);
        string OpenFileName(string fileName, bool wp);

        bool CheckCanOpenFileName(string fileName);
        bool CheckCanOpenFileStream(string fileName, Stream fileStream);
        bool CheckCanSaveFileName(string fileName);

        string GetDefaultExtension();

        void Clear();
        void AddSerializer(IFormatSerializer serializer);
        IFormatSerializer GetSerializer(string ext);
        IEnumerable<IFormatSerializer> GetSerializers();
    }
}
