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

public class Load16Tests
{
    private static (Z80Cpu<TestBus> Cpu, TestBusState State) Create(params byte[] program)
    {
        var state = new TestBusState();
        program.CopyTo(state.Memory, 0);
        var cpu = new Z80Cpu<TestBus>(new(state));
        cpu.Registers = new Z80Registers
        {
            AF = 0xA5FF, BC = 0x1234, DE = 0x5678, HL = 0x9ABC,
            AF_ = 0xDEF1, BC_ = 0x2345, DE_ = 0x6789, HL_ = 0xABCD,
            IX = 0x1357, IY = 0x2468, SP = 0x9000, WZ = 0xBEEF,
            I = 0x42, R = 0x91, IFF1 = true, IFF2 = true, IM = 1, Q = 0,
        };
        return (cpu, state);
    }

    // Test register selectors are independent of the generator's operand emitter.
    private static ref ushort Pair(ref Z80Registers r, int pair)
    {
        switch (pair)
        {
            case 0: return ref r.BC;
            case 1: return ref r.DE;
            case 2: return ref r.HL;
            case 3: return ref r.SP;
            case 4: return ref r.AF;
            case 5: return ref r.IX;
            case 6: return ref r.IY;
            default: throw new ArgumentOutOfRangeException(nameof(pair));
        }
    }

    private static Z80Registers AfterFetch(Z80Cpu<TestBus> cpu, int bytes, int m1 = 1)
    {
        var expected = cpu.Registers;
        expected.PC = (ushort)(expected.PC + bytes);
        for (var i = 0; i < m1; i++)
            expected.IncrementR();
        expected.EiPending = false;
        return expected;
    }

    private static void Check(Z80Cpu<TestBus> cpu, TestBusState state, Z80Registers expected,
        int cycles, params (string Kind, ushort Address, int Value)[] events)
    {
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        Assert.Equal(cycles, state.Cycles);
        Assert.Equal(events, state.Accesses);
    }

    [Theory]
    [InlineData(0x01, 0)]
    [InlineData(0x11, 1)]
    [InlineData(0x21, 2)]
    [InlineData(0x31, 3)]
    public void LdPairImm_AllPairs(byte opcode, int pair)
    {
        var (cpu, state) = Create(opcode, 0x34, 0x12);
        cpu.Registers.BC = 0x4321;
        var expected = AfterFetch(cpu, 3);
        Pair(ref expected, pair) = 0x1234;
        Check(cpu, state, expected, 10, ("M1", 0, opcode), ("Read", 1, 0x34), ("Read", 2, 0x12));
    }

    [Theory]
    [InlineData(0xDD, 5)]
    [InlineData(0xFD, 6)]
    public void LdIndexImm_SetsIndex(byte prefix, int pair)
    {
        var (cpu, state) = Create(prefix, 0x21, 0x34, 0x12);
        var expected = AfterFetch(cpu, 4, 2);
        Pair(ref expected, pair) = 0x1234;
        Check(cpu, state, expected, 14, ("M1", 0, prefix), ("M1", 1, 0x21),
            ("Read", 2, 0x34), ("Read", 3, 0x12));
    }

    private static void Absolute(byte prefix, byte opcode, int pair, bool store, ushort address = 0x1234)
    {
        byte[] program = prefix == 0 ? [opcode, (byte)address, (byte)(address >> 8)]
            : [prefix, opcode, (byte)address, (byte)(address >> 8)];
        var (cpu, state) = Create(program);
        // Put code elsewhere so a wraparound data write cannot alter an operand.
        Array.Copy(program, 0, state.Memory, 0x8000, program.Length);
        cpu.Registers.PC = 0x8000;
        state.Memory[address] = 0xEF;
        var highAddress = (ushort)(address + 1);
        state.Memory[highAddress] = 0xCD;
        var expected = AfterFetch(cpu, program.Length, prefix == 0 ? 1 : 2);
        expected.WZ = highAddress;
        var value = store ? Pair(ref cpu.Registers, pair) : (ushort)0xCDEF;
        if (!store) Pair(ref expected, pair) = value;
        var events = new List<(string, ushort, int)> { ("M1", 0x8000, program[0]) };
        if (prefix != 0) events.Add(("M1", 0x8001, opcode));
        var operandPc = (ushort)(0x8000 + program.Length - 2);
        events.Add(("Read", operandPc, (byte)address));
        events.Add(("Read", (ushort)(operandPc + 1), address >> 8));
        events.Add((store ? "Write" : "Read", address, (byte)value));
        events.Add((store ? "Write" : "Read", highAddress, value >> 8));
        Check(cpu, state, expected, prefix == 0 ? 16 : 20, events.ToArray());
        Assert.Equal((byte)value, state.Memory[address]);
        Assert.Equal((byte)(value >> 8), state.Memory[highAddress]);
    }

