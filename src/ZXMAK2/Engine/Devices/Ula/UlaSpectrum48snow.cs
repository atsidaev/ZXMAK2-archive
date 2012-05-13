using System;
using System.Collections.Generic;
using System.Text;

using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Engine.Devices.Ula
{
    public class UlaSpectrum48snow : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "ZX Spectrum 48 [+Snow]"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRDMEM(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM_M1(0x0000, 0x0000, ReadMemM1);

            bmgr.SubscribeRDNOMREQ(0xC000, 0x4000, ContendNoMreq);
            bmgr.SubscribeWRNOMREQ(0xC000, 0x4000, ContendNoMreq);

            bmgr.SubscribeRDIO(0x0000, 0x0000, ReadPortAll);
            bmgr.SubscribeWRIO(0x0000, 0x0000, WritePortAll);
        }

        #endregion

        public UlaSpectrum48snow()
        {
            // ZX Spectrum 48
            // Total Size:          //+ 448 x 312
            // Visible Size:        //+ 352 x 303 (48+256+48 x 55+192+56)

            c_ulaLineTime = 224;
            c_ulaFirstPaperLine = 64;
            c_ulaFirstPaperTact = 64;      // 64 [40sync+24border+128scr+32border]
            c_frameTactCount = 69888;

            c_ulaBorderTop = 55;      //56
            c_ulaBorderBottom = 56;   //
            c_ulaBorderLeftT = 24;    //16T
            c_ulaBorderRightT = 24;   //32T

            c_ulaIntBegin = 62 + 1;
            c_ulaIntLength = 32;    // according to fuse

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
            
            fillTable(true);
        }


        #region Bus Handlers

        protected override void WriteMem4000(ushort addr, byte value)
        {
            contendMemory();
            base.WriteMem4000(addr, value);
        }

        protected void ReadMem4000(ushort addr, ref byte value)
        {
            contendMemory();
        }

        private int m_ulaNoise = 0xA001;
        private int m_snow = 0;

        protected void ReadMemM1(ushort addr, ref byte value)
        {
            if ((CPU.regs.IR & 0xC000) == 0x4000)
            {
                int frameTactT3 = (int)((CPU.Tact + 3) % c_frameTactCount);
                int do3 = _ulaDo[frameTactT3];
                if ((do3 > 2 && do3 <= 5) || do3==9 || do3==10)
                {
                    UpdateState(frameTactT3-1);
                    m_snow = 2;
                }
            }
        }

        protected void ContendNoMreq(ushort addr)
        {
            if (IsContended(addr))
                contendMemory();
        }

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
        }

        private void WritePortAll(ushort addr, byte value, ref bool iorqge)
        {
            contendPortEarly(addr);
            if ((addr & 0x0001) == 0)
            {
                UpdateState((int)((CPU.Tact + 1) % FrameTactCount));
                PortFE = value;
            }
            contendPortLate(addr);
        }

        private void ReadPortAll(ushort addr, ref byte value, ref bool iorqge)
        {
            contendPortEarly(addr);
            contendPortLate(addr);
            int frameTact = (int)((CPU.Tact - 1) % FrameTactCount);
            base.ReadPortFF(frameTact, ref value);
        }


        #endregion


        protected override unsafe void fetchVideo(
            uint* bitmapBufPtr,
            int startTact,
            int endTact,
            UlaStateBase ulaState)
        {
            if (bitmapBufPtr == null)
                return;

            if (_ulaDo == null)	// VideoParams not set!
                return;

            if (endTact > c_frameTactCount)
                endTact = c_frameTactCount;
            if (startTact > c_frameTactCount)
                startTact = c_frameTactCount;
            if (startTact < c_ulaLineTime * (c_ulaFirstPaperLine - c_ulaBorderTop))
                startTact = c_ulaLineTime * (c_ulaFirstPaperLine - c_ulaBorderTop) - c_ulaIntBegin;

            for (int takt = startTact; takt < endTact; takt++)
            {
                switch (_ulaDo[takt])
                {
                    case 0:     // no action
                        continue;

                    case 1:     // border
                        bitmapBufPtr[_ulaLineOffset[takt]] = _borderColor;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = _borderColor;
                        continue;

                    case 2:     // border & fetch B1
                        bitmapBufPtr[_ulaLineOffset[takt]] = _borderColor;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = _borderColor;

                        ulaState._ulaFetchB1 = _ulaMemory[_ulaAddrBW[takt]];
                        if (m_snow>0)
                        {
                            m_snow--;
                            int addr = _ulaAddrBW[takt];
                            if((m_ulaNoise & 0x0F)>9)
                                addr = (addr & 0x3F00) | (m_ulaNoise&0x00FF);
                            else if ((m_ulaNoise & 3) != 0)
                                addr = ((addr - 1) & 0xFF) | (addr & 0x3F00);
                            ulaState._ulaFetchB1 = _ulaMemory[addr];
                            m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        }
                        break;

                    case 3:     // border & fetch A1
                        bitmapBufPtr[_ulaLineOffset[takt]] = _borderColor;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = _borderColor;

                        ulaState._ulaFetchA1 = _ulaMemory[_ulaAddrAT[takt]];
                        //if (m_snow>0)
                        //{
                        //    m_snow--;
                        //    ulaFetchA2 = _ulaMemory[(m_ulaNoise & 0x0FF) ^ _ulaAddrAT[takt]];
                        //    m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        //}
                        ulaState._ulaFetchInk = _ulaInk[ulaState._ulaFetchA1 + _flashState];
                        ulaState._ulaFetchPaper = _ulaPaper[ulaState._ulaFetchA1 + _flashState];
                        break;

                    case 4:     // shift 1 & fetch B2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState._ulaFetchB1 & 0x80) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState._ulaFetchB1 & 0x40) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        ulaState._ulaFetchB1 <<= 2;

                        ulaState._ulaFetchB2 = _ulaMemory[_ulaAddrBW[takt]];
                        if (m_snow>0)
                        {
                            m_snow--;
                            int addr = _ulaAddrBW[takt];
                            if ((m_ulaNoise & 0x0F) > 9)
                                addr = (addr & 0x3F00) | (m_ulaNoise & 0x00FF);
                            else if ((m_ulaNoise & 3) != 0)
                                addr = ((addr - 1) & 0xFF) | (addr & 0x3F00);
                            ulaState._ulaFetchB2 = _ulaMemory[addr];
                            m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        }
                        break;

                    case 5:     // shift 1 & fetch A2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState._ulaFetchB1 & 0x80) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState._ulaFetchB1 & 0x40) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        ulaState._ulaFetchB1 <<= 2;

                        ulaState._ulaFetchA2 = _ulaMemory[_ulaAddrAT[takt]];
                        //if (m_snow>0)
                        //{
                        //    m_snow--;
                        //    ulaFetchA2 = _ulaMemory[(m_ulaNoise & 0x0FF) ^ _ulaAddrAT[takt]];
                        //    m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        //}
                        break;

                    case 6:     // shift 1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState._ulaFetchB1 & 0x80) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState._ulaFetchB1 & 0x40) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        ulaState._ulaFetchB1 <<= 2;
                        break;

                    case 7:     // shift 1 (last)
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState._ulaFetchB1 & 0x80) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState._ulaFetchB1 & 0x40) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        ulaState._ulaFetchB1 <<= 2;

                        ulaState._ulaFetchInk = _ulaInk[ulaState._ulaFetchA2 + _flashState];
                        ulaState._ulaFetchPaper = _ulaPaper[ulaState._ulaFetchA2 + _flashState];
                        break;

                    case 8:     // shift 2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState._ulaFetchB2 & 0x80) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState._ulaFetchB2 & 0x40) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        ulaState._ulaFetchB2 <<= 2;
                        break;

                    case 9:     // shift 2 & fetch B1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState._ulaFetchB2 & 0x80) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState._ulaFetchB2 & 0x40) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        ulaState._ulaFetchB2 <<= 2;

                        ulaState._ulaFetchB1 = _ulaMemory[_ulaAddrBW[takt]];
                        if (m_snow>0)
                        {
                            m_snow--;
                            int addr = _ulaAddrBW[takt];
                            if ((m_ulaNoise & 0x0F) > 9)
                                addr = (addr & 0x3F00) | (m_ulaNoise & 0x00FF);
                            else if ((m_ulaNoise & 3) != 0)
                                addr = ((addr - 1) & 0xFF) | (addr & 0x3F00);
                            ulaState._ulaFetchB1 = _ulaMemory[addr];
                            m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        }
                        break;

                    case 10:     // shift 2 & fetch A1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState._ulaFetchB2 & 0x80) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState._ulaFetchB2 & 0x40) != 0) ? ulaState._ulaFetchInk : ulaState._ulaFetchPaper;
                        ulaState._ulaFetchB2 <<= 2;

                        ulaState._ulaFetchA1 = _ulaMemory[_ulaAddrAT[takt]];
                        //if (m_snow>0)
                        //{
                        //    m_snow--;
                        //    ulaFetchA1 = _ulaMemory[(m_ulaNoise & 0x0FF) ^ _ulaAddrAT[takt]];
                        //    m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        //}
                        ulaState._ulaFetchInk = _ulaInk[ulaState._ulaFetchA1 + _flashState];
                        ulaState._ulaFetchPaper = _ulaPaper[ulaState._ulaFetchA1 + _flashState];
                        break;
                }
            }
        }
        
        
        private void contendMemory()
        {
            int frameTact = (int)(CPU.Tact % c_frameTactCount);
            CPU.Tact += m_contention[frameTact];
        }

        private bool IsContended(int addr)
        {
            int test = addr & 0xC000;
            return (test == 0x4000);
        }

        private bool IsPortUla(int addr)
        {
            return (addr & 1) == 0;
        }

        private void contendPortEarly(int addr)
        {
            if (IsContended(addr))
            {
                int frameTact = (int)(CPU.Tact % c_frameTactCount);
                CPU.Tact += m_contention[frameTact];
            }
        }

        private void contendPortLate(int addr)
        {
            int shift = 1;
            int frameTact = (int)((CPU.Tact + shift) % c_frameTactCount);

            if (IsPortUla(addr))
            {
                CPU.Tact += m_contention[frameTact];
            }
            else if (IsContended(addr))
            {
                CPU.Tact += m_contention[frameTact]; frameTact += m_contention[frameTact]; frameTact++; frameTact %= c_frameTactCount;
                CPU.Tact += m_contention[frameTact]; frameTact += m_contention[frameTact]; frameTact++; frameTact %= c_frameTactCount;
                CPU.Tact += m_contention[frameTact]; frameTact += m_contention[frameTact]; frameTact++; frameTact %= c_frameTactCount;
            }
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
