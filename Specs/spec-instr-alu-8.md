# Especificación: grupo 4, aritmética y lógica de 8 bits

Grupo 4 de `Specs/spec-proceso-instrucciones.md`: el *8-Bit Arithmetic Group* del manual de Zilog (`ADD`, `ADC`, `SUB`, `SBC`, `AND`, `OR`, `XOR`, `CP`, `INC`, `DEC` de 8 bits) con todos sus modos de direccionamiento y las variantes `DD`/`FD` documentadas y no documentadas. Es la parte más caliente del emulador: el cálculo de flags se especifica con fórmulas de bits y tablas precalculadas, sin ramas. Sigue la estructura de `Specs/spec-instr-saltos.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulo *8-Bit Arithmetic Group*.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus.
- Sean Young, *The Undocumented Z80 Documented*: F3/F5 (de `CP` en particular), H y P/V de cada operación, `IXH`/`IXL`.
- FUSE: 148 casos del grupo (sección 5.1) y entradas de `opcodes_base.dat` y `opcodes_ddfd.dat`. Los eventos de `86`, `dd86`, `34` y `dd34` confirman la temporización de la tabla 2.

## 2. Semántica

Notación de ciclos de spec CPU 4.4. `s` es el operando fuente: `r` ∈ {A, B, C, D, E, H, L}, `n`, `(HL)`, `(ii+d)` o, sin documentar, `IXH`/`IXL`/`IYH`/`IYL`. Los T-states incluyen los M1 de los prefijos.

### 2.1 Operaciones y temporización

| Instrucción | Opcodes | Ciclos de bus | T | WZ |
|---|---|---|---|---|
| `op A,r` | `80`–`BF` salvo `(HL)` (56) | `pc:4` | 4 | — |
| `op A,n` | `C6 CE D6 DE E6 EE F6 FE` | `pc:4, pc+1:3` | 7 | — |
| `op A,(HL)` | `86 8E 96 9E A6 AE B6 BE` | `pc:4, hl:3` | 7 | — |
| `op A,(ii+d)` | `DD`/`FD` `86 8E 96 9E A6 AE B6 BE` | `pc:4, pc+1:4, pc+2:3, pc+2:1 ×5, ii+d:3` | 19 | `ii + d` |
| `op A,IXH/IXL` | `DD`/`FD` `84 85 8C 8D 94 95 9C 9D A4 A5 AC AD B4 B5 BC BD` | `pc:4, pc+1:4` | 8 | — |
| `INC r` / `DEC r` | `04 0C 14 1C 24 2C 3C` / `05 0D 15 1D 25 2D 3D` | `pc:4` | 4 | — |
| `INC (HL)` / `DEC (HL)` | `34` / `35` | `pc:4, hl:3, hl:1, hl(w):3` | 11 | — |
| `INC (ii+d)` / `DEC (ii+d)` | `DD`/`FD` `34` / `35` | `pc:4, pc+1:4, pc+2:3, pc+2:1 ×5, ii+d:3, ii+d:1, ii+d(w):3` | 23 | `ii + d` |
| `INC`/`DEC` `IXH`/`IXL` | `DD`/`FD` `24 25 2C 2D` | `pc:4, pc+1:4` | 8 | — |

`op` ∈ {ADD, ADC, SUB, SBC, AND, XOR, OR, CP}. Las lecturas de memoria de `INC`/`DEC (HL)` y `(ii+d)` van seguidas de un ciclo interno de 1 T **sobre la misma dirección** y después la escritura (FUSE `34`, `dd34`).

### 2.2 Resultado y flags

`a` = A antes, `v` = operando, `c` = flag C antes (0/1), `r` = resultado de 9 bits (o con signo para restas), `r8 = r & 0xFF`.

| Operación | Resultado | S Z F5 F3 | H | P/V | N | C |
|---|---|---|---|---|---|---|
| `ADD` | `A = a + v` | de `r8` | acarreo del bit 3 | desbordamiento | 0 | acarreo del bit 7 |
| `ADC` | `A = a + v + c` | de `r8` | acarreo del bit 3 | desbordamiento | 0 | acarreo del bit 7 |
| `SUB` | `A = a - v` | de `r8` | préstamo del bit 4 | desbordamiento | 1 | préstamo |
| `SBC` | `A = a - v - c` | de `r8` | préstamo del bit 4 | desbordamiento | 1 | préstamo |
| `CP` | A no cambia (`r = a - v`) | S y Z de `r8`; **F5 y F3 de `v`** | préstamo del bit 4 | desbordamiento | 1 | préstamo |
| `AND` | `A = a & v` | de A | **1** | paridad | 0 | 0 |
| `XOR` / `OR` | `A = a ^ v` / `a \| v` | de A | 0 | paridad | 0 | 0 |
| `INC` | `x = v + 1` | de `x` | `(x & 0x0F) == 0` | `x == 0x80` | 0 | **se conserva** |
| `DEC` | `x = v - 1` | de `x` | `(x & 0x0F) == 0x0F` | `x == 0x7F` | 1 | **se conserva** |

Fórmulas sin ramas (con `int`):
- H: `(a ^ v ^ r) & Z80Flags.H` (suma y resta).
- C: `(r >> 8) & Z80Flags.C` (en restas `r` es negativo y `>> 8` es aritmético).
- P/V suma: `((a ^ ~v) & (a ^ r) & 0x80) >> 5`; resta: `((a ^ v) & (a ^ r) & 0x80) >> 5` (bit 7 → bit 2 = `PV`).
- S, Z, F5, F3: `Z80Flags.SZ53[r8]`; con paridad, `Z80Flags.SZ53P[A]`.
- `CP`: `(SZ53[r8] & ~(F5 | F3)) | (v & (F5 | F3))` más H, P/V, N y C de la resta.
- `INC`/`DEC`: tablas nuevas `Z80Flags.Inc[x]` y `Z80Flags.Dec[x]` (256 entradas, indexadas por el resultado, con S Z F5 F3 H P/V N ya combinados); `F = (F & C) | Inc[x]`.

Reglas comunes:
- `INC`/`DEC` de 8 bits no tocan C; ninguna instrucción del grupo toca IFF, I ni R salvo los M1.
- `Q` = F resultante en todas, pero no se escribe hasta el grupo 5 (spec CPU 3).
- `INC (ii+d)` / `DEC (ii+d)` y `op A,(ii+d)` usan `IndexedAddress<TIndex>()` (5 ciclos internos sobre `pc+2` y `WZ = ii + d`).

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `DD`/`FD` + `op A,r` con r ∈ {B, C, D, E, A}, `op A,n`, `INC`/`DEC` de B, C, D, E, A | El prefijo no afecta: como sin prefijo (regla de ausentes del generador) | +4 |
| `DD`/`FD` `84 85 8C 8D 94 95 9C 9D A4 A5 AC AD B4 B5 BC BD` | **No documentadas**: operando IXH/IXL (IYH/IYL) | 8 |
| `DD`/`FD` `24 25 2C 2D` | **No documentadas**: `INC`/`DEC` de IXH/IXL (IYH/IYL) | 8 |
| `DD`/`FD` `86`… `BE`, `34`, `35` | Operando `(ii+d)` | 19 / 23 |
| `NEG` (`ED 44` y alias), `DAA`, `CPL`, `SCF`, `CCF` | Grupo 5 | — |
| `INC`/`DEC`/`ADD`/`ADC`/`SBC` de 16 bits | Grupo 6 | — |

### 3.1 Formas de la tabla de FUSE

Las entradas no son homogéneas: `ADD A,B`, `ADC A,nn`, `SBC A,nn`, `XOR A,nn`, `SUB A,B` llevan `A` explícito, pero `SUB nn`, `AND nn`, `OR nn`, `CP B`, `CP nn`, `CP (HL)` no; en `opcodes_ddfd.dat` todas lo llevan (`CP A,REGISTERH`). Regla del patrón: el operando fuente es el **último**; si hay dos, el primero debe ser `A` (registro de 8 bits). Así `ADD HL,BC`, `ADC HL,BC` y `SBC HL,BC` (16 bits) no casan.

## 4. Implementación

### 4.1 Tablas de flags (`ZXSinclair.Net.Core/Z80/Z80Flags.cs`)

Añadir, en el constructor estático existente:
- `Inc[256]`: para el resultado `x`, `SZ53[x] | ((x & 0x0F) == 0 ? H : 0) | (x == 0x80 ? PV : 0)`.
- `Dec[256]`: `SZ53[x] | N | ((x & 0x0F) == 0x0F ? H : 0) | (x == 0x7F ? PV : 0)`.

### 4.2 Auxiliares escritos a mano (Core)

Nuevo `partial` `ZXSinclair.Net.Core/Z80/Z80Cpu.Alu8.cs`, todo `[MethodImpl(MethodImplOptions.AggressiveInlining)]`, sin ramas en el cálculo de flags (las fórmulas de 2.2):

| Método | Comportamiento |
|---|---|
| `void Add8(byte value)`, `Adc8`, `Sub8`, `Sbc8` | Operación sobre A y flags de 2.2 |
| `void And8(byte value)`, `Xor8`, `Or8` | `A op= value`; `F = SZ53P[A]` (`\| H` en `AND`) |
| `void Cp8(byte value)` | Flags de la resta con F5/F3 de `value`; A intacto |
| `byte Inc8(byte value)` / `byte Dec8(byte value)` | Devuelven `value ± 1` y actualizan F con `Inc`/`Dec` conservando C |
| `void IncMemory(ushort address)` / `DecMemory` | `v = bus.Read(address)`; `bus.Internal(address, 1)`; `bus.Write(address, Inc8(v))` |

Opcionalmente `Add8`/`Adc8` y `Sub8`/`Sbc8` comparten un núcleo con el acarreo como parámetro (`AddCore(value, carry)`), siempre que el JIT lo pliegue al hacer inline; validarlo con el benchmark de 4.5.

### 4.3 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Alu8.cs`, añadidos a `PatternCatalog.Default`:

