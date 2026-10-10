# Repository Guidelines

## Project Structure & Module Organization

This C# ZX Spectrum emulator targets .NET 10. The planned Blazor front end does not exist yet.

The emulator is being rewritten in `ZXSinclair.Net.Core`; all emulator code goes there. The old `ZXSinclair.Net` project has been deleted (it remains in git history). The Z80 CPU of the Core is specified in `Specs/spec-cpu-z80.md` and currently implements NOP, the 8-bit and 16-bit load groups, the jump, call and return group, the 8-bit ALU group, the general arithmetic and CPU control group, the 16-bit ALU group, the rotation and shift group, the bit set, reset and test group, the exchange, block transfer and search group, and the input and output group.

- `ZXSinclair.Net.Core/`: destination for the emulator rewrite; implements `Specs/spec-buses-memoria.md` and the base CPU in `Specs/spec-cpu-z80.md`, with NOP, the 8-bit and 16-bit load groups, the jump, call and return group, the 8-bit ALU group, the general arithmetic and CPU control group, the 16-bit ALU group, the rotation and shift group, the bit set, reset and test group, the exchange, block transfer and search group, and the input and output group implemented.
  - `Abstractions/`: generic contracts for any CPU/machine (`IBus`, `IBusData<TAddress, TData>`, `IBusIo<TPort, TData>`, `IMemory<TAddress, TData>`, `IMemoryBuffer<TAddress, TData>`); `Z80/IZ80Bus` adds the Z80 cycles.
  - `Abstractions/ICpu`, `Z80/Z80Cpu<TBus>`: CPU contract, registers, flag tables, prefix loop, interrupts, HALT state and reset. The bus field is mutable to avoid defensive struct copies. The five dispatches in `Z80/Generated/` are emitted by the generator; NOP, the 8-bit and 16-bit load groups, the jump, call and return group, the 8-bit ALU group, the general arithmetic and CPU control group, the 16-bit ALU group, the rotation and shift group, the bit set, reset and test group, the exchange, block transfer and search group, and the input and output group are implemented. Group 11 also implements the 178 ED holes as two NOPs; `UnimplementedOpcodes` remains for unsupported IM 0 responses (multi-byte opcodes or prefixes).
  - `Memory/`: `MemoryLayout` (any 64K map: page size, ROM/RAM regions, mirrors) and `PagedMemory` (one pinned array, separate read/write page tables, branch-free access). Presets in `Machines/Layouts.cs`: Spectrum 16K/48K/128K(+2), ZX81 1K/16K.
  - `Timing/`: timing presets and precomputed contention and floating-bus tables.
  - `Machines/Spectrum/`: `SpectrumMachine`, `SpectrumBus` (a `readonly struct` used only as a generic argument, `where TBus : struct, IZ80Bus`, so the JIT specialises and inlines it), `Spectrum128Paging` (port 0x7FFD).
- `ZXSinclair.Net.Core.Tests/`: xUnit tests for the Core.
- `ZXSinclair.Net.Generate.Z80OpCodes.Tests/`: xUnit tests for the generator (parser, operands, aliases, patterns, deterministic output and CLI checks); no Core reference.
- `ZXSinclair.Net.Benchmarks/`: BenchmarkDotNet benchmarks (memory strategies, Core bus).
- `ZXSinclair.Net.Generate.Z80OpCodes/`: Z80 instruction generator; reads five embedded FUSE tables in `data/` and emits five dispatch files plus the single-byte IM 0 classification table in `ZXSinclair.Net.Core/Z80/Generated/`, using a pattern catalog with NOP, 12 patterns for 8-bit loads, 6 patterns for 16-bit loads, 9 patterns for jumps, calls and returns, 3 patterns for 8-bit ALU, 9 patterns for general arithmetic and CPU control, 3 patterns for 16-bit ALU, 5 patterns for rotations and shifts, 5 patterns for bit operations, 4 patterns for exchanges and blocks, and 5 patterns for I/O. It does not reference the Core and follows `Specs/spec-cpu-z80.md` section 5 and the indexed-opcode rule in `Specs/spec-instr-nop.md` section 4.1.
- `ZXSinclair.Net.Fuse/`: library shared by the FUSE runner and the Core tests: embedded FUSE fixtures in `data/`, their parser (`FuseTestFile`), the recording `FuseTestBus`, `FuseCpuState` and `FuseComparison`.
- `ZXSinclair.Net.Test/`: FUSE console runner (references the Core and `ZXSinclair.Net.Fuse`).
- `.vscode/`: build tasks and debugger configurations.

## Z80 Technical Reference

