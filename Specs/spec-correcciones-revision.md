# Especificación: correcciones de la revisión del código de instrucciones

Tres correcciones salidas de la revisión de código de `ZXSinclair.Net.Core/Z80` y `ZXSinclair.Net.Generate.Z80OpCodes` (2026-10-04):

1. **Paso de repetición de bloque duplicado**: la lógica común de `LDxR`, `CPxR`, `INxR` y `OTxR` cuando repiten está copiada en cuatro sitios.
2. **Regla errónea en la documentación**: `Docs/z80-no-documentado.md` describe mal F5/F3 de `BIT b,r`.
3. **Inlining de la tabla base generada**: 252 auxiliares `ExecuteMainXX` con `AggressiveInlining` pueden agotar el presupuesto de inlining del JIT en `ExecuteMainDispatch`.

Ninguna cambia el comportamiento emulado: los casos FUSE (1335/0/0) y todos los tests existentes deben seguir pasando sin cambios en sus expectativas.

Lo no confirmado se marca **(verificar)**.

## 1. Paso de repetición de bloque

### 1.1 Situación actual

Cuando una instrucción de bloque repite, la CPU hace un ciclo extra de 5 T y retrocede PC. Hoy cada auxiliar lo escribe a mano:

| Auxiliar (fichero) | Ciclo interno | `PC -= 2` | `WZ = PC + 1` | F5/F3 de PC |
|---|---|---|---|---|
| `BlockLoadRepeat` (`Z80Cpu.Block.cs`) | `Internal(DE anterior, 5)` | sí | sí | sí, con `0x28` literal |
| `BlockCompareRepeat` (`Z80Cpu.Block.cs`) | `Internal(HL anterior, 5)` | sí | sí | sí, con `0x28` literal |
| `BlockInCore` (`Z80Cpu.Io.cs`) | `Internal(HL anterior, 5)` | sí | sí | dentro de `BlockIoRepeatFlags`, con `0x28` literal |
| `BlockOutCore` (`Z80Cpu.Io.cs`) | `Internal(BC, 5)` | sí | sí | dentro de `BlockIoRepeatFlags` |

La duplicación ya causó un fallo real: hasta el commit `6a43a30` las repetitivas de E/S no ponían `WZ = PC + 1`, mientras que `LDxR`/`CPxR` sí lo hacían.

### 1.2 Cambio

