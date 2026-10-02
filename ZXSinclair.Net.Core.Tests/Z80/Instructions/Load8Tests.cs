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

public class Load8Tests
{
    private static readonly int[] RegisterIds = [0, 1, 2, 3, 4, 5, 7];

    public static IEnumerable<object[]> Registers() => RegisterIds.Select(i => new object[] { i });
    public static IEnumerable<object[]> RegisterPairs() =>
        from d in RegisterIds from s in RegisterIds select new object[] { d, s };
    public static IEnumerable<object[]> IndexedDisplacements() =>
        from prefix in new byte[] { 0xDD, 0xFD }
        from d in new int[] { 5, -1, -128, 127 }
        select new object[] { prefix, d };

    private static ref byte Reg(ref Z80Registers r, int id)
    {
        switch (id)
        {
            case 0: return ref r.B;
            case 1: return ref r.C;
            case 2: return ref r.D;
            case 3: return ref r.E;
            case 4: return ref r.H;
            case 5: return ref r.L;
            case 7: return ref r.A;
            default: throw new ArgumentOutOfRangeException(nameof(id));
        }
    }

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
    [MemberData(nameof(RegisterPairs))]
    public void LdRegReg_AllCombinations(int dest, int source)
    {
        var opcode = (byte)(0x40 | dest << 3 | source);
        var (cpu, state) = Create(opcode);
        var expected = AfterFetch(cpu, 1);
        Reg(ref expected, dest) = Reg(ref cpu.Registers, source);
        Check(cpu, state, expected, 4, ("M1", 0, opcode));
    }

    [Theory]
    [MemberData(nameof(Registers))]
    public void LdRegImm_AllRegisters(int dest)
    {
        var opcode = (byte)(0x06 | dest << 3);
        var (cpu, state) = Create(opcode, 0xA5);
        var expected = AfterFetch(cpu, 2);
        Reg(ref expected, dest) = 0xA5;
        Check(cpu, state, expected, 7, ("M1", 0, opcode), ("Read", 1, 0xA5));
    }

    [Theory]
    [MemberData(nameof(Registers))]
    public void LdRegHl_ReadsMemory(int dest)
    {
        var opcode = (byte)(0x46 | dest << 3);
        var (cpu, state) = Create(opcode);
        cpu.Registers.HL = 0x8000;
        state.Memory[0x8000] = 0x5A;
        var expected = AfterFetch(cpu, 1);
        Reg(ref expected, dest) = 0x5A;
        Check(cpu, state, expected, 7, ("M1", 0, opcode), ("Read", 0x8000, 0x5A));
    }

    [Theory]
    [MemberData(nameof(Registers))]
    public void LdHlReg_WritesMemory(int source)
    {
        var opcode = (byte)(0x70 | source);
        var (cpu, state) = Create(opcode);
        var address = cpu.Registers.HL;
        var value = Reg(ref cpu.Registers, source);
        Check(cpu, state, AfterFetch(cpu, 1), 7, ("M1", 0, opcode), ("Write", address, value));
        Assert.Equal(value, state.Memory[address]);
    }

    [Fact]
    public void LdHlN_ReadsThenWrites()
    {
        var (cpu, state) = Create(0x36, 0x5A);
        Check(cpu, state, AfterFetch(cpu, 2), 10,
            ("M1", 0, 0x36), ("Read", 1, 0x5A), ("Write", 0x9ABC, 0x5A));
        Assert.Equal(0x5A, state.Memory[0x9ABC]);
    }

    [Theory]
    [InlineData(0x0A, 0x1234)]
    [InlineData(0x1A, 0x5678)]
    [InlineData(0x0A, 0xFFFF)]
    [InlineData(0x1A, 0xFFFF)]
    public void LdAFromPair_SetsWz(byte opcode, ushort address)
    {
        var (cpu, state) = Create(opcode);
        if (opcode == 0x0A) cpu.Registers.BC = address;
        else cpu.Registers.DE = address;
        state.Memory[address] = 0x28;
        var expected = AfterFetch(cpu, 1);
        expected.A = 0x28;
        expected.WZ = (ushort)(address + 1);
        Check(cpu, state, expected, 7, ("M1", 0, opcode), ("Read", address, 0x28));
    }

