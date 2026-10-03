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

public class NopTests
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

    [Fact]
    public void Nop_EmitsSingleM1()
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0x1234;
        cpu.Step();
        Assert.Equal(("M1", (ushort)0x1234, 0), Assert.Single(state.Accesses));
        Assert.Equal(4, state.Cycles);
    }

    [Fact]
    public void Nop_ChangesOnlyPcAndR()
    {
        var (cpu, _) = Create();
        cpu.Registers.PC = 0x1234;
        var expected = cpu.Registers;
        expected.PC = 0x1235;
        expected.R = 0x92;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
    }

    [Fact]
    public void Nop_DoesNotCountAsUnimplemented()
    {
        var (cpu, _) = Create(0, 0xED, 0, 0);
        cpu.Step();
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        cpu.Step();
        Assert.Equal(1, cpu.UnimplementedOpcodes);
        cpu.Step();
        Assert.Equal(1, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0x7F, 0x00)]
    [InlineData(0xFF, 0x80)]
    public void Nop_RefreshWrapsKeepingBit7(byte initial, byte expected)
    {
        var (cpu, _) = Create();
        cpu.Registers.R = initial;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers.R);
    }

    [Fact]
    public void Nop_AtFFFF_WrapsPcToZero()
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFF;
        cpu.Step();
        Assert.Equal((ushort)0, cpu.Registers.PC);
        Assert.Equal(("M1", (ushort)0xFFFF, 0), Assert.Single(state.Accesses));
        Assert.Equal(4, state.Cycles);
    }

    [Fact]
    public void Nop_DoesNotWriteMemory()
    {
        var (cpu, state) = Create();
        for (var i = 0; i < state.Memory.Length; i++)
            state.Memory[i] = (byte)(i * 37 + 11);
        cpu.Registers.PC = 0x1234;
        state.Memory[0x1234] = 0;
        var before = (byte[])state.Memory.Clone();
        cpu.Step();
        Assert.Equal(before, state.Memory);
        Assert.DoesNotContain(state.Accesses, a => a.Kind == "Write");
    }

    [Theory]
    [InlineData(0xDD)]
    [InlineData(0xFD)]
    public void Nop_WithIndexPrefix_RunsAsPlainNop(byte prefix)
    {
        var (cpu, state) = Create(prefix, 0);
        var expected = cpu.Registers;
        expected.PC = 2;
        expected.R = 0x93;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(new[] { ("M1", (ushort)0, (int)prefix), ("M1", (ushort)1, 0) }, state.Accesses);
        Assert.Equal(8, state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0xDD, 0xFD)]
    [InlineData(0xFD, 0xDD)]
    [InlineData(0xDD, 0xDD)]
    [InlineData(0xFD, 0xFD)]
    public void Nop_WithRepeatedPrefixes_RunsAsPlainNop(byte first, byte second)
    {
        var (cpu, state) = Create(first, second, 0);
        var expected = cpu.Registers;
        expected.PC = 3;
        expected.R = 0x94;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(new[] { ("M1", (ushort)0, (int)first), ("M1", (ushort)1, (int)second),
            ("M1", (ushort)2, 0) }, state.Accesses);
        Assert.Equal(12, state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void Nop_ClearsEiPending()
    {
        var (cpu, state) = Create();
        cpu.Registers.EiPending = true;
        state.IntActive = true;
        var expected = cpu.Registers;
        expected.PC = 1;
        expected.R = 0x92;
        expected.EiPending = false;
        cpu.Step();
        Assert.Equal(expected, cpu.Registers);
        Assert.Equal(4, state.Cycles);
    }

    [Fact]
    public void Nop_AllowsIntOnNextStep()
    {
        var (cpu, state) = Create();
        state.IntAfterCycle = 1;
        cpu.Step();
        Assert.Equal(4, state.Cycles);
        Assert.Equal((ushort)1, cpu.Registers.PC);
        Assert.True(cpu.Registers.IFF1);
        cpu.Step();
        Assert.Equal(17, state.Cycles);
        Assert.Equal((ushort)0x38, cpu.Registers.PC);
        Assert.False(cpu.Registers.IFF1);
        Assert.False(cpu.Registers.IFF2);
        Assert.Equal((ushort)0x8FFE, cpu.Registers.SP);
        Assert.Equal((byte)1, state.Memory[0x8FFE]);
        Assert.Equal((byte)0, state.Memory[0x8FFF]);
        Assert.Equal(new[] { ("M1", (ushort)0, 0), ("Ack", (ushort)0, 0xFF),
            ("Write", (ushort)0x8FFF, 0), ("Write", (ushort)0x8FFE, 1) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void Nop_AllowsNmiOnNextStep()
    {
        var (cpu, state) = Create();
        cpu.Step();
        Assert.Equal(4, state.Cycles);
        Assert.Equal((ushort)1, cpu.Registers.PC);
        cpu.RequestNmi();
        cpu.Step();
        Assert.Equal(15, state.Cycles);
        Assert.Equal((ushort)0x66, cpu.Registers.PC);
        Assert.False(cpu.Registers.IFF1);
        Assert.True(cpu.Registers.IFF2);
        Assert.Equal((ushort)0x8FFE, cpu.Registers.SP);
        Assert.Equal((byte)1, state.Memory[0x8FFE]);
        Assert.Equal((byte)0, state.Memory[0x8FFF]);
        Assert.Equal(new[] { ("M1", (ushort)0, 0), ("M1", (ushort)1, 0),
            ("Internal", (ushort)0x4293, 1), ("Write", (ushort)0x8FFF, 0),
            ("Write", (ushort)0x8FFE, 1) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void Nop_ExecuteStopsAtTarget()
    {
        var (cpu, state) = Create();
        cpu.Execute(10);
        Assert.Equal(12, state.Cycles);
        Assert.Equal((ushort)3, cpu.Registers.PC);
        Assert.Equal((byte)0x94, cpu.Registers.R);
        Assert.Equal(new[] { ("M1", (ushort)0, 0), ("M1", (ushort)1, 0),
            ("M1", (ushort)2, 0) }, state.Accesses);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }
}
