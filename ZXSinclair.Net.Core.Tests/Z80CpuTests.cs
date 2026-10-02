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

namespace ZXSinclair.Net.Core.Tests;

public class Z80CpuTests
{
    private static (Z80Cpu<TestBus> Cpu, TestBusState State) Create(params byte[] program)
    {
        var state = new TestBusState();
        program.CopyTo(state.Memory, 0);
        return (new(new(state)), state);
    }

    [Fact]
    public void ResetClearsAllCpuStateWithoutResettingBus()
    {
        var (cpu, state) = Create(0);
        cpu.Step();
        cpu.Registers = new Z80Registers
        {
            AF = 1, BC = 2, DE = 3, HL = 4, AF_ = 5, BC_ = 6, DE_ = 7, HL_ = 8,
            IX = 9, IY = 10, SP = 11, PC = 12, WZ = 13, IR = 14,
            IFF1 = true, IFF2 = true, IM = 2, Halted = true, EiPending = true, Q = 0xFF,
        };
        cpu.RequestNmi();
        cpu.Reset();
        Assert.Equal(new Z80Registers { AF = 0xFFFF, SP = 0xFFFF }, cpu.Registers);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        Assert.Equal(4, state.Cycles);
        cpu.Step();
        Assert.Equal((ushort)1, cpu.Registers.PC);
        Assert.Equal(8, state.Cycles);
    }

    [Theory]
    [InlineData(new byte[] { 0xD3 }, 4, 1)]
    [InlineData(new byte[] { 0xCB, 0x12 }, 8, 2)]
    [InlineData(new byte[] { 0xED, 0x12 }, 8, 2)]
    [InlineData(new byte[] { 0xDD, 0xD3 }, 8, 2)]
    [InlineData(new byte[] { 0xFD, 0xD3 }, 8, 2)]
    [InlineData(new byte[] { 0xDD, 0xFD, 0xD3 }, 12, 3)]
    [InlineData(new byte[] { 0xFD, 0xDD, 0xD3 }, 12, 3)]
    [InlineData(new byte[] { 0xDD, 0xED, 0x12 }, 12, 3)]
    [InlineData(new byte[] { 0xFD, 0xCB, 0xFE, 0x12 }, 16, 2)]
    [InlineData(new byte[] { 0xDD, 0xCB, 0x80, 0x12 }, 16, 2)]
    public void UnimplementedDispatchCountsFetches(byte[] program, int cycles, byte refresh)
    {
        var (cpu, state) = Create(program);
        cpu.Registers.R = 0x80;
        cpu.Step();
        Assert.Equal(cycles, state.Cycles);
        Assert.Equal((ushort)program.Length, cpu.Registers.PC);
        Assert.Equal((byte)(0x80 | refresh), cpu.Registers.R);
        Assert.Equal(1, cpu.UnimplementedOpcodes);
        if (program.Contains((byte)0xCB) && program[0] is 0xDD or 0xFD)
            Assert.Equal(("Internal", (ushort)3, 2), state.Accesses[^1]);
    }

    [Fact]
    public void LongAlternatingPrefixChainUsesConstantStackSpace()
    {
        var (cpu, state) = Create();
        for (var i = 0; i < 65535; i++)
            state.Memory[i] = (byte)((i & 1) == 0 ? 0xDD : 0xFD);
        state.Memory[0xFFFF] = 0xD3;
        cpu.Registers.R = 0x80;
        cpu.Step();
        Assert.Equal(262144, state.Cycles);
        Assert.Equal((ushort)0, cpu.Registers.PC);
        Assert.Equal((byte)0x80, cpu.Registers.R);
        Assert.Equal(1, cpu.UnimplementedOpcodes);
    }

    [Fact]
    public void PrefixesAndOperandReadsWrapAtEndOfMemory()
    {
        var (cpu, state) = Create(0xCB, 0xFE, 0x12);
        state.Memory[0xFFFE] = 0xDD;
        state.Memory[0xFFFF] = 0xFD;
        cpu.Registers.PC = 0xFFFE;
        cpu.Step();
        Assert.Equal(20, state.Cycles);
        Assert.Equal((ushort)3, cpu.Registers.PC);
        Assert.Equal((byte)3, cpu.Registers.R);
        Assert.Equal(new ushort[] { 0xFFFE, 0xFFFF, 0, 1, 2, 2 },
            state.Accesses.Select(a => a.Address).ToArray());
    }

