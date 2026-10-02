# Especificación: grupo 2, carga de 16 bits

Grupo 2 de `Specs/spec-proceso-instrucciones.md`: el *16-Bit Load Group* del manual de Zilog (cargas de pares, `LD SP,HL`, `PUSH` y `POP`), con sus variantes `DD`/`FD` y los duplicados no documentados de la tabla ED. Se implementa con patrones del generador (`Specs/spec-generador-z80.md`) y auxiliares escritos a mano en el Core. Sigue la estructura de `Specs/spec-instr-carga-8.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulo *16-Bit Load Group*: `LD dd,nn` … `POP IY`.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus.
- Sean Young, *The Undocumented Z80 Documented*: `ED 63`/`ED 6B`, MEMPTR (WZ).
- FUSE: 35 casos (sección 5.1) y entradas de `opcodes_base.dat`, `opcodes_ed.dat` y `opcodes_ddfd.dat`. Los eventos de `c5`, `f1`, `f9`, `ddf9`, `2a` y `ed43` confirman el orden de los accesos de la tabla 2.

## 2. Semántica

Notación de ciclos de spec CPU 4.4. `dd` ∈ {BC, DE, HL, SP}; `qq` ∈ {BC, DE, HL, AF}; `ii` = IX o IY. Los accesos de 16 bits son siempre byte bajo primero y la dirección del byte alto da la vuelta a 64K (`0xFFFF` → `0x0000`). Los T-states incluyen los M1 de los prefijos.

| Instrucción | Opcodes | Ciclos de bus | T | Efecto | WZ |
|---|---|---|---|---|---|
| `LD dd,nn` | `01 11 21 31` | `pc:4, pc+1:3, pc+2:3` | 10 | `dd = nn` | — |
| `LD ii,nn` | `DD`/`FD` `21` | `pc:4, pc+1:4, pc+2:3, pc+3:3` | 14 | `ii = nn` | — |
| `LD HL,(nn)` | `2A` | `pc:4, pc+1:3, pc+2:3, nn:3, nn+1:3` | 16 | `L = (nn)`, `H = (nn+1)` | `nn + 1` |
| `LD dd,(nn)` | `ED 4B 5B 6B 7B` | `pc:4, pc+1:4, pc+2:3, pc+3:3, nn:3, nn+1:3` | 20 | Igual | `nn + 1` |
| `LD ii,(nn)` | `DD`/`FD` `2A` | `pc:4, pc+1:4, pc+2:3, pc+3:3, nn:3, nn+1:3` | 20 | Igual | `nn + 1` |
| `LD (nn),HL` | `22` | `pc:4, pc+1:3, pc+2:3, nn:3, nn+1:3` | 16 | `(nn) = L`, `(nn+1) = H` | `nn + 1` |
| `LD (nn),dd` | `ED 43 53 63 73` | `pc:4, pc+1:4, pc+2:3, pc+3:3, nn:3, nn+1:3` | 20 | Igual | `nn + 1` |
| `LD (nn),ii` | `DD`/`FD` `22` | `pc:4, pc+1:4, pc+2:3, pc+3:3, nn:3, nn+1:3` | 20 | Igual | `nn + 1` |
| `LD SP,HL` | `F9` | `pc:4, ir:1 ×2` | 6 | `SP = HL` | — |
| `LD SP,ii` | `DD`/`FD` `F9` | `pc:4, pc+1:4, ir:1 ×2` | 10 | `SP = ii` | — |
| `PUSH qq` | `C5 D5 E5 F5` | `pc:4, ir:1, sp-1:3, sp-2:3` | 11 | `(SP-1) = alto`, `(SP-2) = bajo`, `SP -= 2` | — |
| `PUSH ii` | `DD`/`FD` `E5` | `pc:4, pc+1:4, ir:1, sp-1:3, sp-2:3` | 15 | Igual | — |
| `POP qq` | `C1 D1 E1 F1` | `pc:4, sp:3, sp+1:3` | 10 | `bajo = (SP)`, `alto = (SP+1)`, `SP += 2` | — |
| `POP ii` | `DD`/`FD` `E1` | `pc:4, pc+1:4, sp:3, sp+1:3` | 14 | Igual | — |

Reglas comunes:
- Ninguna instrucción del grupo modifica F, **salvo `POP AF`**, que carga F entero (los 8 bits, incluidos F3 y F5) desde la pila.
- `PC` avanza por cada byte leído y da la vuelta a 64K; `SP` también da la vuelta (`PUSH` con `SP = 0x0000` escribe en `0xFFFF` y `0xFFFE`; `POP` con `SP = 0xFFFF` lee `0xFFFF` y `0x0000`).
- `PUSH`: primero el ciclo interno y después el byte **alto** en `SP-1` y el **bajo** en `SP-2` (FUSE `c5`). `POP`: primero el bajo (FUSE `f1`).
- `ir:1` es `bus.Internal(Registers.IR, n)` con R ya incrementada por los M1, como en el grupo 1. En los casos FUSE `c5`, `f9` y `ddf9`, `I = 0` y la dirección de `IR` coincide con `PC`, así que FUSE no distingue entre ambas; se sigue la tabla de la wiki (IR) y los tests propios lo fijan con `I ≠ 0` **(verificar con un caso de hardware si aparece)**.
- `PUSH`, `POP`, `LD SP,HL` y `LD dd,nn` no tocan WZ.
- `Q`: este grupo no lo escribe (grupo 5). Valor de `Q` tras `POP AF` **(verificar)**.

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `ED 63` / `ED 6B` | **No documentadas**: duplicados de `LD (nn),HL` / `LD HL,(nn)` con la temporización de ED | 20 |
| `DD`/`FD` `01 11 31 C1 C5 D1 D5 F1 F5` | El prefijo no afecta: como sin prefijo (regla de ausentes del generador) | +4 |
| `DD`/`FD` `21 22 2A E1 E5 F9` | Usan IX/IY en lugar de HL | Tabla 2 |
| `EX (SP),HL`, `EX (SP),ii`, `EX DE,HL` | Grupo 9 (intercambio) | — |
| `INC`/`DEC`/`ADD` de 16 bits | Grupo 6 | — |

## 4. Implementación

### 4.1 Auxiliares escritos a mano (Core)

Ya existen y se reutilizan: `ReadPc16()`, `Push(ushort)`, `Pop()` (`ZXSinclair.Net.Core/Z80/Z80Cpu.cs`).

En un `partial` nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Load16.cs`, `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:

| Método | Comportamiento |
|---|---|
| `ushort LoadWordAbsolute()` | `address = ReadPc16()`; lee `(address)` y `(address + 1)` con vuelta; `WZ = address + 1`; devuelve la palabra |
| `void StoreWordAbsolute(ushort value)` | `address = ReadPc16()`; escribe el byte bajo en `address` y el alto en `address + 1`; `WZ = address + 1`. El valor se captura antes de leer `nn`; ningún par de este grupo cambia al leer operandos |
| `void PushWithDelay(ushort value)` | `bus.Internal(Registers.IR, 1)` y `Push(value)`. `Push` sigue sin ciclo interno porque lo usan las interrupciones y, más adelante, `CALL`/`RST`, con otra temporización |

`Push` y `Pop` pasan a usarse en el camino caliente de instrucciones: se mantienen como están (sin comprobaciones ni ramas).

### 4.2 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Load16.cs`, añadidos a `PatternCatalog.Default`:

