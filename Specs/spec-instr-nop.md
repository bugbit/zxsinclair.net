# Especificación: instrucción NOP

Primera instrucción del Z80 en `ZXSinclair.Net.Core`. Concreta, para `NOP`, el contrato de `Specs/spec-cpu-z80.md` (secciones 4 y 5) y sirve de caso piloto para el generador `ZXSinclair.Net.Generate.Z80OpCodes`: valida de extremo a extremo el despacho, la temporización por bus, los tests FUSE y el benchmark antes de abordar instrucciones con efectos.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (Zilog UM0080), entrada `NOP`: opcode `00`, 1 M-ciclo, 4 T-states, no afecta a ningún flag.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): `NOP` = `pc:4`.
- FUSE: casos `00` y `dd00` de `ZXSinclair.Net.Fuse/data/tests.in` / `tests.expected`; entrada `0x00 NOP` de `opcodes_base.dat` (no aparece en `opcodes_ddfd.dat`).

## 2. Semántica

| Campo | Valor |
|---|---|
| Opcode | `00` |
| Longitud | 1 byte |
| Ciclos de bus | `pc:4` (solo el M1 del fetch) |
| T-states | 4 |
| Registros | Ninguno, salvo `PC += 1` y R (7 bits bajos) `+= 1`, ambos producidos por el fetch |
| Flags | Sin cambios |
| WZ (MEMPTR) | Sin cambios |
| `Q` | 0, porque la instrucción no modifica F **(verificar si se implementa `Q`; spec CPU 3)** |
| IFF1/IFF2/IM/Halted | Sin cambios. `EiPending` lo limpia `Step()` antes de ejecutar, como en cualquier instrucción |

Tras ejecutar `NOP`, la siguiente comprobación de interrupciones de `Step()` puede aceptar INT/NMI con normalidad (no es una instrucción especial como `EI`).

`PC` da la vuelta: `NOP` en `0xFFFF` deja `PC = 0x0000`.

## 3. Variantes y casos relacionados

| Secuencia | Comportamiento | T | R |
|---|---|---|---|
| `00` | NOP | 4 | +1 |
| `DD 00` / `FD 00` | El prefijo consume su M1 y `00` se ejecuta como NOP sin prefijo (spec CPU 4.3: el opcode no usa HL) | 8 | +2 |
| `ED 00` | **No es NOP**: es un hueco de la tabla ED (dos NOP, 8 T, spec CPU 4.3). Fuera de esta spec | 8 | +2 |
| HALT | El NOP interno de HALT lo implementa `Step()` (spec CPU 4.2) y no pasa por `ExecuteMain` | 4 | +1 |
| IM 0 con `00` en el bus | Sigue siendo no implementado en IM0 (spec CPU 6.3); queda para la spec de IM0 | — | — |

## 4. Implementación

`Step()` ya hace el fetch (`bus.FetchOpcode(PC++)` + R++) antes de despachar, así que `NOP` no añade código: es un `case` vacío.

```csharp
// ExecuteMain(byte opcode)
case 0x00: return; // NOP
```

Requisitos:
- No llama a `Unimplemented()`: tras ejecutar `NOP`, `UnimplementedOpcodes` no cambia.
- No emite ciclos adicionales al bus (ni `Internal` ni lecturas).
- Sin llamadas a métodos auxiliares: el `case` vacío es el coste mínimo posible; el JIT lo resuelve en la tabla de saltos del `switch`.

### 4.1 Camino indexado

`ExecuteIndexedOpcode<TIndex>(0x00)` también debe ejecutar `NOP`. Regla general que fija esta spec para el generador: **todo opcode que no figure en `opcodes_ddfd.dat` se genera en `ExecuteIndexedOpcode<TIndex>` con el mismo cuerpo que en `ExecuteMain`** (para `NOP`, `case 0x00: break;`). No se delega con una llamada a `ExecuteMain` desde el `default`, para no añadir un segundo salto indirecto en el camino caliente; el `default` de `ExecuteIndexedOpcode` queda para los opcodes aún no implementados.

### 4.2 Generador

- Entrada: `0x00 NOP` en `opcodes_base.dat`. El generador emite el `case` en `ExecuteMain` y, por la regla de 4.1, en `ExecuteIndexedOpcode<TIndex>`.
- La salida lleva la cabecera GPL y no se edita a mano (spec CPU 5).
- El patrón `NopPattern` emite un cuerpo vacío. El generador lo termina con `return` en `Generated/Z80Cpu.Main.g.cs` y con `break` en `Generated/Z80Cpu.Indexed.g.cs`; ambas formas ejecutan solo el fetch. La tabla base usa retornos directos para reducir el IL al añadir carga de 8 bits (spec CPU 8.4). La salida no se edita a mano.

## 5. Pruebas

### 5.1 FUSE (`ZXSinclair.Net.Test`)

Los dos casos deben dejar de omitirse y pasar, incluida la comparación de eventos:

| Caso | Memoria | `end_tstates` | Eventos esperados | Estado final |
|---|---|---|---|---|
| `00` | `0000: 00` | 1 | `0 MC 0000`, `4 MR 0000 00` | `PC = 0001`, `R = 01`, T = 4 |
| `dd00` | `0000: DD 00 00` | 9 | `MC/MR` en 0000 (`dd`), 0001 (`00`) y 0002 (`00`) a T 0/4, 4/8, 8/12 | `PC = 0003`, `R = 03`, T = 12 |

`dd00` ejecuta `DD 00` (8 T) y, como 8 < 9, un segundo `NOP` en `0002`. También pasa `ddfd00` (prefijos DD/FD seguidos de NOP); el resto de casos siguen omitidos. Resultado verificado el 2026-10-02: 3 correctos, 0 fallos y 1332 omitidos, con comparación de eventos.

