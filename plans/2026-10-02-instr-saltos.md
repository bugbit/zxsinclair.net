# Plan: grupo 3, saltos, llamadas y retornos

Fecha: 2026-10-02
Estado: implementado y verificado

## Objetivo
Implementar el grupo 3 (`JP`, `JR`, `DJNZ`, `CALL`, `RET`, `RETN`/`RETI`, `RST`) según `Specs/spec-instr-saltos.md`: auxiliares en el Core, patrones en el generador, despachos regenerados, tests propios y un benchmark sintético de bucle, con 79 casos FUSE nuevos pasando.

## Contexto
- Grupos 1 y 2 implementados; el runner FUSE pasa 201 casos.
- El Core tiene `ReadPc()`, `ReadPc16()`, `Push` y `Pop` (`ZXSinclair.Net.Core/Z80/Z80Cpu.cs`) y `PushWithDelay` (`Z80Cpu.Load16.cs`).
- `PatternCatalog.Default` lista `NopPattern` y los patrones de Load8, Load16 y saltos.
- El parser clasifica `HL` (de `JP HL`) como `Register16`, `REGISTER` como `IndexPair`, las condiciones como `Condition` (que `OperandEmitter.Value` traduce a `(Registers.F & Z80Flags.X) ==/!= 0`) y el operando de `RST` como `RestartAddress` con `Value`.
- Cobertura verificada en `GeneratorTests.Output_MatchesExpectedCoverage`: base 135, ED 20 y DD/FD 135.
- Benchmark actual: `Z80CpuBenchmarks.ExecuteFrame` (NOP desde `PC = 0`).

## Pasos
1. **Auxiliares** — nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Jumps.cs` (cabecera GPL, `partial`, `AggressiveInlining`): `JumpAbsolute(bool)`, `JumpRelative(bool)`, `DecrementJumpNonZero()`, `CallAbsolute(bool)`, `Return()`, `ReturnConditional(bool)`, `ReturnFromInterrupt()` y `Restart(ushort)`, con la semántica de la spec 4.1: WZ y direcciones de los ciclos `Internal` (`PC - 1` tras leer `e` o `nn`; `IR` en `DJNZ`, `RET cc` y `RST`).
2. **Patrones** — nuevo `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Jumps.cs` con los 9 patrones de la spec 4.2: `JumpAbsolutePattern`, `JumpRegister`, `JumpRelativePattern`, `DecrementJump`, `CallPattern`, `ReturnPattern`, `ReturnConditionalPattern`, `ReturnFromInterruptPattern` y `RestartPattern`. La condición se emite con `OperandEmitter.Value` y la dirección de `RST` en hexadecimal (`Restart(0x38);`). Registrarlos en `PatternCatalog.Default`.
3. **Regenerar** — `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --verbose`; cobertura base 135, ED 20, DDFD 135, CB/DDFDCB 0. Revisar el diff: `RETN` con 8 etiquetas apiladas y `case 0xe9` en Indexed con `TIndex.Pair`.
4. **Tests del generador** — `GeneratorTests.cs`:
   - Cifras: 135/20/135; `135 implemented / 117 pending / 4 prefixes`; `20 implemented / 58 pending`.
   - Añadir `JumpPatterns_EmitExpectedBodies` con muestras `JP nnnn`, `JP NZ,nnnn`, `JP HL`, `JP REGISTER`, `JR C,offset`, `DJNZ offset`, `CALL PE,nnnn`, `RET`, `RET M`, `RETN` con alias y `RST 8` → `Restart(0x08);`.
5. **Tests propios** — nuevo `ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs` con los 19 tests de la spec 5.2: helper `Create` como el de `NopTests`, `I = 0x42`, una tabla condición → valor de F que la cumple o no (para los `[Theory]`), y comparación de `Z80Registers` y de `state.Accesses`.
6. **Benchmark** — `ZXSinclair.Net.Benchmarks/Z80CpuBenchmarks.cs`: añadir `ExecuteLoopFrame` (`OperationsPerInvoke = 69888`, ns por T-state).
   - En `GlobalSetup`, cargar en RAM no contenida (`0x8000`) un bucle con instrucciones ya implementadas, por ejemplo: `LD B,16; LD HL,9000h; loop: LD A,(9100h); LD (HL),A; PUSH BC; CALL sub; POP BC; DJNZ loop; JR inicio; sub: LD C,A; LD (9101h),A; RET`.
   - Cada invocación fija `PC = 0x8000` y `SP = 0xFF00` (con `IFF1 = 0`, sin INT), ejecuta un frame, llama a `EndFrame()` y devuelve `PC`.
   - Medir en Release y anotar en `Specs/spec-cpu-z80.md` 8.4 (ns por T-state, × tiempo real, fecha y entorno) junto a la nueva cifra de `ExecuteFrame`.
7. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings nuevos.
   - `dotnet test ZXSinclair.Net.Core.Tests` y `dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests` en verde.
   - `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check` → 0.
   - `dotnet run --project ZXSinclair.Net.Test`: 280 pasados y 0 fallos (el caso `10` sigue omitido por `INC C`).
8. **Documentación** — `Specs/spec-proceso-instrucciones.md` (grupo 3 Implementado, 79 casos, 280 acumulados y nota de que `10` queda pendiente del grupo 4), `Specs/spec-cpu-z80.md` 8.1 y 8.4, y `CLAUDE.md`/`AGENTS.md` (estado, cifras FUSE y nuevo benchmark).

## Riesgos / cosas a vigilar
- Los operandos no tomados usan `ReadPcDiscarded()`/`ReadPc16Discarded()`, según la spec corregida: misma lectura y temporización en Spectrum, eventos `MC` sin `MR` en FUSE; `JP`/`CALL` conservan `WZ = nn`.
- `JR`: el desplazamiento es relativo al PC tras leer `e`, y el ciclo `Internal` usa `PC - 1` (la dirección de `e`). `CALL`: el ciclo `Internal` va en la dirección del byte alto de `nn`, antes de `Push`.
- `DJNZ`: decrementar B tras el ciclo interno y antes de leer `e`, sin tocar flags.
- `RETN`: los 8 alias deben generar un único `case` múltiple, y el nuevo `IFF1` debe verse en el `Step()` siguiente.
- Benchmark: el programa debe estar en RAM (no en ROM), no habilitar interrupciones y ser un bucle infinito para cubrir el frame entero.
- `RET` lleva un espacio final en la tabla (`0xc9 RET `): el patrón exige 0 operandos.

## Fuera de alcance
- La ALU (necesaria para el caso FUSE `10`), IM 0 con instrucciones arbitrarias, `HALT`/`DI`/`EI` y el arranque de la ROM como benchmark.

## Resultado
- Build Debug: 0 errores y 0 avisos. Core: 702 tests; generador: 74 tests. Generador `--check`: 0. FUSE con eventos: 280 pasados, 0 fallos y 1055 omitidos.
- Benchmark Release con afinidad fija: NOP 2.1545 ns/opcode frente a 2.232 del control; bucle 0.6487 ns/T-state, unos 440 × tiempo real a 3.5 MHz. Ambos con 0 B. Detalles y resultados intermedios en spec CPU 8.4.
- Adaptaciones: `ReadDiscarded` en los operandos no tomados; entrada pequeña generada `ExecuteMain` y único switch en `ExecuteMainDispatch` para eliminar la regresión de NOP; cobertura adicional de prefijos, desbordamientos y contención. Seguimiento: 60/150 entradas completadas.
