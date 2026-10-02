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

namespace ZXSinclair.Net.Core.Tests.Z80.Instructions;

public class JumpTests
{
    private static (Z80Cpu<TestBus> Cpu, TestBusState State) Create(params byte[] program)
    {
        var state = new TestBusState();
        program.CopyTo(state.Memory, 0x8000);
        var cpu = new Z80Cpu<TestBus>(new(state));
        cpu.Registers = new Z80Registers
        {
            AF = 0xA5FF, BC = 0x1234, DE = 0x5678, HL = 0x9ABC,
            AF_ = 0xDEF1, BC_ = 0x2345, DE_ = 0x6789, HL_ = 0xABCD,
            IX = 0x1357, IY = 0x2468, SP = 0x9000, WZ = 0xBEEF,
            PC = 0x8000, I = 0x42, R = 0x91, IFF1 = true, IFF2 = true, IM = 1,
        };
        state.Memory[0x9000] = 0x34;
        state.Memory[0x9001] = 0x12;
        return (cpu, state);
    }

    private static Z80Registers Expected(Z80Cpu<TestBus> cpu, ushort pc, ushort? wz = null, int m1 = 1)
    {
        var expected = cpu.Registers;
        expected.PC = pc;
        if (wz.HasValue) expected.WZ = wz.Value;
        for (var i = 0; i < m1; i++) expected.IncrementR();
        return expected;
    }

    private static void Verify(Z80Cpu<TestBus> cpu, TestBusState state, Z80Registers expected,
        int cycles, params (string Kind, ushort Address, int Value)[] accesses)
    {
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(cycles, state.Cycles);
        Assert.Equal(accesses, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        foreach (var access in accesses.Where(a => a.Kind == "Write"))
            Assert.Equal((byte)access.Value, state.Memory[access.Address]);
    }

    public static IEnumerable<object[]> Conditions()
    {
        byte[] masks = [0x40, 0x40, 0x01, 0x01, 0x04, 0x04, 0x80, 0x80];
        for (var i = 0; i < masks.Length; i++)
        {
            foreach (var taken in new[] { false, true })
            {
                var set = taken == (i % 2 == 1);
                var flags = set ? (byte)0xFF : (byte)(0xFF & ~masks[i]);
                yield return [i, flags, taken];
            }
        }
    }

    public static IEnumerable<object[]> RelativeConditions() => Conditions().Where(c => (int)c[0] < 4);

    [Fact]
    public void JpAbsolute_SetsPcAndWz()
    {
        var (cpu, state) = Create(0xC3, 0x34, 0x12);
        Verify(cpu, state, Expected(cpu, 0x1234, 0x1234), 10,
            ("M1", 0x8000, 0xC3), ("Read", 0x8001, 0x34), ("Read", 0x8002, 0x12));
    }

    [Theory]
    [MemberData(nameof(Conditions))]
    public void JpConditional_AllConditions(int condition, byte flags, bool taken)
    {
        var opcode = (byte)(0xC2 + condition * 8);
        var (cpu, state) = Create(opcode, 0x34, 0x12);
        cpu.Registers.F = flags;
        var read = taken ? "Read" : "ReadDiscarded";
        Verify(cpu, state, Expected(cpu, taken ? (ushort)0x1234 : (ushort)0x8003, 0x1234), 10,
            ("M1", 0x8000, opcode), (read, 0x8001, 0x34), (read, 0x8002, 0x12));
    }

    [Theory]
    [InlineData(64, 0x8042)]
    [InlineData(-2, 0x8000)]
    [InlineData(-128, 0x7F82)]
    [InlineData(127, 0x8081)]
    public void JrRelative_ForwardAndBackward(int displacement, ushort target)
    {
        var value = unchecked((byte)displacement);
        var (cpu, state) = Create(0x18, value);
        Verify(cpu, state, Expected(cpu, target, target), 12,
            ("M1", 0x8000, 0x18), ("Read", 0x8001, value), ("Internal", 0x8001, 5));
    }

    [Theory]
    [MemberData(nameof(RelativeConditions))]
    public void JrConditional_AllConditions(int condition, byte flags, bool taken)
    {
        var opcode = (byte)(0x20 + condition * 8);
        var (cpu, state) = Create(opcode, 0xFE);
        cpu.Registers.F = flags;
        var events = new List<(string, ushort, int)> {
            ("M1", 0x8000, opcode), (taken ? "Read" : "ReadDiscarded", 0x8001, 0xFE) };
        if (taken) events.Add(("Internal", 0x8001, 5));
        Verify(cpu, state, Expected(cpu, taken ? (ushort)0x8000 : (ushort)0x8002,
            taken ? (ushort)0x8000 : null), taken ? 12 : 7, events.ToArray());
    }

    [Theory]
    [InlineData(0xFFFE, 4, 0x0004)]
    [InlineData(0, -128, 0xFF82)]
    public void JrRelative_WrapsPc(ushort pc, int displacement, ushort target)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = pc;
        state.Memory[pc] = 0x18;
        var operand = (ushort)(pc + 1);
        var value = unchecked((byte)displacement);
        state.Memory[operand] = value;
        Verify(cpu, state, Expected(cpu, target, target), 12,
            ("M1", pc, 0x18), ("Read", operand, value), ("Internal", operand, 5));
    }

