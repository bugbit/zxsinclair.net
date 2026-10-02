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

namespace ZXSinclair.Net.Core.Tests;

public class SpectrumBusTests
{
    private static SpectrumMachine At(int tstates, SpectrumModel model = SpectrumModel.Spectrum48K)
    {
        var machine = new SpectrumMachine(model);

        machine.TStates = tstates;

        return machine;
    }

    [Theory]
    [InlineData(0x8000, 0, 3)]
    [InlineData(0x4000, 0, 3)]
    [InlineData(0x4000, 14335, 14335 + 6 + 3)]
    [InlineData(0x4000, 14337, 14337 + 4 + 3)]
    [InlineData(0x8000, 14335, 14335 + 3)]
    [InlineData(0x0000, 14335, 14335 + 3)]
    public void Read_AddsContentionAtT1(int address, int start, int end)
    {
        var machine = At(start);

        machine.Bus.Read((ushort)address);

        Assert.Equal(end, machine.TStates);
    }

    [Fact]
    public void Write_AddsContentionAndStoresTheValue()
    {
        var machine = At(14335);

        machine.Bus.Write(0x4000, 0x42);

        Assert.Equal(14335 + 6 + 3, machine.TStates);
        Assert.Equal(0x42, machine.Memory.Read(0x4000));
    }

    [Theory]
    [InlineData(0x8000, 0, 3)]
    [InlineData(0x4000, 0, 3)]
    [InlineData(0x4000, 14335, 14344)]
    [InlineData(0x4000, 14337, 14344)]
    [InlineData(0x8000, 14335, 14338)]
    public void ReadDiscarded_ReturnsValueWithReadTimingAndContention(int address, int start, int end)
    {
        var machine = At(start);
        machine.Memory.Write((ushort)address, 0x5A);
        Assert.Equal((byte)0x5A, machine.Bus.ReadDiscarded((ushort)address));
        Assert.Equal(end, machine.TStates);
        Assert.Equal((byte)0x5A, machine.Memory.Read((ushort)address));
    }

    [Fact]
    public void FetchOpcode_TakesFourTStatesPlusContention()
    {
        var machine = At(14335);

        machine.Memory.Write(0x4000, 0x3E);

        Assert.Equal(0x3E, machine.Bus.FetchOpcode(0x4000));
        Assert.Equal(14335 + 6 + 4, machine.TStates);
    }

    [Fact]
    public void Internal_ContendsEachTState()
    {
        var machine = At(14335);

        machine.Bus.Internal(0x4000, 3);

        // 14335 +6 +1 -> 14342 (+0 +1) -> 14343 (+6 +1) -> 14350
        Assert.Equal(14350, machine.TStates);
    }

    [Fact]
    public void Internal_UncontendedAddressOnlyAddsTStates()
    {
        var machine = At(14335);

        machine.Bus.Internal(0x8000, 5);

        Assert.Equal(14340, machine.TStates);
    }

    [Theory]
    [InlineData(0x00FF, 14339)] // N:4
    [InlineData(0x00FE, 14344)] // N:1, C:3
    [InlineData(0x40FF, 14351)] // C:1, C:1, C:1, C:1
    [InlineData(0x40FE, 14345)] // C:1, C:3
    public void In_FollowsTheIoContentionTable(int port, int end)
    {
        var machine = At(14335);

        machine.Bus.In((ushort)port);

        Assert.Equal(end, machine.TStates);
    }

    [Theory]
    [InlineData(0x00FF)]
    [InlineData(0x00FE)]
    [InlineData(0x40FF)]
    [InlineData(0x40FE)]
    public void Out_OutsideTheScreenTakesFourTStates(int port)
    {
        var machine = At(0);

        machine.Bus.Out((ushort)port, 0);

        Assert.Equal(4, machine.TStates);
    }

    [Fact]
    public void UlaPort_ReadsKeyboardHalfRowsAndEar()
    {
        var machine = At(0);

        machine.SetKey(0, 0, pressed: true);

        Assert.Equal(0xBE, machine.Bus.In(0xFEFE));
        Assert.Equal(0xBF, machine.Bus.In(0x7FFE));
        Assert.Equal(0xBE, machine.Bus.In(0x00FE));

        machine.SetKey(0, 0, pressed: false);
        machine.EarInput = true;

        Assert.Equal(0xFF, machine.Bus.In(0xFEFE));
    }

    [Fact]
    public void UlaPort_WriteSetsBorderMicAndSpeaker()
    {
        var machine = At(0);

        machine.Bus.Out(0x00FE, 0x15);

        Assert.Equal(5, machine.Border);
        Assert.False(machine.Mic);
        Assert.True(machine.Speaker);
    }

    [Fact]
    public void FloatingBus_ReturnsWhatTheUlaReads()
    {
        var machine = At(0);

        machine.Memory.Write(0x4000, 0x11);
        machine.Memory.Write(0x5800, 0x22);
        machine.Memory.Write(0x4001, 0x33);
        machine.Memory.Write(0x5801, 0x44);

        // The port is read after the first T-state of the I/O cycle.
        byte ReadAt(int tstate)
        {
            machine.TStates = tstate - 1;
            return machine.Bus.In(0x00FF);
        }

        Assert.Equal(0xFF, ReadAt(14337));
        Assert.Equal(0x11, ReadAt(14338));
        Assert.Equal(0x22, ReadAt(14339));
        Assert.Equal(0x33, ReadAt(14340));
        Assert.Equal(0x44, ReadAt(14341));
        Assert.Equal(0xFF, ReadAt(14342));
        Assert.Equal(0xFF, ReadAt(100));
    }

    [Fact]
    public void Interrupt_IsActiveDuringTheFirstTStatesOfTheFrame()
    {
        Assert.True(At(31).Bus.IntActive);
        Assert.False(At(32).Bus.IntActive);
        Assert.True(At(35, SpectrumModel.Spectrum128K).Bus.IntActive);
        Assert.False(At(36, SpectrumModel.Spectrum128K).Bus.IntActive);
    }

    [Fact]
    public void AcknowledgeInterrupt_ReturnsFFAndTakesSevenTStates()
    {
        var machine = At(0);

        Assert.Equal(0xFF, machine.Bus.AcknowledgeInterrupt());
        Assert.Equal(7, machine.TStates);
    }

    [Fact]
    public void EndFrame_KeepsTheOverrun()
    {
        var machine = At(69890);

        machine.EndFrame();

        Assert.Equal(2, machine.TStates);
    }

    [Fact]
    public void Spectrum16K_ContendsTheScreenPage()
    {
        var machine = At(14335, SpectrumModel.Spectrum16K);

        machine.Bus.Read(0x4000);

        Assert.Equal(14335 + 6 + 3, machine.TStates);
    }

    [Fact]
    public void LoadRom_RejectsMissingRoms()
    {
        var machine = new SpectrumMachine(SpectrumModel.Spectrum48K);

        Assert.Throws<ArgumentOutOfRangeException>(() => machine.LoadRom(1, new byte[16]));
    }

    [Fact]
    public void LateTiming_ShiftsContention()
    {
        var machine = new SpectrumMachine(SpectrumModel.Spectrum48K, lateTiming: true) { TStates = 14336 };

        machine.Bus.Read(0x4000);

        Assert.Equal(14336 + 6 + 3, machine.TStates);
    }
}
