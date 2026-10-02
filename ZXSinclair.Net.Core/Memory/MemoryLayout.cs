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

namespace ZXSinclair.Net.Core.Memory;

/// <summary>A block of physical memory: a ROM or a RAM chip/bank.</summary>
public readonly record struct MemoryRegion(int Size, bool ReadOnly);

/// <summary>
/// Description of a 64K memory map: page size, physical regions (ROM/RAM of any size) and which region
/// each page sees. Mirrors are several pages pointing to the same region. Pages left unmapped read 0xFF
/// and ignore writes. Only used to build a <see cref="PagedMemory"/>; it is not in the hot path.
/// </summary>
public sealed class MemoryLayout
{
    public const int AddressSpaceSize = 0x10000;

    private readonly List<MemoryRegion> regions = new();
    private readonly int[] pageRegion;
    private readonly int[] pageRegionOffset;

    /// <param name="pageShift">log2 of the page size: 10 = 1K pages (ZX81), 14 = 16K pages (Spectrum).</param>
    public MemoryLayout(int pageShift)
    {
        if (pageShift < 8 || pageShift > 16)
            throw new ArgumentOutOfRangeException(nameof(pageShift));

        PageShift = pageShift;
        pageRegion = new int[PageCount];
        pageRegionOffset = new int[PageCount];
        Array.Fill(pageRegion, -1);
    }

    public int PageShift { get; }
    public int PageSize => 1 << PageShift;
    public int PageCount => AddressSpaceSize >> PageShift;
    public IReadOnlyList<MemoryRegion> Regions => regions;

    /// <summary>Region mapped at <paramref name="page"/>, or -1 if the page is unmapped.</summary>
    public int GetPageRegion(int page) => pageRegion[page];

    /// <summary>Offset inside its region of the memory seen at <paramref name="page"/>.</summary>
    public int GetPageRegionOffset(int page) => pageRegionOffset[page];

    /// <summary>Adds a ROM region and returns its index.</summary>
    public int AddRom(int size) => AddRegion(size, readOnly: true);

    /// <summary>Adds a RAM region and returns its index.</summary>
    public int AddRam(int size) => AddRegion(size, readOnly: false);

    /// <summary>
    /// Maps <paramref name="length"/> bytes starting at <paramref name="address"/> to <paramref name="region"/>,
    /// starting at <paramref name="regionOffset"/>. If the range is larger than the region it wraps around,
    /// producing mirrors (e.g. 1K of RAM mapped over 16K appears 16 times).
    /// </summary>
    public MemoryLayout Map(int address, int length, int region, int regionOffset = 0)
    {
        if ((uint)region >= (uint)regions.Count)
            throw new ArgumentOutOfRangeException(nameof(region));
        CheckRange(address, length);

        var size = regions[region].Size;

        for (var offset = 0; offset < length; offset += PageSize)
        {
            var page = (address + offset) >> PageShift;

            pageRegion[page] = region;
            pageRegionOffset[page] = (regionOffset + offset) % size;
        }

        return this;
    }

    /// <summary>Removes any mapping from the range: it will read 0xFF and ignore writes.</summary>
    public MemoryLayout Unmap(int address, int length)
    {
        CheckRange(address, length);

        for (var offset = 0; offset < length; offset += PageSize)
            pageRegion[(address + offset) >> PageShift] = -1;

        return this;
    }

    private int AddRegion(int size, bool readOnly)
    {
        if (size <= 0 || size % PageSize != 0)
            throw new ArgumentException($"Region size must be a positive multiple of the page size ({PageSize}).", nameof(size));

        regions.Add(new MemoryRegion(size, readOnly));

        return regions.Count - 1;
    }

    private void CheckRange(int address, int length)
    {
        if (address < 0 || length <= 0 || address + length > AddressSpaceSize
            || (address & (PageSize - 1)) != 0 || (length & (PageSize - 1)) != 0)
            throw new ArgumentException($"Range must be inside 64K and aligned to the page size ({PageSize}).");
    }
}
