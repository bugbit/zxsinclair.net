# Plan: grupo 7, rotaciones y desplazamientos

Fecha: 2026-10-02
Estado: implementado y verificado (2026-10-02)

## Objetivo
Implementar el grupo 7 (rotaciones y desplazamientos de las tablas base, CB, ED y DDCB/FDCB, incluidos `SLL` y las copias no documentadas a registro) según `Specs/spec-instr-rotaciones.md`, con 198 casos FUSE nuevos pasando.

## Contexto
- Grupos 0–6 implementados; el runner FUSE pasa 488 casos. **El grupo 6 está en el árbol de trabajo sin commit** (Alu16, `.g.cs` y specs).
- Las tablas CB y DDFDCB están vacías: `Z80Cpu.CB.g.cs` y `Z80Cpu.IndexedCB.g.cs` solo tienen el `default`.
- El parser ya separa las formas compuestas de DDFDCB (`Opcode.CopyTo` + `InnerInstruction`, en `Model/Opcode.cs`), y `OperandEmitter.Address` devuelve `address` con `EmitContext.IndexedCB`.
- `DispatchEmitter` admite cuerpos de varias líneas y añade `Q` según `WritesFlags`.
- `FinishIndexed` (`ZXSinclair.Net.Core/Z80/Z80Cpu.cs`, hacia la línea 107) calcula la dirección DDCB y hace los ciclos de prefijo, pero no escribe WZ.
- Tests afectados:
  - `Z80CpuTests.UnimplementedDispatchCountsFetches` (`CB 12`, `FD CB FE 12`, `DD CB 80 12`);
  - `PrefixesAndOperandReadsWrapAtEndOfMemory` (`FD CB FE 12`);
  - `FuseTestBusTests.IndexedPrefixCyclesMatchRealFuseEvents` (`ddcb00` truncado a 10 eventos);
  - `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset` (`DD CB 0 0`);
  - las cifras de cobertura de `GeneratorTests`.

