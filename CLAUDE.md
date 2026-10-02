# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ZX Spectrum (Sinclair) emulator in C# / .NET 10, with a Blazor WebAssembly front end planned (the front end does not exist yet). Licensed under GPLv3. Every source file starts with the `#region LICENSE` GPL header block, and generated code must include it too. Code comments and TODOs are often in Spanish.

The emulator is being rewritten in `ZXSinclair.Net.Core`; all emulator code goes there. The old `ZXSinclair.Net` project has been deleted (it remains in git history). The current work is the Z80 CPU of the Core, specified in `Specs/spec-cpu-z80.md`; it has no instructions yet, and they will come from a new generator.

## Z80 Technical Reference

`Docs/z80cpu_um.pdf` is the Z80 technical manual. Consult it when implementing or verifying CPU instructions, registers, flags, and timing.

## Specifications

Specification documents go in `Specs/`, not in `Docs/` (which holds external reference material such as the Z80 manual). `Specs/spec-buses-memoria.md` specifies the Z80 bus and the Spectrum 16K/48K/128K/+2 and ZX81 memory (maps, contention, paging) for the `ZXSinclair.Net.Core` rewrite. `Specs/spec-cpu-z80.md` specifies the CPU contract (`ICpu`) and the Z80 CPU (`Z80Cpu<TBus>`: registers, execution loop, prefixes, interrupts, reset, test bus, generator contract) without instructions.

## Required C# License Header

Every C# file (`.cs`) must start with the following exact block. Include it in new files and preserve it when editing existing files. Generator templates must emit the same header. Do not place code, using directives, or other comments before it.

```csharp
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
```

## Performance is the top priority

Optimization dominates every design decision in this emulator. Very efficient, fast C# is the essence of the project and what distinguishes it from similar emulators (see README.md). It must run cycle-accurate emulation at full speed, including in Blazor WebAssembly. When abstraction or elegance conflicts with speed in the emulation hot path (instruction fetch/decode/execute, memory and bus access, T-state accounting, ULA/video), choose speed:
- No allocations, LINQ, boxing, closures or `async` in the per-instruction path.
- Minimize interface and virtual dispatch. Prefer `sealed` classes, concrete types, structs, `switch` dispatch and precomputed lookup tables (like `mTablePV` and `mTableZS53`). Use `[MethodImpl(MethodImplOptions.AggressiveInlining)]` where it helps.
- Avoid `Enum.HasFlag` on generic enums and similar hidden costs. Use bit operations on plain integers.
- Any new abstraction in the hot path (e.g. the bus redesign in `ZXSinclair.Net.Core`) must justify its cost. Back hot-path changes with measurements (BenchmarkDotNet or a timing harness).

## Commands

All projects target `net10.0`. Solution: `zxsinclair.net.slnx`.

```bash
dotnet build zxsinclair.net.slnx
```

```bash
dotnet test ZXSinclair.Net.Core.Tests
```

```bash
dotnet run --project ZXSinclair.Net.Test
```

- **`ZXSinclair.Net.Core.Tests`** is an xUnit project for the Core (memory maps, paging, contention, bus, floating bus). Run a single test with `dotnet test ZXSinclair.Net.Core.Tests --filter "FullyQualifiedName~SpectrumBusTests.In_FollowsTheIoContentionTable"`.
- **`ZXSinclair.Net.Test`** is the console FUSE Z80 runner (not xUnit), in Debug or Release. It executes `Z80Cpu<FuseTestBus>`, skips unimplemented instructions (currently all 1335 cases), and compares registers with `FuseCpuState`, plus memory/events with `FuseComparison`. `--no-events` disables event recording/comparison. Core xUnit tests use the same `ZXSinclair.Net.Fuse` library to verify the bus events and the parser against the real fixtures.
- `ZXSinclair.Net.Generate.Z80OpCodes` currently does nothing: the old generator was removed and will be rewritten.
- VS Code launch/tasks configs exist for the FUSE runner and the generator; the `build` task builds the whole solution (`.vscode/launch.json`, `.vscode/tasks.json`).
- If git reports "dubious ownership" for this directory, the user needs to add a `safe.directory` exception. Don't change global git config without asking.

## Architecture

