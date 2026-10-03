# Proceso: especificación e implementación de las instrucciones del Z80

Guía de cómo se especifican, planifican, implementan y verifican las instrucciones de `Z80Cpu<TBus>`. Complementa `Specs/spec-cpu-z80.md` (contrato de la CPU y del generador) y toma como modelo `Specs/spec-instr-nop.md` (instrucción piloto).

## 1. Unidad de trabajo

- **Spec y plan: un grupo del manual de Zilog** (`Docs/z80cpu_um.pdf`, UM0080). Cada grupo tiene fuente propia, frontera clara y semántica de flags común.
- **Implementación: los patrones del generador** dentro del grupo (`LD r,r'`, `LD r,(HL)`, `LD r,(IX+d)`…). Un patrón cubre muchos opcodes con un solo cuerpo de código (por ejemplo, `LD r,r'` son 49 opcodes y sus variantes `DD`/`FD`).
- **No se hace una spec por opcode** (más de 1300) **ni por "tipo"** (carga, ALU, flags…), porque se solapan y no tienen una fuente única.
- Las instrucciones **no documentadas** (`SLL`, `IXH`/`IXL`, F3/F5, `DDCB` con copia a registro…) van dentro de su grupo, en una subsección propia, porque FUSE las exige y salen de los mismos patrones.
- Un grupo grande se puede partir en **varios planes** (por ejemplo, carga de 8 bits: registros e inmediatos por un lado, `(HL)`/`(IX+d)`/`(nn)` por otro), manteniendo una sola spec.

## 2. Orden

Por dependencias y por casos FUSE que desbloquea cada paso.

| # | Spec | Contenido | Motivo del orden |
|---|---|---|---|
| 0 | `spec-generador-z80.md` | Cómo cada entrada de `opcodes_*.dat` se convierte en patrón y plantilla; regla 4.1 de `spec-instr-nop.md` (opcodes que no están en `opcodes_ddfd.dat`); cabecera GPL; despachos en `ZXSinclair.Net.Core/Z80/Generated/`. Piloto: `NOP` generado con salida idéntica a la actual. | Sin el generador, todo lo demás se escribiría a mano y habría que rehacerlo. |
| 1 | `spec-instr-carga-8.md` | Carga de 8 bits. | La usan casi todos los tests; desbloquea muchos casos FUSE. |
| 2 | `spec-instr-carga-16.md` | Carga de 16 bits, `PUSH`/`POP`. | Base de las llamadas. |
| 3 | `spec-instr-saltos.md` | Saltos, llamadas, retornos, `RST`, `DJNZ`. | Necesario para ejecutar código real (ROM, z80test). |
| 4 | `spec-instr-alu-8.md` | Aritmética y lógica de 8 bits, `INC`/`DEC` de 8 bits. | Tablas de flags; la parte más caliente. |
| 5 | `spec-instr-control.md` | Aritmética general y control de CPU: `DAA`, `CPL`, `NEG`, `SCF`/`CCF` (con `Q`), `NOP`, `HALT`, `DI`/`EI`, `IM`. | |
| 6 | `spec-instr-alu-16.md` | Aritmética de 16 bits. | |
| 7 | `spec-instr-rotaciones.md` | Rotaciones y desplazamientos, incluida la tabla CB. | |
| 8 | `spec-instr-bits.md` | `BIT`/`SET`/`RES`. | Incluye MEMPTR en `BIT n,(HL)`. |
| 9 | `spec-instr-bloques.md` | Intercambio, transferencia y búsqueda en bloque (`EX`, `EXX`, `LDI`/`LDIR`, `CPI`/`CPIR`…). | Temporización de las repeticiones. |
| 10 | `spec-instr-io.md` | Entrada/salida, incluidas las de bloque. | |
| 11 | `spec-instr-restos.md` | Huecos de la tabla ED, IM 0 con instrucciones arbitrarias y lo que no encaje en otro grupo. | Cierre. |

`NOP` ya está implementado como piloto (`spec-instr-nop.md`); su spec se mantiene y el grupo 5 la referencia en lugar de repetirla.

## 3. Ciclo por grupo

1. **Spec** `Specs/spec-instr-<grupo>.md` con la estructura de la sección 4.
2. **Plan** con `/plan-manual`: uno por spec, o varios si el grupo es grande. Se guarda en `plans/AAAA-MM-DD-<slug>.md`.
3. **Implementación** según el plan: plantillas del generador y métodos auxiliares escritos a mano (`[AggressiveInlining]`, spec CPU 5). La salida generada no se edita a mano.
4. **Verificación**:
   - `dotnet build zxsinclair.net.slnx` sin warnings nuevos.
   - `dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -- --check` y `dotnet test ZXSinclair.Net.Generate.Z80OpCodes.Tests` en verde.
   - `dotnet test ZXSinclair.Net.Core.Tests` en verde, incluidos los tests propios del grupo (spec CPU 8.3).
   - Runner FUSE (`dotnet run --project ZXSinclair.Net.Test`) con eventos: suben los casos pasados y **0 fallos**.
   - Cada plan incluye ejecuciones con `--filter <prefijo>` para los casos del grupo. Ante un fallo, ejecutar `dotnet run --project ZXSinclair.Net.Test -- --filter <caso> --verbose`, adjuntar el informe a la revisión y clasificar la causa como semántica, temporización o convención de FUSE antes de tocar código o spec.
