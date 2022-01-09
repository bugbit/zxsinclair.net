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
    public partial class MachineZ80
    {
        protected void FillTableOpCodesDD()
        {
            FillTableOpCodes
            (
                mOpCodesDD, new Dictionary<byte, Action>
                {
                    [OpCodes.LD_A_M_IX_D_M] = LD_A_M_IX_D_M,
                    [OpCodes.LD_B_M_IX_D_M] = LD_B_M_IX_D_M,
                    [OpCodes.LD_C_M_IX_D_M] = LD_C_M_IX_D_M,
                    [OpCodes.LD_D_M_IX_D_M] = LD_D_M_IX_D_M,
                    [OpCodes.LD_E_M_IX_D_M] = LD_E_M_IX_D_M,
                    [OpCodes.LD_H_M_IX_D_M] = LD_H_M_IX_D_M,
                    [OpCodes.LD_L_M_IX_D_M] = LD_L_M_IX_D_M,
                    [OpCodes.LD_M_IX_D_M_A] = LD_M_IX_D_M_A,
                    [OpCodes.LD_M_IX_D_M_B] = LD_M_IX_D_M_B,
                    [OpCodes.LD_M_IX_D_M_C] = LD_M_IX_D_M_C,
                    [OpCodes.LD_M_IX_D_M_D] = LD_M_IX_D_M_D,
                    [OpCodes.LD_M_IX_D_M_E] = LD_M_IX_D_M_E,
                    [OpCodes.LD_M_IX_D_M_H] = LD_M_IX_D_M_H,
                    [OpCodes.LD_M_IX_D_M_L] = LD_M_IX_D_M_L,
                    [OpCodes.LD_M_IX_D_M_N] = LD_M_IX_D_M_N,
                }
            );
        }

        // LD A,(IX+d)
        protected void LD_A_M_IX_D_M()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            var n = ReadMemByte(mRegs.GetIX_d(d));

            mRegs.A = n;
        }

        // LD B,(IX+d)
        protected void LD_B_M_IX_D_M()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            var n = ReadMemByte(mRegs.GetIX_d(d));

            mRegs.B = n;
        }

        // LD C,(IX+d)
        protected void LD_C_M_IX_D_M()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            var n = ReadMemByte(mRegs.GetIX_d(d));

            mRegs.C = n;
        }

        // LD D,(IX+d)
        protected void LD_D_M_IX_D_M()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            var n = ReadMemByte(mRegs.GetIX_d(d));

            mRegs.D = n;
        }

        // LD E,(IX+d)
        protected void LD_E_M_IX_D_M()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            var n = ReadMemByte(mRegs.GetIX_d(d));

            mRegs.E = n;
        }

        // LD H,(IX+d)
        protected void LD_H_M_IX_D_M()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            var n = ReadMemByte(mRegs.GetIX_d(d));

            mRegs.H = n;
        }

        // LD L,(IX+d)
        protected void LD_L_M_IX_D_M()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            var n = ReadMemByte(mRegs.GetIX_d(d));

            mRegs.L = n;
        }

        // LD (IX+d),A
        protected void LD_M_IX_D_M_A()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIX_d(d), mRegs.A);
        }

        // LD (IX+d),B
        protected void LD_M_IX_D_M_B()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIX_d(d), mRegs.B);
        }

        // LD (IX+d),C
        protected void LD_M_IX_D_M_C()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIX_d(d), mRegs.C);
        }

        // LD (IX+d),D
        protected void LD_M_IX_D_M_D()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIX_d(d), mRegs.D);
        }

        // LD (IX+d),E
        protected void LD_M_IX_D_M_E()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIX_d(d), mRegs.E);
        }

        // LD (IX+d),H
        protected void LD_M_IX_D_M_H()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIX_d(d), mRegs.H);
        }

        // LD (IX+d),L
        protected void LD_M_IX_D_M_L()
        {
            var d = (sbyte)ReadMemBytePCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIX_d(d), mRegs.L);
        }
    }
}
