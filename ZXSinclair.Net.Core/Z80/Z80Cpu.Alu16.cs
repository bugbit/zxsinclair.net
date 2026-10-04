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
    private ushort Add16(ushort a, ushort value)
    {
        bus.Internal(Registers.IR, 7);
        Registers.WZ = (ushort)(a + 1);
        var result = a + value;
        Registers.F = (byte)((Registers.F & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | ((result >> 8) & (Z80Flags.F5 | Z80Flags.F3))
            | (((a ^ value ^ result) >> 8) & Z80Flags.H)
            | ((result >> 16) & Z80Flags.C));
        return (ushort)result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Adc16(ushort value)
    {
        var a = Registers.HL;
        bus.Internal(Registers.IR, 7);
        Registers.WZ = (ushort)(a + 1);
        var result = a + value + (Registers.F & Z80Flags.C);
        Registers.HL = (ushort)result;
        Registers.F = (byte)(((result >> 8) & (Z80Flags.S | Z80Flags.F5 | Z80Flags.F3))
            | (((result & 0xFFFF) == 0) ? Z80Flags.Z : 0)
            | (((a ^ value ^ result) >> 8) & Z80Flags.H)
            | (((a ^ ~value) & (a ^ result) & 0x8000) >> 13)
            | ((result >> 16) & Z80Flags.C));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Sbc16(ushort value)
    {
        var a = Registers.HL;
        bus.Internal(Registers.IR, 7);
        Registers.WZ = (ushort)(a + 1);
        var result = a - value - (Registers.F & Z80Flags.C);
        Registers.HL = (ushort)result;
        Registers.F = (byte)(((result >> 8) & (Z80Flags.S | Z80Flags.F5 | Z80Flags.F3))
            | (((result & 0xFFFF) == 0) ? Z80Flags.Z : 0)
            | (((a ^ value ^ result) >> 8) & Z80Flags.H)
            | (((a ^ value) & (a ^ result) & 0x8000) >> 13)
            | Z80Flags.N
            | ((result >> 16) & Z80Flags.C));
    }

}

