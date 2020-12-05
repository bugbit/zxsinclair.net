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
    // http://www.z80.info/zip/z80cpu_um.pdf
    // http://www.z80.info/zip/z80-documented.pdf
    // https://worldofspectrum.org/z88forever/dn327/z80undoc.htm
    // https://worldofspectrum.org/faq/resources/documents.htm
    // http://biblioteca.museo8bits.es/index.php
    // https://trastero.speccy.org/cosas/Libros/Libros.htm

    public class MachineZ80 : Machine
    {
        protected Regs mRegs = new Regs();

        public MachineZ80() : base()
        {
            mTSatesFetchOpCode = 4;
            mTSatesCounterSync = mTSatesToSync = 224 * 312;
        }

        public Regs Regs => mRegs;

        public override void Reset()
        {
            base.Reset();
            mRegs.Reset();
        }

        protected override byte FetchOpCode() => PeekByte(mRegs.GetPCAndInc());

        protected override void ExecOpCode(int argOpCode)
        {
            mRegs.RefreshR();

            base.ExecOpCode(argOpCode);
        }

        protected override void FillTableOpCodes()
        {
            var q = from r in OpCodes.Rs from r1 in OpCodes.Rs select (r << 3) | r1;

            Parallel.ForEach(q, r_r1 => mOpCodes[OpCodes.LD_r_r1 | r_r1] = mRegs.CreateLDR_R1(r_r1));
        }
    }
}
