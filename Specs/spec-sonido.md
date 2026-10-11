# Especificación de sonido: beeper y AY-3-8912

Documento de diseño previo a la implementación (2026-10-11). Fija cómo se genera el sonido del Spectrum 16K/48K
(beeper), del 128K/+2 gris (beeper y AY-3-8912) y del ZX80/ZX81 (salida de cinta ligada a VSYNC y tarjetas AY
opcionales, sección 3.4), cómo se integra en el Core sin penalizar el camino caliente y
cómo llega al navegador desde el Web Worker de [spec-frontend-blazor.md](spec-frontend-blazor.md) (secciones 4.2
y 6.2). No hay código todavía: las cifras marcadas **(verificar)** deben confirmarse con las fuentes o con medidas
antes de implementar.

El rendimiento sigue siendo el requisito principal (`README.md`, `CLAUDE.md`): el sonido no puede añadir trabajo
por instrucción ni por T-state; solo un coste acotado por `OUT` y una síntesis por frame.

## 1. Alcance

| Incluido | Fuera de alcance (por ahora) |
|---|---|
| Beeper (bit 4 del puerto 0xFE) en 16K, 48K, 128K y +2 gris | +2A/+2B/+3 (mismo AY, otra ULA: se añadirá con su máquina) |
| AY-3-8912 en 128K y +2 gris (puertos 0xFFFD/0xBFFD) | Interfaces AY para 48K (Melodik, Fuller Box), TurboSound (2 AY) |
| Mezcla mono en `float` [-1, 1] a la frecuencia del `AudioContext` | Estéreo ABC/ACB (prevista como opción, sección 6.5) |
| Sonido de cinta (EAR de entrada, MIC de salida) como opción de mezcla | Covox, SpecDrum, General Sound |
| Entrega por frame al worklet (SAB y `postMessage`) | Ruido de vídeo que se cuela en el audio del televisor (ZX80/ZX81) |
| ZX80/ZX81: salida de cinta (VSYNC) y tarjetas AY ZON X-81 / Quicksilva (sección 3.4) | Otras tarjetas de sonido del ZX81 |

## 2. Fuentes

- `Docs/maquinas-sinclair-es.md`: puertos 0xFE, 0xFFFD y 0xBFFD, reloj del AY, registro de cambios con el T del `OUT`.
- [spec-frontend-blazor.md](spec-frontend-blazor.md): worker, `pushAudio`, ring buffer, audio como reloj maestro.
- Hoja de datos General Instrument AY-3-8910/8912 (registros, fórmulas de periodo, formas de envolvente).
- FUSE (`sound.c`, `periph/ay`): niveles del beeper por *issue*, tabla de volúmenes del AY, integración por muestra.
- Implementaciones de referencia para el AY: MAME (`ay8910.cpp`) y Ayumi (tablas de volumen medidas, filtrado).
- `Docs/maquinas-sinclair-es.md` secciones 2 y 3 y `Docs/zx81-video-es.md`: puertos del ZX80/ZX81, VSYNC por
  `IN 0xFE`/`OUT`, salida de cinta y frame definido por software.
- EightyOne (emulador de ZX80/ZX81): tarjetas de sonido ZON X-81 y Quicksilva, y salida de cinta como sonido.
- Blargg, *Blip_Buffer* / BLEP (síntesis de escalones con banda limitada), para la fase de calidad (sección 9).

## 3. El hardware

### 3.1 Beeper

- Escritura en el puerto ULA (A0 = 0): bit 4 = EAR (altavoz), bit 3 = MIC (salida de cinta). El Core ya los guarda
  en `SpectrumMachine.WriteUlaPort` (`Speaker`, `Mic`) pero sin el T del cambio.
- El altavoz es de 1 bit: todo el sonido (incluida la música "multicanal" de motores 1-bit y el PWM de voces
  digitalizadas) se hace conmutando el bit con temporización exacta por software.
- La conmutación más rápida posible es un `OUT (n),A` cada 11 T (≈ 318 000 cambios/s a 3,5 MHz, onda de ≈ 159 kHz),
  muy por encima de Nyquist a 48 kHz: **muestrear el nivel en un instante por muestra produce *aliasing***. Los
  motores PWM dependen de que el promedio del nivel en el intervalo de muestra sea el valor oído.
