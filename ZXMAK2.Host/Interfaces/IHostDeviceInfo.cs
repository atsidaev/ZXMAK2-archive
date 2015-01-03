using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Host.Interfaces
{
    public interface IHostDeviceInfo : IComparable
    {
        string Name { get; }
        string HostId { get; }
    }
}
