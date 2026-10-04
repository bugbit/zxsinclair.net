# Especificación: grupo 5, aritmética general y control de CPU

Grupo 5 de `Specs/spec-proceso-instrucciones.md`: el *General-Purpose Arithmetic and CPU Control Group* del manual de Zilog (`DAA`, `CPL`, `NEG`, `CCF`, `SCF`, `NOP`, `HALT`, `DI`, `EI`, `IM 0/1/2`) con los alias de ED. Incluye además la implementación del registro interno **`Q`**, que fija F3/F5 de `SCF`/`CCF` y que hasta ahora quedaba pendiente (spec CPU 3 y 10). Sigue la estructura de `Specs/spec-instr-alu-8.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulos *General-Purpose Arithmetic and CPU Control Groups* e *Interrupt Response*.
- Sean Young, *The Undocumented Z80 Documented*: `DAA`, F3/F5, `NEG` y modos IM de los alias de ED.
- Patrik Rak, documentación de z80test (`z80ccf`): registro `Q` y F3/F5 de `SCF`/`CCF` en el Z80 NMOS de Zilog.
- FUSE: 27 casos (sección 5.1) y entradas de `opcodes_base.dat` y `opcodes_ed.dat`. `NOP` ya está implementado (`spec-instr-nop.md`).

## 2. Semántica

Todas las instrucciones del grupo son `pc:4` (4 T) sin prefijo y `pc:4, pc+1:4` (8 T) las de ED. No acceden a memoria más allá del fetch ni tocan WZ.

| Instrucción | Opcodes | T | Efecto | Flags |
|---|---|---|---|---|
| `DAA` | `27` | 4 | Ajuste decimal de A (2.1) | Ver 2.1 |
| `CPL` | `2F` | 4 | `A = ~A` | H = N = 1; F5/F3 del nuevo A; S, Z, P/V, C se conservan |
| `NEG` | `ED 44` y 7 alias | 8 | `A = 0 - A` | Como `SUB` con `a = 0`, `v = A` (spec del grupo 4): C = 1 si A ≠ 0, P/V = 1 si A era `0x80` |
| `SCF` | `37` | 4 | — | C = 1, H = N = 0; S, Z, P/V se conservan; F5/F3 de 2.2 |
| `CCF` | `3F` | 4 | — | H = C anterior, C = ¬C anterior, N = 0; S, Z, P/V se conservan; F5/F3 de 2.2 |
| `NOP` | `00` | 4 | — (ya implementado) | — |
| `HALT` | `76` | 4 | `Halted = true`; `PC` vuelve a la dirección de `HALT` | — |
| `DI` | `F3` | 4 | `IFF1 = IFF2 = 0` | — |
| `EI` | `FB` | 4 | `IFF1 = IFF2 = 1`; `EiPending = true` | — |
| `IM 0` | `ED 46` y alias `4E 66 6E` | 8 | `IM = 0` | — |
| `IM 1` | `ED 56` y alias `76` | 8 | `IM = 1` | — |
| `IM 2` | `ED 5E` y alias `7E` | 8 | `IM = 2` | — |

### 2.1 `DAA`

Con `a` = A, y C, H, N los flags antes:
1. `diff = (C || a > 0x99 ? 0x60 : 0) | (H || (a & 0x0F) > 9 ? 0x06 : 0)`.
2. `A = N ? a - diff : a + diff`.
3. C nuevo = `C || a > 0x99`. H nuevo = `N ? H && (a & 0x0F) < 6 : (a & 0x0F) > 9`.
4. `F = SZ53P[A] | (F & N) | C nuevo | H nuevo`.

Comprobación con FUSE `27`: `A = 1F`, `F = 00` → `A = 25`, `F = 30`. Puede implementarse con estas expresiones o con una tabla precalculada indexada por A, C, H y N (2048 entradas de `AF`); la elección se valida con el benchmark (4.5).

### 2.2 Registro `Q` y F3/F5 de `SCF`/`CCF`

- En el Z80 NMOS de Zilog, `Q` vale el F escrito por la instrucción anterior si esa instrucción **modificó** los flags, y 0 si no los modificó.
- `SCF`/`CCF`: `F3/F5 = ((Q ^ F) | A) & (F5 | F3)`, con `Q` y `F` antes de la instrucción. Si la instrucción anterior escribió los flags, el resultado depende solo de A; si no, de `F | A`.
- Instrucciones que escriben flags (`Q = F` tras ejecutarse): ALU de 8 bits e `INC`/`DEC` (grupo 4), `LD A,I`/`LD A,R` (grupo 1), `DAA`, `CPL`, `NEG`, `SCF`, `CCF` (este grupo) y las de grupos posteriores que la spec de cada grupo marque.
- Instrucciones que no escriben flags (`Q = 0`): el resto, incluidos `NOP`, cargas, saltos, `HALT` (y cada M1 interno mientras está detenida), `DI`, `EI`, `IM`, y la aceptación de NMI e INT. `POP AF` y `EX AF,AF'` cambian F sin pasar por la ALU: se tratan como `Q = 0` **(verificar con `z80ccf`)**; la wiki *Z80Decoder* de David Banks confirma que `POP AF` no cuenta como instrucción que modifica flags.
- Un prefijo `DD`/`FD` delante de `SCF`/`CCF` (`DD 37`, `FD 3F`…) cuenta como instrucción anterior que no modificó flags: `SCF`/`CCF` ven `Q = 0` aunque la instrucción previa al prefijo escribiera flags (TonyB, 2026, recogido en *Z80Decoder — Undocumented Flags*). `ExecuteIndexed` pone `Q = 0` tras consumir los prefijos (`Specs/spec-correcciones-no-documentado.md`).
- Los prefijos `DD`/`FD` no son instrucciones: `Q` lo fija la instrucción que completan.

