# Especificación: grupo 8, operaciones de bit

Grupo 8 de `Specs/spec-proceso-instrucciones.md`: el *Bit Set, Reset, and Test Group* del manual de Zilog (`BIT`, `SET`, `RES`) sobre registro, `(HL)` e `(IX+d)`/`(IY+d)`, con los alias de `BIT` en `DDCB`/`FDCB` y las copias no documentadas `LD r,SET/RES b,(ii+d)`. Completa las tablas `CB` y `DDFDCB` iniciadas en el grupo 7. Sigue la estructura de `Specs/spec-instr-rotaciones.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulo *Bit Set, Reset, and Test Group*.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus.
- Sean Young, *The Undocumented Z80 Documented*: F3/F5 de `BIT` (del registro, de MEMPTR en `(HL)` y del byte alto de `ii + d`), alias y copias `DDCB`.
- FUSE: 584 casos (sección 5.1) y entradas de `opcodes_cb.dat` y `opcodes_ddfdcb.dat`. Los casos `cb40`, `cb46`, `cb7e`, `ddcb40`, `ddcb46`, `cbc6` y `ddcbc0` confirman flags y temporización (sección 2).

## 2. Semántica

Notación de ciclos de spec CPU 4.4. `b` ∈ 0…7, `mask = 1 << b`; `r` ∈ {A, B, C, D, E, H, L}. Los T-states incluyen los M1 de los prefijos.

### 2.1 Operaciones y temporización

| Instrucción | Opcodes | Ciclos de bus | T | Efecto |
|---|---|---|---|---|
| `BIT b,r` | `CB 40`–`7F` salvo `(HL)` (56) | `pc:4, pc+1:4` | 8 | Flags (2.2) |
| `BIT b,(HL)` | `CB 46 4E 56 5E 66 6E 76 7E` | `pc:4, pc+1:4, hl:3, hl:1` | 12 | Flags |
| `BIT b,(ii+d)` | `DD`/`FD` `CB d 40`–`7F` (64; 8 opcodes por bit, todos alias de `BIT b,(ii+d)`) | prefijo DDCB + `ii+d:3, ii+d:1` | 20 | Flags |
| `RES b,r` / `SET b,r` | `CB 80`–`BF` / `CB C0`–`FF` salvo `(HL)` (112) | `pc:4, pc+1:4` | 8 | `r &= ~mask` / `r \|= mask` |
| `RES b,(HL)` / `SET b,(HL)` | `CB 86`… / `CB C6`… (16) | `pc:4, pc+1:4, hl:3, hl:1, hl(w):3` | 15 | Memoria |
| `RES b,(ii+d)` / `SET b,(ii+d)` | `DD`/`FD` `CB d 86 8E…` / `C6 CE…` (16) | prefijo DDCB + `ii+d:3, ii+d:1, ii+d(w):3` | 23 | Memoria |
| `LD r,RES b,(ii+d)` / `LD r,SET b,(ii+d)` | `DD`/`FD` `CB d 80`–`FF` salvo `x6`/`xE` (112) | Igual que la anterior | 23 | Memoria **y** `r` (H y L reales) |

"Prefijo DDCB" = `pc:4, pc+1:4, pc+2:3, pc+3:3, pc+3:1 ×2`, que ya hace `FinishIndexed`, junto con `WZ = ii + d` (grupo 7). La R sube 2 en las formas `DDCB` y `CB`. FUSE `cb46` (12 T), `ddcb46` (20 T), `cbc6` (15 T), `ddcbc0` (23 T, `B = 93`).

### 2.2 Flags de `BIT`

`v` = valor probado, `x` = fuente de F5/F3:

| Forma | `x` |
|---|---|
| `BIT b,r` | `r` (el registro probado) |
| `BIT b,(HL)` | `WZ >> 8` (byte alto de MEMPTR, el valor que dejó la instrucción anterior que tocó WZ) |
| `BIT b,(ii+d)` | `WZ >> 8` = byte alto de `ii + d` (WZ lo acaba de fijar `FinishIndexed`) |

`F = (F & C) | H | (SZ53P[v & mask] & (S | Z | PV)) | (x & (F5 | F3))`

- Z = P/V = 1 si el bit es 0; S = 1 solo si `b = 7` y el bit vale 1; H = 1; N = 0; C se conserva. La tabla `SZ53P` aplicada a `v & mask` da Z, P/V y S sin ramas (un bit aislado tiene paridad impar).
- Comprobaciones: `cb40` (`B = BC`, F5/F3 de B → `F = 7C`); `ddcb46` (dirección `A381` → F5 de `A3`, `F = 30`); `ddcb40` (`8BBE` → F3 de `8B`, C conservado, `F = 19`).
- `BIT` no modifica el operando ni WZ.

### 2.2.1 Incompatibilidad con los fixtures de FUSE en `BIT b,(HL)`

Los ocho casos FUSE `cb46 cb4e cb56 cb5e cb66 cb6e cb76 cb7e` calculan F5/F3 **del valor leído** de `(HL)` (modelo de FUSE anterior a MEMPTR), no del byte alto de WZ: `cb4e` lee `5B` y espera `F = 18` (F3 = bit 3 de `5B`); `cb5e` lee `3C` → `F = 38`; `cb6e` lee `31` → `30`; `cb76` lee `18` → `5C`. En los otros cuatro el valor tiene los bits 3 y 5 a 0 y coincide por casualidad con `WZ = 0`. No existe un WZ inicial que satisfaga los ocho, porque FUSE no define MEMPTR en sus entradas.

Decisión:
- La CPU sigue el **hardware** (F5/F3 de `WZ >> 8`), como fija 2.2; no se adapta al fixture.
- `ZXSinclair.Net.Fuse` incorpora una tabla declarada de convenciones (`FuseConventions`) con, para cada caso afectado, los bits de F que no se comparan y el motivo. Para estos ocho casos: F5 y F3 de F (`0x28`), motivo "FUSE calcula F5/F3 de BIT n,(HL) con el valor leído, no con MEMPTR". El resto de F, registros, memoria, eventos y T-states se comparan igual.
- La convención **no oculta** la diferencia: el informe del runner lista los casos en los que se aplicó y los bits ignorados, y el resumen final cuenta "pasados con convención". Un caso que falle en cualquier otro dato sigue fallando.
- La regla de MEMPTR la verifican los tests propios (5.2); z80test (`z80memptr`) será la referencia de hardware cuando se integre.
- Los casos `ddcb4x`–`ddcb7x` y `fdcb…` no necesitan convención: FUSE usa ahí el byte alto de `ii + d`, que coincide con `WZ`.

### 2.3 `SET`/`RES`, WZ y `Q`

- `SET`/`RES` no tocan flags. En `(HL)` no tocan WZ; en `DDCB` WZ ya vale `ii + d`.
- `Q`: `BIT` escribe flags (`WritesFlags = true`, `Q = F`); `SET` y `RES` no (`Q = 0`).
- La exactitud de F5/F3 en `BIT b,(HL)` depende de que todas las instrucciones mantengan WZ según su spec. Los grupos 9–11 (bloques, E/S, `EX (SP),HL`) también lo actualizan; hasta entonces, WZ tras esas instrucciones no es fiable, pero siguen sin implementar.

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `DD`/`FD` `CB d 40`–`7F` | Los 8 opcodes de cada bit son alias de `BIT b,(ii+d)` (en la tabla, líneas sin mnemónico + la del `x6`) | 20 |
| `DD`/`FD` `CB d 80`–`FF` salvo `x6`/`xE` | **No documentadas**: copia del resultado a registro | 23 |
| `CB` con prefijo `DD`/`FD` | Siempre forma `DDCB`/`FDCB` (spec CPU 4.3) | — |

## 4. Implementación

### 4.1 Auxiliares escritos a mano (Core)

Nuevo `partial` `ZXSinclair.Net.Core/Z80/Z80Cpu.Bits.cs`, `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:

