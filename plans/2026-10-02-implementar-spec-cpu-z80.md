# Plan: implementar la especificación de la CPU Z80 (sin instrucciones)

Fecha: 2026-10-02
Estado: implementado y verificado; IM0 no soportado se marca como no implementado y los prefijos repetidos se procesan mediante un bucle.

## Objetivo
Implementar `Specs/spec-cpu-z80.md` en `ZXSinclair.Net.Core`: el contrato `ICpu` y la CPU `Z80Cpu<TBus>` (registros, flags, ciclo de ejecución, prefijos, interrupciones, HALT, reset) sin ninguna instrucción, junto con sus tests, el bus de pruebas FUSE y el runner reconectado.

## Contexto inicial
- `Specs/spec-cpu-z80.md` define el contrato genérico `ICpu` y la CPU `Z80Cpu<TBus>` (registros, ciclo de ejecución, prefijos, interrupciones, HALT, reset, bus de pruebas FUSE y contrato con el generador) **sin instrucciones**. El Core ya tiene el bus (`Z80/IZ80Bus.cs`, `Machines/Spectrum/SpectrumBus.cs`) y la memoria; no hay CPU. `ZXSinclair.Net.Test` solo carga los ficheros FUSE (`Program.cs`), con `FuseComparison.cs` y `Z80BusEvent.cs` preparados para comparar memoria y eventos. El generador está vacío (solo `data/opcodes_*.dat`).
- Hechos de los datos FUSE que fijan comportamiento:
  - `76` (HALT): tras ejecutarlo `PC = 0000` (apunta al propio HALT) y `halted = 1` → la CPU no avanza PC en HALT (spec 4.2/6.1).
  - `dd00`: `DD` + `00` = dos M1 (8 T, R+2): un prefijo seguido de un opcode que no usa HL se ejecuta sin prefijo.
  - Eventos de E/S (`d3_2`, `d3_3`, `d3`, `db_1`): `PC` temprano solo si el byte alto está en 0x40–0x7F; `PR`/`PW` tras la primera fase; `PC` tardío según A0 y byte alto.
- Restricción principal: rendimiento. `TBus` siempre struct (el JIT especializa `Z80Cpu<SpectrumBus>`); campo del bus **no `readonly`** para evitar copias defensivas al llamar métodos de un struct genérico; sin virtuales, delegados ni asignaciones en `Step`/`Execute`. Sin `#if` de test dentro de la CPU.
- Como no hay instrucciones, todo opcode cae en un caso por defecto que cuenta "no implementado" y se comporta como NOP; así el bucle, prefijos, interrupciones y el runner FUSE se pueden probar ya.

