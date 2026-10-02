# Especificación: grupo 1, carga de 8 bits

Grupo 1 de `Specs/spec-proceso-instrucciones.md`: el *8-Bit Load Group* del manual de Zilog, con sus variantes `DD`/`FD` documentadas y no documentadas. Se implementa con patrones del generador (`Specs/spec-generador-z80.md`) y métodos auxiliares escritos a mano en el Core. Sigue la estructura de `Specs/spec-instr-nop.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulo *8-Bit Load Group*: `LD r,r'` … `LD R,A`.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus de cada instrucción.
- Sean Young, *The Undocumented Z80 Documented*: `IXH`/`IXL`, flags de `LD A,I`/`LD A,R`, MEMPTR (WZ).
- FUSE: 163 casos (sección 5.1) y entradas de `opcodes_base.dat`, `opcodes_ed.dat` y `opcodes_ddfd.dat`. Los eventos `MC` de los casos `dd36`, `dd46`, `dd70`, `ed4f` y `ed57` confirman las direcciones de los ciclos internos de la tabla 2.

## 2. Semántica

Notación de ciclos de spec CPU 4.4. `r`, `r'` ∈ {A, B, C, D, E, H, L}; `ii` = IX o IY; `ii+d` con `d` con signo y vuelta a 64K. Los T-states incluyen los M1 de los prefijos.

| Instrucción | Opcodes | Ciclos de bus | T | Efecto | WZ | Flags |
|---|---|---|---|---|---|---|
| `LD r,r'` | `40`–`7F` sin `76` ni `(HL)` (49) | `pc:4` | 4 | `r = r'` | — | — |
| `LD r,n` | `06 0E 16 1E 26 2E 3E` | `pc:4, pc+1:3` | 7 | `r = n` | — | — |
| `LD r,(HL)` | `46 4E 56 5E 66 6E 7E` | `pc:4, hl:3` | 7 | `r = (HL)` | — | — |
| `LD (HL),r` | `70`–`75`, `77` | `pc:4, hl:3` | 7 | `(HL) = r` | — | — |
| `LD (HL),n` | `36` | `pc:4, pc+1:3, hl:3` | 10 | `(HL) = n` | — | — |
| `LD A,(BC)` / `LD A,(DE)` | `0A` / `1A` | `pc:4, ss:3` | 7 | `A = (ss)` | `ss + 1` | — |
| `LD (BC),A` / `LD (DE),A` | `02` / `12` | `pc:4, ss:3` | 7 | `(ss) = A` | `(A << 8) \| ((ss + 1) & 0xFF)` | — |
| `LD A,(nn)` | `3A` | `pc:4, pc+1:3, pc+2:3, nn:3` | 13 | `A = (nn)` | `nn + 1` | — |
| `LD (nn),A` | `32` | `pc:4, pc+1:3, pc+2:3, nn:3` | 13 | `(nn) = A` | `(A << 8) \| ((nn + 1) & 0xFF)` | — |
| `LD A,I` / `LD A,R` | `ED 57` / `ED 5F` | `pc:4, pc+1:4, ir:1` | 9 | `A = I` / `A = R` | — | Ver 2.1 |
| `LD I,A` / `LD R,A` | `ED 47` / `ED 4F` | `pc:4, pc+1:4, ir:1` | 9 | `I = A` / `R = A` (los 8 bits) | — | — |
| `LD r,(ii+d)` | `DD`/`FD` `46 4E 56 5E 66 6E 7E` | `pc:4, pc+1:4, pc+2:3, pc+2:1 ×5, ii+d:3` | 19 | `r = (ii+d)` | `ii + d` | — |
| `LD (ii+d),r` | `DD`/`FD` `70`–`75`, `77` | `pc:4, pc+1:4, pc+2:3, pc+2:1 ×5, ii+d:3` | 19 | `(ii+d) = r` | `ii + d` | — |
| `LD (ii+d),n` | `DD`/`FD` `36` | `pc:4, pc+1:4, pc+2:3, pc+3:3, pc+3:1 ×2, ii+d:3` | 19 | `(ii+d) = n` | `ii + d` | — |

