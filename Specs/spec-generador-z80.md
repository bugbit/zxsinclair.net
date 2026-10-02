# Especificación: generador de instrucciones del Z80

Especificación de `ZXSinclair.Net.Generate.Z80OpCodes`, la herramienta que produce los despachos de instrucciones de `Z80Cpu<TBus>` a partir de las tablas de opcodes de FUSE. Es el paso 0 de `Specs/spec-proceso-instrucciones.md` y desarrolla el contrato de `Specs/spec-cpu-z80.md` sección 5. El piloto es `NOP` (`Specs/spec-instr-nop.md`): el generador debe producir el mismo comportamiento que el despacho provisional actual.

Esta spec fija **la mecánica** del generador (entrada, modelo, patrones, salida, ejecución, pruebas). La **semántica** de cada instrucción (ciclos, flags, registros) la fija la spec de su grupo; cada grupo añade sus patrones al catálogo del generador.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Specs/spec-cpu-z80.md` sección 5 (contrato) y 4.3 (prefijos).
- `Specs/spec-instr-nop.md` sección 4.1 (opcodes indexados que no están en `opcodes_ddfd.dat`).
- Tablas de FUSE en `ZXSinclair.Net.Generate.Z80OpCodes/data/`.

## 2. Entrada: tablas de opcodes

### 2.1 Ficheros

| Fichero | Tabla | Método destino | Estado |
|---|---|---|---|
| `opcodes_base.dat` | Sin prefijo | `ExecuteMain(byte)` | En el proyecto |
| `opcodes_cb.dat` | `CB xx` | `ExecuteCB(byte)` | **Recuperar** del historial de git |
| `opcodes_ed.dat` | `ED xx` | `ExecuteED(byte)` | En el proyecto |
| `opcodes_ddfd.dat` | `DD xx` / `FD xx` | `ExecuteIndexedOpcode<TIndex>(byte)` | En el proyecto |
| `opcodes_ddfdcb.dat` | `DD CB d xx` / `FD CB d xx` | `ExecuteIndexedCB<TIndex>(ushort address, byte)` | **Recuperar** del historial de git |

`opcodes_cb.dat` y `opcodes_ddfdcb.dat` se borraron con el generador antiguo. Están en `builderCodes/generateZ80Ops/` en el commit anterior a `3aa51e0` y se recuperan sin cambios:

```bash
git show 3aa51e0^:builderCodes/generateZ80Ops/opcodes_cb.dat > ZXSinclair.Net.Generate.Z80OpCodes/data/opcodes_cb.dat
```

```bash
git show 3aa51e0^:builderCodes/generateZ80Ops/opcodes_ddfdcb.dat > ZXSinclair.Net.Generate.Z80OpCodes/data/opcodes_ddfdcb.dat
```

Las cinco tablas son recursos incrustados (`EmbeddedResource`) del generador. Proceden de FUSE (GPL v2 o posterior **(verificar)**); se conservan sus cabeceras `$Id`.

### 2.2 Formato

Una entrada por línea: `0xNN MNEMÓNICO operandos`.

| Caso | Ejemplo | Tratamiento |
|---|---|---|
| Comentario o línea vacía | `# opcodes_base…` | Se ignora |
| Entrada normal | `0x41 LD B,C` | Un opcode con su mnemónico y operandos separados por comas |
| Opcode sin mnemónico | `0x44` … `0x7c NEG` (ED); `0x40` … `0x47 BIT 0,(REGISTER+dd)` (DDFDCB) | **Alias**: toma el mnemónico y los operandos de la siguiente línea que los tenga |
| Prefijo | `0xcb shift CB`, `0xdd shift DD`, `0xcb shift DDFDCB` | No genera código: los prefijos los consume el ciclo escrito a mano (spec CPU 4.3) |
| Trampa de FUSE | `0xfb slttrap` (ED) | No es una instrucción del Z80: se trata como hueco de ED |
| Opcode ausente | ED `0x00`…`0x3f`; DD/FD `0x00` | Ver 2.3 |

Errores (opcode duplicado, alias sin línea destino, byte fuera de rango, mnemónico vacío al final) detienen el generador con un mensaje que incluye fichero y línea.

### 2.3 Opcodes ausentes

