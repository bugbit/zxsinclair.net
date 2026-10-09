# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ZX Spectrum (Sinclair) emulator in C# / .NET 10, with a Blazor WebAssembly front end planned (the front end does not exist yet). Licensed under GPLv3. Every source file starts with the `#region LICENSE` GPL header block, and generated code must include it too. Code comments and TODOs are often in Spanish.

The emulator is being rewritten in `ZXSinclair.Net.Core`; all emulator code goes there. The old `ZXSinclair.Net` project has been deleted (it remains in git history). The current work is the Z80 CPU of the Core, specified in `Specs/spec-cpu-z80.md`; it currently implements NOP, the 8-bit and 16-bit load groups, the jump, call and return group, the 8-bit ALU group, the general arithmetic and CPU control group, the 16-bit ALU group, the rotation and shift group, the bit set, reset and test group, the exchange, block transfer and search group, and the input and output group, emitted by the instruction generator into `ZXSinclair.Net.Core/Z80/Generated/`.

## Z80 Technical Reference

`Docs/z80cpu_um.pdf` is the Z80 technical manual. `Docs/z80-no-documentado.md` summarises undocumented opcodes, flags (F5/F3, `Q`, interrupted block instructions), MEMPTR rules, R, interrupts and model differences from external sources, with a table of what this project implements. `Docs/maquinas-sinclair-es.md` lists, per Sinclair machine (ZX80, ZX81, 16K/48K, 128K/+2, +2A/+3), the I/O ports and devices, their synchronisation with CPU T-states and how often each ROM scans the keyboard. `Docs/zx81-video-es.md` describes ZX81 CPU-generated video (forced NOP, pattern address from `I`/char code/line counter, INT on A6, NMI/WAIT margins), pseudo and WRX hi-res, UDG boards and the Chroma 81 colour modes (port 0x7FEF, character-code table at 0xC000, attribute file at `D_FILE | 0xC000`), with the consequences for the emulator bus. `Specs/spec-correcciones-no-documentado.md` specifies (and the Core implements) two fixes from that comparison: `Q = 0` after a `DD`/`FD` prefix (so prefixed `SCF`/`CCF` see no flag history) and `WZ = PC + 1` when `INxR`/`OTxR` repeat. `Specs/spec-correcciones-revision.md` specifies fixes from the instruction code review: a shared `RepeatBlock` step for repeating block instructions, the corrected `BIT b,r` F5/F3 rule in the undocumented reference, and a JIT-inlining investigation of the generated base dispatch with an `ExecuteMixFrame` benchmark (53 distinct base opcodes); all three are implemented, and the investigation kept the current base dispatch (spec CPU 8.4). `Specs/spec-correcciones-revision-2.md` specifies the remaining review fixes: read-only `Z80Flags` tables, an explicit `Opcode.Length` driving the IM 0 table, and a shared `IsInstruction` guard in every pattern; all three are implemented (`Z80Flags` tables are `ReadOnlySpan<byte>` properties over private arrays). Consult it when implementing or verifying CPU instructions, registers, flags, and timing.

## Specifications

