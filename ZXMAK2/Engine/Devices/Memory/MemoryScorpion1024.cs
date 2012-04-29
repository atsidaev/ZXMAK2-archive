using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Engine.Devices.Memory
{
    public class MemoryScorpion1024 : MemoryScorpion256
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion 1024K"; } }
        public override string Description { get { return "Scorpion 1024K Memory Manager"; } }

        #endregion

        protected override void InitRam()
        {
            m_ramPages = new byte[64][];
            for (int i = 0; i < m_ramPages.Length; i++)
                m_ramPages[i] = new byte[0x4000];
        }
    }
}
