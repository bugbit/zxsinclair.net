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

namespace ZXSinclair.Net.Benchmarks;

// Candidate 64K memory implementations. All of them ignore writes to the ROM area (0000-3FFF),
// as a 48K Spectrum does, so the comparison measures only the storage/access strategy.

public sealed class ArrayIndexerMemory
{
    private readonly byte[] mem = new byte[0x10000];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address) => mem[address];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte data)
    {
        if (address >= 0x4000)
            mem[address] = data;
    }
}

public sealed class ArrayUnsafeMemory
{
    private readonly byte[] mem = GC.AllocateUninitializedArray<byte>(0x10000, pinned: true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address) =>
        Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(mem), address);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte data)
    {
        if (address >= 0x4000)
            Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(mem), address) = data;
    }
}

public sealed unsafe class PinnedArrayPointerMemory
{
    private readonly byte[] mem = GC.AllocateUninitializedArray<byte>(0x10000, pinned: true);
    private readonly byte* ptr;

    public PinnedArrayPointerMemory() => ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(mem));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address) => ptr[address];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte data)
    {
        if (address >= 0x4000)
            ptr[address] = data;
    }
}

public sealed unsafe class AllocHGlobalMemory : IDisposable
{
    private byte* ptr = (byte*)Marshal.AllocHGlobal(0x10000);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address) => ptr[address];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte data)
    {
        if (address >= 0x4000)
            ptr[address] = data;
    }

    public void Dispose()
    {
        if (ptr != null)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
            ptr = null;
        }
    }
}

public sealed unsafe class NativeMemoryMemory : IDisposable
{
    private byte* ptr = (byte*)NativeMemory.AlignedAlloc(0x10000, 64);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address) => ptr[address];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte data)
    {
        if (address >= 0x4000)
            ptr[address] = data;
    }

    public void Dispose()
    {
        if (ptr != null)
        {
            NativeMemory.AlignedFree(ptr);
            ptr = null;
        }
    }
}

/// <summary>
/// 128K-style memory: one array with 2 ROMs + 8 RAM banks of 16K and a table of 4 slot offsets.
/// Paging only changes an offset; no data is copied.
/// </summary>
public sealed class PagedArrayMemory
{
    public const int BankSize = 0x4000;
    public const int RomBanks = 2;
    public const int RamBanks = 8;

    private readonly byte[] mem = GC.AllocateUninitializedArray<byte>((RomBanks + RamBanks) * BankSize, pinned: true);
    private readonly int[] slots = new int[4];

    public PagedArrayMemory()
    {
        // ROM 0, RAM 5, RAM 2, RAM 0 (128K power-on layout)
        slots[0] = 0;
        slots[1] = (RomBanks + 5) * BankSize;
        slots[2] = (RomBanks + 2) * BankSize;
        slots[3] = (RomBanks + 0) * BankSize;
    }

    public void PageSlot3(int ramBank) => slots[3] = (RomBanks + (ramBank & 7)) * BankSize;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address)
    {
        var offset = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(slots), address >> 14);

        return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(mem), offset + (address & 0x3FFF));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte data)
    {
        if (address < 0x4000)
            return;

        var offset = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(slots), address >> 14);

        Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(mem), offset + (address & 0x3FFF)) = data;
    }
}

/// <summary>
/// Generic page table memory: configurable page size (shift), separate read and write offset tables.
/// Writes to ROM or unmapped pages go to a sink page, so neither read nor write branches.
/// Layout used here: 48K-like (ROM page(s) at 0000-3FFF, RAM above).
/// </summary>
public sealed class PageTableMemory
{
    private readonly byte[] mem;
    private readonly int[] readOffsets;
    private readonly int[] writeOffsets;
    private readonly int shift;
    private readonly int mask;

    public PageTableMemory(int pageShift)
    {
        var pageSize = 1 << pageShift;
        var pages = 0x10000 >> pageShift;
        var sink = 0x10000;

        shift = pageShift;
        mask = pageSize - 1;
        mem = GC.AllocateUninitializedArray<byte>(0x10000 + pageSize, pinned: true);
        readOffsets = new int[pages];
        writeOffsets = new int[pages];
        for (var p = 0; p < pages; p++)
        {
            var offset = p << pageShift;

            readOffsets[p] = offset;
            writeOffsets[p] = offset < 0x4000 ? sink : offset;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address)
    {
        var offset = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(readOffsets), address >> shift);

        return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(mem), offset + (address & mask));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte data)
    {
        var offset = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(writeOffsets), address >> shift);

        Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(mem), offset + (address & mask)) = data;
    }
}