    [Fact]
    public void LdHlAbsolute_ReadsLowThenHigh() => Absolute(0, 0x2A, 2, false);

    [Theory]
    [InlineData(0x4B, 0)]
    [InlineData(0x5B, 1)]
    [InlineData(0x6B, 2)]
    [InlineData(0x7B, 3)]
    public void LdPairAbsolute_EdForms(byte opcode, int pair) => Absolute(0xED, opcode, pair, false);

    [Theory]
    [InlineData(0x6B, false)]
    [InlineData(0x63, true)]
    public void LdHlAbsolute_UndocumentedEdDuplicates(byte opcode, bool store) => Absolute(0xED, opcode, 2, store);

    [Fact]
    public void LdAbsoluteHl_WritesLowThenHigh() => Absolute(0, 0x22, 2, true);

    [Theory]
    [InlineData(0x43, 0)]
    [InlineData(0x53, 1)]
    [InlineData(0x63, 2)]
    [InlineData(0x73, 3)]
    public void LdAbsolutePair_EdForms(byte opcode, int pair) => Absolute(0xED, opcode, pair, true);

    [Theory]
    [InlineData(0xDD, 0x22, 5, true)]
    [InlineData(0xFD, 0x22, 6, true)]
    [InlineData(0xDD, 0x2A, 5, false)]
    [InlineData(0xFD, 0x2A, 6, false)]
    public void LdAbsoluteIndex(byte prefix, byte opcode, int pair, bool store) => Absolute(prefix, opcode, pair, store);

    [Theory]
    [InlineData(0x2A, false)]
    [InlineData(0x22, true)]
    public void LdWordAbsolute_WrapsAtFFFF(byte opcode, bool store) => Absolute(0, opcode, 2, store, 0xFFFF);

    [Fact]
    public void LdWordAbsolute_OperandWrapsPc()
    {
        var (cpu, state) = Create(0x12);
        cpu.Registers.PC = 0xFFFE;
        state.Memory[0xFFFE] = 0x2A;
        state.Memory[0xFFFF] = 0x34;
        state.Memory[0x1234] = 0xEF;
        state.Memory[0x1235] = 0xCD;
        var expected = AfterFetch(cpu, 3);
        expected.HL = 0xCDEF;
        expected.WZ = 0x1235;
        Check(cpu, state, expected, 16, ("M1", 0xFFFE, 0x2A), ("Read", 0xFFFF, 0x34),
            ("Read", 0, 0x12), ("Read", 0x1234, 0xEF), ("Read", 0x1235, 0xCD));
    }

    [Fact]
    public void LdSpHl_InternalCyclesOnIr()
    {
        var (cpu, state) = Create(0xF9);
        var expected = AfterFetch(cpu, 1);
        expected.SP = expected.HL;
        Check(cpu, state, expected, 6, ("M1", 0, 0xF9), ("Internal", 0x4292, 2));
    }

    [Theory]
    [InlineData(0xDD, 5)]
    [InlineData(0xFD, 6)]
    public void LdSpIndex(byte prefix, int pair)
    {
        var (cpu, state) = Create(prefix, 0xF9);
        var expected = AfterFetch(cpu, 2, 2);
        expected.SP = Pair(ref expected, pair);
        Check(cpu, state, expected, 10, ("M1", 0, prefix), ("M1", 1, 0xF9), ("Internal", 0x4293, 2));
    }

