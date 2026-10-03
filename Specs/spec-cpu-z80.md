# Especificación: CPU base y CPU Z80

Especificación del núcleo de CPU de `ZXSinclair.Net.Core`: el contrato genérico de cualquier CPU y la CPU Z80 (registros, ciclo de ejecución, prefijos, interrupciones, HALT, reset) con todos los grupos de instrucciones implementados. Los cinco despachos y la tabla IM 0 los produce `ZXSinclair.Net.Generate.Z80OpCodes`; las respuestas IM 0 de varios bytes o prefijos quedan fuera de alcance.

Se apoya en `Specs/spec-buses-memoria.md`: la CPU solo habla con un bus `IZ80Bus` y no sabe nada de la ULA, la contención ni la memoria. El rendimiento es el requisito principal del proyecto (ver `README.md`).

## 1. Fuentes

- `Docs/z80cpu_um.pdf` — Zilog *Z80 CPU User Manual* (UM0080): registros, interrupciones, modos IM, temporización.
- Sean Young, *The Undocumented Z80 Documented*: flags F3/F5, MEMPTR, comportamiento tras reset, R, prefijos repetidos.
- FUSE: tests `ZXSinclair.Net.Fuse/data/tests.in` / `tests.expected` y tablas `opcodes_*.dat`.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): secuencia de ciclos de bus de cada instrucción (`pc:4, hl:3, ir:1 ×2…`).

Lo no confirmado se marca **(verificar)**.

## 2. Capas

| Tipo | Dónde | Papel |
|---|---|---|
| `ICpu` | `Abstractions/` | Contrato de cualquier CPU para la máquina y el frontend: `Reset()`, `Step()`, `Execute(int targetCycles)`. Se llama una vez por frame o por paso de depuración, así que aquí sí se admite una llamada por interfaz. |
| `Z80Registers` | `Z80/` | Struct con todos los registros y el estado interno del Z80. |
| `Z80Cpu<TBus>` | `Z80/` | `sealed partial class Z80Cpu<TBus> : ICpu where TBus : struct, IZ80Bus`. Bucle de ejecución, prefijos e interrupciones. Las instrucciones viven en ficheros `partial` generados. |
| `Z80Flags` | `Z80/` | Constantes de flags y tablas precalculadas. |

Reglas (sección 9 de la spec de buses):
- `TBus` es siempre un **struct** para que el JIT especialice `Z80Cpu<SpectrumBus>` y haga inline de cada acceso al bus. Nunca se guarda el bus como `IZ80Bus`.
- Sin clases base con métodos virtuales en el bucle, sin delegados ni tablas de `Action`, sin asignaciones por instrucción.
- La CPU no suma ciclos por su cuenta: **todo el tiempo pasa por el bus** (`FetchOpcode`, `Read`, `ReadDiscarded`, `Write`, `Internal`, `In`, `Out`, `AcknowledgeInterrupt`). Así la contención se aplica sola y los eventos de bus de los tests FUSE salen exactos.

## 3. Registros (`Z80Registers`)

`[StructLayout(LayoutKind.Explicit)]` con los pares de 16 bits superpuestos a sus mitades de 8 bits (little-endian: el byte bajo en el desplazamiento menor). .NET en x86/x64/ARM y WebAssembly es little-endian; se comprueba con un test.

| Par | Alto / bajo | Notas |
|---|---|---|
| AF | A / F | |
| BC, DE, HL | B/C, D/E, H/L | |
| AF', BC', DE', HL' | — | Juego alternativo; `EX AF,AF'` y `EXX` intercambian valores |
| IX, IY | IXH/IXL, IYH/IYL | Las mitades se usan en instrucciones no documentadas |
| SP, PC | — | |
| IR | I / R | R: los 7 bits bajos se incrementan en cada M1; el bit 7 solo cambia con `LD R,A` |
| WZ (MEMPTR) | — | Registro interno; afecta a F3/F5 de `BIT n,(HL)` y otros |

Estado interno, también en el struct:
- `IFF1`, `IFF2` (bool o byte), `IM` (0, 1, 2).
- `Halted`.
- `SpecialLoadPending`: la instrucción anterior fue `LD A,I` o `LD A,R`; una INT inmediata limpia P/V en el modelo NMOS. Se borra al comenzar la siguiente instrucción, aceptar INT/NMI o hacer reset.
- `EiPending`: la instrucción anterior fue `EI`; bloquea INT hasta completar la siguiente instrucción. Los prefijos DD/FD se procesan dentro del mismo paso y no necesitan este estado.
- `Q`: F escrito por la última instrucción que modifica flags, o 0 tras una que no los modifica. Implementado según `spec-instr-control.md`: el generador usa `IPattern.WritesFlags` y SCF/CCF leen Q antes de actualizarlo. INT, NMI y cada M1 de HALT lo ponen a 0. `POP AF` se trata provisionalmente como Q=0; queda su comprobación con z80ccf.

La CPU expone `ref Z80Registers Registers` (campo, no propiedad que copie) para tests, depurador y snapshots.

### 3.1 Flags

| Bit | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
|---|---|---|---|---|---|---|---|---|
| Flag | S | Z | F5 (Y) | H | F3 (X) | P/V | N | C |

Tablas precalculadas de 256 entradas en `Z80Flags` (como `mTablePV` y `mTableZS53` del código legado): `SZ53`, `SZ53P` (con paridad), `Parity`. Las instrucciones combinan tablas y operaciones de bits; nunca `Enum.HasFlag`.

## 4. Ciclo de ejecución

### 4.1 API

| Miembro | Comportamiento |
|---|---|
| `Z80Cpu(TBus bus)` | Guarda el bus en un campo mutable para evitar copias defensivas de structs genéricos; inicializa la CPU mediante `Reset()`. |
| `void Reset()` | Ver 7. Resetea la CPU, no el bus. |
| `void Step()` | Atiende interrupciones pendientes o ejecuta **una** instrucción completa (con todos sus prefijos). |
| `void Execute(int targetCycles)` | Repite `Step()` mientras `bus.Cycles < targetCycles`. Es el bucle de la máquina: un frame = `Execute(TStatesPerFrame)` + `EndFrame()`. |
| `void RequestNmi()` | Marca un NMI pendiente (flanco). |
| `bool Halted`, `ref Z80Registers Registers` | Estado observable. |

`Execute` es el único bucle caliente: debe quedar como un `while` con el `switch` de opcodes inlineado o en un método no virtual, sin llamadas por interfaz.

### 4.2 Un paso

```
si NMI pendiente                       → aceptar NMI (6.2)
si no, si INT activa (bus.IntActive) y IFF1 y no EiPending → aceptar INT (6.3)
si no:
    EiPending = SpecialLoadPending = false
    si Halted → M1 sobre PC sin avanzarlo (NOP interno), R++
    si no     → opcode = M1(PC++); ejecutar(opcode)
```

