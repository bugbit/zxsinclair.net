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
/// CPU cost including fetch, dispatch and interrupt checks.
/// This frame starts at PC=0: the contended addresses are reached after the display interval.
/// ExecuteFrame measures NOP per opcode; ExecuteLoopFrame measures a synthetic mix per T-state.
/// Neither measures WebAssembly.
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
        // Uncontended RAM. CALL targets 8012; DJNZ returns to 8005; JR returns to 8000.
        byte[] program = [
            0x06, 0x10,                   // LD B,16
            0x21, 0x00, 0x90,             // LD HL,9000
            0x3A, 0x00, 0x91,             // LD A,(9100)
            0x77,                         // LD (HL),A
            0xC5,                         // PUSH BC
            0xCD, 0x12, 0x80,             // CALL 8012
            0xC1,                         // POP BC
            0x10, 0xF5,                   // DJNZ 8005
            0x18, 0xEE,                   // JR 8000
            0x4F,                         // LD C,A
            0x32, 0x01, 0x91,             // LD (9101),A
            0xC9,                         // RET
        ];
        for (var i = 0; i < program.Length; i++)
            machine.Memory.Write((ushort)(0x8000 + i), program[i]);
        machine.Memory.Write(0x9100, 0x5A);
    }

    [Benchmark(OperationsPerInvoke = InstructionsPerFrame)]
    public int ExecuteFrame()
    {
        cpu.Registers.PC = 0;
        cpu.Execute(machine.Timing.TStatesPerFrame);
        machine.EndFrame();
        return cpu.Registers.PC;
    }

    [Benchmark(OperationsPerInvoke = 69888)]
    public int ExecuteLoopFrame()
    {
        cpu.Registers.PC = 0x8000;
        cpu.Registers.SP = 0xFF00;
        cpu.Registers.IFF1 = false;
        cpu.Execute(machine.Timing.TStatesPerFrame);
        machine.EndFrame();
        return cpu.Registers.PC;
    }
}
