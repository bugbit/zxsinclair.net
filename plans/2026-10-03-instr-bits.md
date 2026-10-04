# Plan: grupo 8, operaciones de bit

Fecha: 2026-10-03
Estado: implementado y verificado (2026-10-03).

## Objetivo
Implementar el grupo 8 (`BIT`, `SET`, `RES` sobre registro, `(HL)` y `(ii+d)`, con alias de `BIT` y copias no documentadas en DDCB) según `Specs/spec-instr-bits.md`, completando las tablas CB y DDFDCB con 584 casos FUSE nuevos pasando.

## Contexto
- Grupos 0–7 implementados y commiteados; el runner FUSE pasa 686 casos. CB y DDFDCB tienen implementadas las entradas `00`–`3F` (rotaciones).
- `Patterns/Rotate.cs` sirve de modelo: `RotateEmission.MemoryBody` (lectura, `Internal(dir, 1)`, operación, escritura) y `RotateMemoryCopyPattern` (formas con `CopyTo` + `InnerInstruction`).
- `FinishIndexed` (`Z80Cpu.cs`) ya fija `WZ = ii + d` antes de `ExecuteIndexedCB`.
- Los operandos `Bit` llevan su número en `Operand.Value`.
- `DispatchEmitter` agrupa los alias con el mismo cuerpo y añade `Q` según `WritesFlags`.
- Tests que hay que adaptar:
  - `Z80CpuTests.UnimplementedDispatchCountsFetches`: filas `CB 80`, `FD CB FE 86` y `DD CB 80 86`, y la comprobación `Internal(3, 2)` de DDCB;
  - `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset`: fila `DD CB 0 86` (línea ~225);
  - `GeneratorTests`: `64 implemented / 192 pending` (dos veces, línea ~452) y la cobertura por tabla.
- **Hallazgos de la primera implementación** (árbol de trabajo actual, sin commit):
  - El runner da 1266 pasados y 4 fallos: `cb4e`, `cb5e`, `cb6e`, `cb76`. Solo difieren F5/F3 de F. Los fixtures de FUSE calculan F5/F3 de `BIT b,(HL)` con el **valor leído** (modelo de FUSE anterior a MEMPTR), mientras que la spec y el hardware los toman de `WZ >> 8`. En `cb46 cb56 cb66 cb7e` el valor tiene los bits 3 y 5 a 0 y coincide por casualidad con `WZ = 0`. Ningún WZ inicial satisface los ocho casos. Resolución en la spec 2.2.1.
  - Con CB y DDFDCB completas, el `default: Unimplemented(); break;` que emite `DispatchEmitter.cs` (línea ~123) es inaccesible y produce dos warnings **CS0162** en `Z80Cpu.CB.g.cs` y `Z80Cpu.IndexedCB.g.cs`. Resolución en la spec 4.4.
  - `FuseCpuState.Diff` (`ZXSinclair.Net.Fuse/FuseCpuState.cs`) ya compara A y F por separado (`Flags("F", …)`), así que se puede ignorar un subconjunto de bits de F sin tocar la comparación de registros.