### 5.2 Tests propios (xUnit, no FUSE)

Siguen las reglas generales de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/NopTests.cs`, con el `TestBus` existente (registra `("M1", dirección, valor)` por cada fetch). Salvo que se indique otra cosa, el estado inicial carga todos los registros con valores distintos de cero (AF, BC, DE, HL, el juego alternativo, IX, IY, SP, WZ, I, `IFF1 = IFF2 = true`, `IM = 1`, `Q = 0`) para detectar cualquier escritura indebida.

| Test | Montaje | Comprobación |
|---|---|---|
| `Nop_EmitsSingleM1` | `00` en `0x1234`, `PC = 0x1234` | Un único acceso `("M1", 0x1234, 0x00)`; 4 T |
| `Nop_ChangesOnlyPcAndR` | Igual | `Z80Registers` completo igual al inicial salvo `PC = 0x1235` y R +1 (incluye flags, WZ, IFF, IM, `Halted`, `EiPending`) |
| `Nop_DoesNotCountAsUnimplemented` | Igual | `UnimplementedOpcodes == 0` |
| `Nop_RefreshWrapsKeepingBit7` (`[Theory]`) | `R = 0x7F` y `R = 0xFF` | `R = 0x00` y `R = 0x80` |
| `Nop_AtFFFF_WrapsPcToZero` | `00` en `0xFFFF`, `PC = 0xFFFF` | `PC = 0x0000`, acceso `M1` en `0xFFFF`, 4 T |
| `Nop_DoesNotWriteMemory` | Memoria de 64K con un patrón | Memoria idéntica tras el `Step()`; ningún acceso `Write` |
| `Nop_WithIndexPrefix_RunsAsPlainNop` (`[Theory]`) | `DD 00` y `FD 00` | Accesos `M1` en 0 y 1; 8 T; `PC = 2`; R +2; IX/IY y el resto de registros sin cambios; contador a 0 |
| `Nop_WithRepeatedPrefixes_RunsAsPlainNop` (`[Theory]`) | `DD FD 00` y `FD DD 00` | 12 T, `PC = 3`, R +3, contador a 0 |
| `Nop_ClearsEiPending` | `EiPending = true` | Tras el `Step()`, `EiPending == false` |
| `Nop_AllowsIntOnNextStep` | `00`, IM 1, `IFF1 = true`, INT activa desde T 1 (`IntAfterCycle = 1`) | El 1.er `Step()` ejecuta NOP (4 T); el 2.º acepta la INT: 13 T más, `PC = 0x0038`, `IFF1 = IFF2 = false`, retorno `0x0001` apilado |
| `Nop_AllowsNmiOnNextStep` | `00`, `RequestNmi()` tras el 1.er `Step()` | El 2.º `Step()` salta a `0x0066` (11 T) y apila `0x0001` |
| `Nop_ExecuteStopsAtTarget` | Memoria a cero | `Execute(10)` ejecuta 3 NOP: 12 T, `PC = 3`, R +3 |

### 5.3 Tests existentes que hay que adaptar

Al implementar `NOP`, el opcode `00` deja de ser "no implementado" y estos tests de `Z80CpuTests.cs` cambian:

| Test | Cambio |
|---|---|
| `UnimplementedDispatchCountsFetches` | Los casos `00`, `DD 00`, `FD 00`, `DD FD 00` y `FD DD 00` pasan a `NopTests` (5.2) y aquí se sustituyen por otro opcode aún no implementado (p. ej. `01`), para seguir probando el despacho de instrucciones pendientes. |
| `LongAlternatingPrefixChainUsesConstantStackSpace` | El último byte (`0xFFFF`) es `00`: espera `UnimplementedOpcodes == 0`, o se pone ahí un opcode no implementado para conservar la aserción actual. |
| `SpectrumIntegrationRunsAFrameAndAppliesFetchContention` | Con memoria a cero espera `UnimplementedOpcodes == 0` en vez de 17472; PC y T-states no cambian. |

`Z80CpuBenchmarks.ExecuteFrame` devuelve `UnimplementedOpcodes` para que el JIT no elimine el trabajo; tras `NOP` valdría 0, así que debe devolver otro valor dependiente de la ejecución (p. ej. `Registers.PC`).

## 6. Rendimiento

El benchmark `Z80CpuBenchmarks.ExecuteFrame` (spec CPU 8.4) ya recorre memoria a cero: tras esta spec mide `NOP` real en vez del despacho provisional. Tras implementarla:
- Volver a medir con `dotnet run -c Release --project ZXSinclair.Net.Benchmarks -- --filter '*Z80Cpu*'`.
- El coste por opcode no debe superar la medición base de 3.844 ns (se elimina el incremento de `UnimplementedOpcodes`) y debe seguir sin asignaciones.
- Actualizar la tabla de spec CPU 8.4 con la nueva cifra y anotar que mide `NOP`.

## 7. Criterios de aceptación

- `ExecuteMain` y `ExecuteIndexedOpcode<TIndex>` tienen `case 0x00` sin cuerpo y sin llamada a `Unimplemented()`.
- Los casos FUSE `00` y `dd00` pasan con eventos y el runner no informa fallos (los casos que ejecuten otros opcodes siguen omitidos).
- Los tests propios de 5.2 pasan y los de 5.3 están adaptados; `dotnet test ZXSinclair.Net.Core.Tests` en verde.
- Benchmark repetido y spec CPU 8.4 actualizada, sin regresión ni asignaciones.

## 8. Fuera de alcance

- Huecos de la tabla ED (`ED 00` y similares) y su tratamiento como dos NOP.
- Ejecución de instrucciones en IM 0.
- Diseño completo del generador más allá de la regla de 4.1.
