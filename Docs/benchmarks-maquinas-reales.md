# Benchmarks y tests de Z80 contrastados con máquinas reales

Material de referencia para el futuro: suites que comprueban las instrucciones del Z80 contra resultados obtenidos en hardware real, y programas útiles como carga de trabajo para medir el rendimiento del emulador. Revisado el 2026-10-02. Antes de añadir cualquiera al repositorio, volver a confirmar la licencia en su origen.

Lo no confirmado se marca **(verificar)**.

## 1. Suites de conformidad

| Suite | Qué comprueba | Referencia | Licencia | Dónde |
|---|---|---|---|---|
| **ZEXDOC / ZEXALL** (Frank Cringle, 1994, para su emulador YAZE) | Recorre grupos de instrucciones con muchas combinaciones de operandos y compara un CRC por grupo. ZEXDOC solo flags documentados; ZEXALL también F3/F5. | CRC obtenidos en Z80 reales | **GPL v2** | CP/M: [github.com/agn453/ZEXALL](https://github.com/agn453/ZEXALL/), [mdfs.net/Software/Z80/Exerciser](https://mdfs.net/Software/Z80/Exerciser/) |
| **ZEXALL/ZEXDOC para Spectrum** (port de J.G. Harston, mejoras de Stuart Brady) | Igual que el original; imprime con las rutinas de la ROM en lugar de BDOS. Fuentes `zexall.src`/`zexdoc.src`, binarios a cargar en `0x8000` y `.tap`. | Igual; la página incluye resultados en hardware y en varios emuladores | **GPL v2** (derivado) | [mdfs.net/Software/Z80/Exerciser/Spectrum](https://mdfs.net/Software/Z80/Exerciser/Spectrum/) |
| **z80test** (Patrik Rak, raxoft) | `z80full` (flags y registros), `z80doc` (solo flags documentados), `z80flags`, `z80docflags`, `z80ccf` (flags tras `CCF`, es decir, `Q`), `z80ccfscr` (versión visual), `z80memptr` (flags tras `BIT n,(HL)`, es decir, MEMPTR). | Comparado con un Spectrum 48K real con Z80 de Zilog | **MIT** | [github.com/raxoft/z80test](https://github.com/raxoft/z80test) |
| **FUSE tests** (`tests.in` / `tests.expected`) | Estado final y ciclos de bus de cada opcode, con un estado inicial por caso. | Comportamiento de FUSE, contrastado con hardware | GPL (proyecto FUSE) **(verificar versión)** | Ya incluidos en `ZXSinclair.Net.Fuse/data/`; origen: [fuse-emulator.sourceforge.net](https://fuse-emulator.sourceforge.net/) |
| **SingleStepTests Z80** | 1000 casos aleatorios por instrucción, en JSON, con estado inicial, estado final y ciclos de bus (pines de dirección, datos y señales de control). | **No** es hardware real: se generaron traduciendo el núcleo Z80 del emulador Ares y corrigiendo errores después | **MIT** | [github.com/SingleStepTests/z80](https://github.com/SingleStepTests/z80) |
| **Visual Z80 Remix** (Andre Weissflog, floooh) | Simulación a nivel de transistores del Z80 en el navegador; sirve para resolver dudas puntuales de comportamiento, no como suite. | Netlist extraído del chip real (proyecto Visual 6502) | Varias licencias según el componente **(verificar, sobre todo la del netlist)** | Online: [floooh.github.io/visualz80remix](https://floooh.github.io/visualz80remix/); código: [github.com/floooh/v6502r](https://github.com/floooh/v6502r) |

### Notas de uso

- **ZEXDOC/ZEXALL** son muy largos: en un Spectrum real, 68 pruebas de 3–4 minutos cada una (más de cuatro horas y media). En un emulador sirven a la vez de test y de carga de trabajo. Para la versión CP/M basta con capturar la llamada `CALL 5` (BDOS) para imprimir; la versión Spectrum necesita la ROM o capturar `RST 10h`.
- **z80test** resuelve los puntos marcados **(verificar)** en `Specs/spec-cpu-z80.md` sobre `Q` (`SCF`/`CCF`) y MEMPTR. Es la suite más alineada con este proyecto y su licencia MIT permite incluirla sin problemas.
- **SingleStepTests** encaja en xUnit (muchos estados por opcode, lo que le falta a FUSE), pero su referencia es un emulador: ante una discrepancia mandan FUSE, z80test y ZEXALL.
- La temporización de la máquina (contención, ULA, bus flotante) tiene sus propios programas de test para Spectrum medidos en hardware; corresponde a `Specs/spec-buses-memoria.md` y no está recogida aquí **(pendiente de buscar)**.

## 2. Cargas de trabajo para medir rendimiento

No hay benchmarks estándar de Z80 propiamente dichos; en la comunidad de emuladores se usan programas reales como carga:

| Carga | Uso | Notas |
|---|---|---|
| ZEXDOC / ZEXALL / z80test | Mezcla densa de todo el juego de instrucciones; se mide el tiempo hasta completarlos | Más pesada que el código real |
| Arranque de la ROM del 48K hasta el cursor, y programas en BASIC sobre la ROM | Mezcla típica de código de sistema | Amstrad permite distribuir las ROM del Spectrum con emuladores **(verificar condiciones)** |
| Demos | Peor caso realista: mucha contención y temporización exacta | Revisar la licencia de cada una |
| Juegos | Mezcla de instrucciones típica | Revisar la licencia de cada uno |

Métrica recomendada: T-states por segundo o "× tiempo real" (el 48K necesita unos 3,5 millones de T-states por segundo), medida con BenchmarkDotNet en consola y con un arnés propio (`Stopwatch`, frames por segundo) en Blazor WebAssembly, con AOT y con el intérprete.

## 3. Posible integración en el proyecto

1. **Conformidad:** cuando haya suficientes instrucciones, añadir z80test (y opcionalmente ZEXDOC/ZEXALL) como test lento opcional, fuera de `dotnet test` normal, igual que el runner FUSE; y SingleStepTests en xUnit por opcode, como complemento de FUSE.
2. **Rendimiento:** usar los mismos programas (ROM, ZEXDOC, una demo) como cargas de trabajo de los benchmarks de consola y de Blazor.
3. **Licencias:** el proyecto es GPLv3. ZEXALL (GPL v2) solo es compatible si su licencia admite "o versión posterior" **(verificar el texto exacto)**; z80test y SingleStepTests (MIT) son compatibles. Conservar los avisos de copyright de cada suite.
