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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZXSinclair.Net.Core.Abstractions;

namespace ZXSinclair.Net.Core.Memory;

/// <summary>
/// 64K address space built from a <see cref="MemoryLayout"/>. All regions live in one pinned array,
/// followed by a page filled with 0xFF (read by unmapped pages) and a sink page (written by ROM and
/// unmapped pages). Separate read and write offset tables per page mean neither reads nor writes branch.
/// Paging only rewrites table entries; memory is never copied.
/// </summary>
public sealed class PagedMemory : IMemory<ushort, byte>, IMemoryBuffer<ushort, byte>
{
    private readonly byte[] data;
    private readonly int[] readOffsets;
    private readonly int[] writeOffsets;
    private readonly int[] regionBases;
    private readonly MemoryRegion[] regions;
    private readonly int shift;
    private readonly int mask;
    private readonly int emptyPage;
    private readonly int sinkPage;

    public PagedMemory(MemoryLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var pageSize = layout.PageSize;
        var total = 0;

        regions = layout.Regions.ToArray();
        regionBases = new int[regions.Length];
        for (var r = 0; r < regions.Length; r++)
        {
            regionBases[r] = total;
            total += regions[r].Size;
        }

        emptyPage = total;
        sinkPage = total + pageSize;
        data = GC.AllocateArray<byte>(total + 2 * pageSize, pinned: true);
        data.AsSpan(emptyPage, pageSize).Fill(0xFF);

        shift = layout.PageShift;
        mask = pageSize - 1;
        readOffsets = new int[layout.PageCount];
        writeOffsets = new int[layout.PageCount];
        for (var page = 0; page < layout.PageCount; page++)
        {
            var region = layout.GetPageRegion(page);

            if (region < 0)
                UnmapPage(page);
            else
                MapPage(page, region, layout.GetPageRegionOffset(page));
        }
    }

    public int PageShift => shift;
    public int PageSize => mask + 1;
    public int PageCount => readOffsets.Length;
    public int RegionCount => regions.Length;
    public int Size => MemoryLayout.AddressSpaceSize;

    public MemoryRegion GetRegion(int region) => regions[region];

    /// <summary>Physical memory of a region (for ROM loading, the ULA reading the screen, snapshots).</summary>
    public Span<byte> GetRegionSpan(int region) => data.AsSpan(regionBases[region], regions[region].Size);

    /// <summary>Offset of a region inside the physical array (see <see cref="ReadPhysical"/>).</summary>
    public int GetRegionBase(int region) => regionBases[region];

    /// <summary>Reads physical memory directly, bypassing the page tables (e.g. the ULA reading the screen).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadPhysical(int offset) => Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(data), offset);

    /// <summary>Copies <paramref name="content"/> into a region, ignoring its read-only flag (loading a ROM).</summary>
    public void LoadRegion(int region, ReadOnlySpan<byte> content)
    {
        if (content.Length > regions[region].Size)
            throw new ArgumentException("Content is larger than the region.", nameof(content));

        content.CopyTo(GetRegionSpan(region));
    }

    /// <summary>Maps <paramref name="page"/> to <paramref name="region"/> at <paramref name="regionOffset"/>.</summary>
    public void MapPage(int page, int region, int regionOffset = 0)
    {
        if (regionOffset < 0 || regionOffset + PageSize > regions[region].Size || (regionOffset & mask) != 0)
            throw new ArgumentOutOfRangeException(nameof(regionOffset));

        var physical = regionBases[region] + regionOffset;

        readOffsets[page] = physical;
        writeOffsets[page] = regions[region].ReadOnly ? sinkPage : physical;
    }

    /// <summary>Leaves <paramref name="page"/> without memory: reads return 0xFF, writes are lost.</summary>
    public void UnmapPage(int page)
    {
        readOffsets[page] = emptyPage;
        writeOffsets[page] = sinkPage;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address)
    {
        var page = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(readOffsets), address >> shift);

        return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(data), page + (address & mask));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte value)
    {
        var page = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(writeOffsets), address >> shift);

        Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(data), page + (address & mask)) = value;
    }

    public void CopyFrom(ushort address, ReadOnlySpan<byte> content)
    {
        for (var i = 0; i < content.Length; i++)
            Write((ushort)(address + i), content[i]);
    }

    public void CopyTo(ushort address, Span<byte> destination)
    {
        for (var i = 0; i < destination.Length; i++)
            destination[i] = Read((ushort)(address + i));
    }
}
