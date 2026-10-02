# Plan: implementar la especificación de buses y memoria en ZXSinclair.Net.Core

Fecha: 2026-10-02
Estado: implementado (2026-10-02)

## Objetivo
Implementar `Specs/spec-buses-memoria.md` en `ZXSinclair.Net.Core`: abstracciones genéricas de bus y memoria válidas para cualquier máquina, memoria configurable (Spectrum 16K/48K/128K/+2, ZX81 1K/16K) y el bus Z80 del Spectrum con contención, paginación, E/S, bus flotante e interrupciones, priorizando el rendimiento.

## Contexto
- `Specs/spec-buses-memoria.md` define el bus del Z80 y la memoria del 48K/128K/+2 (contención, paginación, bus flotante, puerto 0xFE, INT). Hoy el Core solo tiene cuatro interfaces borrador de 2023 en `Abstractions/` (`IBus` con `SyncBus`, `IBusData`, `IMemory`, `IMemoryBuffer`) que nadie referencia. No hay CPU en el Core; el `Z80Cpu` legado (`ZXSinclair.Net`) no se toca.
- Decisiones del usuario:
  - Tests en un **proyecto xUnit nuevo**.
  - Las abstracciones deben servir para **otros tipos de buses y memoria**, no solo el Z80: se conservan las genéricas `IMemory<TAddress, TData>` e `IBusData<TAddress, TData>` y todo lo demás se construye encima.
  - La memoria no puede asumir 48K/128K: debe permitir **cualquier tamaño y mapa**, p. ej. ZX Spectrum 16K o ZX81 con 1K.
- Restricción principal: rendimiento (CLAUDE.md, README). Los genéricos solo se especializan (JIT con inline, sin despacho por interfaz) cuando el argumento de tipo es **struct**; con clases se comparte el código y vuelve la llamada por interfaz. Por eso el bus será un **struct envoltorio** sobre una clase `sealed` con el estado de la máquina, y la futura CPU será `Z80Cpu<TBus> where TBus : struct, IZ80Bus`.
- Benchmark previo (`ZXSinclair.Net.Benchmarks`): array con `Unsafe.Add` ~1.0 ns/op; `MemoryBuffer8Bit` vía interfaz 1.65×; paginado 128K con tabla de ranuras 2.17×.
- Alcance: memoria configurable con presets Spectrum 16K, 48K, 128K/+2 y ZX81 1K/16K; bus completo (contención, E/S, bus flotante, INT) para los Spectrum 16K/48K/128K/+2. Del ZX81 solo el mapa de memoria.

## Pasos
1. **Ampliar la spec primero** (`Specs/spec-buses-memoria.md`): añadir en la sección 4 los mapas del Spectrum 16K (0x8000–0xFFFF sin RAM → 0xFF) y del ZX81 1K y 16K (ROM de 8K y su espejo, RAM de 1K con espejos, RAM pack de 16K, decodificación parcial de A15), consultando WoS / Sinclair Wiki y marcando **(verificar)** lo no confirmado. Añadir en la sección 9.4 el modelo de memoria por páginas configurable (paso 4).
2. **Abstracciones genéricas** en `Abstractions/` (todas `where TAddress : struct where TData : struct`, válidas para cualquier CPU/máquina):
   - `IMemory<TAddress, TData>` (se conserva): `Read`/`Write` crudos, sin temporización.
   - `IBusData<TAddress, TData>` (se conserva): lectura/escritura **a través del bus**, con temporización y contención.
   - `IBusIo<TPort, TData>` (nuevo): `In`/`Out` para buses con espacio de E/S separado.
   - `IBus` (se rehace): raíz de cualquier bus con reloj — `int Cycles { get; }`, `Reset()`; se quita `SyncBus`.
   - `IMemoryBuffer<TAddress, TData>` (se rehace): `Size` y carga/volcado por `Span` (`CopyFrom(TAddress, ReadOnlySpan<TData>)`, `CopyTo(TAddress, Span<TData>)`) para ROMs y snapshots.
   - `Z80/IZ80Bus.cs`: `IZ80Bus : IBus, IBusData<ushort, byte>, IBusIo<ushort, byte>` con lo propio del Z80:
     - `byte FetchOpcode(ushort address)` — M1, contención en T1 + 4 T.
     - `Read`/`Write` — contención en T1 + 3 T.
     - `void Internal(ushort address, int tstates)` — un T con contención por cada T-state.
     - `In`/`Out` — secuencia de contención de E/S (5.4); el acceso al dispositivo ocurre tras la primera fase (N:1 o C:1), como en FUSE (`d3_2`: `7 PC`, `8 PW`, `8 PC`).
     - `bool IntActive { get; }`, `byte AcknowledgeInterrupt()` (7 T, devuelve 0xFF; **verificar** contra FUSE).
   - Regla: las interfaces son contrato y restricción genérica (`where TBus : struct, IZ80Bus`); nunca se llama a través de una variable de tipo interfaz en el bucle crítico.
   - Todo archivo nuevo con la cabecera GPL obligatoria y namespaces de archivo.
