# Especificación: grupo 10, entrada/salida

Grupo 10 de `Specs/spec-proceso-instrucciones.md`: el *Input and Output Group* del manual de Zilog (`IN A,(n)`, `IN r,(C)`, `INI`, `INIR`, `IND`, `INDR`, `OUT (n),A`, `OUT (C),r`, `OUTI`, `OTIR`, `OUTD`, `OTDR`) más las formas no documentadas `IN F,(C)` y `OUT (C),0`. Con este grupo la tabla base queda completa y pasan todos los casos FUSE. Sigue la estructura de `Specs/spec-instr-bloques.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulo *Input and Output Group*.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus y contención de E/S.
- Sean Young, *The Undocumented Z80 Documented*: flags de `IN r,(C)`, `INI`/`OUTI` y sus repetitivas, `IN F,(C)`, `OUT (C),0`, MEMPTR.
- MAME: [z80.cpp, block_io_interrupted_flags](https://github.com/mamedev/mame/blob/master/src/devices/cpu/z80/z80.cpp), [z80.h, pv()](https://github.com/mamedev/mame/blob/master/src/devices/cpu/z80/z80.h) y [z80.lst, INIR/OTIR/INDR/OTDR](https://github.com/mamedev/mame/blob/master/src/devices/cpu/z80/z80.lst). Contrastados el 2026-10-03: F5/F3, H/P/V y WZ de la repetición.
- FUSE: 51 casos (sección 5.1). Los eventos de `db`, `d3`, `ed40`, `ed41`, `eda2`, `eda3`, `edb2`, `edb3`, `ed70` y `ed71` confirman la temporización y el orden de la tabla 2.1; en la segunda iteración de `edb2`/`edb3` el ciclo interno va a `IR` (`0004`) y no a PC.
- `Specs/spec-buses-memoria.md`: `In`/`Out` del bus aplican la contención de E/S y el puerto de la ULA; la CPU solo llama al bus.

## 2. Semántica

Notación de ciclos de spec CPU 4.4; `IO` es `bus.In(puerto)` / `bus.Out(puerto, v)` (4 T con su contención). Los T-states incluyen los M1 de los prefijos.

### 2.1 Operaciones y temporización

| Instrucción | Opcodes | Ciclos de bus | T | Puerto | Efecto |
|---|---|---|---|---|---|
| `IN A,(n)` | `DB` | `pc:4, pc+1:3, IO` | 11 | `A << 8 \| n` | `A = In` |
| `OUT (n),A` | `D3` | `pc:4, pc+1:3, IO` | 11 | `A << 8 \| n` | `Out(A)` |
| `IN r,(C)` | `ED 40 48 50 58 60 68 78` | `pc:4, pc+1:4, IO` | 12 | `BC` | `r = In` |
| `IN F,(C)` | `ED 70` | `pc:4, pc+1:4, IO` | 12 | `BC` | Solo flags; el dato se descarta |
| `OUT (C),r` | `ED 41 49 51 59 61 69 79` | `pc:4, pc+1:4, IO` | 12 | `BC` | `Out(r)` |
| `OUT (C),0` | `ED 71` | `pc:4, pc+1:4, IO` | 12 | `BC` | `Out(0)` (Z80 NMOS) |
| `INI` / `IND` | `ED A2` / `ED AA` | `pc:4, pc+1:4, ir:1, IO, hl:3` | 16 | `BC` (B antes de decrementar) | `(HL) = In`; `B -= 1`; HL `±1` |
| `INIR` / `INDR` | `ED B2` / `ED BA` | Como `INI`/`IND`; si `B ≠ 0`: `+ hl:1 ×5` y `PC -= 2` | 21 / 16 | Igual | Repite |
| `OUTI` / `OUTD` | `ED A3` / `ED AB` | `pc:4, pc+1:4, ir:1, hl:3, IO` | 16 | `BC` con **B ya decrementado** | `B -= 1`; `Out((HL))`; HL `±1` |
| `OTIR` / `OTDR` | `ED B3` / `ED BB` | Como `OUTI`/`OUTD`; si `B ≠ 0`: `+ bc:1 ×5` y `PC -= 2` | 21 / 16 | Igual | Repite |

- Orden (FUSE `eda2`): `INI` hace el ciclo interno sobre `IR`, la entrada desde `BC` con el B original, y después escribe en `HL`. `OUTI` (FUSE `eda3`): ciclo interno, lectura de `HL`, decremento de B y salida a `BC` con el B nuevo.
- Ciclos de repetición: `INIR`/`INDR` sobre la dirección `HL` en la que se escribió (antes de `±1`); `OTIR`/`OTDR` sobre `BC` (con el B ya decrementado). FUSE `edb2`, `edb3`.
- La repetición, como en el grupo 9, es una instrucción completa por iteración con `PC -= 2`; las interrupciones se aceptan entre iteraciones por el mecanismo normal.

### 2.2 Flags

| Instrucción | Flags |
|---|---|
| `IN A,(n)`, `OUT (n),A`, `OUT (C),r`, `OUT (C),0` | Ninguno |
| `IN r,(C)`, `IN F,(C)` | `F = (F & C) \| SZ53P[v]` (S, Z, F5, F3 de `v`; P/V paridad; H = N = 0; C se conserva) |
| `INI`/`IND`/`OUTI`/`OUTD` y cada iteración de las repetitivas | Ver abajo |

Bloques de E/S, con `v` = byte transferido y `B` = B **después** de decrementar:
- `k = v + ((C + 1) & 0xFF)` en `INI`/`INIR`; `k = v + ((C - 1) & 0xFF)` en `IND`/`INDR`; `k = v + L` en `OUTI`/`OTIR`/`OUTD`/`OTDR` (L **después** de `±1`).
- S, Z, F5, F3 de `B` (`SZ53[B]`); N = bit 7 de `v`; H = C = `k > 255`; P/V = paridad de `(k & 7) ^ B`.
- `F = SZ53[B] | ((v >> 6) & N) | (k > 255 ? H | C : 0) | Parity[(k & 7) ^ B]`.
- Comprobación: `eda2` (`v = 9A`, `C = 82`, `B → 99` → `F = 9F`); `eda3` (`v = B3`, `L → FB`, `B → 62` → `F = 33`).

**Iteraciones que repiten** (`INIR`/`INDR`/`OTIR`/`OTDR` con `B ≠ 0`): tras los flags anteriores y **después** de `PC -= 2`:
1. F5/F3 de PC, como en el grupo 9: `F = (F & ~0x28) | ((PC >> 8) & 0x28)`.
2. Ajuste de H y P/V (MAME, `block_io_interrupted_flags`, contrastado):
   - Si C = 1 (`k > 255`):
     - si N = 1 (bit 7 de `v`): `P/V ^= paridad impar de ((B - 1) & 7)`; H = `(B & 0x0F) == 0x00`.
     - si N = 0: `P/V ^= paridad impar de ((B + 1) & 7)`; H = `(B & 0x0F) == 0x0F`.
   - Si C = 0: `P/V ^= paridad impar de (B & 7)`; H no cambia.
   - "`P/V ^= paridad impar de x`" invierte P/V si `x` tiene un número impar de bits a 1: `F ^= (~Parity[x]) & PV`.
3. La iteración que termina (`B = 0`) conserva los flags clásicos.

Los fixtures FUSE `edb2`, `edb3`, `edba` y `edbb` terminan sus bucles en una iteración que no repite: pasarlos **no** demuestra este ajuste, que comprueban los tests propios (5.2).

### 2.3 WZ y `Q`

| Instrucción | WZ |
|---|---|
| `IN A,(n)` | `(A << 8 \| n) + 1` (A antes de la instrucción) |
| `OUT (n),A` | `(A << 8) \| ((n + 1) & 0xFF)` |
| `IN r,(C)`, `IN F,(C)`, `OUT (C),r`, `OUT (C),0` | `BC + 1` |
| `INI`/`INIR` | `BC + 1` con el B **antes** de decrementar |
| `IND`/`INDR` | `BC - 1` con el B antes de decrementar |
| `OUTI`/`OTIR` | `BC + 1` con el B **después** de decrementar |
| `OUTD`/`OTDR` | `BC - 1` con el B después de decrementar |

- Cuando `INIR`/`INDR`/`OTIR`/`OTDR` **repiten**, el ciclo extra de 5 T que retrocede PC pone `WZ = PC + 1` (PC = dirección del `ED`, ya decrementado), igual que `LDxR`/`CPxR`; la iteración que termina conserva la regla de la tabla. Descubierto a finales de 2023 y confirmado en Zilog y NEC reales (Spectrum Computing, *New discovery on Z80 I/O block instructions*; z80test 1.2a). Implementado según `Specs/spec-correcciones-no-documentado.md`.
- `Q`: escriben flags (`WritesFlags = true`) `IN r,(C)`, `IN F,(C)` y los ocho de bloque; no escriben (`Q = 0`) `IN A,(n)`, `OUT (n),A`, `OUT (C),r` y `OUT (C),0`. En las repetitivas, `Q` recoge el F con los ajustes de repetición.

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `ED 70` | **No documentada**: `IN F,(C)` / `IN (C)`; flags sin escribir registro | 12 |
| `ED 71` | **No documentada**: `OUT (C),0` (0 en NMOS; 0xFF en CMOS, fuera de alcance) | 12 |
| `DD`/`FD` `D3`, `DB` | El prefijo no afecta (regla de ausentes) | 15 |
| `DD`/`FD` `ED …` | `ED` anula el prefijo índice (spec CPU 4.3) | +4 |

## 4. Implementación

### 4.1 Auxiliares escritos a mano (Core)

Nuevo `partial` `ZXSinclair.Net.Core/Z80/Z80Cpu.Io.cs`, `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:

