# Repository Guidelines

## Project Structure & Module Organization

This C# ZX Spectrum emulator targets .NET 10. The planned Blazor front end does not exist yet.

The `ZXSinclair.Net` project is obsolete. The emulator will be rewritten in `ZXSinclair.Net.Core`; implement new emulator functionality there. Use `ZXSinclair.Net` as a reference for the legacy implementation.

- `ZXSinclair.Net/Hardware/`: obsolete CPU, memory, and timing implementations; legacy Z80 code lives in `Hardware/Z80/`.
- `ZXSinclair.Net.Core/`: destination for the emulator rewrite; implements `Specs/spec-buses-memoria.md`. Keep its interfaces distinct from those in legacy `Hardware/`.
  - `Abstractions/`: generic contracts for any CPU/machine (`IBus`, `IBusData<TAddress, TData>`, `IBusIo<TPort, TData>`, `IMemory<TAddress, TData>`, `IMemoryBuffer<TAddress, TData>`); `Z80/IZ80Bus` adds the Z80 cycles.
  - `Memory/`: `MemoryLayout` (any 64K map: page size, ROM/RAM regions, mirrors) and `PagedMemory` (one pinned array, separate read/write page tables, branch-free access). Presets in `Machines/Layouts.cs`: Spectrum 16K/48K/128K(+2), ZX81 1K/16K.
  - `Timing/`: timing presets and precomputed contention and floating-bus tables.
  - `Machines/Spectrum/`: `SpectrumMachine`, `SpectrumBus` (a `readonly struct` used only as a generic argument, `where TBus : struct, IZ80Bus`, so the JIT specialises and inlines it), `Spectrum128Paging` (port 0x7FFD).
- `ZXSinclair.Net.Core.Tests/`: xUnit tests for the Core.
- `ZXSinclair.Net.Benchmarks/`: BenchmarkDotNet benchmarks (memory strategies, Core bus).
- `ZXSinclair.Net.Generate.Z80OpCodes/`: generator, opcode tables in `data/`, and templates in `templates/`.
- `ZXSinclair.Net.Test/`: console test runner and embedded FUSE-format fixtures in `data/`.
- `.vscode/`: build tasks and debugger configurations.

## Z80 Technical Reference

`Docs/z80cpu_um.pdf` is the Z80 technical manual. Consult it when implementing or verifying CPU instructions, registers, flags, and timing.

## Specifications

Specification documents go in `Specs/`, not in `Docs/` (which holds external reference material such as the Z80 manual). `Specs/spec-buses-memoria.md` specifies the Z80 bus and the Spectrum 16K/48K/128K/+2 and ZX81 memory (maps, contention, paging) for the `ZXSinclair.Net.Core` rewrite.

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
dotnet run --project ZXSinclair.Net.Test -c Debug
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -c Debug
dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Bus*'
```

These build the solution, run the Core xUnit tests, execute the legacy CPU conformance checks, regenerate opcode sources, and run benchmarks, respectively. The main executable currently prints `Hello, World!`.

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

## Coding Style & Naming Conventions

Use four-space indentation and braces on separate lines. Follow surrounding namespace style; hardware and core generally use file-scoped namespaces. Use PascalCase for types and public members, camelCase for locals, and `I` prefixes for interfaces. Preserve Z80 notation such as `LD_A_I` and register names.

Nullable reference types and implicit usings are enabled. No repository formatter or linter is configured. Retain existing GPL license headers.

## Generated Code

Edit generator tables, templates, or instruction handlers before regenerating `Z80OpCodes*.cs`, `Z80Cpu.opcodes*.cs`, or `Z80Regs.ld.cs`; direct edits are overwritten. Register new embedded tables/templates in the generator `.csproj`. Review regenerated diffs.

## Testing Guidelines

Core code is tested with xUnit in `ZXSinclair.Net.Core.Tests` (`dotnet test`, Debug or Release); add a test for each rule of the spec you implement.

Run the legacy console FUSE runner in Debug; `dotnet test` does not execute it. It compares registers, memory, exact T-state counts, and the full ordered bus-event sequence (`MC`, `MR`, `MW`, `PC`, `PR`, `PW`). Failures report the test name and first mismatch, then continue. The summary lists passed, failed, and skipped cases; failures return a nonzero exit code. Unimplemented opcodes are skipped. Use `dotnet run --project ZXSinclair.Net.Test -c Debug -- --no-events` to disable event recording and comparison. Parser assertions still require Debug; Release execution is rejected. Add matching, identically named cases to `tests.in` and `tests.expected`. No coverage threshold is configured.

Internal instruction cycles must use `InternalCycles(address, tstates)`, rather than calling `Ticks.AddCycles` directly. Recording is compiled only under `Z80_OPCODES_TEST` and is optional when `BusEvents` is null. The legacy CPU is instrumented solely to validate existing instructions pending the Core rewrite.

## Commit & Pull Request Guidelines

History uses short subjects such as `bus`, `new core`, and `test opcode`; no formal convention is evident. Write concise subjects identifying changed behavior. PRs should explain changes, link relevant issues, and include build/test results and skipped-opcode limitations. Include regenerated sources with generator changes. Exclude `bin/`, `obj/`, and IDE-local artifacts.
