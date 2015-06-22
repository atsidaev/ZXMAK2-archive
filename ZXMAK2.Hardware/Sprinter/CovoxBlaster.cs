/* 
 *  Copyright 2015 Alex Makeev
 * 
 *  This file is part of ZXMAK2 (ZX Spectrum virtual machine).
 *
 *  ZXMAK2 is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ZXMAK2 is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with ZXMAK2.  If not, see <http://www.gnu.org/licenses/>.
 *  
 * 
 */
using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Hardware.Sprinter
{
    public class CovoxBlaster : SoundDeviceBase
    {
        #region Constants

        private const int MODE_CBL = 0x80;      // COVOX-Blaster on (если 0 то обычный режим COVOX)
        private const int MODE_STEREO = 0x40;   // STEREO-mode on
        private const int MODE_16BIT = 0x20;    // 16bit-mode on
        private const int MODE_INTRQ = 0x10;    // Interrupt on - включение прерываний

        #endregion Constants


        #region Fields

        private readonly byte[] _ram = new byte[0x100];
        private int _index;
        private double _lastTime;

        private byte _port4e;
        private double _sampleRateHz;
        private int m_mult;

        private CpuUnit _cpu;

        #endregion Fields


        public CovoxBlaster()
        {
            Category = BusDeviceCategory.Sound;
            Name = "COVOX BLASTER";
            Description = "SPRINTER COVOX BLASTER";
            _sampleRateHz = GetFrequency(_port4e);
        }

        
        #region SoundDeviceBase

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            _cpu = bmgr.CPU;
            bmgr.SubscribeWrIo(0x00FF, 0x004e, WritePort4e);
            bmgr.SubscribeRdIo(0x00FF, 0x004e, ReadPort4e);
            bmgr.SubscribeWrIo(0x00FF, 0x004F, WritePort4f);
            bmgr.SubscribeWrIo(0x00FF, 0x00FB, WritePortFb);
            bmgr.SubscribeRdIo(0x00FF, 0x00FE, ReadPortFe);
            bmgr.SubscribeReset(ResetBus);
        }

        protected override void OnVolumeChanged(int oldVolume, int newVolume)
        {
            m_mult = (ushort.MaxValue * newVolume) / (100 * 0xFF);
        }

        protected override void OnBeginFrame()
        {
            base.OnBeginFrame();
            _lastTime = 0D;
        }

        protected override void OnEndFrame()
        {
            Flush(1D);
            base.OnEndFrame();
        }

        protected override void OnConfigLoad(XmlNode node)
        {
            base.OnConfigLoad(node);
            LogIo = Utils.GetXmlAttributeAsBool(node, "logIo", false);
        }

        protected override void OnConfigSave(XmlNode node)
        {
            base.OnConfigSave(node);
            Utils.SetXmlAttribute(node, "logIo", LogIo);
        }

        private void ResetBus()
        {
            for (var i = 0; i < _ram.Length; i++)
            {
                _ram[i] = 0;
            }
        }

        #endregion SoundDeviceBase


        #region I/O Handlers

        private void WritePort4e(ushort addr, byte value, ref bool handled)
        {
            //if (handled)
            //    return;
            handled = true;
            
            if (_port4e == value)
            {
                return;
            }
            if (LogIo)
            {
                Logger.Debug("CovoxBlaster: mode #{0:X2} => #{1:X2} (PC=#{2:X4}, BC={3:X4})", _port4e, value, _cpu.LPC, _cpu.regs.BC);
            }
            Flush(GetFrameTime());
            _port4e = value;
            _sampleRateHz = GetFrequency(value & 0x0F);
        }

        private void ReadPort4e(ushort addr, ref byte value, ref bool handled)
        {
            //if (handled)
            //    return;
            handled = true;
            value = _port4e;
        }

        private void WritePort4f(ushort addr, byte value, ref bool handled)
        {
            //if (handled)
            //    return;
            //handled = true;

            // CovoxBlaster mode
            Flush(GetFrameTime());
            if (LogIo)
            {
                Logger.Debug("CovoxBlaster: write #{0:X2} (PC=#{1:X4}, BC={2:X4})", value, _cpu.LPC, _cpu.regs.BC);
            }
            _ram[addr >> 8] = value;
        }

        private void WritePortFb(ushort addr, byte value, ref bool handled)
        {
            //if (handled)
            //    return;
            //handled = true;

            if ((_port4e & MODE_CBL) == 0)
            {
                // Covox mode
                var dac = (ushort)(value * m_mult);
                UpdateDac(dac, dac);
            }
        }

        private void ReadPortFe(ushort addr, ref byte value, ref bool handled)
        {
            //if (handled)
            //    return;
            //handled = true;

            if ((_port4e & MODE_INTRQ) != 0)
            {
                Flush(GetFrameTime());
                value &= 0x7F;
                value |= (byte)(_index & 0x80);
            }
        }

        #endregion I/O Handlers


        #region Properties

        public bool LogIo { get; set; }

        #endregion Properties


        #region Private

        private void Flush(double frameTime)
        {
            if (double.IsNaN(_sampleRateHz) || 
                _sampleRateHz < 1000D ||
                _sampleRateHz > 200000D)
            {
                return;
            }
            var isCbl = (_port4e & MODE_CBL) != 0;
            var tick = 50D / _sampleRateHz;
            for (var time = _lastTime; time < frameTime && time <= 1D; time+=tick)
            {
                if (isCbl)
                {
                    var data = _ram[_index];
                    var dac = (ushort)(data * m_mult);
                    UpdateDac(time, dac, dac);
                }
                _index = (_index - 1) & 0xFF;
                _lastTime = time+tick;
            }
        }

        /// <summary>
        /// Returns sample rate frequency in Hz
        /// </summary>
        /// <param name="code">Control code (lower 4 bits of port #4E)</param>
        private static double GetFrequency(int code)
        {
            switch (code)
            {
                // это старые режимы -- не использовать!
                // (old modes, do not use it!)
                case 0: return 16000D;
                case 1: return 22000D;

                case 8: return 7812.5D;
                case 9: return 10937.5D;
                case 10: return 15625D;
                case 11: return 21875D;
                case 12: return 31250D;
                case 13: return 43750D;
                case 14: return 54687.5D;
                case 15: return 109375D;

                // 2-7 - reserved
                default: return double.NaN;
            }
        }

        #endregion Private
    }
}
