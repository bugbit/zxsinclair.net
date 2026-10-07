# Máquinas Sinclair: dispositivos de E/S y sincronización con la CPU

Resumen de referencia, a partir de fuentes externas, de los dispositivos de entrada/salida de cada máquina Sinclair, de cómo se sincronizan con los ciclos (T-states) del Z80 y de cada cuánto lee el teclado la ROM. Complementa a `Specs/spec-buses-memoria.md` (mapas, contención y paginación), que es la especificación del Core; si hay discrepancia, manda la especificación.

Las cifras marcadas **(verificar)** no se han contrastado con una segunda fuente ni con hardware.

Fuentes principales: *The Complete Spectrum ROM Disassembly* (Logan y O'Hara), *The ZX81 ROM disassembly* / *The Complete Timex TS1000/Sinclair ZX81 ROM Disassembly* (Logan y O'Hara), [Sinclair Wiki](https://sinclair.wiki.zxnet.co.uk/), FAQ de World of Spectrum y el código de FUSE.

## 1. Resumen

| Máquina | Reloj CPU | T/línea | T/frame | Frame | Interrupción de vídeo | Lectura de teclado de la ROM |
|---|---|---|---|---|---|---|
| ZX80 | 3,25 MHz | 207 **(verificar)** | ≈ 65 000, por software **(verificar)** | ≈ 50 Hz | No hay generador; el frame lo genera la ROM | Una vez por frame mostrado (≈ 20 ms), solo mientras espera una tecla |
| ZX81 SLOW | 3,25 MHz | 207 | ≈ 65 000, depende de `MARGIN` **(verificar)** | 50 Hz (60 Hz en EE. UU.) | NMI cada 207 T (líneas) + INT por A6 (fin de línea de texto) | Una vez por frame, en la sincronía vertical: 20 ms (16,7 ms a 60 Hz) |
| ZX81 FAST | 3,25 MHz | — | — | Sin imagen al ejecutar | NMI desactivada | Solo mientras la ROM muestra la pantalla esperando una tecla; `BREAK` tras cada sentencia |
| Spectrum 16K/48K | 3,5 MHz | 224 | 69 888 | 50,08 Hz | INT de la ULA al principio del frame, 32 T | En cada INT (IM 1): cada 69 888 T = 19,97 ms |
| Spectrum 128K / +2 gris | 3,5469 MHz | 228 | 70 908 | 50,02 Hz | INT de la ULA, 36 T **(verificar)** | En cada INT: cada 70 908 T = 19,99 ms |
| Spectrum +2A/+2B/+3 | 3,5469 MHz | 228 | 70 908 | 50,02 Hz | INT del gate array, 32 T **(verificar)** | En cada INT: cada 70 908 T = 19,99 ms |

En todos los Spectrum, la tecla `BREAK` (CAPS SHIFT + SPACE) se lee además de forma síncrona, fuera de la interrupción: tras cada sentencia BASIC y dentro de los bucles de cinta y `BEEP`.

## 2. ZX80

### Reloj y vídeo

- Z80A a 3,25 MHz (cristal de 6,5 MHz / 2).
- No hay ULA de vídeo con memoria propia: **la CPU genera la imagen**. La ROM salta al fichero de pantalla (D-FILE) con A15 = 1; cuando hay un M1 en esa zona y el bit 6 del dato es 0, la lógica captura el código de carácter y pone un `NOP` en el bus. El código capturado, el registro `I` y el contador de línea forman la dirección del patrón en la ROM, que se lee durante el ciclo de refresco.
- `HALT` (0x76, bit 6 = 1) termina cada línea de texto; la línea de A6 conectada a INT (cuando el bit 6 de `R` pasa a 0) devuelve el control a la ROM. Por eso la temporización de cada línea depende de valores de `R` cargados por la ROM.
- **No hay NMI ni generador de frames**: la imagen solo existe mientras la ROM ejecuta su bucle de pantalla. Al ejecutar un programa o procesar una tecla la pantalla se queda en blanco (el famoso parpadeo del ZX80).

### Puertos

| Acceso | Decodificación | Efecto |
|---|---|---|
| `IN` puerto 0xFE (A0 = 0) | A8–A15 eligen semifila | Bits 0–4 teclado (0 = pulsada); bit 7 entrada de cinta. **Activa la sincronía vertical** |
| `OUT` a cualquier puerto | — | **Desactiva la sincronía vertical** y reinicia el contador de línea; el nivel de salida de cinta sigue al estado de VSYNC |

La salida de cinta es la propia señal de sincronía: la ROM genera los pulsos con secuencias `IN`/`OUT` temporizadas por software.

### Teclado según la ROM

- La ROM lee las 8 semifilas al generar cada sincronía vertical (la propia lectura de 0xFE produce el VSYNC), es decir, **una vez por frame (≈ 20 ms) y solo mientras está mostrando la pantalla a la espera de una tecla**.
- Durante la ejecución de un programa no hay barrido de teclado periódico **(verificar cómo detecta `BREAK` la ROM del ZX80)**.

## 3. ZX81 (y TS1000)

### Reloj y vídeo

- Z80A a 3,25 MHz.
- Línea horizontal de **207 T** (64 µs); la ULA genera HSYNC e incrementa su contador de línea (0–7) en cada una.
- Mismo principio que el ZX80 (la CPU "ejecuta" el D-FILE, `NOP` forzado, `HALT` al final de línea, INT por A6 con IM 1 a 0x0038), más un **generador de NMI** en la ULA:
  - Activado: NMI cada 207 T. La rutina NMI (0x0066) cuenta las líneas en blanco de los márgenes superior e inferior y devuelve el control al programa del usuario entre línea y línea.
  - Las líneas de texto (192 de imagen) se generan ejecutando el D-FILE.
- El número de líneas de margen sale de la variable `MARGIN` (55 a 50 Hz, 31 a 60 Hz), que la ROM fija al arrancar según el enlace 50/60 Hz. El frame total ronda 3 250 000 / 50 ≈ 65 000 T **(verificar el recuento exacto)**.
- **Modo SLOW**: NMI activa; el programa de usuario solo avanza durante los márgenes (≈ 25 % del tiempo).
- **Modo FAST**: NMI desactivada; sin imagen mientras se ejecuta.

### Puertos

| Acceso | Decodificación | Efecto |
|---|---|---|
| `IN` 0xFE (A0 = 0) | A8–A15 eligen semifila | Bits 0–4 teclado; bit 6 enlace 50/60 Hz; bit 7 entrada de cinta. **Con la NMI desactivada, activa VSYNC** |
| `OUT` 0xFE (A0 = 0) | — | Activa el generador de NMI |
| `OUT` 0xFD (A1 = 0) | — | Desactiva el generador de NMI |
| `OUT` a cualquier puerto | — | Termina VSYNC y reinicia el contador de línea |
| 0xFB (A2 = 0) | — | ZX Printer (ver sección 7) |

Cinta: la ROM graba cada bit como un tren de pulsos (≈ 150 µs a nivel alto y 150 µs bajo cada uno): 4 pulsos para un 0 y 9 para un 1, separados por ≈ 1 300 µs de silencio **(verificar)**. Toda la temporización es por software (sin interrupciones durante `SAVE`/`LOAD`).

### Teclado según la ROM

- La rutina `KEYBOARD` (0x02BB) lee las 8 semifilas con `IN A,(C)` partiendo de `BC = 0xFEFE`. La ROM la llama en la parte de la rutina de pantalla que genera la **sincronía vertical**: esa misma lectura del puerto 0xFE produce el VSYNC.
- **SLOW**: una lectura por frame → **cada 20 ms a 50 Hz (16,7 ms a 60 Hz)**. En el mismo punto se decrementa `FRAMES` y se actualizan `LAST_K` y `DEBOUNCE` (antirrebote por frames).
- **FAST**: no hay barrido periódico. Cuando la ROM espera una tecla (`INPUT`, editor, `PAUSE`) muestra la pantalla con el mismo bucle de frames y lee el teclado en cada uno. Durante la ejecución, `BREAK` se comprueba tras cada sentencia leyendo directamente la semifila de SPACE (`LD A,0x7F` / `IN A,(0xFE)`).

## 4. ZX Spectrum 16K / 48K

### Reloj y vídeo

- Z80A a 3,5 MHz (14 MHz / 4).
- 224 T por línea × 312 líneas = **69 888 T por frame** (50,08 Hz).
- La ULA activa **INT al principio del frame (T 0) durante 32 T**. La ROM usa IM 1 (0x0038). Si la CPU está en `DI` o en una instrucción larga que se extiende más allá de los 32 T, la interrupción se pierde.
- Primer píxel en T 14 336 (línea 64); primer T contenido 14 335. Contención de memoria 0x4000–0x7FFF y de E/S con patrón 6,5,4,3,2,1,0,0 (detalle en `Specs/spec-buses-memoria.md`).
- Bus flotante: leer un puerto no decodificado devuelve el byte que la ULA está leyendo de la memoria de vídeo en ese T.

### Puertos

| Puerto | Decodificación | Lectura | Escritura |
|---|---|---|---|
| ULA 0xFE | A0 = 0 | Bits 0–4 teclado (A8–A15 eligen semifilas; AND si hay varias), bit 6 EAR, bits 5 y 7 a 1 | Bits 0–2 borde, bit 3 MIC (cinta), bit 4 EAR (altavoz) |
| No decodificado | A0 = 1, sin periférico | Bus flotante | — |

- Bit 6 en lectura: en el Issue 2 depende de los bits 3 y 4 escritos; en Issue 3 solo del bit 4 **(verificar umbrales)**.
- Altavoz y cinta se manejan por software con temporización exacta en T-states; el emulador debe muestrear EAR y registrar cambios de MIC/EAR en el T del `OUT`.

### Temporización de cinta en la ROM

| Elemento | T-states |
|---|---|
| Pulso de guía | 2 168 (8 063 pulsos en cabecera, 3 223 en datos) |
| Sincronía | 667 + 735 |
| Bit 0 / bit 1 | 2 × 855 / 2 × 1 710 |
| Bucle de muestreo `LD-SAMPLE` | 59 por iteración |

`LD-BYTES` (0x0556) desactiva las interrupciones durante la carga, así que el teclado no se barre; `LD-EDGE` comprueba SPACE en cada muestreo para abortar con `BREAK`.

### Teclado según la ROM

- La interrupción (0x0038, `MASK-INT`) incrementa `FRAMES` (3 bytes) y llama a `KEYBOARD` (0x02BF), que usa `KEY-SCAN` (0x028E) para leer las 8 semifilas con `IN A,(C)` partiendo de `BC = 0xFEFE`.
- **Frecuencia: una lectura por interrupción = cada 69 888 T = 19,97 ms (50,08 Hz).**
- Antirrebote y repetición contados en interrupciones:
  - `KSTATE` guarda dos teclas con un contador de 5 interrupciones; si la tecla deja de verse 5 veces seguidas, la entrada se libera (≈ 100 ms).
  - `REPDEL` = 35 interrupciones (0,7 s) antes de la primera repetición; `REPPER` = 5 interrupciones (0,1 s) entre repeticiones.
- `BREAK-KEY` (0x1F54) lee las semifilas 0x7FFE y 0xFEFE directamente: tras cada sentencia BASIC, en `BEEPER`, en `SA-BYTES`/`LD-BYTES` y en `COPY`.
- Con `DI`, en `BEEP` y en carga/grabación no hay lectura periódica.

## 5. ZX Spectrum 128K y +2 gris

### Reloj y vídeo

- Z80A a 3,5469 MHz (17,7345 MHz / 5).
- 228 T por línea × 311 líneas = **70 908 T por frame** (50,02 Hz).
- INT al principio del frame durante 36 T **(verificar)**; IM 1.
- Primer píxel en T 14 364 (línea 63); primer T contenido 14 361. Contención en la ranura 0x4000–0x7FFF y en la ranura 0xC000–0xFFFF cuando hay un banco impar paginado.
- El +2 gris es idéntico en temporización y E/S; cambia la ROM, el teclado, el datacassette integrado y añade dos conectores de joystick que se leen como teclas (Sinclair: 6–0 y 1–5).

### Puertos

| Puerto | Decodificación | Función |
|---|---|---|
| 0xFE | A0 = 0 | ULA, igual que el 48K |
| 0x7FFD | A15 = 0, A1 = 0 | Paginación: bits 0–2 banco RAM en 0xC000, bit 3 pantalla (banco 5/7), bit 4 ROM (0 editor 128, 1 BASIC 48), bit 5 bloqueo hasta reset. Solo escritura |
| 0xFFFD | A15 = 1, A14 = 1, A1 = 0 | AY-3-8912: escritura selecciona registro, lectura devuelve el registro seleccionado |
| 0xBFFD | A15 = 1, A14 = 0, A1 = 0 | AY-3-8912: escritura del dato |

- El AY funciona a 1,77345 MHz (CPU / 2). Su puerto de E/S A (registro 14) gestiona por software la interfaz serie RS232/MIDI y el teclado numérico (*keypad*) del 128K **(verificar asignación de bits)**.
- Los cambios de registros del AY se deben registrar con el T del `OUT` para generar el audio con precisión.

### Teclado según la ROM

- **ROM 0 (editor 128)**: su rutina de interrupción (0x0038) incrementa `FRAMES` y barre el teclado con la misma lógica de `KSTATE`/`REPDEL`/`REPPER` que el 48K, **en cada interrupción: cada 70 908 T = 19,99 ms**. En el 128K original también consulta el *keypad* a través del AY **(verificar periodicidad de la lectura del keypad)**.
- **ROM 1 (BASIC 48)**: es la ROM del 48K con mínimos cambios; lee el teclado en cada interrupción, también cada 70 908 T en esta máquina.
- `BREAK` sigue comprobándose tras cada sentencia y en los bucles de cinta y sonido.

## 6. ZX Spectrum +2A / +2B / +3 (gate array de Amstrad)

### Reloj y vídeo

- Z80A a 3,5469 MHz; 228 T × 311 líneas = **70 908 T** por frame.
- INT de 32 T **(verificar)**; IM 1.
- Contención con patrón 1,0,7,6,5,4,3,2, solo cuando MREQ está activo; la E/S no está contenida y los ciclos internos tampoco. No hay bus flotante en los puertos no decodificados (salvo un efecto parcial documentado para ciertos puertos **(verificar)**).

### Puertos

| Puerto | Decodificación | Función |
|---|---|---|
| 0xFE | A0 = 0 | Igual que el 128K (el bit 6 de lectura no depende de los bits escritos) |
| 0x7FFD | A15 = 0, A14 = 1, A1 = 0 | Paginación como el 128K; el bit 4 es el bit bajo de la selección de ROM (4 ROMs) |
| 0x1FFD | A15–A12 = 0001, A1 = 0 | Bit 0 modo especial (4 configuraciones de RAM sin ROM), bits 1–2 configuración/bit alto de ROM, bit 3 motor de disco, bit 4 strobe de impresora |
| 0xFFFD / 0xBFFD | Como en el 128K | AY-3-8912 |
| 0x0FFD | A15–A12 = 0000, A1 = 0 | Datos del puerto paralelo Centronics (escritura); lectura de *busy* **(verificar bit)** |
| 0x2FFD | A15–A12 = 0010, A1 = 0 | +3: registro de estado principal del controlador de disco µPD765A (lectura) |
| 0x3FFD | A15–A12 = 0011, A1 = 0 | +3: registro de datos del µPD765A (lectura/escritura) |

El +2A/+2B no tiene la disquetera, pero el gate array decodifica los mismos puertos. Las ranuras RS232/MIDI y keypad pasan por el AY como en el 128K **(verificar si se mantiene el keypad)**.

### Teclado según la ROM

- La ROM 0 (editor +3) y la ROM 3 (BASIC 48) leen el teclado en cada interrupción: **cada 70 908 T = 19,99 ms**, con el mismo esquema `KSTATE`/`REPDEL`/`REPPER`.
- Durante los accesos a disco, +3DOS (ROM 2) puede desactivar las interrupciones; entonces no hay barrido de teclado **(verificar)**.

## 7. Periféricos comunes

| Periférico | Máquinas | Puerto | Sincronización |
|---|---|---|---|
| ZX Printer / Alphacom 32 | ZX81, 16K/48K, 128K | 0xFB (A2 = 0). Lectura: bit 0 pulso del codificador, bit 6 = 0 si está conectada, bit 7 inicio de línea. Escritura: bit 1 lento, bit 2 motor parado, bit 7 aguja | La ROM sondea el codificador, sin interrupciones |
| Interface 2 (joysticks) | 16K/48K/128K | Ninguno: se leen como teclas 1–5 y 6–0 por 0xFE | Igual que el teclado |
| Kempston | Spectrum | 0x1F (A5 = 0 en la mayoría de clones). Bits 0–4: derecha, izquierda, abajo, arriba, disparo; 1 = activo | El juego lo sondea cuando quiere; la ROM no lo lee |
| Interface 1 | 16K/48K/128K | 0xE7 (datos microdrive), 0xEF (control/estado), 0xF7 (red/RS232) | ROM paginada en 0x0008/0x1708; temporización por software |

## 8. Consecuencias para el emulador

- **Teclado**: todas las ROMs lo leen como mucho una vez por frame (≈ 20 ms), pero los juegos leen el puerto 0xFE en cualquier T. La matriz de teclas debe consultarse en el momento del `IN`, no solo en la interrupción; actualizarla desde el anfitrión en la frontera de frame da como mucho un frame de latencia, coherente con la ROM.
- **Spectrum**: la INT debe mantenerse exactamente 32/36/32 T desde el principio del frame; si dura más o menos, programas que dependen de `HALT` o de ventanas de `EI` cambian de comportamiento.
- **ZX80/ZX81**: el teclado, el VSYNC y la imagen están ligados a la ejecución de la CPU; emularlos exige modelar el `NOP` forzado, la INT por A6, la NMI cada 207 T y los efectos secundarios de `IN 0xFE`/`OUT`, no un frame fijo.
- **Cinta y sonido**: EAR, MIC, el altavoz y los registros del AY se muestrean o registran con el T exacto del acceso; el bucle de carga de la ROM (59 T por muestra) es más fino que cualquier granularidad por líneas.
