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
using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Benchmarks;

/// <summary>
/// Cost of a read + write through the ZXSinclair.Net.Core Spectrum bus (T-state accounting, contention,
/// page tables), over the same emulator-like address stream as <see cref="MemoryAccessBenchmarks"/>.
/// The bus is passed as a struct generic argument, as the CPU will do; the interface variant shows the
/// cost of calling it through <see cref="IZ80Bus"/> instead.
/// </summary>
[MemoryDiagnoser]
public class BusBenchmarks
{
    private const int Operations = 0x10000;

    private ushort[] addresses = null!;
    private ArrayUnsafeMemory arrayUnsafe = null!;
    private SpectrumMachine machine48 = null!;
    private SpectrumMachine machine128 = null!;

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

        arrayUnsafe = new ArrayUnsafeMemory();
        machine48 = new SpectrumMachine(SpectrumModel.Spectrum48K);
        machine128 = new SpectrumMachine(SpectrumModel.Spectrum128K);
        machine128.Bus.Out(0x7FFD, 1); // contended bank at C000
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = Operations)]
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
    public int Spectrum48Bus() => Run(machine48.Bus, machine48, addresses);

    [Benchmark(OperationsPerInvoke = Operations)]
    public int Spectrum128Bus() => Run(machine128.Bus, machine128, addresses);

    [Benchmark(OperationsPerInvoke = Operations)]
    public int Spectrum48BusThroughInterface() => RunInterface(machine48.Bus, machine48, addresses);

    private static int Run<TBus>(TBus bus, SpectrumMachine machine, ushort[] addresses)
        where TBus : struct, IZ80Bus
    {
        var frame = machine.Timing.TStatesPerFrame;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += bus.Read(a);
            bus.Write((ushort)(a ^ 0x8000), (byte)sum);
            if (bus.Cycles >= frame)
                machine.EndFrame();
        }

        return sum;
    }

    private static int RunInterface(IZ80Bus bus, SpectrumMachine machine, ushort[] addresses)
    {
        var frame = machine.Timing.TStatesPerFrame;
        var sum = 0;

        foreach (var a in addresses)
        {
            sum += bus.Read(a);
            bus.Write((ushort)(a ^ 0x8000), (byte)sum);
            if (bus.Cycles >= frame)
                machine.EndFrame();
        }

        return sum;
    }
}