- El nivel analógico depende de EAR y MIC juntos (y en la entrada de cinta, del *issue* de la placa). FUSE usa cuatro
  niveles según (EAR, MIC) **(verificar los valores y si el 128K/+2 se comportan como issue 3)**. Se modela con una
  tabla de 4 entradas indexada por `(value >> 3) & 3`.
- La entrada EAR (señal de cinta) también se oye en el altavoz real durante la carga (el "ruido de carga"). Mezclarla
  es opcional (sección 6.4).

### 3.2 AY-3-8912 (128K y +2 gris)

- Reloj: 1,77345 MHz = reloj de CPU (3,5469 MHz) / 2.
- Puertos (decodificación parcial, `Docs/maquinas-sinclair-es.md` sección 5): `0xFFFD` (A15 = 1, A14 = 1, A1 = 0)
  escritura = selección de registro, lectura = registro seleccionado; `0xBFFD` (A15 = 1, A14 = 0, A1 = 0) escritura
  del dato.
- 16 registros:

| Reg | Contenido | Bits útiles |
|---|---|---|
| 0–1, 2–3, 4–5 | Periodo de tono A, B, C (12 bits: fino + grueso) | R1/R3/R5: 4 bits |
| 6 | Periodo de ruido | 5 bits |
| 7 | Mezclador: bits 0–2 tono A/B/C desactivado, 3–5 ruido desactivado, 6–7 dirección de los puertos de E/S | 8 bits |
| 8–10 | Amplitud A/B/C: bits 0–3 nivel fijo, bit 4 = usar envolvente | 5 bits |
| 11–12 | Periodo de envolvente (16 bits) | 8 + 8 |
| 13 | Forma de envolvente (CONT, ATT, ALT, HOLD); **escribirlo reinicia la envolvente** | 4 bits |
| 14 | Puerto de E/S A (RS232/MIDI y keypad en el 128K) | 8 bits |
| 15 | Puerto B (no existe en el 8912; lectura **(verificar)**) | — |

- Los bits no implementados se leen como 0 en el AY (en el YM2149 no) **(verificar con la hoja de datos y FUSE)**:
  se aplica una máscara por registro al escribir.
- Fórmulas (hoja de datos): tono `f = clk / (16 · TP)`; ruido `f = clk / (16 · NP)`; envolvente: cada uno de sus
  16 pasos dura `16 · EP` ciclos de reloj del AY (periodo completo `256 · EP`). Periodo 0 se comporta como 1
  **(verificar en tono, ruido y envolvente)**.
- Ruido: LFSR de 17 bits (realimentación de los bits 0 y 3) que avanza a la mitad de la frecuencia del tono con el
  mismo periodo **(verificar el divisor exacto)**.
- Salida por canal: `(tono | tono_desactivado) & (ruido | ruido_desactivado)` selecciona entre 0 y el volumen; el
  volumen es el fijo (4 bits) o el de la envolvente (4 bits en el AY; 5 en el YM). La escala de volumen es
  logarítmica (≈ 3 dB por paso): se usa una tabla de 16 niveles medidos (FUSE, MAME o Ayumi) **(elegir fuente y
  registrar los valores en esta especificación)**.
- En el 128K/+2 la salida de los tres canales se suma (mono) con el beeper hacia el modulador/monitor. La proporción
  beeper/AY se toma de FUSE **(verificar)**.

### 3.3 Reloj interno del AY respecto a la CPU

El AY no necesita simularse a 1,77 MHz. Sus contadores de tono avanzan cada 8 ciclos de su reloj (un semiperiodo
de tono son `TP` de esos pasos), es decir, **un paso del AY cada 16 T de la CPU** (221 681 pasos/s). Por frame del
128K son 70 908 / 16 = 4 431,75 pasos: se lleva un resto entre frames. Ruido y envolvente se derivan del mismo paso
(ruido cada 2 pasos, envolvente cada `EP` pasos de 16 ciclos, según la sección 3.2) **(verificar los divisores
contra MAME/FUSE)**.

### 3.4 ZX80 y ZX81

**No tienen altavoz ni generador de sonido.** El modulador de TV solo lleva vídeo. Todo lo audible sale por la
**salida de cinta** (MIC), que en ambos es la propia señal de sincronía vertical
(`Docs/maquinas-sinclair-es.md` secciones 2 y 3, `Docs/zx81-video-es.md`):