    private static void Stack(byte prefix, byte opcode, int pair, bool push, ushort sp = 0x9000, ushort popped = 0xCDEF)
    {
        var (cpu, state) = Create(prefix == 0 ? [opcode] : [prefix, opcode]);
        // Keep program out of wraparound stack reads.
        state.Memory[0x8000] = prefix == 0 ? opcode : prefix;
        state.Memory[0x8001] = opcode;
        cpu.Registers.PC = 0x8000;
        cpu.Registers.SP = sp;
        if (!push)
        {
            state.Memory[sp] = (byte)popped;
            state.Memory[(ushort)(sp + 1)] = (byte)(popped >> 8);
        }
        var expected = AfterFetch(cpu, prefix == 0 ? 1 : 2, prefix == 0 ? 1 : 2);
        expected.SP = (ushort)(sp + (push ? -2 : 2));
        var value = push ? Pair(ref cpu.Registers, pair) : popped;
        if (!push) Pair(ref expected, pair) = value;
        var events = new List<(string, ushort, int)> { ("M1", 0x8000, prefix == 0 ? opcode : prefix) };
        if (prefix != 0) events.Add(("M1", 0x8001, opcode));
        if (push)
        {
            events.Add(("Internal", expected.IR, 1));
            events.Add(("Write", (ushort)(sp - 1), value >> 8));
            events.Add(("Write", (ushort)(sp - 2), (byte)value));
        }
        else
        {
            events.Add(("Read", sp, (byte)value));
            events.Add(("Read", (ushort)(sp + 1), value >> 8));
        }
        Check(cpu, state, expected, (push ? 11 : 10) + (prefix == 0 ? 0 : 4), events.ToArray());
        if (push)
        {
            Assert.Equal((byte)(value >> 8), state.Memory[(ushort)(sp - 1)]);
            Assert.Equal((byte)value, state.Memory[(ushort)(sp - 2)]);
        }
    }

    [Theory]
    [InlineData(0xC5, 0)]
    [InlineData(0xD5, 1)]
    [InlineData(0xE5, 2)]
    [InlineData(0xF5, 4)]
    public void Push_AllPairs(byte opcode, int pair) => Stack(0, opcode, pair, true);

    [Theory]
    [InlineData(0xDD, 5)]
    [InlineData(0xFD, 6)]
    public void PushIndex(byte prefix, int pair) => Stack(prefix, 0xE5, pair, true);

    [Fact]
    public void Push_WrapsStackPointer() => Stack(0, 0xC5, 0, true, 0);

    [Theory]
    [InlineData(0xC1, 0)]
    [InlineData(0xD1, 1)]
    [InlineData(0xE1, 2)]
    [InlineData(0xF1, 4)]
    public void Pop_AllPairs(byte opcode, int pair) => Stack(0, opcode, pair, false);

    [Theory]
    [InlineData(0xFF)]
    [InlineData(0x00)]
    public void PopAf_LoadsAllFlagBits(byte flags) => Stack(0, 0xF1, 4, false, popped: (ushort)(0xA500 | flags));

    [Theory]
    [InlineData(0xDD, 5)]
    [InlineData(0xFD, 6)]
    public void PopIndex(byte prefix, int pair) => Stack(prefix, 0xE1, pair, false);

    [Fact]
    public void Pop_WrapsStackPointer() => Stack(0, 0xC1, 0, false, 0xFFFF);

    [Fact]
    public void PushPop_RoundTrip()
    {
        var (cpu, state) = Create(0xC5, 0xD1);
        var expected = AfterFetch(cpu, 2, 2);
        expected.DE = expected.BC;
        cpu.Step();
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        Assert.Equal(21, state.Cycles);
        Assert.Equal(new[] { ("M1", (ushort)0, 0xC5), ("Internal", (ushort)0x4292, 1),
            ("Write", (ushort)0x8FFF, 0x12), ("Write", (ushort)0x8FFE, 0x34),
            ("M1", (ushort)1, 0xD1), ("Read", (ushort)0x8FFE, 0x34), ("Read", (ushort)0x8FFF, 0x12) }, state.Accesses);
    }