M1 = `bus.FetchOpcode(PC)` + incremento de los 7 bits bajos de R. El incremento de R lo hace la CPU (el bus no conoce R).

### 4.3 Prefijos

| Prefijo | Comportamiento |
|---|---|
| `CB` | Segundo M1; la tabla CB. |
| `ED` | Segundo M1; la tabla ED. Los 178 huecos sin instrucción están implementados como dos NOP (8 T), con Q=0. |
| `DD` / `FD` | M1 propio (4 T, R++). El siguiente opcode se ejecuta con IX/IY en lugar de HL (y IXH/IXL en lugar de H/L, `(IX+d)` en lugar de `(HL)`). Si el opcode no usa HL, se ejecuta como sin prefijo. Una cadena `DD DD …` o `DD FD …` vale solo por el último prefijo; INT no se acepta entre un prefijo y su instrucción. |
| `DD CB d op` / `FD CB d op` | `d` y `op` se leen con `Read` (no M1, R no se incrementa con ellos): `pc:4, pc+1:4, pc+2:3, pc+3:3, pc+3:1 ×2` y el acceso a `(IX+d)`/`(IY+d)`. `FinishIndexed` guarda `WZ = ii + d` antes del despacho CB indexado. |

Las instrucciones indexadas se generan **una sola vez** y se especializan para IX e IY con un parámetro genérico de tipo struct (`ExecuteIndexed<TIndex>() where TIndex : struct, IIndexRegister`), que el JIT convierte en dos copias sin coste en tiempo de ejecución. Nada de `ref` a un registro elegido en tiempo de ejecución ni de duplicar el código a mano.

Los prefijos repetidos se consumen mediante un bucle de espacio constante, conservando el último DD/FD y despachando después a la especialización IX/IY. PC puede dar la vuelta a 64K. Si toda la memoria contiene prefijos, no se completa una instrucción y `Step()` no retorna; el contrato no permite cortar una instrucción a mitad.

### 4.4 Temporización de las instrucciones

Cada instrucción emite exactamente la secuencia de ciclos de la tabla de la Sinclair Wiki, traducida a llamadas al bus:

| Notación | Llamada |
|---|---|
| `pc:4` (M1) | `bus.FetchOpcode(PC)` + R++ |
| `pc:3`, `hl:3`, `sp:3`, `nn:3` | `bus.Read(dir)` / `bus.Write(dir, v)` |
| `pc+1:3` de un salto condicional no tomado | `bus.ReadDiscarded(dir)` (spec de buses 9.2) |
| `ir:1 ×2`, `hl:1`, `pc+2:1 ×5` | `bus.Internal(dir, n)` con la dirección que la CPU deja en el bus |
| `IO` | `bus.In(puerto)` / `bus.Out(puerto, v)` |

Las tablas de la wiki y los eventos `MC` de los tests FUSE son la referencia; los ciclos internos usan **siempre** la dirección correcta, aunque en el bus sin contención no tenga efecto.

## 5. Contrato con el generador de instrucciones

- El generador produce ficheros `partial` de `Z80Cpu<TBus>` con métodos `ExecuteMain(byte)`, `ExecuteCB(byte)`, `ExecuteED(byte)`, `ExecuteIndexedOpcode<TIndex>(byte)`, `ExecuteIndexedCB<TIndex>(ushort address, byte opcode)`, cada uno un `switch` sobre el byte (el JIT lo compila a una tabla de saltos). `ExecuteIndexed<TIndex>()` y `FinishIndexed<TIndex>(byte)` pertenecen al ciclo de prefijos escrito a mano.
- Cada caso llama a métodos pequeños `[AggressiveInlining]` escritos a mano (ALU, rotaciones, `Push`/`Pop`…) o contiene el código directamente.
- Las cinco tablas de entrada son `data/opcodes_*.dat` (formato FUSE), incrustadas en `ZXSinclair.Net.Generate.Z80OpCodes`. El generador está implementado según `spec-generador-z80.md`, con NOP como patrón piloto; no referencia el Core.
- Los ficheros generados llevan la cabecera GPL y no se editan a mano.
- Los cinco despachos y la tabla de clasificación IM 0 se generan en `ZXSinclair.Net.Core/Z80/Generated/` (`Z80Cpu.Main.g.cs`, `Z80Cpu.CB.g.cs`, `Z80Cpu.ED.g.cs`, `Z80Cpu.Indexed.g.cs`, `Z80Cpu.IndexedCB.g.cs`). `00` (NOP) está implementado en `ExecuteMain` y `ExecuteIndexedOpcode<TIndex>` con cuerpo de patrón vacío y escritura Q=0. Las cargas de 8 y 16 bits, los saltos, llamadas y retornos, las ALU de 8 y 16 bits, el grupo de control, las rotaciones y desplazamientos, las operaciones de bit el intercambio y bloques y la E/S también están implementados según `spec-instr-carga-8.md`, `spec-instr-carga-16.md`, `spec-instr-saltos.md`, `spec-instr-alu-8.md`, `spec-instr-control.md`, `spec-instr-alu-16.md`, `spec-instr-rotaciones.md`, `spec-instr-bits.md`, `spec-instr-bloques.md` y `spec-instr-io.md`; el resto queda como "no implementado" (ver 8). El despacho principal y sus cuerpos auxiliares generados usan `AggressiveInlining`; sus casos implementados terminan con `return` para reducir el tamaño del IL.
- El archivo provisional se ha eliminado. Se regenera con `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes`; `-- --check` compara sin escribir y detecta diferencias con código 1. Cada opcode completo aún no implementado llama a `Unimplemented()`: incrementa `UnimplementedOpcodes` y no modifica registros ni emite más ciclos. Solo se han consumido los accesos de fetch/prefijos. `Reset()` limpia el contador.

## 6. Interrupciones

### 6.1 Reglas comunes

- Se comprueban al final de cada instrucción completa (nunca entre un prefijo y su instrucción, ni justo después de `EI`).
- Si la CPU está en HALT, aceptar la interrupción la saca de HALT y `PC` apunta a la instrucción siguiente a `HALT` antes de apilarlo.
- El ciclo de reconocimiento incrementa R (es un M1).
- Tras `LD A,I` / `LD A,R`, una INT inmediata limpia P/V antes de ejecutar la respuesta; `SpecialLoadPending` conserva ese estado hasta la comprobación de interrupciones. NMI conserva P/V según la decisión provisional de `spec-instr-restos.md` 2.3. FUSE no prueba interrupciones.

### 6.2 NMI

Flanco (`RequestNmi()`), prioridad sobre INT, ignora IFF1. M1 normal de 5 T con el dato ignorado, `IFF1 = 0` (IFF2 se conserva), apila PC y salta a `0x0066`: 11 T en total. `RETN` copia IFF2 en IFF1. El Spectrum no lo usa de serie (sí interfaces como el Multiface).

### 6.3 INT