- `IN` del puerto 0xFE (A0 = 0) **activa VSYNC** y pone la salida de cinta a nivel bajo. En el ZX80, siempre; en el
  ZX81, solo con el generador de NMI desactivado.
- Cualquier `OUT` **termina VSYNC** y devuelve la salida de cinta a nivel alto.
- Lo que se oye con un amplificador o un magnetófono conectado a esa salida:
  - el zumbido de 50/60 Hz de los VSYNC normales de cada frame;
  - los pulsos de `SAVE` (trenes de ≈ 150 µs, 4 o 9 por bit, `Docs/maquinas-sinclair-es.md` sección 3);
  - los programas que hacen "música" alternando `IN`/`OUT` temporizados, normalmente en FAST;
  - en alta resolución WRX, un VSYNC muy corto en cada línea (cada 207 T), es decir, un tono de ≈ 15,7 kHz.
- El nivel tiene dos estados (alto/bajo). Se usa la misma tabla de niveles que el beeper, pero con 2 entradas.
- Por defecto el sonido del ZX80/ZX81 está **desactivado**, porque la máquina real no suena por la TV. Una opción
  activa "salida de cinta" y otra "entrada de cinta" (el sonido de la carga, igual que en el Spectrum).

**Tarjetas AY para el ZX81**: hay ampliaciones con un AY-3-8910/8912 que muchos juegos y demos modernos usan:

| Tarjeta | Puertos (según EightyOne) | Reloj del AY |
|---|---|---|
| ZON X-81 (Bi-Pak) | Selección de registro 0xDF, dato 0x0F **(verificar)**. ZXpand+ la emula **(verificar)** | **(verificar)**: propio o reloj de CPU / 2 = 1,625 MHz |
| Quicksilva Sound Board | Mapeada en memoria **(verificar direcciones)** | **(verificar)** |

El sintetizador AY de la sección 6.3 sirve sin cambios. Solo cambian la decodificación en el bus del ZX81 y la
relación entre el reloj del AY y la CPU (el paso del AY deja de ser exactamente 16 T si el reloj no es CPU / 2).
Por eso la relación se parametriza (sección 5).

**El frame no es fijo.** En el ZX80/ZX81 el frame lo define el software con sus VSYNC
(`Docs/zx81-video-es.md`): no hay ULA que marque 69 888 T. El sonido, por tanto, **no puede depender del frame**.
El mezclador trabaja por tramos de T-states de longitud arbitraria (sección 4.4), y `RunFrame()` del ZX81 ejecuta un
presupuesto de T-states (≈ 65 000, o hasta el VSYNC con un límite de seguridad, a decidir en la especificación de la
máquina). El número de muestras sale siempre de los T emulados, con la base de tiempo exacta de la sección 5
(3 250 000 Hz: ≈ 67,7 T por muestra a 48 kHz).

## 4. Principio de diseño: eventos por `OUT`, síntesis por frame

Se descartan tres alternativas:

| Alternativa | Por qué no |
|---|---|
| Muestrear el nivel en cada instrucción o T-state | Trabajo en el camino por instrucción; el *aliasing* sigue sin resolverse |
| Generar muestras dentro de cada `OUT` (render incremental hasta el T actual) | Bucles y ramas dentro del `OUT`; los motores 1-bit hacen miles de `OUT` por frame |
| Simular el AY ciclo a ciclo en el bus | 1,77 M actualizaciones por segundo para un resultado que se diezma a 48 kHz |

**Se elige registrar eventos.** Cada `OUT` que cambia algo audible añade un registro `(t, dato)` a un array
preasignado del frame; al terminar el frame, una pasada separada sintetiza las muestras. Ventajas:

- Coste en el camino caliente: una comparación y, si hay cambio, una escritura de 8 bytes en un array. Sin
  interfaces, sin llamadas virtuales, sin reservas.
- La síntesis es un bucle compacto, independiente de la CPU, medible con BenchmarkDotNet y probado con xUnit sin
  ejecutar Z80.
- Con audio silenciado o en turbo se puede omitir la síntesis entera (sección 7.4).

### 4.1 Capacidad del registro de eventos