## Pasos
1. **`Abstractions/ICpu.cs`** — `void Reset()`, `void Step()`, `void Execute(int targetCycles)`. Cabecera GPL en todos los ficheros nuevos.
2. **`Z80/Z80Flags.cs`** — constantes `C=0x01, N=0x02, PV=0x04, F3=0x08, H=0x10, F5=0x20, Z=0x40, S=0x80` y tablas estáticas de 256 entradas `SZ53`, `SZ53P`, `Parity` (equivalentes a `mTablePV`/`mTableZS53` del código legado, ahora en git).
3. **`Z80/Z80Registers.cs`** — `[StructLayout(LayoutKind.Explicit)] struct` con campos públicos superpuestos (little-endian): AF/A/F (0), BC/B/C (2), DE/D/E (4), HL/H/L (6), AF'/BC'/DE'/HL' (8–14), IX/IXH/IXL (16), IY/IYH/IYL (18), SP (20), PC (22), WZ (24), IR/I/R (26); estado: `IFF1`, `IFF2` (bool), `IM` (byte), `Halted`, `EiPending` (bool), `Q` (byte). Helpers `[AggressiveInlining]`: `IncrementR()` (7 bits, bit 7 intacto), `ExchangeAF()`, `Exx()`.
4. **`Z80/IIndexRegister.cs`** — interfaz con miembros estáticos abstractos (`static abstract ref ushort Pair(ref Z80Registers)`, `High`, `Low`) y structs `IxRegister` / `IyRegister`. `ExecuteIndexed<TIndex>() where TIndex : struct, IIndexRegister` queda especializado por el JIT para IX e IY sin coste en tiempo de ejecución.
5. **`Z80/Z80Cpu.cs`** — `public sealed partial class Z80Cpu<TBus> : ICpu where TBus : struct, IZ80Bus`:
   - Campos: `private TBus bus;` (no readonly), `public Z80Registers Registers;` (campo, acceso por `ref`), `private bool nmiPending`, `public int UnimplementedOpcodes`.
   - `Reset()`: spec 7 (`PC=0`, `I=R=0`, `IFF1=IFF2=false`, `IM=0`, `Halted=EiPending=false`, `AF=SP=0xFFFF`, resto 0, `nmiPending=false`); no resetea el bus.
   - `RequestNmi()`, `bool Halted`.
   - `Step()` según spec 4.2: NMI → INT (`Registers.IFF1 && !Registers.EiPending && bus.IntActive`) → `EiPending=false` → si `Halted`, M1 sobre PC sin avanzar (`bus.FetchOpcode(PC)`, R++) → si no, `opcode = FetchOpcode()` (M1 en PC, PC++, R++) y despacho: `0xCB` → `ExecuteCB(FetchOpcode())`, `0xED` → `ExecuteED(FetchOpcode())`, `0xDD` → `ExecuteIndexed<IxRegister>()`, `0xFD` → `ExecuteIndexed<IyRegister>()`, resto → `ExecuteMain(opcode)`.
   - `ExecuteIndexed<TIndex>()`: siguiente M1; `DD`/`FD` → bucle que conserva el último prefijo y despacha finalmente al tipo IX/IY especializado (sin aceptar INT entre medias); `ED` → `ExecuteED(FetchOpcode())` (el prefijo se ignora); `CB` → ciclo DDCB: `d = bus.Read(PC++)`, `op = bus.Read(PC++)`, `bus.Internal(dirección de op, 2)`, `ExecuteIndexedCB<TIndex>((ushort)(índice + (sbyte)d), op)`; otro → `ExecuteIndexedOpcode<TIndex>(op)`.
   - `Execute(int targetCycles)`: `while (bus.Cycles < targetCycles) Step();`.
   - Helpers `[AggressiveInlining]` para el generador futuro: `FetchOpcode()`, `ReadPc()` (byte en PC++), `Push(ushort)`, `Pop()`, `Unimplemented()` (incrementa el contador; el opcode se comporta como NOP).
6. **`Z80/Z80Cpu.Interrupts.cs`** — spec 6:
   - NMI (11 T): sale de HALT (`PC++`), `nmiPending=false`, `IFF1=false` (IFF2 se conserva), M1 de 5 T = `bus.FetchOpcode(PC)` (dato ignorado) + R++ + `bus.Internal(IR, 1)`, `Push(PC)`, `PC = WZ = 0x0066`.
   - INT: sale de HALT (`PC++`), `IFF1=IFF2=false`, R++, `data = bus.AcknowledgeInterrupt()`; IM0: si `data` es `RST n` (`(data & 0xC7) == 0xC7`) → `Push(PC)`, `PC = data & 0x38`; cualquier otro byte se marca con `Unimplemented()` sin apilar PC ni saltar; su ejecución real queda para la especificación de instrucciones. IM1: `Push(PC)`, `PC=0x0038`; IM2: `Push(PC)`, `PC = Read(v) | Read(v+1) << 8` con `v = (I << 8) | data`; `WZ = PC` para las respuestas implementadas.
