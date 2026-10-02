# Plan: comparar los eventos de bus de los tests FUSE

Fecha: 2026-10-02
Estado: implementado y verificado

## Objetivo
Que el runner de tests compare, además de registros, memoria y T-states, la secuencia de eventos de bus (`MC`, `MR`, `MW`, `PC`, `PR`, `PW`) que FUSE incluye en `tests.expected`, para verificar que cada instrucción hace exactamente los ciclos de bus correctos.

## Contexto
- `ZXSinclair.Net.Test/data/tests.expected` trae, antes de los registros, los eventos de bus de cada test con su T-state, dirección y dato (7560 MC, 4425 MR, 559 MW, 77 PC, 36 PR, 34 PW).
- El parser ya existe: `clsTestEvent.Read/ReadEvents` en `ZXSinclair.Net.Test/clsTests.cs`; `readTestsExpected()` en `Program.cs` rellena `clsTestExpected.Events`. Pero `CompareTest` (`Program.cs:205`) **nunca mira `Events`** y el `Z80Cpu` no registra nada.
- Semántica FUSE (deducida de los datos):
  - M1: `t MC pc` y luego `t+4 MR pc op`. Lectura: `t MC a`, `t+3 MR a v`. Escritura: `t MC a`, `t+3 MW a v`.
  - Ciclos internos con dirección en el bus: un `MC addr` por cada T-state (p. ej. `09` ADD HL,BC → 7 × `MC 0001` sobre IR; `LD r,(IX+d)` → 5 × `MC pc`).
  - E/S: `PC` según la tabla de contención de E/S del 48K (`d3_2`: `7 PC 42ec`, `8 PW 42ec 42`, `8 PC 42ec`).
- Hoy los ciclos internos se suman con `Ticks.AddCycles(n)` **sin dirección** (`Z80Cpu.cs:170,181,231,259,265,270` y el código que emite el generador en `ZXSinclair.Net.Generate.Z80OpCodes/Program.cs:201` → `Z80Cpu.opcodesdd.cs`/`opcodesfd.cs`), así que no pueden producir los `MC` esperados.
- Cualquier `Debug.Assert` fallido termina el proceso: el primer fallo impide ver el resto.
- Decisión: se implementa sobre el `Z80Cpu` legado porque es la única CPU que existe; el registro y la comparación de eventos se diseñan para reutilizarse cuando exista la CPU de `ZXSinclair.Net.Core`. Todo el registro va bajo `#if Z80_OPCODES_TEST`, sin coste en Release (regla de rendimiento del proyecto).

## Pasos
1. **Tipo de evento compartido** — `ZXSinclair.Net/Hardware/Z80/Z80BusEvent.cs` (nuevo, con cabecera GPL): `readonly record struct Z80BusEvent(int Time, Z80BusEventType Type, ushort Address, byte? Data)` y enum `Z80BusEventType { MC, MR, MW, PC, PR, PW }`. `ToString()` con el formato de FUSE (`"%5d MC 0000"`) para los mensajes de error.
2. **Registro en `Z80Cpu`** (`ZXSinclair.Net/Hardware/Z80/Z80Cpu.cs`), todo dentro de `#if Z80_OPCODES_TEST`:
   - Campo `public List<Z80BusEvent>? BusEvents;` (null = no registrar).
   - `ReadOpCode`: `MC` antes de sumar 4, `MR` después. `ReadMemory`: `MC` antes de sumar 3, `MR` después. `WriteMemory`: `MC` antes, `MW` después.
   - `Reset()` vacía la lista si existe.
3. **Ciclos internos con dirección** — nuevo método no virtual en `Z80Cpu`: `[AggressiveInlining] void InternalCycles(ushort address, int tstates)` que suma los T-states y, bajo `Z80_OPCODES_TEST`, registra un `MC address` por cada T-state. Sustituir los `Ticks.AddCycles(n)` internos:
   - `Read_M_IX_PLUS_D_M` / `Read_M_IY_PLUS_D_M`: `InternalCycles(PC del byte d, 5)` según el `MC` esperado en los tests `dd 46`… (comprobar la dirección exacta en `tests.expected`).
   - `LD_A_I`, `LD_A_R`, `LD_I_A`, `LD_R_A`: `InternalCycles(Regs.IR, 1)`.
   - Generador: `GenerateZ80CpuIXIYBefore` (`Generate.Z80OpCodes/Program.cs:~201`) emite `InternalCycles(...)` en lugar de `Ticks.AddCycles(5)`; regenerar `Z80Cpu.opcodesdd.cs`/`opcodesfd.cs` ejecutando el generador (no editar a mano).
   - La dirección correcta de cada caso se toma de las líneas `MC` del test correspondiente.