Cada `OUT` produce como mucho un evento y el `OUT` más corto dura 11 T (`OUT (n),A`; `OUT (C),r` 12 T, `OUTI` 16 T).
Capacidad = `TStatesPerFrame / 11 + margen` (≈ 6 450 para el 128K con margen de 4 eventos por el exceso del último
frame), así que **el array no puede desbordarse** y no hace falta comprobar el límite en el `OUT`. El tamaño se
calcula al construir la máquina. Con `int t` y `int dato` son ≈ 52 KB por máquina.

Formato propuesto (dos arrays paralelos o un array de `struct` de 8 bytes, a decidir por medida):

| Campo | Beeper | AY |
|---|---|---|
| `t` | T del frame en que el `OUT` escribe el dispositivo | Igual |
| `dato` | Nivel (0–3, índice de la tabla EAR/MIC) | `registro << 8 | valor` |

Beeper y AY pueden ir en registros separados (dos arrays, dos contadores) para que cada sintetizador recorra solo
los suyos. Recomendado: separados.

### 4.2 Punto exacto del cambio

El dispositivo se escribe en `SpectrumBus.Out` después de `IoEarly`, así que `machine.tstates` en ese punto es el T
del cambio y ya incluye la contención. **(verificar)** frente a FUSE el T exacto en que el altavoz cambia dentro del
ciclo de E/S (inicio, T2 o T3); un desfase constante de 1–3 T no es audible, pero se documenta.

### 4.3 Cambios en el bus (sin implementar)

```csharp
// SpectrumBus.Out (esquema)
IoEarly(highContended);
if (ula)
    machine.WriteUlaPort(data);           // + registro del evento si cambian EAR/MIC
if ((port & PagingPortMask) == 0)
    machine.Paging?.Write(data);
if ((port & AyPortMask) == AyPortSelect)  // 0xC002 → 0xC000 (0xFFFD) y 0x8000 (0xBFFD)
    machine.Ay?.Write(port, data, machine.tstates);
IoLate(highContended, ula);
```

- `WriteUlaPort` compara `(data >> 3) & 3` con el último nivel y solo entonces añade el evento. Los juegos escriben
  el borde con mucha frecuencia sin cambiar el altavoz: esos `OUT` no generan eventos.
- El AY guarda el valor en su banco de registros **inmediatamente** (la lectura de 0xFFFD debe devolverlo y los
  juegos lo hacen) y añade el evento solo para registros audibles (0–13). La selección (0xFFFD) no genera evento.
- `In` de 0xFFFD devuelve el registro seleccionado en el 128K/+2 en lugar del bus flotante.
- En 16K/48K `machine.Ay` es `null`: el patrón es el mismo que `Paging?` y su coste ya está medido en los benchmarks.
  Si la medida lo desaconseja, la alternativa es un `SpectrumBus` distinto por familia (genérico sobre la máquina),
  decisión que se tomaría con el benchmark de la sección 8.
- El AY se reinicia con la máquina (todos los registros a 0, mezclador 0 = todo activado pero volúmenes 0).

### 4.4 Frontera de frame (tramo)

La CPU ejecuta hasta superar `TStatesPerFrame` y `EndFrame()` resta la longitud del frame. Los eventos con
`t ≥ TStatesPerFrame` (del exceso de la última instrucción) pertenecen al frame siguiente: la síntesis procesa solo
`t < TStatesPerFrame`, y los restantes (como mucho uno o dos) se copian al principio del array con `t` rebajado.
El estado de cada sintetizador (nivel actual del beeper, contadores y fase del AY, filtros) se conserva entre frames.

La síntesis no supone una longitud de frame: recibe la longitud del tramo en T (`EndChunk(int tstates)`). En el
Spectrum el tramo es siempre `TStatesPerFrame`. En el ZX80/ZX81 es el presupuesto que haya ejecutado `RunFrame()`
(sección 3.4).

### 4.5 ZX80/ZX81: eventos de la salida de cinta