### 2.3 `HALT`, `DI` y `EI`

- `HALT` deja `PC` en su propia dirección (`PC -= 1` tras el fetch) y `Halted = true`; el bucle de `Step()` ya repite M1 sobre `PC` sin avanzar (spec CPU 4.2) y la aceptación de una interrupción sale de HALT con `PC + 1` (spec CPU 6.1). FUSE `76`: `PC = 0000`, `halted = 1`, 4 T.
- `DD 76` / `FD 76` ejecutan `HALT` (regla de ausentes) y `PC` vuelve a la dirección del `76`, no a la del prefijo **(verificar con FUSE si aparece un caso)**.
- `EI`: la INT no se acepta hasta completar la instrucción siguiente; `Step()` ya lo implementa con `EiPending`. Una cadena de `EI` mantiene bloqueada la INT. `DI` no tiene retraso.
- `HALT` con `IFF1 = 0` y sin NMI deja la CPU detenida indefinidamente (comportamiento real).

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `ED 4C 54 5C 64 6C 74 7C` | **No documentadas**: alias de `NEG` | 8 |
| `ED 4E 66 6E` | **No documentadas**: `IM 0` (`ED 4E`/`6E` se describen a veces como "IM 0/1"; FUSE y la tabla usan IM 0) | 8 |
| `ED 76` / `ED 7E` | **No documentadas**: `IM 1` / `IM 2` | 8 |
| `DD`/`FD` + `27 2F 37 3F 76 F3 FB` | El prefijo no afecta (regla de ausentes) | +4 |
| `ED` huecos | Grupo 11 | — |

## 4. Implementación

### 4.1 `Q` en el generador

