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

public class RotateTests
{
    private static (Z80Cpu<TestBus> Cpu, TestBusState State) Create()
    {
        var state = new TestBusState();
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

    // Independent reference uses division, remainder and bit counting, without Core tables.
    private static byte Flags(byte value)
    {
        var flags = value & 0xA8;
        if (value == 0) flags |= 0x40;
        var bits = 0;
        for (var n = (int)value; n != 0; n /= 2) bits += n % 2;
        if (bits % 2 == 0) flags |= 4;
        return (byte)flags;
    }

    private static (byte Value, byte Flags) Reference(int op, byte v, byte oldFlags)
    {
        var carry = oldFlags % 2;
        var left = op is 0 or 2 or 4 or 6;
        var outgoing = left ? v / 128 : v % 2;
        var result = op switch
        {
            0 => v * 2 % 256 + outgoing,
            1 => v / 2 + outgoing * 128,
            2 => v * 2 % 256 + carry,
            3 => v / 2 + carry * 128,
            4 => v * 2 % 256,
            5 => v / 2 + v / 128 * 128,
            6 => v * 2 % 256 + 1,
            7 => v / 2,
            _ => throw new ArgumentOutOfRangeException(nameof(op)),
        };
        return ((byte)result, (byte)(Flags((byte)result) | outgoing));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void Rotations_AllValuesAndInitialFlags_MatchReference(int op)
    {
        var (cpu, state) = Create();
        state.Memory[0x8000] = 0xCB;
        state.Memory[0x8001] = (byte)(op * 8);
        for (var v = 0; v < 256; v++)
        for (var f = 0; f < 256; f++)
        {
            cpu.Registers.PC = 0x8000;
            cpu.Registers.B = (byte)v;
            cpu.Registers.F = (byte)f;
            state.Cycles = 0;
            state.Accesses.Clear();
            var expected = Reference(op, (byte)v, (byte)f);
            cpu.Step();
            if (cpu.Registers.B != expected.Value || cpu.Registers.F != expected.Flags
                || cpu.Registers.Q != expected.Flags)
                Assert.Fail($"op={op} v={v:X2} F={f:X2}, expected={expected}");
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0, 0x07)] [InlineData(1, 0x0F)]
    [InlineData(2, 0x17)] [InlineData(3, 0x1F)]
    public void AccumulatorRotations_AllValuesAndFlags(int op, byte opcode)
    {
        var (cpu, state) = Create();
        state.Memory[0x8000] = opcode;
        for (var a = 0; a < 256; a++)
        for (var f = 0; f < 256; f++)
        {
            cpu.Registers.PC = 0x8000;
            cpu.Registers.A = (byte)a;
            cpu.Registers.F = (byte)f;
            state.Cycles = 0;
            state.Accesses.Clear();
            var expected = Reference(op, (byte)a, (byte)f);
            var flags = (byte)((f & 0xC4) | (expected.Value & 0x28) | (expected.Flags & 1));
            cpu.Step();
            if (cpu.Registers.A != expected.Value || cpu.Registers.F != flags || cpu.Registers.Q != flags)
                Assert.Fail($"op={op} A={a:X2} F={f:X2}, expected A={expected.Value:X2} F={flags:X2}");
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    private static byte Register(in Z80Registers r, int register) => register switch
    {
        0 => r.B, 1 => r.C, 2 => r.D, 3 => r.E, 4 => r.H, 5 => r.L, 7 => r.A,
        _ => throw new ArgumentOutOfRangeException(nameof(register)),
    };

    private static void SetRegister(ref Z80Registers r, int register, byte value)
    {
        switch (register)
        {
            case 0: r.B = value; break;
            case 1: r.C = value; break;
            case 2: r.D = value; break;
            case 3: r.E = value; break;
            case 4: r.H = value; break;
            case 5: r.L = value; break;
            case 7: r.A = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(register));
        }
    }

    public static IEnumerable<object[]> BusCases()
    {
        foreach (var pc in new ushort[] { 0x8000, 0xFFFF })
        {
            for (var opcode = 0; opcode < 64; opcode++)
            {
                yield return [new byte[] { 0xCB, (byte)opcode }, pc, (ushort)0x4000];
                foreach (var prefix in new byte[] { 0xDD, 0xFD })
                foreach (var displacement in new byte[] { 7, 0xFF, 0x80 })
                    yield return [new byte[] { prefix, 0xCB, displacement, (byte)opcode }, pc,
                        displacement == 7 ? (ushort)0xFFFF : (ushort)0x4000];
            }
            foreach (var opcode in new byte[] { 7, 15, 23, 31 })
            {
                yield return [new byte[] { opcode }, pc, (ushort)0x4000];
                yield return [new byte[] { 0xDD, opcode }, pc, (ushort)0x4000];
                yield return [new byte[] { 0xFD, opcode }, pc, (ushort)0x4000];
            }
            foreach (var opcode in new byte[] { 0x67, 0x6F })
            foreach (var hl in new ushort[] { 0x4000, 0xFFFF })
            {
                if (pc == 0xFFFF && hl == 0xFFFF) continue; // Keep data separate from the program.
                yield return [new byte[] { 0xED, opcode }, pc, hl];
                yield return [new byte[] { 0xDD, 0xED, opcode }, pc, hl];
                yield return [new byte[] { 0xFD, 0xED, opcode }, pc, hl];
            }
        }
    }

    [Theory]
    [MemberData(nameof(BusCases))]
    public void EveryForm_EmitsExactBusCyclesPreservesRegistersAndSetsQ(byte[] program, ushort pc, ushort pair)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = pc;
        var indexed = program.Length == 4;
        var cb = indexed || program[0] == 0xCB;
        var digit = program.Contains((byte)0xED);
        if (indexed)
        {
            if (program[0] == 0xDD) cpu.Registers.IX = pair; else cpu.Registers.IY = pair;
        }
        else if (cb || digit) cpu.Registers.HL = pair;
        var expected = cpu.Registers;
        var events = new List<(string Kind, ushort Address, int Value)>();
        for (var i = 0; i < program.Length; i++)
        {
            var address = (ushort)(pc + i);
            state.Memory[address] = program[i];
            events.Add((indexed && i >= 2 ? "Read" : "M1", address, program[i]));
        }
        expected.PC = (ushort)(pc + program.Length);
        expected.R = (byte)(0x80 | ((0x7F + (indexed ? 2 : program.Length)) & 0x7F));
        var opcode = program[^1];
        var register = opcode & 7;
        var memory = digit || indexed || cb && register == 6;
        var target = indexed ? (ushort)(pair + (sbyte)program[2]) : expected.HL;
        if (indexed)
        {
            expected.WZ = target;
            events.Add(("Internal", (ushort)(pc + 3), 2));
        }
        var value = memory ? (byte)0x93 : cb ? Register(expected, register) : expected.A;
        if (memory)
        {
            state.Memory[target] = value;
            events.Add(("Read", target, value));
            events.Add(("Internal", target, digit ? 4 : 1));
        }
        byte result;
        if (digit)
        {
            var a = expected.A;
            var low = a % 16;
            var high = value / 16;
            var mlow = value % 16;
            result = opcode == 0x6F ? (byte)(mlow * 16 + low) : (byte)(low * 16 + high);
            expected.A = (byte)(a / 16 * 16 + (opcode == 0x6F ? high : mlow));
            expected.F = (byte)((expected.F & 1) | Flags(expected.A));
            expected.WZ = (ushort)(target + 1);
        }
        else
        {
            var op = opcode / 8;
            var rotated = Reference(op, value, expected.F);
            result = rotated.Value;
            expected.F = cb ? rotated.Flags
                : (byte)((expected.F & 0xC4) | (result & 0x28) | (rotated.Flags & 1));
            if (!cb) expected.A = result;
            else if (register != 6) SetRegister(ref expected, register, result);
        }
        expected.Q = expected.F;
        if (memory) events.Add(("Write", target, result));
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(events, state.Accesses);
        Assert.Equal(indexed ? 23 : digit ? 18 + (program.Length - 2) * 4
            : cb ? memory ? 15 : 8 : program.Length * 4, state.Cycles);
        if (memory) Assert.Equal(result, state.Memory[target]);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0x67)] [InlineData(0x6F)]
    public void RldRrd_AllAccumulatorMemoryAndCarryValues(byte opcode)
    {
        var (cpu, state) = Create();
        state.Memory[0x8000] = 0xED;
        state.Memory[0x8001] = opcode;
        for (var a = 0; a < 256; a++)
        for (var m = 0; m < 256; m++)
        for (var carry = 0; carry < 2; carry++)
        {
            cpu.Registers.PC = 0x8000;
            cpu.Registers.A = (byte)a;
            cpu.Registers.F = (byte)(0xFE | carry);
            state.Memory[cpu.Registers.HL] = (byte)m;
            state.Cycles = 0;
            state.Accesses.Clear();
            var newA = a / 16 * 16 + (opcode == 0x6F ? m / 16 : m % 16);
            var newM = opcode == 0x6F ? m % 16 * 16 + a % 16 : a % 16 * 16 + m / 16;
            var flags = (byte)(Flags((byte)newA) | carry);
            cpu.Step();
            if (cpu.Registers.A != newA || state.Memory[cpu.Registers.HL] != newM
                || cpu.Registers.F != flags || cpu.Registers.Q != flags
                || cpu.Registers.WZ != (ushort)(cpu.Registers.HL + 1))
                Assert.Fail($"op={opcode:X2} A={a:X2} memory={m:X2} C={carry}");
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0x67, 0x33, 0x69, 0x25)]
    [InlineData(0x6F, 0x39, 0x36, 0x2D)]
    public void RldRrd_KnownNibbles(byte opcode, byte a, byte memory, byte flags)
    {
        var (cpu, state) = Create();
        state.Memory[0x8000] = 0xED;
        state.Memory[0x8001] = opcode;
        cpu.Registers.A = 0x36;
        state.Memory[cpu.Registers.HL] = 0x93;
        cpu.Step();
        Assert.Equal(a, cpu.Registers.A);
        Assert.Equal(memory, state.Memory[cpu.Registers.HL]);
        Assert.Equal(flags, cpu.Registers.F);
    }

    [Theory]
    [InlineData(new byte[] { 7 })]
    [InlineData(new byte[] { 0xCB, 0x30 })]
    [InlineData(new byte[] { 0xCB, 0x36 })]
    [InlineData(new byte[] { 0xDD, 0xCB, 0xFF, 0x37 })]
    [InlineData(new byte[] { 0xED, 0x67 })]
    public void IntAndNmi_AreAcceptedAfterEachPattern(byte[] program)
    {
        foreach (var nmi in new[] { false, true })
        {
            var (cpu, state) = Create();
            program.CopyTo(state.Memory, 0x8000);
            cpu.Step();
            var expected = cpu.Registers;
            var returnPc = expected.PC;
            var before = state.Cycles;
            expected.Q = 0;
            expected.IFF1 = false;
            expected.IFF2 = nmi;
            expected.IncrementR();
            expected.SP -= 2;
            expected.PC = expected.WZ = nmi ? (ushort)0x66 : (ushort)0x38;
            state.Accesses.Clear();
            if (nmi) cpu.RequestNmi(); else state.IntActive = true;
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(before + (nmi ? 11 : 13), state.Cycles);
            Assert.Equal((byte)returnPc, state.Memory[expected.SP]);
            Assert.Equal((byte)(returnPc >> 8), state.Memory[(ushort)(expected.SP + 1)]);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }
}
