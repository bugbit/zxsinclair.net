# Plan: grupo 9, intercambio y bloques

Fecha: 2026-10-03
Estado: implementado y verificado (2026-10-03; incluye la corrección de F5/F3 al repetir)

## Objetivo
Implementar el grupo 9 (`EX DE,HL`, `EX AF,AF'`, `EXX`, `EX (SP),HL/IX/IY` y las instrucciones de bloque `LDI`/`LDIR`/`LDD`/`LDDR` y `CPI`/`CPIR`/`CPD`/`CPDR`) según `Specs/spec-instr-bloques.md`, con 14 casos FUSE nuevos pasando y un benchmark de `LDIR`.

## Contexto
- Grupos 0–8 implementados y commiteados. El runner FUSE pasa 1270 casos (8 con la convención de `BIT (HL)`) y omite 65: 14 de este grupo y 51 de E/S.
- `Z80Registers` ya tiene `ExchangeAF()` y `Exx()`.
- Los auxiliares del Core están en `ZXSinclair.Net.Core/Z80/Z80Cpu.*.cs` (`Push`, `Pop`, `ReadPc`…).
- Los patrones están en `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/` y declaran `WritesFlags`. En la tabla DDFD, el único opcode propio del grupo es `E3`; el resto sale de la regla de ausentes.
- Los benchmarks están en `ZXSinclair.Net.Benchmarks/Z80CpuBenchmarks.cs`, con una máquina por carga (`machine`, `aluMachine`) y un `GlobalCleanup` por objetivo.
- Cobertura fijada en `GeneratorTests`: 246/46/256; `246 implemented / 6 pending` y `46 implemented / 32 pending`.

## Pasos
1. **Auxiliares** — nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Block.cs` (cabecera GPL, `partial`, `AggressiveInlining`) con `ExchangeStack(ushort)`, `BlockLoad(int step)`, `BlockLoadRepeat(int step)`, `BlockCompare(int step)` y `BlockCompareRepeat(int step)`, según la spec 2.1–2.3:
   - ciclos internos sobre el `DE` recién escrito o el `HL` leído, antes del `±1`;
   - repetición con `PC -= 2` y `WZ = PC + 1`;
   - `CPI`/`CPD` con `WZ ± 1`;
   - flags clásicos de la spec 2.2 en cada iteración;
   - **si la iteración repite**, después de `PC -= 2`: `F = (F & ~0x28) | ((PC >> 8) & 0x28)` en `BlockLoadRepeat` y `BlockCompareRepeat`. La iteración que termina conserva los flags clásicos. `Q = F` (que añade el generador) recoge ya el F ajustado.
2. **Patrones** — nuevo `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Block.cs`:
   - `ExchangeRegistersPattern` y `ExchangeStackPattern`, con `WritesFlags = false`;
   - `BlockLoadPattern` y `BlockComparePattern`, con `WritesFlags = true`, `step` constante y la variante repetitiva según el mnemónico.

   Registrarlos en `PatternCatalog.Default`.
3. **Regenerar** — `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --verbose`; cobertura base 250 (pendientes `D3` y `DB`), ED 54, DDFD 250, CB/DDFDCB 256. Comprobar que DD `EB` sale con el cuerpo de base (DE↔HL) y que no hay warnings.
4. **Tests del generador** — `GeneratorTests.cs`:
   - Cifras: `250 implemented / 2 pending / 4 prefixes` y `54 implemented / 24 pending`.
   - `BlockPatterns_EmitExpectedBodies`, con muestras `EX DE,HL`, `EX AF,AF'`, `EXX`, `EX (SP),REGISTER`, `LDIR` y `CPD`.
   - `WritesFlags` de los cuatro patrones.
5. **Tests propios** — nuevo `ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs` con los 20 tests de la spec 5.2: modelos de referencia de los flags de `LDI`/`CPI`, accesos y T-states de cada iteración, repetición hasta `BC = 0` y hasta una coincidencia, INT entre iteraciones, vuelta de 16 bits, WZ y `Q`. Para la regla de repetición:
   - `RepeatF5F3_FromPcHigh`: `LDIR`, `LDDR`, `CPIR` y `CPDR` con una iteración que repite y el `ED` en `0x0000`, `0x0800`, `0x2000` y `0x2800` (F5/F3 de PC = ninguno, solo F3, solo F5, ambos), con datos elegidos para que los F5/F3 clásicos difieran de los de PC; comprueba F5/F3 = `(PC >> 8) & 0x28`, el resto de F clásico y `Q = F`.
   - `RepeatFinalIteration_KeepsClassicFlags`: la iteración que termina (`BC = 1`, y coincidencia en `CPxR`) mantiene los F5/F3 clásicos y `Q = F`.
   - `RepeatFlags_SeenByInterrupt`: tras una iteración que repite en `0x2800`, INT con IM 1 y `PUSH AF` en `0x0038`; el F apilado lleva F5/F3 de PC y, tras aceptar la INT, `Q = 0`.
6. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings.
   - `dotnet test ZXSinclair.Net.Core.Tests` y `dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests` en verde.
   - `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check` → 0.
   - Runner con `--filter 08`, `d9`, `e3`, `eb`, `dde3`, `fde3`, `eda` y `edb`; total 1284 pasados y 0 fallos (8 con convención). Ante un fallo, `--filter <caso> --verbose`.
   - `--list-skipped` solo debe mostrar E/S: `D3`, `DB`, las de E/S de `ED 4x`–`7x` y los bloques de E/S.
7. **Benchmark** — `Z80CpuBenchmarks.cs`:
   - Añadir `ExecuteBlockCopyFrame` con su propia máquina y el programa `LD HL,9000h; LD DE,A000h; LD BC,1000h; LDIR; JR inicio` en `0x8000`, en RAM no contenida, con `OperationsPerInvoke = 69888` y un `GlobalCleanup` que verifique `UnimplementedOpcodes == 0`.
   - Repetir los tres benchmarks anteriores.
   - Anotar todo en `Specs/spec-cpu-z80.md` 8.4.
8. **Documentación** — `Specs/spec-proceso-instrucciones.md` (grupo 9 Implementado, 14 casos, 1284 acumulados), `Specs/estado-instrucciones-z80.md` (entradas de intercambio y bloque, y recuento), `Specs/spec-cpu-z80.md` 8.1 y 8.4, y `CLAUDE.md`/`AGENTS.md` (estado, cifras y benchmark nuevo).

## Riesgos / cosas a vigilar
- Dirección de los ciclos internos: guardar la dirección de la iteración en una variable local antes de incrementar o decrementar HL/DE.
- `CPIR` termina por `BC = 0` **o** por coincidencia (`Z = 1`); `LDIR` solo por `BC = 0`.
- `LDIR` con `BC = 0` al empezar copia 65 536 bytes porque BC da la vuelta. Es el comportamiento correcto y no debe tratarse como caso especial.
- `EX (SP),HL`: H se escribe en `SP+1` antes que L en `SP`.
- F5/F3 en las iteraciones que repiten: aplicar el ajuste **después** de `PC -= 2` (con el PC ya decrementado) y solo cuando repite. Los fixtures FUSE `edb0`, `edb1`, `edb8` y `edb9` terminan sus bucles en una iteración que no repite: pasarlos no demuestra la regla, que solo validan los tests propios.
- `ExchangeAF` deja `Q = 0` (decisión provisional del grupo 5).

## Fuera de alcance
- E/S y bloques de E/S (grupo 10).
- Huecos de ED e IM 0 (grupo 11).

## Resultados

- Solución Debug: 0 advertencias y 0 errores. Core: 2550 tests pasados (65 nuevos); generador: 139 (13 nuevos). Generador `--check`: código 0.
- FUSE: 1284 pasados, 0 fallos y 51 omitidos; 8 pasan con la convención existente de BIT (HL). Los filtros `08`, `d9`, `e3`, `eb`, `dde3` y `fde3` pasan 1 caso cada uno; `eda` pasa 4 y omite 23 de E/S; `edb` pasa 4 y omite 4 de E/S. `--list-skipped` confirma solo E/S pendiente.
- Cobertura: base y DD/FD 250, ED 54, CB y DDFDCB 256.
- Las reglas de los 20 tests previstos se agruparon en teorías que cubren también prefijos ignorados, registros completos, vuelta de PC y BC=0 con 65536 iteraciones. Se mantienen los modelos independientes y las pruebas explícitas de repetición e interrupciones.
- LDIR: 0.5117 ns/T-state, 0 B, unas 558 veces el tiempo real a 3.5 MHz. Los intervalos de los tres benchmarks previos se solapan con su control; cifras completas en spec CPU 8.4.