`Docs/z80cpu_um.pdf` is the Z80 technical manual. `Docs/z80-no-documentado.md` summarises undocumented opcodes, flags (F5/F3, `Q`, interrupted block instructions), MEMPTR rules, R, interrupts and model differences from external sources, with a table of what this project implements. `Specs/spec-correcciones-no-documentado.md` specifies (and the Core implements) two fixes from that comparison: `Q = 0` after a `DD`/`FD` prefix (so prefixed `SCF`/`CCF` see no flag history) and `WZ = PC + 1` when `INxR`/`OTxR` repeat. `Specs/spec-correcciones-revision.md` specifies fixes from the instruction code review: a shared `RepeatBlock` step for repeating block instructions, the corrected `BIT b,r` F5/F3 rule in the undocumented reference, and a JIT-inlining investigation of the generated base dispatch with an `ExecuteMixFrame` benchmark (53 distinct base opcodes); all three are implemented, and the investigation kept the current base dispatch (spec CPU 8.4). `Specs/spec-correcciones-revision-2.md` specifies the remaining review fixes: read-only `Z80Flags` tables, an explicit `Opcode.Length` driving the IM 0 table, and a shared `IsInstruction` guard in every pattern; all three are implemented (`Z80Flags` tables are `ReadOnlySpan<byte>` properties over private arrays). Consult it when implementing or verifying CPU instructions, registers, flags, and timing.

## Specifications

Specification documents go in `Specs/`, not in `Docs/` (which holds external reference material such as the Z80 manual). `Specs/spec-buses-memoria.md` specifies the Z80 bus and the Spectrum 16K/48K/128K/+2 and ZX81 memory (maps, contention, paging) for the `ZXSinclair.Net.Core` rewrite. `Specs/spec-cpu-z80.md` specifies the CPU contract (`ICpu`) and the Z80 CPU (`Z80Cpu<TBus>`) with NOP, the 8-bit and 16-bit load groups, the jump, call and return group, the 8-bit ALU group, the general arithmetic and CPU control group, the 16-bit ALU group, the rotation and shift group, the bit set, reset and test group, the exchange, block transfer and search group, and the input and output group implemented. `Specs/spec-instr-nop.md` specifies `NOP`, the pilot instruction (including the rule that opcodes absent from `opcodes_ddfd.dat` are generated in `ExecuteIndexedOpcode` with the same body as in `ExecuteMain`). `Specs/spec-proceso-instrucciones.md` is the process guide for instructions: one spec and plan per Zilog manual group, implemented by generator patterns, in a fixed order starting with the generator spec, plus a progress table. `Specs/spec-generador-z80.md` specifies the generator (FUSE tables, including `opcodes_cb.dat`/`opcodes_ddfdcb.dat` restored from git history, aliases, pattern catalog per group, five generated files in `ZXSinclair.Net.Core/Z80/Generated/`, `--check`, and its own test project). `Specs/spec-instr-carga-8.md` specifies group 1 (8-bit loads, including undocumented IXH/IXL forms, WZ rules and the `ReadPc16`/`IndexedAddress<TIndex>` helpers). `Specs/spec-instr-carga-16.md` specifies group 2 (16-bit loads, `LD SP,HL`, `PUSH`/`POP`, undocumented `ED 63`/`ED 6B`, and the `LoadWordAbsolute`/`StoreWordAbsolute`/`PushWithDelay` helpers). `Specs/spec-instr-saltos.md` specifies group 3 (jumps, calls, returns, `RETN`/`RETI` aliases, `RST`, the `Z80Cpu.Jumps.cs` helpers and the synthetic `ExecuteLoopFrame` benchmark). `Specs/spec-instr-alu-8.md` specifies group 4 (8-bit ALU and `INC`/`DEC`, branch-free flag formulas, new `Z80Flags.Inc`/`Dec` tables, exhaustive reference-model tests and the `ExecuteAluLoopFrame` benchmark). `Specs/spec-instr-control.md` specifies group 5 (`DAA`, `CPL`, `NEG`, `SCF`, `CCF`, `HALT`, `DI`/`EI`, `IM` with ED aliases) and the `Q` register: the generator writes `Q = F` after flag-writing patterns (`IPattern.WritesFlags`) and `Q = 0` otherwise; `FuseCpuState.Load` sets `Q = F` to match FUSE's `SCF`/`CCF`. `Specs/spec-instr-alu-16.md` specifies group 6 (16-bit `ADD`/`ADC`/`SBC`, `ADD IX/IY`, `INC`/`DEC` of pairs, branch-free flag formulas, `WZ = HL + 1`, `Q` via `WritesFlags`). `Specs/spec-instr-rotaciones.md` specifies group 7 (rotations and shifts in base/CB/DDCB tables, undocumented `SLL` and DDCB copies to registers, `RLD`/`RRD`, `WZ = ii + d` set once in `FinishIndexed`). `Specs/spec-instr-bits.md` specifies group 8 (`BIT`/`SET`/`RES` on registers, `(HL)` and `(ii+d)`, DDCB `BIT` aliases and undocumented `SET`/`RES` copies, `BIT` F5/F3 from the register, MEMPTR or `ii + d`; completes the CB and DDFDCB tables). `Specs/spec-instr-bloques.md` specifies group 9 (`EX`, `EXX`, `EX (SP),HL/IX/IY`, `LDI`/`LDIR`/`LDD`/`LDDR`, `CPI`/`CPIR`/`CPD`/`CPDR`; each repeat is a whole instruction with `PC -= 2`, internal cycles on the written `DE` or read `HL`, and the `ExecuteBlockCopyFrame` benchmark). `Specs/spec-instr-io.md` specifies group 10 (`IN`/`OUT`, undocumented `IN F,(C)`/`OUT (C),0`, block I/O with classic flags plus the repeat F5/F3 and H/P/V adjustments, I/O WZ rules, the extended `TestBus` I/O recording, and the optional ROM-boot benchmark); after it all 1335 FUSE cases run. `Specs/spec-instr-restos.md` specifies group 11 (ED holes as two NOPs via `EdHolePattern`, IM 0 execution of single-byte base opcodes using a generated `Z80Cpu.Im0.g.cs` table, multi-byte IM 0 responses left unsupported, and the NMOS P/V reset when an INT follows `LD A,I`/`LD A,R`).

