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

public class ControlTests
{
    private static (Z80Cpu<TestBus> Cpu, TestBusState State) Create(params byte[] program)
    {
        var state = new TestBusState();
        program.CopyTo(state.Memory, 0x8000);
        var cpu = new Z80Cpu<TestBus>(new(state));
        cpu.Registers = new Z80Registers
        {
            AF = 0x89FF, BC = 0x1234, DE = 0x5678, HL = 0x9ABC,
            AF_ = 0xDEF1, BC_ = 0x2345, DE_ = 0x6789, HL_ = 0xABCD,
            IX = 0x1357, IY = 0x2468, SP = 0x9000, PC = 0x8000, WZ = 0xBEEF,
            I = 0x42, R = 0xFF, IFF1 = true, IFF2 = true, IM = 1, Q = 0xAA,
        };
        return (cpu, state);
    }

    private static byte ResultFlags(byte value)
    {
        var f = value >= 128 ? 0x80 : 0;
        if (value == 0) f |= 0x40;
        if ((value & 0x20) != 0) f |= 0x20;
        if ((value & 0x08) != 0) f |= 0x08;
        var bits = 0;
        for (var i = 0; i < 8; i++)
            if ((value & (1 << i)) != 0) bits++;
        if (bits % 2 == 0) f |= 4;
        return (byte)f;
    }

    // Independent DAA reference: decimal digit/range checks and half carry from the
    // before/after bit-4 transition, rather than the Core's conditional H formula.
    private static (byte A, byte F) DaaReference(byte a, byte f)
    {
        var correction = 0;
        var carry = (f & 1) != 0;
        if (a / 16 > 9 || (a / 16 == 9 && a % 16 > 9))
        {
            correction += 96;
            carry = true;
        }
        else if (carry) correction += 96;
        if ((f & 16) != 0 || a % 16 > 9) correction += 6;
        var result = (byte)((f & 2) != 0 ? a - correction : a + correction);
        var flags = ResultFlags(result) | (f & 2);
        if (carry) flags |= 1;
        if ((a & 16) != (result & 16)) flags |= 16;
        return (result, (byte)flags);
    }

    private static byte CarryReference(byte a, byte f, byte q, bool complement)
    {
        var flags = f & 0xC4;
        // Check each undocumented bit independently of the combined Core expression.
        for (var bit = 8; bit <= 32; bit *= 4)
            if ((a & bit) != 0 || (q & bit) != (f & bit)) flags |= bit;
        if (!complement) flags |= 1;
        else if ((f & 1) != 0) flags |= 16;
        else flags |= 1;
        return (byte)flags;
    }

    [Fact]
    public void Daa_AllInputs_MatchReference()
    {
        var (cpu, state) = Create(0x27);
        for (var a = 0; a < 256; a++)
        for (var f = 0; f < 256; f++)
        {
            cpu.Registers.PC = 0x8000;
            cpu.Registers.AF = (ushort)((a << 8) | f);
            cpu.Registers.Q = 0xAA;
            state.Accesses.Clear();
            state.Cycles = 0;
            var expected = DaaReference((byte)a, (byte)f);
            cpu.Step();
            if (cpu.Registers.A != expected.A || cpu.Registers.F != expected.F || cpu.Registers.Q != expected.F)
                Assert.Fail($"DAA {a:X2}/{f:X2}: expected {expected}, actual {cpu.Registers.AF:X4}, Q={cpu.Registers.Q:X2}");
        }
    }

    [Theory]
    [InlineData(0x1F, 0, 0x25, 0x30)]
    [InlineData(0x9A, 2, 0x34, 0x23)]
    [InlineData(0x99, 1, 0xF9, 0xAD)]
    [InlineData(0x13, 0x12, 0x0D, 0x1A)]
    public void Daa_KnownCases(byte a, byte f, byte result, byte flags)
    {
        var (cpu, _) = Create(0x27);
        cpu.Registers.AF = (ushort)((a << 8) | f);
        cpu.Step();
        Assert.Equal(result, cpu.Registers.A);
        Assert.Equal(flags, cpu.Registers.F);
        Assert.Equal(flags, cpu.Registers.Q);
    }

