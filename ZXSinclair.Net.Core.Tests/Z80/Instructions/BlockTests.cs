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

public class BlockTests
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

    // Independent arithmetic model; no Core flag tables.
    private static byte Reference(bool compare, int a, int value, int count, byte flags)
    {
        if (!compare)
        {
            var sum = a + value;
            return (byte)((flags & 0xC1) | (count != 0 ? 4 : 0)
                | (sum & 8) | ((sum & 2) != 0 ? 0x20 : 0));
        }
        var result = (a - value) & 255;
        var borrow = (a & 15) < (value & 15);
        var adjusted = (result - (borrow ? 1 : 0)) & 255;
        return (byte)((flags & 1) | 2 | (result & 0x80) | (result == 0 ? 0x40 : 0)
            | (borrow ? 0x10 : 0) | (count != 0 ? 4 : 0) | (adjusted & 8)
            | ((adjusted & 2) != 0 ? 0x20 : 0));
    }

    [Theory]
    [InlineData(0xEB, 0)]
    [InlineData(0x08, 0)]
    [InlineData(0xD9, 0)]
    [InlineData(0xEB, 0xDD)]
    [InlineData(0xEB, 0xFD)]
    [InlineData(0x08, 0xDD)]
    [InlineData(0x08, 0xFD)]
    [InlineData(0xD9, 0xDD)]
    [InlineData(0xD9, 0xFD)]
    public void ExDeHl_ExAf_Exx(int opcode, int prefix)
    {
        var (cpu, state) = Create(prefix == 0 ? [(byte)opcode] : [(byte)prefix, (byte)opcode]);
        var expected = cpu.Registers;
        if (opcode == 0xEB) (expected.DE, expected.HL) = (expected.HL, expected.DE);
        else if (opcode == 0x08) (expected.AF, expected.AF_) = (expected.AF_, expected.AF);
        else
        {
            (expected.BC, expected.BC_) = (expected.BC_, expected.BC);
            (expected.DE, expected.DE_) = (expected.DE_, expected.DE);
            (expected.HL, expected.HL_) = (expected.HL_, expected.HL);
        }
        var length = prefix == 0 ? 1 : 2;
        expected.PC += (ushort)length;
        expected.R = (byte)(0x80 | ((0x7F + length) & 0x7F));
        expected.Q = 0;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(length * 4, state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0, 0x9000)]
    [InlineData(0xDD, 0x9000)]
    [InlineData(0xFD, 0x9000)]
    [InlineData(0, 0xFFFF)]
    [InlineData(0xDD, 0xFFFF)]
    [InlineData(0xFD, 0xFFFF)]
    public void ExStack_ReadsThenWritesHighFirst(int prefix, int sp)
    {
        var (cpu, state) = Create(prefix == 0 ? [0xE3] : [(byte)prefix, 0xE3]);
        cpu.Registers.SP = (ushort)sp;
        var expected = cpu.Registers;
        var old = prefix == 0 ? expected.HL : prefix == 0xDD ? expected.IX : expected.IY;
        var high = (ushort)(sp + 1);
        state.Memory[sp] = 0xCD;
        state.Memory[high] = 0xAB;
        var length = prefix == 0 ? 1 : 2;
        expected.PC += (ushort)length;
        expected.R = (byte)(0x80 | ((0x7F + length) & 0x7F));
        expected.Q = 0;
        expected.WZ = 0xABCD;
        if (prefix == 0) expected.HL = 0xABCD;
        else if (prefix == 0xDD) expected.IX = 0xABCD;
        else expected.IY = 0xABCD;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(prefix == 0 ? 19 : 23, state.Cycles);
        Assert.Equal(new (string, ushort, int)[]
        {
            ("Read", (ushort)sp, 0xCD), ("Read", high, 0xAB), ("Internal", high, 1),
            ("Write", high, old >> 8), ("Write", (ushort)sp, old & 255), ("Internal", (ushort)sp, 2),
        }, state.Accesses.Skip(length));
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xA0)]
    [InlineData(0xA8)]
    [InlineData(0xA1)]
    [InlineData(0xA9)]
    public void FlagsMatchReferenceExhaustively(int opcode)
    {
        var (cpu, state) = Create(0xED, (byte)opcode);
        var baseline = cpu.Registers;
        for (var a = 0; a < 256; a++)
        for (var value = 0; value < 256; value++)
        for (var count = 1; count <= 2; count++)
        for (var preserved = 0; preserved < 8; preserved++)
        {
            cpu.Registers = baseline;
            cpu.Registers.A = (byte)a;
            var flags = (byte)(0x3E | ((preserved & 1) != 0 ? 1 : 0)
                | ((preserved & 2) != 0 ? 0x40 : 0) | ((preserved & 4) != 0 ? 0x80 : 0));
            cpu.Registers.F = flags;
            cpu.Registers.BC = (ushort)count;
            state.Memory[baseline.HL] = (byte)value;
            state.Accesses.Clear();
            state.Cycles = 0;
            cpu.Step();
            Assert.Equal(Reference((opcode & 1) != 0, a, value, count - 1, flags), cpu.Registers.F);
            Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
        }
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xA0)]
    [InlineData(0xA8)]
    [InlineData(0xA1)]
    [InlineData(0xA9)]
    [InlineData(0xB0)]
    [InlineData(0xB8)]
    [InlineData(0xB1)]
    [InlineData(0xB9)]
    public void Block_AccessesAndRegisters(int opcode)
    {
        foreach (var count in new[] { 1, 2 })
        foreach (var prefix in new[] { 0, 0xDD, 0xFD })
        {
            var (cpu, state) = Create(prefix == 0 ? [0xED, (byte)opcode] : [(byte)prefix, 0xED, (byte)opcode]);
            cpu.Registers.BC = (ushort)count;
            var expected = cpu.Registers;
            var hl = expected.HL;
            var de = expected.DE;
            state.Memory[hl] = 0x10;
            var compare = (opcode & 1) != 0;
            var step = (opcode & 8) == 0 ? 1 : -1;
            var repeat = (opcode & 0x10) != 0 && count != 1;
            var length = prefix == 0 ? 2 : 3;
            expected.PC += (ushort)length;
            expected.R = (byte)(0x80 | ((0x7F + length) & 0x7F));
            expected.HL = (ushort)(hl + step);
            expected.BC--;
            if (compare) expected.WZ = (ushort)(expected.WZ + step);
            else expected.DE = (ushort)(de + step);
            expected.F = Reference(compare, expected.A, 0x10, count - 1, expected.F);
            if (repeat)
            {
                expected.PC -= 2;
                expected.WZ = (ushort)(expected.PC + 1);
                expected.F = (byte)((expected.F & ~0x28) | ((expected.PC >> 8) & 0x28));
            }
            expected.Q = expected.F;
            var accesses = new List<(string, ushort, int)> { ("Read", hl, 0x10) };
            if (compare) accesses.Add(("Internal", hl, 5));
            else
            {
                accesses.Add(("Write", de, 0x10));
                accesses.Add(("Internal", de, 2));
            }
            if (repeat) accesses.Add(("Internal", compare ? hl : de, 5));
            cpu.Step();
            Assert.Equal(expected, cpu.Registers);
            Assert.Equal((repeat ? 21 : 16) + (prefix == 0 ? 0 : 4), state.Cycles);
            Assert.Equal(accesses, state.Accesses.Skip(length));
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0xB0)]
    [InlineData(0xB8)]
    [InlineData(0xB1)]
    [InlineData(0xB9)]
    public void RepeatF5F3_FromPcHigh(int opcode)
    {
        foreach (var pc in new[] { 0, 0x0800, 0x2000, 0x2800 })
        {
            var (cpu, state) = Create();
            cpu.Registers.PC = (ushort)pc;
            cpu.Registers.BC = 2;
            cpu.Registers.A = 0;
            state.Memory[pc] = 0xED;
            state.Memory[pc + 1] = (byte)opcode;
            var compare = (opcode & 1) != 0;
            // Find a byte whose classic F5/F3 are the complement of PC's.
            var value = Enumerable.Range(1, 255).First(v =>
                (Reference(compare, 0, v, 1, 0xFF) & 0x28) == ((~(pc >> 8)) & 0x28));
            state.Memory[cpu.Registers.HL] = (byte)value;
            var classic = Reference(compare, 0, value, 1, cpu.Registers.F);
            cpu.Step();
            Assert.Equal((byte)((classic & ~0x28) | ((pc >> 8) & 0x28)), cpu.Registers.F);
            Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
            Assert.Equal((ushort)pc, cpu.Registers.PC);
            Assert.Equal(21, state.Cycles);
        }
    }

    [Theory]
    [InlineData(0xB0)]
    [InlineData(0xB8)]
    [InlineData(0xB1)]
    [InlineData(0xB9)]
    public void RepeatFinalIteration_KeepsClassicFlags(int opcode)
    {
        foreach (var pc in new[] { 0, 0x0800, 0x2000, 0x2800 })
        foreach (var match in (opcode & 1) != 0 ? new[] { false, true } : new[] { false })
        {
            var (cpu, state) = Create();
            cpu.Registers.PC = (ushort)pc;
            cpu.Registers.BC = (ushort)(match ? 2 : 1);
            cpu.Registers.A = 0;
            state.Memory[pc] = 0xED;
            state.Memory[pc + 1] = (byte)opcode;
            state.Memory[cpu.Registers.HL] = (byte)(match ? 0 : 10);
            var expected = Reference((opcode & 1) != 0, 0, match ? 0 : 10, match ? 1 : 0, cpu.Registers.F);
            cpu.Step();
            Assert.Equal(expected, cpu.Registers.F);
            Assert.Equal(expected, cpu.Registers.Q);
            Assert.Equal((ushort)(pc + 2), cpu.Registers.PC);
            Assert.Equal(16, state.Cycles);
        }
    }

    [Theory]
    [InlineData(0xB0)]
    [InlineData(0xB8)]
    [InlineData(0xB1)]
    [InlineData(0xB9)]
    public void RepeatFlags_SeenByInterrupt(int opcode)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0x2800;
        cpu.Registers.BC = 3;
        state.Memory[0x2800] = 0xED;
        state.Memory[0x2801] = (byte)opcode;
        state.Memory[cpu.Registers.HL] = 0x10;
        state.Memory[0x38] = 0xF5; // PUSH AF
        cpu.Step();
        var af = cpu.Registers.AF;
        Assert.Equal(0x28, af & 0x28);
        state.IntActive = true;
        cpu.Step();
        Assert.Equal(0x38, cpu.Registers.PC);
        Assert.Equal(0, cpu.Registers.Q);
        Assert.Equal(0x00, state.Memory[0x8FFE]);
        Assert.Equal(0x28, state.Memory[0x8FFF]);
        state.IntActive = false;
        cpu.Step();
        Assert.Equal((byte)af, state.Memory[0x8FFC]);
        Assert.Equal((byte)(af >> 8), state.Memory[0x8FFD]);
    }

    [Fact]
    public void Ldir_InterruptBetweenIterations()
    {
        var (cpu, state) = Create(0xED, 0xB0);
        cpu.Registers.BC = 3;
        state.Memory[0x38] = 0xED;
        state.Memory[0x39] = 0x4D; // RETI
        state.Memory[0x9ABC] = 1;
        state.Memory[0x9ABD] = 2;
        state.Memory[0x9ABE] = 3;
        cpu.Step();
        state.IntActive = true;
        cpu.Step();
        Assert.Equal(0x8000, state.Memory[0x8FFE] | state.Memory[0x8FFF] << 8);
        Assert.Equal(2, cpu.Registers.BC);
        state.IntActive = false;
        cpu.Step();
        Assert.Equal(0x8000, cpu.Registers.PC);
        cpu.Step();
        cpu.Step();
        Assert.Equal(0, cpu.Registers.BC);
        Assert.Equal(new byte[] { 1, 2, 3 }, state.Memory[0x5678..0x567B]);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xB0)]
    [InlineData(0xB8)]
    [InlineData(0xB1)]
    [InlineData(0xB9)]
    public void RepeatsUntilBcZeroOrMatch(int opcode)
    {
        foreach (var match in (opcode & 1) != 0 ? new[] { false, true } : new[] { false })
        {
            var (cpu, state) = Create(0xED, (byte)opcode);
            cpu.Registers.A = 3;
            cpu.Registers.BC = (ushort)(match ? 5 : 3);
            var step = (opcode & 8) == 0 ? 1 : -1;
            for (var i = 0; i < 3; i++) state.Memory[(ushort)(0x9ABC + i * step)] = (byte)(match ? i + 1 : 0x10);
            for (var i = 0; i < 3; i++)
            {
                var previous = state.Cycles;
                cpu.Step();
                Assert.Equal(i == 2 ? 16 : 21, state.Cycles - previous);
                Assert.Equal(i == 2 ? 0x8002 : 0x8000, cpu.Registers.PC);
                Assert.Equal(0x80 | (1 + 2 * i), cpu.Registers.R);
                Assert.Equal((ushort)(0x9ABC + (i + 1) * step), cpu.Registers.HL);
                Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
                if ((opcode & 1) == 0)
                    Assert.Equal(state.Memory[(ushort)(0x9ABC + i * step)], state.Memory[(ushort)(0x5678 + i * step)]);
                Assert.Equal((ushort)((i == 2 && (opcode & 1) != 0) ? 0x8001 + step : 0x8001), cpu.Registers.WZ);
            }
            Assert.Equal(match ? 2 : 0, cpu.Registers.BC);
            if ((opcode & 1) != 0) Assert.Equal(match, (cpu.Registers.F & 0x40) != 0);
            Assert.Equal(match, (cpu.Registers.F & 4) != 0);
            Assert.Equal(0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0xA0)]
    [InlineData(0xA8)]
    [InlineData(0xA1)]
    [InlineData(0xA9)]
    [InlineData(0xB0)]
    [InlineData(0xB8)]
    [InlineData(0xB1)]
    [InlineData(0xB9)]
    public void BlockWraps16Bits(int opcode)
    {
        var (cpu, state) = Create();
        var up = (opcode & 8) == 0;
        cpu.Registers.HL = cpu.Registers.DE = cpu.Registers.WZ = (ushort)(up ? 0xFFFF : 0);
        cpu.Registers.PC = 0x7FFF;
        cpu.Registers.BC = 0;
        cpu.Registers.A = 0x80;
        state.Memory[0x7FFF] = 0xED;
        state.Memory[0x8000] = (byte)opcode;
        state.Memory[cpu.Registers.HL] = 1;
        cpu.Step();
        Assert.Equal(up ? 0 : 0xFFFF, cpu.Registers.HL);
        if ((opcode & 1) == 0) Assert.Equal(up ? 0 : 0xFFFF, cpu.Registers.DE);
        Assert.Equal(0xFFFF, cpu.Registers.BC);
        Assert.Equal(4, cpu.Registers.F & 4);
        Assert.Equal((opcode & 0x10) != 0 ? 0x8000 : (opcode & 1) != 0 ? up ? 0 : 0xFFFF : up ? 0xFFFF : 0, cpu.Registers.WZ);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xA0)]
    [InlineData(0xA8)]
    [InlineData(0xA1)]
    [InlineData(0xA9)]
    [InlineData(0xB0)]
    [InlineData(0xB8)]
    [InlineData(0xB1)]
    [InlineData(0xB9)]
    public void PcWrapsAndInterruptAcceptedAfterwards(int opcode)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFF;
        cpu.Registers.BC = 1;
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

    [Fact]
    public void Ldir_ZeroInitialCountCopies65536Bytes()
    {
        var (cpu, state) = Create(0xED, 0xB0);
        cpu.Registers.HL = cpu.Registers.DE = cpu.Registers.BC = 0;
        // Identical source/destination preserves the program during the full address-space copy.
        for (var i = 0; i < 65536; i++)
        {
            state.Accesses.Clear();
            cpu.Step();
            if (i != 65535) Assert.Equal(0x8000, cpu.Registers.PC);
        }
        Assert.Equal(0, cpu.Registers.HL);
        Assert.Equal(0, cpu.Registers.DE);
        Assert.Equal(0, cpu.Registers.BC);
        Assert.Equal(0x8002, cpu.Registers.PC);
        Assert.Equal(0xFF, cpu.Registers.R);
        Assert.Equal(21 * 65535 + 16, state.Cycles);
        Assert.Equal(0, cpu.Registers.F & 4);
        Assert.Equal(cpu.Registers.F, cpu.Registers.Q);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xEB)]
    [InlineData(0x08)]
    [InlineData(0xD9)]
    [InlineData(0xE3)]
    public void Exchange_PcWrapsAndInterruptAcceptedAfterwards(int opcode)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFF;
        state.Memory[0xFFFF] = (byte)opcode;
        cpu.Step();
        Assert.Equal(0, cpu.Registers.PC);
        Assert.Equal(0x80, cpu.Registers.R);
        Assert.Equal(0, cpu.Registers.Q);
        state.IntActive = true;
        cpu.Step();
        Assert.Equal(0x38, cpu.Registers.PC);
        Assert.Equal(0, state.Memory[0x8FFE]);
        Assert.Equal(0, state.Memory[0x8FFF]);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }
}
