# Especificación: grupo 6, aritmética de 16 bits

Grupo 6 de `Specs/spec-proceso-instrucciones.md`: el *16-Bit Arithmetic Group* del manual de Zilog (`ADD HL,ss`, `ADC HL,ss`, `SBC HL,ss`, `ADD IX,pp`, `ADD IY,rr`, `INC ss`, `DEC ss` e `INC`/`DEC` de IX/IY). Se implementa con patrones del generador y auxiliares sin ramas en el Core. Sigue la estructura de `Specs/spec-instr-alu-8.md` y usa el mecanismo de `Q` de `Specs/spec-instr-control.md` (`IPattern.WritesFlags`).

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulo *16-Bit Arithmetic Group*.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus (`ir:1 ×7`, `ir:1 ×2`).
- Sean Young, *The Undocumented Z80 Documented*: F3/F5 del byte alto, MEMPTR (`WZ = HL + 1`).
- FUSE: 32 casos (sección 5.1) y entradas de `opcodes_base.dat`, `opcodes_ed.dat` y `opcodes_ddfd.dat`. Los casos `09` (`HL = 9ABC + 5678 = F134`, `F = 30`) y `ed4a` (`HL = 9AC8 + 24B5 + 1 = BF7E`, `F = A8`) confirman las fórmulas de 2.2.

## 2. Semántica

Notación de ciclos de spec CPU 4.4. `ss` ∈ {BC, DE, HL, SP}; `pp` ∈ {BC, DE, IX, SP} (y `rr` con IY). Los T-states incluyen los M1 de los prefijos.

### 2.1 Operaciones y temporización

| Instrucción | Opcodes | Ciclos de bus | T | WZ | Flags |
|---|---|---|---|---|---|
| `ADD HL,ss` | `09 19 29 39` | `pc:4, ir:1 ×7` | 11 | `HL + 1` (HL antes) | 2.2 |
| `ADD ii,pp` | `DD`/`FD` `09 19 29 39` | `pc:4, pc+1:4, ir:1 ×7` | 15 | `ii + 1` (antes) | 2.2 |
| `ADC HL,ss` | `ED 4A 5A 6A 7A` | `pc:4, pc+1:4, ir:1 ×7` | 15 | `HL + 1` | 2.2 |
| `SBC HL,ss` | `ED 42 52 62 72` | `pc:4, pc+1:4, ir:1 ×7` | 15 | `HL + 1` | 2.2 |
| `INC ss` / `DEC ss` | `03 13 23 33` / `0B 1B 2B 3B` | `pc:4, ir:1 ×2` | 6 | — | Ninguno |
| `INC ii` / `DEC ii` | `DD`/`FD` `23` / `2B` | `pc:4, pc+1:4, ir:1 ×2` | 10 | — | Ninguno |

- Los ciclos internos usan `IR` con R ya incrementada por los M1, como en los grupos 2 y 3. En los casos FUSE (`09`, `ed4a`, `03`, `dd09`) `I = 0` y `IR` coincide con `PC`, así que FUSE no los distingue **(verificar, como en el grupo 2)**; los tests propios lo fijan con `I ≠ 0`.
- El resultado y SP/IX/IY dan la vuelta a 16 bits (`INC SP` con `0xFFFF` → `0x0000`).
- `ADD ii,ii` (`DD 29`) suma el índice consigo mismo.

### 2.2 Flags

`a` = primer operando (HL, IX o IY), `v` = segundo, `c` = flag C anterior, `r` = resultado de 17 bits (con signo en la resta).

| Operación | S | Z | F5 F3 | H | P/V | N | C |
|---|---|---|---|---|---|---|---|
| `ADD` | se conserva | se conserva | bits 13 y 11 de `r` (byte alto) | acarreo del bit 11 | se conserva | 0 | acarreo del bit 15 |
| `ADC` | bit 15 de `r` | `r & 0xFFFF == 0` | byte alto de `r` | acarreo del bit 11 | desbordamiento | 0 | acarreo del bit 15 |
| `SBC` | bit 15 de `r` | `r & 0xFFFF == 0` | byte alto de `r` | préstamo del bit 11 | desbordamiento | 1 | préstamo |
| `INC`/`DEC` de 16 bits | — | — | — | — | — | — | — (F intacto) |