    [Theory]
    [InlineData(2, 1, true)]
    [InlineData(1, 0, false)]
    [InlineData(0, 255, true)]
    public void Djnz_DecrementsAndJumps(byte initial, byte final, bool taken)
    {
        var (cpu, state) = Create(0x10, 0xFE);
        cpu.Registers.B = initial;
        var expected = Expected(cpu, taken ? (ushort)0x8000 : (ushort)0x8002, taken ? (ushort)0x8000 : null);
        expected.B = final;
        var events = new List<(string, ushort, int)> {
            ("M1", 0x8000, 0x10), ("Internal", 0x4292, 1), (taken ? "Read" : "ReadDiscarded", 0x8001, 0xFE) };
        if (taken) events.Add(("Internal", 0x8001, 5));
        Verify(cpu, state, expected, taken ? 13 : 8, events.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0xDD)]
    [InlineData(0xFD)]
    public void JpHl_AndIndex(byte prefix)
    {
        var (cpu, state) = prefix == 0 ? Create(0xE9) : Create(prefix, 0xE9);
        var target = prefix == 0 ? cpu.Registers.HL : prefix == 0xDD ? cpu.Registers.IX : cpu.Registers.IY;
        Verify(cpu, state, Expected(cpu, target, m1: prefix == 0 ? 1 : 2), prefix == 0 ? 4 : 8,
            prefix == 0 ? [("M1", 0x8000, 0xE9)] : [("M1", 0x8000, prefix), ("M1", 0x8001, 0xE9)]);
    }

    [Fact]
    public void Call_PushesReturnAddress()
    {
        var (cpu, state) = Create(0xCD, 0x34, 0x12);
        var expected = Expected(cpu, 0x1234, 0x1234);
        expected.SP = 0x8FFE;
        Verify(cpu, state, expected, 17, ("M1", 0x8000, 0xCD), ("Read", 0x8001, 0x34),
            ("Read", 0x8002, 0x12), ("Internal", 0x8002, 1),
            ("Write", 0x8FFF, 0x80), ("Write", 0x8FFE, 3));
    }

    [Theory]
    [MemberData(nameof(Conditions))]
    public void CallConditional_AllConditions(int condition, byte flags, bool taken)
    {
        var opcode = (byte)(0xC4 + condition * 8);
        var (cpu, state) = Create(opcode, 0x34, 0x12);
        cpu.Registers.F = flags;
        var expected = Expected(cpu, taken ? (ushort)0x1234 : (ushort)0x8003, 0x1234);
        var read = taken ? "Read" : "ReadDiscarded";
        var events = new List<(string, ushort, int)> {
            ("M1", 0x8000, opcode), (read, 0x8001, 0x34), (read, 0x8002, 0x12) };
        if (taken)
        {
            expected.SP = 0x8FFE;
            events.AddRange([("Internal", 0x8002, 1), ("Write", 0x8FFF, 0x80), ("Write", 0x8FFE, 3)]);
        }
        Verify(cpu, state, expected, taken ? 17 : 10, events.ToArray());
    }