El bus del ZX80/ZX81 (pendiente de especificar; `Specs/spec-buses-memoria.md` solo fija su memoria) ya tiene que
seguir el estado de VSYNC para el vídeo. El sonido solo añade un evento cuando ese estado **cambia**: en el `IN` que
activa VSYNC y en el `OUT` que lo termina. Se aplica la misma cota de capacidad (≥ 11 T por `IN`/`OUT`). La ROM hace
`OUT` frecuentes (activar/desactivar la NMI) que no cambian el nivel y no generan eventos. Con el sonido desactivado
por defecto, el registro se mantiene igual (sin rama extra) y solo se omite la síntesis (sección 7.4).

Las tarjetas AY del ZX81 usan el mismo `Ay38912` y su registro de eventos, conectados en el bus del ZX81 según la
tarjeta configurada (nula si no hay ninguna, como `machine.Ay?` en el Spectrum).

## 5. Base de tiempo común: T-states a muestras

- Hace falta la frecuencia de reloj en la máquina: se añade `CpuClockHz` a `MachineTiming` (3 500 000 en 16K/48K,
  3 546 900 en 128K/+2, 3 250 000 en ZX80/ZX81).
- El AY recibe su relación de reloj con la CPU como parámetro: 16 T por paso cuando su reloj es CPU / 2 (128K/+2),
  u otra fracción entera si una tarjeta del ZX81 tiene reloj propio. Se usa el mismo acumulador entero.
- La conversión usa **aritmética entera exacta**: el tiempo se mide en unidades de `T · sampleRate`; cada muestra
  ocupa `CpuClockHz` unidades. Con 48 000 Hz: una muestra = 3 500 000 unidades, un T = 48 000 unidades. No hay
  deriva ni redondeo acumulado: tras N frames, el número de muestras entregadas es exactamente
  `floor(N · TStatesPerFrame · sampleRate / CpuClockHz)` (958 o 959 por frame en el 48K a 48 kHz, como fija
  [spec-frontend-blazor.md](spec-frontend-blazor.md) 6.2).
- Los productos caben en `long`; dentro del bucle de síntesis basta `int` relativo al inicio de la muestra
  **(verificar rangos con 96 kHz)**.
- Para el AY, la misma idea con su paso de 16 T: cada muestra a 48 kHz contiene 73,9 T ≈ 4,6 pasos del AY.

## 6. Síntesis

### 6.1 Beeper: integración por muestra (fase 1)

Cada muestra es **el nivel medio del altavoz en su intervalo** (filtro de caja), calculado exactamente con los
eventos: se suma `nivel × duración` de cada tramo y se divide por la duración de la muestra (multiplicación por una
constante precalculada). Coste O(muestras + eventos) por frame, sin ramas por T-state. Atenúa el *aliasing* de forma
suficiente para la mayoría de juegos y reproduce el PWM de los motores 1-bit.

### 6.2 Beeper: escalones con banda limitada (fase de calidad)

El filtro de caja deja *aliasing* audible con tonos agudos. La mejora es BLEP / *Blip_Buffer*: cada cambio de nivel
suma un escalón de banda limitada (núcleo precalculado de 8–16 coeficientes por fase, con 32–64 fases) a un buffer
de deltas, que se integra al final. Coste ≈ 16 multiplicaciones por evento; con 6 000 eventos por frame en el peor
caso son ≈ 100 000 operaciones por frame **(medir antes de adoptarlo)**. Se implementa detrás de la misma interfaz
interna de la sección 6.1 para compararlos.

### 6.3 AY

1. Se recorren los pasos del AY del frame (≈ 4 432). Entre eventos, cada paso actualiza tres contadores de tono, el
   de ruido (y el LFSR cuando toca) y el de envolvente: sumas, comparaciones y XOR, sin divisiones.
2. Cuando el `t` de un evento cae dentro del paso actual se aplica la escritura al estado del sintetizador (no al
   banco de registros del bus, que ya está al día). Escribir R13 reinicia la envolvente.
3. Salida del paso: suma de los tres canales con la tabla de volumen.
4. **Diezmado** a la frecuencia de muestreo: promedio de los pasos que caen en cada muestra (caja, con el peso
   fraccionario del paso que cruza la frontera). Fase de calidad: FIR polifásico o BLEP también en el AY.

Coste estimado: ≈ 4 432 pasos × (3 canales + ruido + envolvente) ≈ 30 000–50 000 operaciones simples por frame,
frente a ~70 000 T de CPU que cuestan bastante más cada uno. **(medir)** Las 8 formas de envolvente distintas
(de las 16 combinaciones de R13) se pueden precalcular como tablas de 32 pasos para quitar ramas.

