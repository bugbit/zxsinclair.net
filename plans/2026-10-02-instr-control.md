# Plan: grupo 5, aritmética general y control, y registro Q

Fecha: 2026-10-02
Estado: implementado y verificado

## Objetivo
Implementar el grupo 5 (`DAA`, `CPL`, `NEG`, `SCF`, `CCF`, `HALT`, `DI`, `EI`, `IM` y alias de ED) y el registro interno `Q` según `Specs/spec-instr-control.md`, con 27 casos FUSE nuevos pasando y el coste de `Q` medido.

## Contexto
- Grupos 0–4 implementados; el runner FUSE pasa 429 casos.
- `Q` existe en `Z80Registers`, pero nadie lo escribe.
- `IPattern` (`ZXSinclair.Net.Generate.Z80OpCodes/Patterns/IPattern.cs`) tiene `Matches` y `EmitBody`; los patrones están en `Control.cs` (NOP), `Load8.cs`, `Load16.cs`, `Jumps.cs` y `Alu8.cs`.
- `DispatchEmitter` (`Emit/DispatchEmitter.cs`) agrupa por (cuerpo, comentario) y tiene un **atajo de NOP** en la tabla base: si el cuerpo de `0x00` está vacío, emite `if (opcode == 0) return;` antes de `ExecuteMainDispatch`.
- `Z80Cpu.Interrupts.cs` tiene `AcceptNmi`, `AcceptInterrupt` y `ExitHalt`; el M1 de HALT está en `Step()` (`Z80Cpu.cs`). `Sub8` está en `Z80Cpu.Alu8.cs`.
- `FuseCpuState.Load` no inicializa `Q`.
- Estado inicial de los tests: `Q = 0` en Nop/Load8/Load16 y `Q = 0xAA` en `Alu8Tests`.
- `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset` usa `ED 44` (`NEG`) como opcode no implementado.

## Pasos
1. **Q en el generador**:
   - `IPattern`: añadir `bool WritesFlags(Opcode opcode) => false;` (miembro por defecto de interfaz). Sobrescribirlo a `true` en `Alu8Pattern`, `IncDec8Register`, `IncDec8Memory` y `LoadAccumulatorSpecial`.
   - `DispatchEmitter`: tras `EmitBody`, añadir `Registers.Q = Registers.F;` o `Registers.Q = 0;` al cuerpo emitido, guardando aparte el cuerpo del patrón para los tests.
   - Adaptar el atajo de NOP: detectar el cuerpo vacío **del patrón** y emitir `if (opcode == 0) { Registers.Q = 0; return; }`.
