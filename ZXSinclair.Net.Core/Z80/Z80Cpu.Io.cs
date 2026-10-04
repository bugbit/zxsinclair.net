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
    private void InAccumulator()
    {
        var port = (ushort)((Registers.A << 8) | ReadPc());
        Registers.WZ = (ushort)(port + 1);
        Registers.A = bus.In(port);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OutAccumulator()
    {
        var low = ReadPc();
        bus.Out((ushort)((Registers.A << 8) | low), Registers.A);
        Registers.WZ = (ushort)((Registers.A << 8) | ((low + 1) & 255));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte InRegister()
    {
        Registers.WZ = (ushort)(Registers.BC + 1);
        var value = bus.In(Registers.BC);
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.SZ53P[value]);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OutRegister(byte value)
    {
        bus.Out(Registers.BC, value);
        Registers.WZ = (ushort)(Registers.BC + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockIn(int step) => BlockInCore(step, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockInRepeat(int step) => BlockInCore(step, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockInCore(int step, bool repeat)
    {
        bus.Internal(Registers.IR, 1);
        Registers.WZ = (ushort)(Registers.BC + step);
        var value = bus.In(Registers.BC);
        var address = Registers.HL;
        bus.Write(address, value);
        Registers.B--;
        Registers.HL = (ushort)(address + step);
        BlockIoFlags(value, value + ((Registers.C + step) & 255));
        if (!repeat || Registers.B == 0) return;
        bus.Internal(address, 5);
        Registers.PC -= 2;
        Registers.WZ = (ushort)(Registers.PC + 1);
        BlockIoRepeatFlags(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockOut(int step) => BlockOutCore(step, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockOutRepeat(int step) => BlockOutCore(step, true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockOutCore(int step, bool repeat)
    {
        bus.Internal(Registers.IR, 1);
        var value = bus.Read(Registers.HL);
        Registers.B--;
        bus.Out(Registers.BC, value);
        Registers.HL = (ushort)(Registers.HL + step);
        Registers.WZ = (ushort)(Registers.BC + step);
        BlockIoFlags(value, value + Registers.L);
        if (!repeat || Registers.B == 0) return;
        bus.Internal(Registers.BC, 5);
        Registers.PC -= 2;
        Registers.WZ = (ushort)(Registers.PC + 1);
        BlockIoRepeatFlags(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockIoFlags(byte value, int sum)
    {
        Registers.F = (byte)(Z80Flags.SZ53[Registers.B] | ((value >> 6) & Z80Flags.N)
            | (sum > 255 ? Z80Flags.H | Z80Flags.C : 0)
            | Z80Flags.Parity[(sum & 7) ^ Registers.B]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockIoRepeatFlags(byte value)
    {
        var flags = (Registers.F & ~0x28) | ((Registers.PC >> 8) & 0x28);
        var parityValue = Registers.B;
        if ((flags & Z80Flags.C) != 0)
        {
            flags &= ~Z80Flags.H;
            if ((value & 0x80) != 0)
            {
                parityValue = (byte)(Registers.B - 1);
                if ((Registers.B & 15) == 0) flags |= Z80Flags.H;
            }
            else
            {
                parityValue = (byte)(Registers.B + 1);
                if ((Registers.B & 15) == 15) flags |= Z80Flags.H;
            }
        }
        Registers.F = (byte)(flags ^ ((~Z80Flags.Parity[parityValue & 7]) & Z80Flags.PV));
    }
}
