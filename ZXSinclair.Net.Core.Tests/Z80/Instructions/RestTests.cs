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

public class RestTests
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
            I = 0x42, R = 0xFF, IFF1 = true, IFF2 = true, IM = 1,
        };
        state.Memory[0x9000] = 0x34;
        state.Memory[0x9001] = 0x12;
        return (cpu, state);
    }

    public static IEnumerable<object[]> EdHoles()
    {
        for (var value = 0; value < 256; value++)
            if (value is 0x77 or 0x7F || value < 0x40
                || value >= 0x80 && !(value is >= 0xA0 and <= 0xA3
                    or >= 0xA8 and <= 0xAB or >= 0xB0 and <= 0xB3 or >= 0xB8 and <= 0xBB))
                yield return [value];
    }

    [Theory]
    [MemberData(nameof(EdHoles))]
    public void EdHoles_AllAreTwoNops(int value)
    {
        var (cpu, state) = Create(0xED, (byte)value);
        cpu.Registers.Q = 0xFF;
        var expected = cpu.Registers;
        expected.PC += 2;
        expected.R = 0x81;
        expected.Q = 0;
        var memory = (byte[])state.Memory.Clone();
        cpu.Step();
        Assert.Equal(178, EdHoles().Count());
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(memory, state.Memory);
        Assert.Equal(8, state.Cycles);
        Assert.Equal(new[] { ("M1", (ushort)0x8000, 0xED), ("M1", (ushort)0x8001, value) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(new byte[] { 0xDD, 0xED, 0x00 })]
    [InlineData(new byte[] { 0xFD, 0xED, 0x77 })]
    [InlineData(new byte[] { 0xDD, 0xFD, 0xED, 0xFB })]
    public void EdHoles_WithIndexPrefix(byte[] program)
    {
        var (cpu, state) = Create(program);
        var expected = cpu.Registers;
        expected.PC += (ushort)program.Length;
        expected.R = (byte)(0x80 | (program.Length - 1));
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(4 * program.Length, state.Cycles);
        Assert.Equal(program.Select((b, i) => ("M1", (ushort)(0x8000 + i), (int)b)), state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void EdHole_AllowsIntOnNextStep()
    {
        var (cpu, state) = Create(0xED, 0);
        state.IntAfterCycle = 8;
        cpu.Step();
        cpu.Step();
        Assert.Equal(21, state.Cycles);
        Assert.Equal((ushort)0x38, cpu.Registers.PC);
        Assert.Equal((byte)2, state.Memory[0x8FFE]);
        Assert.Equal((byte)0x80, state.Memory[0x8FFF]);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0x00)] [InlineData(0xFB)]
    public void EdHole_OperandWrapsPc(byte opcode)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFF;
        state.Memory[0xFFFF] = 0xED;
        state.Memory[0] = opcode;
        var expected = cpu.Registers;
        expected.PC = 1;
        expected.R = 0x81;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(8, state.Cycles);
        Assert.Equal(new[] { ("M1", (ushort)0xFFFF, 0xED), ("M1", (ushort)0, (int)opcode) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(1, 13)]
    [InlineData(2, 19)]
    public void Ack_IsSixTStates(byte mode, int cycles)
    {
        var (cpu, state) = Create();
        var bus = new TestBus(new TestBusState());
        bus.AcknowledgeInterrupt();
        Assert.Equal(6, bus.Cycles);
        cpu.Registers.IM = mode;
        state.IntActive = true;
        state.Memory[0x42FF] = 0x38;
        cpu.Step();
        Assert.Equal(cycles, state.Cycles);
        Assert.Equal(("Ack", (ushort)0, 0xFF), state.Accesses[0]);
        Assert.Equal(("Internal", (ushort)0x4280, 1), state.Accesses[1]);
        Assert.False(cpu.Registers.IFF1);
        Assert.False(cpu.Registers.IFF2);
        Assert.Equal((byte)0x80, cpu.Registers.R);
        Assert.Equal((byte)0, cpu.Registers.Q);
    }

    [Theory]
    [InlineData(0x00, 6)] [InlineData(0x47, 6)] [InlineData(0x3C, 6)]
    [InlineData(0xEB, 6)] [InlineData(0xF3, 6)] [InlineData(0x03, 8)]
    [InlineData(0xF9, 8)] [InlineData(0xC5, 13)] [InlineData(0xC8, 13)]
    [InlineData(0xC8, 7, 0)] [InlineData(0xFF, 13)]
    public void Im0_TimingIsNormalPlusTwo(byte opcode, int cycles, byte flags = 0xFF)
    {
        var (cpu, state) = Create();
        cpu.Registers.F = flags;
        cpu.Registers.IM = 0;
        state.IntActive = true;
        state.InterruptData = opcode;
        cpu.Step();
        Assert.Equal(cycles, state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        Assert.Equal((byte)0x80, cpu.Registers.R);
        Assert.False(cpu.Registers.IFF1);
        Assert.False(cpu.Registers.IFF2);
        Assert.DoesNotContain(state.Accesses, a => a.Kind == "M1");
        switch (opcode)
        {
            case 0x47: Assert.Equal(cpu.Registers.A, cpu.Registers.B); break;
            case 0x3C: Assert.Equal((byte)0xA6, cpu.Registers.A); Assert.Equal(cpu.Registers.F, cpu.Registers.Q); break;
            case 0xEB: Assert.Equal((ushort)0x9ABC, cpu.Registers.DE); Assert.Equal((ushort)0x5678, cpu.Registers.HL); break;
            case 0x03: Assert.Equal((ushort)0x1235, cpu.Registers.BC); break;
            case 0xF9: Assert.Equal(cpu.Registers.HL, cpu.Registers.SP); break;
            case 0xC5: Assert.Equal((ushort)0x8FFE, cpu.Registers.SP); Assert.Equal((byte)0x34, state.Memory[0x8FFE]); break;
        }
        if (opcode != 0xFF && !(opcode == 0xC8 && cycles == 13))
        {
            Assert.Equal((ushort)0x8000, cpu.Registers.PC);
            Assert.Equal((ushort)0xBEEF, cpu.Registers.WZ);
        }
        else
            Assert.Equal((ushort)(opcode == 0xFF ? 0x38 : 0x1234), cpu.Registers.PC);
    }

    [Theory]
    [InlineData(0xC5, 1)] [InlineData(0x03, 2)] [InlineData(0xC8, 1)]
    public void Im0_ExtendedM1_InternalCyclesOnIr(byte opcode, int cycles)
    {
        var (cpu, state) = Create();
        cpu.Registers.IM = 0;
        state.IntActive = true;
        state.InterruptData = opcode;
        cpu.Step();
        Assert.Equal(("Internal", (ushort)0x4280, cycles), state.Accesses[1]);
    }

    [Theory]
    [InlineData(0xC7)] [InlineData(0xCF)] [InlineData(0xD7)] [InlineData(0xDF)]
    [InlineData(0xE7)] [InlineData(0xEF)] [InlineData(0xF7)] [InlineData(0xFF)]
    public void Im0_Rst_ViaInstructionPath(byte opcode)
    {
        var (cpu, state) = Create();
        cpu.Registers.IM = 0;
        state.IntActive = true;
        state.InterruptData = opcode;
        cpu.Step();
        Assert.Equal(13, state.Cycles);
        Assert.Equal((ushort)(opcode & 0x38), cpu.Registers.PC);
        Assert.Equal(cpu.Registers.PC, cpu.Registers.WZ);
        Assert.Equal(new[] { ("Ack", (ushort)0, (int)opcode), ("Internal", (ushort)0x4280, 1),
            ("Write", (ushort)0x8FFF, 0x80), ("Write", (ushort)0x8FFE, 0) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void Im0_Ei_SetsPending()
    {
        var (cpu, state) = Create(0);
        cpu.Registers.IM = 0;
        state.IntActive = true;
        state.InterruptData = 0xFB;
        cpu.Step();
        Assert.True(cpu.Registers.EiPending);
        Assert.True(cpu.Registers.IFF1);
        Assert.True(cpu.Registers.IFF2);
        Assert.Equal(6, state.Cycles);
        cpu.Step();
        Assert.False(cpu.Registers.EiPending);
        Assert.Equal(10, state.Cycles);
        Assert.Equal((ushort)0x8001, cpu.Registers.PC);
        cpu.Registers.IM = 1;
        cpu.Step();
        Assert.Equal(23, state.Cycles);
        Assert.Equal((ushort)0x38, cpu.Registers.PC);
    }

    [Theory]
    [InlineData(0x8000)] [InlineData(0)]
    public void Im0_Halt_StopsAndReturnsToInterrupted(ushort pc)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = pc;
        cpu.Registers.IM = 0;
        state.IntActive = true;
        state.InterruptData = 0x76;
        cpu.Step();
        Assert.True(cpu.Halted);
        Assert.Equal(unchecked((ushort)(pc - 1)), cpu.Registers.PC);
        Assert.Equal(6, state.Cycles);
        cpu.Step();
        cpu.Step();
        Assert.Equal(14, state.Cycles);
        Assert.Equal((byte)0x82, cpu.Registers.R);
        Assert.All(state.Accesses.Skip(1), a =>
        {
            Assert.Equal("M1", a.Kind);
            Assert.Equal(unchecked((ushort)(pc - 1)), a.Address);
        });
        cpu.Registers.IM = 1;
        cpu.Registers.IFF1 = true;
        cpu.Step();
        Assert.False(cpu.Halted);
        Assert.Equal(27, state.Cycles);
        Assert.Equal((byte)pc, state.Memory[0x8FFE]);
        Assert.Equal((byte)(pc >> 8), state.Memory[0x8FFF]);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xCD)] [InlineData(0xC3)] [InlineData(0x3E)]
    [InlineData(0xCB)] [InlineData(0xDD)] [InlineData(0xED)] [InlineData(0xFD)]
    public void Im0_MultiByteOrPrefix_Unsupported(byte opcode)
    {
        var (cpu, state) = Create();
        cpu.Registers.IM = 0;
        var expected = cpu.Registers;
        expected.IFF1 = expected.IFF2 = false;
        expected.R = 0x80;
        state.IntActive = true;
        state.InterruptData = opcode;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(6, state.Cycles);
        Assert.Equal(1, cpu.UnimplementedOpcodes);
        Assert.Equal((ushort)0x8000, cpu.LastUnimplementedAddress);
        Assert.Equal(("Ack", (ushort)0, (int)opcode), Assert.Single(state.Accesses));
    }

    // Independent reference: all base opcodes except these immediate forms and prefixes.
    private static readonly byte[] MultiByteOpcodes =
    [
        0x01,0x06,0x0E,0x10,0x11,0x16,0x18,0x1E,0x20,0x21,0x22,0x26,0x28,0x2A,0x2E,
        0x30,0x31,0x32,0x36,0x38,0x3A,0x3E,0xC2,0xC3,0xC4,0xC6,0xCA,0xCB,0xCC,0xCD,0xCE,
        0xD2,0xD3,0xD4,0xD6,0xDA,0xDB,0xDC,0xDD,0xDE,0xE2,0xE4,0xE6,0xEA,0xEC,0xED,0xEE,
        0xF2,0xF4,0xF6,0xFA,0xFC,0xFD,0xFE,
    ];

    [Fact]
    public void Im0Table_MatchesSingleByteOpcodes()
    {
        for (var value = 0; value < 256; value++)
        {
            var (cpu, state) = Create();
            cpu.Registers.IM = 0;
            state.IntActive = true;
            state.InterruptData = (byte)value;
            cpu.Step();
            Assert.Equal(MultiByteOpcodes.Contains((byte)value) ? 1 : 0, cpu.UnimplementedOpcodes);
        }
    }

    [Theory]
    [InlineData(0x57)] [InlineData(0x5F)]
    public void LdAI_InterruptedClearsParity(byte opcode)
    {
        var (cpu, state) = Create(0xED, opcode);
        state.Memory[0x38] = 0xF5;
        state.IntAfterCycle = 9;
        cpu.Step();
        var flags = cpu.Registers.F;
        Assert.True(cpu.Registers.SpecialLoadPending);
        Assert.NotEqual(0, flags & Z80Flags.PV);
        cpu.Step();
        Assert.Equal((byte)(flags & ~Z80Flags.PV), cpu.Registers.F);
        Assert.False(cpu.Registers.SpecialLoadPending);
        cpu.Step();
        Assert.Equal((byte)(flags & ~Z80Flags.PV), state.Memory[0x8FFC]);
        Assert.Equal(33, state.Cycles);
    }

    [Theory]
    [InlineData(0x57)] [InlineData(0x5F)]
    public void LdAI_NotInterruptedKeepsParity(byte opcode)
    {
        var (cpu, state) = Create(0xED, opcode, 0);
        state.Memory[0x38] = 0xF5;
        cpu.Step();
        var flags = cpu.Registers.F;
        cpu.Step();
        Assert.False(cpu.Registers.SpecialLoadPending);
        state.IntActive = true;
        cpu.Step();
        cpu.Step();
        Assert.Equal(flags, state.Memory[0x8FFC]);
        Assert.NotEqual(0, flags & Z80Flags.PV);
    }

    [Theory]
    [InlineData(0x57)] [InlineData(0x5F)]
    public void LdAI_NmiDoesNotClearParity(byte opcode)
    {
        var (cpu, _) = Create(0xED, opcode);
        cpu.Step();
        var flags = cpu.Registers.F;
        cpu.RequestNmi();
        cpu.Step();
        Assert.Equal(flags, cpu.Registers.F);
        Assert.False(cpu.Registers.SpecialLoadPending);
    }

    [Fact]
    public void SpecialLoadPending_ClearedByNextInstructionAndReset()
    {
        var (cpu, _) = Create(0xED, 0x57, 0);
        cpu.Step();
        Assert.True(cpu.Registers.SpecialLoadPending);
        cpu.Step();
        Assert.False(cpu.Registers.SpecialLoadPending);
        cpu.Registers.SpecialLoadPending = true;
        cpu.Reset();
        Assert.False(cpu.Registers.SpecialLoadPending);
    }
}
