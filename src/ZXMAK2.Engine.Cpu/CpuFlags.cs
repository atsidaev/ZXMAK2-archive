/* 
 *  Copyright 2007, 2015 Alex Makeev
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


namespace ZXMAK2.Engine.Cpu
{
    public static class CpuFlags
    {
        public const byte S = 0x80;
        public const byte Z = 0x40;
        public const byte F5 = 0x20;
        public const byte H = 0x10;
        public const byte F3 = 0x08;
        public const byte Pv = 0x04;
        public const byte N = 0x02;
        public const byte C = 0x01;

        public const byte NotS = S ^ 0xFF;
        public const byte NotZ = Z ^ 0xFF;
        public const byte NotF5 = F5 ^ 0xFF;
        public const byte NotH = H ^ 0xFF;
        public const byte NotF3 = F3 ^ 0xFF;
        public const byte NotPv = Pv ^ 0xFF;
        public const byte NotN = N ^ 0xFF;
        public const byte NotC = C ^ 0xFF;

        internal const byte F3F5 = F3 | F5;
        internal const byte F3F5S = F3 | F5 | S;
        internal const byte F3F5PvC = F3 | F5 | Pv | C;
        internal const byte NCF3F5H = N | C | F3 | F5 | H;
        internal const byte NHPvF3F5 = N | H | Pv | F3 | F5;
        internal const byte HC = H | C;
        internal const byte SZPv = S | Z | Pv;
        internal const byte NH = N | H;

        internal const byte NotF3F5 = F3F5 ^ 0xFF;
        internal const byte NotF3F5S = F3F5S ^ 0xFF;
        internal const byte NotF3F5PvC = F3F5PvC ^ 0xFF;
        internal const byte NotNCF3F5H = NCF3F5H ^ 0xFF;
        internal const byte NotNHPvF3F5 = NHPvF3F5 ^ 0xFF;
        internal const byte NotHC = HC ^ 0xFF;
        internal const byte NotSZPv = SZPv ^ 0xFF;
        internal const byte NotNH = NH ^ 0xFF;
    }
}
