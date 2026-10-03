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
using ZXSinclair.Net.Fuse;

namespace ZXSinclair.Net.Core.Tests;

public class FuseTestBusTests
{
    // Parsed again on every call: some tests modify the fixture they receive.
    private static clsTestExpected Fixture(string name)
    {
        Assert.True(FuseTestFile.LoadExpected().TryGetValue(name, out var expected), $"Missing fixture {name}");
        return expected!;
    }

    [Fact]
    public void OpcodeFetchMatchesRealFuseNopEvents()
    {
        var expected = Fixture("00");
        var state = new FuseTestBusState { Events = new() };
        var bus = new FuseTestBus(state);
        Assert.Equal((byte)0, bus.FetchOpcode(0));
        Assert.Equal(4, state.Cycles);
        Assert.Null(FuseComparison.CompareEvents(expected, state.Events!));
        state.Events![1] = state.Events[1] with { Time = 3 };
        Assert.Contains("bus event 1", FuseComparison.CompareEvents(expected, state.Events)!);
    }

    [Theory]
    [InlineData("20_2", new byte[] { 0x20, 0x40 })]
    [InlineData("c2_2", new byte[] { 0xC2, 0x1B, 0xE1 })]
    public void NotTakenOperandReadsMatchRealFuseEvents(string name, byte[] program)
    {
        var expected = Fixture(name);
        var state = new FuseTestBusState { Events = new() };
        program.CopyTo(state.Memory, 0);
        Assert.Equal(program[1], new FuseTestBus(state).ReadDiscarded(1));
        Assert.Equal(3, state.Cycles);
        Assert.Equal(new Z80BusEvent(0, Z80BusEventType.MC, 1, null), Assert.Single(state.Events!));

        state = new FuseTestBusState { Events = new() };
        program.CopyTo(state.Memory, 0);
        var cpu = new Z80Cpu<FuseTestBus>(new(state));
        cpu.Registers.F = (byte)(name == "20_2" ? 0x40 : 0xC7);
        cpu.Step();
        Assert.Null(FuseComparison.CompareEvents(expected, state.Events!));
    }

    [Fact]
    public void IndexedPrefixCyclesMatchRealFuseEvents()
    {
        var expected = Fixture("ddcb00");
        var input = FuseTestFile.LoadInputs().Single(t => t.Base.Name == "ddcb00");
        var state = new FuseTestBusState { Events = new() };
        foreach (var block in input.Base.Memories)
            block.Data.CopyTo(state.Memory, block.Address);
        var cpu = new Z80Cpu<FuseTestBus>(new(state));
        cpu.Registers = FuseCpuState.Load(input.Base);
        cpu.Step();
        Assert.Equal(23, state.Cycles);
        Assert.Null(FuseComparison.CompareEvents(expected, state.Events!));
    }

    [Theory]
    [InlineData("d3", 0xA2EC, false)]
    [InlineData("d3_1", 0xA2ED, false)]
    [InlineData("d3_2", 0x42EC, false)]
    [InlineData("d3_3", 0x42ED, false)]
    [InlineData("db", 0xC1E2, true)]
    [InlineData("db_1", 0xC1E3, true)]
    [InlineData("db_2", 0x71E2, true)]
    [InlineData("db_3", 0x71E3, true)]
    public void IoMatchesAllFourRealFuseContentionSequences(string name, ushort port, bool input)
    {
        var expected = Fixture(name);
        var state = new FuseTestBusState { Events = new() };
        state.Memory[0] = (byte)(input ? 0xDB : 0xD3);
        state.Memory[1] = (byte)port;
        var bus = new FuseTestBus(state);
        bus.FetchOpcode(0);
        bus.Read(1);
        if (input)
            Assert.Equal((byte)(port >> 8), bus.In(port));
        else
            bus.Out(port, (byte)(port >> 8));
        Assert.Equal(11, state.Cycles);
        Assert.Null(FuseComparison.CompareEvents(expected, state.Events!));
    }

