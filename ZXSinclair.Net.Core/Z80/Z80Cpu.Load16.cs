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
    private ushort LoadWordAbsolute()
    {
        var address = ReadPc16();
        var low = bus.Read(address);
        var highAddress = (ushort)(address + 1);
        var high = bus.Read(highAddress);
        Registers.WZ = highAddress;
        return (ushort)(low | high << 8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StoreWordAbsolute(ushort value)
    {
        var address = ReadPc16();
        bus.Write(address, (byte)value);
        var highAddress = (ushort)(address + 1);
        bus.Write(highAddress, (byte)(value >> 8));
        Registers.WZ = highAddress;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PushWithDelay(ushort value)
    {
        bus.Internal(Registers.IR, 1);
        Push(value);
    }
}
