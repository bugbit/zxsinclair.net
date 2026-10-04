# Plan: correcciones de comportamiento no documentado

Fecha: 2026-10-04
Estado: implementado y verificado (2026-10-04); test `BlockIoRepeat_WzVisibleThroughBitHl` descartado por inobservable (ver la spec 4.2)

## Objetivo
Aplicar las dos correcciones de `Specs/spec-correcciones-no-documentado.md`: que `SCF`/`CCF` con prefijo `DD`/`FD` vean `Q = 0`, y que `INIR`/`INDR`/`OTIR`/`OTDR` pongan `WZ = PC + 1` cuando repiten. Ambas se cubren con tests propios, porque FUSE no las detecta.

## Contexto
- La CPU está completa (grupos 0–11 commiteados en `b90c304`; FUSE 1335/0/0).
- La comparación de `Docs/z80-no-documentado.md` (sin commit, junto con la spec) detectó dos huecos:
  1. `ExecuteIndexed<TIndex>` (`ZXSinclair.Net.Core/Z80/Z80Cpu.cs`, ~90) no toca `Q`, así que `DD 37`/`FD 3F` usan el `Q` de la instrucción anterior al prefijo. `SetCarry`/`ComplementCarry` (`Z80Cpu.Control.cs`) leen `Registers.Q`.
  2. `BlockInCore`/`BlockOutCore` (`Z80Cpu.Io.cs`, ~66–102) dejan `WZ = BC ± 1` también cuando repiten.
- Tests afectados:
  - `IoTests.BlockIo_AccessesWzWrapAndPrefixes`: el WZ esperado (línea ~258) no distingue la repetición;
  - `IoTests.RepeatUntilBZeroAndInterruptResume` y `Repeat_FlagsSeenByInterruptAt2800`;
  - `ControlTests.Control_AllAliasesPrefixesAndPcWrap_PreserveRegistersAndCycles`, que incluye `DD 37`/`FD 3F`. Con el `Q = 0` inicial no cambia, pero conviene revisarlo.
- No hay casos FUSE `dd37`/`fd3f`.

## Pasos
1. **`Q` tras prefijo** — `Z80Cpu.cs`, `ExecuteIndexed<TIndex>`: tras el bucle de prefijos y antes de `FinishIndexed`, añadir `Registers.Q = 0;`, con un comentario breve que remita a la spec 2.
2. **WZ al repetir** — `Z80Cpu.Io.cs`, `BlockInCore` y `BlockOutCore`: en la rama de repetición, tras `Registers.PC -= 2`, añadir `Registers.WZ = (ushort)(Registers.PC + 1);`.
3. **Tests de `SCF`/`CCF`** — `ControlTests.cs`:
   - `ScfCcf_AfterIndexPrefix_UseQZero`: `A = 00`, `CP 28h` y después `DD 37`/`FD 37`/`DD 3F`/`FD 3F`. Espera F5/F3 = `0x28`, 8 T y `Q = F`.
   - `ScfCcf_WithoutPrefix_KeepPreviousQ`: `CP 28h` seguido de `SCF`/`CCF`. Espera F5/F3 = `0x00`.
   - `ScfCcf_AfterPrefixChain_UseQZero`: `DD FD 37`, 12 T.
   - `IndexPrefix_OtherInstructionsSetQAsBefore`: `DD 84`, `FD 21 nn` y `DD CB d 06`.
4. **Tests de WZ** — `IoTests.cs`:
   - `BlockIoRepeat_WzIsPcPlusOne`: las 4 instrucciones con el `ED` en `0x2800` y en `0xFFFF`; espera `WZ = 0x2801` / `0x0000`.
   - `BlockIoFinalIteration_WzClassic`: `B = 1`.
   - `BlockIoRepeat_WzVisibleThroughBitHl`: `INIR` que repite en `0x2800`, INT con IM 1 y `BIT 0,(HL)` en `0x0038`; espera F5/F3 = `0x28`.
5. **Adaptar tests**:
   - `BlockIo_AccessesWzWrapAndPrefixes`: `expected.WZ = repeat ? (ushort)(expected.PC + 1) : <clásico>`, calculado después de `expected.PC -= 2`.
   - Revisar `RepeatUntilBZeroAndInterruptResume` y `Repeat_FlagsSeenByInterruptAt2800`, por si comparan WZ.
   - Revisar `Control_AllAliasesPrefixes…` en los casos con prefijo.
6. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings.
   - `dotnet test ZXSinclair.Net.Core.Tests` y `dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests` en verde.
   - `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check` → 0 (el código generado no cambia).
   - Runner FUSE en 1335/0/0.
7. **Benchmark** — repetir `Z80CpuBenchmarks` antes y después del paso 1, en especial `ExecuteAluLoopFrame`, que usa `ADD A,(IX+5)`. Anotar el resultado en `Specs/spec-cpu-z80.md` 8.4.
8. **Documentación**:
   - `Specs/spec-instr-control.md` 2.2: el prefijo anula `Q` para `SCF`/`CCF`, con la fuente.
   - `Specs/spec-instr-io.md` 2.3 y 8: regla `WZ = PC + 1` al repetir, quitando el "verificar".
   - `Docs/z80-no-documentado.md` 8: marcar ambas filas como implementadas.
   - `CLAUDE.md`/`AGENTS.md`: quitar "two known gaps" y dejar la referencia a la spec de correcciones.

## Riesgos / cosas a vigilar
- `Q = 0` en `ExecuteIndexed` también se aplica a las rutas `DDCB` y `DD ED`. Todas sus instrucciones reescriben `Q` al terminar, así que su resultado no cambia (lo cubre `IndexPrefix_OtherInstructionsSetQAsBefore`).
- WZ al repetir debe calcularse con el PC ya decrementado y solo en la rama que repite; la iteración final conserva `BC ± 1`.
- La convención FUSE de cargar `Q = F` no interviene: no hay fixtures con prefijo + `SCF`/`CCF`.
- Decidir con el usuario si el commit va separado del documento de no documentado y de la spec, o junto con ellos.

## Fuera de alcance
- Variantes NEC/CMOS de `SCF`/`CCF`.
- Integración de z80test 1.2a.
