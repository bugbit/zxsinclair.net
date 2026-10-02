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

using ZXSinclair.Net.Hardware;

namespace ZXSinclair.Net.Benchmarks;

/// <summary>
/// Read + write throughput of each memory strategy over an emulator-like address stream:
/// mostly sequential runs (opcode fetch through PC) mixed with random jumps (data, stack, screen).
/// Results are per memory operation (one read + one write).
/// </summary>
[MemoryDiagnoser]
public unsafe class MemoryAccessBenchmarks
{
    private const int Operations = 0x10000;

    private ushort[] addresses = null!;
    private ArrayIndexerMemory arrayIndexer = null!;
    private ArrayUnsafeMemory arrayUnsafe = null!;
    private PinnedArrayPointerMemory pinnedPointer = null!;
    private AllocHGlobalMemory allocHGlobal = null!;
    private NativeMemoryMemory nativeMemory = null!;
    private PagedArrayMemory paged = null!;
    private IMemoryBuffer<byte> currentMemoryBuffer = null!;

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
        currentMemoryBuffer = new MemoryBuffer8Bit(0x10000);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        allocHGlobal.Dispose();
        nativeMemory.Dispose();
        currentMemoryBuffer.Dispose();
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

    /// <summary>The existing <see cref="MemoryBuffer8Bit"/> used through its interface, as Cpu does today.</summary>
    [Benchmark(OperationsPerInvoke = Operations)]
    public int CurrentMemoryBufferInterface()
    {
        var m = currentMemoryBuffer;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += m.Read(a);

            var w = (ushort)(a ^ 0x8000);

            if (w >= 0x4000)
                m.Write(w, (byte)sum);
        }

        return sum;
    }
}