7. **`Z80/Z80Cpu.Instructions.cs`** (provisional) — métodos `ExecuteMain(byte)`, `ExecuteCB(byte)`, `ExecuteED(byte)`, `ExecuteIndexedOpcode<TIndex>(byte)`, `ExecuteIndexedCB<TIndex>(ushort, byte)`, cada uno un `switch` con solo `default: Unimplemented(); break;`. Comentario: el generador nuevo sustituirá este fichero (spec 5). `ExecuteIndexedOpcode` delegará en `ExecuteMain` cuando el opcode no use HL (lo hará el generador; hoy el default ya es `Unimplemented()`).
8. **Tests xUnit** — `ZXSinclair.Net.Core.Tests`:
   - `TestBus.cs`: `struct TestBus : IZ80Bus` sobre una clase `TestBusState` (64K planos, `Cycles`, `IntActive` configurable, dato de `AcknowledgeInterrupt` configurable, coste 4/3/1/4/7 T sin contención).
   - `Z80FlagsTests`: tablas (S, Z, F3/F5, paridad) para valores representativos.
   - `Z80RegistersTests`: alias de 8/16 bits (A/F en AF, IXH/IXL…), juego alternativo con `ExchangeAF`/`Exx`, `IncrementR` (0x7F→0x00, 0xFF→0x80).
   - `Z80CpuTests`: `Reset`; opcode desconocido = 4 T, PC+1, R+1, `UnimplementedOpcodes == 1`; `CB xx`/`ED xx` = 8 T, R+2; `DD 00` = 8 T, R+2 (como `dd00` de FUSE); `DD FD xx` = 12 T, R+3; `DD CB d op` = 16 T, R+2, PC+4; HALT simulado (`Halted=true`) = M1 sin avanzar PC; IM1 = 13 T a 0x0038 con PC apilado e IFF a 0; IM2 = 19 T con vector `(I<<8)|dato`; IM0 con 0xFF = RST 38; NMI = 11 T a 0x0066 con IFF2 conservado y prioridad sobre INT; INT bloqueada con `EiPending` o `IFF1=false`; salida de HALT apila la dirección siguiente; `Execute(target)` para en la primera instrucción que alcanza o supera `target`.
   - Integración: `new Z80Cpu<SpectrumBus>(new SpectrumMachine(...).Bus)` ejecuta un frame (todo NOP no implementado) y respeta la contención.
9. **Runner FUSE** — `ZXSinclair.Net.Test`:
   - `FuseTestBus.cs`: `struct FuseTestBus : IZ80Bus` sobre `FuseTestBusState` (64K planos, `Cycles`, `List<Z80BusEvent>?`). Eventos con la semántica de FUSE: `MC` al inicio y `MR`/`MW` al final de cada acceso; un `MC` por T-state en `Internal`; E/S: `PC` temprano solo si el byte alto está en 0x40–0x7F, `PR`/`PW` tras la primera fase, `PC` tardío (A0=0: uno y +3; byte alto contenido: tres de 1 T; si no, +3 sin eventos). `IntActive = false`; `AcknowledgeInterrupt` = 7 T, 0xFF.
   - `Program.cs`: por test, `Reset`, rellenar memoria con `DE AD BE EF`, cargar bloques, fijar registros (incluidos `af_`…`hl_`, I, R, IFF1, IFF2, IM, halted de `clsTestLine1/2`), ejecutar `Step()` mientras `Cycles < end_tstates`; si `UnimplementedOpcodes > 0` → omitido; si no, comparar registros, alternativos, I, R, IFF1, IFF2, IM, halted, T-states, memoria (`FuseComparison.ExpectedMemory/CompareMemory`) y eventos (`FuseComparison.CompareEvents`, desactivable con `--no-events`). Resumen pasados/fallidos/omitidos, código ≠ 0 si hay fallos. Se mantiene Debug obligatorio.
   - Resultado esperado hoy: 1335 omitidos (no hay instrucciones).