Fórmulas sin ramas (con `int`):
- `ADD`: `F = (F & (S | Z | PV)) | ((r >> 8) & (F5 | F3)) | (((a ^ v ^ r) >> 8) & H) | ((r >> 16) & C)`.
- `ADC`: `F = ((r >> 8) & (S | F5 | F3)) | ((r & 0xFFFF) == 0 ? Z : 0) | (((a ^ v ^ r) >> 8) & H) | (((a ^ ~v) & (a ^ r) & 0x8000) >> 13) | ((r >> 16) & C)`.
- `SBC`: `F = ((r >> 8) & (S | F5 | F3)) | ((r & 0xFFFF) == 0 ? Z : 0) | (((a ^ v ^ r) >> 8) & H) | (((a ^ v) & (a ^ r) & 0x8000) >> 13) | N | ((r >> 16) & C)`.

El cálculo de Z es la única comparación; el JIT la compila sin salto (`sete`/`cmov`) **(verificar en el benchmark; si no, usar una expresión aritmética equivalente)**.

### 2.3 `Q`

- `ADD`, `ADC` y `SBC` de 16 bits escriben flags: `Q = F` (sus patrones devuelven `WritesFlags = true`).
- `INC`/`DEC` de 16 bits no escriben flags: `Q = 0`.

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `DD`/`FD` `09 19 29 39` | `ADD IX,BC/DE/IX/SP` (o IY); en la tabla `ADD REGISTER,…` | 15 |
| `DD`/`FD` `23` / `2B` | `INC IX` / `DEC IX` (o IY) | 10 |
| `DD`/`FD` `03 0B 13 1B 33 3B` | El prefijo no afecta: `INC`/`DEC` de BC, DE, SP como sin prefijo (regla de ausentes) | 10 |
| `ED 4A`…`7A`, `ED 42`…`72` con `DD`/`FD` delante | El `ED` anula el prefijo índice (spec CPU 4.3): opera sobre HL | 19 |
| `INC`/`DEC` de 8 bits | Grupo 4 | — |

## 4. Implementación

### 4.1 Auxiliares escritos a mano (Core)

Nuevo `partial` `ZXSinclair.Net.Core/Z80/Z80Cpu.Alu16.cs`, `[MethodImpl(MethodImplOptions.AggressiveInlining)]`, con las fórmulas de 2.2:

| Método | Comportamiento |
|---|---|
| `ushort Add16(ushort a, ushort v)` | `bus.Internal(IR, 7)`; `WZ = a + 1`; devuelve `a + v` y actualiza F (S, Z, P/V conservados) |
| `void Adc16(ushort v)` | `bus.Internal(IR, 7)`; `WZ = HL + 1`; `HL = HL + v + c`; F completo |
| `void Sbc16(ushort v)` | `bus.Internal(IR, 7)`; `WZ = HL + 1`; `HL = HL - v - c`; F completo |

`INC`/`DEC` de 16 bits se emiten en línea (`bus.Internal(Registers.IR, 2); Registers.BC++;`).

### 4.2 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Alu16.cs`, añadidos a `PatternCatalog.Default`:

| Patrón | Reconoce | Cuerpo emitido (ejemplo) | `WritesFlags` |
|---|---|---|---|
| `Add16Pattern` | `ADD` con destino `HL` (`Register16`) o `REGISTER` (`IndexPair`) y origen `Register16`/`IndexPair` | `Registers.HL = Add16(Registers.HL, Registers.BC);` / `TIndex.Pair(ref Registers) = Add16(TIndex.Pair(ref Registers), Registers.SP);` | sí |
| `AdcSbc16Pattern` | `ADC HL,ss`, `SBC HL,ss` | `Adc16(Registers.DE);` / `Sbc16(Registers.HL);` | sí |
| `IncDec16Pattern` | `INC`/`DEC` con `Register16` o `IndexPair` | `bus.Internal(Registers.IR, 2); Registers.SP--;` / `bus.Internal(Registers.IR, 2); TIndex.Pair(ref Registers)++;` | no |

