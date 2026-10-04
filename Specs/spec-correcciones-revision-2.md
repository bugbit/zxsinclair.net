# Especificación: correcciones pendientes de la revisión del código de instrucciones

Segunda parte de las correcciones de la revisión de código del 2026-10-04 (la primera es `Specs/spec-correcciones-revision.md`). Cubre los tres hallazgos que quedaron fuera:

4. **Tablas de flags modificables desde fuera**: `Z80Flags.SZ53`, `SZ53P`, `Parity`, `Inc` y `Dec` son `public static readonly byte[]`, y cualquier código puede cambiar sus valores.
5. **Tabla de IM 0 frágil**: el generador decide qué opcodes son "de un byte" con una lista de tipos de operando excluidos, no con la longitud de la instrucción.
6. **Patrones sin comprobación del tipo de entrada**: algunos patrones no comprueban que la entrada sea una instrucción o un alias.

Ninguna cambia el comportamiento emulado: FUSE (1335/0/0) y los tests existentes deben seguir pasando sin cambiar sus expectativas, salvo los ajustes de tipo de 4.3.

Lo no confirmado se marca **(verificar)**.

## 4. Tablas de flags inmutables

### 4.1 Situación actual

`ZXSinclair.Net.Core/Z80/Z80Flags.cs` rellena en su constructor estático cinco arrays públicos de 256 bytes. Los usan los auxiliares de la CPU (`Z80Cpu.Alu8.cs`, `Rotate`, `Bits`, `Block`, `Control`, `Io`, `Load8`) y los tests (`Z80FlagsTests`, `Alu8Tests`). Una línea como `Z80Flags.SZ53[0] = 0`, desde un test, el futuro frontend o una herramienta de depuración, corrompería en silencio los flags de todas las CPU del proceso.

### 4.2 Cambio

- Los arrays pasan a ser **privados** (`private static readonly byte[] sz53`…), rellenados igual que ahora en el constructor estático.
- Se exponen como propiedades de solo lectura: `public static ReadOnlySpan<byte> SZ53 => sz53;` (lo mismo para `SZ53P`, `Parity`, `Inc` y `Dec`).
- Las constantes de bits (`C`, `N`, `PV`, `F3`, `H`, `F5`, `Z`, `S`) no cambian.
- El código del Core no cambia: el acceso por índice (`Z80Flags.SZ53[value]`) funciona igual sobre `ReadOnlySpan<byte>`.

### 4.3 Rendimiento y verificación del JIT

La tabla se lee en casi todas las instrucciones con flags, así que el cambio solo se acepta si no empeora:

1. Comprobar en el ensamblado tier 1 (`DOTNET_JitDisasm`, como en la spec anterior, sobre `Add8`/`ExecuteMainDispatch`) que el acceso con índice `byte` sigue sin comprobación de límites y sin cargas extra, frente al array actual.
2. Repetir `ExecuteAluLoopFrame`, `ExecuteMixFrame` y el resto de `Z80CpuBenchmarks` antes y después, en la misma sesión y con la máquina en reposo, repitiendo el control si los errores son altos.
3. Si hay regresión fuera del ruido, alternativa: dejar los arrays `internal` (con `InternalsVisibleTo` para `ZXSinclair.Net.Core.Tests`), que también impide modificarlos desde fuera del ensamblado y no cambia el acceso. Documentar la decisión y las mediciones en spec CPU 8.4.

### 4.4 Pruebas

- `Z80FlagsTests`: las comprobaciones de longitud y valores siguen igual (`ReadOnlySpan<byte>.Length` e índice). Ajustar solo lo que dependa del tipo `byte[]` (por ejemplo, `Assert.Equal` sobre el array entero: usar `.ToArray()` o comparar elemento a elemento).
- Nuevo `Z80FlagsTests.Tables_AreReadOnly`: comprobar por reflexión que `Z80Flags` no expone ningún campo público de tipo array (solo propiedades `ReadOnlySpan<byte>` y constantes). Si se elige la alternativa `internal`, comprobar que los campos no son públicos.
- `Alu8Tests` y el resto de tests del Core en verde sin otros cambios.

## 5. Longitud explícita de instrucción para la tabla IM 0

### 5.1 Situación actual

`Emit/Im0TableEmitter.cs` marca un opcode base como "de un byte" si está implementado, no es prefijo y ninguno de sus operandos es `Immediate8`, `Immediate16`, `RelativeOffset`, `AbsoluteMemory`, `PortImmediate` o `IndexedMemory`. La regla es implícita. Si un patrón futuro implementara un opcode con un auxiliar que lee bytes por PC sin uno de esos operandos, la tabla lo marcaría como de un byte. `AcceptInterrupt` lo ejecutaría en IM 0 consumiendo bytes de memoria como operandos y corrompería PC.

### 5.2 Cambio

- `Model/Opcode.cs`: nueva propiedad calculada `OperandBytes`, el número de bytes que la instrucción lee por PC después del opcode, según los operandos de la tabla:

  | Clase de operando | Bytes |
  |---|---|
  | `Immediate8`, `RelativeOffset`, `PortImmediate` | 1 |
  | `Immediate16`, `AbsoluteMemory` | 2 |
  | `IndexedMemory` en DD/FD | 1 (el desplazamiento `d`) |
  | `IndexedMemory` en DDFDCB | 0 (`d` lo lee el bucle de prefijos) |
  | Resto | 0 |

  Más `Length` = bytes de prefijo + 1 + `OperandBytes` (1 en la tabla base).