Q is emitted after every implemented instruction: F for flag writers, zero otherwise. SCF/CCF use the previous Q; interrupt acceptance and repeated HALT M1s clear it. The measured before/after intervals overlap in all three CPU benchmarks (spec section 8.4).

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
dotnet run --project ZXSinclair.Net.Test -- --filter 20_2 --verbose
dotnet run --project ZXSinclair.Net.Test -- --filter dd --list-skipped --max-failures 1
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -c Debug
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check
dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests
dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Bus*'
```

These commands build the solution, run Core and generator tests, execute the FUSE runner (currently 1335 passed cases (8 with the declared BIT (HL) convention), 0 failures and 0 skipped cases), generate and check dispatches, and run bus benchmarks. Generator options: `--output <dir>`, `--check`, `--verbose` (after `--`); exit codes: 0 success, 1 differences, 2 generator/argument/I/O error. CPU benchmark: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. With logical CPU 0 affinity, NOP measures 2.1312 ns/opcode; the synthetic jump/load loop measures 0.6678 ns/T-state and the ALU loop 0.7050 ns/T-state (about 405 times real time at 3.5 MHz). All allocate 0 B. Separate arithmetic helpers were faster than shared carry cores in the ALU comparison (2026-10-02, spec section 8.4). A small generated `ExecuteMain` returns early for implemented opcode 00 with an empty pattern body, clears Q, then delegates to the single switch in `ExecuteMainDispatch`. These measurements cover desktop execution, not WebAssembly. After the 16-bit ALU group, repeated measurements overlap the repeated previous-dispatch control in all three benchmarks; the initial comparison and investigation are documented in spec section 8.4. After rotations and shifts, the intervals also overlap the before-group control (spec section 8.4).

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

Instructions emit all their timing through the bus (`FetchOpcode`, `Read`, `ReadDiscarded`, `Write`, `Internal(address, n)`, `In`, `Out`); never add cycles directly. Operand bytes of a conditional jump that is not taken are read with `ReadDiscarded` (same as `Read` on `SpectrumBus`; `FuseTestBus` logs it as `MC` only, like FUSE's `contend_read`).

For a FUSE failure, run `dotnet run --project ZXSinclair.Net.Test -- --filter <case> --verbose`, attach the report to the review, and classify the cause as semantics, timing or a FUSE convention before changing code or specs. Group plans must include filtered runs of their cases; group specs must document relevant FUSE conventions. The report compares all sections even when registers differ, decodes flags, groups memory ranges and shows an event window. `--max-failures` defaults to 10 detailed failures; the remaining failures get one line each. `--list-skipped` shows the last pending opcode and preceding memory bytes; it is context, not a disassembly.

## Commit & Pull Request Guidelines

History uses short subjects such as `bus`, `new core`, and `test opcode`; no formal convention is evident. Write concise subjects identifying changed behavior. PRs should explain changes, link relevant issues, and include build/test results and skipped-opcode limitations. Include regenerated sources with generator changes. Exclude `bin/`, `obj/`, and IDE-local artifacts.

CB and DDFDCB now cover all 256 opcodes. The generator emits `default: Unimplemented(); break;` only for incomplete tables, including pending opcodes and prefixes; ED holes are implemented.

FUSE fixture limitations are declared by case in `ZXSinclair.Net.Fuse/FuseConventions.cs`. The eight `BIT b,(HL)` cases ignore only F5/F3 of F (mask 0x28), because the fixtures derive them from the value read instead of MEMPTR. The CPU uses WZ; its own tests verify those bits. The runner reports differing ignored bits and counts all eight passes with this convention. All other state, memory, timing and bus events remain checked.

Group 9 verified (2026-10-03): Core 2550 tests, generator 139, FUSE 1284 passed / 0 failed / 51 skipped (8 BIT (HL) convention passes); generated coverage base/DD-FD 250, ED 54, CB/DD-FDCB 256. Repeating LDIR/LDDR/CPIR/CPDR take F5/F3 from the high byte of the rewound PC; the terminating iteration keeps classic flags. ExecuteBlockCopyFrame measures 0.5117 ns/T-state (558 times real time at 3.5 MHz), with 0 B allocated. Before/after intervals overlap in all three existing CPU benchmarks; results and limits are in Specs/spec-cpu-z80.md section 8.4.

Group 10 verified (2026-10-03): Core 2605 tests (56 new I/O cases and one duplicate removed), generator 154, FUSE 1335 passed / 0 failed / 0 skipped (8 existing BIT (HL) convention passes). Base/DD-FD 252 and ED 78 instruction opcodes are implemented; 178 ED holes remain pending for group 11. All four repeated CPU benchmarks allocate 0 B and their intervals overlap the before-group control; results are in spec CPU 8.4. Set ZX_ROM_48K to an existing local 16384-byte ROM to enable ExecuteRomBootFrames (200 frames from reset, RAM cleared, ns/frame; fps = 1e9 / ns, real-time factor = fps / 50.080128). The runner omits it with a message when the path is missing. No Spectrum ROM is included; a synthetic JP 0000 image was used only to validate benchmark execution, not ROM boot performance.


Group 11 verified (2026-10-03): Core 2832 tests, generator 156, FUSE 1335 passed / 0 failed / 0 skipped (8 existing BIT (HL) convention passes), build 0 warnings/errors, generator --check exit 0. All instruction groups and the 178 ED holes are implemented. IM 0 executes single-byte base opcodes, including RST, HALT and EI; multi-byte/prefix responses remain unsupported. Interrupt acknowledgement is 6 T plus the instruction's internal extension (IM 1/2 still total 13/19 T). A pending LD A,I/R clears P/V on an immediately accepted INT. RestTests covers ED holes, IM 0 timing, HALT return/wraparound and the pending flag. NMI P/V and the hardware bus address after HALT in IM 0 remain provisional; the IR contention choice is an inference documented in Specs/spec-instr-restos.md. Five dispatches and Z80Cpu.Im0.g.cs are generated. Repeated benchmark intervals overlap the previous-commit control in NOP/LDIR, and the jump/ALU means decrease; all allocate 0 B. The combined 16-bit pending-flag clear was measured and rejected because it slowed NOP; original offsets are preserved.

### Browser button timing tests

`ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype.Tests` is an xUnit/Playwright project with no Core reference. It measures Test2 and TestPutImage in the WASM handler, checks the canvas pixel and browser errors, and reports the first click separately from warm samples. The secondary Playwright wall clock includes automation latency. Neither clock measures compositor presentation; there are no performance thresholds.

Run from the repository root:

```sh
dotnet build ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype.Tests
pwsh ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet test ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype.Tests --logger "console;verbosity=detailed"
dotnet test zxsinclair.net.slnx --filter "Category!=Browser"
```

Without `ZX_PROTOTYPE_URL`, the fixture starts and stops its own Development server with `dotnet run -c Debug`: WASM is interpreted. `ZX_PROTOTYPE_URL` selects an existing HTTP/HTTPS server, which the fixture leaves running. For AOT measurements, publish the host with `dotnet publish ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype -c Release`, serve the publication separately, and set `ZX_PROTOTYPE_URL` to that URL. External-server configuration cannot be inferred by the test; record the build configuration with the results.

`ZX_TIMING_ITERATIONS` is a positive integer (default 30); five warmup clicks are discarded per button. `ZX_PLAYWRIGHT_HEADED=1` shows Chromium. The report includes URL, browser version, server configuration, cold/min/median/p95/max/mean in ms, and raw samples in `TestResults/button-timings.json` (ignored by Git; overwritten each run). p95 uses nearest rank. A fresh browser context starts each run, but the TestPutImage first click can reuse assets downloaded by the main runtime. Solution-wide tests include the browser test unless `Category!=Browser` is specified; build the test and install Chromium first.
Optional `ZX_PLAYWRIGHT_CHANNEL=chrome` or `msedge` uses an already installed browser; the default is Playwright Chromium. The selected channel and version are recorded so results from different browsers can be distinguished.
