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

/// <summary>
/// Read + write throughput of each memory strategy over an emulator-like address stream:
/// mostly sequential runs (opcode fetch through PC) mixed with random jumps (data, stack, screen).
/// Results are per memory operation (one read + one write).
/// </summary>
[MemoryDiagnoser]
public class MemoryAccessBenchmarks
{
    private const int Operations = 0x10000;

    private ushort[] addresses = null!;
    private ArrayIndexerMemory arrayIndexer = null!;
    private ArrayUnsafeMemory arrayUnsafe = null!;
    private PinnedArrayPointerMemory pinnedPointer = null!;
    private AllocHGlobalMemory allocHGlobal = null!;
    private NativeMemoryMemory nativeMemory = null!;
    private PagedArrayMemory paged = null!;
    private PageTableMemory pageTable1K = null!;
    private PageTableMemory pageTable16K = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(48);
        var pc = 0;

        addresses = new ushort[Operations];
        for (var i = 0; i < Operations; i++)
        {
            if (random.Next(10) < 3)
                pc = random.Next(0x10000);
            else
                pc = (pc + 1) & 0xFFFF;
            addresses[i] = (ushort)pc;
        }

        arrayIndexer = new ArrayIndexerMemory();
        arrayUnsafe = new ArrayUnsafeMemory();
        pinnedPointer = new PinnedArrayPointerMemory();
        allocHGlobal = new AllocHGlobalMemory();
        nativeMemory = new NativeMemoryMemory();
        paged = new PagedArrayMemory();
        pageTable1K = new PageTableMemory(10);
        pageTable16K = new PageTableMemory(14);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        allocHGlobal.Dispose();
        nativeMemory.Dispose();
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = Operations)]
    public int ArrayIndexer()
    {
        var m = arrayIndexer;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);
            m.Write((ushort)(a ^ 0x8000), (byte)sum);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = Operations)]
    public int ArrayUnsafeAdd()
    {
        var m = arrayUnsafe;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);
            m.Write((ushort)(a ^ 0x8000), (byte)sum);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = Operations)]
    public int PinnedArrayPointer()
    {
        var m = pinnedPointer;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);
            m.Write((ushort)(a ^ 0x8000), (byte)sum);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = Operations)]
    public int AllocHGlobal()
    {
        var m = allocHGlobal;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);
            m.Write((ushort)(a ^ 0x8000), (byte)sum);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = Operations)]
    public int NativeMemoryAlloc()
    {
        var m = nativeMemory;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);
            m.Write((ushort)(a ^ 0x8000), (byte)sum);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = Operations)]
    public int PagedArray128K()
    {
        var m = paged;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);
            m.Write((ushort)(a ^ 0x8000), (byte)sum);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = Operations)]
    public int PageTable1K()
    {
        var m = pageTable1K;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);
            m.Write((ushort)(a ^ 0x8000), (byte)sum);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = Operations)]
    public int PageTable16K()
    {
        var m = pageTable16K;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);
            m.Write((ushort)(a ^ 0x8000), (byte)sum);
        }

        return sum;
    }
}
