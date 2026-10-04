# Especificación: grupo 9, intercambio, transferencia y búsqueda en bloque

Grupo 9 de `Specs/spec-proceso-instrucciones.md`: el *Exchange, Block Transfer, and Search Group* del manual de Zilog (`EX DE,HL`, `EX AF,AF'`, `EXX`, `EX (SP),HL`, `EX (SP),IX/IY`, `LDI`, `LDIR`, `LDD`, `LDDR`, `CPI`, `CPIR`, `CPD`, `CPDR`). Las instrucciones de bloque de E/S (`INI`, `OTIR`…) son del grupo 10. Sigue la estructura de `Specs/spec-instr-bits.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulo *Exchange, Block Transfer, and Search Group*.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus, incluidas las repeticiones.
- David Banks, [Undocumented Flags: LDXR / CPXR interrupted](https://github.com/hoglet67/Z80Decoder/wiki/Undocumented-Flags#ldxr--cpxr-interrupted): F5/F3 de PC durante la repetición.
- Sean Young, *The Undocumented Z80 Documented*: F3/F5 de `LDI`/`CPI` y MEMPTR de las repetitivas.
- FUSE: 14 casos (sección 5.1) y entradas de `opcodes_base.dat`, `opcodes_ddfd.dat` y `opcodes_ed.dat`. Los eventos de `e3`, `eda0`, `eda1`, `edb0` y `edb1` confirman la temporización y las direcciones de los ciclos internos de la tabla 2.1; `edb0` (16 iteraciones, 331 T) y `edb1` (79 T) recorren bucles completos.

## 2. Semántica

Notación de ciclos de spec CPU 4.4. Los T-states incluyen los M1 de los prefijos.

### 2.1 Operaciones y temporización

| Instrucción | Opcodes | Ciclos de bus | T | Efecto |
|---|---|---|---|---|
| `EX DE,HL` | `EB` | `pc:4` | 4 | Intercambia DE y HL |
| `EX AF,AF'` | `08` | `pc:4` | 4 | `Registers.ExchangeAF()` |
| `EXX` | `D9` | `pc:4` | 4 | `Registers.Exx()` (BC, DE, HL con sus alternativos) |
| `EX (SP),HL` | `E3` | `pc:4, sp:3, sp+1:3, sp+1:1, sp+1(w):3, sp(w):3, sp:1 ×2` | 19 | Intercambia `(SP)`/`(SP+1)` con L/H |
| `EX (SP),ii` | `DD`/`FD` `E3` | `pc:4, pc+1:4` + lo anterior | 23 | Igual con IX/IY |
| `LDI` / `LDD` | `ED A0` / `ED A8` | `pc:4, pc+1:4, hl:3, de:3, de:1 ×2` | 16 | `(DE) = (HL)`; HL, DE `±1`; `BC -= 1` |
| `LDIR` / `LDDR` | `ED B0` / `ED B8` | Como `LDI`/`LDD`; si `BC ≠ 0` tras decrementar: `+ de:1 ×5` y `PC -= 2` | 21 / 16 | Repite |
| `CPI` / `CPD` | `ED A1` / `ED A9` | `pc:4, pc+1:4, hl:3, hl:1 ×5` | 16 | Compara A con `(HL)`; HL `±1`; `BC -= 1` |
| `CPIR` / `CPDR` | `ED B1` / `ED B9` | Como `CPI`/`CPD`; si `BC ≠ 0` y no hay coincidencia: `+ hl:1 ×5` y `PC -= 2` | 21 / 16 | Repite |

- **Direcciones de los ciclos internos** (FUSE): en `LDI`/`LDIR` usan la dirección `DE` en la que se acaba de escribir (antes de incrementarla), tanto los 2 ciclos de la operación como los 5 de la repetición; en `CPI`/`CPIR`, la dirección `HL` leída (antes de incrementarla). En `EX (SP),HL`, 1 ciclo sobre `SP+1` entre lecturas y escrituras y 2 sobre `SP` al final.
- **Orden de `EX (SP),HL`**: lee `(SP)` y `(SP+1)`, ciclo interno, escribe **H en `SP+1`** y después **L en `SP`** (FUSE `e3`).
- **Repetición**: no hay un bucle dentro de la CPU. Cada iteración es una instrucción completa que, si debe repetir, deja `PC` apuntando otra vez al prefijo `ED` (`PC -= 2`). Así cada iteración vuelve a hacer los dos M1 (R `+= 2`), y las interrupciones se aceptan entre iteraciones por el mecanismo normal de `Step()`.
- `HL`, `DE` y `BC` dan la vuelta a 16 bits.

