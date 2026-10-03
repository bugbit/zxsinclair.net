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



using ZXSinclair.Net.Core.Machines.Spectrum;
using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Core.Tests.Z80.Instructions;

public class IoTests
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
            I = 0x42, R = 0xFF, IFF1 = true, IFF2 = true, IM = 1, Q = 0xFF,
        };
        return (cpu, state);
    }

    private static bool Even(int value)
    {
        var ones = 0;
        for (var bit = 0; bit < 8; bit++) ones += (value >> bit) & 1;
        return ones % 2 == 0;
    }

    private static byte InFlags(int value, byte flags) =>
        (byte)((flags & 1) | (value & 0xA8) | (value == 0 ? 0x40 : 0) | (Even(value) ? 4 : 0));

    // Independent arithmetic model: repeat parity is parity of the combined operand.
    private static byte BlockFlags(bool input, int step, int value, int b, int c, int newLow, int? repeatPc = null)
    {
        var sum = value + (input ? (c + step) & 255 : newLow);
        var carry = sum >= 256;
        var negative = value >= 128;
        var half = carry;
        var parityOperand = (sum & 7) ^ b;
        var undocumented = b & 0x28;
        if (repeatPc is int pc)
        {
            undocumented = (pc >> 8) & 0x28;
            var correction = carry ? (negative ? b - 1 : b + 1) & 7 : b & 7;
            parityOperand ^= correction;
            if (carry) half = (b % 16) == (negative ? 0 : 15);
        }
        return (byte)((b & 0x80) | (b == 0 ? 0x40 : 0) | undocumented
            | (negative ? 2 : 0) | (carry ? 1 : 0) | (half ? 0x10 : 0) | (Even(parityOperand) ? 4 : 0));
    }

    private static void SetRegister(ref Z80Registers r, int code, byte value)
    {
        switch (code)
        {
            case 0: r.B = value; break;
            case 1: r.C = value; break;
            case 2: r.D = value; break;
            case 3: r.E = value; break;
            case 4: r.H = value; break;
            case 5: r.L = value; break;
            case 7: r.A = value; break;
        }
    }

    private static byte Register(Z80Registers r, int code) => code switch
    {
        0 => r.B, 1 => r.C, 2 => r.D, 3 => r.E, 4 => r.H, 5 => r.L, 7 => r.A, _ => 0,
    };

    [Theory]
    [InlineData(0xDB)]
    [InlineData(0xD3)]
    public void Accumulator_PortWzFlagsPrefixesAndPcWrap(int opcode)
    {
        foreach (var prefix in new[] { 0, 0xDD, 0xFD })
        foreach (var start in new[] { 0x8000, 0xFFFF })
        foreach (var a in new[] { 0x12, 0xFF })
        foreach (var low in new[] { 0x34, 0xFF })
        {
            var (cpu, state) = Create();
            var program = prefix == 0 ? new byte[] { (byte)opcode, (byte)low }
                : new byte[] { (byte)prefix, (byte)opcode, (byte)low };
            for (var i = 0; i < program.Length; i++) state.Memory[(ushort)(start + i)] = program[i];
            cpu.Registers.PC = (ushort)start;
            cpu.Registers.A = (byte)a;
            var expected = cpu.Registers;
            var port = (ushort)((a << 8) | low);
            expected.PC = (ushort)(start + program.Length);
            expected.R = (byte)(prefix == 0 ? 0x80 : 0x81);
            expected.Q = 0;
            expected.WZ = opcode == 0xDB ? (ushort)(port + 1) : (ushort)((a << 8) | ((low + 1) & 255));
            state.InputData.Enqueue(0x5A);
            if (opcode == 0xDB) expected.A = 0x5A;
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(prefix == 0 ? 11 : 15, state.Cycles);
            Assert.Equal((opcode == 0xDB ? "In" : "Out", port, opcode == 0xDB ? 0x5A : a), state.Accesses[^1]);
            Assert.Equal(("Read", (ushort)(start + program.Length - 1), low), state.Accesses[^2]);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
            state.IntActive = true;
            cpu.Step();
            Assert.Equal(0x38, cpu.Registers.PC);
            Assert.Equal(expected.PC, state.Memory[0x8FFE] | state.Memory[0x8FFF] << 8);
            Assert.Equal(prefix == 0 ? 0x81 : 0x82, cpu.Registers.R);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void InRegister_AllRegistersAndFlags(int register)
    {
        foreach (var bc in new[] { 0x1234, 0xFFFF })
        foreach (var prefix in new[] { 0, 0xDD, 0xFD })
        {
            var opcode = (byte)(0x40 + register * 8);
            var (cpu, state) = Create(prefix == 0 ? [0xED, opcode] : [(byte)prefix, 0xED, opcode]);
            cpu.Registers.BC = (ushort)bc;
            var baseline = cpu.Registers;
            for (var value = 0; value < 256; value++)
            foreach (var carry in new byte[] { 0, 1 })
            {
                cpu.Registers = baseline;
                cpu.Registers.F = (byte)(0xFE | carry);
                var expected = cpu.Registers;
                expected.PC += (ushort)(prefix == 0 ? 2 : 3);
                expected.R = (byte)(prefix == 0 ? 0x81 : 0x82);
                expected.WZ = (ushort)(bc + 1);
                SetRegister(ref expected, register, (byte)value); // code 6 discards input
                expected.F = expected.Q = InFlags(value, carry);
                state.Cycles = 0;
                state.Accesses.Clear();
                state.InputData.Enqueue((byte)value);
                cpu.Step();
                Assert.Equal(expected, cpu.Registers);
                Assert.Equal(prefix == 0 ? 12 : 16, state.Cycles);
                Assert.Equal(("In", (ushort)bc, value), state.Accesses[^1]);
            }
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void OutRegister_AllRegistersAndZero(int register)
    {
        foreach (var prefix in new[] { 0, 0xDD, 0xFD })
        {
            var opcode = (byte)(0x41 + register * 8);
            var (cpu, state) = Create(prefix == 0 ? [0xED, opcode] : [(byte)prefix, 0xED, opcode]);
            cpu.Registers.BC = 0xFFFF;
            var expected = cpu.Registers;
            var value = Register(expected, register);
            expected.PC += (ushort)(prefix == 0 ? 2 : 3);
            expected.R = (byte)(prefix == 0 ? 0x81 : 0x82);
            expected.WZ = 0;
            expected.Q = 0;
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(prefix == 0 ? 12 : 16, state.Cycles);
            Assert.Equal(("Out", (ushort)0xFFFF, (int)value), state.Accesses[^1]);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0xA2)]
    [InlineData(0xAA)]
    [InlineData(0xA3)]
    [InlineData(0xAB)]
    public void BlockIo_FlagsMatchReference(int opcode)
    {
        var (cpu, state) = Create(0xED, (byte)opcode);
        var baseline = cpu.Registers;
        var input = (opcode & 1) == 0;
        var step = (opcode & 8) == 0 ? 1 : -1;
        foreach (var b in new[] { 0, 1, 2, 16, 17, 127, 128, 255 })
        for (var value = 0; value < 256; value++)
        for (var low = 0; low < 256; low++)
        {
            cpu.Registers = baseline;
            cpu.Registers.B = (byte)b;
            cpu.Registers.C = (byte)low;
            cpu.Registers.HL = (ushort)(0x9000 | low);
            if (input) state.InputData.Enqueue((byte)value);
            else state.Memory[cpu.Registers.HL] = (byte)value;
            state.Accesses.Clear();
            state.Cycles = 0;
            cpu.Step();
            Assert.Equal(BlockFlags(input, step, value, (b - 1) & 255, low, (low + step) & 255), cpu.Registers.F);
            Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xA2)]
    [InlineData(0xAA)]
    [InlineData(0xA3)]
    [InlineData(0xAB)]
    [InlineData(0xB2)]
    [InlineData(0xBA)]
    [InlineData(0xB3)]
    [InlineData(0xBB)]
    public void BlockIo_AccessesWzWrapAndPrefixes(int opcode)
    {
        foreach (var prefix in new[] { 0, 0xDD, 0xFD })
        foreach (var b in new[] { 0, 1, 2 })
        {
            var (cpu, state) = Create(prefix == 0 ? [0xED, (byte)opcode] : [(byte)prefix, 0xED, (byte)opcode]);
            var step = (opcode & 8) == 0 ? 1 : -1;
            var input = (opcode & 1) == 0;
            cpu.Registers.B = (byte)b;
            cpu.Registers.C = (byte)(step == 1 ? 255 : 0);
            cpu.Registers.HL = (ushort)(step == 1 ? 0xFFFF : 0);
            var expected = cpu.Registers;
            var oldHl = expected.HL;
            var oldBc = expected.BC;
            var length = prefix == 0 ? 2 : 3;
            expected.PC += (ushort)length;
            expected.R = (byte)(prefix == 0 ? 0x81 : 0x82);
            expected.B--;
            expected.HL = (ushort)(expected.HL + step);
            expected.WZ = (ushort)((input ? oldBc : expected.BC) + step);
            var repeat = (opcode & 0x10) != 0 && expected.B != 0;
            if (repeat) expected.PC -= 2;
            expected.F = expected.Q = BlockFlags(input, step, 0x9A, expected.B, expected.C, expected.L,
                repeat ? expected.PC : null);
            var accesses = new List<(string, ushort, int)> { ("Internal", expected.IR, 1) };
            if (input)
            {
                state.InputData.Enqueue(0x9A);
                accesses.Add(("In", oldBc, 0x9A));
                accesses.Add(("Write", oldHl, 0x9A));
            }
            else
            {
                state.Memory[oldHl] = 0x9A;
                accesses.Add(("Read", oldHl, 0x9A));
                accesses.Add(("Out", expected.BC, 0x9A));
            }
            if (repeat) accesses.Add(("Internal", input ? oldHl : expected.BC, 5));
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal(accesses, state.Accesses.Skip(length));
            Assert.Equal((repeat ? 21 : 16) + (prefix == 0 ? 0 : 4), state.Cycles);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0xB2)]
    [InlineData(0xBA)]
    [InlineData(0xB3)]
    [InlineData(0xBB)]
    public void BlockIoRepeat_F5F3HAndParityAdjust(int opcode)
    {
        var input = (opcode & 1) == 0;
        var step = (opcode & 8) == 0 ? 1 : -1;
        var (cpu, state) = Create();
        var baseline = cpu.Registers;
        foreach (var pc in new[] { 0, 0x0800, 0x2000, 0x2800 })
        foreach (var b in new[] { 0, 1, 2, 3, 8, 15, 16, 17, 31, 32, 127, 128, 255 })
        foreach (var value in new[] { 0, 1, 2, 127, 128, 129, 254, 255 })
        for (var low = 0; low < 256; low++)
        {
            cpu.Registers = baseline;
            cpu.Registers.PC = (ushort)pc;
            cpu.Registers.B = (byte)b;
            cpu.Registers.C = (byte)low;
            cpu.Registers.HL = (ushort)(0x9000 | low);
            state.Memory[pc] = 0xED;
            state.Memory[pc + 1] = (byte)opcode;
            if (input) state.InputData.Enqueue((byte)value);
            else state.Memory[cpu.Registers.HL] = (byte)value;
            state.Accesses.Clear();
            state.Cycles = 0;
            cpu.Step();
            var nextB = (b - 1) & 255;
            Assert.Equal(BlockFlags(input, step, value, nextB, low, (low + step) & 255,
                nextB != 0 ? pc : null), cpu.Registers.F);
            Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
            Assert.Equal(nextB != 0 ? pc : pc + 2, cpu.Registers.PC);
            Assert.Equal(nextB != 0 ? 21 : 16, state.Cycles);
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xB2)]
    [InlineData(0xBA)]
    [InlineData(0xB3)]
    [InlineData(0xBB)]
    public void RepeatUntilBZeroAndInterruptResume(int opcode)
    {
        var (cpu, state) = Create(0xED, (byte)opcode);
        var input = (opcode & 1) == 0;
        var step = (opcode & 8) == 0 ? 1 : -1;
        cpu.Registers.B = 3;
        for (var i = 0; i < 3; i++)
        {
            if (input) state.InputData.Enqueue((byte)(0x20 + i));
            else state.Memory[(ushort)(0x9ABC + step * i)] = (byte)(0x20 + i);
        }
        state.Memory[0x38] = 0xF5; // PUSH AF
        state.Memory[0x39] = 0xF1; // POP AF
        state.Memory[0x3A] = 0xED;
        state.Memory[0x3B] = 0x4D; // RETI
        cpu.Step();
        Assert.Equal(21, state.Cycles);
        var af = cpu.Registers.AF;
        state.IntActive = true;
        cpu.Step();
        Assert.Equal(0, cpu.Registers.Q);
        Assert.Equal(0x8000, state.Memory[0x8FFE] | state.Memory[0x8FFF] << 8);
        state.IntActive = false;
        cpu.Step();
        Assert.Equal((byte)af, state.Memory[0x8FFC]);
        cpu.Step();
        cpu.Step();
        Assert.Equal(0x8000, cpu.Registers.PC);
        for (var i = 1; i < 3; i++)
        {
            var cycles = state.Cycles;
            cpu.Step();
            Assert.Equal(i == 1 ? 21 : 16, state.Cycles - cycles);
            Assert.Equal(i == 1 ? 0x88 : 0x8A, cpu.Registers.R);
        }
        Assert.Equal(0, cpu.Registers.B);
        Assert.Equal(0x8002, cpu.Registers.PC);
        Assert.Equal((ushort)(0x9ABC + 3 * step), cpu.Registers.HL);
        if (input) for (var i = 0; i < 3; i++)
            Assert.Equal(0x20 + i, state.Memory[(ushort)(0x9ABC + step * i)]);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xB2)]
    [InlineData(0xBA)]
    [InlineData(0xB3)]
    [InlineData(0xBB)]
    public void Repeat_FlagsSeenByInterruptAt2800(int opcode)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0x2800;
        cpu.Registers.B = 17;
        cpu.Registers.C = 0x80;
        cpu.Registers.L = 0x80;
        state.Memory[0x2800] = 0xED;
        state.Memory[0x2801] = (byte)opcode;
        state.Memory[cpu.Registers.HL] = 0xF0;
        state.InputData.Enqueue(0xF0);
        state.Memory[0x38] = 0xF5;
        cpu.Step();
        var expected = BlockFlags((opcode & 1) == 0, (opcode & 8) == 0 ? 1 : -1,
            0xF0, 16, 0x80, cpu.Registers.L, 0x2800);
        Assert.Equal(expected, cpu.Registers.F);
        state.IntActive = true;
        cpu.Step();
        Assert.Equal(0, cpu.Registers.Q);
        state.IntActive = false;
        cpu.Step();
        Assert.Equal(expected, state.Memory[0x8FFC]);
    }

    [Theory]
    [InlineData(0xDB)]
    [InlineData(0xD3)]
    public void Io_SpectrumBusContention(int opcode)
    {
        foreach (var high in new[] { 0, 0x40 })
        {
            var machine = new SpectrumMachine(SpectrumModel.Spectrum48K);
            machine.Memory.Write(0x8000, (byte)opcode);
            machine.Memory.Write(0x8001, 0xFE);
            // Fetch + immediate operand take 7 uncontended T-states.
            machine.TStates = 14328;
            var cpu = new Z80Cpu<SpectrumBus>(machine.Bus);
            cpu.Registers.PC = 0x8000;
            cpu.Registers.A = (byte)high;
            cpu.Registers.F = 0xA5;
            cpu.Step();
            Assert.Equal(high == 0 ? 14344 : 14345, machine.TStates);
            Assert.Equal(0xA5, cpu.Registers.F);
            Assert.Equal(high == 0 ? 0x00FF : 0x40FF, cpu.Registers.WZ);
            if (opcode == 0xDB) Assert.Equal(0xBF, cpu.Registers.A);
            else Assert.Equal(high & 7, machine.Border);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0x40)]
    [InlineData(0x70)]
    [InlineData(0x41)]
    [InlineData(0x71)]
    [InlineData(0xA2)]
    [InlineData(0xAA)]
    [InlineData(0xA3)]
    [InlineData(0xAB)]
    [InlineData(0xB2)]
    [InlineData(0xBA)]
    [InlineData(0xB3)]
    [InlineData(0xBB)]
    public void EdIo_PcAndRefreshWrapInterruptAfterwards(int opcode)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFF;
        cpu.Registers.B = 1;
        state.Memory[0xFFFF] = 0xED;
        state.Memory[0] = (byte)opcode;
        cpu.Step();
        Assert.Equal(1, cpu.Registers.PC);
        Assert.Equal(0x81, cpu.Registers.R);
        state.IntActive = true;
        cpu.Step();
        Assert.Equal(0x38, cpu.Registers.PC);
        Assert.Equal(1, state.Memory[0x8FFE]);
        Assert.Equal(0, state.Memory[0x8FFF]);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }
}