| Método | Comportamiento |
|---|---|
| `void InAccumulator()` | `n = ReadPc()`; `port = A << 8 \| n`; `WZ = port + 1`; `A = bus.In(port)` |
| `void OutAccumulator()` | `n = ReadPc()`; `bus.Out(A << 8 \| n, A)`; `WZ = (A << 8) \| ((n + 1) & 0xFF)` |
| `byte InRegister()` | `WZ = BC + 1`; `v = bus.In(BC)`; flags de 2.2; devuelve `v` (para `IN F,(C)` se descarta) |
| `void OutRegister(byte value)` | `bus.Out(BC, value)`; `WZ = BC + 1` |
| `void BlockIn(int step)` / `BlockInRepeat(int step)` | `Internal(IR, 1)`; `WZ = BC ± 1`; `v = In(BC)`; `Write(HL, v)`; B, HL; flags; si repite: `Internal(HL anterior, 5)`, `PC -= 2` y ajustes de 2.2 |
| `void BlockOut(int step)` / `BlockOutRepeat(int step)` | `Internal(IR, 1)`; `v = Read(HL)`; `B -= 1`; `Out(BC, v)`; HL; `WZ = BC ± 1`; flags; si repite: `Internal(BC, 5)`, `PC -= 2` y ajustes |

