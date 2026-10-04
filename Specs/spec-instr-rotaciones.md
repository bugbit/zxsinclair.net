# Especificación: grupo 7, rotaciones y desplazamientos

Grupo 7 de `Specs/spec-proceso-instrucciones.md`: el *Rotate and Shift Group* del manual de Zilog (`RLCA`, `RLA`, `RRCA`, `RRA`, `RLC`, `RL`, `RRC`, `RR`, `SLA`, `SRA`, `SRL`, `RLD`, `RRD`) más el `SLL` no documentado, en sus formas de registro, `(HL)` y `(IX+d)`/`(IY+d)`, incluidas las copias no documentadas `DDCB`/`FDCB` a registro. Es el primer grupo que llena las tablas `CB` y `DDFDCB`. Sigue la estructura de `Specs/spec-instr-alu-16.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulo *Rotate and Shift Group*.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus.
- Sean Young, *The Undocumented Z80 Documented*: `SLL`, copias `DDCB` a registro, F3/F5, MEMPTR.
- FUSE: 198 casos (sección 5.1) y entradas de `opcodes_base.dat`, `opcodes_ed.dat`, `opcodes_cb.dat` y `opcodes_ddfdcb.dat`. Los eventos de `cb06`, `ddcb06`, `ddcb00` y `ed67` confirman la temporización de la tabla 2.1.

## 2. Semántica

Notación de ciclos de spec CPU 4.4. `m` es `r` ∈ {A, B, C, D, E, H, L}, `(HL)` o `(ii+d)`. Los T-states incluyen los M1 de los prefijos.

### 2.1 Operaciones y temporización

| Instrucción | Opcodes | Ciclos de bus | T | WZ |
|---|---|---|---|---|
| `RLCA` / `RRCA` / `RLA` / `RRA` | `07` / `0F` / `17` / `1F` | `pc:4` | 4 | — |
| `rot r` | `CB 00`–`CB 3F` salvo `(HL)` (56) | `pc:4, pc+1:4` | 8 | — |
| `rot (HL)` | `CB 06 0E 16 1E 26 2E 36 3E` | `pc:4, pc+1:4, hl:3, hl:1, hl(w):3` | 15 | — |
| `rot (ii+d)` | `DD`/`FD` `CB d 06 0E 16 1E 26 2E 36 3E` | `pc:4, pc+1:4, pc+2:3, pc+3:3, pc+3:1 ×2, ii+d:3, ii+d:1, ii+d(w):3` | 23 | `ii + d` |
| `LD r,rot (ii+d)` | `DD`/`FD` `CB d` `00`–`3F` salvo `x6`/`xE` (56) | Igual que `rot (ii+d)` | 23 | `ii + d` |
| `RLD` / `RRD` | `ED 6F` / `ED 67` | `pc:4, pc+1:4, hl:3, hl:1 ×4, hl(w):3` | 18 | `HL + 1` |

`rot` ∈ {RLC, RRC, RL, RR, SLA, SRA, SLL, SRL}. En las formas `DDCB` el ciclo de prefijo (`pc:4, pc+1:4, pc+2:3, pc+3:3, pc+3:1 ×2`) ya lo hace `FinishIndexed` (spec CPU 4.3); la instrucción añade la lectura, 1 ciclo interno sobre la misma dirección y la escritura. La R solo sube 2 (los M1 de `DD` y `CB`).

### 2.2 Resultado y flags

`v` = valor de entrada, `c` = flag C anterior.

| Operación | Resultado | C nuevo |
|---|---|---|
| `RLC` | `(v << 1) \| (v >> 7)` | bit 7 de `v` |
| `RRC` | `(v >> 1) \| (v << 7)` | bit 0 de `v` |
| `RL` | `(v << 1) \| c` | bit 7 de `v` |
| `RR` | `(v >> 1) \| (c << 7)` | bit 0 de `v` |
| `SLA` | `v << 1` | bit 7 |
| `SRA` | `(v >> 1) \| (v & 0x80)` | bit 0 |
| `SLL` (no documentada) | `(v << 1) \| 1` | bit 7 |
| `SRL` | `v >> 1` | bit 0 |

- **CB y DDCB** (todas las anteriores): `F = SZ53P[resultado] | C nuevo` (S, Z, F5, F3 del resultado; P/V paridad; H = N = 0).
- **`RLCA`, `RRCA`, `RLA`, `RRA`** (sobre A): `F = (F & (S | Z | PV)) | (A nuevo & (F5 | F3)) | C nuevo`; H = N = 0.
- **`RLD`**: `(HL)' = ((HL) << 4) | (A & 0x0F)`; `A = (A & 0xF0) | ((HL) >> 4)`.
- **`RRD`**: `(HL)' = (A << 4) | ((HL) >> 4)`; `A = (A & 0xF0) | ((HL) & 0x0F)`.
- **`RLD`/`RRD`**: `F = (F & C) | SZ53P[A]` (H = N = 0, C conservado).
- Copias `LD r,rot (ii+d)`: el resultado se escribe en memoria **y** en `r` (incluidos H y L reales, no IXH/IXL). FUSE `ddcb00`: `B = 43` y `(1DAE) = 43`.

