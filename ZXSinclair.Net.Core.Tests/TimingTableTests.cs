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

using ZXSinclair.Net.Core.Timing;

namespace ZXSinclair.Net.Core.Tests;

public class TimingTableTests
{
    [Theory]
    [InlineData(14334, 0)]
    [InlineData(14335, 6)]
    [InlineData(14336, 5)]
    [InlineData(14337, 4)]
    [InlineData(14338, 3)]
    [InlineData(14339, 2)]
    [InlineData(14340, 1)]
    [InlineData(14341, 0)]
    [InlineData(14342, 0)]
    [InlineData(14343, 6)]
    [InlineData(14460, 1)]
    [InlineData(14462, 0)]
    [InlineData(14463, 0)]
    [InlineData(14335 + 224, 6)]
    [InlineData(14335 + 191 * 224, 6)]
    [InlineData(14335 + 192 * 224, 0)]
    public void Contention48K(int tstate, int delay)
    {
        var table = ContentionTable.Build(MachineTiming.Spectrum48);

        Assert.Equal(delay, table[tstate]);
    }

    [Theory]
    [InlineData(14360, 0)]
    [InlineData(14361, 6)]
    [InlineData(14362, 5)]
    [InlineData(14361 + 228, 6)]
    [InlineData(14361 + 224, 0)]
    public void Contention128K(int tstate, int delay)
    {
        var table = ContentionTable.Build(MachineTiming.Spectrum128);

        Assert.Equal(delay, table[tstate]);
    }

    [Fact]
    public void LateTiming_ShiftsContentionOneTState()
    {
        var table = ContentionTable.Build(MachineTiming.Spectrum48.WithLateTiming());

        Assert.Equal(0, table[14335]);
        Assert.Equal(6, table[14336]);
    }

    [Fact]
    public void Tables_HaveMarginAfterTheFrame()
    {
        var timing = MachineTiming.Spectrum48;

        Assert.Equal(69888, timing.TStatesPerFrame);
        Assert.Equal(70908, MachineTiming.Spectrum128.TStatesPerFrame);
        Assert.Equal(timing.TStatesPerFrame + ContentionTable.Margin, ContentionTable.Build(timing).Length);
        Assert.Equal(timing.TStatesPerFrame + ContentionTable.Margin, FloatingBusTable.Build(timing).Length);
    }

    [Theory]
    [InlineData(14337, FloatingBusTable.Idle)]
    [InlineData(14338, 0x0000)]
    [InlineData(14339, 0x1800)]
    [InlineData(14340, 0x0001)]
    [InlineData(14341, 0x1801)]
    [InlineData(14342, FloatingBusTable.Idle)]
    [InlineData(14345, FloatingBusTable.Idle)]
    [InlineData(14346, 0x0002)]
    [InlineData(14338 + 224, 0x0100)]
    [InlineData(14338 + 8 * 224, 0x0020)]
    [InlineData(14338 + 64 * 224, 0x0800)]
    [InlineData(14338 + 191 * 224 + 3, 0x1AE1)]
    public void FloatingBus48K(int tstate, int offset)
    {
        var table = FloatingBusTable.Build(MachineTiming.Spectrum48);

        Assert.Equal(offset, table[tstate]);
    }
}