Nivel (`bus.IntActive`); se acepta si `IFF1` y no `EiPending`. `IFF1 = IFF2 = 0`. Reconocimiento con `bus.AcknowledgeInterrupt()` (6 T en el Spectrum, devuelve el byte del bus) y después:

| Modo | Acción | T-states en el Spectrum |
|---|---|---|
| IM 0 | Ejecuta la instrucción del bus; en el Spectrum es 0xFF = `RST 38h` | Duración normal + 2; RST = 13 |
| IM 1 | Apila PC (`sp-1:3`, `sp-2:3`) y salta a `0x0038` | 6 + 1 + 6 = 13 |
| IM 2 | Apila PC, lee el vector en `(I << 8) | byte del bus` (`2 × 3`) y salta | 6 + 1 + 6 + 6 = 19 |

IM 0 ejecuta los opcodes base de un byte mediante `ExecuteMain`, sin fetch adicional ni avance de PC. La tabla de 256 bits `Z80Cpu.Im0.g.cs` se genera desde los operandos del modelo; incluye RST, HALT y EI. WZ y Q los fija la instrucción. Los opcodes de varios bytes y los prefijos incrementan `UnimplementedOpcodes` tras el reconocimiento, sin cambiar PC, SP o WZ (salvo la salida previa de HALT).

En IM 1 e IM 2 se emite `Internal(IR, 1)` tras el reconocimiento y antes de apilar. En IM 0 lo emite el cuerpo cuando corresponde: RST/PUSH/RET cc tienen uno; INC ss/LD SP,HL tienen dos. La decisión de contención y sus límites se documentan en la spec de restos 2.2.1.

HALT recibido por IM 0 deja PC=P-1 en la representación actual; los M1 de espera usan P-1. Al aceptar otra interrupción, `ExitHalt` restaura P, la dirección interrumpida que se apila. Los tests incluyen P=0000. La dirección física de estos M1 sigue pendiente de contraste con hardware.

## 7. Reset

`Reset()` deja: `PC = 0`, `I = R = 0`, `IFF1 = IFF2 = 0`, `IM = 0`, `Halted = false`, `EiPending = SpecialLoadPending = false`, `AF = SP = 0xFFFF` (valor del hardware real según Young) y el resto de registros a 0 **(verificar valores no garantizados)**. No resetea el bus ni la máquina.

## 8. Pruebas

### 8.1 Tests FUSE (`ZXSinclair.Net.Test`)

- El runner usa un bus de pruebas, `FuseTestBus : struct, IZ80Bus` (en la librería `ZXSinclair.Net.Fuse`, compartida con los tests xUnit junto con el parser `FuseTestFile`, `FuseCpuState` y `FuseComparison`): memoria plana de 64K, contador de T-states, sin contención real, que **registra los eventos de bus** con la semántica de FUSE: `MC` al empezar cada ciclo de memoria y `MR`/`MW` al terminarlo; un `MC` por T-state en `Internal`; `PC`/`PR`/`PW` siguiendo la tabla de contención de E/S del 48K (byte alto 0x40–0x7F). Así la CPU del Core no necesita instrumentación ni `#if` de test.
- Se ejecuta `Z80Cpu<FuseTestBus>` hasta `end_tstates` y se comparan registros (incluidos AF', BC', DE', HL'), `I`, `R`, `IFF1`, `IFF2`, `IM`, `halted`, T-states, memoria y la secuencia completa de eventos.
- Un opcode no implementado incrementa `UnimplementedOpcodes` y guarda el PC actual en `LastUnimplementedAddress`. Esta propiedad solo se escribe en `Unimplemented()` y se limpia en `Reset()`; el caso se cuenta como omitido.
- El runner está conectado a `Z80Cpu<FuseTestBus>`: en Debug o Release carga y ejecuta los 1335 casos, actualmente 1335 pasan (3 de NOP, 163 del grupo de carga de 8 bits, 35 del de carga de 16 bits, 80 de saltos, llamadas y retornos, 148 de ALU de 8 bits, 27 de control, 32 de ALU de 16 bits, 198 de rotaciones y desplazamientos, 584 de bits, 14 de intercambio y bloques y 51 de E/S), 0 fallan y 0 se omiten por instrucciones pendientes, con comparación de eventos activada. El caso `10` de DJNZ también ejecuta `INC C` y ya pasa. `--no-events` desactiva el registro y comparación de eventos. Devuelve código 1 si hay fallos. Un fichero FUSE mal formado lanza `FormatException` con el nombre del test.
- `FuseConventions` declara los ocho casos `BIT b,(HL)` que ignoran solo F5/F3 de F (máscara 0x28): los fixtures usan el valor leído y la CPU usa MEMPTR. El runner informa de las diferencias ignoradas (cuatro en estos fixtures), muestra F esperado y real con `--verbose` y cuenta `pasados con convención: 8`. Los tests propios verifican F5/F3 de WZ y que la convención no afecte al resto de datos. Core: 2832 tests pasados; generador: 156; build: 0 warnings y 0 errores; `--check`: 0 (2026-10-03).
- Tests xUnit contrastan directamente los ciclos del bus con los fixtures `00`, `ddcb00`, `d3*` y `db*`, y verifican la detección de discrepancias en registros, memoria, ciclos y eventos aun cuando el runner omite instrucciones.

- `FuseReport` compara siempre registros, flags, memoria y T-states; también compara los eventos cuando están activados. Muestra el nombre, cuatro bytes desde el PC inicial con vuelta a 64K, las dos líneas del estado inicial y todas las diferencias por sección. AF/AF' se separan en A/F; F se decodifica como `S Z 5 H 3 P/V N C`, y R distingue su bit 7 de los bits bajos. La memoria se agrupa en rangos contiguos (16 por defecto, configurables en `DiffMemory`/`FuseReport`), con el número de rangos omitidos.
- Para eventos muestra la primera divergencia, los totales y tres filas anteriores y posteriores alineadas por índice. La pista es una posible causa, no una clasificación definitiva. `Compare*` conserva sus firmas como envoltorios de la primera diferencia.

Opciones del runner:

| Opción | Efecto |
|---|---|
| `--filter <prefijo>` | Selecciona nombres que empiezan por ese prefijo, sin distinguir mayúsculas. `--filter 20_2` selecciona ese caso; `--filter dd` selecciona 343 casos. |
| `--verbose` | Añade todos los eventos esperados y reales de los fallos con detalle, y los valores de F y F5/F3 en los casos con convención. |
| `--max-failures <n>` | Detalle de los primeros n fallos; los demás siguen contando y se muestran en una línea. Por defecto 10; 0 muestra solo líneas breves. |
| `--list-skipped` | Muestra el último opcode pendiente y cuatro bytes de contexto anteriores al PC guardado, incluidos los posibles prefijos. El contexto puede contener bytes anteriores a la instrucción y procede de la memoria al terminar el caso. |
| `--no-events` | Desactiva únicamente el registro y la comparación de eventos. |

