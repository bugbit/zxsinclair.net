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

public class Alu16Tests
{
    private static readonly ushort[] Edges =
        [0, 1, 7, 8, 0x07FF, 0x0800, 0x0FFF, 0x1000,
            0x1FFF, 0x2000, 0x7FFE, 0x7FFF, 0x8000, 0x8001, 0xFFFE, 0xFFFF];

    private static (Z80Cpu<TestBus> Cpu, TestBusState State) Create(params byte[] program)
    {
        var state = new TestBusState();
        program.CopyTo(state.Memory, 0x8000);
        var cpu = new Z80Cpu<TestBus>(new(state));
        cpu.Registers = new Z80Registers
        {
            AF = 0xA5FF, BC = 0x1234, DE = 0x5678, HL = 0x9ABC,
            AF_ = 0xDEF1, BC_ = 0x2345, DE_ = 0x6789, HL_ = 0xABCD,
            IX = 0x1357, IY = 0x2468, SP = 0x9000, PC = 0x8000, WZ = 0xBEEF,
            I = 0x42, R = 0xFF, IFF1 = true, IFF2 = true, IM = 1, Q = 0xAA,
        };
        return (cpu, state);
    }

    // Independent reference: integer/nibble arithmetic and signed range checks.
    // No Core lookup tables or XOR-based H/overflow formulas.
    private static (ushort Result, byte Flags) Reference(int op, ushort a, ushort v, byte flags)
    {
        var carry = op == 0 ? 0 : flags & 1;
        var subtract = op == 2;
        var wide = subtract ? a - v - carry : a + v + carry;
        var result = (ushort)wide;
        var f = op == 0 ? flags & 0xC4 : 0;
        var high = result / 256;
        if ((high & 0x20) != 0) f |= 0x20;
        if ((high & 0x08) != 0) f |= 0x08;
        if (subtract ? a % 4096 < v % 4096 + carry : a % 4096 + v % 4096 + carry > 4095)
            f |= 0x10;
        if (wide < 0 || wide > 65535) f |= 1;
        if (op != 0)
        {
            if (result >= 32768) f |= 0x80;
            if (result == 0) f |= 0x40;
            var signed = subtract ? (short)a - (short)v - carry : (short)a + (short)v + carry;
            if (signed < -32768 || signed > 32767) f |= 4;
            if (subtract) f |= 2;
        }
        return (result, (byte)f);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void Arithmetic_MatchesReferenceAcrossAllValuesEdgesAndSeededPairs(int op)
    {
        var (cpu, state) = op == 0 ? Create(0x09) : Create(0xED, op == 1 ? (byte)0x4A : (byte)0x42);
        void Check(ushort a, ushort v, byte oldFlags)
        {
            cpu.Registers.PC = 0x8000;
            cpu.Registers.HL = a;
            cpu.Registers.BC = v;
            cpu.Registers.F = oldFlags;
            state.Cycles = 0;
            state.Accesses.Clear();
            var expected = Reference(op, a, v, oldFlags);
            cpu.Step();
            if (cpu.Registers.HL != expected.Result || cpu.Registers.F != expected.Flags
                || cpu.Registers.Q != expected.Flags || cpu.Registers.WZ != (ushort)(a + 1))
                Assert.Fail($"op={op} a={a:X4} v={v:X4} F={oldFlags:X2}: expected {expected}, actual HL={cpu.Registers.HL:X4} F={cpu.Registers.F:X2}");
        }
        // Each possible 16-bit value appears on both sides against every boundary value.
        for (var value = 0; value < 65536; value++)
        foreach (var edge in Edges)
        for (var carry = 0; carry < (op == 0 ? 1 : 2); carry++)
        {
            var flags = op == 0 ? (byte)(value * 37 + edge * 13)
                : (byte)(((value * 37 + edge * 13) & 0xFE) | carry);
            Check((ushort)value, edge, flags);
            Check(edge, (ushort)value, flags);
        }
        uint seed = 0xC0FFEE;
        for (var i = 0; i < 200000; i++)
        {
            seed = unchecked(seed * 1664525 + 1013904223);
            var a = (ushort)(seed >> 16);
            seed = unchecked(seed * 1664525 + 1013904223);
            var v = (ushort)(seed >> 16);
            var flags = (byte)(seed >> 8);
            Check(a, v, flags);
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0, 0x9ABC, 0x5678, 0, 0xF134, 0x30)]
    [InlineData(0, 0x0FFF, 1, 0, 0x1000, 0x10)]
    [InlineData(0, 0xFFFF, 1, 0, 0, 0x11)]
    [InlineData(0, 0x2000, 0x0800, 0xC4, 0x2800, 0xEC)]
    [InlineData(1, 0x7FFF, 0, 1, 0x8000, 0x94)]
    [InlineData(1, 0xFFFF, 0, 1, 0, 0x51)]
    [InlineData(1, 0x9AC8, 0x24B5, 1, 0xBF7E, 0xA8)]
    [InlineData(2, 0x1234, 0x1234, 0, 0, 0x42)]
    [InlineData(2, 0, 1, 0, 0xFFFF, 0xBB)]
    [InlineData(2, 0x8000, 1, 0, 0x7FFF, 0x3E)]
    [InlineData(2, 0x1000, 1, 0, 0x0FFF, 0x1A)]
    public void KnownFlagsAndBoundaries(int op, ushort a, ushort v, byte oldFlags, ushort result, byte flags)
    {
        var (cpu, _) = op == 0 ? Create(0x09) : Create(0xED, op == 1 ? (byte)0x4A : (byte)0x42);
        cpu.Registers.HL = a;
        cpu.Registers.BC = v;
        cpu.Registers.F = oldFlags;
        cpu.Step();
        Assert.Equal(result, cpu.Registers.HL);
        Assert.Equal(flags, cpu.Registers.F);
        Assert.Equal(flags, cpu.Registers.Q);
    }

    private static ushort Pair(in Z80Registers r, int pair, int prefix) => pair switch
    {
        0 => r.BC, 1 => r.DE, 2 => prefix == 0xDD ? r.IX : prefix == 0xFD ? r.IY : r.HL,
        3 => r.SP, _ => throw new ArgumentOutOfRangeException(nameof(pair)),
    };

    private static void SetPair(ref Z80Registers r, int pair, int prefix, ushort value)
    {
        switch (pair)
        {
            case 0: r.BC = value; break;
            case 1: r.DE = value; break;
            case 2:
                if (prefix == 0xDD) r.IX = value;
                else if (prefix == 0xFD) r.IY = value;
                else r.HL = value;
                break;
            case 3: r.SP = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(pair));
        }
    }