| Patrón | Reconoce | Cuerpo emitido (ejemplo) |
|---|---|---|
| `Alu8Pattern` | `ADD`, `ADC`, `SUB`, `SBC`, `AND`, `XOR`, `OR`, `CP` con fuente `Register8`, `IndexHigh`, `IndexLow`, `Immediate8`, `(HL)` o `IndexedMemory` (regla de 3.1) | `Add8(Registers.B);`, `Sub8(ReadPc());`, `Cp8(bus.Read(Registers.HL));`, `Xor8(bus.Read(IndexedAddress<TIndex>()));`, `And8(TIndex.High(ref Registers));` |
| `IncDec8Register` | `INC`/`DEC` con `Register8`, `IndexHigh`, `IndexLow` | `Registers.B = Inc8(Registers.B);` / `TIndex.Low(ref Registers) = Dec8(TIndex.Low(ref Registers));` |
| `IncDec8Memory` | `INC`/`DEC` con `(HL)` o `IndexedMemory` | `IncMemory(Registers.HL);` / `DecMemory(IndexedAddress<TIndex>());` |

- El valor fuente se traduce con `OperandEmitter.Value`; la dirección de `IncDec8Memory` con `OperandEmitter.Address`.
- Ningún patrón casa operandos de 16 bits (`INC BC`, `ADD HL,BC`, `ADC HL,BC`): `Patterns_AreDisjoint` y un test explícito lo vigilan.