    public static IEnumerable<object[]> UnaffectedPrefixes()
    {
        foreach (var prefix in new byte[] { 0xDD, 0xFD })
            foreach (var opcode in new byte[] { 0x01, 0x11, 0x31, 0xC1, 0xC5, 0xD1, 0xD5, 0xF1, 0xF5 })
                yield return new object[] { prefix, opcode };
    }

    [Theory]
    [MemberData(nameof(UnaffectedPrefixes))]
    public void IndexPrefix_UnaffectedRunAsBase(byte prefix, byte opcode)
    {
        var pair = (opcode >> 4) & 3;
        if (opcode >= 0xC0)
        {
            if (pair == 3) pair = 4;
            Stack(prefix, opcode, pair, (opcode & 4) != 0);
            return;
        }
        var (cpu, state) = Create(prefix, opcode, 0x34, 0x12);
        cpu.Registers.BC = 0x4321;
        var expected = AfterFetch(cpu, 4, 2);
        Pair(ref expected, pair) = 0x1234;
        Check(cpu, state, expected, 14, ("M1", 0, prefix), ("M1", 1, opcode),
            ("Read", 2, 0x34), ("Read", 3, 0x12));
    }

    [Theory]
    [InlineData(0x01)]
    [InlineData(0x2A)]
    [InlineData(0x22)]
    [InlineData(0xF9)]
    [InlineData(0xC5)]
    [InlineData(0xC1)]
    public void Load16_DoesNotTouchFlagsWzOrCounter(byte opcode)
    {
        // The shared checks compare every register, including F, WZ, Q and alternate pairs.
        switch (opcode)
        {
            case 0x01: LdPairImm_AllPairs(opcode, 0); break;
            case 0x2A: Absolute(0, opcode, 2, false); break;
            case 0x22: Absolute(0, opcode, 2, true); break;
            case 0xF9: LdSpHl_InternalCyclesOnIr(); break;
            case 0xC5: Stack(0, opcode, 0, true); break;
            case 0xC1: Stack(0, opcode, 0, false); break;
        }
    }

    [Fact]
    public void Load16_AllowsIntOnNextStep()
    {
        var (cpu, state) = Create(0xF9);
        state.IntAfterCycle = 1;
        var expected = AfterFetch(cpu, 1);
        expected.SP = expected.HL;
        Check(cpu, state, expected, 6, ("M1", 0, 0xF9), ("Internal", 0x4292, 2));
        expected.PC = expected.WZ = 0x38;
        expected.SP = 0x9ABA;
        expected.IncrementR();
        expected.IFF1 = expected.IFF2 = false;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(19, state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        Assert.Equal((byte)1, state.Memory[0x9ABA]);
        Assert.Equal((byte)0, state.Memory[0x9ABB]);
        Assert.Equal(new[] { ("M1", (ushort)0, 0xF9), ("Internal", (ushort)0x4292, 2),
            ("Ack", (ushort)0, 0xFF), ("Write", (ushort)0x9ABB, 0), ("Write", (ushort)0x9ABA, 1) }, state.Accesses);
    }

    [Theory]
    [InlineData(0xDD, 0xFD, 6)]
    [InlineData(0xFD, 0xDD, 5)]
    public void RepeatedPrefixes_LastPrefixSelectsPairAndRefreshWraps(byte first, byte last, int pair)
    {
        var (cpu, state) = Create(first, last, 0x21, 0xEF, 0xCD);
        cpu.Registers.R = 0xFE;
        var expected = AfterFetch(cpu, 5, 3);
        Pair(ref expected, pair) = 0xCDEF;
        Check(cpu, state, expected, 18, ("M1", 0, first), ("M1", 1, last), ("M1", 2, 0x21),
            ("Read", 3, 0xEF), ("Read", 4, 0xCD));
        Assert.Equal((byte)0x81, cpu.Registers.R);
    }
}