| Tabla | Opcode ausente | Significado | Se genera como |
|---|---|---|---|
| ED | No figura o es `slttrap` | Hueco de ED: dos NOP, 8 T (spec CPU 4.3) | Patrón `EdHole` (grupo 11) |
| DD/FD | No figura | El prefijo no afecta: se ejecuta como sin prefijo | **Mismo cuerpo que en `opcodes_base.dat`** (regla 4.1 de `spec-instr-nop.md`); los bytes `CB`, `DD`, `ED`, `FD` no se generan |
| base, CB, DDFDCB | — | Las tres tablas están completas (256 entradas) | — |

Sin patrón implementado, cualquier opcode cae en `default: Unimplemented(); break;`.

### 2.4 Vocabulario de operandos

El parser clasifica cada operando; el emisor lo traduce a una expresión C#. La traducción exacta de los accesos a memoria (ciclos, WZ) la fija la spec de cada grupo; aquí solo se fija el vocabulario y el nombre de su traducción.

| Operando en la tabla | Clase | Traducción C# |
|---|---|---|
| `A`, `B`, `C`, `D`, `E`, `H`, `L`, `F` | Registro de 8 bits | `Registers.A` … |
| `I`, `R` | Registro especial | `Registers.I`, `Registers.R` |
| `AF`, `BC`, `DE`, `HL`, `SP`, `AF'` | Par de 16 bits | `Registers.AF` … (`AF'` → `Registers.AF_`) |
| `REGISTER` | Par índice | `TIndex.Pair(ref Registers)` |
| `REGISTERH`, `REGISTERL` | Mitad del índice | `TIndex.High(ref Registers)`, `TIndex.Low(ref Registers)` |
| `(HL)`, `(BC)`, `(DE)`, `(SP)` | Memoria por par | Acceso al bus en la dirección del par |
| `(REGISTER+dd)` | Memoria indexada | En DD/FD: dirección calculada por un auxiliar (`IndexedAddress<TIndex>()`); en DDFDCB: el parámetro `address` |
| `nn` | Inmediato de 8 bits | `ReadPc()` |
| `nnnn` | Inmediato de 16 bits | Auxiliar `ReadPc16()` |
| `(nnnn)` | Memoria absoluta | Acceso al bus en `ReadPc16()` |
| `offset` | Desplazamiento relativo | `(sbyte)ReadPc()` |
| `NZ`, `Z`, `NC`, `C`, `PO`, `PE`, `P`, `M` | Condición | Expresión sobre `Registers.F` (el contexto distingue `C` registro de `C` condición: solo es condición como primer operando de `JP`, `JR`, `CALL`, `RET`) |
| `(C)`, `(nn)` | Puerto | Ver spec del grupo 10 |
| `0`…`7` | Bit | Constante |
| `00`, `08`…`38` | Dirección de `RST` | Constante |
| `0`, `1`, `2` | Modo `IM` | Constante |

Un operando que el parser no reconoce es un error del generador, no un caso "no implementado".

## 3. Modelo y patrones

### 3.1 Modelo

El parser produce, por tabla, 256 entradas `Opcode { Table, Byte, Mnemonic, Operands[], Kind }` con `Kind` = instrucción, alias resuelto, prefijo, hueco o ausente. El modelo no contiene código C#.

### 3.2 Catálogo de patrones

- Un **patrón** reconoce un conjunto de opcodes por mnemónico y forma de operandos (por ejemplo, `LD r,r'`, `LD r,(HL)`, `ALU A,r`, `JR cc,offset`) y **emite el cuerpo** del `case`.
- Los patrones se agrupan por grupo del manual en ficheros del generador: `Patterns/Control.cs` (`NOP`), `Patterns/Load8.cs`, `Patterns/Load16.cs`… Cada spec de grupo enumera sus patrones y su código emitido.
- Cada opcode encaja **como mucho en un patrón**; si encaja en dos, el generador falla. Si no encaja en ninguno, se emite `Unimplemented()` y cuenta como pendiente (sección 6).
- Un mismo patrón sirve para la tabla base y para la indexada cuando la forma coincide; los operandos `REGISTER*` se traducen según 2.4.

### 3.3 Código emitido