    [Theory]
    [InlineData(0x02, 0x12FF)]
    [InlineData(0x12, 0x12FF)]
    [InlineData(0x02, 0xFFFF)]
    [InlineData(0x12, 0xFFFF)]
    public void LdPairFromA_SetsWz(byte opcode, ushort address)
    {
        var (cpu, state) = Create(opcode);
        if (opcode == 0x02) cpu.Registers.BC = address;
        else cpu.Registers.DE = address;
        var expected = AfterFetch(cpu, 1);
        expected.WZ = 0xA500;
        Check(cpu, state, expected, 7, ("M1", 0, opcode), ("Write", address, 0xA5));
        Assert.Equal(0xA5, state.Memory[address]);
    }

    [Theory]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void LdAAbsolute_ReadsAndSetsWz(ushort address)
    {
        var (cpu, state) = Create(0x3A, (byte)address, (byte)(address >> 8));
        state.Memory[address] = 0x28;
        var expected = AfterFetch(cpu, 3);
        expected.A = 0x28;
        expected.WZ = (ushort)(address + 1);
        Check(cpu, state, expected, 13, ("M1", 0, 0x3A),
            ("Read", 1, (byte)address), ("Read", 2, address >> 8), ("Read", address, 0x28));
    }

    [Fact]
    public void LdAbsoluteA_WritesAndSetsWz()
    {
        var (cpu, state) = Create(0x32, 0xFF, 0x12);
        var expected = AfterFetch(cpu, 3);
        expected.WZ = 0xA500;
        Check(cpu, state, expected, 13, ("M1", 0, 0x32), ("Read", 1, 0xFF),
            ("Read", 2, 0x12), ("Write", 0x12FF, 0xA5));
        Assert.Equal(0xA5, state.Memory[0x12FF]);
    }

