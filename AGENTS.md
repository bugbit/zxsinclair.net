# Repository Guidelines

## Project Structure & Module Organization

This C# ZX Spectrum emulator targets .NET 10. The planned Blazor front end does not exist yet.

The `ZXSinclair.Net` project is obsolete. The emulator will be rewritten in `ZXSinclair.Net.Core`; implement new emulator functionality there. Use `ZXSinclair.Net` as a reference for the legacy implementation.

- `ZXSinclair.Net/Hardware/`: obsolete CPU, memory, and timing implementations; legacy Z80 code lives in `Hardware/Z80/`.
- `ZXSinclair.Net.Core/`: destination for the emulator rewrite, with bus and memory interfaces in `Abstractions/`. This project is currently unreferenced; keep its interfaces distinct from those in legacy `Hardware/`.
- `ZXSinclair.Net.Generate.Z80OpCodes/`: generator, opcode tables in `data/`, and templates in `templates/`.
- `ZXSinclair.Net.Test/`: console test runner and embedded FUSE-format fixtures in `data/`.
- `.vscode/`: build tasks and debugger configurations.

## Z80 Technical Reference

`Docs/z80cpu_um.pdf` is the Z80 technical manual. Consult it when implementing or verifying CPU instructions, registers, flags, and timing.

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
dotnet run --project ZXSinclair.Net.Test -c Debug
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -c Debug
```

These build the solution, execute conformance checks, and regenerate opcode sources, respectively. The main executable currently prints `Hello, World!`.

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

The runner uses `Debug.Assert`, without a unit-test framework; `dotnet test` does not execute these checks. Release builds omit assertions. Add matching, identically named cases to `tests.in` and `tests.expected`, following existing opcode identifiers. Check registers, changed memory, and exact T-state counts. Unimplemented opcodes are skipped under `Z80_OPCODES_TEST`; report remaining gaps. No coverage threshold is configured.

## Commit & Pull Request Guidelines

History uses short subjects such as `bus`, `new core`, and `test opcode`; no formal convention is evident. Write concise subjects identifying changed behavior. PRs should explain changes, link relevant issues, and include build/test results and skipped-opcode limitations. Include regenerated sources with generator changes. Exclude `bin/`, `obj/`, and IDE-local artifacts.