`step` es constante en cada llamada generada. El ajuste de repetición vive en un auxiliar común (`BlockIoRepeatFlags(byte value)`) para que lo compartan las cuatro repetitivas.

### 4.2 `TestBus` de los tests del Core

`ZXSinclair.Net.Core.Tests/TestBus.cs` hoy no registra E/S y devuelve siempre `port >> 8`. Para los tests de este grupo:
- registrar `("In", puerto, valor)` y `("Out", puerto, dato)` en `Accesses`;
- permitir fijar el valor de entrada (`TestBusState.InputData`, por ejemplo una cola de bytes; si está vacía, `port >> 8` como ahora).

Ningún test existente ejecuta E/S, así que el cambio no afecta a sus aserciones.

### 4.3 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Io.cs`, añadidos a `PatternCatalog.Default`:

| Patrón | Reconoce | Cuerpo emitido (ejemplo) | `WritesFlags` |
|---|---|---|---|
| `InImmediatePattern` / `OutImmediatePattern` | `IN A,(nn)` / `OUT (nn),A` | `InAccumulator();` / `OutAccumulator();` | no |
| `InRegisterPattern` | `IN r,(C)` y `IN F,(C)` | `Registers.B = InRegister();` / `InRegister();` | sí |
| `OutRegisterPattern` | `OUT (C),r` y `OUT (C),0` | `OutRegister(Registers.B);` / `OutRegister(0);` | no |
| `BlockIoPattern` | `INI`, `IND`, `INIR`, `INDR`, `OUTI`, `OUTD`, `OTIR`, `OTDR` | `BlockInRepeat(1);`, `BlockOut(-1);` | sí |