    [Fact]
    public void LdAbsolute_OperandWrapsPc()
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFE;
        state.Memory[0xFFFE] = 0x3A;
        state.Memory[0xFFFF] = 0x34;
        state.Memory[0] = 0x12;
        state.Memory[0x1234] = 0x28;
        var expected = AfterFetch(cpu, 3);
        expected.A = 0x28;
        expected.WZ = 0x1235;
        Check(cpu, state, expected, 13, ("M1", 0xFFFE, 0x3A), ("Read", 0xFFFF, 0x34),
            ("Read", 0, 0x12), ("Read", 0x1234, 0x28));
    }

    [Theory]
    [InlineData(false, 0x00, 0x41)]
    [InlineData(true, 0x00, 0x45)]
    [InlineData(false, 0x80, 0x81)]
    [InlineData(true, 0x80, 0x85)]
    [InlineData(false, 0x28, 0x29)]
    [InlineData(true, 0x28, 0x2D)]
    [InlineData(false, 0x00, 0x40, 0)]
    [InlineData(true, 0x00, 0x44, 0)]
    [InlineData(false, 0x80, 0x80, 0)]
    [InlineData(true, 0x80, 0x84, 0)]
    [InlineData(false, 0x28, 0x28, 0)]
    [InlineData(true, 0x28, 0x2C, 0)]
    public void LdAI_SetsFlagsFromIff2(bool iff2, byte value, byte flags, int carry = 1)
    {
        var (cpu, state) = Create(0xED, 0x57);
        cpu.Registers.F = (byte)(0xFE | carry);
        cpu.Registers.I = value;
        cpu.Registers.IFF2 = iff2;
        var expected = AfterFetch(cpu, 2, 2);
        var ir = expected.IR;
        expected.A = value;
        expected.F = flags;
        expected.Q = flags;
        Check(cpu, state, expected, 9, ("M1", 0, 0xED), ("M1", 1, 0x57), ("Internal", ir, 1));
    }

    [Theory]
    [InlineData(0x7E, 0x00, 0x45)]
    [InlineData(0xFF, 0x81, 0x85)]
    public void LdAR_ReadsRefreshAfterFetches(byte initial, byte value, byte flags)
    {
        var (cpu, state) = Create(0xED, 0x5F);
        cpu.Registers.R = initial;
        var expected = AfterFetch(cpu, 2, 2);
        var ir = expected.IR;
        expected.A = value;
        expected.F = flags;
        expected.Q = flags;
        Check(cpu, state, expected, 9, ("M1", 0, 0xED), ("M1", 1, 0x5F), ("Internal", ir, 1));
    }

    [Fact]
    public void LdIA_SetsI()
    {
        var (cpu, state) = Create(0xED, 0x47);
        var expected = AfterFetch(cpu, 2, 2);
        var ir = expected.IR;
        expected.I = expected.A;
        Check(cpu, state, expected, 9, ("M1", 0, 0xED), ("M1", 1, 0x47), ("Internal", ir, 1));
    }

    [Fact]
    public void LdRA_SetsAllBits()
    {
        var (cpu, state) = Create(0xED, 0x4F);
        cpu.Registers.A = 0x80;
        var expected = AfterFetch(cpu, 2, 2);
        var ir = expected.IR;
        expected.R = 0x80;
        Check(cpu, state, expected, 9, ("M1", 0, 0xED), ("M1", 1, 0x4F), ("Internal", ir, 1));
    }

    [Theory]
    [MemberData(nameof(IndexedDisplacements))]
    public void LdRegIndexed_ReadsWithDisplacement(byte prefix, int displacement)
    {
        var (cpu, state) = Create(prefix, 0x46, (byte)displacement);
        var index = prefix == 0xDD ? cpu.Registers.IX : cpu.Registers.IY;
        var address = (ushort)(index + displacement);
        state.Memory[address] = 0x28;
        var expected = AfterFetch(cpu, 3, 2);
        expected.B = 0x28;
        expected.WZ = address;
        Check(cpu, state, expected, 19, ("M1", 0, prefix), ("M1", 1, 0x46),
            ("Read", 2, (byte)displacement), ("Internal", 2, 5), ("Read", address, 0x28));
    }

    [Fact]
    public void LdRegIndexed_WrapsAddress()
    {
        var (cpu, state) = Create(0xDD, 0x46, 0xF6);
        cpu.Registers.IX = 5;
        state.Memory[0xFFFB] = 0x28;
        var expected = AfterFetch(cpu, 3, 2);
        expected.B = 0x28;
        expected.WZ = 0xFFFB;
        Check(cpu, state, expected, 19, ("M1", 0, 0xDD), ("M1", 1, 0x46),
            ("Read", 2, 0xF6), ("Internal", 2, 5), ("Read", 0xFFFB, 0x28));
    }

    [Theory]
    [InlineData(0xDD, 0x70, 0x12)]
    [InlineData(0xFD, 0x77, 0xA5)]
    public void LdIndexedReg_WritesWithDisplacement(byte prefix, byte opcode, byte value)
    {
        var (cpu, state) = Create(prefix, opcode, 0xFF);
        var address = (ushort)((prefix == 0xDD ? cpu.Registers.IX : cpu.Registers.IY) - 1);
        var expected = AfterFetch(cpu, 3, 2);
        expected.WZ = address;
        Check(cpu, state, expected, 19, ("M1", 0, prefix), ("M1", 1, opcode),
            ("Read", 2, 0xFF), ("Internal", 2, 5), ("Write", address, value));
        Assert.Equal(value, state.Memory[address]);
    }

    [Theory]
    [InlineData(0xDD, 0x66)]
    [InlineData(0xDD, 0x6E)]
    [InlineData(0xDD, 0x74)]
    [InlineData(0xDD, 0x75)]
    [InlineData(0xFD, 0x66)]
    [InlineData(0xFD, 0x6E)]
    [InlineData(0xFD, 0x74)]
    [InlineData(0xFD, 0x75)]
    public void LdIndexedH_UsesRealHAndL(byte prefix, byte opcode)
    {
        var (cpu, state) = Create(prefix, opcode, 5);
        var address = (ushort)((prefix == 0xDD ? cpu.Registers.IX : cpu.Registers.IY) + 5);
        state.Memory[address] = 0x28;
        var expected = AfterFetch(cpu, 3, 2);
        expected.WZ = address;
        var load = opcode < 0x70;
        var id = opcode is 0x66 or 0x74 ? 4 : 5;
        var value = load ? (byte)0x28 : Reg(ref cpu.Registers, id);
        if (load) Reg(ref expected, id) = value;
        Check(cpu, state, expected, 19, ("M1", 0, prefix), ("M1", 1, opcode),
            ("Read", 2, 5), ("Internal", 2, 5), (load ? "Read" : "Write", address, value));
        Assert.Equal(value, state.Memory[address]);
    }

    [Theory]
    [InlineData(0xDD)]
    [InlineData(0xFD)]
    public void LdIndexedN_InternalCyclesOnOperand(byte prefix)
    {
        var (cpu, state) = Create(prefix, 0x36, 0xFE, 0x5A);
        var address = (ushort)((prefix == 0xDD ? cpu.Registers.IX : cpu.Registers.IY) - 2);
        var expected = AfterFetch(cpu, 4, 2);
        expected.WZ = address;
        Check(cpu, state, expected, 19, ("M1", 0, prefix), ("M1", 1, 0x36),
            ("Read", 2, 0xFE), ("Read", 3, 0x5A), ("Internal", 3, 2), ("Write", address, 0x5A));
        Assert.Equal(0x5A, state.Memory[address]);
    }

    [Theory]
    [InlineData(0xDD, 0x44)]
    [InlineData(0xDD, 0x65)]
    [InlineData(0xFD, 0x6C)]
    [InlineData(0xFD, 0x7D)]
    [InlineData(0xDD, 0x26)]
    [InlineData(0xFD, 0x2E)]
    public void LdIndexHalves_Undocumented(byte prefix, byte opcode)
    {
        var (cpu, state) = Create(prefix, opcode, 0x5A);
        var immediate = opcode is 0x26 or 0x2E;
        var expected = AfterFetch(cpu, immediate ? 3 : 2, 2);
        switch (opcode)
        {
            case 0x44: expected.B = expected.IXH; break;
            case 0x65: expected.IXH = expected.IXL; break;
            case 0x6C: expected.IYL = expected.IYH; break;
            case 0x7D: expected.A = expected.IYL; break;
            case 0x26: expected.IXH = 0x5A; break;
            case 0x2E: expected.IYL = 0x5A; break;
        }
        if (immediate)
            Check(cpu, state, expected, 11, ("M1", 0, prefix), ("M1", 1, opcode), ("Read", 2, 0x5A));
        else
            Check(cpu, state, expected, 8, ("M1", 0, prefix), ("M1", 1, opcode));
    }

    [Theory]
    [InlineData(0xDD, 0x64)]
    [InlineData(0xFD, 0x6D)]
    public void LdIndexHalves_SameRegisterIsNoOp(byte prefix, byte opcode)
    {
        var (cpu, state) = Create(prefix, opcode);
        Check(cpu, state, AfterFetch(cpu, 2, 2), 8, ("M1", 0, prefix), ("M1", 1, opcode));
    }

    [Theory]
    [InlineData(0xDD, 0x41)]
    [InlineData(0xFD, 0x06)]
    [InlineData(0xDD, 0x0A)]
    [InlineData(0xFD, 0x32)]
    public void IndexPrefix_UnaffectedLoadsRunAsBase(byte prefix, byte opcode)
    {
        var (cpu, state) = Create(prefix, opcode, 0x34, 0x12);
        state.Memory[0x1234] = 0x28;
        var (plain, plainState) = Create(opcode, 0x34, 0x12);
        plainState.Memory[0x1234] = 0x28;
        plain.Step();
        var expected = plain.Registers;
        expected.PC++;
        expected.IncrementR();
        var events = new List<(string, ushort, int)> { ("M1", 0, prefix) };
        foreach (var access in plainState.Accesses)
            events.Add((access.Kind, access.Kind == "M1" || access.Kind == "Read" && access.Address < 3
                ? (ushort)(access.Address + 1) : access.Address, access.Value));
        Check(cpu, state, expected, plainState.Cycles + 4, events.ToArray());
        Assert.Equal(plainState.Memory[0x1234], state.Memory[0x1234]);
    }

    [Theory]
    [InlineData(0x41)]
    [InlineData(0x06, 0x5A)]
    [InlineData(0x46)]
    [InlineData(0x70)]
    [InlineData(0x36, 0x5A)]
    [InlineData(0x3A, 0x34, 0x12)]
    [InlineData(0x32, 0x34, 0x12)]
    [InlineData(0xDD, 0x46, 5)]
    [InlineData(0xDD, 0x70, 5)]
    [InlineData(0xDD, 0x36, 5, 0x5A)]
    [InlineData(0xED, 0x47)]
    [InlineData(0xED, 0x4F)]
    public void Load8_DoesNotTouchFlagsOrCounter(int first, int second = -1, int third = -1, int fourth = -1)
    {
        var program = new[] { first, second, third, fourth }.Where(value => value >= 0)
            .Select(value => (byte)value).ToArray();
        var (cpu, _) = Create(program);
        var indexed = first == 0xDD;
        var expected = AfterFetch(cpu, program.Length, first is 0xDD or 0xED ? 2 : 1);
        switch (indexed ? second : first)
        {
            case 0x41: expected.B = expected.C; break;
            case 0x06: expected.B = 0x5A; break;
            case 0x46:
                expected.B = 0;
                if (indexed) expected.WZ = 0x135C;
                break;
            case 0x70:
            case 0x36:
                if (indexed) expected.WZ = 0x135C;
                break;
            case 0x3A: expected.A = 0; expected.WZ = 0x1235; break;
            case 0x32: expected.WZ = 0xA535; break;
            case 0xED:
                if (second == 0x47) expected.I = expected.A;
                else expected.R = expected.A;
                break;
        }
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void Load8_AllowsIntOnNextStep()
    {
        var (cpu, state) = Create(0x3A, 0x34, 0x12);
        state.Memory[0x1234] = 0x28;
        state.IntAfterCycle = 1;
        var expected = AfterFetch(cpu, 3);
        expected.A = 0x28;
        expected.WZ = 0x1235;
        Check(cpu, state, expected, 13, ("M1", 0, 0x3A), ("Read", 1, 0x34),
            ("Read", 2, 0x12), ("Read", 0x1234, 0x28));
        expected.PC = expected.WZ = 0x38;
        expected.SP = 0x8FFE;
        expected.IFF1 = expected.IFF2 = false;
        expected.IncrementR();
        state.Accesses.Clear();
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(26, state.Cycles);
        Assert.Equal(new[] { ("Ack", (ushort)0, 0xFF), ("Write", (ushort)0x8FFF, 0),
            ("Write", (ushort)0x8FFE, 3) }, state.Accesses);
        Assert.Equal(3, state.Memory[0x8FFE]);
        Assert.Equal(0, state.Memory[0x8FFF]);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }
}