Reglas comunes:
- `PC` avanza por cada byte leído (opcode, prefijo, `d`, `n`, `nn`) y da la vuelta a 64K.
- `R` solo cambia por los M1 (spec CPU 4.2); `LD R,A` la sobrescribe entera **después** de los dos M1, y el ciclo `ir:1` usa el valor de `IR` anterior a la escritura (FUSE `ed4f`).
- `ir:1` es `bus.Internal(Registers.IR, 1)`: dirección `I << 8 | R` con R ya incrementada por los dos M1 (FUSE `ed57`).
- Los ciclos internos de `(ii+d)` usan la dirección del byte `d` (`pc+2`), y los de `LD (ii+d),n` la del byte `n` (`pc+3`), según los eventos de FUSE.
- Ninguna instrucción del grupo modifica F salvo `LD A,I` / `LD A,R`.
- `Q` (spec CPU 3): 0 en todas salvo `LD A,I` / `LD A,R`, donde vale el F resultante. Se implementa junto con `SCF`/`CCF` en el grupo 5 **(verificar si los tests lo exigen antes)**; este grupo no escribe `Q`.

### 2.1 Flags de `LD A,I` y `LD A,R`

| Flag | Valor |
|---|---|
| S, Z, F5, F3 | Del valor cargado en A (`Z80Flags.SZ53[A]`) |
| H, N | 0 |
| P/V | `IFF2` |
| C | Se conserva |

`F = (F & C) | SZ53[A] | (IFF2 ? PV : 0)`.

Peculiaridad NMOS: si se acepta una INT justo después, P/V queda a 0. Es un pendiente de spec CPU 6.1 / 10 y no forma parte de este grupo.

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `DD`/`FD` + opcode del grupo sin `H`, `L` ni `(HL)` (`40`–`43`, `47`–`4B`, `4F`, `50`–`53`, `57`–`5B`, `5F`, `78`–`7B`, `7F`, `06`, `0E`, `16`, `1E`, `3E`, `02`, `0A`, `12`, `1A`, `32`, `3A`) | Como sin prefijo (regla 4.1 de `spec-instr-nop.md`; el generador reutiliza el cuerpo de la tabla base) | +4 |
| `DD`/`FD` `44 45 4C 4D 54 55 5C 5D 7C 7D` | **No documentada**: `LD r,IXH` / `LD r,IXL` (o IY) | 8 |
| `DD`/`FD` `60`–`65`, `67`, `68`–`6D`, `6F` | **No documentada**: `LD IXH,r` / `LD IXL,r`, incluidos `LD IXH,IXL`, `LD IXH,IXH`… | 8 |
| `DD`/`FD` `26` / `2E` | **No documentada**: `LD IXH,n` / `LD IXL,n` (`pc:4, pc+1:4, pc+2:3`) | 11 |
| `DD`/`FD` `66`, `6E`, `74`, `75` | `LD H,(ii+d)`, `LD L,(ii+d)`, `LD (ii+d),H`, `LD (ii+d),L` usan **H y L reales**, no las mitades del índice | 19 |
| `DDCB`/`FDCB` `LD r,RLC (ii+d)`… | Copias no documentadas: pertenecen a los grupos 7 y 8 | — |
| `ED` sin entrada | Huecos de ED: grupo 11 | — |

## 4. Implementación

### 4.1 Auxiliares escritos a mano (Core)

En `ZXSinclair.Net.Core/Z80/Z80Cpu.cs`, junto a `ReadPc()` (los usarán también otros grupos), todos `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:

| Método | Comportamiento |
|---|---|
| `ushort ReadPc16()` | `pc:3` byte bajo y `pc:3` byte alto, con `PC` avanzando y dando la vuelta |
| `ushort IndexedAddress<TIndex>()` | Lee `d` (`pc:3`), emite `bus.Internal(dirección de d, 5)`, calcula `ii + d` con vuelta a 64K, lo guarda en `WZ` y lo devuelve. Es la traducción de `(REGISTER+dd)` en `OperandEmitter` |

En un `partial` nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Load8.cs`:

