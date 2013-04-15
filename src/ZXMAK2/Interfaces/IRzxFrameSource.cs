using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Entities;

namespace ZXMAK2.Interfaces
{
    public interface IRzxFrameSource
    {
        RzxFrame[] GetNextFrameArray();
    }
}