    [Theory]
    [InlineData(0, 0x32)] [InlineData(0xC5, 0xF7)]
    public void Cpl_InvertsAndSetsHN(byte f, byte expected)
    {
        var (cpu, _) = Create(0x2F);
        cpu.Registers.F = f;
        cpu.Step();
        Assert.Equal((byte)0x76, cpu.Registers.A);
        Assert.Equal(expected, cpu.Registers.F);
    }

    [Theory]
    [InlineData(0, 0, 0x42)] [InlineData(0x80, 0x80, 0x87)]
    [InlineData(1, 0xFF, 0xBB)] [InlineData(0xFE, 2, 0x13)]
    public void Neg_EdgeValues(byte a, byte result, byte flags)
    {
        var (cpu, _) = Create(0xED, 0x44);
        cpu.Registers.A = a;
        cpu.Step();
        Assert.Equal(result, cpu.Registers.A);
        Assert.Equal(flags, cpu.Registers.F);
        Assert.Equal(flags, cpu.Registers.Q);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Scf_Ccf_UseQ(bool complement)
    {
        var (cpu, state) = Create(complement ? (byte)0x3F : (byte)0x37);
        for (var a = 0; a < 256; a++)
        for (var f = 0; f < 256; f++)
        for (var qIndex = 0; qIndex < 3; qIndex++)
        {
            var q = qIndex == 0 ? (byte)0 : qIndex == 1 ? (byte)f : (byte)0xAA;
            cpu.Registers.PC = 0x8000;
            cpu.Registers.AF = (ushort)((a << 8) | f);
            cpu.Registers.Q = q;
            state.Cycles = 0;
            state.Accesses.Clear();
            var expected = CarryReference((byte)a, (byte)f, q, complement);
            cpu.Step();
            if (cpu.Registers.F != expected || cpu.Registers.A != a || cpu.Registers.Q != expected)
                Assert.Fail($"carry op={complement} A={a:X2} F={f:X2} Q={q:X2}: expected {expected:X2}, actual {cpu.Registers.AF:X4}");
        }
    }

    [Fact]
    public void Scf_AfterAluVsAfterLoad()
    {
        // CP keeps A=0 but writes operand bits 3/5 into F, exposing the Q distinction.
        var (cpu, _) = Create(0xFE, 0x28, 0x37);
        cpu.Registers.A = 0;
        cpu.Step();
        Assert.Equal((byte)0x28, (byte)(cpu.Registers.F & 0x28));
        cpu.Step();
        Assert.Equal((byte)0, (byte)(cpu.Registers.F & 0x28));

        (cpu, _) = Create(0x40, 0x37);
        cpu.Registers.A = 0;
        cpu.Registers.F = 0x28;
        cpu.Step();
        Assert.Equal((byte)0, cpu.Registers.Q);
        cpu.Step();
        Assert.Equal((byte)0x28, (byte)(cpu.Registers.F & 0x28));
    }

    [Theory]
    [InlineData(0xDD, 0x37)]
    [InlineData(0xFD, 0x37)]
    [InlineData(0xDD, 0x3F)]
    [InlineData(0xFD, 0x3F)]
    public void ScfCcf_AfterIndexPrefix_UseQZero(int prefix, int opcode)
    {
        // CP leaves A = 0 and F5/F3 = 0x28 with Q = F; the prefix must reset that history.
        var (cpu, state) = Create(0xFE, 0x28, (byte)prefix, (byte)opcode);
        cpu.Registers.A = 0;
        cpu.Step();
        Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
        var cycles = state.Cycles;
        cpu.Step();
        Assert.Equal(8, state.Cycles - cycles);
        Assert.Equal((byte)0x28, (byte)(cpu.Registers.F & 0x28));
        Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
    }

    [Theory]
    [InlineData(0x37)]
    [InlineData(0x3F)]
    public void ScfCcf_WithoutPrefix_KeepPreviousQ(int opcode)
    {
        var (cpu, _) = Create(0xFE, 0x28, (byte)opcode);
        cpu.Registers.A = 0;
        cpu.Step();
        cpu.Step();
        Assert.Equal((byte)0, (byte)(cpu.Registers.F & 0x28));
    }

    [Fact]
    public void ScfCcf_AfterPrefixChain_UseQZero()
    {
        var (cpu, state) = Create(0xFE, 0x28, 0xDD, 0xFD, 0x37);
        cpu.Registers.A = 0;
        cpu.Step();
        var cycles = state.Cycles;
        cpu.Step();
        Assert.Equal(12, state.Cycles - cycles);
        Assert.Equal((byte)0x28, (byte)(cpu.Registers.F & 0x28));
    }

    [Theory]
    [InlineData(new byte[] { 0xDD, 0x84 }, true)]
    [InlineData(new byte[] { 0xFD, 0x21, 0x34, 0x12 }, false)]
    [InlineData(new byte[] { 0xDD, 0xCB, 0x05, 0x06 }, true)]
    public void IndexPrefix_OtherInstructionsSetQAsBefore(byte[] program, bool writesFlags)
    {
        var (cpu, _) = Create(program);
        cpu.Step();
        Assert.Equal(writesFlags ? cpu.Registers.F : (byte)0, cpu.Registers.Q);
    }

    public static IEnumerable<object[]> ControlCases()
    {
        foreach (var pc in new[] { 0x8000, 0xFFFF })
        foreach (var prefix in new[] { 0, 0xDD, 0xFD })
        {
            foreach (var opcode in new[] { 0, 0x27, 0x2F, 0x37, 0x3F, 0x76, 0xF3, 0xFB })
                yield return [prefix == 0 ? new byte[] { (byte)opcode } : new byte[] { (byte)prefix, (byte)opcode }, pc];
            foreach (var opcode in new[] { 0x44, 0x4C, 0x54, 0x5C, 0x64, 0x6C, 0x74, 0x7C,
                0x46, 0x4E, 0x56, 0x5E, 0x66, 0x6E, 0x76, 0x7E })
                yield return [prefix == 0 ? new byte[] { 0xED, (byte)opcode } : new byte[] { (byte)prefix, 0xED, (byte)opcode }, pc];
        }
    }

    [Theory]
    [MemberData(nameof(ControlCases))]
    public void Control_AllAliasesPrefixesAndPcWrap_PreserveRegistersAndCycles(byte[] program, ushort pc)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = pc;
        cpu.Registers.A = 0xFE;
        var expected = cpu.Registers;
        var events = new List<(string Kind, ushort Address, int Value)>();
        for (var i = 0; i < program.Length; i++)
        {
            var address = (ushort)(pc + i);
            state.Memory[address] = program[i];
            events.Add(("M1", address, program[i]));
        }
        expected.PC = (ushort)(pc + program.Length);
        expected.R = (byte)(0x80 | ((0x7F + program.Length) & 0x7F));
        expected.Q = 0;
        var opcode = program[^1];
        if (program.Contains((byte)0xED))
        {
            if ((opcode & 7) == 4)
            {
                expected.A = 2;
                expected.F = expected.Q = 0x13;
            }
            else expected.IM = opcode switch
            {
                0x46 or 0x4E or 0x66 or 0x6E => 0,
                0x56 or 0x76 => 1, _ => 2,
            };
        }
        else switch (opcode)
        {
            case 0x27:
                var daa = DaaReference(expected.A, expected.F);
                expected.A = daa.A;
                expected.F = expected.Q = daa.F;
                break;
            case 0x2F:
                expected.A = (byte)~expected.A;
                expected.F = expected.Q = (byte)((expected.F & 0xC5) | 0x12 | (expected.A & 0x28));
                break;
            case 0x37: case 0x3F:
                expected.F = expected.Q = CarryReference(expected.A, expected.F, cpu.Registers.Q, opcode == 0x3F);
                break;
            case 0x76: expected.Halted = true; expected.PC--; break;
            case 0xF3: expected.IFF1 = expected.IFF2 = false; break;
            case 0xFB: expected.IFF1 = expected.IFF2 = true; expected.EiPending = true; break;
        }
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(events, state.Accesses);
        Assert.Equal(4 * program.Length, state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    public static IEnumerable<object[]> QCases()
    {
        byte[][] nonWriters = [
            [0], [0x41], [6, 1], [0x46], [0x70], [0x36, 1], [0x3A, 0, 0x90],
            [0x32, 0, 0x90], [0xDD, 0x46, 1], [0xFD, 0x70, 1], [0xDD, 0x36, 1, 1],
            [0xED, 0x47], [1, 0, 0x90], [0x2A, 0, 0x90], [0x22, 0, 0x90], [0xF9],
            [0xC5], [0xC1], [0xF1], [0xC3, 0, 0x80], [0xE9], [0x18, 0], [0x10, 0],
            [0xCD, 0, 0x80], [0xC9], [0xC0], [0xED, 0x45], [0xC7],
            [0x76], [0xF3], [0xFB], [0xED, 0x46],
        ];
        byte[][] writers = [
            [0xED, 0x57], [0xED, 0x5F], [0x80], [0x04], [0x05], [0x34], [0x35],
            [0xDD, 0x86, 1], [0xFD, 0x24], [0x27], [0x2F], [0xED, 0x44], [0x37], [0x3F],
        ];
        foreach (var program in nonWriters) yield return [program, false];
        foreach (var program in writers) yield return [program, true];
    }

    [Theory]
    [MemberData(nameof(QCases))]
    public void Q_SetByFlagWritersClearedByOthers(byte[] program, bool writes)
    {
        var (cpu, _) = Create(program);
        cpu.Step();
        Assert.Equal(writes ? cpu.Registers.F : (byte)0, cpu.Registers.Q);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void Q_FlagWriterWithUnchangedF_StillWritesQ()
    {
        var (cpu, _) = Create(0xB7); // OR A, A=0 -> F stays 44
        cpu.Registers.AF = 0x0044;
        cpu.Registers.Q = 0;
        cpu.Step();
        Assert.Equal((byte)0x44, cpu.Registers.F);
        Assert.Equal((byte)0x44, cpu.Registers.Q);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Q_ClearedByInterruptAcceptance(bool nmi)
    {
        var (cpu, state) = Create(0x80);
        cpu.Step();
        Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
        if (nmi) cpu.RequestNmi(); else state.IntActive = true;
        cpu.Step();
        Assert.Equal((byte)0, cpu.Registers.Q);
        Assert.Equal(nmi ? (ushort)0x66 : (ushort)0x38, cpu.Registers.PC);
    }

    [Fact]
    public void Halt_StaysOnOpcodeAndClearsQOnEachM1()
    {
        var (cpu, state) = Create(0x76);
        cpu.Registers.IFF1 = false;
        state.IntActive = true;
        cpu.Step();
        Assert.True(cpu.Halted);
        Assert.Equal((ushort)0x8000, cpu.Registers.PC);
        for (var i = 0; i < 3; i++)
        {
            cpu.Registers.Q = 0xAA;
            var expected = cpu.Registers;
            expected.Q = 0;
            expected.IncrementR();
            state.Accesses.Clear();
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(("M1", (ushort)0x8000, 0x76), Assert.Single(state.Accesses));
        }
        Assert.Equal(16, state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Halt_ExitsOnInterruptWithNextAddress(bool nmi)
    {
        var (cpu, state) = Create(0x76);
        cpu.Step();
        state.Accesses.Clear();
        if (nmi) cpu.RequestNmi(); else state.IntActive = true;
        cpu.Step();
        Assert.False(cpu.Halted);
        Assert.Equal(nmi ? (ushort)0x66 : (ushort)0x38, cpu.Registers.PC);
        Assert.Equal((byte)0x80, state.Memory[0x8FFF]);
        Assert.Equal((byte)1, state.Memory[0x8FFE]);
        Assert.Equal(nmi ? 15 : 17, state.Cycles);
        Assert.Equal((byte)0, cpu.Registers.Q);
    }

    [Fact]
    public void Di_ClearsBothFlipFlops()
    {
        var (cpu, state) = Create(0xF3, 0);
        cpu.Step();
        state.IntActive = true;
        cpu.Step();
        Assert.False(cpu.Registers.IFF1);
        Assert.False(cpu.Registers.IFF2);
        Assert.Equal((ushort)0x8002, cpu.Registers.PC);
        Assert.Equal(8, state.Cycles);
        Assert.DoesNotContain(state.Accesses, e => e.Kind == "Ack");
    }

    [Theory]
    [InlineData(1)] [InlineData(3)]
    public void Ei_DelaysInterruptAndRepeatedKeepsBlocking(int count)
    {
        var (cpu, state) = Create(0xFB, 0xFB, 0xFB, 0);
        state.Memory[0x8000 + count] = 0;
        cpu.Registers.IFF1 = cpu.Registers.IFF2 = false;
        state.IntActive = true;
        for (var i = 0; i < count; i++)
        {
            cpu.Step();
            Assert.True(cpu.Registers.IFF1);
            Assert.True(cpu.Registers.IFF2);
            Assert.True(cpu.Registers.EiPending);
            Assert.Equal((ushort)(0x8001 + i), cpu.Registers.PC);
        }
        cpu.Step();
        Assert.False(cpu.Registers.EiPending);
        Assert.Equal((ushort)(0x8001 + count), cpu.Registers.PC);
        Assert.DoesNotContain(state.Accesses, e => e.Kind == "Ack");
        cpu.Step();
        Assert.Equal((ushort)0x38, cpu.Registers.PC);
        Assert.Equal(4 * (count + 1) + 13, state.Cycles);
        Assert.Equal((byte)(count + 1), state.Memory[0x8FFE]);
    }

    [Fact]
    public void FuseLoad_SeedsQFromF()
    {
        var test = new ZXSinclair.Net.Fuse.clsTestBase();
        test.Line1.af = 0xA528;
        var registers = ZXSinclair.Net.Fuse.FuseCpuState.Load(test);
        Assert.Equal(registers.F, registers.Q);
    }

    public static IEnumerable<object[]> InterruptCases()
    {
        byte[][] programs = [[0x27], [0x2F], [0x37], [0x3F], [0xED, 0x44],
            [0xED, 0x46], [0xED, 0x56], [0xED, 0x5E]];
        foreach (var program in programs)
        {
            yield return [program, false];
            yield return [program, true];
        }
    }

    [Theory]
    [MemberData(nameof(InterruptCases))]
    public void Control_AcceptsIntOrNmiImmediatelyAfterCompletion(byte[] program, bool nmi)
    {
        var (cpu, state) = Create(program);
        state.Memory[0x42FF] = 0x34;
        state.Memory[0x4300] = 0x12;
        cpu.Step();
        var expected = cpu.Registers;
        var returnPc = expected.PC;
        var cycles = state.Cycles;
        var mode = expected.IM;
        expected.Q = 0;
        expected.IFF1 = false;
        expected.IFF2 = nmi && expected.IFF2;
        expected.IncrementR();
        expected.SP -= 2;
        expected.PC = expected.WZ = nmi ? (ushort)0x66 : mode == 2 ? (ushort)0x1234 : (ushort)0x38;
        var events = new List<(string Kind, ushort Address, int Value)>();
        if (nmi)
        {
            events.Add(("M1", returnPc, 0));
            events.Add(("Internal", expected.IR, 1));
            cpu.RequestNmi();
        }
        else
        {
            events.Add(("Ack", 0, 0xFF));
            events.Add(("Internal", expected.IR, 1));
            state.IntActive = true;
        }
        events.Add(("Write", 0x8FFF, returnPc >> 8));
        events.Add(("Write", 0x8FFE, returnPc & 0xFF));
        if (!nmi && mode == 2)
        {
            events.Add(("Read", 0x42FF, 0x34));
            events.Add(("Read", 0x4300, 0x12));
        }
        state.Accesses.Clear();
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(events, state.Accesses);
        Assert.Equal(cycles + (nmi ? 11 : mode == 2 ? 19 : 13), state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }
}
