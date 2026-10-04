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

public sealed partial class Z80Cpu<TBus> where TBus : struct, IZ80Bus
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BitTest(byte mask, byte value)
    {
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.H
            | (Z80Flags.SZ53P[value & mask] & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | (value & (Z80Flags.F5 | Z80Flags.F3)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BitTestMemory(byte mask, byte value)
    {
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.H
            | (Z80Flags.SZ53P[value & mask] & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | ((Registers.WZ >> 8) & (Z80Flags.F5 | Z80Flags.F3)));
    }
}
