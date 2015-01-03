using System;

namespace ZXMAK2.Entities
{
    public class HostDeviceInfo : IComparable
    {
        public string Name { get; private set; }
        public string HostId { get; private set; }

        public HostDeviceInfo(string name, string hostId)
        {
            Name = name;
            HostId = hostId;
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            return Name.CompareTo(obj.ToString());
        }
    }
}
