using System;
using ZXMAK2.Interfaces;


namespace ZXMAK2.Hardware.Spectrum
{
    public class UlaSpectrum48snow : UlaSpectrum48
    {
        #region IBusDevice

        public override string Name { get { return "ZX Spectrum 48 [snow]"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRDMEM_M1(0x0000, 0x0000, ReadMemM1);
        }

        #endregion

        #region Bus Handlers

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
                if (!c_ulaBorder4T || (takt & 3) == c_ulaBorder4Tstage)
                {
                    ulaState.Border = _borderColor;
                }
                switch (_ulaDo[takt])
                {
                    case 0:     // no action
                        break;

                    case 1:     // border
                        bitmapBufPtr[_ulaLineOffset[takt]] = ulaState.Border;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ulaState.Border;
                        break;

                    case 2:     // border & fetch B1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ulaState.Border;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ulaState.Border;

                        ulaState.B1 = _ulaMemory[_ulaAddrBW[takt]];
                        if (m_snow>0)
                        {
                            m_snow--;
                            int addr = _ulaAddrBW[takt];
                            if((m_ulaNoise & 0x0F)>9)
                                addr = (addr & 0x3F00) | (m_ulaNoise&0x00FF);
                            else if ((m_ulaNoise & 3) != 0)
                                addr = ((addr - 1) & 0xFF) | (addr & 0x3F00);
                            ulaState.B1 = _ulaMemory[addr];
                            m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        }
                        break;

                    case 3:     // border & fetch A1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ulaState.Border;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ulaState.Border;

                        ulaState.A1 = _ulaMemory[_ulaAddrAT[takt]];
                        //if (m_snow>0)
                        //{
                        //    m_snow--;
                        //    ulaFetchA2 = _ulaMemory[(m_ulaNoise & 0x0FF) ^ _ulaAddrAT[takt]];
                        //    m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        //}
                        ulaState.Ink = _ulaInk[ulaState.A1 + _flashState];
                        ulaState.Paper = _ulaPaper[ulaState.A1 + _flashState];
                        break;

                    case 4:     // shift 1 & fetch B2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState.B1 & 0x80) != 0) ? ulaState.Ink : ulaState.Paper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState.B1 & 0x40) != 0) ? ulaState.Ink : ulaState.Paper;
                        ulaState.B1 <<= 2;

                        ulaState.B2 = _ulaMemory[_ulaAddrBW[takt]];
                        if (m_snow>0)
                        {
                            m_snow--;
                            int addr = _ulaAddrBW[takt];
                            if ((m_ulaNoise & 0x0F) > 9)
                                addr = (addr & 0x3F00) | (m_ulaNoise & 0x00FF);
                            else if ((m_ulaNoise & 3) != 0)
                                addr = ((addr - 1) & 0xFF) | (addr & 0x3F00);
                            ulaState.B2 = _ulaMemory[addr];
                            m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        }
                        break;

                    case 5:     // shift 1 & fetch A2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState.B1 & 0x80) != 0) ? ulaState.Ink : ulaState.Paper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState.B1 & 0x40) != 0) ? ulaState.Ink : ulaState.Paper;
                        ulaState.B1 <<= 2;

                        ulaState.A2 = _ulaMemory[_ulaAddrAT[takt]];
                        //if (m_snow>0)
                        //{
                        //    m_snow--;
                        //    ulaFetchA2 = _ulaMemory[(m_ulaNoise & 0x0FF) ^ _ulaAddrAT[takt]];
                        //    m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        //}
                        break;

                    case 6:     // shift 1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState.B1 & 0x80) != 0) ? ulaState.Ink : ulaState.Paper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState.B1 & 0x40) != 0) ? ulaState.Ink : ulaState.Paper;
                        ulaState.B1 <<= 2;
                        break;

                    case 7:     // shift 1 (last)
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState.B1 & 0x80) != 0) ? ulaState.Ink : ulaState.Paper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState.B1 & 0x40) != 0) ? ulaState.Ink : ulaState.Paper;
                        ulaState.B1 <<= 2;

                        ulaState.Ink = _ulaInk[ulaState.A2 + _flashState];
                        ulaState.Paper = _ulaPaper[ulaState.A2 + _flashState];
                        break;

                    case 8:     // shift 2
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState.B2 & 0x80) != 0) ? ulaState.Ink : ulaState.Paper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState.B2 & 0x40) != 0) ? ulaState.Ink : ulaState.Paper;
                        ulaState.B2 <<= 2;
                        break;

                    case 9:     // shift 2 & fetch B1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState.B2 & 0x80) != 0) ? ulaState.Ink : ulaState.Paper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState.B2 & 0x40) != 0) ? ulaState.Ink : ulaState.Paper;
                        ulaState.B2 <<= 2;

                        ulaState.B1 = _ulaMemory[_ulaAddrBW[takt]];
                        if (m_snow>0)
                        {
                            m_snow--;
                            int addr = _ulaAddrBW[takt];
                            if ((m_ulaNoise & 0x0F) > 9)
                                addr = (addr & 0x3F00) | (m_ulaNoise & 0x00FF);
                            else if ((m_ulaNoise & 3) != 0)
                                addr = ((addr - 1) & 0xFF) | (addr & 0x3F00);
                            ulaState.B1 = _ulaMemory[addr];
                            m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        }
                        break;

                    case 10:     // shift 2 & fetch A1
                        bitmapBufPtr[_ulaLineOffset[takt]] = ((ulaState.B2 & 0x80) != 0) ? ulaState.Ink : ulaState.Paper;
                        bitmapBufPtr[_ulaLineOffset[takt] + 1] = ((ulaState.B2 & 0x40) != 0) ? ulaState.Ink : ulaState.Paper;
                        ulaState.B2 <<= 2;

                        ulaState.A1 = _ulaMemory[_ulaAddrAT[takt]];
                        //if (m_snow>0)
                        //{
                        //    m_snow--;
                        //    ulaFetchA1 = _ulaMemory[(m_ulaNoise & 0x0FF) ^ _ulaAddrAT[takt]];
                        //    m_ulaNoise = (((m_ulaNoise >> 16) ^ (m_ulaNoise >> 13)) & 1) ^ ((m_ulaNoise << 1) + 1);
                        //}
                        ulaState.Ink = _ulaInk[ulaState.A1 + _flashState];
                        ulaState.Paper = _ulaPaper[ulaState.A1 + _flashState];
                        break;
                }
            }
        }
    }
}