- Cuerpos cortos: asignaciones directas o llamadas a métodos auxiliares `[MethodImpl(MethodImplOptions.AggressiveInlining)]` escritos a mano en ficheros `partial` de `Z80Cpu<TBus>` del Core (`Z80Cpu.Alu.cs`, `Z80Cpu.Memory.cs`…, que crea cada grupo según lo necesite). Los auxiliares no se generan.
- Los cuerpos se especializan por operandos concretos en tiempo de generación: nada de `switch` sobre el registro en tiempo de ejecución, `ref` elegidos dinámicamente, delegados ni tablas de `Action`.
- Cada `case` lleva un comentario con el mnemónico original de la tabla: `case 0x41: Registers.B = Registers.C; break; // LD B,C`.
- Los alias comparten cuerpo apilando etiquetas: `case 0x44: case 0x4c: … case 0x7c: Neg(); break; // NEG`.

## 4. Salida

### 4.1 Ficheros

Se sustituye `ZXSinclair.Net.Core/Z80/Z80Cpu.Instructions.cs` (despacho provisional) por cinco ficheros en `ZXSinclair.Net.Core/Z80/Generated/`:

| Fichero | Contenido |
|---|---|
| `Z80Cpu.Main.g.cs` | `ExecuteMain(byte opcode)` |
| `Z80Cpu.CB.g.cs` | `ExecuteCB(byte opcode)` |
| `Z80Cpu.ED.g.cs` | `ExecuteED(byte opcode)` |
| `Z80Cpu.Indexed.g.cs` | `ExecuteIndexedOpcode<TIndex>(byte opcode) where TIndex : struct, IIndexRegister` |
| `Z80Cpu.IndexedCB.g.cs` | `ExecuteIndexedCB<TIndex>(ushort address, byte opcode) where TIndex : struct, IIndexRegister` |

Las firmas son las actuales; el ciclo de prefijos (`Step`, `ExecuteIndexed`, `FinishIndexed`) sigue escrito a mano en `Z80Cpu.cs`.

### 4.2 Formato

Cada fichero:
1. Empieza con la cabecera GPL exacta de `CLAUDE.md` (nada antes).
2. Sigue con `// <auto-generated>` y una línea indicando que lo genera `ZXSinclair.Net.Generate.Z80OpCodes` y no se edita a mano.
3. `namespace ZXSinclair.Net.Core.Z80;` y `public sealed partial class Z80Cpu<TBus>` con un único método: un `switch` sobre `opcode` con los `case` ordenados por byte y `default: Unimplemented(); break;`.

La salida es **determinista**: mismo orden, sangría de 4 espacios, finales de línea `Environment.NewLine` (el repositorio no tiene `.gitattributes` y usa `core.autocrlf`, así que en git quedan como LF), UTF-8 sin BOM, sin fechas ni rutas absolutas. Regenerar sin cambios en las tablas o los patrones no produce diff.

### 4.3 Control de versiones

Los ficheros generados **se versionan** en git: el Core compila sin ejecutar el generador y los cambios de código emitido se revisan en el diff. No se usa un source generator de Roslyn porque ocultaría el código emitido a la revisión y complicaría depurarlo.

## 5. Ejecución

```bash
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes
```

| Opción | Comportamiento |
|---|---|
| (ninguna) | Genera los cinco ficheros en `ZXSinclair.Net.Core/Z80/Generated/` (ruta localizada subiendo desde el directorio actual hasta `zxsinclair.net.slnx`) y muestra el resumen de la sección 6. |
| `--output <dir>` | Escribe en otro directorio. |
| `--check` | Genera en memoria y compara con los ficheros existentes, normalizando los finales de línea; sale con código 1 y lista los ficheros distintos si no coinciden. No escribe nada. |

- El generador **no referencia el Core**: hoy tiene un `ProjectReference` a `ZXSinclair.Net.Core` que se elimina, para poder regenerar aunque el código generado no compile.
- Las configuraciones de VS Code existentes (`.vscode/launch.json`, `.vscode/tasks.json`) se ajustan a las opciones nuevas.

## 6. Cobertura

Al terminar, el generador muestra por tabla los opcodes implementados, pendientes, alias y huecos, por ejemplo:

```
base    : 1 implementados / 251 pendientes / 4 prefijos
cb      : 0 / 256
ed      : 0 / 78 pendientes / 178 huecos (EdHole pendiente)
ddfd    : 1 / …
ddfdcb  : 0 / 256
```

Con `--verbose` lista los opcodes pendientes por tabla. Las cifras alimentan la tabla de seguimiento de `spec-proceso-instrucciones.md`.

## 7. Rendimiento

