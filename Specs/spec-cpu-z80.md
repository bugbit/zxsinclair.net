# Especificación: CPU base y CPU Z80

Especificación del núcleo de CPU de `ZXSinclair.Net.Core`: el contrato genérico de cualquier CPU y la CPU Z80 (registros, ciclo de ejecución, prefijos, interrupciones, HALT, reset) **con NOP como primera instrucción implementada** (ver `Specs/spec-instr-nop.md`). Las instrucciones se generarán con un generador nuevo (`ZXSinclair.Net.Generate.Z80OpCodes`) en una especificación posterior; aquí se fija el esqueleto que ese código rellenará.

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
- La CPU no suma ciclos por su cuenta: **todo el tiempo pasa por el bus** (`FetchOpcode`, `Read`, `Write`, `Internal`, `In`, `Out`, `AcknowledgeInterrupt`). Así la contención se aplica sola y los eventos de bus de los tests FUSE salen exactos.

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
- `EiPending`: la instrucción anterior fue `EI`; bloquea INT hasta completar la siguiente instrucción. Los prefijos DD/FD se procesan dentro del mismo paso y no necesitan este estado.
- `Q`: flags modificados por la última instrucción, necesarios para F3/F5 de `SCF`/`CCF` en el NMOS **(verificar si los tests FUSE lo exigen)**.

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
    EiPending = false
    si Halted → M1 sobre PC sin avanzarlo (NOP interno), R++
    si no     → opcode = M1(PC++); ejecutar(opcode)
