# Plan: implementar la instrucción NOP

Fecha: 2026-10-02
Estado: implementado y verificado (2026-10-02)

## Objetivo
Implementar `NOP` según `Specs/spec-instr-nop.md`: despacho en la CPU, tests propios no FUSE, adaptación de los tests existentes, benchmark re-medido y documentación al día.

## Contexto
- La CPU base (`Z80Cpu<TBus>`) no tiene instrucciones: todos los opcodes caen en `Unimplemented()` desde el despacho provisional `ZXSinclair.Net.Core/Z80/Z80Cpu.Instructions.cs`.
- El fetch (M1 + R++ + PC++) ya lo hacen `Step()`/`FetchOpcode()` en `ZXSinclair.Net.Core/Z80/Z80Cpu.cs`, así que `NOP` es un `case` vacío.
- Los prefijos `DD`/`FD` con un opcode que no usa HL llegan a `ExecuteIndexedOpcode<TIndex>` vía `FinishIndexed<TIndex>`; la spec (4.1) exige el mismo cuerpo allí.
- `Q` existe en `Z80Registers` pero la CPU aún no lo usa: `NOP` no lo toca.
- Tests: `ZXSinclair.Net.Core.Tests/TestBus.cs` (`TestBus`/`TestBusState`, registra `M1`, `Read`, `Write`, `Internal`, `Ack`; `IntAfterCycle` para activar INT a partir de un T-state).
- Tests que dependen de que `00` sea no implementado: `UnimplementedDispatchCountsFetches`, `LongAlternatingPrefixChainUsesConstantStackSpace` (byte `0xFFFF` = `00`) y `SpectrumIntegrationRunsAFrameAndAppliesFetchContention` (espera 17472) en `Z80CpuTests.cs`.
- `ZXSinclair.Net.Benchmarks/Z80CpuBenchmarks.cs` devuelve `UnimplementedOpcodes`, que pasaría a valer 0.
- El runner FUSE (`ZXSinclair.Net.Test/Program.cs`) omite un caso si `UnimplementedOpcodes != 0` tras ejecutarlo; `00` y `dd00` se ejecutarán.

## Pasos
1. **Despacho** — `ZXSinclair.Net.Core/Z80/Z80Cpu.Instructions.cs`: añadir `case 0x00: break; // NOP` en `ExecuteMain` y en `ExecuteIndexedOpcode<TIndex>`. Resto de `switch` sin cambios; mantener cabecera GPL y comentario de fichero provisional.
2. **Tests propios** — nuevo `ZXSinclair.Net.Core.Tests/Z80/Instructions/NopTests.cs` (cabecera GPL, namespace `ZXSinclair.Net.Core.Tests.Z80.Instructions`), con helper `Create(...)` propio como el de `Z80CpuTests` y estado inicial con todos los registros ≠ 0 (`IFF1 = IFF2 = true`, `IM = 1`, `Q = 0`). Los 12 tests de la tabla 5.2: `Nop_EmitsSingleM1`, `Nop_ChangesOnlyPcAndR`, `Nop_DoesNotCountAsUnimplemented`, `Nop_RefreshWrapsKeepingBit7`, `Nop_AtFFFF_WrapsPcToZero`, `Nop_DoesNotWriteMemory`, `Nop_WithIndexPrefix_RunsAsPlainNop`, `Nop_WithRepeatedPrefixes_RunsAsPlainNop`, `Nop_ClearsEiPending`, `Nop_AllowsIntOnNextStep` (`IntAfterCycle = 1`), `Nop_AllowsNmiOnNextStep`, `Nop_ExecuteStopsAtTarget`.
3. **Adaptar tests existentes** — `ZXSinclair.Net.Core.Tests/Z80CpuTests.cs`:
   - `UnimplementedDispatchCountsFetches`: cambiar el `00` final de `{00}`, `{DD,00}`, `{FD,00}`, `{DD,FD,00}`, `{FD,DD,00}` por `01`; ciclos, R y PC iguales.
   - `LongAlternatingPrefixChainUsesConstantStackSpace`: poner `0x01` en `Memory[0xFFFF]` para conservar `UnimplementedOpcodes == 1`.
   - `SpectrumIntegrationRunsAFrameAndAppliesFetchContention`: esperar `UnimplementedOpcodes == 0`; PC y T-states sin cambios.
   - Comprobar que el resto (`ResetClears…`, `ExecuteUses…`, HALT, interrupciones) sigue pasando.
4. **Benchmark** — `ZXSinclair.Net.Benchmarks/Z80CpuBenchmarks.cs`: devolver `cpu.Registers.PC` en vez de `UnimplementedOpcodes` y actualizar el `<summary>` (ahora mide `NOP` real con fetch, despacho y comprobación de interrupciones).
5. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings nuevos.
   - `dotnet test ZXSinclair.Net.Core.Tests` en verde.
   - `dotnet run --project ZXSinclair.Net.Test`: `00` y `dd00` pasan con eventos, 0 fallos; anotar passed/skipped.
   - `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*'`: ≤ 3.844 ns/opcode y 0 B asignados.
6. **Documentación**:
   - `Specs/spec-cpu-z80.md`: sección 5 (los `switch` ya no están vacíos: `00` implementado), 8.1 (casos ejecutados/omitidos) y 8.4 (nueva medición con fecha, indicando que mide `NOP`).
   - `CLAUDE.md` y `AGENTS.md`: "has no instructions yet" → solo `NOP`; "currently all 1335 cases skipped" → cifra real del runner; descripción del benchmark.

## Riesgos / cosas a vigilar
- Otros casos FUSE que solo ejecuten `00` antes de `end_tstates` pasarán a ejecutarse; si alguno falla, investigarlo antes de cerrar.
- `01` como opcode "no implementado" en los tests adaptados dejará de serlo al implementar `LD BC,nn`; habrá que cambiarlo entonces.
- El benchmark tiene ruido; repetir si supera la base por poco.
- `Q` no se gestiona aún; no introducirlo en este cambio.

## Fuera de alcance
- El generador de instrucciones.
- Huecos de la tabla ED, IM 0 con instrucciones arbitrarias y cualquier otra instrucción.

## Resultado

- NOP implementado como caso vacio en ambos despachos, sin ciclos adicionales.
- 12 metodos de prueba propios (17 casos), incluidos prefijos repetidos DD DD y FD FD, y contador previamente no nulo.
- Build Debug: 0 errores y 0 advertencias. xUnit: 197 correctos, 0 fallos, 0 omitidos.
- FUSE con eventos: 3 correctos (`00`, `dd00`, `ddfd00`), 0 fallos, 1332 omitidos.
- Benchmark Release: 2.113 ns/opcode, error 0.0105 ns, desviacion 0.0082 ns, 0 B asignados; base 3.844 ns/opcode.
- Informe local: `ZXSinclair.Net.Benchmarks/bin/nop-benchmark-artifacts/results/ZXSinclair.Net.Benchmarks.Z80CpuBenchmarks-report-github.md`.
- Generador, Q, huecos ED e IM0 arbitrario permanecen fuera del alcance. Sin commit.