- `Alu8Pattern` exige `A` como primer operando cuando hay dos, e `IncDec8Register` solo registros de 8 bits: no hay solape; `Patterns_AreDisjoint` y un test explícito lo vigilan.
- El valor del segundo operando se evalúa antes de los ciclos internos (argumento del auxiliar); no hay accesos a memoria, así que el orden no afecta a los eventos.

### 4.3 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | 242 | 12 (`03 09 0B 13 19 1B 23 29 2B 33 39 3B`) |
| ED | 44 | 8 (`42 4A 52 5A 62 6A 72 7A`) |
| DD/FD | 242 | 12 (6 entradas propias + 6 por la regla de ausentes) |
| CB, DDFDCB | 0 | 0 |

### 4.4 Rendimiento

- Sin ramas salvo la comparación de Z (2.2); sin asignaciones.
- `INC`/`DEC` de 16 bits son frecuentes en bucles reales (`INC HL`, `DEC BC`): añadirlos al bucle de `ExecuteLoopFrame` no es necesario, pero sí repetir los tres benchmarks de spec CPU 8.4 para comprobar que el crecimiento del `switch` no empeora el despacho. Según la guía, el hito de medición con cargas reales es tras el grupo 7 o cuando arranque la ROM.

## 5. Pruebas

### 5.1 FUSE

32 casos:

| Bloque | Casos |
|---|---|
| Base (12) | `03 09 0b 13 19 1b 23 29 2b 33 39 3b` |
| ED (8) | `ed42 ed4a ed52 ed5a ed62 ed6a ed72 ed7a` |
| DD (6) | `dd09 dd19 dd23 dd29 dd2b dd39` |
| FD (6) | `fd09 fd19 fd23 fd29 fd2b fd39` |