El resumen conserva pasados/fallidos/omitidos y añade el número de casos fallidos por sección y por primer byte del programa (los prefijos DD/FD/ED/CB agrupan sus casos). Un caso puede contar en varias secciones. Código de salida 0 sin fallos, 1 con fallos, 2 ante argumentos inválidos. Ante un fallo, ejecutar `dotnet run --project ZXSinclair.Net.Test -- --filter <caso> --verbose`, adjuntar el informe y clasificar su causa antes de modificar CPU o spec.

### 8.2 Tests xUnit (`ZXSinclair.Net.Core.Tests`)


El núcleo de CPU se prueba con un bus de prueba:
- Alias de registros (endianness) y juego alternativo.
- R: 7 bits por M1, bit 7 intacto, +1 por cada prefijo, +1 en el reconocimiento de interrupción.
- `Reset()`.
- Interrupciones: IM1 = 13 T a `0x0038`, IM2 = 19 T con el vector correcto, NMI = 11 T a `0x0066` con IFF2 conservado, no aceptar INT con `EiPending` ni con IFF1 = 0, salida de HALT.
- `Execute(target)` se detiene en la primera instrucción que alcanza o supera `target`.

### 8.3 Tests propios de cada instrucción (no FUSE)

Pasar los casos FUSE no basta: cada instrucción que se implemente lleva además tests xUnit propios en `ZXSinclair.Net.Core.Tests`. FUSE cubre un único estado inicial por caso y no comprueba casos límite ni la interacción con el resto de la CPU; estos tests sí.

- **Ubicación**: una clase por grupo de instrucciones en `ZXSinclair.Net.Core.Tests/Z80/Instructions/` (`NopTests`, `LdRegRegTests`, `AluTests`…), con el `TestBus` existente, que registra cada acceso (`M1`, `Read`, `Write`, `Internal`, `Ack`) y sus T-states.
- **Nombres**: `Instrucción_Condición_Resultado` (`Nop_AtFFFF_WrapsPcToZero`). Las variantes que solo cambian datos se agrupan en `[Theory]` con `[InlineData]`.
- **Qué cubren**, como mínimo, para cada instrucción:
  1. **Ciclos de bus**: la secuencia exacta de accesos (tipo, dirección, valor) y el total de T-states según la tabla de la Sinclair Wiki (4.4), incluidas las direcciones de los ciclos `Internal`.
  2. **Efecto**: registros y memoria que cambian, y que **ningún otro** cambia (comparar el struct `Z80Registers` completo con el esperado, incluidos el juego alternativo, WZ e IFF/IM).
  3. **Flags**: cada flag afectado, incluidos F3/F5 y casos límite (cero, signo, acarreo, desbordamiento, medio acarreo, paridad), y que los no afectados se conservan.
  4. **R**: incremento de los 7 bits bajos por cada M1 y bit 7 intacto (empezar con `R = 0x7F` o `0xFF`).
  5. **Vuelta de 64K**: PC, SP, operandos y `(IX+d)` que cruzan `0xFFFF`.
  6. **Prefijos**: variantes `DD`/`FD` (con IX/IY y sus mitades) y el caso en que el prefijo no afecta a la instrucción.
  7. **Contador**: `UnimplementedOpcodes` no cambia.
  8. **Interrupciones**: que una INT/NMI pendiente se acepta justo después (salvo `EI` y prefijos), y comportamiento especial si lo hay (`HALT`, `RETN`, `LD A,I`…).
- Las especificaciones de cada instrucción (`Specs/spec-instr-*.md`) enumeran sus casos concretos; los tests de 8.2 que dependían de que un opcode fuese "no implementado" se adaptan al implementarlo, usando otro opcode aún no implementado.

### 8.4 Rendimiento

Benchmark en `ZXSinclair.Net.Benchmarks` del bucle `Execute` con `Z80Cpu<SpectrumBus>` (cuando haya instrucciones: un programa de prueba con mezcla típica). Objetivo orientativo: muy por encima de tiempo real en escritorio (el Spectrum necesita ~3.5 millones de T-states por segundo) y al menos tiempo real holgado en Blazor WebAssembly **(fijar cifras al medir)**.

Medición del 2026-10-02: `Z80CpuBenchmarks.ExecuteFrame`, BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11, SDK 10.0.401, .NET 10.0.12 x64 RyuJIT, Release:

| Media por opcode | Error (IC 99.9%) | Desviación estándar | Asignaciones |
|---|---|---|---|
| 2.113 ns | 0.0105 ns | 0.0082 ns | 0 B |

Cada invocación fija PC=0, ejecuta 69888 T-states y llama a `EndFrame()`: 17472 NOP (`00`), normalizados con `OperationsPerInvoke`. Las direcciones contenidas se alcanzan tras el intervalo de pantalla, por lo que este recorrido no añade esperas. Incluye fetch, despacho de NOP y comprobación de interrupciones. Devuelve PC para conservar un resultado dependiente de la ejecución. No mide una mezcla de instrucciones ni WebAssembly. La base del despacho sin instrucciones era 3.844 ns/opcode y 0 B (2026-10-02).

Reproducir: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*'`. Informe de esta ejecución en `ZXSinclair.Net.Benchmarks/bin/nop-benchmark-artifacts/results/ZXSinclair.Net.Benchmarks.Z80CpuBenchmarks-report-github.md` (artefacto local excluido de git).


### Comparación tras implementar el generador (2026-10-02)

Mismo benchmark, hardware, SDK y runtime indicados arriba, Release. Se repitió la medición del despacho generado y se midió el despacho anterior en una copia aislada bajo `bin/`, restaurando desde HEAD su archivo provisional y conservando el mismo benchmark.

| Despacho | Media por opcode | Error (IC 99.9%) | Desviación estándar | Asignaciones |
|---|---|---|---|---|
| Generado, repetición | 3.782 ns | 0.0029 ns | 0.0027 ns | 0 B |
| Anterior, control actual | 3.783 ns | 0.0021 ns | 0.0017 ns | 0 B |

Los intervalos se solapan; esta comparación no muestra una regresión del despacho generado. La primera ejecución generada dio 3.770 ns (error 0.0626 ns, desviación 0.0586 ns, 0 B). La cifra histórica de 2.113 ns no se reprodujo tampoco con el despacho anterior; la causa de la diferencia entre ejecuciones no está determinada. Se conserva la medición histórica y se usa el control actual para evaluar este cambio.

Informes locales, excluidos de git: `ZXSinclair.Net.Benchmarks/bin/generator-repeat-benchmark-artifacts/results/ZXSinclair.Net.Benchmarks.Z80CpuBenchmarks-report-github.md` y `ZXSinclair.Net.Benchmarks/bin/generator-baseline-control-artifacts/results/ZXSinclair.Net.Benchmarks.Z80CpuBenchmarks-report-github.md`.

