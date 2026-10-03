# Especificación: grupo 11, restos y cierre

Grupo 11 de `Specs/spec-proceso-instrucciones.md`: lo que no encaja en los grupos anteriores y cierra la CPU.

1. **Huecos de la tabla ED**: los 178 opcodes `ED xx` sin instrucción (incluido el `slttrap` de FUSE en `ED FB`).
2. **IM 0 con instrucciones de un byte distintas de `RST`**: ejecución de la respuesta del bus en el reconocimiento de interrupción, con la temporización del manual.
3. **Peculiaridad NMOS de `LD A,I`/`LD A,R`**: P/V a 0 si se acepta una interrupción justo después (pendiente desde spec CPU 6.1).

Con este grupo, todas las instrucciones y huecos ED tienen `case`; los bytes de prefijo base/DD-FD los resuelve el bucle de CPU, y `UnimplementedOpcodes` solo puede incrementarse por respuestas de IM 0 de varios bytes o prefijos, que quedan fuera de alcance (2.2.4). Sigue la estructura de `Specs/spec-instr-io.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080):
  - p. 19, *CPU Response / Mode 0*: el dispositivo puede poner **cualquier** instrucción en el bus; "el número de ciclos necesarios para ejecutar esta instrucción es **dos más** que el normal", por los dos estados de espera que la CPU añade al ciclo de respuesta. Lo mismo para Mode 1.
  - p. 184, `IM 0`: "el primer byte de una instrucción de varios bytes se lee durante el ciclo de reconocimiento; **los bytes siguientes se leen mediante una secuencia normal de lectura de memoria**".
  - Descripción de `LD A,I`/`LD A,R`.
- Sean Young, *The Undocumented Z80 Documented*: huecos de ED ("NOP de 8 T"), y P/V de `LD A,I`/`LD A,R` interrumpido en el NMOS.
- `Specs/spec-cpu-z80.md` 4.2 (HALT), 4.3 (huecos de ED como dos NOP), 6.1, 6.3 y 10; `Specs/spec-buses-memoria.md` 9.2 y la tabla de reconocimiento de INT.
- `Specs/spec-generador-z80.md` 2.3 (patrón `EdHole`) y la regla del `default` de `Specs/spec-instr-bits.md` 4.4.
- FUSE: no hay casos de este grupo; la suite ya pasa entera (1335). Los tests propios son la única verificación.

## 2. Semántica

### 2.1 Huecos de ED

| Secuencia | Ciclos de bus | T | Efecto |
|---|---|---|---|
| `ED xx` sin instrucción (178 valores, incluido `ED FB`) | `pc:4, pc+1:4` | 8 | Ninguno: no toca registros, flags, WZ ni memoria. R `+= 2` por los dos M1 |
| `DD`/`FD` + `ED xx` hueco | `pc:4, pc+1:4, pc+2:4` | 12 | `ED` anula el prefijo índice (spec CPU 4.3); R `+= 3` |

- `Q = 0` (no escriben flags).
- Las interrupciones se aceptan después con normalidad (no son `EI`).

### 2.2 IM 0

#### 2.2.1 Temporización: reconocimiento de 6 T

El ciclo de reconocimiento es un M1 con IORQ y **dos estados de espera**: 4 + 2 = **6 T** (UM0080 p. 19). Sustituye al M1 de 4 T de la instrucción recibida, que por eso dura **su duración normal + 2 T**. Si la instrucción tiene un M1 extendido (5 o 6 T), la extensión son los ciclos internos `ir:1` que su cuerpo ya emite tras el M1, así que también suma exactamente + 2 T.

Hoy el contrato tiene un reconocimiento de **7 T** (`SpectrumMachine.InterruptAcknowledgeTStates`, `FuseTestBus`, `TestBus`), que solo cuadra con las respuestas cuyo M1 es de 5 T (IM 1, IM 2, `RST`). Cambio de contrato:
- `IZ80Bus.AcknowledgeInterrupt()` pasa a costar **6 T** (M1 + 2 TW) en `SpectrumBus` (`InterruptAcknowledgeTStates = 6`), `FuseTestBus` y `TestBus`.
- La CPU emite el T restante como ciclo interno `bus.Internal(Registers.IR, 1)` donde corresponde: IM 1 e IM 2 antes de apilar, y en IM 0 a través del propio cuerpo de la instrucción (por ejemplo, `Restart` ya hace `Internal(IR, 1)`).
- Totales resultantes: IM 1 = 6 + 1 + 6 = **13 T** e IM 2 = 6 + 1 + 6 + 6 = **19 T** (sin cambios); `RST p` en IM 0 = 11 + 2 = **13 T** (sin cambios).
- Diferencia con el modelo actual: el T interno pasa por `Internal`, que en el Spectrum aplica contención si `IR` cae en memoria contenida (`I` entre `0x40` y `0x7F`). Hoy el reconocimiento de 7 T es constante y no se contiende **(verificar con la Sinclair Wiki / hardware cuál de los dos es correcto; si el T no debe contenderse, `SpectrumBus` lo modela en `AcknowledgeInterrupt` con un parámetro de "M1 extendido" y se documenta)**.

Ejemplos (Spectrum, sin contención):

| Respuesta en IM 0 | Normal | En IM 0 |
|---|---|---|
| `NOP` | 4 | **6** |
| `LD B,A`, `INC A`, `EX DE,HL`, `DI`, `EI`, `HALT` | 4 | 6 |
| `INC BC`, `LD SP,HL` | 6 | 8 |
| `PUSH BC` | 11 | 13 |
| `RET Z` cumplida / no cumplida | 11 / 5 | 13 / 7 |
| `RST 38h` | 11 | 13 |

#### 2.2.2 Ejecución

Tras el reconocimiento (que ya incrementa R e `IFF1 = IFF2 = 0`), la CPU ejecuta como instrucción el byte devuelto, **sin** otro M1 y **sin** incrementar PC (el byte no sale de memoria). `RST p` deja de ser un caso especial: se ejecuta por el mismo camino que el resto de instrucciones de un byte (`Restart` apila PC, salta y pone `WZ = p`).

- En el Spectrum el bus devuelve `0xFF` (`RST 38h`), así que el caso habitual no cambia.
- `EI` recibido en IM 0 pone `EiPending`, como la instrucción normal.

#### 2.2.3 `HALT` recibido en IM 0

Representación del estado detenido (spec CPU 4.2 y 6.1): mientras `Halted`, `PC` apunta al opcode `HALT`, cada `Step()` hace un M1 en `PC` sin avanzarlo, y al aceptar una interrupción `ExitHalt` hace `PC += 1` antes de apilar, para volver a la instrucción siguiente.

Con `HALT` recibido en IM 0, `PC` apunta a la instrucción interrumpida `P` (no se ha leído ningún byte de memoria). Se aplica `Halt()` tal cual (`Halted = true; PC -= 1`):
- tras la respuesta, `PC = P - 1` (con vuelta a 64K) y `Halted = true`; la respuesta dura 6 T;
- cada `Step()` siguiente hace un M1 de 4 T en `P - 1` con R `+= 1`, como cualquier HALT;
- al aceptar la siguiente interrupción, `ExitHalt` restaura `PC = P`, que es la dirección que se apila: la ejecución vuelve a la instrucción interrumpida, que no llegó a ejecutarse.
- La dirección de los M1 de espera (`P - 1`) es consecuencia de la representación; qué dirección pone el hardware en el bus en ese caso no está confirmado **(verificar)**. El efecto observable (PC, R, T-states y dirección de retorno) queda definido.

#### 2.2.4 Instrucciones de varios bytes y prefijos (fuera de alcance)

Según el manual (p. 184), el primer byte llega en el ciclo de reconocimiento y los siguientes se leen con **ciclos normales de lectura de memoria**. Que esos bytes los aporte el dispositivo y no la memoria (y cómo evoluciona PC durante esas lecturas) depende del hardware externo; este grupo no lo modela porque:
- el Spectrum nunca responde con instrucciones de varios bytes (devuelve `0xFF`);
- hacerlo bien exige definir quién conduce el bus en esas lecturas, algo que hoy no expresa `IZ80Bus` (las lecturas normales devuelven memoria).

Por eso las respuestas de varios bytes (`CALL nn`, `JP nn`, `LD r,n`…) y los prefijos (`CB`, `DD`, `ED`, `FD`) siguen contándose con `Unimplemented()`, tras el reconocimiento de 6 T y sin apilar ni saltar.

### 2.3 `LD A,I` / `LD A,R` interrumpidos

- En el Z80 NMOS, si se acepta una **INT** justo al terminar `LD A,I` o `LD A,R`, el P/V que quedó en F es 0 en lugar de `IFF2`.
- Se aplica al aceptar la INT, antes de apilar PC (el F que ve la rutina de servicio tiene P/V = 0).
- Con **NMI**: el comportamiento no está confirmado **(verificar)**; no se aplica.
- Si entre `LD A,I` y la INT hay otra instrucción, no se aplica.

## 3. Implementación

### 3.1 Huecos de ED en el generador

- Nuevo patrón `EdHolePattern` en `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Control.cs`: reconoce `OpcodeKind.Hole` de la tabla ED y emite cuerpo vacío; `WritesFlags = false` (el emisor añade `Registers.Q = 0;`). Los 178 huecos comparten un único `case` múltiple.
- Con los huecos, la tabla ED cubre los 256 valores: por la regla de `Specs/spec-instr-bits.md` 4.4, `DispatchEmitter` deja de emitir su `default` (sin CS0162). Base y DD/FD lo conservan (bytes de prefijo).
- `CoverageReport`: los huecos implementados dejan de contarse como pendientes **(comprobar el texto real antes de fijar aserciones)**.

### 3.2 Tabla IM 0 en el generador

El generador emite un fichero más, `ZXSinclair.Net.Core/Z80/Generated/Z80Cpu.Im0.g.cs`, con una tabla de 256 bits (`ReadOnlySpan<byte>` de 32 bytes) que marca los opcodes base **implementados de un byte**: sin operandos `nn`, `nnnn`, `offset`, `(nn)`, `(nnnn)` ni `(REGISTER+dd)`, y que no son prefijo. Incluye `RST p`. La tabla sale del modelo del generador, no se mantiene a mano.

### 3.3 Reconocimiento e IM 0 en el Core

- `SpectrumMachine.InterruptAcknowledgeTStates = 6`; `FuseTestBus.AcknowledgeInterrupt` y `TestBus.AcknowledgeInterrupt` suman 6 T.
- `Z80Cpu.Interrupts.cs`, `AcceptInterrupt`:
  - IM 1 e IM 2: `bus.Internal(Registers.IR, 1)` después del reconocimiento y antes de apilar; `WZ = PC` como ahora.
  - IM 0: si `IsIm0SingleByte(data)`, `ExecuteMain(data)` (PC sin tocar; WZ y `Q` los fija la instrucción; no se añade `WZ = PC` al final). Si no, `Unimplemented()`.
  - La rama especial de `RST` desaparece (la cubre `ExecuteMain` con `Restart`).
- `HALT` en IM 0 no necesita código propio (2.2.3): `ExecuteMain(0x76)` llama a `Halt()`.

### 3.4 P/V tras `LD A,I`/`LD A,R`

- `Z80Registers`: nuevo `[FieldOffset(34)] public bool SpecialLoadPending`.
- `LoadAFromSpecial` (`Z80Cpu.Load8.cs`) lo pone a `true`.
- `Step()`: se limpia en el mismo punto que `EiPending` (después de comprobar interrupciones y antes de ejecutar la instrucción siguiente). El coste es una escritura de un byte por instrucción: medir (3.5). Si hay regresión, colocar ambos campos contiguos y limpiarlos con una sola escritura de 16 bits.
- `AcceptInterrupt`: si `SpecialLoadPending`, `F &= ~PV` antes de apilar, y lo limpia. `AcceptNmi` solo lo limpia.
- `Reset()` y `FuseCpuState.Load` lo dejan a `false`.

### 3.5 Rendimiento

- Huecos de ED: `case` vacío, sin coste.
- IM 0 y reconocimiento: camino frío.
- `SpecialLoadPending`: una escritura más por instrucción en `Step()`. Repetir los benchmarks de spec CPU 8.4 antes y después y documentar la diferencia.

## 4. Pruebas

### 4.1 FUSE

Sin casos nuevos (FUSE no prueba interrupciones). El runner debe seguir en **1335 pasados, 0 fallos, 0 omitidos** (8 con convención).

### 4.2 Tests propios (xUnit)

Clase nueva `ZXSinclair.Net.Core.Tests/Z80/Instructions/RestTests.cs`, con `TestBus` y el estado inicial de `NopTests`. Todos los tests de interrupción comprueban T-states, accesos (`Ack`, `Internal`, `Read`/`Write`), PC, SP, WZ, R, IFF y `Q`.

| Test | Montaje | Comprobación |
|---|---|---|
| `EdHoles_AllAreTwoNops` (`[Theory]`, 178) | Cada `ED xx` hueco, incluido `ED FB` | Solo cambian PC (+2) y R (+2); dos `M1`; 8 T; `Q = 0`; `UnimplementedOpcodes == 0` |
| `EdHoles_WithIndexPrefix` (`[Theory]`) | `DD ED 00`, `FD ED 77`, `DD FD ED FB` | 12 / 16 T; R +3 / +4; IX/IY intactos |
| `EdHole_AllowsIntOnNextStep` | `ED 00` e INT con IM 1 | INT aceptada en el `Step()` siguiente |
| `Ack_IsSixTStates` | INT con IM 1 e IM 2 | Acceso `Ack` de 6 T seguido de `Internal(IR, 1)`; totales 13 y 19 T (sin cambios) |
| `Im0_TimingIsNormalPlusTwo` (`[Theory]`) | Respuestas `00`, `47`, `3C`, `EB`, `F3`, `03` (`INC BC`), `F9` (`LD SP,HL`), `C5` (`PUSH BC`), `C8` (`RET Z`) cumplida y no cumplida, `FF` | 6, 6, 6, 6, 6, 8, 8, 13, 13 / 7, 13 T; efecto de la instrucción; PC sin cambios salvo en las que saltan; `UnimplementedOpcodes == 0` |
| `Im0_ExtendedM1_InternalCyclesOnIr` (`[Theory]`) | `C5`, `03`, `C8` | Los `Internal` del cuerpo usan `IR` (con `I = 0x42`) y van tras el `Ack` |
| `Im0_Rst_ViaInstructionPath` (`[Theory]`, 8) | `C7`…`FF` | 13 T; `Ack`, `Internal(IR, 1)`, dos `Write`; `PC = WZ = p` |
| `Im0_Ei_SetsPending` | Respuesta `FB` | `IFF1 = IFF2 = 1`, `EiPending`; la siguiente INT no se acepta hasta completar otra instrucción |
| `Im0_Halt_StopsAndReturnsToInterrupted` | `PC = P = 0x8000`, IM 0 con respuesta `76`; después varios `Step()` y una INT con IM 1 | Tras la respuesta: 6 T, `Halted`, `PC = 0x7FFF`; cada `Step()`: M1 de 4 T en `0x7FFF`, R `+= 1`; al aceptar la INT se apila `0x8000` y `Halted = false` |
| `Im0_Halt_PcWrapsAtZero` | `P = 0x0000` | `PC = 0xFFFF` mientras está detenida; se apila `0x0000` |
| `Im0_MultiByteOrPrefix_Unsupported` (`[Theory]`) | `CD`, `C3`, `3E`, `CB`, `DD`, `ED`, `FD` | `UnimplementedOpcodes == 1`; 6 T; sin escrituras; PC, SP, WZ intactos; `LastUnimplementedAddress` según spec CPU |
| `Im0Table_MatchesSingleByteOpcodes` | Tabla generada frente a una lista de referencia en el test | Coinciden los 256 bits |
| `LdAI_InterruptedClearsParity` (`[Theory]`) | `ED 57` y `ED 5F` con `IFF2 = 1`, INT activa justo después, IM 1, y `PUSH AF` en `0x0038` | F apilado con P/V = 0; resto de F igual |
| `LdAI_NotInterruptedKeepsParity` | `LD A,I` + `NOP` + INT | P/V = 1 en el F apilado |
| `LdAI_NmiDoesNotClearParity` | `LD A,I` seguido de NMI | P/V = `IFF2` |
| `SpecialLoadPending_ClearedByNextInstructionAndReset` | `LD A,I`, `NOP`; `Reset()` | `false` tras el `NOP` y tras `Reset()` |

Además, un test de `SpectrumBus` (`SpectrumBusTests`): el reconocimiento cuesta 6 T y, con `I` en `0x40`–`0x7F`, el `Internal(IR, 1)` de IM 1 aplica la contención según la decisión que se tome en 2.2.1.

### 4.3 Tests existentes que hay que adaptar

Los huecos de ED dejan de ser "opcodes pendientes": los tests que los usaban para provocar `Unimplemented()` pasan a usar IM 0 con una respuesta de varios bytes (`CD`), la única vía que queda. El cambio del reconocimiento a 6 T + `Internal(IR, 1)` cambia la secuencia de accesos de los tests de interrupción.

| Test | Cambio |
|---|---|
| `Z80CpuTests.UnimplementedDispatchCountsFetches` | Pasa a `EdHoleDispatchCountsFetches`: mismos programas con huecos de ED, mismos ciclos y R, pero `UnimplementedOpcodes == 0` |
| `Z80CpuTests.LongAlternatingPrefixChainUsesConstantStackSpace` | Termina en un hueco de ED que ahora se ejecuta: `UnimplementedOpcodes == 0`; resto igual |
| `Z80CpuTests.IntRestartsPushPcAndClearBothFlipFlops` | Secuencia de accesos `Ack`, `Internal`, `Write`, `Write` (antes sin `Internal`); totales iguales |
| `Z80CpuTests.UnsupportedIm0DoesNotInventJumpOrStackWrites` | Quitar `00` (se ejecuta; cubierto por `Im0_TimingIsNormalPlusTwo`); mantener `CD`, `CB`, `DD`; 6 T en lugar de 7 |
| `Z80CpuTests.Im2ReadsBothVectorBytesIncludingAddressWrap`, `AcceptingInterruptExitsHaltAndPushesFollowingAddress` y demás tests con `Ack` | Ajustar la secuencia de accesos (nuevo `Internal(IR, 1)`); totales iguales |
| `NopTests.Nop_DoesNotCountAsUnimplemented` | Comprobar que ni NOP ni `ED 00` cuentan; el incremento del contador se prueba con IM 0 + `CD` |
| `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset` | Provocar el pendiente con IM 0 + `CD` en lugar de `ED 00`/`DD ED 00` |
| `SpectrumBusTests` y tests de máquina que cuenten T-states de interrupción | Reconocimiento de 6 T + `Internal(IR, 1)` |
| `GeneratorTests` | ED: 256 cubiertos, sin pendientes, sin `default`; nuevo fichero `Z80Cpu.Im0.g.cs` en `Output_MatchesCommittedFiles`, cabecera GPL y determinismo; `EdHolePattern` con `WritesFlags = false`; tabla IM 0 frente a lista de referencia |

## 5. Documentación

- `Specs/spec-cpu-z80.md`: 4.3 (huecos implementados), 6.1 y 6.3 (reconocimiento de 6 T + `Internal(IR, 1)`, IM 0, HALT en IM 0, P/V), 10 (quitar los pendientes resueltos), 8.1/8.4.
- `Specs/spec-buses-memoria.md`: reconocimiento de 6 T (tabla de 9.2 y lista de constantes de FUSE) y la decisión sobre la contención del T interno.
- `Specs/spec-generador-z80.md`: patrón `EdHole` y sexto fichero generado (`Z80Cpu.Im0.g.cs`).
- `Specs/spec-proceso-instrucciones.md`: grupo 11 Implementado y cierre del proceso (todas las tablas completas).
- `Specs/estado-instrucciones-z80.md`: nota de cierre (los huecos de ED no cuentan como entradas del manual).
- `CLAUDE.md`/`AGENTS.md`: CPU completa; `UnimplementedOpcodes` solo por IM 0 de varios bytes o prefijos.

## 6. Criterios de aceptación

- Reconocimiento de 6 T con `Internal(IR, 1)` en IM 1/IM 2, `EdHolePattern`, tabla y ejecución de IM 0 de un byte (incluidos `RST` y `HALT`), y peculiaridad de P/V implementados; `*.g.cs` regenerados (incluido `Z80Cpu.Im0.g.cs`), `--check` a 0 y compilación sin warnings.
- Tabla ED completa sin `default`; base y DD/FD con `default`.
- Runner FUSE en 1335 / 0 / 0.
- Tests de 4.2 en verde y los de 4.3 adaptados; `dotnet test` de los dos proyectos en verde.
- Benchmarks repetidos con el coste de `SpecialLoadPending` documentado y sin regresión fuera del ruido.
- Documentación de 5 actualizada.

## 7. Fuera de alcance

- IM 0 con respuestas de varios bytes o prefijos (2.2.4).
- Comportamiento CMOS y otras variantes del Z80.
- Integración de z80test / ZEXALL (ver `Docs/benchmarks-maquinas-reales.md`), recomendada como siguiente paso para contrastar los puntos **(verificar)** de todos los grupos.

## 8. Pendiente de verificar

- Contención del T interno del reconocimiento (`Internal(IR, 1)`) en el Spectrum con `I` en memoria contenida (2.2.1).
- Dirección de los M1 de espera tras un `HALT` recibido en IM 0 (2.2.3).
- P/V de `LD A,I`/`LD A,R` con NMI.
- Texto exacto del informe de cobertura con huecos implementados.


## 9. Decisiones y verificación (2026-10-03)

Se usa `Internal(IR, 1)` para la extensión del reconocimiento. [Sinclair Wiki, Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory) describe el ciclo ir:1 de RST/PUSH y la contención de direcciones contenidas en la ULA. Aplicarlo a ese T del reconocimiento es una inferencia; la fuente no ofrece un desglose de INT. Los tests verifican el modelo elegido, sin afirmar una medición en hardware. Se mantiene pendiente ese contraste, la dirección física de los M1 tras HALT en IM 0 y el caso NMI de P/V.

La temporización IM 0 de duración normal +2 también se contrasta con [redcode/Z80, sources/Z80.c](https://github.com/redcode/Z80/blob/master/sources/Z80.c), que reutiliza las instrucciones con ese incremento y separa las lecturas de datos en IM 0.

Core 2832 tests, generador 156; build sin warnings ni errores; --check a 0. FUSE 1335/0/0, con eventos y las 8 convenciones BIT (HL) existentes. No hay fixtures de interrupciones; los tests propios comprueban sus efectos y secuencias.

Se probó la limpieza conjunta de EiPending/SpecialLoadPending con un ushort superpuesto y Q desplazado; empeoró NOP y se descartó. Se conservan EiPending=32, Q=33 y SpecialLoadPending=34, con dos escrituras de byte. Las mediciones repetidas y el control aislado están en spec CPU 8.4.
