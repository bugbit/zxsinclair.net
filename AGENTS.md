# Repository Guidelines

## Project Structure & Module Organization

This C# ZX Spectrum emulator targets .NET 10. The planned Blazor front end does not exist yet.

The emulator is being rewritten in `ZXSinclair.Net.Core`; all emulator code goes there. The old `ZXSinclair.Net` project has been deleted (it remains in git history). The Z80 CPU of the Core is specified in `Specs/spec-cpu-z80.md` and currently implements NOP and the 8-bit and 16-bit load groups.

- `ZXSinclair.Net.Core/`: destination for the emulator rewrite; implements `Specs/spec-buses-memoria.md` and the base CPU in `Specs/spec-cpu-z80.md`, with NOP and the 8-bit and 16-bit load groups implemented.
  - `Abstractions/`: generic contracts for any CPU/machine (`IBus`, `IBusData<TAddress, TData>`, `IBusIo<TPort, TData>`, `IMemory<TAddress, TData>`, `IMemoryBuffer<TAddress, TData>`); `Z80/IZ80Bus` adds the Z80 cycles.
  - `Abstractions/ICpu`, `Z80/Z80Cpu<TBus>`: CPU contract, registers, flag tables, prefix loop, interrupts, HALT state and reset. The bus field is mutable to avoid defensive struct copies. The five dispatches in `Z80/Generated/` are emitted by the generator; NOP and the 8-bit and 16-bit load groups are implemented and other opcodes increment `UnimplementedOpcodes`.
  - `Memory/`: `MemoryLayout` (any 64K map: page size, ROM/RAM regions, mirrors) and `PagedMemory` (one pinned array, separate read/write page tables, branch-free access). Presets in `Machines/Layouts.cs`: Spectrum 16K/48K/128K(+2), ZX81 1K/16K.
  - `Timing/`: timing presets and precomputed contention and floating-bus tables.
  - `Machines/Spectrum/`: `SpectrumMachine`, `SpectrumBus` (a `readonly struct` used only as a generic argument, `where TBus : struct, IZ80Bus`, so the JIT specialises and inlines it), `Spectrum128Paging` (port 0x7FFD).
- `ZXSinclair.Net.Core.Tests/`: xUnit tests for the Core.
- `ZXSinclair.Net.Generate.Z80OpCodes.Tests/`: xUnit tests for the generator (parser, operands, aliases, patterns, deterministic output and CLI checks); no Core reference.
- `ZXSinclair.Net.Benchmarks/`: BenchmarkDotNet benchmarks (memory strategies, Core bus).
- `ZXSinclair.Net.Generate.Z80OpCodes/`: Z80 instruction generator; reads five embedded FUSE tables in `data/` and emits five dispatch files in `ZXSinclair.Net.Core/Z80/Generated/`, using a pattern catalog with NOP, 12 patterns for 8-bit loads and 6 patterns for 16-bit loads. It does not reference the Core and follows `Specs/spec-cpu-z80.md` section 5 and the indexed-opcode rule in `Specs/spec-instr-nop.md` section 4.1.
- `ZXSinclair.Net.Fuse/`: library shared by the FUSE runner and the Core tests: embedded FUSE fixtures in `data/`, their parser (`FuseTestFile`), the recording `FuseTestBus`, `FuseCpuState` and `FuseComparison`.
- `ZXSinclair.Net.Test/`: FUSE console runner (references the Core and `ZXSinclair.Net.Fuse`).
- `.vscode/`: build tasks and debugger configurations.

## Z80 Technical Reference

`Docs/z80cpu_um.pdf` is the Z80 technical manual. Consult it when implementing or verifying CPU instructions, registers, flags, and timing.

## Specifications

Specification documents go in `Specs/`, not in `Docs/` (which holds external reference material such as the Z80 manual). `Specs/spec-buses-memoria.md` specifies the Z80 bus and the Spectrum 16K/48K/128K/+2 and ZX81 memory (maps, contention, paging) for the `ZXSinclair.Net.Core` rewrite. `Specs/spec-cpu-z80.md` specifies the CPU contract (`ICpu`) and the Z80 CPU (`Z80Cpu<TBus>`) with NOP and the 8-bit and 16-bit load groups implemented. `Specs/spec-instr-nop.md` specifies `NOP`, the pilot instruction (including the rule that opcodes absent from `opcodes_ddfd.dat` are generated in `ExecuteIndexedOpcode` with the same body as in `ExecuteMain`). `Specs/spec-proceso-instrucciones.md` is the process guide for instructions: one spec and plan per Zilog manual group, implemented by generator patterns, in a fixed order starting with the generator spec, plus a progress table. `Specs/spec-generador-z80.md` specifies the generator (FUSE tables, including `opcodes_cb.dat`/`opcodes_ddfdcb.dat` restored from git history, aliases, pattern catalog per group, five generated files in `ZXSinclair.Net.Core/Z80/Generated/`, `--check`, and its own test project). `Specs/spec-instr-carga-8.md` specifies group 1 (8-bit loads, including undocumented IXH/IXL forms, WZ rules and the `ReadPc16`/`IndexedAddress<TIndex>` helpers). `Specs/spec-instr-carga-16.md` specifies group 2 (16-bit loads, `LD SP,HL`, `PUSH`/`POP`, undocumented `ED 63`/`ED 6B`, and the `LoadWordAbsolute`/`StoreWordAbsolute`/`PushWithDelay` helpers).

