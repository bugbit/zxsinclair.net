# Plan: informe detallado de fallos FUSE

Fecha: 2026-10-02
Estado: implementado y verificado

## Objetivo
Que el runner FUSE explique cada fallo con todas sus diferencias (registros, flags bit a bit, memoria, eventos con contexto y T-states) en lugar de solo la primera, y que el proceso de implementación de instrucciones use ese informe para diagnosticar los fallos.

## Contexto
- Cuando un caso FUSE falla, el runner (`ZXSinclair.Net.Test/Program.cs`) imprime una sola línea con **la primera** diferencia:
  - `FuseCpuState.Compare` (`ZXSinclair.Net.Fuse/FuseCpuState.cs`) para en el primer registro distinto;
  - `FuseComparison.CompareMemory` (`ZXSinclair.Net.Fuse/FuseComparison.cs`) para en el primer byte;
  - `FuseComparison.CompareEvents` para en el primer evento, y los eventos ni se comparan si fallan registros o memoria.
- Con los fallos del grupo 3 (`bus event 3: expected [<missing>] … (counts 3/4)`) hubo que abrir los ficheros FUSE a mano para entender que eran lecturas que FUSE modela solo como contención.
- `FuseTestBusTests` usa `Compare`, `CompareMemory` y `CompareEvents`; conviene mantener esas firmas.
- `Unimplemented()` (`ZXSinclair.Net.Core/Z80/Z80Cpu.cs`) solo incrementa `UnimplementedOpcodes`; el runner no sabe qué opcode provocó la omisión.

## Pasos
1. **Modelo de diferencias** — nuevo `ZXSinclair.Net.Fuse/FuseMismatch.cs`: `enum FuseMismatchKind { Register, Flags, State, TStates, Memory, Event }` y `record FuseMismatch(Kind, string Field, string Expected, string Actual, string? Detail)`.
2. **Registros completos** — `FuseCpuState.Diff(expected, in actual, cycles)` devuelve **todas** las diferencias (pares, I, R, IFF1/2, IM, halted y T-states).
   - Para AF y AF' separa A de F y añade una entrada `Flags` con F decodificado por bits (`S Z 5 H 3 P/V N C`), por ejemplo `expected 44 (Z P/V), actual 40 (Z) → difiere P/V`.
   - Para R indica si difieren solo los 7 bits bajos o el bit 7.
   - `Compare` se mantiene como envoltorio (primera diferencia formateada) para no romper `FuseTestBusTests`.
3. **Memoria completa** — `FuseComparison.DiffMemory` devuelve todas las direcciones distintas, agrupadas en rangos contiguos (`5d2f-5d30: expected 8d 12, actual 00 00`), con un límite configurable (por defecto 16 rangos más "y N más"). `CompareMemory` queda como envoltorio.
4. **Eventos con contexto** — `FuseComparison.DiffEvents` localiza el primer evento distinto y devuelve:
   - una ventana alineada (3 eventos antes y 3 después) en dos columnas, esperado y real, marcando la línea divergente;
   - el total de eventos de cada lado;
   - una pista del tipo de discrepancia: `MR` de más tras un `MC` (lectura que FUSE modela como contención; ver `ReadDiscarded`), evento que falta o sobra al final, mismo tipo con distinto tiempo (contención o ciclos internos), o distinta dirección o dato.

   `CompareEvents` queda como envoltorio.
5. **Informe por caso** — nuevo `ZXSinclair.Net.Fuse/FuseReport.cs`. Para un caso fallido compone el nombre, los bytes del programa en el PC inicial (hasta 4) y el estado inicial (líneas de registros de `tests.in`), y después las secciones Registros, Flags, Memoria, Eventos y T-states con todas sus diferencias. Se comparan **siempre** las cuatro secciones: no se saltan los eventos aunque fallen registros.
6. **Runner** — `ZXSinclair.Net.Test/Program.cs` usa `FuseReport`. Opciones nuevas:
   - `--filter <prefijo>`: solo los casos cuyo nombre empieza por el prefijo (por ejemplo `20` o `dd`).
   - `--verbose`: secuencia completa de eventos esperada y real de los fallidos.
   - `--max-failures <n>`: detalle solo de los n primeros; el resto, en una línea.
   - `--list-skipped`: casos omitidos con el opcode no implementado que los provocó.

   Al final, además del resumen actual, un resumen de fallos por sección (registros, flags, memoria, eventos, T-states) y por prefijo de opcode. Se mantienen `--no-events` y el código de salida 1.
