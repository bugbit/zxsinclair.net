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

using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Fuse;

public static class FuseCpuState
{
    public static Z80Registers Load(clsTestBase test)
    {
        var a = test.Line1;
        var b = test.Line2;
        return new Z80Registers
        {
            AF = a.af, BC = a.bc, DE = a.de, HL = a.hl,
            AF_ = a.af_, BC_ = a.bc_, DE_ = a.de_, HL_ = a.hl_,
            IX = a.ix, IY = a.iy, SP = a.sp, PC = a.pc,
            I = (byte)b.i, R = (byte)b.r, IFF1 = b.iff1 != 0,
            IFF2 = b.iff2 != 0, IM = (byte)b.im, Halted = b.halted != 0,
        };
    }

    public static string? Compare(clsTestBase expected, in Z80Registers actual, int cycles)
    {
        var e = Load(expected);
        if (e.AF != actual.AF) return Difference("AF", e.AF, actual.AF);
        if (e.BC != actual.BC) return Difference("BC", e.BC, actual.BC);
        if (e.DE != actual.DE) return Difference("DE", e.DE, actual.DE);
        if (e.HL != actual.HL) return Difference("HL", e.HL, actual.HL);
        if (e.AF_ != actual.AF_) return Difference("AF'", e.AF_, actual.AF_);
        if (e.BC_ != actual.BC_) return Difference("BC'", e.BC_, actual.BC_);
        if (e.DE_ != actual.DE_) return Difference("DE'", e.DE_, actual.DE_);
        if (e.HL_ != actual.HL_) return Difference("HL'", e.HL_, actual.HL_);
        if (e.IX != actual.IX) return Difference("IX", e.IX, actual.IX);
        if (e.IY != actual.IY) return Difference("IY", e.IY, actual.IY);
        if (e.SP != actual.SP) return Difference("SP", e.SP, actual.SP);
        if (e.PC != actual.PC) return Difference("PC", e.PC, actual.PC);
        if (e.I != actual.I) return Difference("I", e.I, actual.I);
        if (e.R != actual.R) return Difference("R", e.R, actual.R);
        if (e.IFF1 != actual.IFF1) return $"IFF1: expected {e.IFF1}, actual {actual.IFF1}";
        if (e.IFF2 != actual.IFF2) return $"IFF2: expected {e.IFF2}, actual {actual.IFF2}";
        if (e.IM != actual.IM) return Difference("IM", e.IM, actual.IM);
        if (e.Halted != actual.Halted) return $"halted: expected {e.Halted}, actual {actual.Halted}";
        return expected.Line2.endtstates == cycles ? null
            : $"T-states: expected {expected.Line2.endtstates}, actual {cycles}";
    }

    private static string Difference(string name, ushort expected, ushort actual) =>
        $"{name}: expected {expected:x4}, actual {actual:x4}";
}