| Método | Comportamiento |
|---|---|
| `void BitTest(byte mask, byte value)` | Flags de 2.2 con `x = value` |
| `void BitTestMemory(byte mask, byte value)` | Flags de 2.2 con `x = WZ >> 8` |

`SET`/`RES` se emiten en línea con la máscara como constante (`(byte)(r | 0x08)`, `(byte)(r & 0xF7)`).

### 4.2 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Bits.cs`, añadidos a `PatternCatalog.Default`. La máscara se calcula en el generador a partir del operando `Bit` (`Operand.Value`) y se emite en hexadecimal.

| Patrón | Reconoce | Cuerpo emitido (ejemplo) | `WritesFlags` |
|---|---|---|---|
| `BitTestRegisterPattern` | `BIT b,r` (CB) | `BitTest(0x01, Registers.B);` | sí |
| `BitTestMemoryPattern` | `BIT b,(HL)` (CB) y `BIT b,(REGISTER+dd)` con alias (DDFDCB) | `var value = bus.Read(Registers.HL); bus.Internal(Registers.HL, 1); BitTestMemory(0x01, value);` (con `address` en DDFDCB) | sí |
| `SetResRegisterPattern` | `SET`/`RES b,r` (CB) | `Registers.B = (byte)(Registers.B \| 0x01);` / `Registers.H = (byte)(Registers.H & 0x7F);` | no |
| `SetResMemoryPattern` | `SET`/`RES b,(HL)` y `b,(REGISTER+dd)` | `var value = bus.Read(Registers.HL); bus.Internal(Registers.HL, 1); value = (byte)(value \| 0x40); bus.Write(Registers.HL, value);` | no |
| `SetResMemoryCopyPattern` | Formas compuestas de DDFDCB `LD r,SET/RES b,(REGISTER+dd)` | Igual con `address` y `Registers.B = value;` al final | no |

