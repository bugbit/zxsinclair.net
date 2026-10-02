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
/// Fixed CPU fetch/dispatch/interrupt-check cost, in ns per unimplemented opcode.
/// This frame starts at PC=0: the contended addresses are reached after the display interval.
/// It is not an instruction-mix benchmark or a WebAssembly measurement.
/// </summary>
[MemoryDiagnoser]
public class Z80CpuBenchmarks
{
    private const int InstructionsPerFrame = 69888 / 4;
    private SpectrumMachine machine = null!;
    private Z80Cpu<SpectrumBus> cpu = null!;

    [GlobalSetup]
    public void Setup()
    {
        machine = new SpectrumMachine(SpectrumModel.Spectrum48K);
        cpu = new Z80Cpu<SpectrumBus>(machine.Bus);
    }

    [Benchmark(OperationsPerInvoke = InstructionsPerFrame)]
    public int ExecuteFrame()
    {
        cpu.Registers.PC = 0;
        cpu.Execute(machine.Timing.TStatesPerFrame);
        machine.EndFrame();
        return cpu.UnimplementedOpcodes;
    }
}