3. **Benchmark previo de la memoria por páginas** — ampliar `ZXSinclair.Net.Benchmarks/Memories.cs` y `MemoryAccessBenchmarks.cs` con variantes de tabla de páginas (tamaño de página como `shift` en campo `readonly`; desplazamientos en `int[]` con `Unsafe.Add`; tablas separadas de lectura y escritura) para páginas de 1K y 16K, y compararlas con el array plano (`ArrayUnsafeAdd`, ~1.0 ns) y con la versión paginada actual (2.17×). Si la tabla de páginas genérica queda claramente por detrás del plano, se mantiene además una memoria plana especializada para 48K (paso 4). Anotar resultados en la spec (9.4).
4. **Memoria configurable** en `Memory/`:
   - `MemoryLayout` (descripción, fuera del bucle crítico): tamaño de página (potencia de 2: 1K, 16K…), regiones (`AddRom(size)`, `AddRam(size)`) y mapeo de rangos de direcciones a regiones con desplazamiento, solo lectura o ausente. Los espejos son varios rangos apuntando a la misma región.
   - `PagedMemory` (`sealed`, implementa `IMemory<ushort, byte>` e `IMemoryBuffer<ushort, byte>`): un único array con todas las regiones + una página "0xFF" para huecos + una página sumidero para escrituras en ROM o huecos; tablas `readOffset[]` y `writeOffset[]` por página (sin ramas) y `contended[]` por página. API: `Map(pageIndex, region, offset, readOnly)`, `LoadRom(region, span)`, `Read`/`Write` con `Unsafe.Add`.
   - `FlatMemory64K` (`sealed`) solo si el benchmark del paso 3 lo justifica: 48K sin tabla de páginas.
   - Presets en `Machines/Layouts.cs`: `Spectrum16K`, `Spectrum48K`, `Spectrum128K` (+2 = 128K), `Zx81_1K`, `Zx81_16K`.
   - **Paginación 128K/+2** — `Spectrum128Paging`: `Write7FFD(byte)` remapea la página de 0xC000 (bits 0–2), la ROM (bit 4), la pantalla visible (bit 3 → `ScreenOffset` banco 5 o 7) y el bloqueo (bit 5); recalcula `contended[]` (bancos impares); `Reset()` = 0. Nunca copia memoria.
5. **Temporización** — `Timing/MachineTiming.cs`: `sealed record` con `TStatesPerLine`, `LinesPerFrame`, `TStatesPerFrame`, `FirstContendedTState`, `FloatingBusFirstTState`, `InterruptLength`, `LateTiming` (+1). Presets `Spectrum48` (también 16K: 224/312/69 888/14 335/14 338/32) y `Spectrum128` (228/311/70 908/14 361/14 364/36 **verificar**).
6. **Tabla de contención** — `Timing/ContentionTable.cs`: `byte[]` de `TStatesPerFrame + margen` (≥ 256) con el patrón 6,5,4,3,2,1,0,0 en las 128 T visibles de cada una de las 192 líneas (fórmula de 5.2, +1 en late timing).
7. **Tabla de bus flotante** — `Timing/FloatingBusTable.cs`: `short[]` por T-state con el desplazamiento dentro de la pantalla visible (bitmap, atributo, bitmap+1, atributo+1) o −1 (= 0xFF), según la sección 6.
8. **Máquinas y buses Spectrum** — `Machines/Spectrum/SpectrumMachine.cs` (`sealed`): `TStates` (int), `PagedMemory`, tablas, paginación opcional (128K), estado del puerto 0xFE (borde, MIC, EAR, matriz de teclado `byte[8]`, EAR de entrada), `EndFrame()` (resta `TStatesPerFrame`), `Reset()`. Fábrica con los presets 16K/48K/128K/+2. Bus `SpectrumBus`: `readonly struct` con una sola referencia a la máquina, implementa `IZ80Bus`, métodos `[AggressiveInlining]`:
   - Contención: `t += contended[addr >> shift] ? table[t] : 0` con `Unsafe.Add`.
   - E/S: tabla de 5.4 con "byte alto contenido" = test de contención de memoria sobre `port`. Puerto 0xFE si A0 = 0. Con paginación 128K: 0x7FFD si A15 = 0 y A1 = 0 (escritura → `Write7FFD`; lectura → bus flotante). Resto → bus flotante.
   - `IntActive = TStates < InterruptLength`; `Cycles` = `TStates`.
   - Sin delegados, LINQ, `HasFlag` ni asignaciones.
   - Si el benchmark del paso 3 lo requiere, un segundo struct `Spectrum48Bus` sobre `FlatMemory64K`.