4. **Runner** (`ZXSinclair.Net.Test/Program.cs`):
   - `RunTests`: asignar `z80.BusEvents = new List<Z80BusEvent>(64)` una vez; `Reset` la vacía en cada test.
   - Convertir `clsTestEvent` a `Z80BusEvent` (método `ToBusEvent()` en `clsTests.cs`, mapeando el string de tipo al enum).
   - `CompareTest`: comparar la secuencia completa (misma longitud, mismo orden, mismo time/type/address/data).
   - Sustituir los `Debug.Assert` de comparación por comprobaciones que **acumulan fallos** (nombre del test + primer campo/evento distinto, con esperado vs obtenido) y siguen con el siguiente test. Al final, resumen `pasados / fallidos / omitidos (no implementados)` y código de salida ≠ 0 si hay fallos. Los asserts de parseo de `clsTests.cs` se quedan.
   - Opción `--no-events` (o constante) para desactivar la comparación de eventos.
5. **Documentación**: actualizar la sección de tests de `CLAUDE.md` y `AGENTS.md` (y la 9.5 de `Specs/spec-buses-memoria.md`): el runner compara eventos de bus, informa de fallos sin abortar y los ciclos internos deben usar `InternalCycles(dirección, n)`, nunca `Ticks.AddCycles`.
6. **Verificación**:
   - `dotnet build zxsinclair.net.slnx -c Debug` y `-c Release` (Release compila sin el código de registro).
   - `dotnet run --project ZXSinclair.Net.Test -c Debug`: los opcodes implementados (LD r,r', LD r,n, LD r,(HL), LD (IX+d),…, LD A,I…) pasan también la comparación de eventos; el resumen muestra los omitidos.
   - Prueba negativa: alterar temporalmente un `InternalCycles` y comprobar que el runner informa el test y el evento distinto sin abortar; revertir.
   - Benchmark de memoria en Release para confirmar que no cambia.

## Riesgos / cosas a vigilar
- Al activar la comparación, tests que hoy "pasan" fallarán por ciclos internos mal direccionados; es el objetivo, pero cada caso se revisa contra `tests.expected`, no se ajusta a ojo.
- El orden de eventos importa (`PW` antes del `PC` tardío en E/S). La E/S no está implementada; el diseño debe soportarla pero no se valida aún.
- `Z80_OPCODES_TEST` también está definido en Debug del proyecto `ZXSinclair.Net`: el registro solo actúa si `BusEvents != null`.
- El generador sobrescribe los ficheros de opcodes: los cambios en los `.cs` generados se hacen en `Program.cs`/plantillas del generador.

## Fuera de alcance
- Implementar opcodes nuevos, E/S (IN/OUT) o contención real con retrasos de la ULA (los tests FUSE solo registran `MC`/`PC`, sin retraso).
- Portar la CPU a `ZXSinclair.Net.Core`.
- Migrar a un framework de tests (xUnit/NUnit).
## Resultado de implementación

- Debug y Release compilan sin errores. El runner rechaza Release con código 2 porque el parser conserva sus aserciones Debug.
- FUSE en Debug: 116 pasados, 0 fallidos, 1219 omitidos; mismo resultado con `--no-events`.
- Prueba negativa: una dirección alterada en `InternalCycles` produce 30 fallos identificados, continúa hasta el resumen y devuelve código 1. La alteración se revirtió; la ejecución final vuelve a pasar.
- Los fixtures `ed47` y `ed4f` exigen registrar el ciclo con `IR` antes de modificar I/R. Las lecturas `ed57` y `ed5f` usan el IR actual. El manual Z80 confirma 9 T-states para las cuatro instrucciones.
- La inspección de metadatos confirma que `BusEvents` existe en Debug y está ausente en Release.
- Benchmark Release `CurrentMemoryBufferInterface`: 1,567 ns antes y 1,310 ns después, sin asignaciones gestionadas. Tres iteraciones, una de calentamiento y un lanzamiento: la variabilidad no permite afirmar una mejora ni demostrar equivalencia estadística. Informes en `ZXSinclair.Net.Benchmarks/bin/fuse-verification-artifacts/`.
- Verificados los encabezados de licencia de los 42 archivos C# del repositorio y las diferencias de espacios; no se crearon commits.
