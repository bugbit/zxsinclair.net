#region LICENSE
/*
    ZXSinclair Emulador ZX Computers make in .Net and .Net CORE
    Copyright (C) 2016 Oscar Hernandez Bano
    This file is part of ZEsarUX.
    ZEsarUX is free software: you can redistribute it and/or modify
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

namespace ZXSinclair.CPU.Z80
{
    public class Ops
    {
        private Regs mRegs = new Regs();

        public Regs Regs => mRegs;

        public void Ld_R1_R2(Action<byte> argSet, Func<byte> argGet)
        {
            mRegs.PC += 2;
            argSet.Invoke(argGet.Invoke());
            mRegs.PC += 2;
        }
        public void Ld_R1_R2(Action argSetGet)
        {
            mRegs.PC += 2;
            argSetGet.Invoke();
            mRegs.PC += 2;
        }
        public void LdA_B_1()
        {
            mRegs.PC += 2;
            mRegs.A = mRegs.B;
            mRegs.PC += 2;
        }

        public void LdA_B_2() => Ld_R1_R2(mRegs.CreateSetterA(), mRegs.CreateGetterB());

        public void LdA_B_3() => Ld_R1_R2(() => mRegs.A = mRegs.B);
    }
}