10. **Benchmark** — `ZXSinclair.Net.Benchmarks/Z80CpuBenchmarks.cs`: `Execute(TStatesPerFrame)` + `EndFrame()` con `Z80Cpu<SpectrumBus>` sobre memoria a ceros (todo "no implementado" = NOP), en ns por instrucción. Mide el coste fijo de fetch + despacho + comprobación de interrupciones; dejar el resultado en la sección 8.3 de la spec.
11. **Documentación** — `CLAUDE.md`, `AGENTS.md` (Core: `ICpu`, `Z80Cpu<TBus>`, ficheros provisionales a sustituir por el generador, `FuseTestBus`, runner funcionando con omitidos) y la spec (8.3 con la medición; anotar decisiones: `Unimplemented()`, IM0 con bytes que no son RST).
12. **Verificación**:
    - `dotnet build zxsinclair.net.slnx -c Debug` y `-c Release` sin advertencias nuevas.
    - `dotnet test ZXSinclair.Net.Core.Tests` (Debug y Release) en verde.
    - `dotnet run --project ZXSinclair.Net.Test -c Debug` → `0 passed / 0 failed / 1335 skipped`, código 0.
    - `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*'` y anotar ns/instrucción.

## Riesgos / cosas a vigilar
- Los casos FUSE se omiten hasta que haya instrucciones. Cubrir `FuseTestBus` y las comparaciones con tests xUnit: M1 de `00`, prefijo DDCB, memoria, ciclos internos y las cuatro secuencias de E/S, usando eventos de los fixtures reales. Comprobar que discrepancias de registros, memoria, T-states y eventos se detectan.
- Copias defensivas: si `bus` fuera `readonly` y `TBus` no fuera `readonly struct`, cada llamada copiaría el struct. Mantener el campo mutable.
- Prefijos `DD`/`FD` encadenados: procesar con espacio constante y probar cadenas largas, alternas y cruce de 0xFFFF. Una memoria formada solo por prefijos no termina una instrucción; `Step` y `Execute` no tienen presupuesto parcial por diseño.
- IM0 con datos distintos de `RST` queda explícitamente no implementado. P/V tras `LD A,I` interrumpido queda pendiente de la especificación de instrucciones.
- `Halted` y `PC` deben seguir a FUSE (`76`: PC sobre el HALT), coherente con spec 6.1.

## Fuera de alcance
- Cualquier instrucción (incluidas NOP, HALT, EI/DI, `LD A,I`) y el generador nuevo.
- Integración en un bucle de máquina/frontend.
- Z80 CMOS.

## Resultado de la implementación
- CPU base, registros, flags, prefijos iterativos e interrupciones implementados. Los despachos de instrucciones siguen provisionales.
- IM0 admite los ocho RST; los demás bytes incrementan el contador de no implementados sin escrituras de pila ni saltos.
- Tests: 174/174 en Debug y 174/174 en Release. Incluyen una cadena de 65535 prefijos alternos, cruce de 0xFFFF, INT diferida durante prefijos y comparación de eventos del bus con fixtures FUSE reales.
- Solución compilada en Debug y Release sin errores ni advertencias nuevas. La primera compilación del runner muestra las dos advertencias previas CS8629/CS8618 de `clsTests.cs`; las compilaciones incrementales finales no muestran advertencias.
- Runner FUSE, con eventos y con `--no-events`: 0 pasados / 0 fallidos / 1335 omitidos, código 0. Generador placeholder: código 0.
- BenchmarkDotNet: 3.844 ns por opcode no implementado, error 0.0293 ns, desviación 0.0260 ns y 0 B asignados; entorno y alcance en spec 8.3.
- Se añadió `FuseCpuState` para compartir la carga/comparación del estado entre runner y tests. El proyecto xUnit referencia el runner para comprobar su bus y sus comparaciones también en Release, cargando los registros de los fixtures sin depender de `Debug.Assert`.
- Documentación y cabeceras GPL verificadas. No se han hecho commits.
