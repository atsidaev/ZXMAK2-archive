using System;
using System.Text;

using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Devices.Ula;


namespace Plugins.Ula
{
    public class UlaDelta : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "Delta-C [Cheboksary-91/74]"; } }
        public override string Description { get { return base.Description + Environment.NewLine + "Delta-C [Cheboksary 1991, 74 chips]" + Environment.NewLine + "Version 1.5"; } }


        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRDMEM(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeWRMEM(0xC000, 0x4000, WriteMem4000);
            bmgr.SubscribeRDNOMREQ(0xC000, 0x4000, ContendNoMreq);
            bmgr.SubscribeWRNOMREQ(0xC000, 0x4000, ContendNoMreq);
        }

        #endregion

        #region Bus Handlers

        protected override void WriteMem4000(ushort addr, byte value)
        {
            int frameTact = (int)(CPU.Tact % c_frameTactCount);
            CPU.Tact += m_contention[frameTact];
            base.WriteMem4000(addr, value);
        }

        protected void ReadMem4000(ushort addr, ref byte value)
        {
            int frameTact = (int)(CPU.Tact % c_frameTactCount);
            CPU.Tact+=m_contention[frameTact];
        }

        protected void ContendNoMreq(ushort addr)
        {
            int frameTact = (int)(CPU.Tact % c_frameTactCount);
            CPU.Tact += m_contention[frameTact];
        }

        #endregion


        public UlaDelta()
        {
            // Delta-C
            // Total Size:          448 x 320
            // Visible Size:        384 x 304 (72+256+56 x 64+192+48)

            c_ulaLineTime = 224;
            c_ulaFirstPaperLine = 68;
            c_ulaFirstPaperTact = 68;
            c_frameTactCount = 69216;//69888;

            c_ulaBorderTop = 64;
            c_ulaBorderBottom = 48;
            c_ulaBorderLeftT = 24;
            c_ulaBorderRightT = 24;

            c_ulaIntBegin = 0;
            c_ulaIntLength = 836;//224;
            c_ulaFlashPeriod = 8;

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);

            fillTable(true);
        }


        private void fillTable(bool lateModel)
        {
            m_contention = new int[c_frameTactCount];
            int[] byteContention = new int[] { 6, 5, 4, 3, 2, 1, 0, 0, };
            for (int t = 0; t < c_frameTactCount; t++)
            {
                int shifted = t - c_ulaIntBegin;
                if (!lateModel)
                    shifted -= 1;
                if (shifted < 0)
                    shifted += c_frameTactCount;

                m_contention[shifted] = 0;
                int line = t / c_ulaLineTime;
                int pix = t % c_ulaLineTime;
                if (line < c_ulaFirstPaperLine || line >= (c_ulaFirstPaperLine + 192))
                {
                    m_contention[shifted] = 0;
                    continue;
                }
                int scrPix = pix - c_ulaFirstPaperTact + 1;
                if (scrPix < 0 || scrPix >= 128)
                {
                    m_contention[shifted] = 0;
                    continue;
                }
                int pixByte = scrPix % 8;

                m_contention[shifted] = byteContention[pixByte];
            }
        }

        private int[] m_contention;
    }
}