| Método | Comportamiento |
|---|---|
| `void StoreIndexedImmediate<TIndex>()` | `LD (ii+d),n`: lee `d` y `n`, `bus.Internal(dirección de n, 2)`, `WZ = ii + d`, escribe `n` |
| `void LoadAFromSpecial(byte value)` | `LD A,I` / `LD A,R`: `bus.Internal(Registers.IR, 1)`, `A = value` y flags de 2.1. El valor se lee **antes** del ciclo interno (el ciclo no cambia I ni R) |

### 4.2 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Load8.cs`, añadidos a `PatternCatalog.Default`. Los cuerpos ordenan los accesos al bus con variables locales cuando hay más de uno, para que el orden no dependa de la evaluación de argumentos.

| Patrón | Reconoce | Cuerpo emitido (ejemplo) |
|---|---|---|
| `LoadRegisterRegister` | `LD` con destino y origen de clase `Register8`, `IndexHigh` o `IndexLow` | `Registers.B = Registers.C;` — vacío si destino y origen son el mismo operando (`LD B,B`, `LD IXH,IXH`) |
| `LoadRegisterImmediate` | `LD r,nn` (incluidos `REGISTERH`/`REGISTERL`) | `Registers.B = ReadPc();` |
| `LoadRegisterIndirect` | `LD r,(HL)`, `LD A,(BC)`, `LD A,(DE)` | `Registers.B = bus.Read(Registers.HL);`; con BC/DE añade `Registers.WZ = (ushort)(Registers.BC + 1);` |
| `StoreIndirectRegister` | `LD (HL),r`, `LD (BC),A`, `LD (DE),A` | `bus.Write(Registers.HL, Registers.B);`; con BC/DE añade el WZ de la tabla 2 |
| `StoreIndirectImmediate` | `LD (HL),nn` | `var value = ReadPc(); bus.Write(Registers.HL, value);` |
| `LoadAccumulatorAbsolute` | `LD A,(nnnn)` | `var address = ReadPc16(); Registers.A = bus.Read(address); Registers.WZ = (ushort)(address + 1);` |
| `StoreAbsoluteAccumulator` | `LD (nnnn),A` | `var address = ReadPc16(); bus.Write(address, Registers.A); Registers.WZ = …;` |
| `LoadRegisterIndexed` | `LD r,(REGISTER+dd)` | `Registers.B = bus.Read(IndexedAddress<TIndex>());` |
| `StoreIndexedRegister` | `LD (REGISTER+dd),r` | `var address = IndexedAddress<TIndex>(); bus.Write(address, Registers.B);` |
| `StoreIndexedImmediate` | `LD (REGISTER+dd),nn` | `StoreIndexedImmediate<TIndex>();` |
| `LoadAccumulatorSpecial` | `LD A,I`, `LD A,R` | `LoadAFromSpecial(Registers.I);` |
| `StoreSpecialAccumulator` | `LD I,A`, `LD R,A` | `bus.Internal(Registers.IR, 1); Registers.I = Registers.A;` |

- Ningún patrón reconoce cargas de 16 bits (`LD BC,nnnn`, `LD (nnnn),HL`, `LD SP,HL`…), que son del grupo 2; `Patterns_AreDisjoint` lo vigila.
- Los opcodes `DD`/`FD` ausentes de `opcodes_ddfd.dat` toman el cuerpo de base por la regla del generador; no hace falta patrón específico.
- La traducción de operandos es la de `OperandEmitter` (spec del generador 2.4); `H`/`L` reales y `REGISTERH`/`REGISTERL` ya se distinguen por su clase.

### 4.3 Cobertura esperada del generador

| Tabla | Implementados (incluido NOP) |
|---|---|
| base | 78 (77 del grupo + `NOP`) |
| ED | 4 |
| DD/FD | 78 (41 entradas propias + 36 por la regla de ausentes + `NOP`) |
| CB, DDFDCB | 0 |

### 4.4 Rendimiento

- Sin asignaciones, sin `switch` en tiempo de ejecución sobre el registro: cada cuerpo lleva los registros concretos. En la tabla base, el generador emite cuerpos auxiliares con `AggressiveInlining` y casos con retorno directo para mantener pequeño el despacho; ver la medición en spec CPU 8.4.
- `IndexedAddress<TIndex>` y `StoreIndexedImmediate<TIndex>` se especializan por IX/IY sin coste.
- Según la guía del proceso, este grupo no exige benchmark; se mide tras el grupo 3 con cargas reales. Basta con comprobar que `*Z80Cpu*` (NOP) no empeora por el crecimiento de los `switch`.