- `Im0TableEmitter.BuildBits`: un opcode entra en la tabla si está implementado, no es prefijo y `Length == 1`. Se elimina la lista de tipos excluidos.
- La tabla generada (`Z80Cpu.Im0.g.cs`) no debe cambiar: `--check` lo confirma.

### 5.3 Pruebas (`ZXSinclair.Net.Generate.Z80OpCodes.Tests`)

- `Opcode_LengthMatchesReference`: longitudes de una muestra representativa frente a valores conocidos del manual: `00` → 1, `06 nn` → 2, `01 nnnn` → 3, `18 e` → 2, `3A nnnn` → 3, `DB nn` → 2, `CD nnnn` → 3, `ED 43 nnnn` → 4, `DD 36 d n` → 4, `DD CB d 06` → 4.
- `Im0Table_UnchangedByLengthRule`: la tabla calculada con `Length == 1` es idéntica a la versión actual (los 32 bytes de `Z80Cpu.Im0.g.cs`).
- `SingleByteBodies_DoNotReadThroughPc`: para cada opcode base con `Length == 1`, el cuerpo emitido no contiene ninguna llamada a auxiliares que lean por PC (`ReadPc`, `ReadPc16`, `ReadPcDiscarded`, `ReadPc16Discarded`, `IndexedAddress`, `InAccumulator`, `OutAccumulator`, `StoreIndexedImmediate`, `JumpAbsolute`, `JumpRelative`, `CallAbsolute`, `DecrementJumpNonZero`, `LoadWordAbsolute`, `StoreWordAbsolute`). La lista vive en el test con un comentario que obliga a ampliarla cuando un auxiliar nuevo lea por PC. Así, un patrón que lea operandos sin declararlos rompe el test en lugar de colarse en la tabla IM 0.

## 6. Comprobación del tipo de entrada en todos los patrones

### 6.1 Situación actual

Casi todos los patrones exigen `opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias`. No lo hacen `ExchangeRegistersPattern` y `ExchangeStackPattern` (`Patterns/Block.cs`), los de E/S (`Patterns/Io.cs`), ni los que se apoyan solo en la tabla y el mnemónico (`Patterns/Rotate.cs`, `Patterns/Bits.cs`, `BlockLoadPattern`, `BlockComparePattern`). Hoy ninguna entrada `Prefix`, `Hole` o `Absent` tiene esos mnemónicos, así que no hay fallo. Pero `Patterns_AreDisjoint` solo detecta solapes entre dos patrones, no que un patrón reclame una entrada que no es una instrucción.

### 6.2 Cambio

- `Patterns/IPattern.cs` (o un fichero nuevo `Patterns/PatternGuards.cs`): método de extensión `internal static bool IsInstruction(this Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias;`.
- Todos los patrones, salvo `EdHolePattern` (que reconoce precisamente `Hole`), empiezan su `Matches` con `opcode.IsInstruction() && …`. Los que ya lo comprueban sustituyen su expresión por la llamada, para que haya una sola definición.
- Las entradas `Absent` de DD/FD no llegan nunca a los patrones con ese tipo: `DispatchEmitter` las sustituye antes por la entrada de la tabla base. El guard no cambia ese comportamiento.
- El código generado no cambia: `--check` lo confirma.

### 6.3 Pruebas (`ZXSinclair.Net.Generate.Z80OpCodes.Tests`)

- `Patterns_IgnoreNonInstructions`: para cada patrón de `PatternCatalog.Default` salvo `EdHolePattern`, y para cada opcode real de las cinco tablas, una copia con `Kind = Prefix`, `Hole` y `Absent` (mismo mnemónico y operandos) no casa.
- `EdHolePattern_OnlyMatchesHoles`: casa los huecos de ED y nada más.
- `Output_MatchesCommittedFiles` y `--check` en verde (sin cambios en los `.g.cs`).

## 7. Documentación

- `Specs/spec-cpu-z80.md` 3.1 (tablas de flags como `ReadOnlySpan<byte>`, o `internal` si se elige la alternativa) y 8.4 (mediciones de 4.3).
- `Specs/spec-generador-z80.md` 3.1 (`OperandBytes`/`Length` en el modelo), 3.2 (guard `IsInstruction` común) y la descripción de `Z80Cpu.Im0.g.cs` (regla `Length == 1`).
- `Specs/spec-instr-restos.md` 3.2: la tabla IM 0 se deriva de `Length == 1`.
- `CLAUDE.md`/`AGENTS.md`: mención de la spec y, si cambia, de la forma de `Z80Flags`.

## 8. Criterios de aceptación

- `Z80Flags` no expone arrays públicos; benchmarks y ensamblado sin regresión (o alternativa `internal` documentada).
- `Opcode.Length` en el modelo y tabla IM 0 derivada de `Length == 1`, idéntica a la actual.
- Guard `IsInstruction` en todos los patrones salvo `EdHolePattern`.
- Tests de 4.4, 5.3 y 6.3 en verde; `dotnet test` de los dos proyectos en verde; `--check` a 0 sin cambios en los `.g.cs`; compilación sin warnings; runner FUSE 1335/0/0.

## 9. Fuera de alcance

- Cambios de comportamiento emulado.
- Generar las tablas de flags como datos constantes en el código fuente (literales `ReadOnlySpan<byte>` en el binario): solo si 4.3 muestra que la propiedad sobre el array privado empeora y la alternativa `internal` no basta.
