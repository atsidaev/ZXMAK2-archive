﻿/* 
 *  Copyright 2008-2018 Alex Makeev
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
 *  Description: DirectX native wrapper
 *  Date: 10.07.2018
 */
using System;


namespace ZXMAK2.DirectX.DirectSound
{
    // <unmanaged>DSBPLAY_FLAGS</unmanaged>	
    [Flags]
    public enum DSBPLAY_FLAGS
    {
        // <unmanaged>DSBPLAY_LOOPING</unmanaged>	
        LOOPING = 1,
        // <unmanaged>DSBPLAY_LOCHARDWARE</unmanaged>	
        LOCHARDWARE = 2,
        // <unmanaged>DSBPLAY_LOCSOFTWARE</unmanaged>	
        LOCSOFTWARE = 4,
        // <unmanaged>DSBPLAY_TERMINATEBY_TIME</unmanaged>	
        TERMINATEBY_TIME = 8,
        // <unmanaged>DSBPLAY_TERMINATEBY_DISTANCE</unmanaged>	
        TERMINATEBY_DISTANCE = 16,
        // <unmanaged>DSBPLAY_TERMINATEBY_PRIORITY</unmanaged>	
        TERMINATEBY_PRIORITY = 32,
        // <unmanaged>None</unmanaged>	
        None = 0
    }
}