5. **Documentación**: actualizar la tabla de seguimiento (sección 6), spec CPU 8.1 (cifras del runner) y `CLAUDE.md`/`AGENTS.md` si cambia algo que describan.
6. **Benchmark**: cada dos o tres grupos, no por grupo (sección 5).

Un grupo solo se da por cerrado si **ningún caso FUSE falla**. Un caso que falla por una instrucción de otro grupo aún no implementada se cuenta como omitido, no como fallo.

## 4. Estructura de cada spec de grupo

Mismas secciones que `spec-instr-nop.md`:

1. **Fuentes**: capítulo del UM0080, tabla de la Sinclair Wiki, casos FUSE y entradas de `opcodes_*.dat`.
2. **Semántica por patrón**: indicar cuáles escriben flags (`IPattern.WritesFlags`, Q=F) y cuáles no (Q=0). tabla con opcodes, longitud, ciclos de bus (`pc:4, hl:3, ir:1 ×2…`), T-states, registros, flags (incluidos F3/F5, `Q` y WZ) y casos límite.
3. **Variantes**: prefijos `DD`/`FD`, `CB`, `ED`, `DDCB`/`FDCB` y no documentadas.
4. **Implementación**: patrones y plantillas del generador, métodos auxiliares a mano, tablas de flags que se reutilizan (`Z80Flags`) y requisitos de rendimiento del camino caliente.
5. **Pruebas**:
   - FUSE: casos del grupo que deben pasar y convenciones conocidas que afectan a su comparación (por ejemplo, `ReadDiscarded` en los saltos no tomados).
   - Tests propios (spec CPU 8.3): tabla con nombre, montaje y comprobación.
   - Tests existentes que hay que adaptar (opcodes usados como "no implementado" que dejan de serlo).
6. **Rendimiento**: impacto esperado y si toca medir en este grupo.
7. **Criterios de aceptación**.
8. **Fuera de alcance** y puntos **(verificar)**.

## 5. Rendimiento

- **Microbenchmarks** (BenchmarkDotNet en consola): solo para decidir entre implementaciones de primitivas calientes (despacho, flags de la ALU, `(IX+d)`, `LDIR`, acceso al bus). No se mantiene uno por instrucción.
- **Cargas de trabajo** (consola): desde el grupo 3, arranque de la ROM del 48K; después ZEXDOC/z80test y una demo (`Docs/benchmarks-maquinas-reales.md`). Métrica: T-states por segundo o × tiempo real (~3,5 M T-states/s).
- **Blazor WebAssembly**: las mismas cargas con un arnés propio (`Stopwatch`, frames por segundo), con AOT y con intérprete, en los hitos.
- Cada medición se anota en spec CPU 8.4 con fecha, entorno y cifra; una regresión debe justificarse.

## 6. Seguimiento

| # | Grupo | Spec | Estado | Casos FUSE pasados |
|---|---|---|---|---|
| — | Piloto `NOP` | `spec-instr-nop.md` | Implementado | 3 (`00`, `dd00`, `ddfd00`) |
| 0 | Generador | `spec-generador-z80.md` | Implementado | 3 (piloto NOP) |
| 1 | Carga de 8 bits | `spec-instr-carga-8.md` | Implementado | 163 (166 acumulados, 0 fallos; eventos activados) |
| 2 | Carga de 16 bits | `spec-instr-carga-16.md` | Implementado | 35 (201 acumulados, 0 fallos; eventos activados) |
| 3 | Saltos y llamadas | `spec-instr-saltos.md` | Implementado | 79 (280 acumulados, 0 fallos; eventos activados) |
| 4 | ALU de 8 bits | `spec-instr-alu-8.md` | Implementado | 148 más el caso `10` de DJNZ (429 acumulados, 0 fallos; eventos activados) |
| 5 | Aritmética general y control | `spec-instr-control.md` | Implementado | 27 (456 acumulados, 0 fallos; eventos activados) |
| 6 | ALU de 16 bits | `spec-instr-alu-16.md` | Implementado | 32 (488 acumulados, 0 fallos; eventos activados) |
| 7 | Rotaciones y desplazamientos | `spec-instr-rotaciones.md` | Implementado | 198 (686 acumulados, 0 fallos; eventos activados) |
| 8 | Bits | `spec-instr-bits.md` | Implementado | 584 (1270 acumulados, 0 fallos; eventos activados; 8 con convención F5/F3 de BIT (HL)) |
| 9 | Intercambio y bloques | — | Pendiente | — |
| 10 | Entrada/salida | — | Pendiente | — |
| 11 | Restos | — | Pendiente | — |

Total de casos FUSE: 1335. Estados: Pendiente → Especificado → Planificado → Implementado.

## 7. Después de las instrucciones

Cuando estén los grupos 1–4, valorar añadir z80test (y opcionalmente ZEXDOC/ZEXALL) como test lento opcional, y SingleStepTests en xUnit (ver `Docs/benchmarks-maquinas-reales.md`, con sus licencias).
