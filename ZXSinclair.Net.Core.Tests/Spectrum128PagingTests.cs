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

using ZXSinclair.Net.Core.Machines;
using ZXSinclair.Net.Core.Machines.Spectrum;

namespace ZXSinclair.Net.Core.Tests;

public class Spectrum128PagingTests
{
    private static SpectrumMachine Create(SpectrumModel model = SpectrumModel.Spectrum128K)
    {
        var machine = new SpectrumMachine(model);
        var rom0 = new byte[0x4000];
        var rom1 = new byte[0x4000];

        Array.Fill(rom0, (byte)0xA0);
        Array.Fill(rom1, (byte)0xA1);
        machine.LoadRom(0, rom0);
        machine.LoadRom(1, rom1);

        return machine;
    }

    private static void Page(SpectrumMachine machine, byte value, ushort port = 0x7FFD) => machine.Bus.Out(port, value);

    [Theory]
    [InlineData(SpectrumModel.Spectrum128K)]
    [InlineData(SpectrumModel.SpectrumPlus2)]
    public void Reset_SelectsRom0Bank0AndNormalScreen(SpectrumModel model)
    {
        var machine = Create(model);

        Assert.NotNull(machine.Paging);
        Assert.Equal(0, machine.Paging!.Value);
        Assert.Equal(0, machine.Paging.RamBank);
        Assert.Equal(5, machine.Paging.ScreenBank);
        Assert.Equal(0xA0, machine.Memory.Read(0x0000));
    }

    [Fact]
    public void Bit4_SelectsTheRom()
    {
        var machine = Create();

        Page(machine, 0x10);

        Assert.Equal(0xA1, machine.Memory.Read(0x0000));
    }

    [Fact]
    public void Bits0To2_SelectTheBankAtC000()
    {
        var machine = Create();

        Page(machine, 3);
        machine.Memory.Write(0xC000, 0x33);
        Page(machine, 0);

        Assert.Equal(0x00, machine.Memory.Read(0xC000));

        Page(machine, 3);

        Assert.Equal(0x33, machine.Memory.Read(0xC000));
    }

    [Fact]
    public void Banks5And2_AreTheSameMemoryInTwoPlaces()
    {
        var machine = Create();

        Page(machine, 5);
        machine.Memory.Write(0xC000, 0x55);
        Page(machine, 2);
        machine.Memory.Write(0xC010, 0x22);

        Assert.Equal(0x55, machine.Memory.Read(0x4000));
        Assert.Equal(0x22, machine.Memory.Read(0x8010));
    }

    [Fact]
    public void Rom_IsReadOnly()
    {
        var machine = Create();

        machine.Memory.Write(0x0000, 0x01);

        Assert.Equal(0xA0, machine.Memory.Read(0x0000));
    }

    [Fact]
    public void Bit5_LocksPagingUntilReset()
    {
        var machine = Create();

        Page(machine, 0x21);
        Page(machine, 0x03);

        Assert.Equal(1, machine.Paging!.RamBank);
        Assert.True(machine.Paging.Locked);

        machine.Reset();
        Page(machine, 0x03);

        Assert.Equal(3, machine.Paging.RamBank);
    }

    [Fact]
    public void Bit3_SelectsTheShadowScreen()
    {
        var machine = Create();

        Page(machine, 0x08);

        Assert.Equal(7, machine.Paging!.ScreenBank);
        Assert.Equal(machine.Memory.GetRegionBase(Layouts.Spectrum128RamBank(7)), machine.ScreenBase);
    }

    [Theory]
    [InlineData(0x3FFD, true)]
    [InlineData(0x7FFC, true)]
    [InlineData(0xFFFD, false)]
    [InlineData(0x7FFF, false)]
    public void Port_IsDecodedByA15AndA1(int port, bool decoded)
    {
        var machine = Create();

        Page(machine, 3, (ushort)port);

        Assert.Equal(decoded ? 3 : 0, machine.Paging!.RamBank);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    [InlineData(7, true)]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(6, false)]
    public void OddBanks_AreContendedAtC000(int bank, bool contended)
    {
        var machine = Create();

        Page(machine, (byte)bank);
        machine.TStates = 14361;
        machine.Bus.Read(0xC000);

        Assert.Equal(14361 + (contended ? 6 : 0) + 3, machine.TStates);
    }

    [Fact]
    public void FloatingBus_ReadsTheDisplayedScreen()
    {
        var machine = Create();

        machine.Memory.GetRegionSpan(Layouts.Spectrum128RamBank(5))[0] = 0x55;
        machine.Memory.GetRegionSpan(Layouts.Spectrum128RamBank(7))[0] = 0x77;

        machine.TStates = 14363;
        Assert.Equal(0x55, machine.Bus.In(0x00FF));

        Page(machine, 0x08);
        machine.TStates = 14363;
        Assert.Equal(0x77, machine.Bus.In(0x00FF));
    }
}