- `IPattern` gana `bool WritesFlags(Opcode opcode)`; por defecto `false`. Devuelven `true`: `Alu8Pattern`, `IncDec8Register`, `IncDec8Memory`, `LoadAccumulatorSpecial` y los patrones de este grupo que escriben flags.
- `DispatchEmitter` añade al final de cada cuerpo implementado `Registers.Q = Registers.F;` si el patrón escribe flags y `Registers.Q = 0;` si no. Los cuerpos que devuelven los patrones (`EmitBody`) no cambian; solo el texto del `case`.
- Coste: una escritura de un byte por instrucción, sin lecturas ni ramas. Se mide con los benchmarks de spec CPU 8.4 (4.5); no se aceptan alternativas inexactas (por ejemplo, comparar F antes y después, que falla cuando una instrucción reescribe F con el mismo valor).
- `Z80Cpu`: `AcceptNmi`, `AcceptInterrupt` y el M1 interno de `HALT` en `Step()` ponen `Registers.Q = 0`. `Reset()` ya deja `Q = 0`.

### 4.2 Auxiliares escritos a mano (Core)

Nuevo `partial` `ZXSinclair.Net.Core/Z80/Z80Cpu.Control.cs`, `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:

| Método | Comportamiento |
|---|---|
| `void DecimalAdjust()` | 2.1 |
| `void Complement()` | `A = ~A`; `F = (F & (S \| Z \| PV \| C)) \| H \| N \| (A & (F5 \| F3))` |
| `void Negate()` | `value = A; A = 0; Sub8(value);` (reutiliza el grupo 4) |
| `void SetCarry()` | `F = (F & (S \| Z \| PV)) \| (((Q ^ F) \| A) & (F5 \| F3)) \| C` |
| `void ComplementCarry()` | `c = F & C; F = (F & (S \| Z \| PV)) \| (c << 4) \| (c ^ C) \| (((Q ^ F) \| A) & (F5 \| F3))` |
| `void Halt()` | `Halted = true; PC -= 1` |
| `void EnableInterrupts()` | `IFF1 = IFF2 = true; EiPending = true` |

`DI` e `IM n` se emiten en línea.

### 4.3 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Control.cs` (junto a `NopPattern`), añadidos a `PatternCatalog.Default`:

| Patrón | Reconoce | Cuerpo emitido | `WritesFlags` |
|---|---|---|---|
| `DecimalAdjustPattern` | `DAA` | `DecimalAdjust();` | sí |
| `ComplementPattern` | `CPL` | `Complement();` | sí |
| `NegatePattern` | `NEG` (y alias) | `Negate();` | sí |
| `SetCarryPattern` | `SCF` | `SetCarry();` | sí |
| `ComplementCarryPattern` | `CCF` | `ComplementCarry();` | sí |
| `HaltPattern` | `HALT` | `Halt();` | no |
| `DisableInterruptsPattern` | `DI` | `Registers.IFF1 = Registers.IFF2 = false;` | no |
| `EnableInterruptsPattern` | `EI` | `EnableInterrupts();` | no |
| `InterruptModePattern` | `IM n` (y alias) | `Registers.IM = 2;` | no |

### 4.4 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | 230 | 7 (`27 2F 37 3F 76 F3 FB`) |
| ED | 36 | 16 (8 `NEG` + 8 `IM`) |
| DD/FD | 230 | 7 (por la regla de ausentes) |
| CB, DDFDCB | 0 | 0 |

### 4.5 Rendimiento

- El cambio relevante es `Q`: una escritura más en **todas** las instrucciones. Repetir `ExecuteFrame`, `ExecuteLoopFrame` y `ExecuteAluLoopFrame` antes y después de añadir `Q` y anotar la diferencia en spec CPU 8.4. Si la regresión supera el ruido de la medición, documentarla y valorar (sin perder exactitud) mover la escritura de `Q = F` dentro de los auxiliares de flags para que el JIT la combine con la escritura de F.
- `DAA`: medir fórmula frente a tabla solo si aparece en los benchmarks; por defecto, fórmula.

## 5. Pruebas

### 5.1 FUSE

27 casos:

