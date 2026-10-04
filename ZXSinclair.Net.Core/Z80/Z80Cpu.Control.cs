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

using System.Runtime.CompilerServices;

namespace ZXSinclair.Net.Core.Z80;

public sealed partial class Z80Cpu<TBus>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DecimalAdjust()
    {
        var a = Registers.A;
        var f = Registers.F;
        var carry = (f & Z80Flags.C) != 0 || a > 0x99;
        var half = (f & Z80Flags.H) != 0;
        var subtract = (f & Z80Flags.N) != 0;
        var correction = (carry ? 0x60 : 0) | (half || (a & 0x0F) > 9 ? 0x06 : 0);
        Registers.A = (byte)(subtract ? a - correction : a + correction);
        var newHalf = subtract ? half && (a & 0x0F) < 6 : (a & 0x0F) > 9;
        Registers.F = (byte)(Z80Flags.SZ53P[Registers.A] | (f & Z80Flags.N)
            | (carry ? Z80Flags.C : 0) | (newHalf ? Z80Flags.H : 0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Complement()
    {
        Registers.A = (byte)~Registers.A;
        Registers.F = (byte)((Registers.F & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV | Z80Flags.C))
            | Z80Flags.H | Z80Flags.N | (Registers.A & (Z80Flags.F5 | Z80Flags.F3)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Negate()
    {
        var value = Registers.A;
        Registers.A = 0;
        Sub8(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetCarry()
    {
        var f = Registers.F;
        Registers.F = (byte)((f & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | (((Registers.Q ^ f) | Registers.A) & (Z80Flags.F5 | Z80Flags.F3)) | Z80Flags.C);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComplementCarry()
    {
        var f = Registers.F;
        var carry = f & Z80Flags.C;
        Registers.F = (byte)((f & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | (carry << 4) | (carry ^ Z80Flags.C)
            | (((Registers.Q ^ f) | Registers.A) & (Z80Flags.F5 | Z80Flags.F3)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Halt()
    {
        Registers.Halted = true;
        Registers.PC--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnableInterrupts()
    {
        Registers.IFF1 = Registers.IFF2 = true;
        Registers.EiPending = true;
    }
}