Optimización posible, solo si la medida lo pide: los canales en silencio (volumen 0 sin envolvente) o con tono y
ruido desactivados y volumen fijo dan salida constante; un frame sin eventos y con todos los canales constantes se
resuelve sin recorrer pasos.

### 6.4 Mezcla y filtros de salida

- `beeper[nivel]` (tabla de 4 niveles, sección 3.1) + `ay` (suma de canales normalizada) + opcional `cinta`
  (EAR de entrada a volumen bajo) con pesos configurables; resultado escalado para que el máximo teórico no pase
  de 1.
- **Filtro paso alto de un polo** (bloqueo de continua, ≈ 10–20 Hz): el beeper es unipolar y un nivel fijo después
  de un `OUT` produciría un desplazamiento permanente y un clic al silenciar. Se aplica tras mezclar, por muestra.
- Paso bajo opcional (emulación del altavoz/TV) para suavizar; desactivado por defecto **(decidir con escucha)**.
- La salida se escribe en un `float[]` preasignado del tamaño máximo de muestras por frame (+1). La interfaz del
  Core es `ReadOnlySpan<float> AudioSamples` ([spec-frontend-blazor.md](spec-frontend-blazor.md) 8).

### 6.5 Estéreo (opcional, posterior)

El 128K original es mono. Las mezclas ABC/ACB son habituales en emuladores; si se añaden, el formato de salida pasa
a intercalado (L, R) y el worklet usa dos canales. Duplica el volumen de datos por frame (≈ 7,7 KB). Queda fuera
de la primera implementación; el contrato reserva un `AudioChannels` (1 por defecto).

## 7. Rendimiento en el navegador (Web Worker)

### 7.1 Dónde se ejecuta cada parte

| Hilo | Trabajo de audio |
|---|---|
| Worker (.NET AOT) | Registro de eventos durante `RunFrame()`; síntesis al final del frame; `pushAudio` |
| Worker (JS) | Copia del `Span` de WASM al ring buffer SAB, o al `Float32Array` transferible |
| Hilo de audio (`AudioWorklet`) | Solo copia 128 muestras por cuántum; no emula ni filtra |
| Principal | Crea el `AudioContext` tras un gesto del usuario; nada por frame |

La síntesis en C# en lugar de en el worklet es deliberada: el worklet tiene un presupuesto de ≈ 2,7 ms por cuántum
de 128 muestras a 48 kHz y corre en un hilo de tiempo real donde un pico de GC de JS o un trabajo variable produce
cortes. Además, el estado del AY vive en el Core (lecturas de 0xFFFD) y en C# se prueba con xUnit.

### 7.2 Coste por frame y presupuesto

- Presupuesto de [spec-frontend-blazor.md](spec-frontend-blazor.md) 10: `RunFrame()` ≤ 3 ms con AOT. El sonido debe
  quedar **por debajo del 10 % de `RunFrame()`** en el peor caso (motor 1-bit + AY con envolvente) **(objetivo a
  confirmar con medidas)**.
- Interop: un único cruce `pushAudio(Span<byte>)` por frame con `MemoryView` (≈ 3,8 KB en mono `float`). JS copia
  con `set` sobre una vista preasignada; sin `new` por frame. La memoria de WASM no es el `SharedArrayBuffer` (sin
  `WasmEnableThreads`), así que esa copia es inevitable y es barata.
- Se mantiene `Float32`: Web Audio trabaja en `float` y convertir desde `Int16` en el worklet movería trabajo al
  hilo de tiempo real a cambio de ahorrar 1,9 KB por frame.
- SIMD: el diezmado y el filtro admiten `Vector128` (WASM SIMD activo por defecto), pero no se introduce hasta que
  la medida lo justifique.
- Las estadísticas del worker (`stats`, [spec-frontend-blazor.md](spec-frontend-blazor.md) 7) añaden ms medios y
  máximos de síntesis por frame, eventos máximos por frame y faltas de audio.

### 7.3 Reloj maestro y frecuencia de muestreo

- Como el consumo del worklet decide cuándo se emula un frame (spec del front end 4.2), el emulador produce
  exactamente las muestras que consume el dispositivo: **no hace falta remuestreo dinámico**. La velocidad de
  emulación se desvía de la real en la deriva del reloj de la tarjeta de sonido (ppm), imperceptible.