    [Fact]
    public void Call_WrapsStackPointer()
    {
        var (cpu, state) = Create(0xCD, 0x34, 0x12);
        cpu.Registers.SP = 1;
        var expected = Expected(cpu, 0x1234, 0x1234);
        expected.SP = 0xFFFF;
        Verify(cpu, state, expected, 17, ("M1", 0x8000, 0xCD), ("Read", 0x8001, 0x34),
            ("Read", 0x8002, 0x12), ("Internal", 0x8002, 1), ("Write", 0, 0x80), ("Write", 0xFFFF, 3));
    }

    [Fact]
    public void Ret_PopsPc()
    {
        var (cpu, state) = Create(0xC9);
        var expected = Expected(cpu, 0x1234, 0x1234);
        expected.SP = 0x9002;
        Verify(cpu, state, expected, 10,
            ("M1", 0x8000, 0xC9), ("Read", 0x9000, 0x34), ("Read", 0x9001, 0x12));
    }

    [Theory]
    [MemberData(nameof(Conditions))]
    public void RetConditional_AllConditions(int condition, byte flags, bool taken)
    {
        var opcode = (byte)(0xC0 + condition * 8);
        var (cpu, state) = Create(opcode);
        cpu.Registers.F = flags;
        var expected = Expected(cpu, taken ? (ushort)0x1234 : (ushort)0x8001, taken ? (ushort)0x1234 : null);
        var events = new List<(string, ushort, int)> { ("M1", 0x8000, opcode), ("Internal", 0x4292, 1) };
        if (taken)
        {
            expected.SP = 0x9002;
            events.AddRange([("Read", 0x9000, 0x34), ("Read", 0x9001, 0x12)]);
        }
        Verify(cpu, state, expected, taken ? 11 : 5, events.ToArray());
    }