    public static IEnumerable<object[]> AddressCases()
    {
        foreach (var prefix in new[] { 0, 0xDD, 0xFD })
        for (var pair = 0; pair < 4; pair++)
        {
            var add = (byte)(9 + 16 * pair);
            var inc = (byte)(3 + 16 * pair);
            var dec = (byte)(11 + 16 * pair);
            var adc = (byte)(0x4A + 16 * pair);
            var sbc = (byte)(0x42 + 16 * pair);
            foreach (var pc in new[] { 0x8000, 0xFFFF })
            {
                foreach (var opcode in new[] { add, inc, dec })
                    yield return [prefix == 0 ? new byte[] { opcode } : new byte[] { (byte)prefix, opcode }, pc];
                foreach (var opcode in new[] { adc, sbc })
                    yield return [prefix == 0 ? new byte[] { 0xED, opcode } : new byte[] { (byte)prefix, 0xED, opcode }, pc];
            }
        }
    }

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void AllPairsPrefixesAndPcWrap_PreserveRegistersEmitIrCyclesAndQ(byte[] program, ushort pc) =>
        VerifyAddressCase(program, pc);

    private static void VerifyAddressCase(byte[] program, ushort pc, ushort? target = null)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = pc;
        var opcode = program[^1];
        var ed = program.Contains((byte)0xED);
        var prefix = !ed && program.Length > 1 ? program[0] : 0;
        var pair = (opcode >> 4) & 3;
        var arithmetic = ed || (opcode & 0x0F) == 9;
        var destination = arithmetic ? 2 : pair;
        if (target.HasValue) SetPair(ref cpu.Registers, destination, prefix, target.Value);
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
        events.Add(("Internal", expected.IR, arithmetic ? 7 : 2));
        var a = Pair(expected, destination, prefix);
        if (arithmetic)
        {
            var op = ed ? (opcode & 8) != 0 ? 1 : 2 : 0;
            var result = Reference(op, a, Pair(expected, pair, prefix), expected.F);
            SetPair(ref expected, destination, prefix, result.Result);
            expected.F = expected.Q = result.Flags;
            expected.WZ = (ushort)(a + 1);
        }
        else SetPair(ref expected, destination, prefix, (ushort)(a + ((opcode & 8) == 0 ? 1 : -1)));
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(events, state.Accesses);
        Assert.Equal(4 * program.Length + (arithmetic ? 7 : 2), state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(new byte[] { 0x33 }, 0xFFFF)]
    [InlineData(new byte[] { 0x0B }, 0)]
    [InlineData(new byte[] { 0xDD, 0x23 }, 0xFFFF)]
    [InlineData(new byte[] { 0xFD, 0x2B }, 0)]
    [InlineData(new byte[] { 0x09 }, 0xFFFF)]
    [InlineData(new byte[] { 0xDD, 0x29 }, 0xFFFF)]
    [InlineData(new byte[] { 0xED, 0x4A }, 0xFFFF)]
    [InlineData(new byte[] { 0xED, 0x42 }, 0)]
    public void PairAndWz_Wrap(byte[] program, ushort initial) => VerifyAddressCase(program, 0x8000, initial);