| Patrón | Reconoce | Cuerpo emitido (ejemplo) |
|---|---|---|
| `LoadPairImmediate` | `LD dd,nnnn` y `LD REGISTER,nnnn` | `Registers.BC = ReadPc16();` / `TIndex.Pair(ref Registers) = ReadPc16();` |
| `LoadPairAbsolute` | `LD dd,(nnnn)` y `LD REGISTER,(nnnn)` | `Registers.HL = LoadWordAbsolute();` |
| `StoreAbsolutePair` | `LD (nnnn),dd` y `LD (nnnn),REGISTER` | `StoreWordAbsolute(Registers.BC);` |
| `LoadStackPointer` | `LD SP,HL` y `LD SP,REGISTER` | `bus.Internal(Registers.IR, 2); Registers.SP = Registers.HL;` |
| `PushPair` | `PUSH qq` y `PUSH REGISTER` | `PushWithDelay(Registers.AF);` |
| `PopPair` | `POP qq` y `POP REGISTER` | `Registers.AF = Pop();` / `TIndex.Pair(ref Registers) = Pop();` |

- Los operandos `Register16` y `IndexPair` se traducen con `OperandEmitter.Value` (`Registers.BC`, `TIndex.Pair(ref Registers)`); `TIndex.Pair` devuelve `ref`, así que es asignable.
- `LoadPairAbsolute`/`StoreAbsolutePair` **no** usan `OperandEmitter.Value` para `(nnnn)` (que traduce a una lectura de 8 bits): llaman a los auxiliares de 4.1.
- Los patrones del grupo 1 no deben casar ninguna de estas formas (todas tienen un operando de 16 bits); los del grupo 2 exigen `Register16`/`IndexPair` en el operando de registro. `LD SP,HL` tiene dos `Register16`: solo lo reconoce `LoadStackPointer` (destino `SP`, origen `HL` o `REGISTER`). `EX`, `ADD`, `INC`, `DEC` y `JP (HL)` quedan fuera por mnemónico.
- `PUSH AF`/`POP AF` usan el par `AF` completo; no hay tratamiento especial de F.