### 4.4 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | 223 | 88 (`80`–`BF`, 8 inmediatos, 16 `INC`/`DEC`) |
| ED | 20 | 0 |
| DD/FD | 223 | 88 (30 entradas propias + 58 por la regla de ausentes) |
| CB, DDFDCB | 0 | 0 |

### 4.5 Rendimiento

- Es el código más ejecutado: cero ramas en los flags, tablas de 256 bytes (`SZ53`, `SZ53P`, `Inc`, `Dec`), sin `checked`, sin conversiones innecesarias.
- Hito de medición: añadir `Z80CpuBenchmarks.ExecuteAluLoopFrame`, un bucle en RAM no contenida con mezcla de ALU (`ADD A,r`, `ADC A,n`, `SUB (HL)`, `AND`, `XOR`, `CP`, `INC r`, `DEC (HL)`, `op A,(IX+d)`) y `DJNZ`/`JR`, ns por T-state y × tiempo real; anotarlo en spec CPU 8.4 junto a `ExecuteFrame` y `ExecuteLoopFrame`. Si se prueba más de una variante del núcleo (4.2), anotar la comparación.

## 5. Pruebas

### 5.1 FUSE

148 casos del grupo:

| Bloque | Casos |
|---|---|
| Base (88) | `04 05 0c 0d 14 15 1c 1d 24 25 2c 2d 34 35 3c 3d`, `80`–`bf`, `c6 ce d6 de e6 ee f6 fe` |
| DD (30) | `dd24 dd25 dd2c dd2d dd34 dd35`, `dd84`–`dd86`, `dd8c`–`dd8e`, `dd94`–`dd96`, `dd9c`–`dd9e`, `dda4`–`dda6`, `ddac`–`ddae`, `ddb4`–`ddb6`, `ddbc`–`ddbe` |
| FD (30) | Los mismos con `fd` |