### Carga de 8 bits y tamaño del despacho (2026-10-02)

Mismo benchmark, hardware, SDK y runtime anteriores, Release. Control aislado con los tres despachos de HEAD (solo NOP), conservando el resto del Core y el benchmark. Proyecto temporal bajo `ZXSinclair.Net.Benchmarks/bin/load8-control/`, con nombre `Load8Baseline` para evitar proyectos duplicados en BenchmarkDotNet.

| Despacho | Media por opcode | Error (IC 99.9%) | Desviación estándar | Asignaciones |
|---|---|---|---|---|
| Anterior, control actual | 3.768 ns | 0.0433 ns | 0.0384 ns | 0 B |
| Carga de 8 bits, final | 3.774 ns | 0.0435 ns | 0.0407 ns | 0 B |

Los intervalos se solapan; esta comparación no muestra una regresión de NOP. La emisión directa inicial dio 4.873 ns y 0 B. Se redujo el IL del despacho principal mediante cuerpos auxiliares generados con registros concretos, `AggressiveInlining` y retornos directos en los casos. Se mantiene un único switch por tabla, sin selección dinámica de registros. Esta medida solo cubre NOP en escritorio; la mezcla de instrucciones y WebAssembly siguen pendientes.

Informes locales excluidos de git: `ZXSinclair.Net.Benchmarks/bin/load8-control-artifacts/results/ZXSinclair.Net.Benchmarks.Z80CpuBenchmarks-report-github.md` y `ZXSinclair.Net.Benchmarks/bin/load8-returns-artifacts/results/ZXSinclair.Net.Benchmarks.Z80CpuBenchmarks-report-github.md`.

### Carga de 16 bits y salida temprana de NOP (2026-10-02)

Mismo benchmark, hardware, SDK y runtime anteriores, Release. La base se midió en este checkout antes de añadir el grupo 2.

| Despacho | Media por opcode | Error (IC 99.9%) | Desviación estándar | Asignaciones |
|---|---|---|---|---|
| Carga de 8 bits, control previo | 3.765 ns | 0.0742 ns | 0.0793 ns | 0 B |
| Carga de 16 bits, emisión inicial | 4.026 ns | 0.0017 ns | 0.0014 ns | 0 B |
| Carga de 16 bits, salida temprana de NOP | 3.769 ns | 0.0724 ns | 0.0833 ns | 0 B |

La emisión inicial mostró una regresión de NOP. El generador ahora emite una salida temprana para `00` antes del único switch base, solo si su cuerpo está implementado y vacío. Los intervalos del control y de la versión final se solapan; no se observa una regresión en esta medición. Se conservan los auxiliares por opcode, los registros concretos y los despachos de las demás tablas. Esta medida cubre NOP en escritorio; no mide la mezcla de instrucciones ni WebAssembly.

Artefactos locales excluidos de git: `ZXSinclair.Net.Benchmarks/bin/load16-initial-benchmark-artifacts/` conserva los logs del control previo y de la emisión inicial; `ZXSinclair.Net.Benchmarks/bin/load16-nop-guard-artifacts/` conserva el informe final.

### Saltos, llamadas y retornos (2026-10-02)

BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11 (10.0.26300.9550), SDK 10.0.401, .NET 10.0.12 x64 RyuJIT, Release. Comparación con afinidad fija al procesador lógico 0 (`--affinity 1`), 6 iteraciones de calentamiento y 15 de medición. El control aislado en `ZXSinclair.Net.Benchmarks/bin/jumps-control/` usa los cinco despachos anteriores de HEAD, conservando el Core y el benchmark actuales.

| Despacho | NOP, ns/opcode | Error (IC 99.9%) | Desviación estándar | Asignaciones |
|---|---|---|---|---|
| Carga de 16 bits, control actual | 2.232 | 0.0159 | 0.0141 | 0 B |
| Saltos, emisión inicial | 2.7293 | 0.0085 | 0.0075 | 0 B |
| Saltos, entrada pequeña e inlining del switch | 2.1545 | 0.0248 | 0.0220 | 0 B |

La emisión inicial produjo una regresión de NOP. El generador ahora separa la entrada pequeña `ExecuteMain`, con la salida temprana de NOP, del único switch `ExecuteMainDispatch`; ambos usan `AggressiveInlining`. La versión final mejora respecto al control actual. Al dejar el switch sin `AggressiveInlining`, NOP dio 2.1371 ns y el bucle 0.6883 ns/T-state; se conserva el atributo para reducir el coste del bucle.

| ExecuteLoopFrame | Media, ns/T-state | Error (IC 99.9%) | Desviación estándar | Asignaciones |
|---|---|---|---|---|
| Emisión inicial | 0.6451 | 0.0033 | 0.0030 | 0 B |
| Entrada pequeña e inlining del switch | 0.6487 | 0.0059 | 0.0049 | 0 B |

Los intervalos del bucle se solapan. La media final equivale a unos 45.34 µs por frame de 69888 T-states y 440 veces tiempo real a 3.5 MHz. Cada invocación fija PC=0x8000, SP=0xFF00 e IFF1=false, ejecuta un frame y llama a `EndFrame()`. El programa está en RAM no contenida y combina cargas, PUSH/POP, CALL/RET, DJNZ y JR; no alcanza opcodes pendientes. La ejecución puede superar el límite del frame en una instrucción; `EndFrame()` conserva ese exceso para el siguiente frame. Es una carga sintética de escritorio; no mide el arranque de la ROM ni WebAssembly.

La primera ejecución sin afinidad fija dio NOP 4.340 ns (error 0.3022, desviación 0.8572) y bucle 1.018 ns/T-state (error 0.0659, desviación 0.1934), ambos con 0 B y dos bandas claras de tiempos. Su variabilidad motivó la comparación con afinidad fija; la causa de las bandas no se ha confirmado.

Reproducir la versión final: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Informes locales excluidos de git en `ZXSinclair.Net.Benchmarks/bin/jumps-control-artifacts/`, `jumps-affinity-artifacts/`, `jumps-wrapper-inline-artifacts/`; la primera ejecución y la variante sin inlining se conservan en `jumps-benchmark-artifacts/` y `jumps-wrapper-artifacts/`.

### Informe FUSE y registro del opcode pendiente (2026-10-02)

Mismo entorno y parámetros de la medición anterior: Release, afinidad 1, 6 iteraciones de calentamiento y 15 de medición. El benchmark se conserva sin cambios.

| Método | Antes | Con informe FUSE | Error (IC 99.9%) | Desviación estándar | Asignaciones |
|---|---|---|---|---|---|
| ExecuteFrame, ns/opcode | 2.1545 | 2.1597 | 0.0149 | 0.0132 | 0 B |
| ExecuteLoopFrame, ns/T-state | 0.6487 | 0.6537 | 0.0035 | 0.0030 | 0 B |

