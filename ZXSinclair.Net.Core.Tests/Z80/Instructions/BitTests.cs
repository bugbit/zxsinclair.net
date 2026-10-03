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

public class BitTests
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
            IX = 0x1357, IY = 0x2468, SP = 0x9000, PC = 0x8000, WZ = 0xBEEF,
            I = 0x42, R = 0xFF, IFF1 = true, IFF2 = true, IM = 1, Q = 0,
        };
        return (cpu, state);
    }

    // Independent reference: no Core flags table or parity computation.
    private static byte Reference(int bit, byte value, byte flags, byte source)
    {
        var set = (value & (1 << bit)) != 0;
        return (byte)((flags & 1) | 0x10 | (source & 0x28)
            | (!set ? 0x44 : bit == 7 ? 0x80 : 0));
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

    private static Z80Registers AfterFetch(Z80Registers r, int length = 2)
    {
        r.PC += (ushort)length;
        r.IncrementR();
        r.IncrementR();
        return r;
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void BitRegister_AllBitsAllValues(int bit)
    {
        var (cpu, state) = Create(0xCB, (byte)(0x40 + bit * 8));
        for (var value = 0; value < 256; value++)
        for (var flags = 0; flags < 256; flags++)
        {
            cpu.Registers.PC = 0x8000;
            cpu.Registers.B = (byte)value;
            cpu.Registers.F = (byte)flags;
            state.Cycles = 0;
            state.Accesses.Clear();
            var expected = AfterFetch(cpu.Registers);
            expected.F = expected.Q = Reference(bit, (byte)value, (byte)flags, (byte)value);
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(8, state.Cycles);
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(7)]
    public void BitRegister_AllRegisters(int register)
    {
        for (var bit = 0; bit < 8; bit++)
        {
            var (cpu, state) = Create(0xCB, (byte)(0x40 + bit * 8 + register));
            SetRegister(ref cpu.Registers, register, 0xA8);
            var expected = AfterFetch(cpu.Registers);
            expected.F = expected.Q = Reference(bit, 0xA8, expected.F, 0xA8);
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(8, state.Cycles);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0x2800)] [InlineData(0)] [InlineData(0xFFFF)]
    public void BitHl_F5F3FromWz(ushort wz)
    {
        var (cpu, state) = Create(0xCB, 0x5E);
        cpu.Registers.HL = 0xA000;
        cpu.Registers.WZ = wz;
        state.Memory[0xA000] = 0x08;
        var expected = AfterFetch(cpu.Registers);
        expected.F = expected.Q = Reference(3, 8, expected.F, (byte)(wz >> 8));
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(12, state.Cycles);
        Assert.Equal(new (string, ushort, int)[] {
            ("M1", 0x8000, 0xCB), ("M1", 0x8001, 0x5E),
            ("Read", 0xA000, 8), ("Internal", 0xA000, 1),
        }, state.Accesses);
        Assert.Equal(8, state.Memory[0xA000]);
    }

    [Fact]
    public void BitHl_AfterInstructionThatSetsWz()
    {
        var (cpu, state) = Create(0x3A, 0xFF, 0x27, 0xCB, 0x46);
        state.Memory[0x27FF] = 0x5A;
        state.Memory[cpu.Registers.HL] = 1;
        cpu.Step();
        Assert.Equal((ushort)0x2800, cpu.Registers.WZ);
        cpu.Step();
        Assert.Equal((byte)0x39, cpu.Registers.F);
        Assert.Equal((ushort)0x2800, cpu.Registers.WZ);
        Assert.Equal(25, state.Cycles);
    }

    [Theory]
    [InlineData(0xDD, 0x2801)] [InlineData(0xFD, 0x2801)]
    [InlineData(0xDD, 0xA381)] [InlineData(0xFD, 0xA381)]
    [InlineData(0xDD, 0x8BBE)] [InlineData(0xFD, 0x8BBE)]
    [InlineData(0xDD, 0xFFFF)] [InlineData(0xFD, 0xFFFF)]
    public void BitIndexed_F5F3FromAddressHigh(byte prefix, ushort address)
    {
        var (cpu, state) = Create(prefix, 0xCB, 0xFF, 0x7E);
        var index = unchecked((ushort)(address + 1));
        if (prefix == 0xDD) cpu.Registers.IX = index; else cpu.Registers.IY = index;
        state.Memory[address] = 0x80;
        var expected = AfterFetch(cpu.Registers, 4);
        expected.WZ = address;
        expected.F = expected.Q = Reference(7, 0x80, expected.F, (byte)(address >> 8));
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(20, state.Cycles);
        Assert.Equal(new (string, ushort, int)[] {
            ("M1", 0x8000, prefix), ("M1", 0x8001, 0xCB),
            ("Read", 0x8002, 0xFF), ("Read", 0x8003, 0x7E), ("Internal", 0x8003, 2),
            ("Read", address, 0x80), ("Internal", address, 1),
        }, state.Accesses);
        Assert.Equal(0x80, state.Memory[address]);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void BitIndexed_AllAliases(int alias)
    {
        foreach (var prefix in new byte[] { 0xDD, 0xFD })
        for (var bit = 0; bit < 8; bit++)
        {
            var (cpu, state) = Create(prefix, 0xCB, 1, (byte)(0x40 + bit * 8 + alias));
            cpu.Registers.IX = cpu.Registers.IY = 0x27FF;
            state.Memory[0x2800] = 0x81;
            var expected = AfterFetch(cpu.Registers, 4);
            expected.WZ = 0x2800;
            expected.F = expected.Q = Reference(bit, 0x81, expected.F, 0x28);
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(20, state.Cycles);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SetResRegister_AllBits(bool set)
    {
        for (var bit = 0; bit < 8; bit++)
        foreach (var register in new[] { 0, 1, 2, 3, 4, 5, 7 })
        foreach (var value in new byte[] { 0, 0xFF, 0xA5 })
        {
            var (cpu, state) = Create(0xCB, (byte)((set ? 0xC0 : 0x80) + bit * 8 + register));
            SetRegister(ref cpu.Registers, register, value);
            cpu.Registers.Q = 0xAA;
            var expected = AfterFetch(cpu.Registers);
            expected.Q = 0;
            SetRegister(ref expected, register, (byte)(set ? value | (1 << bit) : value & ~(1 << bit)));
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(8, state.Cycles);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0x86, 0xFF, 0xFE)] [InlineData(0xFE, 0, 0x80)]
    public void SetResHl_ReadInternalWrite(byte opcode, byte value, byte result)
    {
        var (cpu, state) = Create(0xCB, opcode);
        state.Memory[cpu.Registers.HL] = value;
        var expected = AfterFetch(cpu.Registers);
        expected.Q = 0;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(result, state.Memory[expected.HL]);
        Assert.Equal(15, state.Cycles);
        Assert.Equal(new (string, ushort, int)[] {
            ("M1", 0x8000, 0xCB), ("M1", 0x8001, opcode), ("Read", expected.HL, value),
            ("Internal", expected.HL, 1), ("Write", expected.HL, result),
        }, state.Accesses);
    }

    [Theory]
    [InlineData(0xDD, 0x86, 0xFF, 0xFE)] [InlineData(0xFD, 0xFE, 0, 0x80)]
    public void SetResIndexed_ReadInternalWrite(byte prefix, byte opcode, byte value, byte result)
    {
        var (cpu, state) = Create(prefix, 0xCB, 0x80, opcode);
        cpu.Registers.IX = cpu.Registers.IY = 0x807F;
        state.Memory[0x7FFF] = value;
        var expected = AfterFetch(cpu.Registers, 4);
        expected.WZ = 0x7FFF;
        expected.Q = 0;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(result, state.Memory[0x7FFF]);
        Assert.Equal(23, state.Cycles);
        Assert.Equal(new (string, ushort, int)[] {
            ("M1", 0x8000, prefix), ("M1", 0x8001, 0xCB), ("Read", 0x8002, 0x80),
            ("Read", 0x8003, opcode), ("Internal", 0x8003, 2), ("Read", 0x7FFF, value),
            ("Internal", 0x7FFF, 1), ("Write", 0x7FFF, result),
        }, state.Accesses);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SetResIndexedCopy_AllRegisters(bool set)
    {
        foreach (var prefix in new byte[] { 0xDD, 0xFD })
        for (var bit = 0; bit < 8; bit++)
        foreach (var register in new[] { 0, 1, 2, 3, 4, 5, 7 })
        {
            var (cpu, state) = Create(prefix, 0xCB, 0xFF,
                (byte)((set ? 0xC0 : 0x80) + bit * 8 + register));
            cpu.Registers.IX = cpu.Registers.IY = 0x2801;
            var value = (byte)(set ? 0 : 0xFF);
            var result = (byte)(set ? 1 << bit : 0xFF ^ (1 << bit));
            state.Memory[0x2800] = value;
            var expected = AfterFetch(cpu.Registers, 4);
            expected.WZ = 0x2800;
            expected.Q = 0;
            SetRegister(ref expected, register, result);
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(result, state.Memory[0x2800]);
            Assert.Equal(23, state.Cycles);
            Assert.Equal(("Write", (ushort)0x2800, (int)result), state.Accesses[^1]);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0x40, true)] [InlineData(0x46, true)]
    [InlineData(0x80, false)] [InlineData(0xC6, false)]
    public void Bit_SetsQ_SetResClearQ(byte opcode, bool writesFlags)
    {
        var (cpu, _) = Create(0xCB, opcode);
        cpu.Registers.Q = 0xAA;
        cpu.Step();
        Assert.Equal(writesFlags ? cpu.Registers.F : (byte)0, cpu.Registers.Q);
    }

    public static IEnumerable<object[]> Programs()
    {
        yield return [new byte[] { 0xCB, 0x40 }];
        yield return [new byte[] { 0xCB, 0x46 }];
        yield return [new byte[] { 0xCB, 0x80 }];
        yield return [new byte[] { 0xCB, 0xC6 }];
        yield return [new byte[] { 0xDD, 0xCB, 0xFF, 0x46 }];
        yield return [new byte[] { 0xFD, 0xCB, 0xFF, 0x86 }];
        yield return [new byte[] { 0xDD, 0xCB, 0xFF, 0xC4 }];
        yield return [new byte[] { 0xFD, 0xCB, 0xFF, 0x85 }];
    }

    [Theory]
    [MemberData(nameof(Programs))]
    public void Bits_DoNotCountAsUnimplemented(byte[] program)
    {
        var (cpu, _) = Create(program);
        cpu.Step();
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [MemberData(nameof(Programs))]
    public void Bits_PcAndRefreshWrap(byte[] program)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFF;
        for (var i = 0; i < program.Length; i++) state.Memory[(ushort)(0xFFFF + i)] = program[i];
        cpu.Step();
        Assert.Equal((ushort)(program.Length - 1), cpu.Registers.PC);
        Assert.Equal((byte)0x81, cpu.Registers.R);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [MemberData(nameof(Programs))]
    public void IntAndNmi_AreAcceptedAfterEachPattern(byte[] program)
    {
        foreach (var nmi in new[] { false, true })
        {
            var (cpu, state) = Create(program);
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
            if (nmi) cpu.RequestNmi(); else state.IntActive = true;
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(before + (nmi ? 11 : 13), state.Cycles);
            Assert.Equal((byte)returnPc, state.Memory[expected.SP]);
            Assert.Equal((byte)(returnPc >> 8), state.Memory[(ushort)(expected.SP + 1)]);
        }
    }
}
