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
/// ExecuteFrame measures NOP per opcode; the loop benchmarks measure synthetic mixes per T-state.
/// Neither measures WebAssembly.
/// </summary>
[MemoryDiagnoser]
public class Z80CpuBenchmarks
{
    private const int InstructionsPerFrame = 69888 / 4;
    private SpectrumMachine machine = null!;
    private Z80Cpu<SpectrumBus> cpu = null!;
    private SpectrumMachine aluMachine = null!;
    private Z80Cpu<SpectrumBus> aluCpu = null!;
    private SpectrumMachine blockMachine = null!;
    private Z80Cpu<SpectrumBus> blockCpu = null!;

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

        aluMachine = new SpectrumMachine(SpectrumModel.Spectrum48K);
        aluCpu = new Z80Cpu<SpectrumBus>(aluMachine.Bus);
        // Separate machine keeps the existing jump/load benchmark intact.
        // DJNZ returns to 8009; JR returns to 8000. All data is uncontended.
        byte[] aluProgram = [
            0x06, 0x10,                   // LD B,16
            0x21, 0x00, 0x90,             // LD HL,9000
            0xDD, 0x21, 0x00, 0x91,       // LD IX,9100
            0x81,                         // ADD A,C
            0xCE, 0x03,                   // ADC A,3
            0x96,                         // SUB (HL)
            0x98,                         // SBC A,B
            0xA2,                         // AND D
            0xAB,                         // XOR E
            0xBC,                         // CP H
            0x0C,                         // INC C
            0x35,                         // DEC (HL)
            0xDD, 0x86, 0x05,             // ADD A,(IX+5)
            0x10, 0xF1,                   // DJNZ 8009
            0x18, 0xE6,                   // JR 8000
        ];
        for (var i = 0; i < aluProgram.Length; i++)
            aluMachine.Memory.Write((ushort)(0x8000 + i), aluProgram[i]);
        aluMachine.Memory.Write(0x9000, 0x7F);
        aluMachine.Memory.Write(0x9105, 0x23);
        blockMachine = new SpectrumMachine(SpectrumModel.Spectrum48K);
        blockCpu = new Z80Cpu<SpectrumBus>(blockMachine.Bus);
        byte[] blockProgram = [
            0x21, 0x00, 0x90,             // LD HL,9000
            0x11, 0x00, 0xA0,             // LD DE,A000
            0x01, 0x00, 0x10,             // LD BC,1000
            0xED, 0xB0,                   // LDIR
            0x18, 0xF3,                   // JR 8000
        ];
        for (var i = 0; i < blockProgram.Length; i++)
            blockMachine.Memory.Write((ushort)(0x8000 + i), blockProgram[i]);
        for (var i = 0; i < 0x1000; i++)
            blockMachine.Memory.Write((ushort)(0x9000 + i), (byte)(i * 37 + 1));
    }

    [GlobalCleanup(Target = nameof(ExecuteAluLoopFrame))]
    public void ValidateAluLoop()
    {
        if (aluCpu.UnimplementedOpcodes != 0)
            throw new InvalidOperationException("ALU benchmark executed an unimplemented opcode.");
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

    [Benchmark(OperationsPerInvoke = 69888)]
    public int ExecuteAluLoopFrame()
    {
        aluCpu.Registers.PC = 0x8000;
        aluCpu.Registers.AF = 0x5A01;
        aluCpu.Registers.C = 0x34;
        aluCpu.Registers.DE = 0x5678;
        aluCpu.Registers.IFF1 = false;
        aluCpu.Execute(aluMachine.Timing.TStatesPerFrame);
        aluMachine.EndFrame();
        return aluCpu.Registers.PC;
    }

    [GlobalCleanup(Target = nameof(ExecuteBlockCopyFrame))]
    public void ValidateBlockCopy()
    {
        if (blockCpu.UnimplementedOpcodes != 0)
            throw new InvalidOperationException("Block benchmark executed an unimplemented opcode.");
        // One frame transfers over 3000 bytes before restarting the program.
        for (var i = 0; i < 3000; i++)
            if (blockMachine.Memory.Read((ushort)(0xA000 + i)) != (byte)(i * 37 + 1))
                throw new InvalidOperationException("Block benchmark did not copy the expected data.");
    }

    [Benchmark(OperationsPerInvoke = 69888)]
    public int ExecuteBlockCopyFrame()
    {
        blockCpu.Registers.PC = 0x8000;
        blockCpu.Registers.IFF1 = false;
        blockCpu.Execute(blockMachine.Timing.TStatesPerFrame);
        blockMachine.EndFrame();
        return blockCpu.Registers.PC;
    }
}