### 4.3 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | 93 | 15 (`01 11 21 31 22 2A F9 C1 D1 E1 F1 C5 D5 E5 F5`) |
| ED | 12 | 8 (`43 4B 53 5B 63 6B 73 7B`) |
| DD/FD | 93 | 15 (6 entradas propias + 9 por la regla de ausentes) |
| CB, DDFDCB | 0 | 0 |

### 4.4 Rendimiento

- Cuerpos de una línea o llamadas a auxiliares `AggressiveInlining`; sin asignaciones ni ramas sobre el par en tiempo de ejecución.
- Según la guía del proceso, la siguiente medición toca tras el grupo 3; aquí solo se comprueba que `*Z80Cpu*` no empeora.

## 5. Pruebas

### 5.1 FUSE

Deben pasar con comparación de eventos los 35 casos del grupo:

| Bloque | Casos |
|---|---|
| Base (15) | `01 11 21 22 2a 31 c1 c5 d1 d5 e1 e5 f1 f5 f9` |
| ED (8) | `ed43 ed4b ed53 ed5b ed63 ed6b ed73 ed7b` |
| DD (6) | `dd21 dd22 dd2a dde1 dde5 ddf9` |
| FD (6) | `fd21 fd22 fd2a fde1 fde5 fdf9` |