2. **Q en el Core** — `Registers.Q = 0` en `AcceptNmi`, `AcceptInterrupt` y en la rama de HALT de `Step()`.
3. **Auxiliares** — nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Control.cs` (cabecera GPL, `partial`, `AggressiveInlining`): `DecimalAdjust`, `Complement`, `Negate` (vía `Sub8`), `SetCarry`, `ComplementCarry` (con `Q`), `Halt` y `EnableInterrupts`, según la spec 2.1 y 4.2.
4. **Patrones** — `Patterns/Control.cs`:
   - Con `WritesFlags = true`: `DecimalAdjustPattern`, `ComplementPattern`, `NegatePattern`, `SetCarryPattern` y `ComplementCarryPattern`.
   - Sin escribir flags: `HaltPattern`, `DisableInterruptsPattern`, `EnableInterruptsPattern` e `InterruptModePattern` (`Registers.IM = n;` a partir de `Operand.Value`).

   Registrarlos en `PatternCatalog.Default`.
5. **FUSE** — `ZXSinclair.Net.Fuse/FuseCpuState.cs`: `Load` pone `Q = F` (convención de FUSE para `SCF`/`CCF`), con un comentario que remita a la spec 5.1.
6. **Regenerar** — `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --verbose`; cobertura base 230, ED 36, DDFD 230. Revisar el diff: cada `case` termina con la escritura de `Q`, `NEG` lleva 8 etiquetas e `IM 0`, 4.
7. **Tests del generador** — `GeneratorTests.cs`:
   - Cifras: 230/36/230; `230 implemented / 22 pending / 4 prefixes`; `36 implemented / 42 pending`.
   - Ajustar las aserciones de texto de los `case` (NOP y muestras) a la escritura de `Q`.
   - Nuevos `Patterns_WritesFlagsMatchesSpec` (los patrones que deben devolver `true` lo hacen y el resto devuelve `false`) y `Dispatch_AppendsQWrite`.
8. **Adaptar tests del Core**:
   - `Alu8Tests` (estado inicial `Q = 0xAA`): esperar `Q = F` tras cada operación.
   - `Load8Tests`: `Q = F` tras `LD A,I`/`LD A,R`.
   - El resto de tests que comparan `Z80Registers` completo esperan `Q = 0`, que ya es el valor inicial.
   - `FuseReportTests.LastUnimplementedAddress_TracksFetchAndReset`: `ED 44` → `ED 00`.
   - Revisar `FuseTestBusTests`/`FuseReportTests` que usan `FuseCpuState.Load`.
9. **Tests propios** — nuevo `ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs` con los 17 tests de la spec 5.2: `DAA` exhaustivo con modelo de referencia independiente, `SCF`/`CCF` con `Q`, `Q` por patrón de los grupos 1–5, `HALT`, `DI`/`EI` e `IM`.
10. **Verificar**:
    - `dotnet build zxsinclair.net.slnx` sin warnings nuevos.
    - `dotnet test ZXSinclair.Net.Core.Tests` y `dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests` en verde.
    - `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check` → 0.
    - Runner con `--filter 27`, `2f`, `3`, `76`, `f` y `ed4` a `ed7`; total 456 pasados y 0 fallos. Ante un fallo, `--filter <caso> --verbose`.
11. **Benchmark** — repetir `ExecuteFrame`, `ExecuteLoopFrame` y `ExecuteAluLoopFrame` en Release antes de `Q` (rama actual) y después, y anotar la diferencia en `Specs/spec-cpu-z80.md` 8.4. Si la regresión supera el ruido, probar a mover `Q = F` dentro de los auxiliares de flags y quedarse con la variante exacta más rápida.
12. **Documentación**:
    - `Specs/spec-cpu-z80.md` 3 y 10 (`Q` resuelto), 8.1 y 8.4.
    - `Specs/spec-proceso-instrucciones.md`: grupo 5 Implementado, 27 casos, 456 acumulados; regla nueva: cada spec de grupo indica qué patrones escriben flags.
    - `Specs/spec-generador-z80.md`: `WritesFlags` y escritura de `Q` en 3.2/3.3.
    - `CLAUDE.md`/`AGENTS.md`.

## Riesgos / cosas a vigilar
- El atajo de NOP depende de que el cuerpo esté vacío: si no se adapta, se pierde la optimización o NOP deja de escribir `Q`.
- Agrupación de `case`: añadir `Q` cambia la clave de grupo. Los alias siguen compartiendo cuerpo, pero hay que comprobar que no se rompe ninguna agrupación existente.
- `SCF`/`CCF` leen `Q` **antes** de que el generador escriba `Q = F` al final del `case`: el orden (cuerpo y después `Q`) es el correcto.
- `HALT`: `PC -= 1` tras el fetch; la salida por interrupción ya hace `PC++`.
- `FuseCpuState.Load` con `Q = F` no debe alterar las comparaciones (FUSE no compara `Q`).
- Coste de `Q` en todas las instrucciones: medir antes y después.

## Fuera de alcance
- IM 0 con instrucciones arbitrarias y huecos de ED (grupo 11).
- Peculiaridad NMOS de P/V tras `LD A,I` con INT.
- Valor definitivo de `Q` tras `POP AF`/`EX AF,AF'`.

## Resultado (2026-10-02)

- Q, auxiliares, nueve patrones de control y convención Q=F de FUSE implementados. Dispatches regenerados: 230/36/230.
- Build Debug: 0 errores y 0 warnings. Core: 1300 pruebas; generador: 101. `--check`: 0. Runner con eventos: 456 pasados / 0 fallos / 879 omitidos, incluido el filtrado por bloques.
- Comparación antes/después: NOP 2.1257→2.1234 ns/opcode; bucle 0.6532→0.6585 ns/T-state; ALU 0.6821→0.6905 ns/T-state. 0 B en todos; intervalos solapados. Se conserva Q generado; no hizo falta moverlo dentro de los auxiliares. Entorno e intervalos en spec CPU 8.4.
- La prueba de SCF tras una ALU usa CP 28 con A=0 para mantener A y generar F3/F5, en vez de XOR A que borraría ambos bits. DAA se amplió a A×F completos y SCF/CCF a una matriz exhaustiva. Se actualizaron 11 entradas de la tabla: 88 de 150 completadas.
- Sin commits. Pendientes externos: z80ccf para POP AF/EX AF,AF', alias IM 0/1 y DD/FD HALT; WebAssembly.
