# Plan: grupo 1, carga de 8 bits

Fecha: 2026-10-02
Estado: implementado y verificado

## Objetivo
Implementar el grupo 1 (carga de 8 bits) según `Specs/spec-instr-carga-8.md`: auxiliares en el Core, patrones en el generador, despachos regenerados y tests propios, con los 163 casos FUSE del grupo pasando.

## Contexto
- El generador ya existe (`ZXSinclair.Net.Generate.Z80OpCodes`): `PatternCatalog.Default` solo tiene `NopPattern` (`Patterns/Control.cs`); `IPattern` = `Matches(Opcode)` + `EmitBody(Opcode, EmitContext)`.
- `OperandEmitter` traduce `(REGISTER+dd)` → `bus.Read(IndexedAddress<TIndex>())` y `nnnn` → `ReadPc16()`, pero **`ReadPc16` e `IndexedAddress<TIndex>` aún no existen en el Core**.
- `DispatchEmitter` agrupa por (cuerpo, comentario), sustituye los DD/FD ausentes por el opcode base y emite cuerpos de una o varias líneas.
- El Core tiene `ReadPc()`, `Push`/`Pop` y `Unimplemented()` en `ZXSinclair.Net.Core/Z80/Z80Cpu.cs`; flags en `Z80Flags` (`SZ53`, `PV`, `C`).
- Tests: `TestBus` registra `M1`/`Read`/`Write`/`Internal`; `NopTests` tiene el helper `Create` con registros distintos de cero.
- Tests del generador que dependen de la cobertura del piloto: `Output_PilotContainsOnlyNop` y `Coverage_IncludesHolesAliasesAndPendingDetails` (`ZXSinclair.Net.Generate.Z80OpCodes.Tests/GeneratorTests.cs`).
- Runner FUSE: hoy pasan 3 casos.

## Pasos
1. **Auxiliares compartidos** — `ZXSinclair.Net.Core/Z80/Z80Cpu.cs`, junto a `ReadPc()` (`AggressiveInlining`): `ReadPc16()` (byte bajo y alto con `ReadPc()`) e `IndexedAddress<TIndex>()` (lee `d`, `bus.Internal((ushort)(Registers.PC - 1), 5)`, `address = (ushort)(TIndex.Pair(ref Registers) + d)`, `WZ = address`, devuelve `address`).
2. **Auxiliares del grupo** — nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Load8.cs` (cabecera GPL, `partial`): `StoreIndexedImmediate<TIndex>()` (lee `d` y `n`, `Internal(PC-1, 2)`, WZ, `Write`) y `LoadAFromSpecial(byte value)` (`Internal(IR, 1)`, `A = value`, `F = (F & C) | SZ53[A] | (IFF2 ? PV : 0)`).
3. **Patrones** — nuevo `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Load8.cs` con los 12 patrones de la spec 4.2: `LoadRegisterRegister` (cuerpo vacío si destino == origen), `LoadRegisterImmediate`, `LoadRegisterIndirect` (WZ solo con BC/DE), `StoreIndirectRegister`, `StoreIndirectImmediate`, `LoadAccumulatorAbsolute`, `StoreAbsoluteAccumulator`, `LoadRegisterIndexed`, `StoreIndexedRegister`, `StoreIndexedImmediate`, `LoadAccumulatorSpecial` y `StoreSpecialAccumulator`. Todos aceptan `Kind` Instruction/Alias, mnemónico `LD`, dos operandos y clases exactas de 8 bits (nunca `Register16`, `IndexPair` ni `Immediate16` como valor). Usar variables locales cuando haya más de un acceso al bus. Registrarlos en `PatternCatalog.Default`.
4. **Regenerar** — `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --verbose`; comprobar la cobertura (base 78, ED 4, DDFD 78, CB 0, DDFDCB 0) y revisar el diff de los `*.g.cs`, sin editarlos a mano.
5. **Tests del generador** — `GeneratorTests.cs`:
   - Sustituir `Output_PilotContainsOnlyNop` por `Output_MatchesExpectedCoverage` (cifras de la spec 4.3).
   - Actualizar las cifras de `Coverage_IncludesHolesAliasesAndPendingDetails`.
   - Añadir `Load8Patterns_EmitExpectedBodies` con muestras: `LD B,C`, `LD B,B` (vacío), `LD A,(BC)` con WZ, `LD (REGISTER+dd),nn`, `LD H,(REGISTER+dd)` con H real, `LD REGISTERH,nn` y `LD A,I`.
   - Comprobar explícitamente que ningún patrón del grupo casa cargas de 16 bits (`LD BC,nnnn`, `LD SP,HL`, `LD (nnnn),HL`), además de `Patterns_AreDisjoint`.
6. **Tests propios** — nuevo `ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs` con los 24 tests de la spec 5.2, un helper `Create` como el de `NopTests` y comparación del `Z80Registers` completo; el `[Theory]` de `LD r,r'` genera los 49 casos a partir de índices de registro.
7. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings nuevos.
   - `dotnet test ZXSinclair.Net.Core.Tests` y `dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests` en verde.
   - `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check` → 0.
   - `dotnet run --project ZXSinclair.Net.Test`: los 163 casos del grupo pasan, total ≥ 166 y 0 fallos. Si alguno falla, revisar primero el orden de los ciclos y las direcciones de los ciclos internos.
   - `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*'` sin regresión ni asignaciones.