Los intervalos se solapan en ambos métodos; no se observa una regresión en esta medición. El informe se compone fuera de la ejecución de instrucciones. `LastUnimplementedAddress` solo se actualiza al registrar un opcode pendiente y en reset. Informe local excluido de git: `ZXSinclair.Net.Benchmarks/bin/fuse-report-benchmark-artifacts/results/ZXSinclair.Net.Benchmarks.Z80CpuBenchmarks-report-github.md`.


### ALU de 8 bits (2026-10-02)

BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11 (10.0.26300.9550), SDK 10.0.401, .NET 10.0.12 x64 RyuJIT, Release. Afinidad al procesador lógico 0 (`--affinity 1`), 6 iteraciones de calentamiento y 15 de medición.

| Variante | Método | Media | Error (IC 99.9%) | Desviación estándar | Asignaciones |
|---|---|---|---|---|---|
| Métodos separados (elegida) | ExecuteFrame | 2.1293 ns/opcode | 0.0038 | 0.0030 | 0 B |
| Métodos separados (elegida) | ExecuteLoopFrame | 0.6505 ns/T-state | 0.0021 | 0.0019 | 0 B |
| Métodos separados (elegida) | ExecuteAluLoopFrame | 0.6627 ns/T-state | 0.0056 | 0.0046 | 0 B |
| Núcleos con acarreo compartidos | ExecuteFrame | 2.1325 ns/opcode | 0.0099 | 0.0088 | 0 B |
| Núcleos con acarreo compartidos | ExecuteLoopFrame | 0.6508 ns/T-state | 0.0074 | 0.0065 | 0 B |
| Núcleos con acarreo compartidos | ExecuteAluLoopFrame | 0.6736 ns/T-state | 0.0045 | 0.0037 | 0 B |

Se conservan los métodos separados: su media en el bucle ALU es un 1.6% menor en esta comparación. La fórmula de flags permanece sin ramas y los auxiliares llevan `AggressiveInlining`. Frente al control anterior del informe FUSE (NOP 2.1597 ns/opcode, bucle 0.6537 ns/T-state), NOP mejora y los intervalos del bucle se solapan; esta medición no muestra regresión de los benchmarks anteriores.

| Método final | ns/T-state | × tiempo real a 3.5 MHz |
|---|---|---|
| ExecuteFrame (NOP: 4 T/opcode) | 0.532325 | 537 |
| ExecuteLoopFrame | 0.6505 | 439 |
| ExecuteAluLoopFrame | 0.6627 | 431 |

El bucle ALU usa una máquina separada para conservar el programa del benchmark anterior. Se ejecuta en RAM no contenida desde 0x8000: ADD A,C, ADC A,n, SUB (HL), SBC A,B, AND D, XOR E, CP H, INC C, DEC (HL), ADD A,(IX+5), DJNZ y JR; HL=0x9000, IX=0x9100. Cada invocación fija PC, AF, C, DE e IFF1, ejecuta 69888 T-states y conserva el exceso de la última instrucción mediante `EndFrame()`. `GlobalCleanup` comprueba que no se ejecutaron opcodes pendientes. La media ALU equivale a 46.31 µs por frame. Son cargas sintéticas de escritorio; WebAssembly y z80test siguen pendientes.

La primera comparación validaba el bucle ALU en `GlobalSetup` y dio 0.7597 ns/T-state con métodos separados y 0.7137 con núcleos compartidos. Se trasladó esa validación al final para evitar introducir instrucciones ALU en el calentamiento de los otros benchmarks y se repitieron ambas variantes con el mismo código del benchmark. La elección se basa en esta comparación final.

Reproducir: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Informes locales excluidos de git: `ZXSinclair.Net.Benchmarks/bin/alu-separate-final-artifacts/` y `alu-shared-final-artifacts/`; las mediciones iniciales se conservan en `alu-separate-artifacts/` y `alu-shared-artifacts/`.


### Control y registro Q (2026-10-02)

BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11 (10.0.26300.9550), SDK 10.0.401, .NET 10.0.12 x64 RyuJIT, Release; afinidad 1, 6 iteraciones de calentamiento y 15 de medición. Control medido en este checkout antes de editar Q, con el mismo benchmark.

| Método | Antes de Q | Con Q generado | Error final (IC 99.9%) | Desviación estándar final | Asignaciones |
|---|---|---|---|---|---|
| ExecuteFrame, ns/opcode | 2.1257 | 2.1234 | 0.0030 | 0.0023 | 0 B |
| ExecuteLoopFrame, ns/T-state | 0.6532 | 0.6585 | 0.0026 | 0.0021 | 0 B |
| ExecuteAluLoopFrame, ns/T-state | 0.6821 | 0.6905 | 0.0103 | 0.0086 | 0 B |

El control tiene errores 0.0095 / 0.0054 / 0.0079 y desviaciones 0.0080 / 0.0045 / 0.0066, respectivamente. Los intervalos se solapan en los tres métodos. Las medias cambian -0.11%, +0.81% y +1.23%; esta comparación no distingue ese coste del ruido de la medición. Se conserva Q al final del cuerpo generado y la salida temprana de NOP; no se activó la alternativa de mover Q dentro de los auxiliares, prevista solo si aparecía una regresión superior al ruido.

NOP equivale a 0.53085 ns/T-state (538× tiempo real a 3.5 MHz), el bucle anterior a 434× y el ALU a 414×. Todas las ejecuciones tienen 0 B asignados. Las instrucciones de control no aparecen en estas cargas: se mide el cambio del despacho y la escritura de Q, sin atribuir estos resultados al coste de DAA. DAA conserva la fórmula; no se justifica comparar una tabla en estas cargas. No mide WebAssembly.

Reproducir: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Informes locales excluidos de git: `ZXSinclair.Net.Benchmarks/bin/control-before-artifacts/` y `control-q-generated-artifacts/`.

### ALU de 16 bits (2026-10-02)

BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11 (10.0.26300.9550), SDK 10.0.401, .NET 10.0.12 x64 RyuJIT, Release; afinidad 1, 6 iteraciones de calentamiento y 15 de medición. Mismos benchmarks y parámetros en todas las ejecuciones.

| Método | Control anterior repetido | ALU 16 repetida | Error control / final (IC 99.9%) | Desviación control / final | Asignaciones |
|---|---|---|---|---|---|
| ExecuteFrame, ns/opcode | 2.1315 | 2.1239 | 0.0062 / 0.0080 | 0.0052 / 0.0071 | 0 B |
| ExecuteLoopFrame, ns/T-state | 0.6640 | 0.6676 | 0.0020 / 0.0034 | 0.0017 / 0.0028 | 0 B |
| ExecuteAluLoopFrame, ns/T-state | 0.6900 | 0.6843 | 0.0084 / 0.0042 | 0.0070 / 0.0035 | 0 B |

Los intervalos se solapan en los tres métodos. Esta comparación no confirma una regresión reproducible del despacho. Las cargas conservan sus programas anteriores; no miden el coste de las nuevas instrucciones de 16 bits ni WebAssembly.