7. **Opcode no implementado** — para `--list-skipped`, añadir a `Z80Cpu` una propiedad `LastUnimplementedAddress` (PC tras el fetch del opcode no implementado) que se escribe **solo** dentro de `Unimplemented()`, en el camino frío y sin coste en el bucle caliente; `Reset()` la limpia. El runner muestra los bytes de memoria anteriores a esa dirección (prefijos y opcode).
8. **Tests** — en `ZXSinclair.Net.Core.Tests/FuseTestBusTests.cs` o en un nuevo `FuseReportTests.cs`:
   - varias diferencias de registros a la vez;
   - decodificación de flags;
   - rangos de memoria y su límite;
   - ventana de eventos con la pista de `MR` de más (fixture `20_2` leído con `Read` en vez de `ReadDiscarded`);
   - sección de eventos presente aunque fallen registros;
   - `LastUnimplementedAddress`.

   Los tests actuales de `Compare*` siguen pasando.
9. **Proceso de instrucciones** — `Specs/spec-proceso-instrucciones.md`:
   - §3 (ciclo por grupo, verificación): ante un fallo FUSE, ejecutar `dotnet run --project ZXSinclair.Net.Test -- --filter <caso> --verbose`, adjuntar el informe a la revisión y clasificar la causa (semántica, temporización o convención de FUSE) antes de tocar código o spec. Los planes de cada grupo incluyen en su verificación una ejecución con `--filter` de los casos del grupo.
   - §4 (estructura de spec de grupo): la sección de pruebas FUSE debe listar las convenciones de FUSE conocidas que afectan al grupo (por ejemplo, `ReadDiscarded` en saltos no tomados).
10. **Documentación** — `Specs/spec-cpu-z80.md` 8.1 (formato del informe y opciones del runner), y `CLAUDE.md` y `AGENTS.md` (comandos con `--filter`, `--verbose` y `--list-skipped`, y la regla del paso 9).
11. **Verificación**:
    - `dotnet build zxsinclair.net.slnx` sin warnings nuevos; `dotnet test ZXSinclair.Net.Core.Tests` en verde.
    - `dotnet run --project ZXSinclair.Net.Test` da el mismo resumen que hoy (280 / 0 / 1055).
    - Fallo provocado temporalmente (por ejemplo, cambiar `ReadDiscarded` por `Read` en `JumpRelative`, sin commitear): el informe de `20_2` muestra la ventana de eventos y la pista de `MR` de más. Revertir después.
    - `--filter dd`, `--list-skipped` y `--max-failures 1` producen la salida esperada.
    - Benchmark `*Z80Cpu*` sin cambios.

## Riesgos / cosas a vigilar
- Volumen de salida con muchos fallos: limitarlo con `--max-failures` y el límite de rangos de memoria.
- `LastUnimplementedAddress` no debe añadir trabajo al bucle caliente: solo se escribe dentro de `Unimplemented()`.
- Mantener las firmas actuales de `Compare*` evita reescribir los tests existentes.

## Fuera de alcance
- Desensamblador de mnemónicos en el informe: se muestran bytes. Un desensamblador a partir del modelo del generador queda para otro plan.
- Informe HTML o integración con CI.

## Resultado
- Build Debug: 0 errores y 0 avisos. Core: 723 tests en verde. Runner con eventos y con `--no-events`: 280 pasados, 0 fallos y 1055 omitidos.
- `--filter dd --list-skipped`: 343 seleccionados, 50 pasados y 293 omitidos, con dirección y contexto del último opcode pendiente.
- Fallo temporal en `JumpRelative`: `--filter 2 --verbose --max-failures 1` dio 7 pasados, 2 fallos y 10 omitidos, código 1. El primer fallo mostró la ventana y la pista `MR`/`ReadDiscarded`; el segundo se resumió en una línea. Archivo restaurado byte por byte. Evidencia local en `ZXSinclair.Net.Test/bin/fuse-report-checks/`.
- Benchmark sin cambios: NOP 2.1597 ns/opcode y bucle 0.6537 ns/T-state, 0 B en ambos. Intervalos solapados con la medición anterior; resultados en spec CPU 8.4.
- Decisiones de CLI: detalle de 10 fallos por defecto; `--max-failures 0` muestra solo líneas breves; argumentos inválidos devuelven código 2. Los rangos de memoria se limitan por la API, con 16 por defecto. El contexto de omitidos muestra los cuatro bytes anteriores al PC guardado, sin desensamblar.