### Projects
- **ZXSinclair.Net.Core**: the emulator (see "Core architecture" below).
- **ZXSinclair.Net.Core.Tests**: xUnit tests for the Core.
- **ZXSinclair.Net.Benchmarks**: BenchmarkDotNet benchmarks (memory strategies, Core bus). Run with `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Bus*'`.
- **ZXSinclair.Net.Generate.Z80OpCodes**: future Z80 instruction generator. Only the FUSE opcode tables remain (`data/opcodes_*.dat`, embedded resources, format `0xNN MNEMONIC args`; DD and FD share `opcodes_ddfd.dat`). The new generator must follow the contract in `Specs/spec-cpu-z80.md` section 5 and its output must not be hand-edited.
- **ZXSinclair.Net.Fuse**: library shared by the FUSE runner and the Core tests: the FUSE files (`data/tests.in`, `data/tests.expected`, embedded) and their parser (`FuseTestFile`, malformed lines throw `FormatException` with the test name), the recording `FuseTestBus`, `FuseCpuState` and `FuseComparison`.
- **ZXSinclair.Net.Test**: FUSE Z80 conformance runner (console; references the Core and `ZXSinclair.Net.Fuse`).

### Core architecture (`ZXSinclair.Net.Core`)
Implements `Specs/spec-buses-memoria.md` and the base CPU in `Specs/spec-cpu-z80.md`, without instructions.
- `ICpu`, `Z80Registers`, `Z80Flags`, `Z80Cpu<TBus>`: registers, flag tables, prefix loop, interrupts, HALT state and reset. The CPU stores its bus in a mutable field to avoid defensive copies. `Z80Cpu.Instructions.cs` holds provisional dispatches that increment `UnimplementedOpcodes`; the future generator replaces this file. Repeated DD/FD prefixes use constant stack space; unsupported IM0 bytes are counted without inventing a jump.
- CPU benchmark: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*'`. Fixed fetch/dispatch overhead is documented in spec section 8.3; it does not measure real instructions or WebAssembly.
- `Abstractions/`: generic contracts for any CPU/machine — `IBus` (clock + reset), `IBusData<TAddress, TData>` (timed bus access), `IBusIo<TPort, TData>` (I/O space), `IMemory<TAddress, TData>` (raw access), `IMemoryBuffer<TAddress, TData>` (block load/save). `Z80/IZ80Bus` combines them with the Z80 cycles (`FetchOpcode`, `Internal`, INT).
- **Buses are structs used only as generic arguments** (`where TBus : struct, IZ80Bus`): the JIT specialises generics only for struct type arguments, so a class or an interface-typed variable brings back interface dispatch. `SpectrumBus` is a `readonly struct` holding one reference to a `sealed` `SpectrumMachine` (T-state counter, memory, tables, ports); the future CPU must be `Z80Cpu<TBus>`.
- `Memory/`: `MemoryLayout` describes any 64K map (page size as a power of two, ROM/RAM regions of any size, mirrors, unmapped pages); `PagedMemory` builds it into one pinned array with separate read/write offset tables per page (ROM and unmapped writes go to a sink page, unmapped reads to a 0xFF page), so accesses never branch. `Machines/Layouts.cs` has presets for Spectrum 16K/48K/128K(+2) and ZX81 1K/16K.
- `Timing/`: `MachineTiming` presets, and precomputed per-T-state `ContentionTable` and `FloatingBusTable` (one table read per access). The machine must call `EndFrame()` before T-states exceed the frame by `ContentionTable.Margin`.
- `Machines/Spectrum/`: `SpectrumMachine` (16K, 48K, 128K, grey +2), `SpectrumBus` (memory/IO contention, ULA port 0xFE, floating bus, INT), `Spectrum128Paging` (port 0x7FFD: paging only rewrites page table entries and contention flags).

### Test data
The data files are `ZXSinclair.Net.Fuse/data/tests.in` and `tests.expected` (embedded resources), in the FUSE Z80 test suite format. Each test has a name, a register line (AF BC DE HL AF' BC' DE' HL' IX IY SP PC), a line with I R IFF1 IFF2 IM halted end-tstates, and memory blocks terminated by `-1`. In `tests.expected`, bus event lines (MC/MR/MW...) come before the registers. The runner uses a recording `FuseTestBus` (no instrumentation in the CPU), fills memory with `DE AD BE EF`, executes until `end_tstates`, and compares registers, IFF/IM/halted, T-states, memory, and every bus event (time, type, address, and optional data), unless the case contains unimplemented instructions.