## Performance Is the Top Priority

Optimization dominates every design decision in this emulator. Very efficient, fast C# is the essence of the project and what distinguishes it from similar emulators (see README.md). It must run cycle-accurate emulation at full speed, including in Blazor WebAssembly. When abstraction or elegance conflicts with speed in the emulation hot path (instruction fetch/decode/execute, memory and bus access, T-state accounting, ULA/video), choose speed:

- No allocations, LINQ, boxing, closures, or `async` in the per-instruction path.
- Minimize interface and virtual dispatch. Prefer `sealed` classes, concrete types, structs, `switch` dispatch, and precomputed lookup tables (like `mTablePV` and `mTableZS53`). Use `[MethodImpl(MethodImplOptions.AggressiveInlining)]` where it helps.
- Avoid `Enum.HasFlag` on generic enums and similar hidden costs. Use bit operations on plain integers.
- Any new abstraction in the hot path (e.g. the bus redesign in `ZXSinclair.Net.Core`) must justify its cost. Back hot-path changes with measurements (BenchmarkDotNet or a timing harness), and report them in PRs.

## Build, Test, and Development Commands

Run from the repository root with an SDK supporting `net10.0` and the .NET 10 runtime:

```sh
dotnet build zxsinclair.net.slnx -c Debug -m:1
dotnet test ZXSinclair.Net.Core.Tests
dotnet run --project ZXSinclair.Net.Test
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -c Debug
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check
dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests
dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Bus*'
```

These commands build the solution, run Core and generator tests, execute the FUSE runner (currently 201 passed cases, 0 failures and 1134 skipped cases), generate and check dispatches, and run bus benchmarks. Generator options: `--output <dir>`, `--check`, `--verbose` (after `--`); exit codes: 0 success, 1 differences, 2 generator/argument/I/O error. CPU benchmark: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*'`. It measures NOP including fetch, dispatch and interrupt checks: 3.769 ns/opcode, 0 B allocated, versus 3.765 ns before the 16-bit load group, with overlapping confidence intervals (2026-10-02, spec section 8.4). The base dispatch returns early for implemented opcode 00 with an empty body.

## Required C# License Header

Every C# file (`.cs`) must start with the following exact block. Include it in new files and preserve it when editing existing files. Generated code must emit the same header. Do not place code, using directives, or other comments before it.

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

## Coding Style & Naming Conventions

Use four-space indentation and braces on separate lines. Follow surrounding namespace style; hardware and core generally use file-scoped namespaces. Use PascalCase for types and public members, camelCase for locals, and `I` prefixes for interfaces. Preserve Z80 notation such as `LD_A_I` and register names.

Nullable reference types and implicit usings are enabled. No repository formatter or linter is configured. Retain existing GPL license headers.

## Generated Code

The generator produces `partial` files of `Z80Cpu<TBus>` (`Specs/spec-cpu-z80.md`, section 5) from `data/opcodes_*.dat`; generated files must not be edited by hand. Register new embedded tables in the generator `.csproj` and review regenerated diffs.

## Testing Guidelines

Core code is tested with xUnit in `ZXSinclair.Net.Core.Tests` (`dotnet test`, Debug or Release); add a test for each rule of the spec you implement.

Every instruction implementation must include, besides passing its FUSE cases, its own xUnit tests in `ZXSinclair.Net.Core.Tests`, covering the rules of its spec that FUSE does not check on its own (timing and bus cycles, flags, R, PC wraparound, DD/FD variants, interrupt acceptance afterwards, `UnimplementedOpcodes` unchanged). FUSE passing alone is not enough.

Whenever an instruction is implemented and verified, update [Specs/estado-instrucciones-z80.md](Specs/estado-instrucciones-z80.md) in the same change: mark its entry as completed (`[x]`), add links to its specification and tests, and update the completed/pending counts. Mark an entry only when all its documented variants are implemented and their own xUnit tests and applicable FUSE cases pass. For partial implementations, leave the checkbox unchecked and note the available variants.

Run the FUSE console runner (Debug or Release); `dotnet test` does not execute it. Malformed FUSE files throw `FormatException` with the test name. It runs `Z80Cpu<FuseTestBus>` on a recording bus (no test instrumentation inside the CPU), skips cases with unimplemented opcodes, and compares registers through `FuseCpuState`, plus memory and the full ordered bus-event sequence (`MC`, `MR`, `MW`, `PC`, `PR`, `PW`) through `FuseComparison`. `--no-events` disables event recording/comparison. Core xUnit tests use the same `ZXSinclair.Net.Fuse` library to verify the recording bus and the parser against the real fixtures. Add matching, identically named cases to `tests.in` and `tests.expected`.

Instructions emit all their timing through the bus (`FetchOpcode`, `Read`, `Write`, `Internal(address, n)`, `In`, `Out`); never add cycles directly.

## Commit & Pull Request Guidelines

History uses short subjects such as `bus`, `new core`, and `test opcode`; no formal convention is evident. Write concise subjects identifying changed behavior. PRs should explain changes, link relevant issues, and include build/test results and skipped-opcode limitations. Include regenerated sources with generator changes. Exclude `bin/`, `obj/`, and IDE-local artifacts.
