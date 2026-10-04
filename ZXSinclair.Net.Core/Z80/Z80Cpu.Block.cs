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
    private ushort ExchangeStack(ushort value)
    {
        var low = bus.Read(Registers.SP);
        var highAddress = (ushort)(Registers.SP + 1);
        var high = bus.Read(highAddress);
        bus.Internal(highAddress, 1);
        bus.Write(highAddress, (byte)(value >> 8));
        bus.Write(Registers.SP, (byte)value);
        bus.Internal(Registers.SP, 2);
        Registers.WZ = (ushort)(low | (high << 8));
        return Registers.WZ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockLoad(int step)
    {
        var value = bus.Read(Registers.HL);
        bus.Write(Registers.DE, value);
        bus.Internal(Registers.DE, 2);
        Registers.HL = (ushort)(Registers.HL + step);
        Registers.DE = (ushort)(Registers.DE + step);
        Registers.BC--;
        var sum = Registers.A + value;
        Registers.F = (byte)((Registers.F & (Z80Flags.S | Z80Flags.Z | Z80Flags.C))
            | (Registers.BC != 0 ? Z80Flags.PV : 0)
            | (sum & Z80Flags.F3) | ((sum << 4) & Z80Flags.F5));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockLoadRepeat(int step)
    {
        var address = Registers.DE;
        BlockLoad(step);
        if (Registers.BC == 0) return;
        RepeatBlock(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockCompare(int step)
    {
        var value = bus.Read(Registers.HL);
        bus.Internal(Registers.HL, 5);
        var result = Registers.A - value;
        var half = (Registers.A ^ value ^ result) & Z80Flags.H;
        var adjusted = result - (half >> 4);
        Registers.HL = (ushort)(Registers.HL + step);
        Registers.BC--;
        Registers.WZ = (ushort)(Registers.WZ + step);
        Registers.F = (byte)((Registers.F & Z80Flags.C) | Z80Flags.N
            | (Z80Flags.SZ53[(byte)result] & (Z80Flags.S | Z80Flags.Z)) | half
            | (Registers.BC != 0 ? Z80Flags.PV : 0)
            | (adjusted & Z80Flags.F3) | ((adjusted << 4) & Z80Flags.F5));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlockCompareRepeat(int step)
    {
        var address = Registers.HL;
        BlockCompare(step);
        if (Registers.BC == 0 || (Registers.F & Z80Flags.Z) != 0) return;
        RepeatBlock(address);
    }

    // Extra 5 T-state M-cycle of a repeating block instruction: internal cycles, PC back to the ED
    // prefix, WZ = PC + 1 and F5/F3 from the PC high byte (spec-instr-bloques.md 2.2-2.3, spec-instr-io.md 2.2-2.3).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RepeatBlock(ushort internalAddress)
    {
        bus.Internal(internalAddress, 5);
        Registers.PC -= 2;
        Registers.WZ = (ushort)(Registers.PC + 1);
        Registers.F = (byte)((Registers.F & ~(Z80Flags.F5 | Z80Flags.F3))
            | ((Registers.PC >> 8) & (Z80Flags.F5 | Z80Flags.F3)));
    }
}
