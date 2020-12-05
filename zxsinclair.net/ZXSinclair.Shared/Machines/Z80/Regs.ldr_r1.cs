
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
        public Action CreateLDR_R1(int r_r1)
        {
            switch (r_r1)
            {


                case (OpCodes.R_A << 3) | OpCodes.R_A:
                    return () => A = A;


                case (OpCodes.R_A << 3) | OpCodes.R_B:
                    return () => A = B;


                case (OpCodes.R_A << 3) | OpCodes.R_C:
                    return () => A = C;


                case (OpCodes.R_A << 3) | OpCodes.R_D:
                    return () => A = D;


                case (OpCodes.R_A << 3) | OpCodes.R_E:
                    return () => A = E;


                case (OpCodes.R_A << 3) | OpCodes.R_H:
                    return () => A = H;


                case (OpCodes.R_A << 3) | OpCodes.R_L:
                    return () => A = L;


                case (OpCodes.R_B << 3) | OpCodes.R_A:
                    return () => B = A;


                case (OpCodes.R_B << 3) | OpCodes.R_B:
                    return () => B = B;


                case (OpCodes.R_B << 3) | OpCodes.R_C:
                    return () => B = C;


                case (OpCodes.R_B << 3) | OpCodes.R_D:
                    return () => B = D;


                case (OpCodes.R_B << 3) | OpCodes.R_E:
                    return () => B = E;


                case (OpCodes.R_B << 3) | OpCodes.R_H:
                    return () => B = H;


                case (OpCodes.R_B << 3) | OpCodes.R_L:
                    return () => B = L;


                case (OpCodes.R_C << 3) | OpCodes.R_A:
                    return () => C = A;


                case (OpCodes.R_C << 3) | OpCodes.R_B:
                    return () => C = B;


                case (OpCodes.R_C << 3) | OpCodes.R_C:
                    return () => C = C;


                case (OpCodes.R_C << 3) | OpCodes.R_D:
                    return () => C = D;


                case (OpCodes.R_C << 3) | OpCodes.R_E:
                    return () => C = E;


                case (OpCodes.R_C << 3) | OpCodes.R_H:
                    return () => C = H;


                case (OpCodes.R_C << 3) | OpCodes.R_L:
                    return () => C = L;


                case (OpCodes.R_D << 3) | OpCodes.R_A:
                    return () => D = A;


                case (OpCodes.R_D << 3) | OpCodes.R_B:
                    return () => D = B;


                case (OpCodes.R_D << 3) | OpCodes.R_C:
                    return () => D = C;


                case (OpCodes.R_D << 3) | OpCodes.R_D:
                    return () => D = D;


                case (OpCodes.R_D << 3) | OpCodes.R_E:
                    return () => D = E;


                case (OpCodes.R_D << 3) | OpCodes.R_H:
                    return () => D = H;


                case (OpCodes.R_D << 3) | OpCodes.R_L:
                    return () => D = L;


                case (OpCodes.R_E << 3) | OpCodes.R_A:
                    return () => E = A;


                case (OpCodes.R_E << 3) | OpCodes.R_B:
                    return () => E = B;


                case (OpCodes.R_E << 3) | OpCodes.R_C:
                    return () => E = C;


                case (OpCodes.R_E << 3) | OpCodes.R_D:
                    return () => E = D;


                case (OpCodes.R_E << 3) | OpCodes.R_E:
                    return () => E = E;


                case (OpCodes.R_E << 3) | OpCodes.R_H:
                    return () => E = H;


                case (OpCodes.R_E << 3) | OpCodes.R_L:
                    return () => E = L;


                case (OpCodes.R_H << 3) | OpCodes.R_A:
                    return () => H = A;


                case (OpCodes.R_H << 3) | OpCodes.R_B:
                    return () => H = B;


                case (OpCodes.R_H << 3) | OpCodes.R_C:
                    return () => H = C;


                case (OpCodes.R_H << 3) | OpCodes.R_D:
                    return () => H = D;


                case (OpCodes.R_H << 3) | OpCodes.R_E:
                    return () => H = E;


                case (OpCodes.R_H << 3) | OpCodes.R_H:
                    return () => H = H;


                case (OpCodes.R_H << 3) | OpCodes.R_L:
                    return () => H = L;


                case (OpCodes.R_L << 3) | OpCodes.R_A:
                    return () => L = A;


                case (OpCodes.R_L << 3) | OpCodes.R_B:
                    return () => L = B;


                case (OpCodes.R_L << 3) | OpCodes.R_C:
                    return () => L = C;


                case (OpCodes.R_L << 3) | OpCodes.R_D:
                    return () => L = D;


                case (OpCodes.R_L << 3) | OpCodes.R_E:
                    return () => L = E;


                case (OpCodes.R_L << 3) | OpCodes.R_H:
                    return () => L = H;


                case (OpCodes.R_L << 3) | OpCodes.R_L:
                    return () => L = L;


            }

            return null;
        }
    }
}