- `IN F,(C)`: el operando `F` es `Register8`; el patrón lo trata como "solo flags" y no escribe F con el dato (los flags ya los fija `InRegister`).
- `OUT (C),0`: el operando `0` es `Constant`.

### 4.4 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | **252** (todos salvo los 4 prefijos) | 2 (`D3 DB`) |
| ED | **78** (todos los opcodes; quedan 178 huecos, grupo 11) | 24 |
| DD/FD | 252 | 2 (regla de ausentes) |
| CB, DDFDCB | 256 | 0 |

Base y DD/FD siguen teniendo `default` (los bytes de prefijo no tienen `case`), así que no aparece CS0162 (regla del grupo 8).

### 4.5 Rendimiento

- E/S es poco frecuente en el bucle caliente salvo la lectura del teclado y el borde (`OUT (FE)`); los auxiliares son cortos y sin asignaciones.
- Con este grupo la CPU ejecuta todo el código documentado: es el hito de la guía para medir el **arranque de la ROM del 48K**. La ROM no está en el repositorio: añadir `Z80CpuBenchmarks.ExecuteRomBootFrames` que lea la ROM desde una ruta local configurable (variable de entorno `ZX_ROM_48K`) y se omita con un mensaje si no existe; mide frames por segundo y × tiempo real desde el reset hasta un número fijo de frames. Repetir también los benchmarks existentes y anotar todo en spec CPU 8.4 **(verificar las condiciones de distribución de la ROM antes de incluirla en el repositorio)**.

## 5. Pruebas

### 5.1 FUSE

51 casos:

| Bloque | Casos |
|---|---|
| Base (8) | `d3 d3_1 d3_2 d3_3 db db_1 db_2 db_3` |
| `IN`/`OUT` con `(C)` (16) | `ed40 ed41 ed48 ed49 ed50 ed51 ed58 ed59 ed60 ed61 ed68 ed69 ed70 ed71 ed78 ed79` |
| Bloques simples (23) | `eda2 eda2_01`–`eda2_03`, `eda3 eda3_01`–`eda3_11`, `edaa edaa_01`–`edaa_03`, `edab edab_01 edab_02` |
| Bloques repetitivos (4) | `edb2 edb3 edba edbb` |

Recuento comprobado con `--list-skipped`: 8 + 16 + 23 + 4 = 51.

El runner debe pasar de 1284 a **1335** pasados, 0 fallos y **0 omitidos** (8 con la convención de `BIT (HL)`). Verificar con `--filter d3`, `db`, `ed4`…`ed7`, `eda`, `edb`; ante un fallo, `--filter <caso> --verbose`.