## Pasos
1. **Auxiliares** — nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Bits.cs` (cabecera GPL, `partial`, `AggressiveInlining`) con `BitTest(byte mask, byte value)` y `BitTestMemory(byte mask, byte value)`. Ambos calculan `F = (F & C) | H | (SZ53P[value & mask] & (S | Z | PV)) | (x & (F5 | F3))`, con `x = value` en `BitTest` y `x = WZ >> 8` en `BitTestMemory`.
2. **Patrones** — nuevo `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Bits.cs`. La máscara se emite en hexadecimal a partir de `Operand.Value`; `SET`/`RES` en memoria siguen la forma de `RotateEmission.MemoryBody`.
   - `WritesFlags = true`: `BitTestRegisterPattern` y `BitTestMemoryPattern` (`(HL)` y `(REGISTER+dd)` con sus alias; `address` en DDFDCB).
   - `WritesFlags = false`: `SetResRegisterPattern`, `SetResMemoryPattern` y `SetResMemoryCopyPattern` (registro `CopyTo` real).

   Registrarlos en `PatternCatalog.Default`.
3. **`default` solo en tablas incompletas** — `DispatchEmitter.cs`: emitir `default: Unimplemented(); break;` solo si algún valor 0–255 de la tabla no tiene `case` (opcode pendiente, hueco o prefijo). CB y DDFDCB quedan sin `default`; base, ED y DDFD lo conservan.
4. **Convención de FUSE para `BIT b,(HL)`** (spec 2.2.1), sin cambiar la CPU:
   - Nuevo `ZXSinclair.Net.Fuse/FuseConventions.cs` (cabecera GPL): tabla declarada `nombre de caso → (máscara de bits de F ignorados, motivo)` con `cb46 cb4e cb56 cb5e cb66 cb6e cb76 cb7e` → `0x28`, motivo "FUSE calcula F5/F3 de BIT n,(HL) con el valor leído, no con MEMPTR".
   - `FuseCpuState.Diff`: aceptar una máscara opcional de bits de F a ignorar y aplicarla solo a la comparación `Flags("F", …)`; el resto (A, registros, memoria, eventos, T-states) igual.
   - `FuseReport` y el runner (`ZXSinclair.Net.Test/Program.cs`): pasar la convención del caso; cuando se aplica y los bits ignorados difieren, el caso cuenta como pasado pero se lista (`convención aplicada: cb4e — F5/F3 ignorados (…)`), y el resumen final añade `pasados con convención: N`. Con `--verbose`, mostrar también los valores esperado y real de F.
5. **Regenerar** — `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --verbose`; CB y DDFDCB quedan en 256 de 256 y el resto no cambia. Revisar que los alias de `BIT b,(REGISTER+dd)` salen como un `case` múltiple de 8 etiquetas y que las copias usan registros reales.
6. **Tests del generador** — `GeneratorTests.cs`:
   - CB y DDFDCB `256 implemented / 0 pending`.
   - `BitPatterns_EmitExpectedBodies`, con muestras `BIT 7,A`, `BIT 0,(HL)`, el alias `BIT 0,(REGISTER+dd)`, `SET 3,C`, `RES 7,(HL)` y `LD L,SET 1,(REGISTER+dd)`.
   - `Patterns_WritesFlagsMatchesSpec` con los cinco patrones nuevos.
   - Que no se solapan con los patrones de rotación.
   - La aserción que exige `default: Unimplemented(); break;` en todos los ficheros pasa a exigirlo solo en tablas incompletas; nuevo `Output_OmitsDefaultWhenTableComplete`.
7. **Adaptar tests del Core**:
   - `UnimplementedDispatchCountsFetches`: quitar las tres filas CB/DDCB y la rama de `Internal(3, 2)`.
   - `LastUnimplementedAddress_TracksFetchAndReset`: `DD CB 0 86` → `DD ED 00`, con dirección final 3.
   - Revisar que `RotateTests` no usa opcodes `40`–`FF` como pendientes.
   - `FuseReportTests`: nuevos `Convention_IgnoresOnlyDeclaredFlagBits` (con `cb4e`: pasa ignorando F5/F3 y falla si difiere otro bit de F o un registro), `Convention_IsReportedInOutput` y `Conventions_ListOnlyBitHlCases`.
8. **Tests propios** — nuevo `ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs` con los 12 tests de la spec 5.2:
   - modelo de referencia exhaustivo de `BIT b,r`;
   - F5/F3 de WZ en `BIT b,(HL)`, también tras `LD A,(nn)`;
   - F5/F3 del byte alto de `ii+d` y los 8 alias;
   - `SET`/`RES` en cada forma, con accesos y T-states;
   - copias con H/L reales y `Q`.
9. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings (en concreto, sin CS0162 en los `.g.cs`).
   - `dotnet test ZXSinclair.Net.Core.Tests` y `dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests` en verde.
   - `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check` → 0.
   - Runner con `--filter cb4` a `cbf`, `ddcb` y `fdcb`; total 1270 pasados y 0 fallos, con `pasados con convención: 8` (los ocho `BIT b,(HL)`). Ante un fallo, `--filter <caso> --verbose`.
   - `--filter cb4e --verbose` muestra la convención aplicada y los F5/F3 esperado y real.
   - `--list-skipped` debe mostrar solo opcodes de los grupos 9–11.
10. **Benchmark** — repetir `ExecuteFrame`, `ExecuteLoopFrame` y `ExecuteAluLoopFrame` en Release y anotar el resultado en `Specs/spec-cpu-z80.md` 8.4.
11. **Documentación** — `Specs/spec-proceso-instrucciones.md` (grupo 8 Implementado, 584 casos, 1270 acumulados), `Specs/estado-instrucciones-z80.md` (`BIT`, `SET` y `RES` con enlaces y recuento), `Specs/spec-cpu-z80.md` 8.1 y 8.4, y `CLAUDE.md`/`AGENTS.md` (convenciones FUSE declaradas en `FuseConventions` y regla de `default` del generador; `Specs/spec-generador-z80.md` 4.2 con la regla del `default`; `Specs/spec-cpu-z80.md` 8.1 con el recuento de pasados con convención).

## Riesgos / cosas a vigilar
- `BIT b,(HL)` toma F5/F3 de `WZ >> 8`, no del valor leído ni de H; `BIT b,(ii+d)` los toma del byte alto de la dirección (WZ ya lo fijó `FinishIndexed`).
- S solo vale 1 con el bit 7 a 1; lo garantiza `SZ53P[value & mask]`.
- Copias DDCB de `SET`/`RES`: el registro destino es el real (H/L), no IXH/IXL. `BIT` no tiene copias.
- `SET`/`RES` no escriben flags (`Q = 0`); `BIT` sí (`Q = F`).
- Formato del informe de cobertura con 0 pendientes: comprobar el texto real antes de fijar la aserción.
- La convención de FUSE no debe convertirse en un "ignorar fallos": solo los ocho casos declarados, solo la máscara `0x28` de F, y siempre informada en la salida. No adaptar la CPU al fixture.
- Quitar el `default` cambia el texto generado: `--check` y `Output_MatchesCommittedFiles` deben regenerarse en el mismo cambio.

## Fuera de alcance
- Grupos 9–11 (intercambios, bloques, E/S y huecos de ED) y su efecto en WZ.

## Resultado

- Core: 2485 tests; generador: 126 tests; todos pasados. Los tests propios amplían los doce montajes con wraparound de PC/R e interrupciones tras cada patrón.
- Build Debug: 0 warnings y 0 errores; generator `--check`: 0. CB y DDFDCB: 256/256.
- FUSE completo: 1270 pasados, 0 fallos, 65 omitidos; 8 pasados con convención declarada. Filtros cb4…cbf, ddcb y fdcb en verde; cb4e verbose informa F esperado=18, real=10.
- Benchmarks: 0 B en los tres métodos; intervalos antes/después solapados. Resultados y límites de la comparación en spec CPU 8.4.
- Sin commit ni push.