Specification documents go in `Specs/`, not in `Docs/` (which holds external reference material such as the Z80 manual). `Specs/spec-buses-memoria.md` specifies the Z80 bus and the Spectrum 16K/48K/128K/+2 and ZX81 memory (maps, contention, paging) for the `ZXSinclair.Net.Core` rewrite. `Specs/spec-cpu-z80.md` specifies the CPU contract (`ICpu`) and the Z80 CPU (`Z80Cpu<TBus>`: registers, execution loop, prefixes, interrupts, reset, test bus, generator contract) with NOP, the 8-bit and 16-bit load groups, the jump, call and return group, the 8-bit ALU group, the general arithmetic and CPU control group, the 16-bit ALU group, the rotation and shift group, the bit set, reset and test group, the exchange, block transfer and search group, and the input and output group implemented. `Specs/spec-instr-nop.md` specifies `NOP`, the pilot instruction (including the rule that opcodes absent from `opcodes_ddfd.dat` are generated in `ExecuteIndexedOpcode` with the same body as in `ExecuteMain`). `Specs/spec-proceso-instrucciones.md` is the process guide for instructions: one spec and plan per Zilog manual group, implemented by generator patterns, in a fixed order starting with the generator spec, plus a progress table. `Specs/spec-generador-z80.md` specifies the generator (FUSE tables, including `opcodes_cb.dat`/`opcodes_ddfdcb.dat` restored from git history, aliases, pattern catalog per group, five generated files in `ZXSinclair.Net.Core/Z80/Generated/`, `--check`, and its own test project). `Specs/spec-instr-carga-8.md` specifies group 1 (8-bit loads, including undocumented IXH/IXL forms, WZ rules and the `ReadPc16`/`IndexedAddress<TIndex>` helpers). `Specs/spec-instr-carga-16.md` specifies group 2 (16-bit loads, `LD SP,HL`, `PUSH`/`POP`, undocumented `ED 63`/`ED 6B`, and the `LoadWordAbsolute`/`StoreWordAbsolute`/`PushWithDelay` helpers). `Specs/spec-instr-saltos.md` specifies group 3 (jumps, calls, returns, `RETN`/`RETI` aliases, `RST`, the `Z80Cpu.Jumps.cs` helpers and the synthetic `ExecuteLoopFrame` benchmark). `Specs/spec-instr-alu-8.md` specifies group 4 (8-bit ALU and `INC`/`DEC`, branch-free flag formulas, new `Z80Flags.Inc`/`Dec` tables, exhaustive reference-model tests and the `ExecuteAluLoopFrame` benchmark). `Specs/spec-instr-control.md` specifies group 5 (`DAA`, `CPL`, `NEG`, `SCF`, `CCF`, `HALT`, `DI`/`EI`, `IM` with ED aliases) and the `Q` register: the generator writes `Q = F` after flag-writing patterns (`IPattern.WritesFlags`) and `Q = 0` otherwise; `FuseCpuState.Load` sets `Q = F` to match FUSE's `SCF`/`CCF`. `Specs/spec-instr-alu-16.md` specifies group 6 (16-bit `ADD`/`ADC`/`SBC`, `ADD IX/IY`, `INC`/`DEC` of pairs, branch-free flag formulas, `WZ = HL + 1`, `Q` via `WritesFlags`). `Specs/spec-instr-rotaciones.md` specifies group 7 (rotations and shifts in base/CB/DDCB tables, undocumented `SLL` and DDCB copies to registers, `RLD`/`RRD`, `WZ = ii + d` set once in `FinishIndexed`). `Specs/spec-instr-bits.md` specifies group 8 (`BIT`/`SET`/`RES` on registers, `(HL)` and `(ii+d)`, DDCB `BIT` aliases and undocumented `SET`/`RES` copies, `BIT` F5/F3 from the register, MEMPTR or `ii + d`; completes the CB and DDFDCB tables). `Specs/spec-instr-bloques.md` specifies group 9 (`EX`, `EXX`, `EX (SP),HL/IX/IY`, `LDI`/`LDIR`/`LDD`/`LDDR`, `CPI`/`CPIR`/`CPD`/`CPDR`; each repeat is a whole instruction with `PC -= 2`, internal cycles on the written `DE` or read `HL`, and the `ExecuteBlockCopyFrame` benchmark). `Specs/spec-instr-io.md` specifies group 10 (`IN`/`OUT`, undocumented `IN F,(C)`/`OUT (C),0`, block I/O with classic flags plus the repeat F5/F3 and H/P/V adjustments, I/O WZ rules, the extended `TestBus` I/O recording, and the optional ROM-boot benchmark); after it all 1335 FUSE cases run. `Specs/spec-instr-restos.md` specifies group 11 (ED holes as two NOPs via `EdHolePattern`, IM 0 execution of single-byte base opcodes using a generated `Z80Cpu.Im0.g.cs` table, multi-byte IM 0 responses left unsupported, and the NMOS P/V reset when an INT follows `LD A,I`/`LD A,R`). `Specs/spec-frontend-blazor.md` specifies the planned Blazor WebAssembly front end: emulation in a Web Worker with its own .NET runtime (not `WasmEnableThreads`), the worker's JS driving a `[JSExport] RunFrame()` loop, audio consumption as the master clock, input as a full state per frame (SharedArrayBuffer or `postMessage`), video as palette indexes presented through `OffscreenCanvas`/WebGL2, audio through an `AudioWorklet` ring buffer, and the platform-independent `SpectrumEmulator` contract in the Core (not implemented yet).

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
dotnet run --project ZXSinclair.Net.Test -- --filter 20_2 --verbose
dotnet run --project ZXSinclair.Net.Test -- --filter dd --list-skipped --max-failures 1
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check
dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests
```

- **`ZXSinclair.Net.Core.Tests`** is an xUnit project for the Core (memory maps, paging, contention, bus, floating bus). Run a single test with `dotnet test ZXSinclair.Net.Core.Tests --filter "FullyQualifiedName~SpectrumBusTests.In_FollowsTheIoContentionTable"`.
- **`ZXSinclair.Net.Test`** is the console FUSE Z80 runner (not xUnit), in Debug or Release. It executes `Z80Cpu<FuseTestBus>`, skips unimplemented instructions (currently 847 of 1335 cases; 488 pass with no failures), and compares registers with `FuseCpuState`, plus memory/events with `FuseComparison`. `--no-events` disables event recording/comparison. Core xUnit tests use the same `ZXSinclair.Net.Fuse` library to verify the bus events and the parser against the real fixtures.
- `ZXSinclair.Net.Generate.Z80OpCodes` generates the five dispatches. Use `-- --check` to compare without writing, `-- --verbose` to list pending opcodes, or `-- --output <dir>` to target another directory. Exit codes: 0 success, 1 generated-file differences, 2 generator/argument/I/O error.
- VS Code launch/tasks configs exist for the FUSE runner and the generator; the `build` task builds the whole solution (`.vscode/launch.json`, `.vscode/tasks.json`).
- If git reports "dubious ownership" for this directory, the user needs to add a `safe.directory` exception. Don't change global git config without asking.


CPU benchmarks: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. On 2026-10-02, NOP measured 2.1312 ns/opcode, the jump/load loop 0.6678 ns/T-state and the ALU loop 0.7050 ns/T-state (405 times real time at 3.5 MHz), all with 0 B allocated. Separate arithmetic helpers remain as selected in the preceding ALU comparison. Environment and results: `Specs/spec-cpu-z80.md` section 8.4. After the 16-bit ALU group, repeated measurements overlap the repeated previous-dispatch control in all three benchmarks; the initial comparison and investigation are documented in spec section 8.4. After rotations and shifts, the intervals also overlap the before-group control (spec section 8.4).

## Architecture

### Projects
- **ZXSinclair.Net.Core**: the emulator (see "Core architecture" below).
- **ZXSinclair.Net.Core.Tests**: xUnit tests for the Core.
- **ZXSinclair.Net.Generate.Z80OpCodes.Tests**: xUnit tests for the generator (parser, operands, aliases, patterns, deterministic output and CLI checks); no Core reference.
- **ZXSinclair.Net.Benchmarks**: BenchmarkDotNet benchmarks (memory strategies, Core bus). Run with `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Bus*'`.
- **ZXSinclair.Net.Generate.Z80OpCodes**: Z80 instruction generator. Reads five embedded FUSE tables (`data/opcodes_*.dat`, format `0xNN MNEMONIC args`; DD and FD share `opcodes_ddfd.dat`) and emits five files in `ZXSinclair.Net.Core/Z80/Generated/`. It uses a pattern catalog with NOP, 12 patterns for 8-bit loads, 6 patterns for 16-bit loads, 9 patterns for jumps, calls and returns, 3 patterns for 8-bit ALU, 9 patterns for general arithmetic and CPU control, and 3 patterns for 16-bit ALU, has no Core reference, and follows the contract in `Specs/spec-cpu-z80.md` section 5 and its output must not be hand-edited.
- **ZXSinclair.Net.Fuse**: library shared by the FUSE runner and the Core tests: the FUSE files (`data/tests.in`, `data/tests.expected`, embedded) and their parser (`FuseTestFile`, malformed lines throw `FormatException` with the test name), the recording `FuseTestBus`, `FuseCpuState` and `FuseComparison`.
- **ZXSinclair.Net.Test**: FUSE Z80 conformance runner (console; references the Core and `ZXSinclair.Net.Fuse`).