```

M1 = `bus.FetchOpcode(PC)` + incremento de los 7 bits bajos de R. El incremento de R lo hace la CPU (el bus no conoce R).

### 4.3 Prefijos

| Prefijo | Comportamiento |
|---|---|
| `CB` | Segundo M1; la tabla CB. |
| `ED` | Segundo M1; la tabla ED. Los huecos sin instrucción actúan como dos NOP (8 T). |
| `DD` / `FD` | M1 propio (4 T, R++). El siguiente opcode se ejecuta con IX/IY en lugar de HL (y IXH/IXL en lugar de H/L, `(IX+d)` en lugar de `(HL)`). Si el opcode no usa HL, se ejecuta como sin prefijo. Una cadena `DD DD …` o `DD FD …` vale solo por el último prefijo; INT no se acepta entre un prefijo y su instrucción. |
| `DD CB d op` / `FD CB d op` | `d` y `op` se leen con `Read` (no M1, R no se incrementa con ellos): `pc:4, pc+1:4, pc+2:3, pc+3:3, pc+3:1 ×2` y el acceso a `(IX+d)`. |

Las instrucciones indexadas se generan **una sola vez** y se especializan para IX e IY con un parámetro genérico de tipo struct (`ExecuteIndexed<TIndex>() where TIndex : struct, IIndexRegister`), que el JIT convierte en dos copias sin coste en tiempo de ejecución. Nada de `ref` a un registro elegido en tiempo de ejecución ni de duplicar el código a mano.

Los prefijos repetidos se consumen mediante un bucle de espacio constante, conservando el último DD/FD y despachando después a la especialización IX/IY. PC puede dar la vuelta a 64K. Si toda la memoria contiene prefijos, no se completa una instrucción y `Step()` no retorna; el contrato no permite cortar una instrucción a mitad.

### 4.4 Temporización de las instrucciones

Cada instrucción emite exactamente la secuencia de ciclos de la tabla de la Sinclair Wiki, traducida a llamadas al bus:

| Notación | Llamada |
|---|---|
| `pc:4` (M1) | `bus.FetchOpcode(PC)` + R++ |
| `pc:3`, `hl:3`, `sp:3`, `nn:3` | `bus.Read(dir)` / `bus.Write(dir, v)` |
| `ir:1 ×2`, `hl:1`, `pc+2:1 ×5` | `bus.Internal(dir, n)` con la dirección que la CPU deja en el bus |
| `IO` | `bus.In(puerto)` / `bus.Out(puerto, v)` |

Las tablas de la wiki y los eventos `MC` de los tests FUSE son la referencia; los ciclos internos usan **siempre** la dirección correcta, aunque en el bus sin contención no tenga efecto.

## 5. Contrato con el generador de instrucciones

- El generador produce ficheros `partial` de `Z80Cpu<TBus>` con métodos `ExecuteMain(byte)`, `ExecuteCB(byte)`, `ExecuteED(byte)`, `ExecuteIndexedOpcode<TIndex>(byte)`, `ExecuteIndexedCB<TIndex>(ushort address, byte opcode)`, cada uno un `switch` sobre el byte (el JIT lo compila a una tabla de saltos). `ExecuteIndexed<TIndex>()` y `FinishIndexed<TIndex>(byte)` pertenecen al ciclo de prefijos escrito a mano.
- Cada caso llama a métodos pequeños `[AggressiveInlining]` escritos a mano (ALU, rotaciones, `Push`/`Pop`…) o contiene el código directamente.
- Las cinco tablas de entrada son `data/opcodes_*.dat` (formato FUSE), incrustadas en `ZXSinclair.Net.Generate.Z80OpCodes`. El generador está implementado según `spec-generador-z80.md`, con NOP como patrón piloto; no referencia el Core.
- Los ficheros generados llevan la cabecera GPL y no se editan a mano.
- Los cinco despachos se generan en `ZXSinclair.Net.Core/Z80/Generated/` (`Z80Cpu.Main.g.cs`, `Z80Cpu.CB.g.cs`, `Z80Cpu.ED.g.cs`, `Z80Cpu.Indexed.g.cs`, `Z80Cpu.IndexedCB.g.cs`). `00` (NOP) está implementado en `ExecuteMain` y `ExecuteIndexedOpcode<TIndex>` como un caso vacío. Las cargas de 8 bits también están implementadas según `spec-instr-carga-8.md`; el resto queda como "no implementado" (ver 8). El despacho principal y sus cuerpos auxiliares generados usan `AggressiveInlining`; sus casos implementados terminan con `return` para reducir el tamaño del IL.
- El archivo provisional se ha eliminado. Se regenera con `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes`; `-- --check` compara sin escribir y detecta diferencias con código 1. Cada opcode completo aún no implementado llama a `Unimplemented()`: incrementa `UnimplementedOpcodes` y no modifica registros ni emite más ciclos. Solo se han consumido los accesos de fetch/prefijos. `Reset()` limpia el contador.

## 6. Interrupciones

### 6.1 Reglas comunes

- Se comprueban al final de cada instrucción completa (nunca entre un prefijo y su instrucción, ni justo después de `EI`).
- Si la CPU está en HALT, aceptar la interrupción la saca de HALT y `PC` apunta a la instrucción siguiente a `HALT` antes de apilarlo.
- El ciclo de reconocimiento incrementa R (es un M1).
- Tras `LD A,I` / `LD A,R`, si se acepta una INT justo después, el NMOS deja P/V a 0 **(verificar con FUSE; afecta a juegos que detectan el modelo)**.

### 6.2 NMI

Flanco (`RequestNmi()`), prioridad sobre INT, ignora IFF1. M1 normal de 5 T con el dato ignorado, `IFF1 = 0` (IFF2 se conserva), apila PC y salta a `0x0066`: 11 T en total. `RETN` copia IFF2 en IFF1. El Spectrum no lo usa de serie (sí interfaces como el Multiface).

### 6.3 INT

Nivel (`bus.IntActive`); se acepta si `IFF1` y no `EiPending`. `IFF1 = IFF2 = 0`. Reconocimiento con `bus.AcknowledgeInterrupt()` (7 T en el Spectrum, devuelve el byte del bus) y después:

| Modo | Acción | T-states en el Spectrum |
|---|---|---|
| IM 0 | Ejecuta la instrucción del bus; en el Spectrum es 0xFF = `RST 38h` | 13 **(verificar)** |
| IM 1 | Apila PC (`sp-1:3`, `sp-2:3`) y salta a `0x0038` | 7 + 6 = 13 |
| IM 2 | Apila PC, lee el vector en `(I << 8) | byte del bus` (`2 × 3`) y salta | 7 + 6 + 6 = 19 |

En este esqueleto IM0 solo admite respuestas `RST n` (13 T con reconocimiento de 7 T). Los otros bytes incrementan `UnimplementedOpcodes`, sin apilar PC ni cambiar PC/WZ tras salir de HALT. IFF1/IFF2 y R sí reflejan el reconocimiento realizado. No es su ejecución real: queda pendiente junto con las instrucciones. Zilog UM0080, apartado CPU Response / Mode 0, confirma que el dispositivo puede proporcionar cualquier instrucción.

## 7. Reset

`Reset()` deja: `PC = 0`, `I = R = 0`, `IFF1 = IFF2 = 0`, `IM = 0`, `Halted = false`, `EiPending = false`, `AF = SP = 0xFFFF` (valor del hardware real según Young) y el resto de registros a 0 **(verificar valores no garantizados)**. No resetea el bus ni la máquina.

## 8. Pruebas

### 8.1 Tests FUSE (`ZXSinclair.Net.Test`)

- El runner usa un bus de pruebas, `FuseTestBus : struct, IZ80Bus` (en la librería `ZXSinclair.Net.Fuse`, compartida con los tests xUnit junto con el parser `FuseTestFile`, `FuseCpuState` y `FuseComparison`): memoria plana de 64K, contador de T-states, sin contención real, que **registra los eventos de bus** con la semántica de FUSE: `MC` al empezar cada ciclo de memoria y `MR`/`MW` al terminarlo; un `MC` por T-state en `Internal`; `PC`/`PR`/`PW` siguiendo la tabla de contención de E/S del 48K (byte alto 0x40–0x7F). Así la CPU del Core no necesita instrumentación ni `#if` de test.
- Se ejecuta `Z80Cpu<FuseTestBus>` hasta `end_tstates` y se comparan registros (incluidos AF', BC', DE', HL'), `I`, `R`, `IFF1`, `IFF2`, `IM`, `halted`, T-states, memoria y la secuencia completa de eventos.
- Un opcode no implementado se marca en la CPU (contador u opción de compilación del test) y el test se cuenta como omitido, como hasta ahora.
- El runner está conectado a `Z80Cpu<FuseTestBus>`: en Debug o Release carga y ejecuta los 1335 casos, actualmente 201 pasan (3 de NOP, 163 del grupo de carga de 8 bits y 35 del de carga de 16 bits), 0 fallan y 1134 se omiten por instrucciones pendientes, con comparación de eventos activada. `--no-events` desactiva el registro y comparación de eventos. Devuelve código 1 si hay fallos. Un fichero FUSE mal formado lanza `FormatException` con el nombre del test.
- Tests xUnit contrastan directamente los ciclos del bus con los fixtures `00`, `ddcb00`, `d3*` y `db*`, y verifican la detección de discrepancias en registros, memoria, ciclos y eventos aun cuando el runner omite instrucciones.

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

## 9. Fuera de alcance

- Las instrucciones distintas de NOP y de los grupos de carga de 8 y 16 bits; se implementan por grupos mediante el generador existente.
- Z80 CMOS y diferencias NMOS/CMOS más allá de anotarlas.
- Depurador, desensamblador y snapshots (usarán `Registers` y `Step()`).
- Integración con la máquina Spectrum (bucle de frame, vídeo).

## 10. Pendiente de verificar

- Uso de `Q` y MEMPTR en los tests FUSE.
- Valores de los registros tras reset más allá de PC, I, R, IFF, IM.
- Temporización exacta de IM 0 con 0xFF en el bus del Spectrum.
- P/V tras `LD A,I`/`LD A,R` interrumpido.
