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
using System.Text;

namespace ZXSinclair.Machines.Z80
{
    public partial class OpCodes
    {
        public const int R_A = 0b111;
        public const int R_B = 0b000;
        public const int R_C = 0b001;
        public const int R_D = 0b010;
        public const int R_E = 0b011;
        public const int R_H = 0b100;
        public const int R_L = 0b101;

        public static readonly int[] Rs = { R_A, R_B, R_C, R_D, R_E, R_H, R_L };
        public static readonly char[] Car_Rs = { 'A', 'B', 'C', 'D', 'E', 'H', 'L' };

        public const int LD_r_r1 = 0b01000000;
        public const int LD_r_r1_msk = 0b11000000;
    }
}
