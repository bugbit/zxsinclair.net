
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

namespace ZXSinclair.Machines.Z80
{
    public partial class Regs
    {
        /// <summary>
        /// R=r1
        /// 00rrrrRRR
        /// </summary>
        /// <param name="r_r1"></param>
        /// <returns></returns>
        public Action CreateLDR_R1(int r_r1)=> r_r1 switch
        {

(OpCodes.R_A << 3) | OpCodes.R_A => () => A = A,
(OpCodes.R_A << 3) | OpCodes.R_B => () => A = B,
(OpCodes.R_A << 3) | OpCodes.R_C => () => A = C,
(OpCodes.R_A << 3) | OpCodes.R_D => () => A = D,
(OpCodes.R_A << 3) | OpCodes.R_E => () => A = E,
(OpCodes.R_A << 3) | OpCodes.R_H => () => A = H,
(OpCodes.R_A << 3) | OpCodes.R_L => () => A = L,
(OpCodes.R_B << 3) | OpCodes.R_A => () => B = A,
(OpCodes.R_B << 3) | OpCodes.R_B => () => B = B,
(OpCodes.R_B << 3) | OpCodes.R_C => () => B = C,
(OpCodes.R_B << 3) | OpCodes.R_D => () => B = D,
(OpCodes.R_B << 3) | OpCodes.R_E => () => B = E,
(OpCodes.R_B << 3) | OpCodes.R_H => () => B = H,
(OpCodes.R_B << 3) | OpCodes.R_L => () => B = L,
(OpCodes.R_C << 3) | OpCodes.R_A => () => C = A,
(OpCodes.R_C << 3) | OpCodes.R_B => () => C = B,
(OpCodes.R_C << 3) | OpCodes.R_C => () => C = C,
(OpCodes.R_C << 3) | OpCodes.R_D => () => C = D,
(OpCodes.R_C << 3) | OpCodes.R_E => () => C = E,
(OpCodes.R_C << 3) | OpCodes.R_H => () => C = H,
(OpCodes.R_C << 3) | OpCodes.R_L => () => C = L,
(OpCodes.R_D << 3) | OpCodes.R_A => () => D = A,
(OpCodes.R_D << 3) | OpCodes.R_B => () => D = B,
(OpCodes.R_D << 3) | OpCodes.R_C => () => D = C,
(OpCodes.R_D << 3) | OpCodes.R_D => () => D = D,
(OpCodes.R_D << 3) | OpCodes.R_E => () => D = E,
(OpCodes.R_D << 3) | OpCodes.R_H => () => D = H,
(OpCodes.R_D << 3) | OpCodes.R_L => () => D = L,
(OpCodes.R_E << 3) | OpCodes.R_A => () => E = A,
(OpCodes.R_E << 3) | OpCodes.R_B => () => E = B,
(OpCodes.R_E << 3) | OpCodes.R_C => () => E = C,
(OpCodes.R_E << 3) | OpCodes.R_D => () => E = D,
(OpCodes.R_E << 3) | OpCodes.R_E => () => E = E,
(OpCodes.R_E << 3) | OpCodes.R_H => () => E = H,
(OpCodes.R_E << 3) | OpCodes.R_L => () => E = L,
(OpCodes.R_H << 3) | OpCodes.R_A => () => H = A,
(OpCodes.R_H << 3) | OpCodes.R_B => () => H = B,
(OpCodes.R_H << 3) | OpCodes.R_C => () => H = C,
(OpCodes.R_H << 3) | OpCodes.R_D => () => H = D,
(OpCodes.R_H << 3) | OpCodes.R_E => () => H = E,
(OpCodes.R_H << 3) | OpCodes.R_H => () => H = H,
(OpCodes.R_H << 3) | OpCodes.R_L => () => H = L,
(OpCodes.R_L << 3) | OpCodes.R_A => () => L = A,
(OpCodes.R_L << 3) | OpCodes.R_B => () => L = B,
(OpCodes.R_L << 3) | OpCodes.R_C => () => L = C,
(OpCodes.R_L << 3) | OpCodes.R_D => () => L = D,
(OpCodes.R_L << 3) | OpCodes.R_E => () => L = E,
(OpCodes.R_L << 3) | OpCodes.R_H => () => L = H,
(OpCodes.R_L << 3) | OpCodes.R_L => () => L = L,

        };
    }
}