### 2.3 WZ y `Q`

- Todas las instrucciones `DDCB`/`FDCB` (de este grupo y del 8) ponen `WZ = ii + d`. Se hace una sola vez en `FinishIndexed`, antes de despachar, para que lo compartan las 256 entradas **(cambio en el bucle de prefijos escrito a mano)**.
- `RLD`/`RRD`: `WZ = HL + 1`. El resto no toca WZ.
- Todas las instrucciones del grupo escriben flags: sus patrones devuelven `WritesFlags = true` (`Q = F`).

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `CB 30`–`CB 37` | **No documentada**: `SLL` | 8 / 15 |
| `DD`/`FD` `CB d` `00`–`3F` salvo `x6`/`xE` | **No documentadas**: copia del resultado a registro | 23 |
| `DD`/`FD` `07 0F 17 1F` | El prefijo no afecta: como sin prefijo (regla de ausentes) | 8 |
| `DD`/`FD` `CB` con `0x40`–`0xFF` | `BIT`/`RES`/`SET`: grupo 8 | — |
| `CB 40`–`CB FF` | Grupo 8 | — |

## 4. Implementación

### 4.1 Auxiliares escritos a mano (Core)

Nuevo `partial` `ZXSinclair.Net.Core/Z80/Z80Cpu.Rotate.cs`, `[MethodImpl(MethodImplOptions.AggressiveInlining)]`, sin ramas:

| Método | Comportamiento |
|---|---|
| `byte Rlc(byte v)`, `Rrc`, `Rl`, `Rr`, `Sla`, `Sra`, `Sll`, `Srl` | Devuelven el resultado y ponen `F = SZ53P[r] \| c` (2.2) |
| `void Rlca()`, `Rrca()`, `Rla()`, `Rra()` | Operan sobre A con los flags de 2.2 |
| `void Rld()`, `Rrd()` | Lectura de `(HL)`, `bus.Internal(HL, 4)`, escritura, A, flags y `WZ = HL + 1` |

`Z80Cpu.cs`, `FinishIndexed`: guardar `Registers.WZ = address` (2.3) al calcular la dirección de `DDCB`.

### 4.2 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Rotate.cs`, añadidos a `PatternCatalog.Default`; todos con `WritesFlags = true`.

| Patrón | Reconoce | Cuerpo emitido (ejemplo) |
|---|---|---|
| `RotateAccumulatorPattern` | `RLCA`, `RRCA`, `RLA`, `RRA` | `Rlca();` |
| `RotateRegisterPattern` | `rot r` en la tabla CB | `Registers.B = Rlc(Registers.B);` |
| `RotateMemoryPattern` | `rot (HL)` (CB) y `rot (REGISTER+dd)` (DDFDCB, `EmitContext.IndexedCB`) | `var value = bus.Read(Registers.HL); bus.Internal(Registers.HL, 1); value = Sla(value); bus.Write(Registers.HL, value);` (con `address` en DDFDCB) |
| `RotateMemoryCopyPattern` | Formas compuestas de DDFDCB `LD r,rot (REGISTER+dd)` (`Opcode.CopyTo` + `InnerInstruction`) | Igual que la anterior con `address` y, al final, `Registers.B = value;` |
| `RotateDigitPattern` | `RLD`, `RRD` | `Rld();` |

