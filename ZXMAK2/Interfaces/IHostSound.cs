using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Interfaces
{
    public interface IHostSound
    {
        /// <summary>
        /// Lock next sound buffer.
        /// In case of all sound buffers is busy, method returns null.
        /// </summary>
        byte[] LockBuffer();
        /// <summary>
        /// Release previously locked sound buffer.
        /// </summary>
        void UnlockBuffer(byte[] sndbuf);
    }
}
