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
/// One-off cost of allocating (and releasing) the emulated memory: 64K (48K model) and 160K (128K model: 2 ROM + 8 RAM banks).
/// This happens once per machine, so it matters far less than access cost.
/// </summary>
[MemoryDiagnoser]
public class MemoryAllocationBenchmarks
{
    [Params(0x10000, 0x28000)]
    public int Size;

    [Benchmark(Baseline = true)]
    public byte[] NewArray() => new byte[Size];

    [Benchmark]
    public byte[] UninitializedPinnedArray() => GC.AllocateUninitializedArray<byte>(Size, pinned: true);

    [Benchmark]
    public void AllocHGlobalAndFree() => Marshal.FreeHGlobal(Marshal.AllocHGlobal(Size));

    [Benchmark]
    public unsafe void NativeMemoryAllocZeroedAndFree() => NativeMemory.Free(NativeMemory.AllocZeroed((nuint)Size));
}