- El mnemónico se traduce a su auxiliar con una tabla fija en el generador (`RLC` → `Rlc`…).
- En DDFDCB la dirección es siempre el parámetro `address` (`OperandEmitter.Address` con `IndexedCB`); `CopyTo` es un `Register8` real (H y L, no IXH/IXL).
- Las entradas `0x40`–`0xFF` de CB y DDFDCB (BIT/RES/SET) no casan: quedan para el grupo 8.

### 4.3 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | 246 | 4 (`07 0F 17 1F`) |
| ED | 46 | 2 (`67 6F`) |
| CB | 64 | 64 (`00`–`3F`) |
| DD/FD | 246 | 4 (por la regla de ausentes) |
| DDFDCB | 64 | 64 (`00`–`3F`) |

### 4.4 Rendimiento

- Auxiliares de una o dos operaciones de bits y una lectura de `SZ53P`; sin ramas.
- `WZ` en `FinishIndexed` es una escritura más solo en el camino `DDCB`.
- Hito de la guía (tras el grupo 7): repetir los tres benchmarks de spec CPU 8.4 y, si la ROM del 48K ya puede arrancar con los grupos implementados (falta E/S y `EX`), dejarlo anotado para el grupo 10.

## 5. Pruebas

### 5.1 FUSE

198 casos:

| Bloque | Casos |
|---|---|
| Base y ED (6) | `07 0f 17 1f ed67 ed6f` |
| CB (64) | `cb00`–`cb3f` |
| DDCB (64) | `ddcb00`–`ddcb3f` |
| FDCB (64) | `fdcb00`–`fdcb3f` |