- El `switch` sobre un `byte` con casos densos lo compila el JIT a una tabla de saltos; el generador emite siempre todos los `case` del mismo método en un único `switch`.
- `ExecuteIndexedOpcode<TIndex>` y `ExecuteIndexedCB<TIndex>` se instancian dos veces (IX e IY) al ser `TIndex` un struct: el tamaño de código se duplica a propósito.
- Métodos muy grandes pueden perder optimizaciones del JIT (límites de inlining y de registros) **(verificar con el benchmark al crecer los grupos)**. Si ocurre, la medida es mover cuerpos a auxiliares `AggressiveInlining`, no partir el `switch` en varios niveles.
- No se añaden comprobaciones, contadores ni trazas en el código generado salvo `Unimplemented()` en el `default`.

## 8. Pruebas

### 8.1 Proyecto de tests del generador

Nuevo proyecto xUnit `ZXSinclair.Net.Generate.Z80OpCodes.Tests` (añadido a `zxsinclair.net.slnx`), que referencia el generador; el parser, el modelo y los emisores se exponen como tipos `internal` con `InternalsVisibleTo`.

| Test | Comprobación |
|---|---|
| `Parser_ReadsAllTables` | Las cinco tablas cargan sin errores; base, CB y DDFDCB tienen 256 entradas |
| `Parser_ResolvesAliases` | ED `0x44`, `0x4c`…`0x7c` son `NEG`; DDFDCB `0x40`…`0x47` son `BIT 0,(REGISTER+dd)` |
| `Parser_MarksPrefixes` | `CB`, `DD`, `ED`, `FD` de base y `CB` de DDFD son prefijos |
| `Parser_TreatsSlttrapAsEdHole` | ED `0xFB` es hueco |
| `Parser_RejectsMalformedLines` (`[Theory]`) | Duplicado, alias huérfano, byte inválido y operando desconocido fallan con fichero y línea |
| `Indexed_AbsentOpcodesUseBaseBody` | DD/FD `0x00` emite el mismo cuerpo que base `0x00`; `0xEB` (`EX DE,HL`) igual que en base |
| `Patterns_AreDisjoint` | Ningún opcode encaja en dos patrones |
| `Output_StartsWithLicenseHeader` | Los cinco ficheros empiezan exactamente con la cabecera GPL |
| `Output_IsDeterministic` | Dos generaciones seguidas producen bytes idénticos |
| `Output_MatchesCommittedFiles` | Equivale a `--check`: la salida coincide con `ZXSinclair.Net.Core/Z80/Generated/` (detecta ediciones a mano y regeneraciones olvidadas) |

### 8.2 Piloto `NOP`

Con solo el patrón `NOP`:
- `ExecuteMain` y `ExecuteIndexedOpcode<TIndex>` contienen `case 0x00: break; // NOP`; el resto de métodos solo el `default`.
- `Z80Cpu.Instructions.cs` desaparece.
- Siguen en verde, sin cambios, `dotnet test ZXSinclair.Net.Core.Tests` (incluido `NopTests`) y el runner FUSE (3 casos pasados, 0 fallos).
- El benchmark `*Z80Cpu*` no empeora respecto a la última medición de spec CPU 8.4.

## 9. Criterios de aceptación

- Tablas CB y DDFDCB recuperadas e incrustadas; el generador no referencia el Core.
- `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes` genera los cinco ficheros con cabecera GPL, deterministas, y muestra la cobertura.
- `--check` devuelve 0 con los ficheros versionados y 1 tras editar uno a mano.
- Tests de 8.1 y piloto de 8.2 en verde.
- `CLAUDE.md`, `AGENTS.md`, spec CPU 5 y la tabla de seguimiento de `spec-proceso-instrucciones.md` actualizados (generador implementado, nuevo proyecto de tests, nuevas opciones).

## 10. Fuera de alcance

- La semántica de cada instrucción y sus patrones concretos (specs de grupo 1–11).
- Los auxiliares escritos a mano (`Z80Cpu.Alu.cs`…), que crea cada grupo.
- Desensamblador a partir de las mismas tablas (posible uso futuro del modelo de 3.1).
- Integración en CI (el test `Output_MatchesCommittedFiles` cubre la comprobación localmente).

## 11. Pendiente de verificar

- Licencia exacta de las tablas de FUSE (GPL v2 "o posterior").
- Si conviene añadir `.gitattributes` (`*.g.cs text eol=lf` o similar) para no depender de `core.autocrlf`.
- Efecto del tamaño de los métodos generados en el JIT y en WebAssembly AOT.