    [Fact]
    public void InterruptBecomingActiveDuringPrefixesWaitsForNextStep()
    {
        var (cpu, state) = Create(0xDD, 0xFD, 0);
        cpu.Registers.IFF1 = true;
        cpu.Registers.IM = 1;
        state.IntAfterCycle = 4;
        cpu.Step();
        Assert.Equal((ushort)3, cpu.Registers.PC);
        Assert.Equal(12, state.Cycles);
        Assert.True(cpu.Registers.IFF1);
        cpu.Step();
        Assert.Equal((ushort)0x38, cpu.Registers.PC);
        Assert.Equal(25, state.Cycles);
        Assert.Equal((byte)3, state.Memory[0xFFFD]);
    }

    [Fact]
    public void HaltRepeatsM1WithoutAdvancingOrDispatching()
    {
        var (cpu, state) = Create(0xDD);
        cpu.Registers.Halted = true;
        cpu.Registers.R = 0xFF;
        cpu.Step();
        Assert.True(cpu.Halted);
        Assert.Equal((ushort)0, cpu.Registers.PC);
        Assert.Equal((byte)0x80, cpu.Registers.R);
        Assert.Equal(4, state.Cycles);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        Assert.Equal(("M1", (ushort)0, 0xDD), Assert.Single(state.Accesses));
    }