## Pasos
0. **Precondición resuelta** — el usuario indicó que había commiteado el grupo 6; este checkout todavía lo muestra sin commit. Se continúa sobre el árbol actual conservando sus cambios, sin crear commits.
1. **Auxiliares** — nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Rotate.cs` (cabecera GPL, `partial`, `AggressiveInlining`), según la spec 2.2:
   - `Rlc`, `Rrc`, `Rl`, `Rr`, `Sla`, `Sra`, `Sll` y `Srl`: devuelven el byte y ponen `F = SZ53P[r] | c`.
   - `Rlca`, `Rrca`, `Rla` y `Rra`.
   - `Rld` y `Rrd`: lectura, `bus.Internal(HL, 4)`, escritura y `WZ = HL + 1`.
2. **WZ en DDCB** — en `FinishIndexed`, guardar la dirección calculada en `Registers.WZ` antes de llamar a `ExecuteIndexedCB`.
3. **Patrones** — nuevo `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Rotate.cs`:
   - `RotateAccumulatorPattern`;
   - `RotateRegisterPattern`;
   - `RotateMemoryPattern` (`(HL)` en CB; `(REGISTER+dd)` en DDFDCB con `address`);
   - `RotateMemoryCopyPattern` (las formas con `CopyTo`);
   - `RotateDigitPattern`.

   Una tabla fija asigna cada mnemónico a su auxiliar. Todos con `WritesFlags = true`, y ninguno casa `BIT`/`RES`/`SET`. Registrarlos en `PatternCatalog.Default`.
4. **Regenerar** — `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --verbose`; cobertura base 246, ED 46, CB 64, DDFD 246, DDFDCB 64. Revisar el diff de `Z80Cpu.CB.g.cs` y `Z80Cpu.IndexedCB.g.cs`: lectura, ciclo interno y escritura, y copia a un registro real.
5. **Tests del generador** — `GeneratorTests.cs`:
   - Cifras de la spec 4.3: `246 implemented / 6 pending / 4 prefixes`, `46 implemented / 32 pending`, y CB y DDFDCB `64 implemented / 192 pending`.
   - `RotatePatterns_EmitExpectedBodies`, con muestras `RLCA`, `RL C`, `SRA (HL)`, `SLL (REGISTER+dd)`, `LD H,RRC (REGISTER+dd)` y `RLD`.
   - `WritesFlags` de los cinco patrones.
   - Que `BIT 0,B`, `RES 0,(HL)` y `SET 7,(REGISTER+dd)` no casan.
6. **Adaptar tests del Core**:
   - `UnimplementedDispatchCountsFetches`: `CB 12` → `CB 80`, `FD CB FE 12` → `FD CB FE 86` y `DD CB 80 12` → `DD CB 80 86`.
   - `PrefixesAndOperandReadsWrapAtEndOfMemory`: comprobar la instrucción completa (`LD D,RL (IY-2)`: lectura, `Internal(dir, 1)`, escritura, D, 23 T y WZ).
   - `IndexedPrefixCyclesMatchRealFuseEvents`: comparar la secuencia completa de `ddcb00`.
   - `LastUnimplementedAddress_TracksFetchAndReset`: `DD CB 0 0` → `DD CB 0 86`.
7. **Tests propios** — nuevo `ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs` con los 11 tests de la spec 5.2: modelo de referencia independiente y exhaustivo por operación, accesos y T-states de cada forma, copias DDCB con H/L reales, `SLL`, `RLD`/`RRD` con el caso FUSE `ed67`, WZ y `Q`.
8. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings nuevos.
   - `dotnet test ZXSinclair.Net.Core.Tests` y `dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests` en verde.
   - `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check` → 0.
   - Runner con `--filter cb`, `ddcb`, `fdcb`, `ed6`, `0` y `1`; total 686 pasados y 0 fallos. Ante un fallo, `--filter <caso> --verbose`.
9. **Benchmark** — repetir `ExecuteFrame`, `ExecuteLoopFrame` y `ExecuteAluLoopFrame` en Release y anotar el resultado en `Specs/spec-cpu-z80.md` 8.4.
10. **Documentación** — `Specs/spec-proceso-instrucciones.md` (grupo 7 Implementado, 198 casos, 686 acumulados), `Specs/estado-instrucciones-z80.md` (entradas de rotación y desplazamiento y recuento), `Specs/spec-cpu-z80.md` 4.3 (WZ en `FinishIndexed`) y 8.1/8.4, y `CLAUDE.md`/`AGENTS.md`.

## Riesgos / cosas a vigilar
- `RotateMemoryCopyPattern`: el registro destino es `CopyTo`, con H y L reales; no confundirlo con `REGISTERH`/`REGISTERL`.
- Orden de bus en memoria: lectura → `Internal(dir, 1)` → escritura. En DDCB la dirección es `address`, no `IndexedAddress`, porque los ciclos de prefijo ya los hizo `FinishIndexed`.
- Escribir WZ en `FinishIndexed` afecta también a los futuros `BIT n,(ii+d)` del grupo 8, que lo necesitan para F3/F5; es coherente con la spec.
- `RLA`/`RRA` usan el C anterior; `RLCA`/`RRCA` no.
- Duración de los tests exhaustivos: una sola CPU y sin asignaciones por iteración.

## Resultado

- Auxiliares, cinco patrones y WZ en `FinishIndexed` implementados; cinco despachos regenerados y `--check` correcto.
- Build Debug sin errores ni warnings; 2414 tests Core y 117 del generador pasan. FUSE con eventos: 686 pasados, 0 fallos y 649 omitidos.
- Los tests de la spec se agrupan en teorías comunes, con 1 048 576 combinaciones exhaustivas y todos los registros, índices y copias. Se conserva el prefijo extra del test existente de PC (27 T), y se carga el fixture completo de ddcb00 para sus eventos.
- Benchmarks con 0 B e intervalos solapados en las tres cargas; resultados y limitaciones en spec CPU 8.4. Arranque de ROM 48K pendiente para el grupo 10; WebAssembly sin verificar.
- Documentación actualizada: 115/150 entradas completas, 35 pendientes. Pendiente contraste con z80full/z80memptr cuando se integren. Sin commits.

## Fuera de alcance
- `BIT`, `RES`, `SET` y sus copias DDCB (grupo 8).
- E/S e intercambios.