La primera comparación antes/después fue 2.1251 / 2.1403 ns/opcode, 0.6589 / 0.6715 ns/T-state y 0.6589 / 0.6888 ns/T-state, con errores respectivos 0.0074 / 0.0170, 0.0034 / 0.0020 y 0.0083 / 0.0099. Los intervalos de ambos bucles no se solapaban. Para comprobar ese resultado se volvió a medir el despacho anterior en una copia aislada: mismo Core y mismos benchmarks, sustituyendo únicamente Main, ED e Indexed por los generados de HEAD antes del grupo 6. Después se repitió la versión final sin cambios. La variación del propio control, especialmente en el bucle ALU, impide atribuir la diferencia inicial al nuevo grupo.

Reproducir la versión final: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Informes locales excluidos de git: `ZXSinclair.Net.Benchmarks/bin/alu16-before-artifacts/`, `alu16-after-artifacts/`, `alu16-control-artifacts/` y `alu16-repeat-artifacts/`. Copia del control en `ZXSinclair.Net.Benchmarks/bin/alu16-control/`.

En `Z80Cpu<SpectrumBus>.ExecuteED`, con ADC/SBC inlined y JIT FullOpts x64 (`DOTNET_TieredCompilation=0`), las ocho variantes calculan Z con `test` y `cmovne`; no hay salto para elegir Z. Sonda y desensamblado locales en `ZXSinclair.Net.Benchmarks/bin/alu16-jit/`. No se ha comprobado el código generado de WebAssembly.

### Rotaciones y desplazamientos (2026-10-02)

BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11 (10.0.26300.9550), SDK 10.0.401, .NET 10.0.12 x64 RyuJIT, Release; afinidad 1, 6 iteraciones de calentamiento y 15 de medición. Control medido en el mismo checkout antes de añadir el grupo 7, conservando los cambios del grupo 6.

| Método | Antes | Grupo 7 | Error antes / después (IC 99.9%) | Desviación antes / después | Asignaciones |
|---|---|---|---|---|---|
| ExecuteFrame, ns/opcode | 2.1306 | 2.1312 | 0.0192 / 0.0038 | 0.0171 / 0.0032 | 0 B |
| ExecuteLoopFrame, ns/T-state | 0.6623 | 0.6678 | 0.0054 / 0.0025 | 0.0048 / 0.0022 | 0 B |
| ExecuteAluLoopFrame, ns/T-state | 0.6868 | 0.7050 | 0.0050 / 0.0175 | 0.0041 / 0.0155 | 0 B |

Los intervalos se solapan en los tres métodos. Las medias cambian +0.03%, +0.83% y +2.65%; la comparación no confirma una regresión superior al ruido. El bucle ALU tuvo mayor variación después del cambio. Estas cargas conservan sus programas anteriores: no incluyen las nuevas rotaciones ni el camino DDCB. No miden el coste de cada instrucción ni WebAssembly.

Reproducir: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Informes locales excluidos de git: `ZXSinclair.Net.Benchmarks/bin/rotate-before-artifacts/` y `rotate-after-artifacts/`.

Hito tras el grupo 7: arranque de la ROM 48K sin verificar; siguen pendientes E/S e intercambios (base 08, D3, D9, DB, E3, EB). Se mantiene la carga de ROM como pendiente para el grupo 10. No se presentan estos bucles sintéticos como una medida de una máquina real.

### Operaciones de bit (2026-10-03)

BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11 (10.0.26300.9550), SDK 10.0.401, .NET 10.0.12 x64 RyuJIT, Release; afinidad 1, 6 iteraciones de calentamiento y 15 de medición. Control medido antes de añadir el grupo 8; mismos programas y parámetros.

| Método | Antes | Grupo 8 | Error antes / después (IC 99.9%) | Desviación antes / después | Asignaciones |
|---|---|---|---|---|---|
| ExecuteFrame, ns/opcode | 2.3214 | 2.4982 | 0.1871 / 0.0329 | 0.1659 / 0.0292 | 0 B |
| ExecuteLoopFrame, ns/T-state | 0.6889 | 0.6926 | 0.0106 / 0.0085 | 0.0094 / 0.0071 | 0 B |
| ExecuteAluLoopFrame, ns/T-state | 0.7201 | 0.7370 | 0.0227 / 0.0101 | 0.0177 / 0.0084 | 0 B |

Los intervalos se solapan en los tres métodos; esta comparación no confirma una regresión superior al ruido. Las medias cambian +7.62%, +0.54% y +2.35%. El control NOP tiene un intervalo amplio, por lo que no permite concluir que su coste sea idéntico. Las cargas no incluyen BIT/SET/RES ni el camino DDCB; miden los programas sintéticos anteriores y no WebAssembly. No hay asignaciones.

Reproducir: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Informes locales excluidos de git: `ZXSinclair.Net.Benchmarks/bin/bits-before-artifacts/` y `bits-after-artifacts/`.

### Intercambio y bloques (2026-10-03)

BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11 (10.0.26300.9550), SDK 10.0.401, .NET 10.0.12 x64 RyuJIT, Release; afinidad 1, 6 iteraciones de calentamiento y 15 de medición. Control obtenido antes de implementar el grupo 9.

| Método | Antes | Grupo 9 | Error antes / después (IC 99.9%) | Desviación antes / después | Asignaciones |
|---|---|---|---|---|---|
| ExecuteFrame, ns/opcode | 2.2624 | 2.2660 | 0.0443 / 0.0342 | 0.0392 / 0.0303 | 0 B |
| ExecuteLoopFrame, ns/T-state | 0.6784 | 0.6890 | 0.0111 / 0.0085 | 0.0098 / 0.0075 | 0 B |
| ExecuteAluLoopFrame, ns/T-state | 0.7253 | 0.7495 | 0.0195 / 0.0115 | 0.0173 / 0.0096 | 0 B |
| ExecuteBlockCopyFrame, ns/T-state | — | 0.5117 | — / 0.0054 | — / 0.0045 | 0 B |

Los intervalos se solapan en los tres programas existentes. Las medias cambian +0.16%, +1.56% y +3.34%; esta comparación no confirma una regresión superior al ruido. Esos programas conservan su mezcla de instrucciones anterior.

El nuevo programa carga HL=9000h, DE=A000h y BC=1000h, ejecuta LDIR y vuelve a empezar con JR desde 800Bh. Código, origen y destino están en RAM no contenida. Cada invocación fija PC=8000h, desactiva IFF1, ejecuta un frame de 69888 T-states y llama a EndFrame. El resultado se normaliza por T-state; incluye fetch, despacho, comprobación de interrupciones y recarga de registros. GlobalCleanup comprueba cero opcodes pendientes y los primeros 3000 bytes copiados. No hay asignaciones; 0.5117 ns/T-state equivale a unas 558 veces el tiempo real a 3.5 MHz. No mide WebAssembly ni una instrucción aislada.