| Bloque | Casos |
|---|---|
| Base (11) | `27 27_1 2f 37 37_1 37_2 37_3 3f 76 f3 fb` |
| ED (16) | `ed44 ed4c ed54 ed5c ed64 ed6c ed74 ed7c`, `ed46 ed4e ed56 ed5e ed66 ed6e ed76 ed7e` |

**Convención de FUSE para `Q`:** FUSE calcula F3/F5 de `SCF`/`CCF` solo a partir de A (casos `37_1`: `A = 00`, `F = FF` → `F = C5`; `3f`: `A = 00`, `F = 5B` → `F = 50`), lo que equivale a la fórmula de Zilog con `Q = F` al empezar (la instrucción "anterior" escribió los flags). Por eso `FuseCpuState.Load` (`ZXSinclair.Net.Fuse/FuseCpuState.cs`) inicializa `Q = F`. FUSE no compara `Q`.

El runner debe pasar de 429 a **456** pasados, 0 fallos. Verificar con `--filter 27`, `2f`, `3`, `76`, `f`, `ed4`, `ed5`, `ed6`, `ed7`; ante un fallo, `--filter <caso> --verbose`.

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs`, con `TestBus` y el estado inicial de `NopTests`.

| Test | Montaje | Comprobación |
|---|---|---|
| `Daa_AllInputs_MatchReference` | A de 0 a 255 × C, H, N (2048 casos), modelo de referencia independiente en el test | A y F exactos |
| `Daa_KnownCases` (`[Theory]`) | `1F/00 → 25/30`, `9A/02 → 34/23` (FUSE `27_1`), `99` + C, resta con H | A, F |
| `Cpl_InvertsAndSetsHN` | `A = 0x89` | `A = 0x76`; `F = 0x32` desde `F = 0` (FUSE `2f`); S, Z, P/V, C conservados |
| `Neg_AllAliases` (`[Theory]`, 8) | `ED 44`…`ED 7C` con `A = FE` | `A = 02`, `F = 13`; 8 T |
| `Neg_EdgeValues` (`[Theory]`) | `A` = `0x00`, `0x80`, `0x01` | C = 0 / P/V = 1 / H y C |
| `Scf_Ccf_UseQ` (`[Theory]`) | `Q` = F (instrucción anterior con flags) frente a `Q = 0`, con combinaciones de A y F | F3/F5 = `((Q^F)\|A) & 0x28`; C, H, N, S, Z, P/V |
| `Scf_AfterAluVsAfterLoad` | Programa `CP 0x28` con A=0 + `SCF` frente a `LD B,B` + `SCF`, con F3/F5 de F a 1 | F3/F5 según Q (A solo / F \| A) |
| `Q_SetByFlagWritersClearedByOthers` (`[Theory]`) | Un opcode de cada patrón de los grupos 1–5 | `Q == F` tras los que escriben flags y `Q == 0` tras el resto |
| `Q_ClearedByInterruptAcceptanceAndHalt` | `ADD` y después INT (IM 1), NMI y M1 de HALT | `Q = 0` |
| `Halt_StaysOnOpcode` | `76` en `0x8000` | `PC = 0x8000`, `Halted`; 4 T; los `Step()` siguientes repiten M1 en `0x8000` con R++ |
| `Halt_ExitsOnInterruptWithNextAddress` | `HALT` + INT con IM 1 | Apila `0x8001`; `Halted = false` |
| `Halt_WithIndexPrefix` | `DD 76` | `Halted`; `PC` en la dirección del `76` |
| `Di_ClearsBothFlipFlops` | `F3` con IFF a 1 | IFF1 = IFF2 = 0; INT no se acepta después |
| `Ei_DelaysInterruptOneInstruction` | `EI`, `NOP`, INT activa con IM 1 | Tras `EI` no se acepta; tras `NOP` sí |
| `Ei_RepeatedKeepsBlocking` | `EI`, `EI`, `EI` con INT activa | Ninguna aceptación hasta la instrucción siguiente al último `EI` |
| `Im_AllAliases` (`[Theory]`, 8) | `ED 46 4E 56 5E 66 6E 76 7E` | IM 0/0/1/2/0/0/1/2; 8 T |
| `Control_DoesNotCountAsUnimplemented` | Un opcode de cada patrón | `UnimplementedOpcodes == 0` |

### 5.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| Tests de los grupos 1–4 que comparan el `Z80Registers` completo (`Load8Tests` con `LD A,I`/`LD A,R`, `Alu8Tests`) | El estado esperado incluye `Q = F` tras instrucciones que escriben flags; `Q = 0` tras el resto (el estado inicial de `NopTests` ya tiene `Q = 0`) |
| `GeneratorTests` | Las comprobaciones sobre el texto de los `case` (por ejemplo, `case 0x00: break; // NOP`) incluyen la escritura de `Q`; las de `EmitBody` no cambian. Cifras de 4.4: base y DD/FD `230`; `230 implemented / 22 pending / 4 prefixes`; ED `36 implemented / 42 pending`. Nuevo test: `WritesFlags` correcto en todos los patrones |
| `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset` | Usa `ED 44` como opcode no implementado: sustituirlo por un hueco de ED de grupos posteriores (por ejemplo, `ED 00`, grupo 11) |
| `FuseTestBusTests` / `FuseReportTests` que construyen registros a partir de FUSE | Revisar que la carga `Q = F` no cambie sus expectativas |
| `NopTests` | `NOP` emite `Registers.Q = 0;`; el estado inicial tiene `Q = 0`, así que no cambian |