    [Fact]
    public void CallRet_RoundTrip()
    {
        var (cpu, state) = Create(0xCD, 0x34, 0x12);
        state.Memory[0x1234] = 0xC9;
        var expected = Expected(cpu, 0x8003, 0x8003, 2);
        cpu.Step();
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(27, state.Cycles);
        Assert.Equal(new (string, ushort, int)[] {
            ("M1", 0x8000, 0xCD), ("Read", 0x8001, 0x34), ("Read", 0x8002, 0x12),
            ("Internal", 0x8002, 1), ("Write", 0x8FFF, 0x80), ("Write", 0x8FFE, 3),
            ("M1", 0x1234, 0xC9), ("Read", 0x8FFE, 3), ("Read", 0x8FFF, 0x80) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    public static IEnumerable<object[]> InterruptReturns()
    {
        foreach (var opcode in new byte[] { 0x45, 0x4D, 0x55, 0x5D, 0x65, 0x6D, 0x75, 0x7D })
            foreach (var iff2 in new[] { false, true })
                yield return [opcode, iff2];
    }

    [Theory]
    [MemberData(nameof(InterruptReturns))]
    public void Retn_CopiesIff2ToIff1(byte opcode, bool iff2)
    {
        var (cpu, state) = Create(0xED, opcode);
        cpu.Registers.IFF1 = false;
        cpu.Registers.IFF2 = iff2;
        var expected = Expected(cpu, 0x1234, 0x1234, 2);
        expected.SP = 0x9002;
        expected.IFF1 = iff2;
        Verify(cpu, state, expected, 14, ("M1", 0x8000, 0xED), ("M1", 0x8001, opcode),
            ("Read", 0x9000, 0x34), ("Read", 0x9001, 0x12));
    }

    [Fact]
    public void Retn_AfterNmiRestoresIff1()
    {
        var (cpu, state) = Create();
        state.Memory[0x66] = 0xED;
        state.Memory[0x67] = 0x45;
        var expected = Expected(cpu, 0x8000, 0x8000, 3);
        cpu.RequestNmi();
        cpu.Step();
        Assert.False(cpu.Registers.IFF1);
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(25, state.Cycles);
        Assert.Equal(new (string, ushort, int)[] {
            ("M1", 0x8000, 0), ("Internal", 0x4292, 1), ("Write", 0x8FFF, 0x80), ("Write", 0x8FFE, 0),
            ("M1", 0x66, 0xED), ("M1", 0x67, 0x45), ("Read", 0x8FFE, 0), ("Read", 0x8FFF, 0x80) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void Retn_IntAcceptedOnNextStep()
    {
        var (cpu, state) = Create(0xED, 0x45);
        cpu.Registers.IFF1 = false;
        state.IntActive = true;
        var expected = Expected(cpu, 0x38, 0x38, 3);
        expected.IFF1 = expected.IFF2 = false;
        cpu.Step();
        Assert.True(cpu.Registers.IFF1);
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(27, state.Cycles);
        Assert.Equal(new (string, ushort, int)[] {
            ("M1", 0x8000, 0xED), ("M1", 0x8001, 0x45), ("Read", 0x9000, 0x34), ("Read", 0x9001, 0x12),
            ("Ack", 0, 0xFF), ("Write", 0x9001, 0x12), ("Write", 0x9000, 0x34) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xC7, 0)]
    [InlineData(0xCF, 8)]
    [InlineData(0xD7, 16)]
    [InlineData(0xDF, 24)]
    [InlineData(0xE7, 32)]
    [InlineData(0xEF, 40)]
    [InlineData(0xF7, 48)]
    [InlineData(0xFF, 56)]
    public void Rst_AllVectors(byte opcode, ushort target)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0x1234;
        state.Memory[0x1234] = opcode;
        var expected = Expected(cpu, target, target);
        expected.SP = 0x8FFE;
        Verify(cpu, state, expected, 11, ("M1", 0x1234, opcode), ("Internal", 0x4292, 1),
            ("Write", 0x8FFF, 0x12), ("Write", 0x8FFE, 0x35));
    }

    public static IEnumerable<object[]> UnaffectedJumps()
    {
        var opcodes = new List<byte> { 0x10, 0x18, 0x20, 0x28, 0x30, 0x38, 0xC3, 0xCD, 0xC9 };
        for (var i = 0; i < 8; i++)
            opcodes.AddRange([(byte)(0xC0 + i * 8), (byte)(0xC2 + i * 8),
                (byte)(0xC4 + i * 8), (byte)(0xC7 + i * 8)]);
        foreach (var prefix in new byte[] { 0xDD, 0xFD })
            foreach (var opcode in opcodes)
                foreach (var flags in new byte[] { 0, 0xFF })
                    yield return [prefix, opcode, flags];
    }

    [Theory]
    [MemberData(nameof(UnaffectedJumps))]
    public void IndexPrefix_UnaffectedJumpsRunAsBase(byte prefix, byte opcode, byte flags)
    {
        // Place the base opcode at the same address in both runs, so PC/stack/WZ and
        // operand addresses must match; only the extra M1 and refresh address differ.
        var (plain, plainState) = Create(prefix, opcode, 0x34, 0x12);
        plain.Registers.PC++;
        var (indexed, indexedState) = Create(prefix, opcode, 0x34, 0x12);
        plain.Registers.F = indexed.Registers.F = flags;
        plain.Step();
        indexed.Step();
        var expected = plain.Registers;
        expected.IncrementR();
        Assert.Equal(expected, indexed.Registers);
        Assert.Equal(plainState.Cycles + 4, indexedState.Cycles);
        var events = plainState.Accesses.ToArray();
        for (var i = 0; i < events.Length; i++)
            if (events[i].Kind == "Internal" && events[i].Address == 0x4292)
                events[i].Address = 0x4293;
        Assert.Equal(new[] { ("M1", (ushort)0x8000, (int)prefix) }.Concat(events), indexedState.Accesses);
        Assert.Equal(plainState.Memory, indexedState.Memory);
        Assert.Equal(0, indexed.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xC3, true)]
    [InlineData(0xCD, true)]
    [InlineData(0xC2, false)]
    [InlineData(0xC4, false)]
    public void AbsoluteOperand_WrapsPcDuringRead(byte opcode, bool taken)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFE;
        state.Memory[0xFFFE] = opcode;
        state.Memory[0xFFFF] = 0x34;
        state.Memory[0] = 0x12;
        var expected = Expected(cpu, taken ? (ushort)0x1234 : (ushort)1, 0x1234);
        var read = taken ? "Read" : "ReadDiscarded";
        var events = new List<(string, ushort, int)> {
            ("M1", 0xFFFE, opcode), (read, 0xFFFF, 0x34), (read, 0, 0x12) };
        if (opcode == 0xCD)
        {
            expected.SP = 0x8FFE;
            events.AddRange([("Internal", 0, 1), ("Write", 0x8FFF, 0), ("Write", 0x8FFE, 1)]);
        }
        Verify(cpu, state, expected, opcode == 0xCD ? 17 : 10, events.ToArray());
    }

    [Fact]
    public void Ret_WrapsStackPointer()
    {
        var (cpu, state) = Create(0xC9);
        cpu.Registers.SP = 0xFFFF;
        state.Memory[0xFFFF] = 0x34;
        state.Memory[0] = 0x12;
        var expected = Expected(cpu, 0x1234, 0x1234);
        expected.SP = 1;
        Verify(cpu, state, expected, 10,
            ("M1", 0x8000, 0xC9), ("Read", 0xFFFF, 0x34), ("Read", 0, 0x12));
    }

    [Theory]
    [InlineData(0x7F, 0, 0x00)]
    [InlineData(0xFF, 0, 0x80)]
    [InlineData(0x7E, 0xDD, 0x00)]
    [InlineData(0xFE, 0xFD, 0x80)]
    public void Jump_RefreshWrapsKeepingBit7(byte initial, byte prefix, byte final)
    {
        var (cpu, state) = prefix == 0 ? Create(0xFF) : Create(prefix, 0xFF);
        cpu.Registers.R = initial;
        var expected = Expected(cpu, 0x38, 0x38, prefix == 0 ? 1 : 2);
        expected.R = final;
        expected.SP = 0x8FFE;
        var events = new List<(string, ushort, int)>();
        if (prefix != 0) events.Add(("M1", 0x8000, prefix));
        events.AddRange([("M1", (ushort)(prefix == 0 ? 0x8000 : 0x8001), 0xFF),
            ("Internal", (ushort)(0x4200 | final), 1), ("Write", 0x8FFF, 0x80),
            ("Write", 0x8FFE, prefix == 0 ? 1 : 2)]);
        Verify(cpu, state, expected, prefix == 0 ? 11 : 15, events.ToArray());
    }

    [Theory]
    [InlineData(0xC3)]
    [InlineData(0xE9)]
    [InlineData(0x18)]
    [InlineData(0x10)]
    [InlineData(0xCD)]
    [InlineData(0xC9)]
    [InlineData(0xC0)]
    [InlineData(0xFF)]
    [InlineData(0xED)]
    public void Jumps_DoNotTouchFlagsOrCounter(byte opcode)
    {
        var (cpu, state) = Create(opcode, 0x45, 0x12);
        var flags = cpu.Registers.F;
        state.IntAfterCycle = 1;
        cpu.Step();
        Assert.Equal(flags, cpu.Registers.F);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        // Every pattern permits an interrupt immediately after the instruction.
        var expected = Expected(cpu, 0x38, 0x38);
        expected.SP -= 2;
        expected.IFF1 = expected.IFF2 = false;
        var pc = cpu.Registers.PC;
        var sp = cpu.Registers.SP;
        var cycles = state.Cycles;
        state.Accesses.Clear();
        state.Cycles = 0;
        state.IntActive = true;
        Verify(cpu, state, expected, 13, ("Ack", 0, 0xFF),
            ("Write", (ushort)(sp - 1), pc >> 8), ("Write", (ushort)(sp - 2), (byte)pc));
        Assert.True(cycles > 0);
    }
}