Reproducir: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Informes locales excluidos de git: `ZXSinclair.Net.Benchmarks/bin/block-before-artifacts/` y `block-after-artifacts/`.

### Entrada/salida (2026-10-03)

BenchmarkDotNet 0.15.8, Intel Core i7-14700, Windows 11 (10.0.26300.9550), SDK 10.0.401 y .NET 10.0.12 x64 RyuJIT. Release, afinidad 1, 6 iteraciones de calentamiento y 15 de medición. Control anterior al grupo 10, con los mismos programas.

| Método | Antes | Grupo 10 | Error antes / después (IC 99.9%) | Desviación antes / después | Asignaciones |
|---|---|---|---|---|---|
| ExecuteFrame, ns/opcode | 2.2103 | 2.2512 | 0.0231 / 0.0616 | 0.0205 / 0.0576 | 0 B |
| ExecuteLoopFrame, ns/T-state | 0.6765 | 0.7118 | 0.0093 / 0.0344 | 0.0087 / 0.0287 | 0 B |
| ExecuteAluLoopFrame, ns/T-state | 0.7283 | 0.7361 | 0.0250 / 0.0188 | 0.0209 / 0.0167 | 0 B |
| ExecuteBlockCopyFrame, ns/T-state | 0.5485 | 0.5488 | 0.0113 / 0.0276 | 0.0094 / 0.0258 | 0 B |

Los intervalos se solapan en los cuatro métodos. Las medias cambian +1.85%, +5.22%, +1.07% y +0.05%. Esta medición no confirma una regresión superior al ruido; la variación posterior es mayor en NOP, saltos y LDIR. Estos programas no ejecutan E/S y solo comparan su coste tras ampliar los dispatches. No miden WebAssembly.

`ExecuteRomBootFrames` carga una ROM de 16384 bytes desde `ZX_ROM_48K`, imprime la ruta y SHA256 y mide 200 frames desde reset por invocación. Resetea CPU y máquina y limpia la RAM sin asignaciones; ese coste se incluye. Llama a EndFrame entre frames, devuelve PC y GlobalCleanup rechaza opcodes pendientes. Se normaliza por frame: fps = 1e9 / media en ns, y factor de tiempo real = fps / (3500000 / 69888). Cuando no hay ruta existente, un filtro del lanzador lo omite con un aviso.

No había ROM local configurada, por lo que el arranque real queda sin medir. Se comprobó la ejecución del benchmark con una imagen sintética propia de 16 KiB que contiene JP 0000, usando `--job Dry`: terminó 200 frames y cero opcodes pendientes. Su única muestra en frío valida el montaje; no se usa como cifra de arranque de la ROM del Spectrum. Imagen e informes quedan en bin, excluidos de git; no se incluye una ROM de terceros.

Reproducir los cuatro benchmarks: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Para el arranque, fijar `ZX_ROM_48K` a una ruta absoluta y usar `--filter '*ExecuteRomBootFrames*'` con los mismos parámetros. Informes locales: `ZXSinclair.Net.Benchmarks/bin/io-before-artifacts/`, `io-after-artifacts/` e `io-rom-smoke-artifacts/`.

### Grupo 11: restos y estado pendiente (2026-10-03)

Mismo SDK/runtime, afinidad 1, 6 warmups y 15 iteraciones. Se midió antes, después, la alternativa de limpieza conjunta, una repetición de la versión final y el commit anterior 280d6d3 en una copia aislada bajo bin. Esa copia se retiró tras medir; los informes se conservan fuera de ella. BDN Error es la semiamplitud del intervalo del 99.9%.

| Método | Antes inicial | Control anterior repetido | Final repetido | Error control / final | StdDev control / final | Asignaciones |
|---|---:|---:|---:|---:|---:|---:|
| ExecuteFrame, ns/opcode | 2.2701 | 2.1925 | 2.1993 | 0.0267 / 0.0119 | 0.0237 / 0.0099 | 0 B |
| ExecuteLoopFrame, ns/T-state | 0.6865 | 0.6786 | 0.6669 | 0.0045 / 0.0030 | 0.0037 / 0.0025 | 0 B |
| ExecuteAluLoopFrame, ns/T-state | 0.7567 | 0.7202 | 0.7000 | 0.0045 / 0.0125 | 0.0035 / 0.0111 | 0 B |
| ExecuteBlockCopyFrame, ns/T-state | 0.5213 | 0.5211 | 0.5277 | 0.0068 / 0.0013 | 0.0060 / 0.0012 | 0 B |

NOP y LDIR se solapan con el control repetido; saltos y ALU tienen medias menores. No se confirma una regresión fuera de la variación entre ejecuciones. El primer después dio 2.1893/0.6591/0.9313/0.5313, con errores 0.0295/0.0029/0.1362/0.0014; ALU mostró mucha variación y LDIR fue más lento que el primer control. Por eso se repitieron ambos lados.

La alternativa prevista en el plan, dos bool contiguos borrados con una sola escritura de ushort, dio 3.2447/0.6758/0.6892/0.5228 (errores 0.7787/0.0048/0.0069/0.0021). Empeoró NOP y se descartó; no se atribuye la causa al JIT sin un análisis de ensamblado. Se conservan los offsets anteriores de EiPending/Q y SpecialLoadPending en 34, con escrituras separadas.

La nueva clasificación IM 0 usa ReadOnlySpan sobre 32 bytes estáticos, sin asignación por reconocimiento. Los benchmarks son cargas de escritorio; no miden WebAssembly, contención durante INT ni una ROM real. ExecuteRomBootFrames se omitió porque no hay ROM configurada.

Reproducir: `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*' --affinity 1 --warmupCount 6 --iterationCount 15`. Informes excluidos de git: `ZXSinclair.Net.Benchmarks/bin/rest-before-artifacts`, `rest-after-artifacts`, `rest-combined-artifacts`, `rest-repeat-artifacts` y `rest-control-repeat-artifacts`.

## 9. Fuera de alcance

- Respuestas de IM 0 de varios bytes o prefijos; ver spec de restos 2.2.4.
- Z80 CMOS y diferencias NMOS/CMOS más allá de anotarlas.
- Depurador, desensamblador y snapshots (usarán `Registers` y `Step()`).
- Integración con la máquina Spectrum (bucle de frame, vídeo).

## 10. Pendiente de verificar

- Q tras `POP AF`/`EX AF,AF'` con z80ccf (la actualización general de Q está implementada); MEMPTR no forma parte del formato FUSE.
- Valores de los registros tras reset más allá de PC, I, R, IFF, IM.
- Confirmación con hardware de la contención del T interno del reconocimiento y de la dirección de los M1 tras HALT recibido en IM 0.
- P/V tras `LD A,I`/`LD A,R` seguido de NMI; con INT está implementado y probado.
