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
using ZXSinclair.Net.Core.Abstractions;

namespace ZXSinclair.Net.Core.Z80;

public sealed partial class Z80Cpu<TBus> : ICpu where TBus : struct, IZ80Bus
{
    // Mutable to avoid defensive copies of generic struct buses.
    private TBus bus;
    private bool nmiPending;
    public Z80Registers Registers;
    public int UnimplementedOpcodes { get; private set; }
    /// <summary>PC when the last unimplemented opcode was recorded; ordinary fetches leave it just after the opcode.</summary>
    public ushort LastUnimplementedAddress { get; private set; }
    public bool Halted => Registers.Halted;

    public Z80Cpu(TBus bus)
    {
        this.bus = bus;
        Reset();
    }

    public void Reset()
    {
        Registers = default;
        Registers.AF = Registers.SP = 0xFFFF;
        nmiPending = false;
        UnimplementedOpcodes = 0;
        LastUnimplementedAddress = 0;
    }

    public void RequestNmi() => nmiPending = true;

    public void Execute(int targetCycles)
    {
        while (bus.Cycles < targetCycles)
            Step();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Step()
    {
        if (nmiPending)
        {
            AcceptNmi();
            return;
        }
        if (Registers.IFF1 && !Registers.EiPending && bus.IntActive)
        {
            AcceptInterrupt();
            return;
        }
        Registers.EiPending = false;
        Registers.SpecialLoadPending = false;
        if (Registers.Halted)
        {
            Registers.Q = 0;
            bus.FetchOpcode(Registers.PC);
            Registers.IncrementR();
            return;
        }
        var opcode = FetchOpcode();
        switch (opcode)
        {
            case 0xCB: ExecuteCB(FetchOpcode()); break;
            case 0xED: ExecuteED(FetchOpcode()); break;
            case 0xDD: ExecuteIndexed<IxRegister>(); break;
            case 0xFD: ExecuteIndexed<IyRegister>(); break;
            default: ExecuteMain(opcode); break;
        }
    }

    private void ExecuteIndexed<TIndex>() where TIndex : struct, IIndexRegister
    {
        // Constant for each generic instantiation; repeated prefixes use constant stack space.
        var iy = typeof(TIndex) == typeof(IyRegister);
        byte opcode;
        while (true)
        {
            opcode = FetchOpcode();
            if (opcode != 0xDD && opcode != 0xFD)
                break;
            iy = opcode == 0xFD;
        }
        if (iy)
            FinishIndexed<IyRegister>(opcode);
        else
            FinishIndexed<IxRegister>(opcode);
    }

    private void FinishIndexed<TIndex>(byte opcode) where TIndex : struct, IIndexRegister
    {
        if (opcode == 0xED)
            ExecuteED(FetchOpcode());
        else if (opcode == 0xCB)
        {
            var displacement = (sbyte)ReadPc();
            var opcodeAddress = Registers.PC;
            var indexedOpcode = ReadPc();
            bus.Internal(opcodeAddress, 2);
            var address = unchecked((ushort)(TIndex.Pair(ref Registers) + displacement));
            Registers.WZ = address;
            ExecuteIndexedCB<TIndex>(address, indexedOpcode);
        }
        else
            ExecuteIndexedOpcode<TIndex>(opcode);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte FetchOpcode()
    {
        var opcode = bus.FetchOpcode(Registers.PC++);
        Registers.IncrementR();
        return opcode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadPc() => bus.Read(Registers.PC++);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort ReadPc16()
    {
        var low = ReadPc();
        return (ushort)(low | ReadPc() << 8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadPcDiscarded() => bus.ReadDiscarded(Registers.PC++);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort ReadPc16Discarded()
    {
        var low = ReadPcDiscarded();
        return (ushort)(low | ReadPcDiscarded() << 8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort IndexedAddress<TIndex>() where TIndex : struct, IIndexRegister
    {
        var displacement = (sbyte)ReadPc();
        bus.Internal((ushort)(Registers.PC - 1), 5);
        var address = (ushort)(TIndex.Pair(ref Registers) + displacement);
        Registers.WZ = address;
        return address;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Push(ushort value)
    {
        bus.Write(--Registers.SP, (byte)(value >> 8));
        bus.Write(--Registers.SP, (byte)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort Pop()
    {
        var low = bus.Read(Registers.SP++);
        return (ushort)(low | bus.Read(Registers.SP++) << 8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Unimplemented()
    {
        UnimplementedOpcodes++;
        LastUnimplementedAddress = Registers.PC;
    }
}