### Core architecture (`ZXSinclair.Net.Core`)

For a FUSE failure, run `dotnet run --project ZXSinclair.Net.Test -- --filter <case> --verbose`, attach the report to the review, and classify semantics, timing or a FUSE convention before changing code or specs. Include filtered group runs in instruction plans and known FUSE conventions in their specs. Reports compare registers, flags, memory, events and T-states independently; `--max-failures` defaults to 10 detailed failures, while `--list-skipped` shows the last pending opcode and surrounding bytes.
Implements `Specs/spec-buses-memoria.md` and the base CPU in `Specs/spec-cpu-z80.md`, with NOP, the 8-bit and 16-bit load groups, the jump, call and return group, the 8-bit ALU group, the general arithmetic and CPU control group, the 16-bit ALU group, the rotation and shift group, the bit set, reset and test group, the exchange, block transfer and search group, and the input and output group implemented.
- `ICpu`, `Z80Registers`, `Z80Flags`, `Z80Cpu<TBus>`: registers, flag tables, prefix loop, interrupts, HALT state and reset. The CPU stores its bus in a mutable field to avoid defensive copies. The generator emits the five dispatches in `Z80/Generated/`, with NOP, the 8-bit and 16-bit load groups, the jump, call and return group, the 8-bit ALU group, the general arithmetic and CPU control group, the 16-bit ALU group, the rotation and shift group, the bit set, reset and test group, the exchange, block transfer and search group, and the input and output group implemented and other opcodes incrementing `UnimplementedOpcodes`. Repeated DD/FD prefixes use constant stack space; unsupported IM0 bytes are counted without inventing a jump.
- CPU benchmark: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. With logical CPU 0 affinity, NOP including fetch, dispatch and interrupt checks measures 2.1545 ns/opcode versus 2.232 ns for the previous dispatch control. `ExecuteLoopFrame` measures a synthetic loop of loads, stack operations, calls, returns and relative jumps in uncontended RAM: 0.6487 ns/T-state, about 440 times real time at 3.5 MHz. Both allocate 0 B (2026-10-02, spec section 8.4); neither measures WebAssembly. The generated base dispatch keeps a small `ExecuteMain` entry with an early NOP return and a single switch in `ExecuteMainDispatch`.
- `Abstractions/`: generic contracts for any CPU/machine — `IBus` (clock + reset), `IBusData<TAddress, TData>` (timed bus access), `IBusIo<TPort, TData>` (I/O space), `IMemory<TAddress, TData>` (raw access), `IMemoryBuffer<TAddress, TData>` (block load/save). `Z80/IZ80Bus` combines them with the Z80 cycles (`FetchOpcode`, `Internal`, INT). `ReadDiscarded` reads the operand bytes of a conditional jump that is not taken: identical to `Read` on `SpectrumBus`, logged as `MC` only by `FuseTestBus` (FUSE's `contend_read`).
- **Buses are structs used only as generic arguments** (`where TBus : struct, IZ80Bus`): the JIT specialises generics only for struct type arguments, so a class or an interface-typed variable brings back interface dispatch. `SpectrumBus` is a `readonly struct` holding one reference to a `sealed` `SpectrumMachine` (T-state counter, memory, tables, ports); the future CPU must be `Z80Cpu<TBus>`.
- `Memory/`: `MemoryLayout` describes any 64K map (page size as a power of two, ROM/RAM regions of any size, mirrors, unmapped pages); `PagedMemory` builds it into one pinned array with separate read/write offset tables per page (ROM and unmapped writes go to a sink page, unmapped reads to a 0xFF page), so accesses never branch. `Machines/Layouts.cs` has presets for Spectrum 16K/48K/128K(+2) and ZX81 1K/16K.
- `Timing/`: `MachineTiming` presets, and precomputed per-T-state `ContentionTable` and `FloatingBusTable` (one table read per access). The machine must call `EndFrame()` before T-states exceed the frame by `ContentionTable.Margin`.
- `Machines/Spectrum/`: `SpectrumMachine` (16K, 48K, 128K, grey +2), `SpectrumBus` (memory/IO contention, ULA port 0xFE, floating bus, INT), `Spectrum128Paging` (port 0x7FFD: paging only rewrites page table entries and contention flags).

### Test data
The data files are `ZXSinclair.Net.Fuse/data/tests.in` and `tests.expected` (embedded resources), in the FUSE Z80 test suite format. Each test has a name, a register line (AF BC DE HL AF' BC' DE' HL' IX IY SP PC), a line with I R IFF1 IFF2 IM halted end-tstates, and memory blocks terminated by `-1`. In `tests.expected`, bus event lines (MC/MR/MW...) come before the registers. The runner uses a recording `FuseTestBus` (no instrumentation in the CPU), fills memory with `DE AD BE EF`, executes until `end_tstates`, and compares registers, IFF/IM/halted, T-states, memory, and every bus event (time, type, address, and optional data), unless the case contains unimplemented instructions.

Every instruction implementation must include, besides passing its FUSE cases, its own xUnit tests in `ZXSinclair.Net.Core.Tests`, covering the rules of its spec that FUSE does not check on its own (timing and bus cycles, flags, R, PC wraparound, DD/FD variants, interrupt acceptance afterwards, `UnimplementedOpcodes` unchanged). FUSE passing alone is not enough.

Whenever an instruction is implemented and verified, update [Specs/estado-instrucciones-z80.md](Specs/estado-instrucciones-z80.md) in the same change: mark its entry as completed (`[x]`), add links to its specification and tests, and update the completed/pending counts. Mark an entry only when all its documented variants are implemented and their own xUnit tests and applicable FUSE cases pass. For partial implementations, leave the checkbox unchecked and note the available variants.

CB and DDFDCB now cover all 256 opcodes. The generator emits `default: Unimplemented(); break;` only for incomplete tables, including pending opcodes and prefixes; ED holes are implemented.

FUSE fixture limitations are declared by case in `ZXSinclair.Net.Fuse/FuseConventions.cs`. The eight `BIT b,(HL)` cases ignore only F5/F3 of F (mask 0x28), because the fixtures derive them from the value read instead of MEMPTR. The CPU uses WZ; its own tests verify those bits. The runner reports differing ignored bits and counts all eight passes with this convention. All other state, memory, timing and bus events remain checked.

Group 9 verified (2026-10-03): Core 2550 tests, generator 139, FUSE 1284 passed / 0 failed / 51 skipped (8 BIT (HL) convention passes); generated coverage base/DD-FD 250, ED 54, CB/DD-FDCB 256. Repeating LDIR/LDDR/CPIR/CPDR take F5/F3 from the high byte of the rewound PC; the terminating iteration keeps classic flags. ExecuteBlockCopyFrame measures 0.5117 ns/T-state (558 times real time at 3.5 MHz), with 0 B allocated. Before/after intervals overlap in all three existing CPU benchmarks; results and limits are in Specs/spec-cpu-z80.md section 8.4.

Group 10 verified (2026-10-03): Core 2605 tests (56 new I/O cases and one duplicate removed), generator 154, FUSE 1335 passed / 0 failed / 0 skipped (8 existing BIT (HL) convention passes). Base/DD-FD 252 and ED 78 instruction opcodes are implemented; 178 ED holes remain pending for group 11. All four repeated CPU benchmarks allocate 0 B and their intervals overlap the before-group control; results are in spec CPU 8.4. Set ZX_ROM_48K to an existing local 16384-byte ROM to enable ExecuteRomBootFrames (200 frames from reset, RAM cleared, ns/frame; fps = 1e9 / ns, real-time factor = fps / 50.080128). The runner omits it with a message when the path is missing. No Spectrum ROM is included; a synthetic JP 0000 image was used only to validate benchmark execution, not ROM boot performance.


Group 11 verified (2026-10-03): Core 2832 tests, generator 156, FUSE 1335 passed / 0 failed / 0 skipped (8 existing BIT (HL) convention passes), build 0 warnings/errors, generator --check exit 0. All instruction groups and the 178 ED holes are implemented. IM 0 executes single-byte base opcodes, including RST, HALT and EI; multi-byte/prefix responses remain unsupported. Interrupt acknowledgement is 6 T plus the instruction's internal extension (IM 1/2 still total 13/19 T). A pending LD A,I/R clears P/V on an immediately accepted INT. RestTests covers ED holes, IM 0 timing, HALT return/wraparound and the pending flag. NMI P/V and the hardware bus address after HALT in IM 0 remain provisional; the IR contention choice is an inference documented in Specs/spec-instr-restos.md. Five dispatches and Z80Cpu.Im0.g.cs are generated. Repeated benchmark intervals overlap the previous-commit control in NOP/LDIR, and the jump/ALU means decrease; all allocate 0 B. The combined 16-bit pending-flag clear was measured and rejected because it slowed NOP; original offsets are preserved.