Convenciones de FUSE que afectan al grupo: el bus de FUSE devuelve en `In` el byte alto del puerto; los bucles `edb*` terminan en una iteración que no repite, así que no cubren los ajustes de 2.2; FUSE no compara WZ ni `Q`.

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs`, con el `TestBus` ampliado (4.2) y el estado inicial de `NopTests` (`I = 0x42`, `Q = 0`).

| Test | Montaje | Comprobación |
|---|---|---|
| `InAccumulator_PortAndWz` | `DB 34` con `A = 12` y entrada `5A` | Puerto `1234`; `A = 5A`; F intacto; `WZ = 1235`; 11 T; `Q = 0` |
| `OutAccumulator_PortAndWz` (`[Theory]`) | `D3 34` y `D3 FF` con `A = 12` | Puerto `1234`/`12FF`, dato `12`; `WZ = 1235` / `1200` |
| `InRegister_AllRegistersAndFlags` (`[Theory]`, 7) | `ED 40`…`78` con entradas `00`, `80`, `29`, `FF` | Registro; F = `(F & C) \| SZ53P[v]`; `WZ = BC + 1`; 12 T; `Q = F` |
| `InF_OnlyFlags` | `ED 70` | Ningún registro cambia salvo F |
| `OutRegister_AllRegisters` (`[Theory]`, 7) | `ED 41`…`79` | Puerto `BC`, dato del registro; F intacto; `WZ = BC + 1` |
| `OutZero_WritesZero` | `ED 71` | Dato `00` |
| `BlockIo_FlagsMatchReference` (`[Theory]`, 4) | Modelo de referencia independiente; `INI`, `IND`, `OUTI`, `OUTD` con `v`, B, C y L variados (incluidos `k` = 255 y 256, B = 1) | F exacto (2.2) |
| `Ini_AccessesAndWz` | `ED A2` | Orden `Internal(IR, 1)`, `In(BC original)`, `Write HL`; B, HL; `WZ = BC + 1` (B original); 16 T |
| `Outi_AccessesAndWz` | `ED A3` | Orden `Internal(IR, 1)`, `Read HL`, `Out(BC con B nuevo)`; `WZ = BC + 1` (B nuevo); 16 T |
| `Inir_Otir_RepeatUntilBZero` (`[Theory]`) | `ED B2`, `ED B3` con `B = 3` | 21, 21, 16 T; ciclos de repetición en `HL` escrito / `BC`; PC vuelve al `ED`; R `+= 2` por iteración |
| `BlockIoRepeat_F5F3FromPcHigh` (`[Theory]`, 4 × 4) | `INIR`, `INDR`, `OTIR`, `OTDR` con una iteración que repite y el `ED` en `0x0000`, `0x0800`, `0x2000`, `0x2800` | F5/F3 = `(PC >> 8) & 0x28` tras repetir; `Q = F` |
| `BlockIoRepeat_HAndParityAdjust` (`[Theory]`) | Casos que cubren las tres ramas de 2.2 (C = 1 con N = 1, C = 1 con N = 0, C = 0) y los bordes de H (`B & 0x0F` = `00`/`0F`) | H y P/V según el ajuste; resto de F |
| `BlockIoFinalIteration_KeepsClassicFlags` (`[Theory]`, 4) | `B = 1` | Flags clásicos |
| `BlockIoRepeat_FlagsSeenByInterrupt` | Iteración que repite en `0x2800`, INT con IM 1 y `PUSH AF` en `0x0038` | F apilado con los ajustes; `Q = 0` tras aceptar la INT |
| `Io_SpectrumBusContention` | `OUT (FE),A` e `IN A,(FE)` sobre `Z80Cpu<SpectrumBus>` en un T-state contenido | T-states según la contención de E/S de `SpectrumBus` (spec de buses) |
| `Io_DoesNotCountAsUnimplemented` | Un opcode de cada patrón | `UnimplementedOpcodes == 0` |

### 5.3 Tests existentes que hay que adaptar

Tras este grupo los únicos opcodes sin implementar son los huecos de ED (y las respuestas de IM 0 distintas de `RST`), así que los tests que usan `D3` como opcode pendiente deben cambiar:

| Test | Cambio |
|---|---|
| `Z80CpuTests.UnimplementedDispatchCountsFetches` | `{D3}` → `{ED, 00}` (8 T, R + 2); `{DD, D3}`/`{FD, D3}` → `{DD, ED, 00}`/`{FD, ED, 00}` (12 T, R + 3); `{DD, FD, D3}`/`{FD, DD, D3}` → con `ED 00` al final (16 T, R + 4) |
| `Z80CpuTests.LongAlternatingPrefixChainUsesConstantStackSpace` | El último byte (`0xFFFF`) deja de poder ser `D3`: terminar la cadena con `ED` en `0xFFFF` y `00` en `0x0000` (hueco de ED) y ajustar ciclos/PC, o comprobar la ejecución completa de `OUT (n),A` |
| `NopTests.Nop_DoesNotCountAsUnimplemented` | `0, D3, 0` → `0, ED, 00, 0` |
| `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset` | Filas `{D3}` y `{DD, D3}` → `{ED, 00}` (ya existe) y `{DD, ED, 00}`; eliminar duplicados |
| `GeneratorTests` (cobertura) | base y DD/FD `252 implemented / 0 pending / 4 prefixes`; ED `78 implemented / 0 pending` con `178 holes (178 pending)`. Añadir `IoPatterns_EmitExpectedBodies` y `WritesFlags`; comprobar que base, DD/FD y ED conservan `default` |

## 6. Criterios de aceptación

- Auxiliares de 4.1, `TestBus` de 4.2 y patrones de 4.3; `*.g.cs` regenerados (no editados a mano), `--check` a 0 y compilación sin warnings.
- Cobertura del generador igual a 4.4.
- Los 51 casos FUSE de 5.1 pasan con eventos; runner con **1335 pasados, 0 fallos, 0 omitidos**.
- Tests de 5.2 en verde y los de 5.3 adaptados; `dotnet test` de los dos proyectos en verde.
- Benchmarks repetidos y, si hay ROM local, `ExecuteRomBootFrames` medido; resultados en spec CPU 8.4.
- Tabla de seguimiento de `spec-proceso-instrucciones.md`, `Specs/estado-instrucciones-z80.md` (entradas de E/S con enlaces y recuento), spec CPU 8.1 y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- Huecos de ED e IM 0 con instrucciones distintas de `RST` (grupo 11).
- Comportamiento CMOS (`OUT (C),0` con `FF`).
- Periféricos concretos del Spectrum más allá de lo que ya implementa `SpectrumBus` (teclado, cinta, AY…).

## 8. Pendiente de verificar

- Ajuste de H y P/V de las iteraciones que repiten frente a z80test cuando se integre; ya se contrastó con MAME.
- Medición del arranque con una ROM local del 48K. No se distribuye una ROM de terceros.

## 9. Implementación y verificación (2026-10-03)

Los cinco patrones y auxiliares están implementados. BlockIn/BlockInRepeat y BlockOut/BlockOutRepeat delegan en núcleos comunes con un booleano constante; todos llevan AggressiveInlining. La repetición conserva el byte transferido en una variable local para ajustar flags. TestBus registra entradas/salidas y dispone de una cola InputData.

MAME guarda P/V mediante un operando cuya paridad calcula pv(). Su asignación final no representa directamente el bit F: al convertirla, coincide con invertir P/V por paridad impar de x. La spec 2.2 se mantiene. Los macros repetitivos retroceden PC y ajustan flags sin volver a escribir WZ.

Core: 2605 tests, con 56 casos propios de E/S y un duplicado eliminado. Generador: 154, con 15 ejemplos de emisión nuevos. Las reglas de los 16 tests de 5.2 se agrupan en las teorías de IoTests; además se cubren vuelta de PC/HL/BC/R, prefijos DD/FD, registros completos y retorno tras INT. Los modelos independientes prueban todas las combinaciones de valor y byte bajo, con B y los bordes de H/P/V variados.

Solución Debug: cero advertencias y errores; generador `--check`: código 0. FUSE: 1335 pasados, 0 fallos y 0 omitidos, con eventos activados y las 8 convenciones existentes de BIT (HL). Filtros: d3/db 4 cada uno, ed4/ed5/ed6 16 cada uno, ed7 14, eda 27 y edb 8. Cobertura: base/DD-FD 252, ED 78, CB/DD-FDCB 256; quedan 178 huecos de ED.

Los cuatro benchmarks existentes conservan 0 B y sus intervalos se solapan con el control anterior; detalle en spec CPU 8.4. El benchmark opcional de ROM omite el arranque si falta ZX_ROM_48K. Su montaje se verificó con una imagen sintética JP 0000, sin presentarla como arranque real.