## 5. Pruebas

### 5.1 FUSE

Deben pasar con comparación de eventos los 163 casos del grupo:

| Bloque | Casos |
|---|---|
| Base (77) | `02 06 0a 0e 12 16 1a 1e 26 2e 32 36 3a 3e`, `40`–`7f` salvo `76` |
| ED (4) | `ed47 ed4f ed57 ed5f` |
| DD (41) | `dd26 dd2e dd36`, `dd44`–`dd46`, `dd4c`–`dd4e`, `dd54`–`dd56`, `dd5c`–`dd5e`, `dd60`–`dd75`, `dd77`, `dd7c`–`dd7e` |
| FD (41) | Los mismos con `fd` |

Con `NOP`, el runner debe pasar de 3 a 166 casos, 0 fallos. FUSE no comprueba WZ: lo cubren los tests propios.

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs`, con `TestBus` y el mismo estado inicial de `NopTests` (todos los registros distintos de cero). Cada test compara el `Z80Registers` completo con el esperado.

| Test | Montaje | Comprobación |
|---|---|---|
| `LdRegReg_AllCombinations` (`[Theory]`, 49 casos) | `40`–`7F` sin `(HL)` ni `76` | Solo cambia el destino; 4 T; un único `M1` |
| `LdRegImm_AllRegisters` (`[Theory]`, 7) | `06`…`3E` con `n = 0xA5` | Destino `0xA5`; accesos `M1`, `Read pc+1`; 7 T; `PC += 2` |
| `LdRegHl_ReadsMemory` (`[Theory]`, 7) | `46`…`7E`, `HL = 0x8000` | Destino = `(HL)`; WZ sin cambios; 7 T |
| `LdHlReg_WritesMemory` (`[Theory]`, 7) | `70`…`77` | `(HL) = r`; acceso `Write` en HL; 7 T |
| `LdHlN_ReadsThenWrites` | `36 5A` | Orden `M1`, `Read pc+1`, `Write HL`; 10 T |
| `LdAFromPair_SetsWz` (`[Theory]`) | `0A` (BC), `1A` (DE), incluido `0xFFFF` | `A = (ss)`; `WZ = ss + 1` con vuelta |
| `LdPairFromA_SetsWz` (`[Theory]`) | `02`, `12`, `ss = 0x12FF` | `(ss) = A`; `WZ = (A << 8) | 0x00` |
| `LdAAbsolute_ReadsAndSetsWz` | `3A 34 12` | `A = (0x1234)`; `WZ = 0x1235`; 13 T; orden de accesos |
| `LdAbsoluteA_WritesAndSetsWz` | `32 FF 12` | `(0x12FF) = A`; `WZ = (A << 8) | 0x00`; 13 T |
| `LdAbsolute_OperandWrapsPc` | `3A` en `0xFFFE` | Lee `nn` en `0xFFFF` y `0x0000`; `PC = 0x0001` |
| `LdAI_SetsFlagsFromIff2` (`[Theory]`) | `ED 57` con `IFF2` = 0/1, `I` = `0x00`, `0x80`, `0x28` | Flags de 2.1 (S, Z, F5, F3, H = N = 0, P/V = IFF2, C conservado); 9 T; `Internal` en IR |
| `LdAR_ReadsRefreshAfterFetches` | `ED 5F` con `R = 0x7E` y `R = 0xFF` | `A = 0x00`/`0x81` con el bit 7 correcto (R tras dos M1) y flags de 2.1 |
| `LdIA_SetsI` | `ED 47` | `I = A`; flags sin cambios; `Internal` con IR previo; 9 T |
| `LdRA_SetsAllBits` | `ED 4F` con `A = 0x80` | `R = 0x80` tras la instrucción; `Internal` con el IR anterior a la escritura |
| `LdRegIndexed_ReadsWithDisplacement` (`[Theory]`) | `DD 46 d` y `FD 46 d` con `d` = `+5`, `-1`, `-128`, `+127` | `B = (ii+d)`; `WZ = ii+d`; `Internal(pc+2, 5)`; 19 T |
| `LdRegIndexed_WrapsAddress` | `IX = 0x0005`, `d = -10` | Lee `0xFFFB` |
| `LdIndexedReg_WritesWithDisplacement` (`[Theory]`) | `DD 70 d`, `FD 77 d` | `(ii+d) = r`; WZ; 19 T |
| `LdIndexedH_UsesRealHAndL` (`[Theory]`) | `DD 66`, `DD 6E`, `DD 74`, `DD 75` | Usa/cambia H y L reales; IXH/IXL intactos |
| `LdIndexedN_InternalCyclesOnOperand` | `DD 36 d n` | Orden `M1`, `M1`, `Read d`, `Read n`, `Internal(pc+3, 2)`, `Write`; WZ; 19 T |
| `LdIndexHalves_Undocumented` (`[Theory]`) | `DD 44`, `DD 65`, `FD 6C`, `FD 7D`, `DD 26 n`, `FD 2E n` | Leen/escriben IXH/IXL/IYH/IYL; 8 T / 11 T |
| `LdIndexHalves_SameRegisterIsNoOp` (`[Theory]`) | `DD 64`, `FD 6D` | Ningún registro cambia salvo PC y R; 8 T |
| `IndexPrefix_UnaffectedLoadsRunAsBase` (`[Theory]`) | `DD 41`, `FD 06 n`, `DD 0A`, `FD 32 nn` | Mismo efecto que sin prefijo; +4 T; IX/IY intactos |
| `Load8_DoesNotTouchFlagsOrCounter` (`[Theory]`) | Un opcode de cada patrón salvo `LD A,I/R` | `F` sin cambios; `UnimplementedOpcodes == 0` |
| `Load8_AllowsIntOnNextStep` | `LD A,(nn)` con INT activa desde T 1, IM 1 | El siguiente `Step()` acepta la INT |

### 5.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| `ZXSinclair.Net.Generate.Z80OpCodes.Tests` `Output_PilotContainsOnlyNop` | Pasa a comprobar la cobertura de 4.3 (o se sustituye por `Output_MatchesExpectedCoverage`). |
| `ZXSinclair.Net.Generate.Z80OpCodes.Tests` `Coverage_IncludesHolesAliasesAndPendingDetails` | Cifras nuevas de la tabla base (78 implementados / 174 pendientes) y de ED. |
| `Z80CpuTests` | No hace falta cambiar nada: usa `01` (`LD BC,nn`, grupo 2) como opcode no implementado. Los bytes `0x78`/`0x56` del test de IM 2 son datos del vector, no se ejecutan. |

## 6. Criterios de aceptación

- Patrones de 4.2 en `Patterns/Load8.cs` y auxiliares de 4.1; ficheros `*.g.cs` regenerados (no editados a mano) y `--check` a 0.
- Cobertura del generador igual a 4.3.
- Los 163 casos FUSE de 5.1 pasan con eventos; runner con 166 pasados y 0 fallos.
- Tests de 5.2 en verde y los de 5.3 adaptados; `dotnet test` de los dos proyectos en verde.
- Benchmark `*Z80Cpu*` sin regresión ni asignaciones.
- Tabla de seguimiento de `spec-proceso-instrucciones.md` actualizada (grupo 1 y casos FUSE), spec CPU 8.1 con las cifras del runner, y `CLAUDE.md`/`AGENTS.md` si describen el estado de las instrucciones.

## 7. Fuera de alcance

- Cargas de 16 bits, `PUSH`/`POP` y `EX`: grupos 2 y 9.
- Copias no documentadas `DDCB`/`FDCB` (`LD r,RLC (ii+d)`…): grupos 7 y 8.
- `Q` y la peculiaridad NMOS de P/V tras `LD A,I`/`LD A,R` con INT: grupo 5 y spec CPU 10.

## 8. Pendiente de verificar

- Si algún caso FUSE de este grupo depende de `Q` (no se espera: FUSE no compara `Q`).
- Comportamiento de WZ con z80test (`z80memptr`), cuando se integre (`Docs/benchmarks-maquinas-reales.md`).