El runner debe pasar de 456 a **488** pasados, 0 fallos. Verificar con `--filter 0`, `1`, `2`, `3`, `ed4`…`ed7`, `dd`, `fd`; ante un fallo, `--filter <caso> --verbose`. Convenciones de FUSE que afectan al grupo: ninguna nueva (FUSE no compara WZ ni `Q`, y no distingue IR de PC en los ciclos internos).

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs`, con `TestBus` y el estado inicial de `NopTests` (`I = 0x42`, `Q = 0`).

| Test | Montaje | Comprobación |
|---|---|---|
| `Add16_MatchesReference` | Modelo de referencia independiente en el test; muestreo amplio de pares (todos los valores de `a` y `v` con los 16 bits de borde: `0x0000`, `0x0FFF`, `0x1000`, `0x7FFF`, `0x8000`, `0xFFFF`… combinados con valores pseudoaleatorios deterministas, ≥ 100 000 casos) y F inicial variado | HL y F exactos; S, Z, P/V conservados |
| `Adc16_Sbc16_MatchReference` (`[Theory]`, 2) | Igual, con C = 0 y 1 | HL y F exactos |
| `Add16_KnownCases` (`[Theory]`) | `9ABC + 5678` (FUSE `09`), `0FFF + 1` (H), `FFFF + 1` (C), F5/F3 del byte alto | HL, F |
| `Adc16_ZeroAndOverflow` (`[Theory]`) | `7FFF + 0 + 1` (P/V, S), `FFFF + 0 + 1` (Z, C) | F |
| `Sbc16_ZeroBorrowOverflow` (`[Theory]`) | `HL - HL - 0` (Z), `0 - 1` (C, S), `8000 - 1` (P/V), préstamo del bit 11 | F |
| `Add16_AllPairs` (`[Theory]`, 4) | `09 19 29 39` | Origen correcto (incluido `ADD HL,HL`); `WZ = HL + 1`; `Internal(IR, 7)` con `I = 0x42`; 11 T |
| `AdcSbc16_AllPairs` (`[Theory]`, 8) | `ED 42`…`7A` | Origen correcto; WZ; 15 T |
| `AddIndex_AllPairs` (`[Theory]`, 8) | `DD`/`FD` `09 19 29 39` | IX/IY; `ADD IX,IX`; HL intacto; `WZ = ii + 1`; 15 T |
| `IncDec16_AllPairs` (`[Theory]`, 8) | `03`…`3B` | Solo cambia el par; F y WZ intactos; `Internal(IR, 2)`; 6 T |
| `IncDec16_Wraps` (`[Theory]`) | `INC SP` con `FFFF`, `DEC BC` con `0000`, `INC IX` con `FFFF` | Vuelta a 16 bits |
| `IncDecIndex` (`[Theory]`, 4) | `DD 23`, `DD 2B`, `FD 23`, `FD 2B` | IX/IY; 10 T |
| `IndexPrefix_UnaffectedRunAsBase` (`[Theory]`) | `DD 03`, `FD 3B` | Efecto de base; +4 T; IX/IY intactos |
| `Alu16_SetsQ` (`[Theory]`) | `ADD HL,BC`, `ADC HL,DE`, `SBC HL,SP`, `INC BC` | `Q = F` tras los tres primeros; `Q = 0` tras `INC BC` |
| `Alu16_DoesNotCountAsUnimplemented` | Un opcode de cada patrón | `UnimplementedOpcodes == 0` |

### 5.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| `GeneratorTests` (cobertura) | Cifras de 4.3: base y DD/FD `242`; `242 implemented / 10 pending / 4 prefixes`; ED `44 implemented / 34 pending`. Añadir `Alu16Patterns_EmitExpectedBodies` y comprobar `WritesFlags` de los tres patrones |
| `Z80CpuTests`, `NopTests`, `FuseReportTests` | Sin cambios: usan `D3` y `ED 00` (grupos 10 y 11) como opcodes pendientes |

## 6. Criterios de aceptación

- Auxiliares de 4.1 y patrones de 4.2 con su `WritesFlags`; `*.g.cs` regenerados (no editados a mano) y `--check` a 0.
- Cobertura del generador igual a 4.3.
- Los 32 casos FUSE de 5.1 pasan con eventos; runner con 488 pasados y 0 fallos.
- Tests de 5.2 en verde y los de 5.3 adaptados; `dotnet test` de los dos proyectos en verde.
- Benchmarks de spec CPU 8.4 repetidos sin regresión ni asignaciones.
- Tabla de seguimiento de `spec-proceso-instrucciones.md`, spec CPU 8.1 y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- `INC`/`DEC` y ALU de 8 bits (grupo 4).
- `EX DE,HL` y demás intercambios (grupo 9).
- Rotaciones de 16 bits inexistentes en el Z80; `RLD`/`RRD` (grupo 7).

## 8. Verificación (2026-10-02)

- Build Debug de la solución: 0 errores y 0 warnings. Core: 1455 tests; generador: 108 tests. El generador tiene cobertura 242/44/242 y `--check` devuelve 0.
- FUSE con eventos: 488 pasados, 0 fallos, 847 omitidos; pasan los 32 casos del grupo.
- `Alu16Tests` comprueba 11 085 760 operaciones aritméticas contra un modelo independiente, todos los valores de INC/DEC, los pares y prefijos, IR con I=0x42, WZ, Q, R, PC al cruzar FFFF y aceptación posterior de INT/NMI. Los casos de la tabla 5.2 se agrupan por comportamiento en teorías comunes.
- Los tres benchmarks repetidos tienen 0 B asignados y sus intervalos se solapan con el control anterior repetido; la primera comparación y la investigación se documentan en spec CPU 8.4.
- JIT FullOpts x64, .NET 10.0.12, `Z80Cpu<SpectrumBus>.ExecuteED`: los auxiliares se inlinean. Las ocho variantes ADC/SBC usan `test` seguido de `cmovne` para Z, sin salto condicional. Sonda temporal ejecutada sin opcodes pendientes; desensamblado en `ZXSinclair.Net.Benchmarks/bin/alu16-jit/jit.asm`. No verificado en WebAssembly.

Pendiente: contrastar WZ con z80test (`z80memptr`) cuando se integre. Los tests propios verifican las reglas de WZ de esta spec; FUSE no incluye ese registro.
