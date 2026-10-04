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
    private byte Rlc(byte value)
    {
        var result = (byte)((value << 1) | (value >> 7));
        Registers.F = (byte)(Z80Flags.SZ53P[result] | (value >> 7));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Rrc(byte value)
    {
        var result = (byte)((value >> 1) | (value << 7));
        Registers.F = (byte)(Z80Flags.SZ53P[result] | (value & 1));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Rl(byte value)
    {
        var result = (byte)((value << 1) | (Registers.F & Z80Flags.C));
        Registers.F = (byte)(Z80Flags.SZ53P[result] | (value >> 7));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Rr(byte value)
    {
        var result = (byte)((value >> 1) | ((Registers.F & Z80Flags.C) << 7));
        Registers.F = (byte)(Z80Flags.SZ53P[result] | (value & 1));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Sla(byte value)
    {
        var result = (byte)(value << 1);
        Registers.F = (byte)(Z80Flags.SZ53P[result] | (value >> 7));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Sra(byte value)
    {
        var result = (byte)((value >> 1) | (value & 0x80));
        Registers.F = (byte)(Z80Flags.SZ53P[result] | (value & 1));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Sll(byte value)
    {
        var result = (byte)((value << 1) | 1);
        Registers.F = (byte)(Z80Flags.SZ53P[result] | (value >> 7));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Srl(byte value)
    {
        var result = (byte)(value >> 1);
        Registers.F = (byte)(Z80Flags.SZ53P[result] | (value & 1));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rlca()
    {
        var value = Registers.A;
        var result = (byte)((value << 1) | (value >> 7));
        Registers.A = result;
        Registers.F = (byte)((Registers.F & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | (result & (Z80Flags.F5 | Z80Flags.F3)) | (value >> 7));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rrca()
    {
        var value = Registers.A;
        var result = (byte)((value >> 1) | (value << 7));
        Registers.A = result;
        Registers.F = (byte)((Registers.F & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | (result & (Z80Flags.F5 | Z80Flags.F3)) | (value & 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rla()
    {
        var value = Registers.A;
        var result = (byte)((value << 1) | (Registers.F & Z80Flags.C));
        Registers.A = result;
        Registers.F = (byte)((Registers.F & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | (result & (Z80Flags.F5 | Z80Flags.F3)) | (value >> 7));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rra()
    {
        var value = Registers.A;
        var result = (byte)((value >> 1) | ((Registers.F & Z80Flags.C) << 7));
        Registers.A = result;
        Registers.F = (byte)((Registers.F & (Z80Flags.S | Z80Flags.Z | Z80Flags.PV))
            | (result & (Z80Flags.F5 | Z80Flags.F3)) | (value & 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rld()
    {
        var address = Registers.HL;
        var value = bus.Read(address);
        var a = Registers.A;
        bus.Internal(address, 4);
        bus.Write(address, (byte)((value << 4) | (a & 0x0F)));
        Registers.A = (byte)((a & 0xF0) | (value >> 4));
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.SZ53P[Registers.A]);
        Registers.WZ = (ushort)(address + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rrd()
    {
        var address = Registers.HL;
        var value = bus.Read(address);
        var a = Registers.A;
        bus.Internal(address, 4);
        bus.Write(address, (byte)((a << 4) | (value >> 4)));
        Registers.A = (byte)((a & 0xF0) | (value & 0x0F));
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.SZ53P[Registers.A]);
        Registers.WZ = (ushort)(address + 1);
    }

}