9. **Proyecto de tests xUnit** — `ZXSinclair.Net.Core.Tests` (`dotnet new xunit`, `net10.0`, referencia al Core, añadido a `zxsinclair.net.slnx`):
   - Memoria: cada preset (16K, 48K, 128K, ZX81 1K, ZX81 16K) — ROM protegida, huecos leen 0xFF y no guardan escrituras, espejos, `LoadRom`, `CopyFrom`/`CopyTo`.
   - Paginación 128K: estado tras reset, banco en 0xC000, alias del banco 5 (0x4000/0xC000) y del 2 (0x8000/0xC000), selección de ROM, bloqueo, pantalla sombra, contención de bancos impares.
   - Contención: 48K en 14 335…14 343, 0 en 14 463, segunda línea +224, nada tras la línea 192; 128K en 14 361 con período 228; late timing +1; 16K igual que 48K.
   - Bus: costes de `FetchOpcode`/`Read`/`Write`/`Internal` con y sin contención; las cuatro secuencias de E/S de 5.4.
   - Bus flotante: 14 338 → bitmap 0x4000, 14 339 → atributo 0x5800, 14 340 → 0x4001, 14 341 → 0x5801, 14 342 → 0xFF; fuera de pantalla 0xFF; 128K desde 14 364 y con pantalla sombra.
   - Puerto 0xFE e `IntActive`/`EndFrame`.
10. **Benchmark del bus** — `ZXSinclair.Net.Benchmarks/BusBenchmarks.cs` (referencia al Core): `Run<TBus>(TBus bus) where TBus : struct, IZ80Bus` con lecturas/escrituras para 48K y 128K, frente a `ArrayUnsafeMemory`, para medir el coste de la contención y confirmar que no hay despacho por interfaz.
11. **Documentación** — actualizar `CLAUDE.md` y `AGENTS.md` (contenido del Core, abstracciones genéricas, memoria configurable con presets, patrón bus struct + máquina sealed, `dotnet test ZXSinclair.Net.Core.Tests`) y la sección 9 de la spec con las decisiones y resultados; mantener la sección 10 con lo pendiente.
12. **Verificación**:
    - `dotnet build zxsinclair.net.slnx -c Debug` y `-c Release` sin errores ni advertencias nuevas.
    - `dotnet test ZXSinclair.Net.Core.Tests` en verde (Debug y Release).
    - `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Bus*' '*MemoryAccess*'`: el bus 48K sin contención cerca de `ArrayUnsafeAdd` (~1 ns/op) y la memoria por páginas claramente mejor que la paginada actual (2.17×).
    - `dotnet run --project ZXSinclair.Net.Test -c Debug` sigue igual (el legado no se toca).

## Riesgos / cosas a vigilar
- Generalizar la memoria (tabla de páginas) puede costar velocidad frente al array plano; el paso 3 decide con datos si 48K necesita su propia memoria plana.
- Mapas del ZX81 (espejos, A15 sin decodificar) y del 16K sin confirmar del todo: marcarlos **(verificar)** y cubrirlos con tests fáciles de ajustar.
- Puntos **(verificar)** de la spec (INT de 36 T en 128K, lectura de 0x7FFD, contención de E/S en ranura 3, coste de `AcknowledgeInterrupt`): usar el valor de FUSE en una constante con nota.
- El momento de muestreo del bus flotante y de la lectura de puertos dentro del ciclo de E/S afecta a juegos como *Arkanoid*; seguir FUSE y cubrirlo con tests.
- Si el bus struct se pasa como interfaz (boxing) se pierde todo el beneficio: usarlo solo como argumento genérico restringido a struct.
- La tabla de contención necesita margen al final del frame; documentar que la máquina llama a `EndFrame()` antes de agotarlo.
- Sin CPU en el Core, la validación con los eventos FUSE (9.5) queda para cuando se porte la CPU.

## Fuera de alcance
- CPU Z80 en el Core y migración del `Z80Cpu` legado.
- Bus y temporización del ZX81 (vídeo por NMI/WAIT, ejecución por encima de 32K): solo se implementa su mapa de memoria.
- +2A/+3 (puerto 0x1FFD, patrón 1,0,7,…).
- Generación de vídeo, borde, efecto nieve, sonido y carga de cintas.
- Modelo de hilos / sincronización CPU–interfaz (el antiguo `SyncBus`).
- Implementaciones de buses de otras CPU: solo se deja el contrato genérico preparado.
