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
using ZXSinclair.Net.Core.Memory;

namespace ZXSinclair.Net.Core.Tests;

public class MemoryLayoutTests
{
    [Fact]
    public void Spectrum16K_UpperMemoryIsEmpty()
    {
        var memory = new PagedMemory(Layouts.Spectrum16K());

        memory.Write(0x8000, 0x12);
        memory.Write(0xFFFF, 0x34);

        Assert.Equal(0xFF, memory.Read(0x8000));
        Assert.Equal(0xFF, memory.Read(0xFFFF));
    }

    [Theory]
    [InlineData(0x4000)]
    [InlineData(0x7FFF)]
    public void Spectrum16K_RamIsWritable(int address)
    {
        var memory = new PagedMemory(Layouts.Spectrum16K());

        memory.Write((ushort)address, 0x5A);

        Assert.Equal(0x5A, memory.Read((ushort)address));
    }

    [Theory]
    [InlineData(0x4000)]
    [InlineData(0x8000)]
    [InlineData(0xFFFF)]
    public void Spectrum48K_RamIsWritable(int address)
    {
        var memory = new PagedMemory(Layouts.Spectrum48K());

        memory.Write((ushort)address, 0xA5);

        Assert.Equal(0xA5, memory.Read((ushort)address));
    }

    [Fact]
    public void Spectrum48K_RomIsReadOnly()
    {
        var memory = new PagedMemory(Layouts.Spectrum48K());

        memory.LoadRegion(Layouts.SpectrumRom, new byte[] { 0xF3, 0xAF });
        memory.Write(0x0000, 0x00);

        Assert.Equal(0xF3, memory.Read(0x0000));
        Assert.Equal(0xAF, memory.Read(0x0001));
    }

    [Fact]
    public void Zx81_1K_RamIsMirroredOverItsAreas()
    {
        var memory = new PagedMemory(Layouts.Zx81_1K());

        memory.Write(0x4000, 0x12);

        Assert.Equal(0x12, memory.Read(0x4400));
        Assert.Equal(0x12, memory.Read(0x7C00));
        Assert.Equal(0x12, memory.Read(0xC000));
        Assert.Equal(0x12, memory.Read(0xFC00));
    }

    [Fact]
    public void Zx81_RomIsMirroredAndReadOnly()
    {
        var memory = new PagedMemory(Layouts.Zx81_1K());

        memory.LoadRegion(Layouts.Zx81Rom, new byte[] { 0xD3 });
        memory.Write(0x0000, 0x00);
        memory.Write(0x2000, 0x00);

        Assert.Equal(0xD3, memory.Read(0x0000));
        Assert.Equal(0xD3, memory.Read(0x2000));
        Assert.Equal(0xD3, memory.Read(0x8000));
        Assert.Equal(0xD3, memory.Read(0xA000));
    }

    [Fact]
    public void Zx81_16K_RamIsNotRepeatedInside16K()
    {
        var memory = new PagedMemory(Layouts.Zx81_16K());

        memory.Write(0x4000, 0x11);
        memory.Write(0x7FFF, 0x22);

        Assert.Equal(0x00, memory.Read(0x4400));
        Assert.Equal(0x22, memory.Read(0xFFFF));
        Assert.Equal(0x11, memory.Read(0xC000));
    }

    [Fact]
    public void CopyFrom_FollowsTheMemoryMap()
    {
        var memory = new PagedMemory(Layouts.Spectrum48K());
        var copy = new byte[4];

        memory.CopyFrom(0x3FFE, new byte[] { 1, 2, 3, 4 });
        memory.CopyTo(0x3FFE, copy);

        Assert.Equal(new byte[] { 0, 0, 3, 4 }, copy);
    }

    [Fact]
    public void Layout_RejectsRegionsThatAreNotWholePages()
    {
        var layout = new MemoryLayout(14);

        Assert.Throws<ArgumentException>(() => layout.AddRam(0x0400));
    }

    [Fact]
    public void Layout_RejectsUnalignedRanges()
    {
        var layout = new MemoryLayout(14);
        var ram = layout.AddRam(0x4000);

        Assert.Throws<ArgumentException>(() => layout.Map(0x2000, 0x4000, ram));
    }

    [Fact]
    public void Layout_UnmappedRangeReadsFF()
    {
        var layout = Layouts.Spectrum48K().Unmap(0xC000, 0x4000);
        var memory = new PagedMemory(layout);

        memory.Write(0xC000, 0x01);

        Assert.Equal(0xFF, memory.Read(0xC000));
    }
}