El runner debe pasar de 488 a **686** pasados, 0 fallos. Verificar con `--filter cb`, `ddcb`, `fdcb`, `ed6`, `0`, `1`; ante un fallo, `--filter <caso> --verbose`. Convenciones de FUSE que afectan al grupo: ninguna nueva (FUSE no compara WZ ni `Q`).

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs`, con `TestBus` y el estado inicial de `NopTests` (`I = 0x42`, `Q = 0`).

| Test | Montaje | Comprobación |
|---|---|---|
| `Rotations_AllValues_MatchReference` (`[Theory]`, 8) | Modelo de referencia independiente; `v` de 0 a 255 × C 0/1 con `CB 00`…`CB 38` sobre B | Resultado y F exactos |
| `AccumulatorRotations_AllValues` (`[Theory]`, 4) | `07 0F 17 1F` con A de 0 a 255 × C × F inicial con S/Z/PV a 1 | A, C, F3/F5 de A; S, Z, P/V conservados; H = N = 0 |
| `RotateRegister_AllRegisters` (`[Theory]`, 8 ops × 7 regs) | `CB 00`–`CB 3F` salvo `(HL)` | Solo cambia el registro y F; 8 T; R + 2 |
| `RotateHl_ReadInternalWrite` (`[Theory]`, 8) | `CB 06`…`CB 3E` | Orden `M1`, `M1`, `Read HL`, `Internal(HL, 1)`, `Write HL`; 15 T; WZ intacto |
| `RotateIndexed_ReadInternalWrite` (`[Theory]`) | `DD CB d 06`, `FD CB d 3E` con `d` = `+7`, `-1`, `-128` | Accesos completos; `WZ = ii + d`; 23 T; R + 2 |
| `RotateIndexedCopy_AllRegisters` (`[Theory]`, 7) | `DD CB d 00`…`07` salvo `06` | Memoria y registro con el resultado (H y L reales); IXH/IXL intactos; 23 T |
| `Sll_IsUndocumentedShiftWithOne` (`[Theory]`) | `CB 30`, `CB 36`, `DD CB d 37` | `(v << 1) \| 1`; C = bit 7 |
| `Rld_Rrd_Nibbles` (`[Theory]`) | `ED 6F`, `ED 67` con `A = 36`, `(HL) = 93` (FUSE `ed67`) y otros | A, `(HL)`, F (C conservado), `WZ = HL + 1`; `Internal(HL, 4)`; 18 T |
| `IndexPrefix_UnaffectedAccumulatorRotations` (`[Theory]`) | `DD 07`, `FD 1F` | Efecto de base; +4 T |
| `Rotate_SetsQ` (`[Theory]`) | Un opcode de cada patrón | `Q = F` |
| `Rotate_DoesNotCountAsUnimplemented` | Un opcode de cada patrón | `UnimplementedOpcodes == 0` |

### 5.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| `Z80CpuTests.UnimplementedDispatchCountsFetches` | `CB 12`, `FD CB FE 12`, `DD CB 80 12` pasan a estar implementados: sustituirlos por opcodes del grupo 8 (`CB 80`, `FD CB FE 86`, `DD CB 80 86`). En el grupo 8 estas filas se eliminarán porque no quedarán opcodes CB pendientes |
| `Z80CpuTests.PrefixesAndOperandReadsWrapAtEndOfMemory` | `FD CB FE 12` (en `0xFFFE`) se ejecuta entero: comprobar la instrucción completa (lectura, ciclo interno y escritura en `IY - 2`, copia a D, 23 T) en lugar de cambiar de opcode, para que el test no dependa de qué falta por implementar |
| `FuseTestBusTests.IndexedPrefixCyclesMatchRealFuseEvents` | `ddcb00` ya no se trunca a 10 eventos: comparar la secuencia completa del caso |
| `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset` | `DD CB 0 0` → `DD CB 0 86` (grupo 8) |
| `GeneratorTests` (cobertura) | Cifras de 4.3: base y DD/FD `246`; `246 implemented / 6 pending / 4 prefixes`; ED `46 implemented / 32 pending`; CB `64 implemented / 192 pending`; DDFDCB `64 implemented / 192 pending`. Añadir `RotatePatterns_EmitExpectedBodies` (incluida una forma compuesta) y `WritesFlags` |

## 6. Criterios de aceptación

- Auxiliares de 4.1, `WZ` en `FinishIndexed` y patrones de 4.2; `*.g.cs` regenerados (no editados a mano) y `--check` a 0.
- Cobertura del generador igual a 4.3.
- Los 198 casos FUSE de 5.1 pasan con eventos; runner con 686 pasados y 0 fallos.
- Tests de 5.2 en verde y los de 5.3 adaptados; `dotnet test` de los dos proyectos en verde.
- Benchmarks de spec CPU 8.4 repetidos sin regresión ni asignaciones.
- Tabla de seguimiento de `spec-proceso-instrucciones.md`, `Specs/estado-instrucciones-z80.md` (entradas de rotación y desplazamiento marcadas), spec CPU 4.3 (`WZ` en `FinishIndexed`) y 8.1, y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- `BIT`, `SET`, `RES` y sus copias `DDCB` (grupo 8).
- Prefijos `DDCB` con `ED` u otras combinaciones raras más allá de spec CPU 4.3.

## 8. Verificación (2026-10-02)

- Build Debug de la solución: 0 errores, 0 warnings. Core: 2414 tests; generador: 117 tests. `--check` a 0; cobertura 246/64/46/246/64.
- FUSE con eventos: 686 pasados, 0 fallos, 649 omitidos. Pasan los 198 casos del grupo y los filtros de 5.1.
- `RotateTests` agrupa la tabla 5.2 por comportamiento: 1 048 576 combinaciones exhaustivas de valores y flags para CB, acumulador y RLD/RRD, más todas las formas de registro y memoria, ambos índices, copias a H/L reales, desplazamientos negativos, vuelta de dirección/PC, ciclos ordenados, R, WZ, Q y aceptación posterior de INT/NMI.
- El test de PC existente tiene DD y FD antes de CB: se conserva la cadena y se comprueban 27 T. La prueba FUSE `ddcb00` carga su estado y memoria iniciales y compara los eventos completos.
- Benchmarks antes/después: 0 B y solapamiento de intervalos en los tres métodos; medias y errores en spec CPU 8.4. ROM 48K y WebAssembly sin verificar.

### Pendiente

- Flags de las copias `DDCB` a registro frente a z80test (`z80full`), cuando se integre.
- WZ de `RLD`/`RRD` y `DDCB` frente a `z80memptr`.
