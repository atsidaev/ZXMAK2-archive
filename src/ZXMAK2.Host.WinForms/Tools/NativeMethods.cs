using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ZXMAK2.Host.WinForms.Tools
{
    internal sealed class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public unsafe static extern void CopyMemory(int* destination, int* source, int length);

        [DllImport("kernel32.dll", SetLastError = true)]
        public unsafe static extern void CopyMemory(uint* destination, uint* source, int length);
    }
}
