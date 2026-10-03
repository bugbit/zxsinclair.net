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

using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Core.Tests;

internal sealed class TestBusState
{
    public byte[] Memory { get; } = new byte[0x10000];
    public int Cycles;
    public bool IntActive;
    public int IntAfterCycle = int.MaxValue;
    public byte InterruptData = 0xFF;
    public Queue<byte> InputData { get; } = new();
    public List<(string Kind, ushort Address, int Value)> Accesses { get; } = new();
}

internal struct TestBus : IZ80Bus
{
    private readonly TestBusState state;
    public TestBus(TestBusState state) => this.state = state;
    public int Cycles => state.Cycles;
    public bool IntActive => state.IntActive || state.Cycles >= state.IntAfterCycle;
    public void Reset() => state.Cycles = 0;
    public byte FetchOpcode(ushort address) => Read(address, 4, "M1");
    public byte Read(ushort address) => Read(address, 3, "Read");
    public byte ReadDiscarded(ushort address) => Read(address, 3, "ReadDiscarded");
    private byte Read(ushort address, int cost, string kind)
    {
        var value = state.Memory[address];
        state.Accesses.Add((kind, address, value));
        state.Cycles += cost;
        return value;
    }
    public void Write(ushort address, byte data)
    {
        state.Accesses.Add(("Write", address, data));
        state.Memory[address] = data;
        state.Cycles += 3;
    }
    public void Internal(ushort address, int tstates)
    {
        state.Accesses.Add(("Internal", address, tstates));
        state.Cycles += tstates;
    }
    public byte In(ushort port)
    {
        var value = state.InputData.Count != 0 ? state.InputData.Dequeue() : (byte)(port >> 8);
        state.Accesses.Add(("In", port, value));
        state.Cycles += 4;
        return value;
    }
    public void Out(ushort port, byte data)
    {
        state.Accesses.Add(("Out", port, data));
        state.Cycles += 4;
    }
    public byte AcknowledgeInterrupt()
    {
        state.Accesses.Add(("Ack", 0, state.InterruptData));
        state.Cycles += 7;
        return state.InterruptData;
    }
}
