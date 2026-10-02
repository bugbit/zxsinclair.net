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
    private void JumpAbsolute(bool condition)
    {
        if (condition)
            Registers.PC = Registers.WZ = ReadPc16();
        else
            Registers.WZ = ReadPc16Discarded();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void JumpRelative(bool condition)
    {
        if (condition)
        {
            var displacement = (sbyte)ReadPc();
            bus.Internal((ushort)(Registers.PC - 1), 5);
            Registers.PC = (ushort)(Registers.PC + displacement);
            Registers.WZ = Registers.PC;
        }
        else
            ReadPcDiscarded();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DecrementJumpNonZero()
    {
        bus.Internal(Registers.IR, 1);
        Registers.B--;
        JumpRelative(Registers.B != 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CallAbsolute(bool condition)
    {
        if (condition)
        {
            var address = ReadPc16();
            Registers.WZ = address;
            bus.Internal((ushort)(Registers.PC - 1), 1);
            Push(Registers.PC);
            Registers.PC = address;
        }
        else
            Registers.WZ = ReadPc16Discarded();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Return()
    {
        Registers.PC = Pop();
        Registers.WZ = Registers.PC;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnConditional(bool condition)
    {
        bus.Internal(Registers.IR, 1);
        if (condition)
            Return();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnFromInterrupt()
    {
        Registers.IFF1 = Registers.IFF2;
        Return();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Restart(ushort address)
    {
        bus.Internal(Registers.IR, 1);
        Push(Registers.PC);
        Registers.PC = Registers.WZ = address;
    }
}