    [Fact]
    public void IncDec_AllWordValues_PreserveEveryFlagAndWz()
    {
        foreach (var opcode in new byte[] { 3, 11 })
        {
            var (cpu, state) = Create(opcode);
            for (var value = 0; value < 65536; value++)
            {
                cpu.Registers.PC = 0x8000;
                cpu.Registers.BC = (ushort)value;
                cpu.Registers.F = (byte)value;
                cpu.Registers.Q = 0xAA;
                state.Cycles = 0;
                state.Accesses.Clear();
                cpu.Step();
                Assert.Equal((ushort)(value + (opcode == 3 ? 1 : -1)), cpu.Registers.BC);
                Assert.Equal((byte)value, cpu.Registers.F);
                Assert.Equal((ushort)0xBEEF, cpu.Registers.WZ);
                Assert.Equal((byte)0, cpu.Registers.Q);
            }
        }
    }

    [Theory]
    [InlineData(new byte[] { 9 }, false)] [InlineData(new byte[] { 9 }, true)]
    [InlineData(new byte[] { 0xDD, 9 }, false)] [InlineData(new byte[] { 0xDD, 9 }, true)]
    [InlineData(new byte[] { 0xED, 0x4A }, false)] [InlineData(new byte[] { 0xED, 0x4A }, true)]
    [InlineData(new byte[] { 0xED, 0x42 }, false)] [InlineData(new byte[] { 0xED, 0x42 }, true)]
    [InlineData(new byte[] { 3 }, false)] [InlineData(new byte[] { 3 }, true)]
    [InlineData(new byte[] { 11 }, false)] [InlineData(new byte[] { 11 }, true)]
    public void AcceptsIntOrNmiOnNextStep(byte[] program, bool nmi)
    {
        var (cpu, state) = Create(program);
        cpu.Step();
        var expected = cpu.Registers;
        var returnPc = expected.PC;
        var cycles = state.Cycles;
        expected.Q = 0;
        expected.IFF1 = false;
        expected.IFF2 = nmi && expected.IFF2;
        expected.IncrementR();
        var ir = expected.IR;
        var oldSp = expected.SP;
        expected.SP -= 2;
        expected.PC = expected.WZ = nmi ? (ushort)0x66 : (ushort)0x38;
        state.Accesses.Clear();
        if (nmi) cpu.RequestNmi(); else state.IntActive = true;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        var events = new List<(string Kind, ushort Address, int Value)>();
        if (nmi)
        {
            events.Add(("M1", returnPc, 0));
            events.Add(("Internal", ir, 1));
        }
        else
        {
            events.Add(("Ack", 0, 0xFF));
            events.Add(("Internal", ir, 1));
        }
        events.Add(("Write", (ushort)(oldSp - 1), returnPc >> 8));
        events.Add(("Write", (ushort)(oldSp - 2), returnPc & 0xFF));
        Assert.Equal(events, state.Accesses);
        Assert.Equal(cycles + (nmi ? 11 : 13), state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }
}