Además pasa a ejecutarse el caso `10` (`DJNZ` que termina con `INC C`), pendiente desde el grupo 3. El runner debe pasar de 280 a **429** pasados (148 + 1), 0 fallos. Verificar con `--filter` por bloques (`8`, `9`, `a`, `b`, `dd`, `fd`) y, ante un fallo, con `--filter <caso> --verbose`: el informe muestra los flags bit a bit.

Convenciones de FUSE conocidas que afectan al grupo: ninguna nueva. FUSE no compara `Q` ni WZ.

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs`, con `TestBus` y el estado inicial de `NopTests`.

**Exhaustivos contra un modelo de referencia.** El test incluye una implementación de referencia **independiente** de las fórmulas de 2.2 (aritmética entera explícita y comprobaciones bit a bit, sin las tablas del Core) y recorre todas las combinaciones:

| Test | Recorrido | Comprobación |
|---|---|---|
| `Alu_AllOperands_MatchReference` (`[Theory]` por operación, 8) | `a` y `v` de 0 a 255 y `c` 0/1 (131 072 casos por operación), ejecutando el opcode `op A,B` | A y F exactos frente a la referencia |
| `IncDec_AllValues_MatchReference` (`[Theory]`, 2) | `v` de 0 a 255 y C 0/1 | Resultado y F; C conservado |
| `FlagTables_IncDec` | Las 256 entradas de `Z80Flags.Inc`/`Dec` | Iguales a la referencia |

**Casos concretos y temporización:**

| Test | Montaje | Comprobación |
|---|---|---|
| `Add_OverflowAndHalfCarry` (`[Theory]`) | `0x7F + 1`, `0xFF + 1`, `0x0F + 1`, `0x80 + 0x80` | P/V, H, C, Z, S esperados |
| `Sub_BorrowAndOverflow` (`[Theory]`) | `0x80 - 1`, `0 - 1`, `0x10 - 1` | P/V, H, C, N |
| `Cp_TakesF5F3FromOperand` | `A = 0x00`, `CP 0x28` | F5 y F3 = bits de `0x28`; A intacto |
| `And_SetsHalfCarry`, `XorOr_ClearHalfAndCarry` | Valores con paridad par/impar | H, P/V (paridad), N = C = 0 |
| `Adc_Sbc_UseCarry` (`[Theory]`) | Con C = 0 y 1 | Resultado ± 1 |
| `Inc_PreservesCarry` / `Dec_PreservesCarry` (`[Theory]`) | `0x7F`, `0x80`, `0xFF`, `0x00`, `0x0F`, `0x10` con C = 0/1 | Resultado, P/V, H, N y C intacto |
| `AluRegister_AllSources` (`[Theory]`, 8 ops × 8 fuentes) | `80`–`BF` | Operando leído del registro correcto (incluido `(HL)`); 4 / 7 T; accesos |
| `AluImmediate_AllOps` (`[Theory]`, 8) | `C6`…`FE` | `Read pc+1`; 7 T; `PC += 2` |
| `AluIndexed_DisplacementAndWz` (`[Theory]`) | `DD 86 d`, `FD BE d` con `d` = `+5`, `-1`, `-128` | `(ii+d)`; `WZ = ii+d`; `Internal(pc+2, 5)`; 19 T |
| `AluIndexHalves_Undocumented` (`[Theory]`) | `DD 84`, `FD 95`, `DD AC`, `FD BD` | Operando IXH/IXL/IYH/IYL; 8 T |
| `IncDecRegister_All` (`[Theory]`, 14) | `04`…`3D` | Solo cambia el registro y F; 4 T |
| `IncDecHl_ReadInternalWrite` (`[Theory]`) | `34`, `35` | Orden `M1`, `Read HL`, `Internal(HL, 1)`, `Write HL`; 11 T; WZ intacto |
| `IncDecIndexed_ReadInternalWrite` (`[Theory]`) | `DD 34 d`, `FD 35 d` | Orden de accesos completo; WZ; 23 T |
| `IncDecIndexHalves_Undocumented` (`[Theory]`) | `DD 24`, `DD 2D`, `FD 25`, `FD 2C` | IXH/IXL/IYH/IYL; 8 T |
| `IndexPrefix_UnaffectedAluRunsAsBase` (`[Theory]`) | `DD 80`, `FD C6 n`, `DD 3C` | Efecto de base; +4 T; IX/IY intactos |
| `Alu8_DoesNotCountAsUnimplemented` | Un opcode de cada patrón | `UnimplementedOpcodes == 0` |
| `Alu8_AllowsIntOnNextStep` | `ADD A,n` e INT con IM 1 | La INT se acepta en el `Step()` siguiente |

En `Z80FlagsTests.cs`, añadir la comprobación de `Inc`/`Dec` si no queda cubierta por `FlagTables_IncDec`.

### 5.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| `GeneratorTests` (cobertura) | Cifras de 4.4: base y DD/FD `223`; `223 implemented / 29 pending / 4 prefixes`; ED sin cambios (`20 implemented / 58 pending`) |
| `JumpTests` | Si algún test usa `INC`/`DEC`/ALU como opcode "no implementado" o como dato ejecutado, revisarlo |
| `Z80CpuTests`, `NopTests` | Sin cambios: `D3` (grupo 10) sigue pendiente |
| Tabla de seguimiento | El caso `10` deja de estar pendiente del grupo 4 |

## 6. Criterios de aceptación

- Tablas `Inc`/`Dec`, auxiliares de 4.2 y patrones de 4.3; `*.g.cs` regenerados (no editados a mano) y `--check` a 0.
- Cobertura del generador igual a 4.4.
- Los 148 casos FUSE de 5.1 y el caso `10` pasan con eventos; runner con 429 pasados y 0 fallos.
- Tests de 5.2 en verde (incluidos los exhaustivos) y los de 5.3 adaptados; `dotnet test` de los dos proyectos en verde.
- `ExecuteAluLoopFrame` añadido y medido; `*Z80Cpu*` sin regresión ni asignaciones; resultados en spec CPU 8.4.
- Tabla de seguimiento de `spec-proceso-instrucciones.md`, spec CPU 8.1 y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- `NEG`, `DAA`, `CPL`, `SCF`, `CCF` y `Q` (grupo 5).
- Aritmética de 16 bits (grupo 6).
- Rotaciones, desplazamientos y `BIT`/`SET`/`RES`, incluidas las copias `DDCB` (grupos 7 y 8).

## 8. Verificación y pendientes

- Flags de las variantes no documentadas con IXH/IXL frente a z80test (`z80full`), cuando se integre.
- Comparación realizada el 2026-10-02 (spec CPU 8.4): métodos separados 0.6627 ns/T-state frente a núcleos compartidos 0.6736. Se conservan los separados, con 0 B asignados y sin regresión observada en NOP ni en el bucle anterior.
- Verificado: 1070 tests del Core (347 nuevos, incluido el modelo exhaustivo), 86 del generador, cobertura 223/20/223, `--check` a 0 y FUSE 429 pasados / 0 fallos / 906 omitidos con eventos.