## 6. Criterios de aceptación

- `Q` implementado según 2.2 y 4.1 (generador, interrupciones y HALT) y `FuseCpuState.Load` con `Q = F`.
- Auxiliares de 4.2 y patrones de 4.3; `*.g.cs` regenerados (no editados a mano) y `--check` a 0.
- Cobertura del generador igual a 4.4.
- Los 27 casos FUSE de 5.1 pasan con eventos; runner con 456 pasados y 0 fallos.
- Tests de 5.2 en verde y los de 5.3 adaptados; `dotnet test` de los dos proyectos en verde.
- Benchmarks de spec CPU 8.4 repetidos con el coste de `Q` anotado.
- Spec CPU 3 y 10 (Q resuelto), tabla de seguimiento de `spec-proceso-instrucciones.md`, spec CPU 8.1 y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- IM 0 con instrucciones distintas de `RST` y huecos de ED (grupo 11).
- Peculiaridad NMOS de P/V tras `LD A,I`/`LD A,R` con INT (spec CPU 6.1 / 10).
- `Q` en `POP AF`/`EX AF,AF'` más allá de la decisión provisional de 2.2.

## 8. Pendiente de verificar

- `Q` tras `POP AF` y `EX AF,AF'` con `z80ccf`.
- `IM` de `ED 4E`/`ED 6E` en hardware real ("IM 0/1").
- `PC` tras `DD 76`.

## 9. Resultado de implementación (2026-10-02)

- Cobertura del generador: 230 base, 36 ED y 230 DD/FD; `--check` a 0. Q se escribe al final del cuerpo generado; se conserva el atajo de NOP.
- Core: 1300 pruebas, incluidas 230 nuevas. DAA recorre A×F completos (65536 casos) y SCF/CCF recorre A×F con Q=0, Q=F y Q=AA. La matriz incluye todos los alias, DD/FD, R y PC cruzando FFFF, registros completos y ciclos M1. Generador: 101 pruebas.
- FUSE con eventos: 456 pasados / 0 fallos / 879 omitidos. Build Debug: 0 errores, 0 warnings.
- Benchmark ALU con Q: 0.6905 ns/T-state, 0 B, 414× tiempo real; intervalos solapados con el control sin Q (spec CPU 8.4).
- `PC` tras DD/FD HALT queda cubierto por pruebas propias; FUSE solo tiene el caso base. La confirmación externa de las variantes indicadas en §8 sigue pendiente.