- Los alias de `BIT` en DDFDCB (8 por bit) comparten cuerpo: el emisor los agrupa en un `case` múltiple.
- Los patrones de rotación del grupo 7 no casan `BIT`/`SET`/`RES`, y estos no casan rotaciones: `Patterns_AreDisjoint` lo vigila.

### 4.3 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | 246 | 0 |
| ED | 46 | 0 |
| CB | **256** (completa) | 192 (`40`–`FF`) |
| DD/FD | 246 | 0 |
| DDFDCB | **256** (completa) | 192 (`40`–`FF`) |

### 4.4 Tablas completas en el generador

Con este grupo CB y DDFDCB cubren los 256 valores del `switch (opcode)` sobre `byte`, y el `default: Unimplemented(); break;` que emite `DispatchEmitter` queda inaccesible: el compilador da **CS0162** (código inaccesible) en `Z80Cpu.CB.g.cs` y `Z80Cpu.IndexedCB.g.cs`. `DispatchEmitter` debe emitir el `default` **solo** si la tabla tiene algún valor sin `case` (opcode pendiente, hueco o prefijo). Ninguna tabla generada debe producir warnings.

### 4.5 Rendimiento

- `BIT` es muy frecuente en la ROM y en juegos (lectura de teclado, bucles de espera): auxiliar de una lectura de tabla y operaciones de bits, sin ramas.
- `SET`/`RES` con constante en línea: una operación lógica.
- Repetir los tres benchmarks de spec CPU 8.4. Con este grupo solo faltan intercambios, bloques, E/S y huecos de ED; según la guía, el arranque de la ROM del 48K como carga de trabajo llega con el grupo 10.

## 5. Pruebas

### 5.1 FUSE

584 casos:

| Bloque | Casos |
|---|---|
| CB (200) | `cb40`–`cbff` y las variantes `cb47_1 cb4f_1 cb57_1 cb5f_1 cb67_1 cb6f_1 cb77_1 cb7f_1` |
| DDCB (192) | `ddcb40`–`ddcbff` |
| FDCB (192) | `fdcb40`–`fdcbff` |

El runner debe pasar de 686 a **1270** pasados, 0 fallos (quedan 65 casos de los grupos 9–11). Verificar con `--filter cb4`…`cbf`, `ddcb`, `fdcb`; ante un fallo, `--filter <caso> --verbose`.