- Nuevo auxiliar en `Z80Cpu.Block.cs`, con `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:

  ```csharp
  // Extra 5 T-state M-cycle of a repeating block instruction (Specs/spec-instr-bloques.md 2.2-2.3, spec-instr-io.md 2.2-2.3).
  private void RepeatBlock(ushort internalAddress)
  {
      bus.Internal(internalAddress, 5);
      Registers.PC -= 2;
      Registers.WZ = (ushort)(Registers.PC + 1);
      Registers.F = (byte)((Registers.F & ~(Z80Flags.F5 | Z80Flags.F3))
          | ((Registers.PC >> 8) & (Z80Flags.F5 | Z80Flags.F3)));
  }
  ```

- `BlockLoadRepeat`, `BlockCompareRepeat`, `BlockInCore` y `BlockOutCore` llaman a `RepeatBlock(dirección)` en su rama de repetición, en lugar de las cuatro líneas actuales.
- `BlockIoRepeatFlags` deja de tocar F5/F3 (ya lo hace `RepeatBlock`) y se renombra a `BlockIoRepeatAdjust`: solo aplica el ajuste de H y P/V de `spec-instr-io.md` 2.2, que lee el F que deja `RepeatBlock`. Se llama después de `RepeatBlock`.
- Desaparecen los `0x28` literales del Core: se usa `Z80Flags.F5 | Z80Flags.F3`.

### 1.3 Pruebas

- Sin tests nuevos de comportamiento: lo cubren los existentes (`BlockTests`, `IoTests`, incluidos `RepeatF5F3_FromPcHigh`, `BlockIoRepeat_F5F3HAndParityAdjust`, `BlockIoRepeat_WzIsPcPlusOne` y los casos FUSE `edb*`), que deben pasar sin tocarlos.
- Comprobación de que no queda duplicado: `0x28` no aparece en `Z80Cpu.Block.cs` ni en `Z80Cpu.Io.cs`, y `PC -= 2` aparece solo en `RepeatBlock`.
- Benchmark `ExecuteBlockCopyFrame` sin regresión fuera del ruido.

## 2. F5/F3 de `BIT b,r` en la documentación

### 2.1 Error

`Docs/z80-no-documentado.md`, sección 3.1, dice para `BIT b,r`: "del registro probado (en la práctica: F5 = 1 solo si `b = 5` y el bit vale 1; F3 igual con `b = 3`)". La regla del paréntesis es la de Sean Young v0.6, que los toma del resultado del `AND` (`r & (1 << b)`).

Es incorrecta frente a los datos de hardware que usa el proyecto: el caso FUSE `cb40` (`BIT 0,B` con `B = BC`) espera `F = 7C`, con F5 y F3 a 1 aunque se pruebe el bit 0, es decir, **copiados del valor completo del registro**. `BitTest` (`Z80Cpu.Bits.cs`) hace exactamente eso (`value & (F5 | F3)`) y pasa los 64 casos FUSE de `BIT b,r`.

### 2.2 Cambio

- Sección 3.1, fila `BIT b,r`: "Bits 5 y 3 del **registro probado** (el valor completo, no el resultado del `AND` con la máscara)".
- Sección 3.2, regla de `BIT`: añadir "F5/F3: ver 3.1".
- Sección 7, erratas de fuentes no oficiales: añadir que Young v0.6 deriva F5/F3 de `BIT b,r` del resultado del `AND`, y que FUSE (`cb40` y siguientes) y el código del proyecto los toman del registro completo.
- Sección 8: la fila de F5/F3 menciona que `BIT b,r` sigue a FUSE y no a Young v0.6.

### 2.3 Pruebas

Documentación. Comprobar que existe un test que fije la regla para que el código no se "corrija" siguiendo la fuente equivocada: `BitTests.BitRegister_AllBitsAllValues` ya compara contra un modelo de referencia con F5/F3 del registro; añadir en su comentario la referencia a `cb40` y a esta errata.

## 3. Inlining de la tabla base generada

### 3.1 Situación actual

`DispatchEmitter` emite para la tabla base (`Specs/spec-generador-z80.md` 4.2):
- `ExecuteMain` (pequeña, `AggressiveInlining`): salida temprana de `NOP` y llamada a `ExecuteMainDispatch`;
- `ExecuteMainDispatch` (`AggressiveInlining`): el `switch`, donde cada caso implementado llama a un auxiliar `ExecuteMainXX()` y retorna;
- **252** auxiliares `ExecuteMainXX`, todos con `AggressiveInlining`.

Si el JIT los inlinea todos, `ExecuteMainDispatch` vuelve a ser tan grande como sin auxiliares, y además tiene que inlinearse en `Step`. RyuJIT tiene límites de crecimiento de IL y de variables locales por método: si se superan, deja de inlinear y algunos casos pasan a ser llamadas reales según su posición en el método. Los benchmarks actuales solo ejercitan unos pocos opcodes, así que no lo detectarían **(verificar)**.

### 3.2 Investigación (antes de cambiar nada)

1. Obtener el ensamblado JIT en Release (tier 1) de `ExecuteMainDispatch` y `Step` para `Z80Cpu<SpectrumBus>`, por ejemplo con `DOTNET_JitDisasm="ExecuteMainDispatch Step"` y `DOTNET_JitStdOutFile` sobre el proyecto de benchmarks, o con `[DisassemblyDiagnoser(maxDepth: 2)]` en un benchmark.
2. Contar cuántos `call` a `ExecuteMainXX` quedan sin inlinear, y si `ExecuteMainDispatch` se inlinea en `Step`.
3. Anotar el resultado en spec CPU 8.4.

### 3.3 Benchmark de mezcla de opcodes

Para medir el efecto, añadir `Z80CpuBenchmarks.ExecuteMixFrame`: un bucle en RAM no contenida que ejecute muchos opcodes base distintos y repartidos por toda la tabla (cargas `40`–`7F`, ALU `80`–`BF`, inmediatos y saltos `C0`–`FF`, `INC`/`DEC`, `PUSH`/`POP`, `EX`, rotaciones de A…), al menos 40 opcodes distintos, sin E/S ni interrupciones, con ns por T-state. `GlobalCleanup` comprueba que `UnimplementedOpcodes == 0`.

### 3.4 Variantes y decisión

Si la investigación (3.2) muestra casos sin inlinear, o en cualquier caso para fijar la decisión con datos, medir con `ExecuteFrame`, `ExecuteLoopFrame`, `ExecuteAluLoopFrame`, `ExecuteBlockCopyFrame` y `ExecuteMixFrame`:

| Variante | Generación |
|---|---|
| A (actual) | Auxiliares `ExecuteMainXX` con `AggressiveInlining` |
| B | Sin auxiliares: los cuerpos dentro de los `case` de `ExecuteMainDispatch`, como en las demás tablas |
| C | Auxiliares sin atributo de inlining (el JIT decide) |

- Se queda la variante más rápida en el conjunto de benchmarks (prioridad: `ExecuteMixFrame` y los bucles; `ExecuteFrame` no debe empeorar fuera del ruido). Si A gana, no se cambia el generador y se documenta la medición.
- Si gana B o C: cambiar `DispatchEmitter`, regenerar, actualizar `Specs/spec-generador-z80.md` 4.2 y los tests del generador que comprueban el texto de la tabla base.
- Las variantes se prueban generando en un directorio temporal (`--output`) o en una rama local; la salida final se regenera con el generador, nunca a mano.

### 3.5 Pruebas

- Tests del generador: si cambia la emisión, actualizar las aserciones sobre `ExecuteMainXX` y el texto de la tabla base; `Output_MatchesCommittedFiles` y `--check` en verde.
- Runner FUSE 1335/0/0 y `dotnet test` de los dos proyectos sin cambios de expectativas en el Core.

## 4. Documentación

- `Specs/spec-instr-bloques.md` 4.1 y `Specs/spec-instr-io.md` 4.1: mencionar `RepeatBlock` como paso común de repetición.
- `Specs/spec-generador-z80.md` 4.2: resultado de 3.4.
- `Specs/spec-cpu-z80.md` 8.4: ensamblado de 3.2, `ExecuteMixFrame` y comparación de variantes.
- `Docs/z80-no-documentado.md`: cambios de 2.2.
- `CLAUDE.md`/`AGENTS.md`: nuevo benchmark y, si cambia, la forma de la tabla base.

## 5. Criterios de aceptación

- `RepeatBlock` usado por las cuatro repetitivas; sin `0x28` literales ni `PC -= 2` duplicados en `Z80Cpu.Block.cs`/`Z80Cpu.Io.cs`.
- Documentación de `BIT b,r` corregida, con la errata de Young v0.6 anotada.
- Ensamblado de `ExecuteMainDispatch` analizado, `ExecuteMixFrame` añadido y decisión de 3.4 tomada con mediciones anotadas.
- Compilación sin warnings; `dotnet test` de los dos proyectos en verde; `--check` a 0; runner FUSE 1335/0/0.

## 6. Fuera de alcance

- Los otros hallazgos de la revisión (tablas de flags públicas y mutables, longitud de instrucción explícita para la tabla IM 0, comprobación de `opcode.Kind` en todos los patrones).
- Cambios de comportamiento emulado.
