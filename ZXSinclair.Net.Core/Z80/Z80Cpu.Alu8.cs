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
    private void Add8(byte value)
    {
        var a = Registers.A;
        var result = a + value;
        Registers.A = (byte)result;
        Registers.F = (byte)(Z80Flags.SZ53[(byte)result]
            | ((a ^ value ^ result) & Z80Flags.H)
            | (((a ^ ~value) & (a ^ result) & 0x80) >> 5)
            | ((result >> 8) & Z80Flags.C));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Adc8(byte value)
    {
        var a = Registers.A;
        var result = a + value + (Registers.F & Z80Flags.C);
        Registers.A = (byte)result;
        Registers.F = (byte)(Z80Flags.SZ53[(byte)result]
            | ((a ^ value ^ result) & Z80Flags.H)
            | (((a ^ ~value) & (a ^ result) & 0x80) >> 5)
            | ((result >> 8) & Z80Flags.C));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Sub8(byte value)
    {
        var a = Registers.A;
        var result = a - value;
        Registers.A = (byte)result;
        Registers.F = (byte)(Z80Flags.SZ53[(byte)result] | Z80Flags.N
            | ((a ^ value ^ result) & Z80Flags.H)
            | (((a ^ value) & (a ^ result) & 0x80) >> 5)
            | ((result >> 8) & Z80Flags.C));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Sbc8(byte value)
    {
        var a = Registers.A;
        var result = a - value - (Registers.F & Z80Flags.C);
        Registers.A = (byte)result;
        Registers.F = (byte)(Z80Flags.SZ53[(byte)result] | Z80Flags.N
            | ((a ^ value ^ result) & Z80Flags.H)
            | (((a ^ value) & (a ^ result) & 0x80) >> 5)
            | ((result >> 8) & Z80Flags.C));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void And8(byte value)
    {
        Registers.A = (byte)(Registers.A & value);
        Registers.F = (byte)(Z80Flags.SZ53P[Registers.A] | Z80Flags.H);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Xor8(byte value)
    {
        Registers.A = (byte)(Registers.A ^ value);
        Registers.F = (byte)(Z80Flags.SZ53P[Registers.A]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Or8(byte value)
    {
        Registers.A = (byte)(Registers.A | value);
        Registers.F = (byte)(Z80Flags.SZ53P[Registers.A]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Cp8(byte value)
    {
        var a = Registers.A;
        var result = a - value;
        Registers.F = (byte)((Z80Flags.SZ53[(byte)result] & ~(Z80Flags.F5 | Z80Flags.F3))
            | (value & (Z80Flags.F5 | Z80Flags.F3)) | Z80Flags.N
            | ((a ^ value ^ result) & Z80Flags.H)
            | (((a ^ value) & (a ^ result) & 0x80) >> 5)
            | ((result >> 8) & Z80Flags.C));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Inc8(byte value)
    {
        var result = (byte)(value + 1);
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.Inc[result]);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IncMemory(ushort address)
    {
        var value = bus.Read(address);
        bus.Internal(address, 1);
        bus.Write(address, Inc8(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Dec8(byte value)
    {
        var result = (byte)(value - 1);
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.Dec[result]);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DecMemory(ushort address)
    {
        var value = bus.Read(address);
        bus.Internal(address, 1);
        bus.Write(address, Dec8(value));
    }

}