**Convenciones de FUSE:** los casos no inicializan MEMPTR (el runner carga `WZ = 0`), y en `BIT b,(HL)` FUSE calcula F5/F3 con el valor leído (2.2.1): esos ocho casos pasan con la convención declarada que ignora F5/F3 de F, y el runner informa de ello (`pasados con convención: 8`). FUSE no compara WZ ni `Q`.

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs`, con `TestBus` y el estado inicial de `NopTests` (`I = 0x42`, `Q = 0`).

| Test | Montaje | Comprobación |
|---|---|---|
| `BitRegister_AllBitsAllValues` (`[Theory]` por bit, 8) | Modelo de referencia independiente; `BIT b,B` con B de 0 a 255 × C 0/1 | F exacto (Z, P/V, S, H, N, C conservado, F5/F3 de B); B intacto |
| `BitRegister_AllRegisters` (`[Theory]`, 7) | `CB 40`…`7F` sobre cada registro | Prueba el registro correcto; 8 T |
| `BitHl_F5F3FromWz` (`[Theory]`) | `BIT 3,(HL)` con WZ = `0x2800`, `0x0000`, `0xFFFF` | F5/F3 del byte alto de WZ, no de `(HL)` ni de H; WZ intacto; 12 T; accesos `Read HL`, `Internal(HL, 1)` |
| `BitHl_AfterInstructionThatSetsWz` | `LD A,(nn)` y después `BIT 0,(HL)` | F5/F3 de `(nn + 1) >> 8` |
| `BitIndexed_F5F3FromAddressHigh` (`[Theory]`) | `DD CB d 46`, `FD CB d 7E` con direcciones de byte alto `0x28`, `0xA3`, `0x8B` | F5/F3 del byte alto de `ii + d`; `WZ = ii + d`; 20 T |
| `BitIndexed_AllAliases` (`[Theory]`, 8) | `DD CB d 40`…`47` | Mismo resultado que `46` |
| `SetResRegister_AllBits` (`[Theory]`, 2 × 8 × 7) | `CB 80`–`FF` salvo `(HL)` | Solo cambia el bit del registro; F intacto; `Q = 0`; 8 T |
| `SetResHl_ReadInternalWrite` (`[Theory]`) | `CB 86`, `CB FE` | Orden `Read HL`, `Internal(HL, 1)`, `Write HL`; 15 T; F y WZ intactos |
| `SetResIndexed_ReadInternalWrite` (`[Theory]`) | `DD CB d 86`, `FD CB d FE` con `d` negativo | Accesos completos; 23 T |
| `SetResIndexedCopy_AllRegisters` (`[Theory]`, 2 × 7) | `DD CB d 80`…`87` y `C0`…`C7` salvo `x6` | Memoria y registro (H y L reales); IXH/IXL intactos (FUSE `ddcbc0`) |
| `Bit_SetsQ_SetResClearQ` (`[Theory]`) | `BIT`, `SET`, `RES` | `Q = F` tras `BIT`; `Q = 0` tras `SET`/`RES` |
| `Bits_DoNotCountAsUnimplemented` | Un opcode de cada patrón | `UnimplementedOpcodes == 0` |

### 5.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| `Z80CpuTests.UnimplementedDispatchCountsFetches` | Eliminar las filas `CB 80`, `FD CB FE 86`, `DD CB 80 86` y la comprobación específica de `DDCB`: ya no quedan opcodes CB/DDCB pendientes (lo anunció la spec del grupo 7). Los ciclos de prefijo `DDCB` siguen cubiertos por `RotateTests`, `BitTests` y FUSE |
| `FuseReportTests` | Nuevos: `Convention_IgnoresOnlyDeclaredFlagBits` (con `cb4e`: pasa ignorando F5/F3, falla si difiere otro bit de F o un registro), `Convention_IsReportedInOutput` (el informe y el resumen mencionan la convención) y `Conventions_ListOnlyBitHlCases` |
| `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset` | `DD CB 0 86` → `DD ED 00` (hueco de ED tras prefijo, grupo 11) con la dirección final correspondiente |
| `GeneratorTests` (cobertura y `default`) | La aserción que exige `default: Unimplemented(); break;` en todos los ficheros pasa a exigirlo solo en tablas incompletas; nuevo `Output_OmitsDefaultWhenTableComplete` (CB y DDFDCB sin `default`, base/ED/DDFD con él). CB y DDFDCB `256 implemented / 0 pending`; resto sin cambios. Añadir `BitPatterns_EmitExpectedBodies` (`BIT 7,A`, `BIT 0,(HL)`, alias `BIT 0,(REGISTER+dd)`, `SET 3,C`, `RES 7,(HL)`, `LD L,SET 1,(REGISTER+dd)`) y `WritesFlags` (true en los dos de `BIT`, false en los de `SET`/`RES`) |

## 6. Criterios de aceptación

- Auxiliares de 4.1 y patrones de 4.2; `*.g.cs` regenerados (no editados a mano) y `--check` a 0.
- Cobertura del generador igual a 4.3 (CB y DDFDCB completas).
- Los 584 casos FUSE de 5.1 pasan con eventos (8 de ellos con la convención de 2.2.1, informada por el runner); runner con 1270 pasados y 0 fallos.
- Compilación sin warnings: ni CS0162 en las tablas completas ni ningún otro nuevo.
- Tests de 5.2 en verde y los de 5.3 adaptados; `dotnet test` de los dos proyectos en verde.
- Benchmarks de spec CPU 8.4 repetidos sin regresión ni asignaciones.
- Tabla de seguimiento de `spec-proceso-instrucciones.md`, `Specs/estado-instrucciones-z80.md` (entradas `BIT`, `SET`, `RES` con enlaces y recuento), spec CPU 8.1 y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- Intercambios, bloques, E/S y huecos de ED (grupos 9–11), incluido su efecto sobre WZ.

## 8. Pendiente de verificar

- F5/F3 de `BIT b,(HL)` con MEMPTR tras todas las instrucciones, frente a `z80memptr` (z80test), cuando se integre.
- Flags de las copias `DDCB` de `SET`/`RES` frente a `z80full`.