- La frecuencia la fija el `AudioContext` (44 100, 48 000 o 96 000 Hz según el dispositivo) y se pasa en `init`. El
  Core no asume ninguna. No se fuerza `sampleRate` en el constructor del `AudioContext` **(verificar en Safari, que
  ha tenido problemas con frecuencias forzadas)**.
- Latencia objetivo: 2 frames (≈ 40 ms) en el ring buffer, más la del `AudioContext` (`latencyHint: 'interactive'`,
  `outputLatency` informativo).
- Si la pestaña pasa a segundo plano, los temporizadores del worker pueden ralentizarse; el anillo se vacía y el
  worklet atenúa a silencio contando faltas **(verificar el comportamiento de `setTimeout` en workers ocultos en
  Chrome, Firefox y Safari; si se pausa la emulación al ocultarse, se suspende también el `AudioContext`)**.

### 7.4 Modos sin audio

- Antes del primer gesto, silenciado o en turbo: el bus sigue registrando eventos (el `OUT` no lleva una rama más) y
  el banco de registros del AY se mantiene, pero `RunFrame()` no sintetiza; solo vacía los contadores de eventos y
  actualiza el estado mínimo (último nivel del beeper; contadores del AY avanzados en bloque o reiniciados al
  reanudar **(decidir)**). Así el turbo no paga el sonido.
- Al reanudar el audio se rellena el anillo hasta el objetivo antes de dejar que el worklet consuma, para evitar una
  falta inicial.

## 8. Contrato propuesto en el Core

Clases concretas y `sealed`, sin interfaces en el camino caliente:

| Tipo | Ubicación | Papel |
|---|---|---|
| `BeeperRecorder` (o dentro de `SpectrumMachine`) | `Machines/Spectrum/` | Array de eventos del beeper y último nivel |
| `Ay38912` | `Sound/` | Banco de registros, selección, lectura, array de eventos; usado por el bus |
| `AySynthesizer` | `Sound/` | Estado de tono/ruido/envolvente, síntesis y diezmado del frame |
| `BeeperSynthesizer` | `Sound/` | Integración (y BLEP en la fase de calidad) |
| `SoundMixer` | `Sound/` | Mezcla, paso alto, buffer `float[]` de salida, base de tiempo de la sección 5 |

- `SpectrumMachine` crea `Ay38912` solo en 128K/+2 y expone `Ay` (nulo en 16K/48K).
- La futura máquina ZX80/ZX81 reutiliza `BeeperSynthesizer` (nivel de la salida de cinta, 2 niveles), `Ay38912`/
  `AySynthesizer` (tarjetas opcionales) y `SoundMixer`. El mezclador no conoce el modelo de máquina.
- `SpectrumEmulator.RunFrame()` (spec del front end 8): CPU hasta fin de frame → `SoundMixer.EndFrame(frameT)` →
  vídeo → `EndFrame()`. `AudioEnabled` controla si se sintetiza.
- Nada de esto depende de JS ni de Blazor: sirve igual para un front end de escritorio (NAudio, SDL) o para generar
  WAV en pruebas.

## 9. Pruebas y medidas previstas

### 9.1 xUnit (Core)

- Base de tiempo: muestras acumuladas en N frames = `floor(N · T_frame · rate / clock)` exacto para 44 100, 48 000 y
  96 000 Hz, 48K y 128K.
- Beeper: nivel constante → muestras constantes (antes del paso alto); un cambio a mitad de muestra → valor medio
  exacto; onda cuadrada de periodo conocido → frecuencia fundamental y amplitud media; PWM 25 % → nivel 0,25.
- Eventos: un `OUT` que no cambia EAR/MIC no genera evento; contención incluida en `t`; eventos del exceso pasan al
  frame siguiente; capacidad suficiente con un bucle de `OUT (n),A` durante un frame completo.
- AY: máscaras de bits por registro y lectura por 0xFFFD; selección por decodificación parcial (puertos espejo);
  periodo de tono → número de pasos por semiperiodo; periodo 0; secuencia del LFSR; las 16 formas de R13 (8 distintas)
  y el reinicio al escribir R13; mezclador (tono/ruido desactivados).
