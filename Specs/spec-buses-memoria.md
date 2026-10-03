# Especificación: buses y memoria

Especificación del bus del Z80 y del sistema de memoria del ZX Spectrum 48K, 128K y +2 (gris), como base para la reescritura en `ZXSinclair.Net.Core`. Incluye memoria contenida y paginación. El rendimiento es el requisito principal del proyecto (ver `README.md`); la sección 9 fija cómo debe implementarse para no penalizar el bucle de emulación.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` — Zilog *Z80 CPU User Manual* (UM0080), capítulo "Architectural Overview": pines, ciclos máquina y temporización.
- World of Spectrum FAQ: [48K reference](https://worldofspectrum.org/faq/reference/48kreference.htm), [128K/+2/+2A/+3 reference](https://worldofspectrum.org/faq/reference/128kreference.htm).
- Sinclair Wiki: [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory), [Floating bus](https://sinclair.wiki.zxnet.co.uk/wiki/Floating_bus), [ZX Spectrum 128](https://sinclair.wiki.zxnet.co.uk/wiki/ZX_Spectrum_128).
- FUSE (emulador de referencia; los tests de `ZXSinclair.Net.Fuse/data` vienen de él).

Donde las fuentes discrepan o el dato no está confirmado, se marca **(verificar)**.

## 2. El bus del Z80 (manual de Zilog)

### 2.1 Señales

| Grupo | Señal | Dirección | Uso |
|---|---|---|---|
| Direcciones | A15–A0 | salida | 64 KB de memoria; en E/S, el puerto completo de 16 bits (A15–A8 = registro alto) |
| Datos | D7–D0 | bidireccional | |
| Control de sistema | M1 | salida | Con MREQ: lectura de opcode. Con IORQ: reconocimiento de interrupción |
| | MREQ | salida | Dirección válida para lectura/escritura de memoria |
| | IORQ | salida | Dirección válida de E/S; también en reconocimiento de interrupción |
| | RD / WR | salida | Lectura / escritura |
| | RFSH | salida | Con MREQ: los 7 bits bajos del bus son dirección de refresco (registro R) |
| Control de CPU | HALT | salida | CPU en HALT, ejecutando NOPs |
| | WAIT | entrada | Alarga el ciclo; se muestrea en T2 y en cada TW |
| | INT | entrada | Interrupción enmascarable, al final de la instrucción |
| | NMI | entrada | No enmascarable, flanco, salta a 0066h |
| | RESET | entrada | IFF=0, PC=0, I=R=0, IM 0 |
| Control de bus | BUSREQ / BUSACK | E / S | DMA. No se usa en el Spectrum |

### 2.2 Ciclos máquina

Toda instrucción es una secuencia de ciclos M; cada uno dura 3–6 T-states, más los estados de espera (TW) que se inserten.

| Ciclo | T-states | Detalle |
|---|---|---|
| **M1 – lectura de opcode** | 4 | T1–T2: PC en el bus, MREQ+RD, dato leído en el flanco de T3. T3–T4: refresco (R en A6–A0, I en A15–A8, RFSH+MREQ) |
| **Lectura de memoria** | 3 | MREQ+RD |
| **Escritura de memoria** | 3 | MREQ+WR |
| **E/S (IN/OUT)** | 4 | 3 + 1 TW **automático** siempre insertado; WAIT se muestrea durante ese TW |
| **Reconocimiento INT** | M1 especial | IORQ en vez de MREQ, +2 TW automáticos; el dispositivo pone el vector en el bus |
| **NMI** | M1 normal | Dato ignorado; PC a la pila y salto a 0066h |
| **HALT** | M1 repetidos | NOPs internos para mantener el refresco; INT/NMI se muestrean en cada T4 |

Consecuencias para el emulador:
- El refresco de M1 pone `IR` en el bus durante T3–T4. En el 48K esa dirección **puede sufrir contención** (ver 5.3) y provoca el efecto "nieve" si I apunta a 0x40–0x7F.
- Además de los ciclos con MREQ/IORQ, muchas instrucciones tienen **ciclos internos** (p. ej. `LD (IX+d),n`, `INC (HL)`, `PUSH`) en los que la CPU deja una dirección en el bus sin activar MREQ. En el 48K/128K la ULA también los contiene; el bus debe recibirlos.

## 3. Modelos

| | 16K / 48K | 128K | +2 (gris) | +2A / +3 (referencia) |
|---|---|---|---|---|
| Reloj CPU | 3.5000 MHz | 3.5469 MHz | 3.5469 MHz | 3.5469 MHz |
| T-states / línea | 224 | 228 | 228 | 228 |
| Líneas / frame | 312 | 311 | 311 | 311 |
| T-states / frame | 69 888 | 70 908 | 70 908 | 70 908 |
| Primer T-state con contención | 14 335 | 14 361 | 14 361 | 14 361 (patrón distinto) |
| Duración de la señal INT | 32 T | 36 T **(verificar)** | 36 T **(verificar)** | 32 T **(verificar)** |
| ROM | 1 × 16K | 2 × 16K | 2 × 16K | 4 × 16K |
| RAM | 16K / 48K | 8 × 16K | 8 × 16K | 8 × 16K |
| Bancos contenidos | 0x4000–0x7FFF | 1, 3, 5, 7 | 1, 3, 5, 7 | 4, 5, 6, 7 |
| Contención sin MREQ | sí | sí | sí | no (gate array) |
| Puertos de paginación | — | 0x7FFD | 0x7FFD | 0x7FFD, 0x1FFD |

El **+2 gris** es, a efectos de memoria, paginación y temporización, idéntico al 128K (cambian la ROM, el teclado y el datacassette integrado). El **+2A/+3** ("+2 negro") usa un gate array de Amstrad con reglas distintas; queda fuera del alcance inicial pero el diseño no debe impedirlo.

## 4. Mapa de memoria y paginación

### 4.1 48K (y 16K)

| Rango | Contenido |
|---|---|
| 0x0000–0x3FFF | ROM (escrituras ignoradas) |
| 0x4000–0x57FF | Pantalla: bitmap (6144 bytes) — **contenido** |
| 0x5800–0x5AFF | Atributos (768 bytes) — **contenido** |
| 0x5B00–0x7FFF | RAM — **contenido** |
| 0x8000–0xFFFF | RAM en el 48K. En el 16K no hay memoria: las escrituras se pierden y la lectura devuelve el bus flotante de la ULA (0xFF salvo cuando la ULA lee pantalla) |

En el 16K la implementación inicial devuelve 0xFF en 0x8000–0xFFFF; devolver el bus flotante real queda **(verificar)**, porque exige que el bus distinga páginas ausentes en cada lectura.

### 4.2 128K / +2

Cuatro ranuras de 16K:

| Ranura | Rango | Contenido |
|---|---|---|
| 0 | 0x0000–0x3FFF | ROM 0 (editor 128) o ROM 1 (BASIC 48), según bit 4 de 0x7FFD |
| 1 | 0x4000–0x7FFF | RAM banco 5 (fijo) — contenido |
| 2 | 0x8000–0xBFFF | RAM banco 2 (fijo) |
| 3 | 0xC000–0xFFFF | RAM banco 0–7 (bits 0–2 de 0x7FFD); contenido si el banco es impar |

Los bancos 5 y 2 también pueden aparecer en la ranura 3: es la misma memoria vista en dos direcciones (escribir en una se ve en la otra).

### 4.3 Puerto 0x7FFD (128K / +2)

| Bit | Función |
|---|---|
| 0–2 | Banco de RAM en 0xC000 |
| 3 | Pantalla que muestra la ULA: 0 = banco 5, 1 = banco 7 (pantalla sombra) |
| 4 | ROM: 0 = ROM 0 (128), 1 = ROM 1 (48 BASIC) |
| 5 | Bloqueo: una vez a 1, ignora más escrituras hasta un reset |
| 6–7 | Sin uso |

- **Decodificación parcial**: responde a cualquier puerto con **A15 = 0 y A1 = 0**.
- **Estado tras reset**: 0 (ROM 0, banco 0 en 0xC000, pantalla en banco 5, sin bloqueo).
- **Lectura de 0x7FFD**: no devuelve el valor del puerto. WoS indica que devuelve el bus flotante; la Sinclair Wiki indica que en el 128K provoca un cuelgue (el chip HAL10H8 lo trata como escritura con el valor del bus flotante) **(verificar)**. Mínimo: devolver bus flotante.
- La ROM guarda una copia del último valor escrito en `BANKM` (0x5B5C). El hardware no lo necesita, pero es útil para depurar.
- La pantalla sombra cambia solo lo que **muestra la ULA**; la contención de la ranura 1 sigue dependiendo del banco 5.

### 4.4 Puertos del +2A/+3 (referencia)

- 0x7FFD: decodifica A15 = 0, A14 = 1, A1 = 0; el bit 4 pasa a ser el bit bajo de la selección de ROM.
- 0x1FFD: decodifica A15 = A14 = A13 = 0, A12 = 1, A1 = 0. Bit 0 = modo especial (4 configuraciones de RAM en las 4 ranuras, sin ROM), bit 2 = bit alto de la ROM, bit 3 = motor de disco, bit 4 = strobe de impresora.

### 4.5 ZX81 (1K y 16K)

El ZX81 decodifica la memoria de forma parcial, así que ROM y RAM aparecen repetidas (espejos). Fuentes: [Nocash ZX specs](https://problemkaputt.de/zxdocs.htm), [Tynemouth Software](http://blog.tynemouthsoftware.co.uk/2019/10/how-the-zx80-works.html).

| Rango | 1K (interna) | 16K (RAM pack) |
|---|---|---|
| 0x0000–0x1FFF | ROM 8K | ROM 8K |
| 0x2000–0x3FFF | Espejo de la ROM | Espejo de la ROM |
| 0x4000–0x7FFF | RAM 1K repetida 16 veces (0x4000–0x43FF y espejos) | RAM 16K (la interna queda desactivada) |
| 0x8000–0xBFFF | Espejo de 0x0000–0x3FFF | Espejo de 0x0000–0x3FFF |
| 0xC000–0xFFFF | Espejo de 0x4000–0x7FFF | Espejo de 0x4000–0x7FFF |

- La ROM es de solo lectura en todos sus espejos.
- El espejo de la RAM en 0xC000–0xFFFF es el que usa el ZX81 para generar el vídeo (ejecución "M1 por encima de 32K"). Su temporización y el vídeo pertenecen a otra especificación; aquí solo cuenta el mapa.
- Hay variantes de RAM pack y modificaciones (8K en 0x2000, etc.) **(verificar)**: el modelo de memoria por páginas (9.4) debe permitir describirlas sin código nuevo.

## 5. Memoria contenida

### 5.1 Qué es

La ULA comparte el bus con la CPU para leer la pantalla. Mientras dibuja las 192 líneas visibles, si la CPU accede a memoria contenida, la ULA retiene el reloj de la CPU hasta terminar su lectura. Los retrasos solo se producen durante la parte visible de cada línea (128 T-states); en borde y retrazado no hay retraso.

### 5.2 Patrón de retraso

En cada línea visible, a partir del primer T-state contenido, se repite el patrón **6, 5, 4, 3, 2, 1, 0, 0** (8 T-states) 16 veces (128 T-states), y la línea siguiente empieza `T-states por línea` después.

| T-state (48K) | 14335 | 14336 | 14337 | 14338 | 14339 | 14340 | 14341 | 14342 | 14343 … |
|---|---|---|---|---|---|---|---|---|---|
| Retraso | 6 | 5 | 4 | 3 | 2 | 1 | 0 | 0 | 6 … |

- 48K: primera línea en 14 335, período 224; última zona contenida de la primera línea 14 335–14 462.
- 128K / +2: primera línea en 14 361, período 228.
- +2A/+3: patrón **1, 0, 7, 6, 5, 4, 3, 2** desde 14 361 **(verificar con la tabla de WoS antes de implementarlo)**.
- Algunas máquinas con ULA son "late timing": todo ocurre 1 T-state más tarde. Debe ser configurable (desplazamiento de +1).

Retraso para un T-state `t` dentro del frame (48K/128K):

```
línea = (t - primerContenido) / tPorLínea
pos   = (t - primerContenido) % tPorLínea
retraso = (t >= primerContenido && línea < 192 && pos < 128) ? patrón[pos & 7] : 0
```

En la implementación se precalcula como tabla (ver 9.3).

### 5.3 Cuándo se aplica

- En cada ciclo de memoria (M1, lectura, escritura) la contención se aplica en **T1**, antes de que transcurran los T-states del ciclo. Es decir: `t += retraso(t); t += duración`.
- **48K / 128K / +2**: la ULA no mira MREQ, solo la dirección. Se aplica también a:
  - los **ciclos internos** con dirección en el bus (cada T-state extra se contiene por separado: la notación de la Sinclair Wiki `hl:1 ×5` significa 5 comprobaciones de 1 T-state sobre la dirección HL);
  - el **refresco** de M1 (dirección `IR`) — origen del efecto nieve.
- **+2A/+3**: solo cuando MREQ está activo; los ciclos internos no se contienen.

La tabla completa de qué ciclos tiene cada instrucción (`pc:4, n:3, hl:3, …`) está en la página "Contended memory" de la Sinclair Wiki y es la referencia para que el generador de opcodes emita las llamadas al bus correctas.

### 5.4 Contención de E/S (48K / 128K / +2)

Depende de si el byte alto del puerto direcciona memoria contenida y de A0 (A0 = 0 → el puerto es de la ULA):

| Byte alto contenido | A0 | Secuencia |
|---|---|---|
| No | 1 | N:4 |
| No | 0 | N:1, C:3 |
| Sí | 1 | C:1, C:1, C:1, C:1 |
| Sí | 0 | C:1, C:3 |

`N:n` = sin retraso, avanzar n T-states. `C:n` = aplicar `retraso(t)` y avanzar n T-states.

- 48K: "byte alto contenido" = 0x40–0x7F.
- 128K / +2: FUSE comprueba si la dirección `puerto & 0xFF00` cae en una ranura contenida, de modo que 0xC0–0xFF también cuenta si hay un banco impar paginado **(verificar)**. El puerto 0x7FFD (byte alto 0x7F) está contenido por su byte alto.
- +2A/+3: el puerto 0xFE no está contenido.

## 6. Bus flotante

Al leer un puerto que nadie decodifica (p. ej. 0xFF) en 48K/128K/+2, el valor es lo que la ULA tenga en el bus de datos:

| T-state relativo (ciclo de 8) | 0 | 1 | 2 | 3 | 4–7 |
|---|---|---|---|---|---|
| Byte en el bus | bitmap | atributo | bitmap + 1 | atributo + 1 | 0xFF |

- Empieza en el T-state 14 338 (48K) y 14 364 (128K/+2), una vez por cada bloque de 8 durante las 128 T-states visibles de cada línea; fuera de ahí, 0xFF.
- Juegos como *Arkanoid* o *Cobra* dependen de esto.
- En "late timing", 1 T-state más tarde.

## 7. Puerto 0xFE (ULA)

- Decodificación: cualquier puerto con **A0 = 0**.
- Escritura: bits 0–2 borde, bit 3 MIC, bit 4 EAR/altavoz.
- Lectura: bits 0–4 teclado (semifila seleccionada por A8–A15, 0 = pulsada, AND de las semifilas activas), bit 6 EAR, bits 5 y 7 a 1.
- El borde se actualiza con su propia granularidad de T-state (128K: cambios visibles tras 14 365–14 368 T-states); pertenece a la especificación de vídeo, pero el bus debe entregar el T-state exacto del OUT.

## 8. Interrupciones

- La ULA activa INT al principio de cada frame (T-state 0), durante 32 T-states en el 48K (36 en 128K/+2, **verificar**). Si la CPU no la acepta en esa ventana (DI o instrucción larga), se pierde.
- Se acepta al final de una instrucción; el reconocimiento es un M1 especial con 2 TW (7 T-states en IM1/IM2 hasta empezar a apilar, según la tabla de cada modo).
- El valor del vector en IM2 es lo que haya en el bus (0xFF normalmente).

## 9. Requisitos de implementación (`ZXSinclair.Net.Core`)

Todo lo anterior está en el bucle crítico: cada instrucción hace varios accesos a memoria. Las decisiones siguientes se toman por rendimiento; cualquier alternativa debe demostrar con BenchmarkDotNet que no es más lenta.

### 9.1 Sin llamadas por interfaz en el bucle

- El benchmark de `ZXSinclair.Net.Benchmarks` mide el `MemoryBuffer8Bit` actual vía interfaz en **1.65×** el coste de un array directo.
- La CPU se parametriza con el tipo concreto del bus: `Z80Cpu<TBus> where TBus : struct, IZ80Bus` (struct ⇒ el JIT especializa y puede hacer inline) o bien una clase `sealed` por modelo usada como tipo concreto. Las interfaces de `Abstractions/` sirven para el contrato, no para el despacho en tiempo de ejecución.
- Sin `Enum.HasFlag`, sin delegados, sin asignaciones por acceso.

### 9.2 Contrato del bus

Operaciones mínimas que la CPU necesita (nombres orientativos):

| Operación | Ciclo | Coste base |
|---|---|---|
| `FetchOpcode(addr)` | M1 + refresco (`IR`) | 4 T |
| `Read(addr)` | lectura | 3 T |
| `ReadDiscarded(addr)` | lectura de un operando que una instrucción condicional no usa para saltar (`JR`/`JP`/`CALL`/`DJNZ` no tomados) | 3 T |
| `Write(addr, value)` | escritura | 3 T |
| `Internal(addr, n)` | ciclos internos con dirección en el bus | n × 1 T |
| `In(port)` / `Out(port, value)` | E/S con TW automático | 4 T |
| `AcknowledgeInterrupt()` | M1 con IORQ y dos TW | 6 T; la extensión se emite con `Internal(IR, 1)` |

- `ReadDiscarded` es, en el bus real, idéntica a `Read` (mismo ciclo, contención, coste y dato devuelto); `SpectrumBus` la implementa llamando a `Read`. Existe para que un bus de pruebas pueda seguir a FUSE, que modela ese ciclo como `contend_read` y lo registra solo como `MC`, sin `MR`.
- **El contador de T-states pasa al bus/máquina**, no a la CPU: quien conoce la contención es la ULA. La CPU solo emite ciclos.
- Cada operación aplica la contención en el T-state en que empieza (5.3) y suma su coste. Así el retraso queda dentro del acceso y la CPU no necesita saber nada de la ULA.
- Las líneas de entrada (INT, NMI, RESET) se exponen como enteros/bits que la CPU consulta al final de cada instrucción.

### 9.3 Contención por tabla

- Tabla precalculada `byte[] contention` de longitud `T-states por frame` (+ margen para instrucciones que cruzan el final del frame), con el retraso de cada T-state. Una lectura por acceso, sin divisiones.
- Bitmask/array de 4 entradas `contendedSlot[addr >> 14]` actualizado al paginar. En el 48K basta comparar `(addr & 0xC000) == 0x4000`.
- Acceso: `if (contendedSlot[addr >> 14]) t += contention[t];` — una rama muy predecible.

### 9.4 Almacenamiento y paginación

- Un único array gestionado con todas las ROM y bancos de RAM, `GC.AllocateUninitializedArray<byte>(n, pinned: true)`, accedido con `Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(...), …)` (más rápido y estable que `AllocHGlobal` en el benchmark).
- Tablas separadas de desplazamientos **de lectura** y **de escritura** por ranura. Las escrituras en ROM apuntan a un bloque de 16K "sumidero" que nunca se lee: elimina la comprobación de ROM en cada escritura.
- Paginar = recalcular 4 desplazamientos + 4 flags de contención; nunca copiar memoria.
- La versión paginada medida (2.17×) es demasiado lenta: hay que probar alternativas (por ejemplo, una implementación plana específica para el 48K y, para 128K, desplazamientos guardados en campos en vez de en array, o arrays de 16K por ranura) y elegir la más rápida antes de cerrar el diseño.
- **Memoria por páginas configurable**: la memoria no asume 48K/128K. Un `MemoryLayout` describe el tamaño de página (potencia de 2: 1K para el ZX81, 16K para el Spectrum), las regiones (ROM/RAM de cualquier tamaño) y qué región ve cada página; los espejos son varias páginas apuntando a la misma región. Las páginas sin memoria leen de una página llena de 0xFF y escriben en el sumidero. La paginación del 128K solo reescribe entradas de la tabla.
- La ULA lee la pantalla del banco 5 o 7 directamente por desplazamiento, sin pasar por el bus.

### 9.4.1 Implementación y mediciones (2026-10-02)

Implementado en `ZXSinclair.Net.Core` (`Abstractions/`, `Memory/`, `Timing/`, `Machines/`). Medido con BenchmarkDotNet (i7-14700, .NET 10, x64), una lectura + una escritura por operación sobre un flujo de direcciones tipo emulador:

| Variante | ns/op |
|---|---:|
| Array plano con `Unsafe.Add` (referencia) | 0.95 – 1.03 |
| `PagedMemory` (tabla de páginas 1K o 16K, lectura/escritura sin ramas) | 1.17 |
| Paginada anterior (tabla de ranuras + comprobación de ROM) | 1.47 |
| `SpectrumBus` 48K / 128K (T-states + contención + páginas) | 3.4 |

Decisiones:
- **Una sola memoria por páginas para todos los modelos** (16K, 48K, 128K/+2, ZX81). Cuesta ~0.2 ns/op más que el array plano; no se añade una memoria plana específica para 48K por ahora. Revisarlo cuando exista la CPU y se pueda medir una instrucción completa.
- **Contención sin ramas**: los flags de contención por página son máscaras 0x00/0xFF y el retraso se aplica como `t += table[t] & mask`. Con la rama `if (contended)`, el benchmark daba entre 3.8 y 5 ns con mucha variación por fallos de predicción.
- **Contador de T-states leído y escrito una vez por ciclo** (en una variable local). El coste restante del bus (~3.4 ns frente a ~1 ns) viene sobre todo de la cadena de dependencia del contador a través de memoria; reducirlo exigiría que la CPU mantenga el contador en un registro y lo pase al bus, decisión a tomar al diseñar `Z80Cpu<TBus>`.
- La tabla de contención y la de bus flotante tienen `ContentionTable.Margin` (256) entradas tras el frame; la máquina debe llamar a `EndFrame()` antes de agotarlas (comprobado con `Debug.Assert`).
- Valores tomados de FUSE y pendientes de verificar, aislados en constantes: INT de 36 T en 128K (`MachineTiming.Spectrum128`), reconocimiento de interrupción de 6 T con vector 0xFF (`SpectrumMachine.InterruptAcknowledgeTStates`), lectura de 0x7FFD = bus flotante, contención de E/S según la página del byte alto (incluida la ranura 3 en 128K).

### 9.5 Validación

- Los tests FUSE (`tests.expected`) ya incluyen los eventos de bus `MC` (contención de memoria), `MR`, `MW`, `PC`, `PR`, `PW` con su T-state. El runner los compara además de registros, memoria y T-states, respetando longitud, orden, tiempo, tipo, dirección y dato (`FuseComparison` en `ZXSinclair.Net.Fuse`).
- Los ciclos internos de las instrucciones pasan siempre por `IZ80Bus.Internal(dirección, n)`. En `LD I,A` y `LD R,A`, el ciclo interno usa `IR` antes de modificar el registro. El registro de eventos lo hace un bus de pruebas (`FuseTestBus`, ver `Specs/spec-cpu-z80.md`, sección 8.1), no la CPU.
- Pruebas de máquina con programas de test de contención y bus flotante (p. ej. los de FUSE/ZX test suites) una vez exista la ULA.

## 10. Pendiente de verificar

- Duración exacta de INT en 128K/+2 y +2A/+3.
- Efecto real de leer 0x7FFD en 128K/+2 (bus flotante frente a escritura espuria).
- Contención de E/S en 128K cuando el byte alto cae en la ranura 3 con banco impar.
- Patrón y tabla exactos del +2A/+3.
- Lectura de 0x8000–0xFFFF en el 16K: hoy devuelve 0xFF; en el hardware real es el bus flotante.
- Variantes de memoria del ZX81 (RAM packs, 8K en 0x2000) y bus/temporización del ZX81.


### Reconocimiento de INT (grupo 11)

`AcknowledgeInterrupt` consume 6 T, sin contención. IM 1 e IM 2 añaden `Internal(IR, 1)` antes de apilar; en IM 0 la extensión sale del cuerpo de la instrucción. Se conservan 13/19 T para IM 1/2 y 13 para RST.

Se aplica contención al T interno cuando IR está en una página contenida. Es una inferencia de los ciclos `ir:1` de RST/PUSH y de la regla de la ULA de los modelos 16K/48K/128K/+2, descritos en [Sinclair Wiki, Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory). Esa fuente no desglosa el reconocimiento de INT; falta contrastar ese T concreto con hardware. `SpectrumBusTests.InterruptInternalCycle_UsesIrContention` verifica la secuencia elegida dentro y fuera de memoria contenida, con 6 T de retraso al comienzo de la ventana de contención.