    [Theory]
    [InlineData(0, 0xC7, 0x00)]
    [InlineData(0, 0xCF, 0x08)]
    [InlineData(0, 0xD7, 0x10)]
    [InlineData(0, 0xDF, 0x18)]
    [InlineData(0, 0xE7, 0x20)]
    [InlineData(0, 0xEF, 0x28)]
    [InlineData(0, 0xF7, 0x30)]
    [InlineData(0, 0xFF, 0x38)]
    [InlineData(1, 0x00, 0x38)]
    public void IntRestartsPushPcAndClearBothFlipFlops(byte mode, byte data, ushort target)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0x1234;
        cpu.Registers.SP = 0;
        cpu.Registers.R = 0xFF;
        cpu.Registers.IM = mode;
        cpu.Registers.IFF1 = cpu.Registers.IFF2 = true;
        state.IntActive = true;
        state.InterruptData = data;
        cpu.Step();
        Assert.Equal(13, state.Cycles);
        Assert.Equal(target, cpu.Registers.PC);
        Assert.Equal(target, cpu.Registers.WZ);
        Assert.Equal((ushort)0xFFFE, cpu.Registers.SP);
        Assert.Equal((byte)0x12, state.Memory[0xFFFF]);
        Assert.Equal((byte)0x34, state.Memory[0xFFFE]);
        Assert.Equal((byte)0x80, cpu.Registers.R);
        Assert.False(cpu.Registers.IFF1);
        Assert.False(cpu.Registers.IFF2);
        Assert.Equal(new[] { "Ack", "Write", "Write" }, state.Accesses.Select(a => a.Kind));
        Assert.Equal(0, cpu.UnimplementedOpcodes);
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0xCD)]
    [InlineData(0xCB)]
    [InlineData(0xDD)]
    public void UnsupportedIm0DoesNotInventJumpOrStackWrites(byte data)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0x1234;
        cpu.Registers.SP = 0x8000;
        cpu.Registers.WZ = 0xABCD;
        cpu.Registers.IFF1 = cpu.Registers.IFF2 = true;
        state.IntActive = true;
        state.InterruptData = data;
        cpu.Step();
        Assert.Equal(7, state.Cycles);
        Assert.Equal((ushort)0x1234, cpu.Registers.PC);
        Assert.Equal((ushort)0x8000, cpu.Registers.SP);
        Assert.Equal((ushort)0xABCD, cpu.Registers.WZ);
        Assert.Equal(1, cpu.UnimplementedOpcodes);
        Assert.Equal("Ack", Assert.Single(state.Accesses).Kind);
        Assert.False(cpu.Registers.IFF1);
        Assert.False(cpu.Registers.IFF2);
    }

    [Theory]
    [InlineData(0x42, 0x81)]
    [InlineData(0xFF, 0xFF)]
    public void Im2ReadsBothVectorBytesIncludingAddressWrap(byte i, byte data)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0x1234;
        cpu.Registers.SP = 0x9000;
        cpu.Registers.IM = 2;
        cpu.Registers.I = i;
        cpu.Registers.IFF1 = cpu.Registers.IFF2 = true;
        state.IntActive = true;
        state.InterruptData = data;
        var vector = (ushort)((i << 8) | data);
        state.Memory[vector] = 0x78;
        state.Memory[unchecked((ushort)(vector + 1))] = 0x56;
        cpu.Step();
        Assert.Equal(19, state.Cycles);
        Assert.Equal((ushort)0x5678, cpu.Registers.PC);
        Assert.Equal((ushort)0x5678, cpu.Registers.WZ);
        Assert.Equal(new ushort[] { 0, 0x8FFF, 0x8FFE, vector, unchecked((ushort)(vector + 1)) },
            state.Accesses.Select(a => a.Address));
        Assert.Equal((byte)0x12, state.Memory[0x8FFF]);
        Assert.Equal((byte)0x34, state.Memory[0x8FFE]);
        Assert.False(cpu.Registers.IFF1);
        Assert.False(cpu.Registers.IFF2);
        Assert.Equal((byte)1, cpu.Registers.R);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NmiTakesPriorityAndPreservesIff2(bool iff2)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0x1234;
        cpu.Registers.SP = 0x8000;
        cpu.Registers.I = 0x42;
        cpu.Registers.R = 0xFF;
        cpu.Registers.IFF1 = true;
        cpu.Registers.IFF2 = iff2;
        cpu.Registers.EiPending = true;
        state.IntActive = true;
        cpu.RequestNmi();
        cpu.Step();
        Assert.Equal(11, state.Cycles);
        Assert.Equal((ushort)0x66, cpu.Registers.PC);
        Assert.Equal((ushort)0x66, cpu.Registers.WZ);
        Assert.False(cpu.Registers.IFF1);
        Assert.Equal(iff2, cpu.Registers.IFF2);
        Assert.False(cpu.Registers.EiPending);
        Assert.Equal((byte)0x80, cpu.Registers.R);
        Assert.Equal(("Internal", (ushort)0x4280, 1), state.Accesses[1]);
        Assert.Equal((byte)0x12, state.Memory[0x7FFF]);
        Assert.Equal((byte)0x34, state.Memory[0x7FFE]);
        cpu.Step();
        Assert.Equal(15, state.Cycles);
        Assert.Equal((ushort)0x67, cpu.Registers.PC);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcceptingInterruptExitsHaltAndPushesFollowingAddress(bool nmi)
    {
        var (cpu, state) = Create();
        cpu.Registers.PC = 0xFFFF;
        cpu.Registers.Halted = true;
        cpu.Registers.IFF1 = true;
        cpu.Registers.IM = 1;
        state.IntActive = true;
        if (nmi) cpu.RequestNmi();
        cpu.Step();
        Assert.False(cpu.Halted);
        Assert.Equal(nmi ? 11 : 13, state.Cycles);
        Assert.Equal((byte)0, state.Memory[0xFFFD]);
        Assert.Equal((byte)0, state.Memory[0xFFFE]);
        Assert.Equal((ushort)(nmi ? 0x66 : 0x38), cpu.Registers.PC);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void IntIsBlockedByIff1OrEiDelay(bool iff1, bool pending)
    {
        var (cpu, state) = Create(0, 0);
        cpu.Registers.IFF1 = iff1;
        cpu.Registers.EiPending = pending;
        cpu.Registers.IM = 1;
        state.IntActive = true;
        cpu.Step();
        Assert.Equal(4, state.Cycles);
        Assert.Equal((ushort)1, cpu.Registers.PC);
        Assert.False(cpu.Registers.EiPending);
        cpu.Step();
        Assert.Equal(iff1 ? 17 : 8, state.Cycles);
    }

    [Fact]
    public void ExecuteUsesAbsoluteTargetAndFinishesWholeInstruction()
    {
        var (cpu, state) = Create(0xDD, 0, 0);
        cpu.Execute(5);
        Assert.Equal(8, state.Cycles);
        Assert.Equal((ushort)2, cpu.Registers.PC);
        cpu.Execute(8);
        Assert.Equal(8, state.Cycles);
        cpu.Execute(9);
        Assert.Equal(12, state.Cycles);
    }

    [Fact]
    public void SpectrumIntegrationRunsAFrameAndAppliesFetchContention()
    {
        var machine = new SpectrumMachine(SpectrumModel.Spectrum48K);
        var cpu = new Z80Cpu<SpectrumBus>(machine.Bus);
        cpu.Execute(machine.Timing.TStatesPerFrame);
        Assert.Equal(machine.Timing.TStatesPerFrame, machine.TStates);
        Assert.Equal(0, cpu.UnimplementedOpcodes);
        Assert.Equal((ushort)17472, cpu.Registers.PC);
        machine.EndFrame();
        Assert.Equal(0, machine.TStates);

        machine.TStates = machine.Timing.FirstContendedTState;
        cpu.Registers.PC = 0x4000;
        cpu.Step();
        Assert.Equal(machine.Timing.FirstContendedTState + 10, machine.TStates);
    }
}