### 2.2 Flags

`v` = byte transferido o comparado; `bc` = BC **después** de decrementar.

| Instrucción | S, Z | H | P/V | N | C | F5 / F3 |
|---|---|---|---|---|---|---|
| `EX`, `EXX` | — (F intacto; `EX AF,AF'` cambia F por F') | | | | | |
| `LDI`/`LDD`/`LDIR`/`LDDR` | Se conservan | 0 | `bc ≠ 0` | 0 | Se conserva | `n = A + v`: F5 = bit 1 de `n`, F3 = bit 3 de `n` |
| `CPI`/`CPD`/`CPIR`/`CPDR` | De `r = A - v` | Préstamo del bit 4 de `A - v` | `bc ≠ 0` | 1 | Se conserva | `n = r - H`: F5 = bit 1 de `n`, F3 = bit 3 de `n` |

Fórmulas:
- `LDI`: `n = A + v`; `F = (F & (S | Z | C)) | (bc != 0 ? PV : 0) | (n & F3) | ((n << 4) & F5)`.
- `CPI`: `r = A - v`; `h = (A ^ v ^ r) & H`; `n = r - (h >> 4)`; `F = (F & C) | N | (SZ53[(byte)r] & (S | Z)) | h | (bc != 0 ? PV : 0) | (n & F3) | ((n << 4) & F5)`.
- Comprobación: `eda0` (`A = 1B`, `F = C9`, `v = B7`, `BC = 3D11` → `F = E5`); `eda1` (`A = EC`, `F = DB`, `v = B4`, `BC = 7666` → `F = 0F`).
- En las repetitivas cada iteración calcula primero los flags de la no repetitiva (fórmulas anteriores). **Si la iteración repite**, después de `PC -= 2` se sustituyen F5 y F3 por los bits 13 y 11 de PC (bits 5 y 3 de su byte alto): `F = (F & ~0x28) | ((PC >> 8) & 0x28)`. El resto de flags (S, Z, H, P/V, N, C) se mantiene. La **iteración que termina** (`BC = 0`, o coincidencia en `CPxR`) conserva los flags clásicos.
- `Q` es el F final de cada iteración, ya con el ajuste de repetición si lo hubo (`Q = F`).
- Si se acepta una interrupción entre iteraciones, F (el que se ve en la rutina de servicio, por ejemplo con `PUSH AF`) es el de la última iteración ejecutada: con el ajuste de PC si repitió.
- **Convención de FUSE:** los fixtures `edb0`, `edb1`, `edb8` y `edb9` ejecutan el bucle hasta su última iteración, que no repite; sus flags finales son los clásicos. Pasar esos casos **no** demuestra la regla de repetición: la comprueban los tests propios de 5.2.

### 2.3 WZ y `Q`

| Instrucción | WZ |
|---|---|
| `EX (SP),HL` / `EX (SP),ii` | Nuevo valor de HL / ii |
| `LDIR`/`LDDR` que repite | `PC + 1` (PC tras `PC -= 2`, es decir, la dirección del `ED` más 1) |
| `LDI`/`LDD`, `LDIR`/`LDDR` que termina | Sin cambios |
| `CPI` / `CPD` | `WZ + 1` / `WZ - 1` |
| `CPIR`/`CPDR` que repite | `PC + 1` |
| `CPIR`/`CPDR` que termina | `WZ + 1` / `WZ - 1` (como `CPI`/`CPD`) |
| `EX DE,HL`, `EX AF,AF'`, `EXX` | Sin cambios |

- `Q`: `LDI`… y `CPI`… escriben flags (`WritesFlags = true`, `Q = F`). `EX DE,HL`, `EXX` y `EX (SP),HL` no (`Q = 0`). `EX AF,AF'` cambia F sin pasar por la ALU: `Q = 0` según la decisión provisional del grupo 5 **(verificar con `z80ccf`)**.

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `DD`/`FD` `E3` | `EX (SP),IX` / `EX (SP),IY` | 23 |
| `DD`/`FD` `EB` | `EX DE,HL`: el prefijo **no** afecta (opera sobre HL, no sobre IX/IY; regla de ausentes) | 8 |
| `DD`/`FD` `08`, `D9` | El prefijo no afecta | 8 |
| `DD`/`FD` `ED A0`… | `ED` anula el prefijo índice (spec CPU 4.3) | +4 |
| `ED A2`/`A3`/`AA`/`AB`/`B2`/`B3`/`BA`/`BB` | Bloques de E/S: grupo 10 | — |

## 4. Implementación

### 4.1 Auxiliares escritos a mano (Core)

El ciclo extra de las iteraciones que repiten (5 T internos, `PC -= 2`, `WZ = PC + 1`, F5/F3 de PC) lo hace un único auxiliar, `RepeatBlock(ushort internalAddress)`, compartido con las repetitivas de E/S (`Specs/spec-correcciones-revision.md` 1).

Nuevo `partial` `ZXSinclair.Net.Core/Z80/Z80Cpu.Block.cs`, `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:

| Método | Comportamiento |
|---|---|
| `ushort ExchangeStack(ushort value)` | `low = Read(SP)`, `high = Read(SP + 1)`, `Internal(SP + 1, 1)`, `Write(SP + 1, value >> 8)`, `Write(SP, value)`, `Internal(SP, 2)`; `WZ` = resultado; devuelve `low \| high << 8` |
| `void BlockLoad(int step)` | Una iteración de `LDI` (`step = +1`) o `LDD` (`-1`): lectura, escritura, `Internal(DE, 2)`, HL/DE/BC y flags de 2.2 |
| `void BlockLoadRepeat(int step)` | `BlockLoad(step)`; si `BC ≠ 0`: `Internal(DE anterior, 5)`, `PC -= 2`, `WZ = PC + 1`, `F = (F & ~0x28) \| ((PC >> 8) & 0x28)` |
| `void BlockCompare(int step)` | Una iteración de `CPI`/`CPD`: lectura, `Internal(HL, 5)`, HL, BC, `WZ ± 1` y flags |
| `void BlockCompareRepeat(int step)` | `BlockCompare(step)`; si `BC ≠ 0` y `Z = 0`: `Internal(HL anterior, 5)`, `PC -= 2`, `WZ = PC + 1`, `F = (F & ~0x28) \| ((PC >> 8) & 0x28)` |

- `step` es una constante en cada llamada generada; tras el inlining el JIT la pliega. La dirección de los ciclos de repetición es la de la iteración (antes de `±1`), que el auxiliar guarda en una variable local.
- `EX DE,HL`, `EX AF,AF'` y `EXX` se emiten en línea (`(Registers.DE, Registers.HL) = (Registers.HL, Registers.DE);`, `Registers.ExchangeAF();`, `Registers.Exx();`).

### 4.2 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Block.cs`, añadidos a `PatternCatalog.Default`:

| Patrón | Reconoce | Cuerpo emitido (ejemplo) | `WritesFlags` |
|---|---|---|---|
| `ExchangeRegistersPattern` | `EX DE,HL`, `EX AF,AF'`, `EXX` | `(Registers.DE, Registers.HL) = (Registers.HL, Registers.DE);` | no |
| `ExchangeStackPattern` | `EX (SP),HL`, `EX (SP),REGISTER` | `Registers.HL = ExchangeStack(Registers.HL);` / `TIndex.Pair(ref Registers) = ExchangeStack(TIndex.Pair(ref Registers));` | no |
| `BlockLoadPattern` | `LDI`, `LDD`, `LDIR`, `LDDR` | `BlockLoad(1);`, `BlockLoadRepeat(-1);` | sí |
| `BlockComparePattern` | `CPI`, `CPD`, `CPIR`, `CPDR` | `BlockCompare(1);`, `BlockCompareRepeat(-1);` | sí |

- `EX AF,AF'` lleva el operando `AF'` (`Register16`); el patrón lo reconoce por mnemónico y operandos exactos.
- `EX (SP),HL` tiene `(SP)` como `MemoryPair`: no casa con ningún patrón de carga.

### 4.3 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | 250 (pendientes: `D3`, `DB`) | 4 (`08 D9 E3 EB`) |
| ED | 54 (pendientes: 16 de E/S y 8 de bloques de E/S) | 8 (`A0 A1 A8 A9 B0 B1 B8 B9`) |
| DD/FD | 250 | 4 (`E3` propio + `08 D9 EB` por la regla de ausentes) |
| CB, DDFDCB | 256 | 0 |

### 4.4 Rendimiento

- `LDIR`/`LDDR` son de lo más ejecutado en programas reales (copias y borrados de memoria): cada iteración debe ser un camino corto sin asignaciones; las ramas son solo las de terminación.
- Añadir `Z80CpuBenchmarks.ExecuteBlockCopyFrame`: un `LDIR` de un bloque grande en RAM no contenida que se reinicia en bucle (`LD HL/DE/BC`, `LDIR`, `JR`), ns por T-state y × tiempo real; anotarlo en spec CPU 8.4 junto a los tres anteriores, repetidos.

## 5. Pruebas

### 5.1 FUSE

14 casos:

| Bloque | Casos |
|---|---|
| Intercambio (6) | `08 d9 e3 eb dde3 fde3` |
| Bloques simples (4) | `eda0 eda1 eda8 eda9` |
| Bloques repetitivos (4) | `edb0 edb1 edb8 edb9` |

El runner debe pasar de 1270 a **1284** pasados, 0 fallos (los 51 restantes son de E/S, grupo 10). Verificar con `--filter e`, `08`, `d9`, `dde3`, `fde3`, `eda`, `edb`; ante un fallo, `--filter <caso> --verbose`. Convenciones de FUSE que afectan al grupo: los bucles de `edb*` terminan en una iteración que no repite y solo comprueban sus flags clásicos, así que no cubren el ajuste F5/F3 de las iteraciones que repiten (2.2); FUSE no compara WZ ni `Q`.

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs`, con `TestBus` y el estado inicial de `NopTests` (`I = 0x42`, `Q = 0`).

| Test | Montaje | Comprobación |
|---|---|---|
| `ExDeHl_ExAf_Exx` (`[Theory]`) | `EB`, `08`, `D9` | Solo cambian los registros intercambiados; F intacto salvo `EX AF,AF'`; WZ intacto; 4 T; `Q = 0` |
| `ExDeHl_IgnoresIndexPrefix` (`[Theory]`) | `DD EB`, `FD EB` | Intercambia DE y HL; IX/IY intactos; 8 T |
| `ExStack_ReadsThenWritesHighFirst` | `E3` con `SP = 0x9000` | Orden `Read SP`, `Read SP+1`, `Internal(SP+1, 1)`, `Write SP+1 (H)`, `Write SP (L)`, `Internal(SP, 2)`; `WZ` = nuevo HL; 19 T |
| `ExStack_Index` (`[Theory]`) | `DD E3`, `FD E3` | IX/IY; 23 T |
| `ExStack_WrapsAtFFFF` | `E3` con `SP = 0xFFFF` | Usa `0xFFFF` y `0x0000` |
| `Ldi_Ldd_FlagsMatchReference` (`[Theory]`, 2) | Modelo de referencia; A y `v` de 0 a 255 con `BC` = 1 y 2, F inicial variado | F exacto (F5/F3 de `A + v`, P/V de `BC ≠ 0`, S/Z/C conservados) |
| `Cpi_Cpd_FlagsMatchReference` (`[Theory]`, 2) | Igual para la comparación | F exacto (F5/F3 de `r - H`) |
| `Ldi_Accesses` | `ED A0` | `Read HL`, `Write DE`, `Internal(DE, 2)` sobre la dirección escrita; HL/DE `+1`, BC `-1`; 16 T; WZ intacto |
| `Ldir_RepeatsUntilBcZero` | `ED B0` con `BC = 3` | Tres `Step()`: 21, 21, 16 T; PC vuelve al `ED` en las dos primeras; R `+= 2` por iteración; `WZ = PC + 1` tras repetir; memoria copiada |
| `Lddr_RepeatInternalAddress` | `ED B8` con `BC = 2` | `Internal(DE escrito, 5)` en la repetición |
| `Ldir_InterruptBetweenIterations` | `LDIR` largo e INT activa con IM 1 | La INT se acepta entre iteraciones y apila la dirección del `ED`; al volver, el bloque continúa |
| `RepeatF5F3_FromPcHigh` (`[Theory]`, 4 instrucciones × 4 PC) | `LDIR`, `LDDR`, `CPIR` (sin coincidencia), `CPDR` (sin coincidencia) con `BC = 2`, ejecutando una iteración que repite con el `ED` en `0x0000`, `0x0800`, `0x2000` y `0x2800` (byte alto con F5/F3 = 00, solo F3, solo F5, ambos), y con A/F/datos elegidos para que los F5/F3 clásicos sean distintos de los de PC | Tras la iteración que repite: F5/F3 = bits 5 y 3 de `PC >> 8` (PC ya decrementado), el resto de F igual al clásico, `Q = F` |
| `RepeatFinalIteration_KeepsClassicFlags` (`[Theory]`, 4) | Las mismas cuatro con `BC = 1` (y, en `CPxR`, también terminando por coincidencia con `BC > 1`) en las mismas direcciones | F5/F3 clásicos de 2.2 (no los de PC); `Q = F` |
| `RepeatFlags_SeenByInterrupt` (`[Theory]`, 4) | Una iteración que repite en `0x2800` seguida de INT con IM 1, y en `0x0038` un `PUSH AF` | El F apilado tiene F5/F3 de PC (`0x28`); tras aceptar la INT, `Q = 0` |
| `Cpir_StopsOnMatch` | `ED B1` buscando un byte en la 3.ª posición | Termina con Z = 1, BC restante, HL tras el byte; T de cada iteración |
| `Cpir_StopsOnBcZero` | Sin coincidencia | Termina con P/V = 0; `WZ` según 2.3 |
| `Cpi_Cpd_Wz` (`[Theory]`) | `ED A1`, `ED A9` | `WZ + 1` / `WZ - 1` |
| `BlockWraps16Bits` (`[Theory]`) | `LDI` con HL/DE = `0xFFFF`, BC = 0 (BC → `0xFFFF`) | Vuelta de HL, DE y BC; P/V = 1 |
| `Block_SetsQ_ExchangeClearsQ` (`[Theory]`) | `LDI`, `CPI`, `EX DE,HL`, `EX AF,AF'` | `Q = F` / `Q = 0` |
| `Block_DoesNotCountAsUnimplemented` | Un opcode de cada patrón | `UnimplementedOpcodes == 0` |

### 5.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| `GeneratorTests` (cobertura) | base y DD/FD `250 implemented / 2 pending / 4 prefixes`; ED `54 implemented / 24 pending`; CB y DDFDCB sin cambios. Añadir `BlockPatterns_EmitExpectedBodies` y `WritesFlags` de los cuatro patrones |
| `Z80CpuTests`, `NopTests`, `FuseReportTests` | Sin cambios: usan `D3`, `DD D3`, `ED 00`/`ED 12` y `DD ED 00` (grupos 10 y 11) como pendientes |

## 6. Criterios de aceptación

- Auxiliares de 4.1 y patrones de 4.2; `*.g.cs` regenerados (no editados a mano), `--check` a 0 y compilación sin warnings.
- Cobertura del generador igual a 4.3.
- Los 14 casos FUSE de 5.1 pasan con eventos; runner con 1284 pasados y 0 fallos.
- Tests de 5.2 en verde; `dotnet test` de los dos proyectos en verde.
- `ExecuteBlockCopyFrame` añadido y medido; benchmarks anteriores sin regresión ni asignaciones; resultados en spec CPU 8.4.
- Tabla de seguimiento de `spec-proceso-instrucciones.md`, `Specs/estado-instrucciones-z80.md` (entradas de intercambio y bloque con enlaces y recuento), spec CPU 8.1 y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- Bloques de E/S (`INI`, `INIR`, `IND`, `INDR`, `OUTI`, `OTIR`, `OUTD`, `OTDR`) y el resto de E/S: grupo 10.

## 8. Pendiente de verificar

- La regla F5/F3 de las iteraciones que repiten (2.2) frente a z80test u otra prueba de hardware, cuando se integre. Este grupo no cambia H ni P/V al repetir (esos ajustes aparecen en los bloques de E/S del grupo 10).
- `Q` tras `EX AF,AF'` con `z80ccf`.
- WZ de las repetitivas frente a `z80memptr`.

## 9. Verificación de la implementación (2026-10-03)

Los helpers y cuatro patrones están implementados; dispatches regenerados y `--check` a 0. Core: 2550 tests, incluidos 65 del grupo; generador: 139, incluidos 13 ejemplos nuevos. La solución Debug compila sin advertencias ni errores. FUSE pasa los 14 casos del grupo y suma 1284 pasados, 0 fallos y 51 omitidos de E/S.

Las pruebas se agrupan en `ExDeHl_ExAf_Exx`, `ExStack_ReadsThenWritesHighFirst`, `FlagsMatchReferenceExhaustively`, `Block_AccessesAndRegisters`, `RepeatF5F3_FromPcHigh`, `RepeatFinalIteration_KeepsClassicFlags`, `RepeatFlags_SeenByInterrupt`, `Ldir_InterruptBetweenIterations`, `RepeatsUntilBcZeroOrMatch`, `BlockWraps16Bits`, `PcWrapsAndInterruptAcceptedAfterwards`, `Ldir_ZeroInitialCountCopies65536Bytes` y `Exchange_PcWrapsAndInterruptAcceptedAfterwards`. Estas teorías reúnen las reglas de 5.2 y añaden la copia completa con BC inicial cero. Los modelos de flags recorren A y valor de 0 a 255, BC inicial 1/2 y ocho combinaciones de S/Z/C.

`ExecuteBlockCopyFrame`: 0.5117 ns/T-state, 0 B asignados, unas 558 veces el tiempo real a 3.5 MHz. Los intervalos de los tres benchmarks anteriores se solapan con el control; detalle en spec CPU 8.4. Las verificaciones de hardware indicadas en sección 8 siguen pendientes.
