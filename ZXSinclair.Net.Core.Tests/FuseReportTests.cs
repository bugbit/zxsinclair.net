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

public class FuseReportTests
{
    [Fact]
    public void Convention_IgnoresOnlyDeclaredFlagBits()
    {
        var expected = FuseTestFile.LoadExpected()["cb4e"].Base;
        var actual = FuseCpuState.Load(expected);
        actual.F ^= 0x28;
        var convention = FuseConventions.ForCase("cb4e")!;
        Assert.NotEmpty(FuseCpuState.Diff(expected, in actual, expected.Line2.endtstates));
        Assert.Empty(FuseCpuState.Diff(expected, in actual, expected.Line2.endtstates, convention.IgnoredFlags));
        foreach (var bit in new byte[] { 1, 2, 4, 0x10, 0x40, 0x80 })
        {
            var changed = actual;
            changed.F ^= bit;
            Assert.Contains(FuseCpuState.Diff(expected, in changed, expected.Line2.endtstates,
                convention.IgnoredFlags), m => m.Field == "F");
        }
        actual.A ^= 1;
        actual.AF_ ^= 0x28;
        actual.BC ^= 1;
        var differences = FuseCpuState.Diff(expected, in actual, expected.Line2.endtstates + 1,
            convention.IgnoredFlags);
        Assert.Contains(differences, m => m.Field == "A");
        Assert.Contains(differences, m => m.Field == "F'");
        Assert.Contains(differences, m => m.Field == "BC");
        Assert.Contains(differences, m => m.Kind == FuseMismatchKind.TStates);
    }

    [Fact]
    public void Convention_IsReportedInOutput()
    {
        var input = FuseTestFile.LoadInputs().Single(t => t.Base.Name == "cb4e");
        var expected = FuseTestFile.LoadExpected()["cb4e"];
        var memory = new byte[65536];
        foreach (var block in input.Base.Memories) block.Data.CopyTo(memory, block.Address);
        var state = new FuseTestBusState { Events = new() };
        memory.CopyTo(state.Memory, 0);
        var cpu = new Z80Cpu<FuseTestBus>(new(state)) { Registers = FuseCpuState.Load(input.Base) };
        cpu.Execute(input.Base.Line2.endtstates);
        var report = new FuseReport(expected, in cpu.Registers, state.Cycles, memory,
            a => state.Memory[a], state.Events, convention: FuseConventions.ForCase("cb4e"));
        Assert.False(report.Failed);
        Assert.True(report.IgnoredFlagsDiffer);
        Assert.Contains("convención aplicada: cb4e", report.ConventionSummary("cb4e"));
        var output = report.Format(input, expected, memory, state.Events, true);
        Assert.StartsWith("PASS cb4e", output);
        Assert.Contains("F esperado=18, real=10", output);
        Assert.Contains("F5/F3 esperado=08, real=00", output);
        state.Memory[0x1234] ^= 1;
        state.Events!.RemoveAt(0);
        var wrong = new FuseReport(expected, in cpu.Registers, state.Cycles, memory,
            a => state.Memory[a], state.Events, convention: FuseConventions.ForCase("cb4e"));
        Assert.True(wrong.Failed);
        Assert.Contains(wrong.Mismatches, m => m.Kind == FuseMismatchKind.Memory);
        Assert.Contains(wrong.Mismatches, m => m.Kind == FuseMismatchKind.Event);
        Assert.Contains("convención aplicada: cb4e", wrong.Summary("cb4e"));
    }

    [Fact]
    public void Conventions_ListOnlyBitHlCases()
    {
        Assert.Equal(new[] { "cb46", "cb4e", "cb56", "cb5e", "cb66", "cb6e", "cb76", "cb7e" },
            FuseConventions.Cases.Keys.OrderBy(n => n, StringComparer.Ordinal));
        Assert.All(FuseConventions.Cases.Values, c => Assert.Equal(0x28, c.IgnoredFlags));
        Assert.Null(FuseConventions.ForCase("cb40"));
        Assert.Null(FuseConventions.ForCase("ddcb46"));
        Assert.Null(FuseConventions.ForCase("fdcb46"));
        Assert.Null(FuseConventions.ForCase("cb4e_1"));
    }

    [Fact]
    public void StateDiff_ReturnsEveryMismatchIncludingAlternateFlags()
    {
        var expected = new clsTestBase();
        var actual = new Z80Registers
        {
            AF = 0x0144, BC = 1, DE = 1, HL = 1, AF_ = 0x0280, BC_ = 1, DE_ = 1, HL_ = 1,
            IX = 1, IY = 1, SP = 1, PC = 1, I = 1, R = 1, IFF1 = true, IFF2 = true, IM = 1, Halted = true,
        };
        var differences = FuseCpuState.Diff(expected, in actual, 1);
        Assert.Equal(21, differences.Count);
        Assert.Equal(new[] { "A", "F", "BC", "DE", "HL", "A'", "F'", "BC'", "DE'", "HL'",
            "IX", "IY", "SP", "PC", "I", "R", "IFF1", "IFF2", "IM", "halted", "T-states" },
            differences.Select(d => d.Field));
        Assert.Equal(2, differences.Count(d => d.Kind == FuseMismatchKind.Flags));
        Assert.Equal(4, differences.Count(d => d.Kind == FuseMismatchKind.State));
        Assert.StartsWith("A:", FuseCpuState.Compare(expected, in actual, 1));
    }