El runner debe pasar de 166 a 201 casos, 0 fallos. FUSE no comprueba WZ ni distingue IR de PC en los ciclos internos: lo cubren los tests propios.

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs`, con `TestBus` y el estado inicial de `NopTests` (registros distintos de cero, `I = 0x42`). Cada test compara el `Z80Registers` completo con el esperado.

| Test | Montaje | Comprobación |
|---|---|---|
| `LdPairImm_AllPairs` (`[Theory]`, 4) | `01`, `11`, `21`, `31` con `nn = 0x1234` | Solo cambia el par; accesos `M1`, `Read pc+1`, `Read pc+2`; 10 T; WZ intacto |
| `LdIndexImm_SetsIndex` (`[Theory]`) | `DD 21 nn`, `FD 21 nn` | IX/IY = `nn`; HL intacto; 14 T |
| `LdHlAbsolute_ReadsLowThenHigh` | `2A 34 12` | `L = (0x1234)`, `H = (0x1235)`; orden de accesos; `WZ = 0x1235`; 16 T |
| `LdPairAbsolute_EdForms` (`[Theory]`, 4) | `ED 4B/5B/6B/7B nn` | Par cargado; `WZ = nn + 1`; 20 T |
| `LdHlAbsolute_UndocumentedEdDuplicates` (`[Theory]`) | `ED 6B nn`, `ED 63 nn` | Mismo efecto que `2A`/`22` con 20 T |
| `LdAbsoluteHl_WritesLowThenHigh` | `22 34 12` | `(0x1234) = L`, `(0x1235) = H`; orden `Write` bajo y alto; `WZ = 0x1235`; 16 T |
| `LdAbsolutePair_EdForms` (`[Theory]`, 4) | `ED 43/53/63/73 nn` | Memoria; WZ; 20 T |
| `LdAbsoluteIndex` (`[Theory]`) | `DD 22 nn`, `FD 2A nn` | IX/IY en memoria y desde memoria; WZ; 20 T |
| `LdWordAbsolute_WrapsAtFFFF` (`[Theory]`) | `2A FF FF`, `22 FF FF` | Byte alto en `0x0000`; `WZ = 0x0000` |
| `LdWordAbsolute_OperandWrapsPc` | `2A` en `0xFFFE` | Lee `nn` en `0xFFFF` y `0x0000`; `PC = 0x0001` |
| `LdSpHl_InternalCyclesOnIr` | `F9` | `SP = HL`; `Internal(IR, 2)` con `I = 0x42` y R tras el M1; 6 T |
| `LdSpIndex` (`[Theory]`) | `DD F9`, `FD F9` | `SP = IX/IY`; 10 T |
| `Push_AllPairs` (`[Theory]`, 4) | `C5 D5 E5 F5`, `SP = 0x9000` | `(0x8FFF) = alto`, `(0x8FFE) = bajo`; orden `Internal(IR,1)`, `Write SP-1`, `Write SP-2`; `SP = 0x8FFE`; 11 T |
| `PushIndex` (`[Theory]`) | `DD E5`, `FD E5` | IX/IY apilados; 15 T |
| `Push_WrapsStackPointer` | `C5` con `SP = 0x0000` | Escribe en `0xFFFF` y `0xFFFE`; `SP = 0xFFFE` |
| `Pop_AllPairs` (`[Theory]`, 4) | `C1 D1 E1 F1` | Par = palabra de la pila; orden `Read SP`, `Read SP+1`; `SP += 2`; 10 T |
| `PopAf_LoadsAllFlagBits` | `F1` con `0xFF` y `0x00` en la pila | `F` exactamente `0xFF` / `0x00` (incluidos F3/F5) |
| `PopIndex` (`[Theory]`) | `DD E1`, `FD E1` | IX/IY; 14 T |
| `Pop_WrapsStackPointer` | `C1` con `SP = 0xFFFF` | Lee `0xFFFF` y `0x0000`; `SP = 0x0001` |
| `PushPop_RoundTrip` | `C5` seguido de `D1` | `DE = BC` original; `SP` restaurado |
| `IndexPrefix_UnaffectedRunAsBase` (`[Theory]`) | `DD 01 nn`, `FD C5`, `DD F1` | Efecto de base; +4 T; IX/IY intactos |
| `Load16_DoesNotTouchFlagsWzOrCounter` (`[Theory]`) | Un opcode de cada patrón salvo `POP AF` (y WZ salvo `(nn)`) | `F` y WZ sin cambios; `UnimplementedOpcodes == 0` |
| `Load16_AllowsIntOnNextStep` | `LD SP,HL` y después INT con IM 1 | La INT apila en el `SP` nuevo |

### 5.3 Tests existentes que hay que adaptar

`01` (`LD BC,nn`) se usa hoy como opcode "no implementado" y pasa a estar implementado. Se sustituye por `D3` (`OUT (n),A`, grupo 10), que seguirá pendiente más tiempo; con `DD`/`FD` delante también cae en la tabla base por la regla de ausentes.

| Test | Cambio |
|---|---|
| `Z80CpuTests.UnimplementedDispatchCountsFetches` | `01` → `D3` en `{01}`, `{DD,01}`, `{FD,01}`, `{DD,FD,01}`, `{FD,DD,01}`; ciclos, R y PC iguales |
| `Z80CpuTests.LongAlternatingPrefixChainUsesConstantStackSpace` | `Memory[0xFFFF] = 0xD3` |
| `NopTests.Nop_DoesNotCountAsUnimplemented` | `0x01` → `0xD3` |
| `GeneratorTests` (cobertura) | Cifras de 4.3: base y DD/FD `93`; mensaje `93 implemented / 159 pending / 4 prefixes`; ED `12 implemented / 66 pending` |

Comprobar además que ningún test de `Load8Tests` usa opcodes de este grupo como "no implementados".

## 6. Criterios de aceptación

- Patrones de 4.2 en `Patterns/Load16.cs` y auxiliares de 4.1; `*.g.cs` regenerados (no editados a mano) y `--check` a 0.
- Cobertura del generador igual a 4.3.
- Los 35 casos FUSE de 5.1 pasan con eventos; runner con 201 pasados y 0 fallos.
- Tests de 5.2 en verde y los de 5.3 adaptados; `dotnet test` de los dos proyectos en verde.
- Benchmark `*Z80Cpu*` sin regresión ni asignaciones.
- Tabla de seguimiento de `spec-proceso-instrucciones.md`, spec CPU 8.1 y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- `EX (SP),HL`, `EX DE,HL`, `EX AF,AF'`, `EXX`: grupo 9.
- `INC`/`DEC`/`ADD`/`ADC`/`SBC` de 16 bits: grupo 6.
- `CALL`, `RET`, `RST` (usan `Push`/`Pop` con su propia temporización): grupo 3.
- `Q` tras `POP AF`: grupo 5.

## 8. Pendiente de verificar

- Dirección de los ciclos internos de `PUSH` y `LD SP,HL` (IR según la wiki; FUSE no lo distingue).
- Valor de `Q` tras `POP AF`.