8. **Documentación** — `Specs/spec-proceso-instrucciones.md` (grupo 1 Implementado y casos FUSE), `Specs/spec-cpu-z80.md` 8.1 (cifras del runner) y 8.4 si se vuelve a medir, y `CLAUDE.md`/`AGENTS.md` donde digan que solo `NOP` está implementado o citen el número de casos FUSE.

## Riesgos / cosas a vigilar
- Orden de evaluación: `bus.Write(IndexedAddress<TIndex>(), r)` es correcto (C# evalúa de izquierda a derecha), pero se emiten variables locales siempre que haya más de un acceso para no depender de ello.
- `LD R,A`: el ciclo `Internal` debe usar IR antes de la escritura; `LD A,R` lee R antes del ciclo interno.
- Patrones demasiado amplios (que casen `LD BC,nnnn`, `LD SP,HL`, `LD (nnnn),HL` o `LD I,A` con `LoadRegisterRegister`): `Patterns_AreDisjoint` solo los detecta si chocan con otro patrón; de ahí la comprobación explícita del paso 5.
- Crecimiento de los `switch`: vigilar el benchmark aunque el grupo no exija uno propio.

## Fuera de alcance
- Grupos 2–11.
- `Q` y la peculiaridad NMOS de P/V tras `LD A,I`/`LD A,R` con INT.

## Resultado de la implementación

- Auxiliares Core, 12 patrones y despachos regenerados. Cobertura: base 78, ED 4, DD/FD 78, CB y DD/FD/CB 0.
- FUSE con eventos: 166 pasados (163 del grupo), 0 fallos, 1169 omitidos.
- Compilación Debug: 0 errores y 0 advertencias. Core: 342/342 tests; generador: 47/47; `--check`: código 0.
- Tests propios de las 24 reglas, comparación del estado completo, variantes DD/FD, flags con C activo e inactivo y aceptación de INT posterior.
- Adaptación por rendimiento: el generador emite auxiliares inline y retornos directos en la tabla base. Emisión directa inicial: 4.873 ns/NOP; final: 3.774 ns, frente a 3.768 ns del control aislado. Intervalos solapados y 0 B en ambas mediciones finales. Detalles en spec CPU 8.4.
- Añadido el seguimiento requerido por AGENTS.md: 22/150 entradas completadas, 128 pendientes.
- Sin commit ni push. La medición no cubre una mezcla de instrucciones ni WebAssembly.
