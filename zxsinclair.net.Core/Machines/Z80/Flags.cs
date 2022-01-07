#region LICENSE
/*
    ZXSinclair Emulador ZX Computers make in .Net and .Net CORE
    Copyright (C) 2016 Oscar Hernandez Bano
    This file is part of ZXSincalir.Net.
    ZXSincalir.Net is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXSinclair.Machines.Z80
{
    [Flags]
    public enum Flags
    {
        /// <summary>
        /// Sign Flag 
        /// [Set if the 2-complement value is negative (copy of MSB)]
        /// </summary>
        S = 0x80,

        /// <summary>
        /// Zero Flag 
        /// [Set if the value is zero]
        /// </summary>
        Z = 0x40,

        /// <summary>
        /// Undocumented flag F5
        /// </summary>
        F5 = 0x20,

        /// <summary>
        /// Half Carry Flag 
        /// [Carry from bit 3 to bit 4]
        /// </summary>
        H = 0x10,

        /// <summary>
        /// Undocumented flag F3
        /// </summary>
        F3 = 0x08,

        /// <summary>
        /// P/V - Parity/Overflow Flag
        /// [Parity set if even number of bits set.
        /// Overflow set if the 2-complement result does not fit in the register]
        /// </summary>
        P = 0x04,

        /// <summary>
        /// Add/Subtract Flag 
        /// [Set if the last operation was a subtraction]
        /// </summary>
        N = 0x02,

        /// <summary>
        /// Carry Flag
        /// [Set if the result did not fit in the register]
        /// </summary>
        C = 0x01
    }
}
