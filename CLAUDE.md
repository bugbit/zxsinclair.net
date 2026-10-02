# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ZX Spectrum (Sinclair) emulator in C# / .NET 10, with a Blazor WebAssembly front end planned (the front end does not exist yet). Licensed under GPLv3. Every source file starts with the `#region LICENSE` GPL header block, and generator templates include it too. Code comments and TODOs are often in Spanish. The current work is the Z80 CPU core.

The `ZXSinclair.Net` project is obsolete. The emulator will be rewritten in `ZXSinclair.Net.Core`; implement new emulator functionality there. Use `ZXSinclair.Net` as a reference for the legacy implementation.

## Z80 Technical Reference

`Docs/z80cpu_um.pdf` is the Z80 technical manual. Consult it when implementing or verifying CPU instructions, registers, flags, and timing.

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
dotnet run --project ZXSinclair.Net.Test
```

```bash
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes
```

- **Tests are not xUnit/NUnit.** `ZXSinclair.Net.Test` is a console app (`Program.cs`) that checks results with `Debug.Assert`, so it must run in **Debug** configuration. In Release the asserts are compiled out and nothing is checked. You can't run a single test from the CLI. To focus on one, filter `testsin` in `RunTests` (e.g. by `t.Base.Name`) while debugging.
- VS Code launch/tasks configs exist for the three runnable projects (`.vscode/launch.json`, `.vscode/tasks.json`).
- If git reports "dubious ownership" for this directory, the user needs to add a `safe.directory` exception. Don't change global git config without asking.

## Architecture

### Projects
- **ZXSinclair.Net** is the obsolete emulator implementation (currently an Exe with a placeholder `Program.cs`). Legacy hardware lives in `Hardware/`, and the Z80 is in `Hardware/Z80/`.
- **ZXSinclair.Net.Core** is the destination for the emulator rewrite and all new emulator functionality. It currently holds abstractions (`IBus`, `IBusData`, `IMemory`, `IMemoryBuffer`). No other project references it yet, and its interfaces are distinct from the similarly named ones in legacy `ZXSinclair.Net/Hardware`.
- **ZXSinclair.Net.Generate.Z80OpCodes** is a code generator that produces the Z80 opcode dispatch code. See below.
- **ZXSinclair.Net.Test** is the Z80 opcode conformance runner, using the FUSE emulator's test format.

### CPU model
- `Cpu<A, D, E, R>` (A = address type, D = data type, E = pins enum, R = register set) is the generic base. It owns an `IMemoryBuffer<D>` (raw storage), an `IMemory<A, D>` (the access layer over the buffer: RAM/ROM/Null), a register object, and `ITicks` (T-state counter).
- `Z80Cpu : Cpu<ushort, byte, Z80Pins, Z80Regs>` is a `partial` class. Timing is added inside the memory overrides. `ReadOpCode` adds 4 T-states and refreshes R. `ReadMemory` and `WriteMemory` each add 3. Instructions add any extra cycles explicitly with `Ticks.AddCycles(n)`. Tests compare exact T-state counts, so timing matters.
- `Z80Regs` uses `[StructLayout(LayoutKind.Explicit)]` with overlapping `FieldOffset`s, so 8-bit registers alias the halves of 16-bit pairs (little-endian: e.g. `F` at offset 0, `A` at offset 1 of `AF`). Alternate registers and IXH/IXL/IYH/IYL are still commented out.
- `MemoryBuffer` uses unmanaged memory (`Marshal.AllocHGlobal`) and unsafe pointers, so `AllowUnsafeBlocks` is enabled.
- The prefixes DD/FD/ED go through `InstrfetchDD/FD/ED` → `ExecOpCodeDD/FD/ED`. CB/DDCB/FDCB are not implemented yet.

### Generated code (do not hand-edit)
These files under `ZXSinclair.Net/Hardware/Z80/` are **overwritten** by the generator:
- `Z80OpCodes.cs`, `Z80OpCodesDD.cs`, `Z80OpCodesFD.cs`, `Z80OpCodesED.cs`: opcode enums
- `Z80Cpu.opcodes.cs`, `Z80Cpu.opcodesdd.cs`, `Z80Cpu.opcodesfd.cs`, `Z80Cpu.opcodesed.cs`: `ExecOpCode*` switch statements
- `Z80Regs.ld.cs`: register-to-register `Set*` helpers

To change them, edit the generator instead:
- `data/*.dat`: opcode tables (FUSE format: `0xNN MNEMONIC args`). DD and FD share `opcodes_ddfd.dat`, with `REGISTER` replaced by IX or IY.
- `templates/*.txt`: file skeletons with `{{CODE}}` and `{{BEFORE}}` placeholders.
- `Program.cs`: one generator function per mnemonic, registered in the `opcodesGenerators` dictionary (currently `NOP`, `LD`, `shift`). Each emits a call to a hand-written helper on `Z80Cpu`/`Z80Regs` (e.g. `Read_M_HL_M()`, `LD_A_I()`). A generator returns `false` for unsupported operands.
- Data files and templates are **embedded resources**, so any new one must be added to the `.csproj`.
- Output goes to `<assembly dir>/../../../../ZXSinclair.Net`, so run the generator from its default `bin/<Config>/net10.0` output (`dotnet run` does this).

To add an instruction, add or extend its generator function and any helper methods it calls in `Z80Cpu.cs`. Then rerun the generator and the test runner.

### `Z80_OPCODES_TEST` and unimplemented opcodes
In Debug, `Z80_OPCODES_TEST` is defined in both `ZXSinclair.Net` and the test project. Unimplemented opcodes get generated code that sets `instrNotImp = true`, and the test runner then **skips** that test instead of failing it. A green run therefore means only that the implemented opcodes pass.

### Test data
The data files are `ZXSinclair.Net.Test/data/tests.in` and `tests.expected` (embedded resources), in the FUSE Z80 test suite format. Each test has a name, a register line (AF BC DE HL AF' BC' DE' HL' IX IY SP PC), a line with I R IFF1 IFF2 IM halted end-tstates, and memory blocks terminated by `-1`. In `tests.expected`, bus event lines (MC/MR/MW...) come before the registers. The runner fills memory with `DE AD BE EF`, executes until `end_tstates`, then compares registers, T-states and the changed memory bytes.