- Sin reservas: `GC.GetAllocatedBytesForCurrentThread` sin cambios tras 1 000 frames con sonido.
- ZX80/ZX81: `IN 0xFE` con la NMI desactivada genera el evento de nivel bajo y con la NMI activa (ZX81) no lo
  genera; un `OUT` que no cambia el nivel no genera evento; muestras correctas con tramos de longitud variable;
  tono WRX de 207 T promediado por el filtro de caja.
- Determinismo: la misma secuencia de `OUT` produce las mismas muestras bit a bit.
- Opcional: comparación con salidas de referencia (WAV generados por FUSE para un programa de prueba) con tolerancia,
  si se consigue una forma reproducible de obtenerlos.

### 9.2 BenchmarkDotNet

- `SynthesizeBeeperFrame` (0, 100 y ~6 000 eventos) y `SynthesizeAyFrame` (silencio, tonos, ruido + envolvente),
  en ns/frame y 0 B reservados.
- Los benchmarks de CPU existentes (`*Z80Cpu*`, `*Bus*`) antes y después de añadir el registro de eventos en `Out`:
  los intervalos deben solaparse con el control (mismo criterio que `spec-cpu-z80.md` 8.4).
- Un `RunFrame` con un programa que haga beeper intensivo, con y sin síntesis.

### 9.3 Navegador

- Estadísticas del worker: ms de síntesis por frame, faltas de audio en 10 minutos (criterio de
  [spec-frontend-blazor.md](spec-frontend-blazor.md) 10), con AOT y con intérprete.
- Ampliar el prototipo (`ZXSinclair.Net.Web.Prototype`, [prototipo-web-worker-render.md](prototipo-web-worker-render.md))
  con un botón de audio antes del Core completo: tono sintético generado en C# dentro del worker, enviado al worklet
  por SAB y por `postMessage`, para medir el cruce `pushAudio`, la latencia y las faltas sin depender de la emulación.

## 10. Fases

| # | Fase | Resultado |
|---|---|---|
| S0 | Prototipo de audio en el worker (sección 9.3) | Cadena C# → worker → worklet medida en SAB y `postMessage` |
| S1 | `CpuClockHz`, eventos del beeper en `Out`, integración por muestra, paso alto, `SoundMixer` | Beeper del 48K con xUnit y benchmarks; sin regresión en CPU |
| S2 | Integración en `SpectrumEmulator` y fase 2 de [spec-frontend-blazor.md](spec-frontend-blazor.md) | Beeper audible con el audio como reloj maestro |
| S3 | `Ay38912` (puertos, registros, lectura) y `AySynthesizer` | Sonido del 128K/+2 |
| S4 | Calidad: BLEP en beeper, diezmado FIR en el AY, sonido de cinta, estéreo opcional | Solo lo que justifiquen escucha y medidas |
| S5 | ZX80/ZX81: eventos de la salida de cinta en su bus, tramos de longitud variable; después, tarjetas ZON X-81 / Quicksilva | Depende de la especificación de bus y vídeo del ZX81 |

## 11. Decisiones abiertas y pendiente de verificar

- Niveles del beeper por (EAR, MIC) e *issue*; si el 128K/+2 equivalen a issue 3.
- T exacto del cambio del altavoz dentro del ciclo de E/S frente a FUSE.
- Divisores internos del AY (tono, ruido, envolvente) y comportamiento con periodo 0.
- Tabla de volumen del AY (fuente medida) y proporción beeper/AY en el 128K/+2.
- Máscaras de bits y lectura de R15 en el AY-3-8912.
- Uno o dos arrays de eventos; array de `struct` frente a arrays paralelos (medir).
- `machine.Ay?` en el bus frente a un bus específico por familia (medir con los benchmarks de CPU).
- Filtro de caja frente a BLEP como opción por defecto (medir y escuchar).
- Comportamiento de los temporizadores del worker y del `AudioContext` con la pestaña oculta.
- Qué hace el AY durante el turbo (avanzar contadores o reiniciar fase al reanudar).
- ZX80/ZX81: polaridad real de la salida de cinta respecto a VSYNC; puertos, mapeo y reloj de ZON X-81 y Quicksilva;
  presupuesto de T por `RunFrame()` (fijo o hasta VSYNC).
