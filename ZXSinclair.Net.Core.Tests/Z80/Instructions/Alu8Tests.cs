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

public class Alu8Tests
{
    private static (Z80Cpu<TestBus> Cpu, TestBusState State) Create()
    {
        var state = new TestBusState();
        var cpu = new Z80Cpu<TestBus>(new(state));
        cpu.Registers = new Z80Registers
        {
            AF = 0xA5FF, BC = 0x1234, DE = 0x5678, HL = 0x9ABC,
            AF_ = 0xDEF1, BC_ = 0x2345, DE_ = 0x6789, HL_ = 0xABCD,
            IX = 0x1357, IY = 0x2468, SP = 0x9000, WZ = 0xBEEF,
            I = 0x42, R = 0x91, IFF1 = true, IFF2 = true, IM = 1, Q = 0xAA,
        };
        return (cpu, state);
    }

    // Reference uses integer/nibble arithmetic and signed range checks, without Core tables
    // or its XOR formulas. INC/DEC keep the incoming carry; CP copies operand bits 3/5.
    private static (byte Result, byte Flags) Reference(int op, int a, int v, int carry)
    {
        int result, signed;
        bool half = false, overflow = false, negative = false, outputCarry = false;
        if (op is 0 or 1)
        {
            var c = op == 1 ? carry : 0;
            result = a + v + c;
            signed = (sbyte)a + (sbyte)v + c;
            half = (a % 16) + (v % 16) + c > 15;
            overflow = signed < -128 || signed > 127;
            outputCarry = result > 255;
        }
        else if (op is 2 or 3 or 7)
        {
            var c = op == 3 ? carry : 0;
            result = a - v - c;
            signed = (sbyte)a - (sbyte)v - c;
            half = (a % 16) < (v % 16) + c;
            overflow = signed < -128 || signed > 127;
            outputCarry = result < 0;
            negative = true;
        }
        else if (op is 8 or 9)
        {
            var increment = op == 8;
            result = v + (increment ? 1 : -1);
            half = increment ? v % 16 == 15 : v % 16 == 0;
            overflow = increment ? v == 127 : v == 128;
            negative = !increment;
            outputCarry = carry != 0;
        }
        else
        {
            result = op switch { 4 => a & v, 5 => a ^ v, 6 => a | v, _ => throw new ArgumentOutOfRangeException(nameof(op)) };
            half = op == 4;
            var count = 0;
            for (var bit = 0; bit < 8; bit++)
                if ((result & (1 << bit)) != 0) count++;
            overflow = count % 2 == 0;
        }
        var x = (byte)result;
        var flags = x >= 128 ? 0x80 : 0;
        if (x == 0) flags |= 0x40;
        var undocumented = op == 7 ? v : x;
        if ((undocumented & 0x20) != 0) flags |= 0x20;
        if ((undocumented & 0x08) != 0) flags |= 0x08;
        if (half) flags |= 0x10;
        if (overflow) flags |= 0x04;
        if (negative) flags |= 0x02;
        if (outputCarry) flags |= 0x01;
        return (op == 7 ? (byte)a : x, (byte)flags);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void Alu_AllOperands_MatchReference(int op)
    {
        var (cpu, state) = Create();
        state.Memory[0] = (byte)(0x80 + op * 8);
        for (var a = 0; a < 256; a++)
        for (var v = 0; v < 256; v++)
        for (var carry = 0; carry < 2; carry++)
        {
            cpu.Registers.PC = 0;
            cpu.Registers.A = (byte)a;
            cpu.Registers.B = (byte)v;
            cpu.Registers.F = (byte)(0xFE | carry);
            state.Cycles = 0;
            state.Accesses.Clear();
            var expected = Reference(op, a, v, carry);
            cpu.Step();
            if (cpu.Registers.A != expected.Result || cpu.Registers.F != expected.Flags)
                Assert.Fail($"op={op} A={a:X2} operand={v:X2} C={carry}: expected {expected}, actual ({cpu.Registers.A}, {cpu.Registers.F})");
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(8)] [InlineData(9)]
    public void IncDec_AllValues_MatchReference(int op)
    {
        var (cpu, state) = Create();
        state.Memory[0] = (byte)(op == 8 ? 0x04 : 0x05);
        for (var v = 0; v < 256; v++)
        for (var carry = 0; carry < 2; carry++)
        {
            cpu.Registers.PC = 0;
            cpu.Registers.B = (byte)v;
            cpu.Registers.F = (byte)(0xFE | carry);
            state.Cycles = 0;
            state.Accesses.Clear();
            var expected = Reference(op, 0, v, carry);
            cpu.Step();
            Assert.Equal(expected.Result, cpu.Registers.B);
            Assert.Equal(expected.Flags, cpu.Registers.F);
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void FlagTables_IncDec()
    {
        for (var result = 0; result < 256; result++)
        {
            Assert.Equal(Reference(8, 0, (byte)(result - 1), 0).Flags, Z80Flags.Inc[result]);
            Assert.Equal(Reference(9, 0, (byte)(result + 1), 0).Flags, Z80Flags.Dec[result]);
        }
    }

    private static byte Register(in Z80Registers r, int source) => source switch
    {
        0 => r.B, 1 => r.C, 2 => r.D, 3 => r.E, 4 => r.H, 5 => r.L, 7 => r.A,
        _ => throw new ArgumentOutOfRangeException(nameof(source)),
    };

    private static void SetRegister(ref Z80Registers r, int source, byte value)
    {
        switch (source)
        {
            case 0: r.B = value; break;
            case 1: r.C = value; break;
            case 2: r.D = value; break;
            case 3: r.E = value; break;
            case 4: r.H = value; break;
            case 5: r.L = value; break;
            case 7: r.A = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(source));
        }
    }

    public static IEnumerable<object[]> AddressCases()
    {
        foreach (var prefix in new[] { 0, 0xDD, 0xFD })
        {
            for (var opcode = 0x80; opcode <= 0xBF; opcode++)
                yield return [prefix, opcode, 5];
            foreach (var opcode in new[] { 0xC6, 0xCE, 0xD6, 0xDE, 0xE6, 0xEE, 0xF6, 0xFE })
                yield return [prefix, opcode, 5];
            for (var source = 0; source < 8; source++)
            {
                yield return [prefix, 0x04 + source * 8, 5];
                yield return [prefix, 0x05 + source * 8, 5];
            }
        }
        foreach (var prefix in new[] { 0xDD, 0xFD })
        foreach (var displacement in new[] { -1, -128 })
        {
            for (var op = 0; op < 8; op++)
                yield return [prefix, 0x86 + op * 8, displacement];
            yield return [prefix, 0x34, displacement];
            yield return [prefix, 0x35, displacement];
        }
    }

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void AllAddressingModes_PreserveOtherRegistersAndEmitExactCycles(int prefix, int opcode, int displacement) =>
        VerifyAddressCase(prefix, opcode, displacement, 0x8000);

    private static void VerifyAddressCase(int prefix, int opcode, int displacement, ushort pc, ushort? index = null)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = pc;
        cpu.Registers.R = 0xFF;
        if (index.HasValue)
        {
            cpu.Registers.IX = index.Value;
            cpu.Registers.IY = index.Value;
        }
        var expected = cpu.Registers;
        var events = new List<(string Kind, ushort Address, int Value)>();
        var cursor = pc;
        if (prefix != 0)
        {
            state.Memory[cursor] = (byte)prefix;
            events.Add(("M1", cursor, prefix));
            cursor = (ushort)(cursor + 1);
        }
        state.Memory[cursor] = (byte)opcode;
        events.Add(("M1", cursor, opcode));
        cursor = (ushort)(cursor + 1);

        var alu = opcode >= 0x80;
        var immediate = opcode >= 0xC0;
        var op = alu ? (immediate ? (opcode - 0xC6) / 8 : (opcode - 0x80) / 8) : ((opcode & 1) == 0 ? 8 : 9);
        var source = alu ? opcode & 7 : (opcode >> 3) & 7;
        var memory = !immediate && source == 6;
        var half = prefix != 0 && !immediate && source is 4 or 5;
        var pair = prefix == 0xDD ? expected.IX : expected.IY;
        ushort address = expected.HL;
        byte operand;
        if (immediate)
        {
            operand = 0x28;
            state.Memory[cursor] = operand;
            events.Add(("Read", cursor, operand));
            cursor = (ushort)(cursor + 1);
        }
        else if (memory)
        {
            if (prefix != 0)
            {
                state.Memory[cursor] = (byte)displacement;
                events.Add(("Read", cursor, (byte)displacement));
                events.Add(("Internal", cursor, 5));
                cursor = (ushort)(cursor + 1);
                address = (ushort)(pair + displacement);
                expected.WZ = address;
            }
            operand = 0x7F;
            state.Memory[address] = operand;
            events.Add(("Read", address, operand));
        }
        else
            operand = half ? (source == 4 ? (byte)(pair >> 8) : (byte)pair) : Register(expected, source);

        var result = Reference(op, expected.A, operand, expected.F & 1);
        expected.F = result.Flags;
        expected.Q = result.Flags;
        if (alu) expected.A = result.Result;
        else if (memory)
        {
            events.Add(("Internal", address, 1));
            events.Add(("Write", address, result.Result));
        }
        else if (half)
        {
            pair = source == 4 ? (ushort)((pair & 0xFF) | (result.Result << 8))
                : (ushort)((pair & 0xFF00) | result.Result);
            if (prefix == 0xDD) expected.IX = pair; else expected.IY = pair;
        }
        else SetRegister(ref expected, source, result.Result);
        expected.PC = cursor;
        expected.R = (byte)(0x80 | ((0x7F + (prefix == 0 ? 1 : 2)) & 0x7F));

        cpu.Step();

        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(events, state.Accesses);
        Assert.Equal(events.Sum(e => e.Kind switch { "M1" => 4, "Internal" => e.Value, _ => 3 }), state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        if (memory) Assert.Equal(alu ? operand : result.Result, state.Memory[address]);
    }

    [Theory]
    [InlineData(0, 0xC6, 0xFFFF)]
    [InlineData(0xDD, 0x8E, 0xFFFE)]
    [InlineData(0xFD, 0x35, 0xFFFF)]
    [InlineData(0, 0x04, 0xFFFF)]
    public void PcAndRefresh_Wrap(int prefix, int opcode, ushort pc) =>
        VerifyAddressCase(prefix, opcode, -128, pc);

    [Theory]
    [InlineData(0xDD, 0x86, -1, 0)]
    [InlineData(0xFD, 0x35, 5, 0xFFFE)]
    public void EffectiveAddress_Wraps(int prefix, int opcode, int displacement, ushort index) =>
        VerifyAddressCase(prefix, opcode, displacement, 0x8000, index);

    [Theory]
    [InlineData(0, 0x7F, 1, 0, 0x80, 0x94)]
    [InlineData(0, 0xFF, 1, 0, 0, 0x51)]
    [InlineData(0, 0x0F, 1, 0, 0x10, 0x10)]
    [InlineData(0, 0x80, 0x80, 0, 0, 0x45)]
    [InlineData(2, 0x80, 1, 0, 0x7F, 0x3E)]
    [InlineData(2, 0, 1, 0, 0xFF, 0xBB)]
    [InlineData(2, 0x10, 1, 0, 0x0F, 0x1A)]
    [InlineData(7, 0, 0x28, 0, 0, 0xBB)]
    [InlineData(4, 3, 1, 1, 1, 0x10)]
    [InlineData(4, 3, 3, 1, 3, 0x14)]
    [InlineData(5, 3, 3, 1, 0, 0x44)]
    [InlineData(6, 0, 1, 1, 1, 0)]
    [InlineData(1, 1, 1, 0, 2, 0)]
    [InlineData(1, 1, 1, 1, 3, 0)]
    [InlineData(3, 3, 1, 0, 2, 2)]
    [InlineData(3, 3, 1, 1, 1, 2)]
    public void KnownFlagBoundaries(int op, byte a, byte operand, byte carry, byte result, byte flags)
    {
        var (cpu, state) = Create();
        state.Memory[0] = (byte)(0x80 + op * 8);
        cpu.Registers.A = a;
        cpu.Registers.B = operand;
        cpu.Registers.F = (byte)(0xFE | carry);
        cpu.Step();
        Assert.Equal(result, cpu.Registers.A);
        Assert.Equal(flags, cpu.Registers.F);
    }

    [Theory]
    [InlineData(0xC6)] [InlineData(0xCE)] [InlineData(0xD6)] [InlineData(0xDE)]
    [InlineData(0xE6)] [InlineData(0xEE)] [InlineData(0xF6)] [InlineData(0xFE)]
    [InlineData(0x04)] [InlineData(0x05)]
    public void Alu8_AllowsIntOnNextStep(byte opcode)
    {
        var (cpu, state) = Create();
        state.Memory[0] = opcode;
        state.Memory[1] = 1;
        state.IntAfterCycle = opcode >= 0xC0 ? 7 : 4;
        cpu.Step();
        var pc = cpu.Registers.PC;
        var cycles = state.Cycles;
        var refresh = cpu.Registers.R;
        state.Accesses.Clear();
        cpu.Step();
        Assert.Equal((ushort)0x38, cpu.Registers.PC);
        Assert.False(cpu.Registers.IFF1);
        Assert.False(cpu.Registers.IFF2);
        Assert.Equal(cycles + 13, state.Cycles);
        Assert.Equal((byte)(refresh + 1), cpu.Registers.R);
        Assert.Equal(new[] { ("Ack", (ushort)0, 0xFF), ("Internal", (ushort)0x4293, 1),
            ("Write", (ushort)0x8FFF, pc >> 8), ("Write", (ushort)0x8FFE, pc & 0xFF) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }
}