    [Fact]
    public void Flags_ShowSetAndDifferingBits()
    {
        var expected = new clsTestBase();
        expected.Line1.af = 0x0044;
        var actual = new Z80Registers { AF = 0x0040 };
        var flags = Assert.Single(FuseCpuState.Diff(expected, in actual, 0));
        Assert.Equal(FuseMismatchKind.Flags, flags.Kind);
        Assert.Equal("44 (Z P/V)", flags.Expected);
        Assert.Equal("40 (Z)", flags.Actual);
        Assert.Equal("Difieren: P/V", flags.Detail);
        Assert.Equal("S Z 5 H 3 P/V N C", FuseCpuState.DecodeFlags(0xFF));
        Assert.Equal("ninguno", FuseCpuState.DecodeFlags(0));
    }

    [Theory]
    [InlineData(0x01, "7 bits bajos")]
    [InlineData(0x80, "solo el bit 7")]
    [InlineData(0x81, "bit 7 y los bits bajos")]
    public void Refresh_ExplainsBothParts(byte value, string detail)
    {
        var actual = new Z80Registers { R = value };
        var mismatch = Assert.Single(FuseCpuState.Diff(new clsTestBase(), in actual, 0));
        Assert.Equal("R", mismatch.Field);
        Assert.Contains(detail, mismatch.Detail);
    }

    [Fact]
    public void Memory_GroupsRangesAndCountsEveryOmittedRange()
    {
        var expected = new byte[65536];
        var actual = new byte[65536];
        expected[0x5D2F] = 0x8D;
        expected[0x5D30] = 0x12;
        expected[0x5D32] = 1;
        expected[0xFFFF] = 2;
        var reads = 0;
        var differences = FuseComparison.DiffMemory(expected, address => { reads++; return actual[address]; }, 1);
        Assert.Equal(65536, reads);
        Assert.Equal(2, differences.Count);
        Assert.Equal("memory 5d2f-5d30", differences[0].Field);
        Assert.Equal("8d 12", differences[0].Expected);
        Assert.Equal("00 00", differences[0].Actual);
        Assert.Equal("y 2 rangos más", differences[1].Detail);
        var all = FuseComparison.DiffMemory(expected, address => actual[address]);
        Assert.Equal(3, all.Count);
        Assert.Equal("memory ffff", all[^1].Field);
        Assert.Contains("5d2f", FuseComparison.CompareMemory(expected, address => actual[address]));
    }

    [Fact]
    public void Memory_DefaultLimitIsSixteenRanges()
    {
        var expected = new byte[40];
        for (var i = 0; i < 40; i += 2) expected[i] = 1;
        var differences = FuseComparison.DiffMemory(expected, _ => 0);
        Assert.Equal(17, differences.Count);
        Assert.Equal("y 4 rangos más", differences[^1].Detail);
        Assert.Throws<ArgumentOutOfRangeException>(() => FuseComparison.DiffMemory(expected, _ => 0, 0));
        Assert.Throws<ArgumentException>(() => FuseComparison.DiffMemory(new byte[65537], _ => 0));
        Assert.Empty(FuseComparison.DiffMemory([], _ => 0));
    }

    private static (clsTestIn Input, clsTestExpected Expected, byte[] Memory, FuseTestBusState State) WrongRead()
    {
        var input = FuseTestFile.LoadInputs().Single(t => t.Base.Name == "20_2");
        var expected = FuseTestFile.LoadExpected()["20_2"];
        var memory = new byte[65536];
        foreach (var block in input.Base.Memories)
            block.Data.CopyTo(memory, block.Address);
        var state = new FuseTestBusState { Events = new() };
        memory.CopyTo(state.Memory, 0);
        var bus = new FuseTestBus(state);
        bus.FetchOpcode(input.Base.Line1.pc);
        bus.Read((ushort)(input.Base.Line1.pc + 1));
        return (input, expected, memory, state);
    }

    [Fact]
    public void Events_IdentifyExtraReadUsingRealFixture()
    {
        var (_, expected, _, state) = WrongRead();
        var difference = Assert.Single(FuseComparison.DiffEvents(expected, state.Events!));
        Assert.Equal("bus event 3", difference.Field);
        Assert.Equal("<missing>", difference.Expected);
        Assert.Contains("MR de más tras MC", difference.Detail);
        Assert.Contains("ReadDiscarded", difference.Detail);
        Assert.Contains("expected 3, actual 4", difference.Detail);
        Assert.Contains(">    3 |", difference.Detail);
        Assert.Contains("MC 0001", difference.Detail);
    }

