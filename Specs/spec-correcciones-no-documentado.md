# Especificación: correcciones de comportamiento no documentado

Dos correcciones sobre grupos ya implementados, detectadas al contrastar el proyecto con `Docs/z80-no-documentado.md` (sección 8):

1. **`SCF`/`CCF` tras un prefijo `DD`/`FD`**: el prefijo cuenta como instrucción anterior que no modificó flags, así que `SCF`/`CCF` deben ver `Q = 0`.
2. **MEMPTR (WZ) de `INIR`/`INDR`/`OTIR`/`OTDR` al repetir**: pasa a valer `PC + 1`, como en `LDxR`/`CPxR`.

Ninguna la detecta FUSE: no hay casos `DD 37`/`FD 3F`… y FUSE no compara WZ ni `Q`. Las verifican tests propios.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80-no-documentado.md`, secciones 3.4, 4 y 8.
- David Banks, *Z80Decoder — Undocumented Flags*: `SCF`/`CCF` y `Q` por modelo; prefijo `DD`/`FD` delante de `SCF`/`CCF` (TonyB, 2026).
- Spectrum Computing, *New discovery on Z80 I/O block instructions* (finales de 2023): MEMPTR de `INxR`/`OTxR` al repetir, confirmado en Zilog y NEC reales; corrección de z80test 1.2 → 1.2a.
- Specs afectadas: `Specs/spec-instr-control.md` 2.2 (`Q`) y `Specs/spec-instr-io.md` 2.3 (WZ).

## 2. `SCF`/`CCF` tras un prefijo `DD`/`FD`

### 2.1 Comportamiento

- En el Z80 NMOS de Zilog, F5/F3 de `SCF`/`CCF` = `((Q ^ F) | A) & 0x28`, con `Q` = F escrito por la instrucción anterior, o 0 si no escribió flags (spec del grupo 5).
- Un `DD` o `FD` delante de `SCF`/`CCF` (por ejemplo, `DD 37`, `FD 3F`) actúa como "instrucción anterior" que **no** modificó flags: `SCF`/`CCF` ven `Q = 0`, y F5/F3 = `(F | A) & 0x28`, aunque la instrucción previa al prefijo hubiera escrito flags.
- Con una cadena de prefijos (`DD FD 37`) el resultado es el mismo.
- Tras `SCF`/`CCF` con prefijo, `Q = F`, como sin prefijo (lo escribe el generador).
- El resto de instrucciones con prefijo no leen `Q` y terminan escribiéndolo (`Q = F` o `Q = 0` según su patrón), así que no cambian.

### 2.2 Implementación

- `Z80Cpu.cs`, `ExecuteIndexed<TIndex>`: tras consumir la cadena de prefijos `DD`/`FD` y antes de `FinishIndexed`, poner `Registers.Q = 0`.
- Coste: una escritura de un byte por instrucción **con prefijo** `DD`/`FD`; las instrucciones sin prefijo no cambian. Comprobar con los benchmarks (6).
- No hace falta tocar el generador ni `SetCarry`/`ComplementCarry`.
- Las rutas `DDCB`/`FDCB` y `DD ED`/`FD ED` también pasan por este punto: sus instrucciones escriben `Q` al terminar, así que el cambio no las afecta.

## 3. MEMPTR de `INxR`/`OTxR` al repetir

### 3.1 Comportamiento

| Instrucción | WZ si **termina** (`B = 0`) | WZ si **repite** (`B ≠ 0`) |
|---|---|---|
| `INIR` / `INDR` | `BC ± 1` con B antes de decrementar (sin cambios) | `PC + 1` |
| `OTIR` / `OTDR` | `BC ± 1` con B después de decrementar (sin cambios) | `PC + 1` |

- `PC` es la dirección del prefijo `ED`, es decir, el PC **después** de `PC -= 2`: la misma regla que `LDxR`/`CPxR` (spec del grupo 9).
- Se aplica en el ciclo extra de 5 T que retrocede PC, después de calcular WZ como la instrucción simple, así que lo sustituye.
- `INI`, `IND`, `OUTI`, `OUTD` no cambian.

### 3.2 Implementación

- `Z80Cpu.Io.cs`, `BlockInCore` y `BlockOutCore`: en la rama que repite, después de `Registers.PC -= 2`, poner `Registers.WZ = (ushort)(Registers.PC + 1)`.
- Sin coste en las no repetitivas; una escritura más por iteración que repite.

## 4. Pruebas

### 4.1 FUSE

Sin casos afectados. El runner debe seguir en **1335 pasados, 0 fallos, 0 omitidos** (8 con convención).

### 4.2 Tests propios (xUnit)

En `ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs`:

| Test | Montaje | Comprobación |
|---|---|---|
| `ScfCcf_AfterIndexPrefix_UseQZero` (`[Theory]`: `DD 37`, `FD 37`, `DD 3F`, `FD 3F`) | `A = 00` y `CP 28h` (escribe flags: F5/F3 = `0x28`, del operando; A sigue a 0), seguido de la instrucción con prefijo | F5/F3 = `(F \| A) & 0x28` = `0x28` (con `Q = F` habría salido `0x00`); C/H/N como `SCF`/`CCF`; S, Z, P/V conservados; 8 T; `Q = F` al terminar |
| `ScfCcf_WithoutPrefix_KeepPreviousQ` | La misma secuencia sin prefijo (`CP 28h` + `SCF`/`CCF`) | F5/F3 = `((Q ^ F) \| A) & 0x28` = `0x00` (comportamiento actual) |
| `ScfCcf_AfterPrefixChain_UseQZero` | `DD FD 37` tras una instrucción que escribe flags | Igual que con un prefijo; 12 T |
| `IndexPrefix_OtherInstructionsSetQAsBefore` (`[Theory]`) | `DD 84` (`ADD A,IXH`), `FD 21 nn` (`LD IY,nn`), `DD CB d 06` (`RLC (IX+d)`) | `Q = F` / `Q = 0` / `Q = F`, como antes del cambio |

En `ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs`:

| Test | Montaje | Comprobación |
|---|---|---|
| `BlockIoRepeat_WzIsPcPlusOne` (`[Theory]`: `INIR`, `INDR`, `OTIR`, `OTDR`) | `B = 2`, el `ED` en `0x2800` (y en `0xFFFF`, con vuelta), `BC` y `HL` elegidos para que `BC ± 1` sea distinto de `PC + 1` | Tras la iteración que repite: `WZ = 0x2801` (y `0x0000` con el `ED` en `0xFFFF`) |
| `BlockIoFinalIteration_WzClassic` (`[Theory]`, 4) | `B = 1` | `WZ = BC ± 1` según 3.1 (B antes de decrementar en `INxR`, después en `OTxR`) |

Nota de implementación: se descartó el test previsto `BlockIoRepeat_WzVisibleThroughBitHl` (ver `BIT 0,(HL)` en la rutina de interrupción tras una iteración que repite). No puede observar el valor: al aceptar la interrupción, WZ pasa a la dirección de la rutina (`0x0038` en IM 1, regla de MEMPTR), y la iteración que termina también sobrescribe WZ. El valor `PC + 1` se comprueba directamente en `Registers.WZ`.

### 4.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| `IoTests.BlockIo_AccessesWzWrapAndPrefixes` | En los casos que repiten (`INIR`/`INDR`/`OTIR`/`OTDR` con `B = 2`), el WZ esperado pasa a `PC + 1` (PC ya decrementado) |
| `IoTests.RepeatUntilBZeroAndInterruptResume` y `Repeat_FlagsSeenByInterruptAt2800` | Si comparan el `Z80Registers` completo tras una iteración que repite, WZ esperado = `PC + 1` |
| Tests de `ControlTests` que ejecuten `SCF`/`CCF` con prefijo | Revisar que el F esperado use `Q = 0` |

## 5. Documentación

- `Specs/spec-instr-control.md` 2.2: añadir que un prefijo `DD`/`FD` anula la historia de `Q` para `SCF`/`CCF`, con la fuente.
- `Specs/spec-instr-io.md` 2.3: sustituir la nota "las repetitivas de E/S no ponen `WZ = PC + 1` (verificar)" por la regla de 3.1, con la fuente, y quitarla de 8.
- `Specs/spec-cpu-z80.md`: 8.4 con la medición de 6.
- `Docs/z80-no-documentado.md` 8: marcar ambas filas como implementadas.
- `CLAUDE.md`/`AGENTS.md`: quitar la mención a "two known gaps" del resumen del documento de no documentado.

## 6. Rendimiento

- `Q = 0` en `ExecuteIndexed`: solo en instrucciones con prefijo `DD`/`FD`. Repetir `ExecuteAluLoopFrame` (incluye `ADD A,(IX+5)`) y el resto de benchmarks de `Z80CpuBenchmarks`; la diferencia esperada está dentro del ruido.
- WZ al repetir en E/S en bloque: fuera del bucle caliente salvo `OTIR`/`INIR`; no hace falta benchmark propio.

## 7. Criterios de aceptación

- `Registers.Q = 0` en `ExecuteIndexed` y `WZ = PC + 1` al repetir en `BlockInCore`/`BlockOutCore`.
- Tests de 4.2 en verde y los de 4.3 adaptados; `dotnet test` de los dos proyectos en verde; compilación sin warnings.
- Runner FUSE en 1335 / 0 / 0.
- Benchmarks repetidos sin regresión fuera del ruido.
- Documentación de 5 actualizada.

## 8. Fuera de alcance

- Comportamiento de `SCF`/`CCF` en NEC y CMOS (`Docs/z80-no-documentado.md` 3.4 y 6).
- Integración de z80test 1.2a, que contrastaría ambas correcciones con hardware.

## 9. Pendiente de verificar

- Si el prefijo anula `Q` también para un `SCF`/`CCF` alcanzado tras `DD`/`FD` seguido de otro prefijo `ED`/`CB` (combinaciones sin sentido práctico; con la implementación de 2.2 también ven `Q = 0`).