    [Fact]
    public void MemoryAndInternalCyclesHaveExactTimesAndData()
    {
        var state = new FuseTestBusState { Events = new() };
        var bus = new FuseTestBus(state);
        bus.Write(0x8000, 0x42);
        Assert.Equal((byte)0x42, bus.Read(0x8000));
        bus.Internal(0x1234, 2);
        Assert.Equal(8, state.Cycles);
        Assert.Equal(new Z80BusEvent[]
        {
            new(0, Z80BusEventType.MC, 0x8000, null),
            new(3, Z80BusEventType.MW, 0x8000, 0x42),
            new(3, Z80BusEventType.MC, 0x8000, null),
            new(6, Z80BusEventType.MR, 0x8000, 0x42),
            new(6, Z80BusEventType.MC, 0x1234, null),
            new(7, Z80BusEventType.MC, 0x1234, null),
        }, state.Events!);
        Assert.False(bus.IntActive);
        Assert.Equal((byte)0xFF, bus.AcknowledgeInterrupt());
        Assert.Equal(14, state.Cycles);
        bus.Reset();
        Assert.Equal(0, bus.Cycles);
        Assert.Equal((byte)0x42, state.Memory[0x8000]);
    }

    [Fact]
    public void RecordingCanBeDisabledWithoutChangingBusBehavior()
    {
        var state = new FuseTestBusState();
        var bus = new FuseTestBus(state);
        bus.FetchOpcode(0);
        bus.Write(1, 0x12);
        bus.Internal(0x1234, 2);
        Assert.Equal((byte)0x40, bus.In(0x4001));
        Assert.Equal(13, state.Cycles);
        Assert.Null(state.Events);
    }

    [Fact]
    public void MemoryComparisonAppliesExpectedChangesAndDetectsMismatches()
    {
        var initial = new byte[65536];
        initial[0x1234] = 0x12;
        var expected = new clsTestExpected();
        expected.Base.Memories = new[] { new clsTestMemory { Address = 0xFFFF, Data = new byte[] { 0xAB } } };
        var memory = FuseComparison.ExpectedMemory(initial, expected);
        Assert.Equal((byte)0, initial[0xFFFF]);
        Assert.Equal((byte)0x12, memory[0x1234]);
        Assert.Null(FuseComparison.CompareMemory(memory, a => memory[a]));
        Assert.Contains("ffff", FuseComparison.CompareMemory(memory, a => initial[a])!);
    }

    [Theory]
    [InlineData("AF")]
    [InlineData("BC")]
    [InlineData("DE")]
    [InlineData("HL")]
    [InlineData("AF_")]
    [InlineData("BC_")]
    [InlineData("DE_")]
    [InlineData("HL_")]
    [InlineData("IX")]
    [InlineData("IY")]
    [InlineData("SP")]
    [InlineData("PC")]
    [InlineData("I")]
    [InlineData("R")]
    [InlineData("IFF1")]
    [InlineData("IFF2")]
    [InlineData("IM")]
    [InlineData("Halted")]
    public void StateComparisonDetectsEveryFuseRegister(string fieldName)
    {
        var expected = Fixture("00");
        var registers = FuseCpuState.Load(expected.Base);
        Assert.Null(FuseCpuState.Compare(expected.Base, registers, 4));
        var field = typeof(Z80Registers).GetField(fieldName)!;
        object boxed = registers;
        var value = field.GetValue(boxed);
        object changed = value switch
        {
            bool b => !b,
            byte b => (byte)(b ^ 1),
            ushort u => (ushort)(u ^ 1),
            _ => throw new InvalidOperationException(),
        };
        field.SetValue(boxed, changed);
        registers = (Z80Registers)boxed;
        Assert.NotNull(FuseCpuState.Compare(expected.Base, registers, 4));
    }

    [Fact]
    public void ComparisonsDetectWrongCycleCountAndMissingOrExtraEvents()
    {
        var expected = Fixture("00");
        var registers = FuseCpuState.Load(expected.Base);
        Assert.Contains("T-states", FuseCpuState.Compare(expected.Base, registers, 5)!);
        var events = expected.Events.Select(e => e.ToBusEvent()).ToList();
        events.RemoveAt(1);
        Assert.NotNull(FuseComparison.CompareEvents(expected, events));
        events = expected.Events.Select(e => e.ToBusEvent()).ToList();
        events.Add(events[0]);
        Assert.NotNull(FuseComparison.CompareEvents(expected, events));
    }
}
