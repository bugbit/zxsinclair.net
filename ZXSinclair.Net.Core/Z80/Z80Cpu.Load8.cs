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
    private void StoreIndexedImmediate<TIndex>() where TIndex : struct, IIndexRegister
    {
        var displacement = (sbyte)ReadPc();
        var value = ReadPc();
        bus.Internal((ushort)(Registers.PC - 1), 2);
        var address = (ushort)(TIndex.Pair(ref Registers) + displacement);
        Registers.WZ = address;
        bus.Write(address, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LoadAFromSpecial(byte value)
    {
        Registers.SpecialLoadPending = true;
        bus.Internal(Registers.IR, 1);
        Registers.A = value;
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.SZ53[value]
            | (Registers.IFF2 ? Z80Flags.PV : 0));
    }
}