    [Theory]
    [InlineData("time", "distinto tiempo")]
    [InlineData("address", "distinta dirección")]
    [InlineData("data", "distinto dato")]
    [InlineData("missing", "Falta un evento")]
    [InlineData("extra", "Evento de más")]
    public void Events_ExplainTimingAddressDataAndSequenceLength(string change, string hint)
    {
        var expected = FuseTestFile.LoadExpected()["00"];
        var events = expected.Events.Select(e => e.ToBusEvent()).ToList();
        switch (change)
        {
            case "time": events[1] = events[1] with { Time = 5 }; break;
            case "address": events[1] = events[1] with { Address = 1 }; break;
            case "data": events[1] = events[1] with { Data = 1 }; break;
            case "missing": events.RemoveAt(1); break;
            case "extra": events.Add(new(4, Z80BusEventType.MC, 0, null)); break;
        }
        Assert.Contains(hint, Assert.Single(FuseComparison.DiffEvents(expected, events)).Detail);
    }

    [Fact]
    public void Events_WindowHasThreeRowsBeforeAndAfterFirstMismatch()
    {
        var expected = new clsTestExpected
        {
            Events = Enumerable.Range(0, 10).Select(i => new clsTestEvent {
                time = (ushort)i, type = "MC", address = 0 }).ToArray(),
        };
        var events = expected.Events.Select(e => e.ToBusEvent()).ToArray();
        events[5] = events[5] with { Address = 1 };
        var detail = Assert.Single(FuseComparison.DiffEvents(expected, events)).Detail!;
        Assert.Contains("     2 |", detail);
        Assert.Contains(">    5 |", detail);
        Assert.Contains("     8 |", detail);
        Assert.DoesNotContain("     1 |", detail);
        Assert.DoesNotContain("     9 |", detail);
        Assert.Throws<ArgumentOutOfRangeException>(() => FuseComparison.DiffEvents(expected, events, -1));
    }

    [Fact]
    public void Report_AlwaysComparesEventsAlongsideRegistersFlagsMemoryAndCycles()
    {
        var (input, expected, memory, state) = WrongRead();
        var actual = FuseCpuState.Load(expected.Base);
        actual.BC++;
        actual.F ^= 4;
        state.Memory[0x9000] = 1;
        var report = new FuseReport(expected, in actual, state.Cycles + 1, memory, a => state.Memory[a], state.Events);
        Assert.True(report.Failed);
        Assert.Equal(new[] { FuseMismatchKind.Register, FuseMismatchKind.Flags, FuseMismatchKind.TStates,
            FuseMismatchKind.Memory, FuseMismatchKind.Event }.Order(), report.Mismatches.Select(d => d.Kind).Order());
        var text = report.Format(input, expected, memory, state.Events, true);
        foreach (var section in new[] { "Registros:", "Flags:", "Memoria:", "Eventos:", "T-states:", "Estado inicial", "Programa en PC", "Eventos completos" })
            Assert.Contains(section, text);
        Assert.Contains("ReadDiscarded", text);
        Assert.DoesNotContain(Environment.NewLine, report.Summary(input.Base.Name));
    }

    [Fact]
    public void Report_NoEventsDisablesOnlyEventComparison()
    {
        var (input, expected, memory, state) = WrongRead();
        var actual = FuseCpuState.Load(expected.Base);
        var report = new FuseReport(expected, in actual, state.Cycles, memory, a => state.Memory[a], null);
        Assert.False(report.Failed);
        Assert.Empty(report.Mismatches);
        Assert.Contains("no comparados (--no-events)", report.Format(input, expected, memory, null, true));
    }

    [Fact]
    public void Report_ProgramBytesWrapAtEndOfMemory()
    {
        var input = new clsTestIn();
        input.Base.Line1.pc = 0xFFFF;
        var expected = new clsTestExpected();
        var actual = new Z80Registers();
        var memory = new byte[65536];
        memory[0xFFFF] = 0xDD;
        memory[0] = 0xCB;
        memory[1] = 0x10;
        memory[2] = 0x04;
        var report = new FuseReport(expected, in actual, 0, memory, a => memory[a], null);
        Assert.Contains("PC=ffff: dd cb 10 04", report.Format(input, expected, memory, null));
    }

    [Theory]
    [InlineData(new byte[] { 0xD3 }, 0, 1)]
    [InlineData(new byte[] { 0xDD, 0xD3 }, 0, 2)]
    [InlineData(new byte[] { 0xDD, 0xED, 0 }, 0, 3)]
    [InlineData(new byte[] { 0xED, 0x00 }, 0xFFFE, 0)]
    public void LastUnimplementedAddress_TracksFetchAndReset(byte[] program, ushort start, ushort end)
    {
        var state = new TestBusState();
        for (var i = 0; i < program.Length; i++) state.Memory[(ushort)(start + i)] = program[i];
        var cpu = new Z80Cpu<TestBus>(new(state));
        cpu.Registers.PC = start;
        cpu.Step();
        Assert.Equal(1, cpu.UnimplementedOpcodes);
        Assert.Equal(end, cpu.LastUnimplementedAddress);
        state.Memory[end] = 0;
        cpu.Step();
        Assert.Equal(end, cpu.LastUnimplementedAddress);
        cpu.Reset();
        Assert.Equal((ushort)0, cpu.LastUnimplementedAddress);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }
}
