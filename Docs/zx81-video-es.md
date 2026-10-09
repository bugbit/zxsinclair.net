# ZX81: vídeo generado por la CPU, alta resolución y color Chroma 81

Resumen de referencia, a partir de fuentes externas, de cómo el ZX81 genera la imagen con el propio Z80, de las técnicas de alta resolución que se apoyan en ese mecanismo y del modo color de la interfaz Chroma 81. Complementa a `Docs/maquinas-sinclair-es.md` (puertos, teclado y sincronización de cada máquina) y a `Specs/spec-buses-memoria.md` sección 4.5 (mapa de memoria del ZX81). No es una especificación del Core: cuando se especifique el vídeo del ZX81, la especificación irá en `Specs/` y mandará sobre este documento.

Las cifras marcadas **(verificar)** no se han contrastado con una segunda fuente ni con hardware.

Fuentes principales:

- *Nocash ZX specs* (Martin Korth): [problemkaputt.de/zxdocs.htm](https://problemkaputt.de/zxdocs.htm), secciones de vídeo del ZX80/ZX81, pseudo y true hi-res, ampliaciones de caracteres.
- Wilf Rigter, *ZX-81 Video Display System* (ZXir QLive Alive!, 1996) y artículos WRX16 (ZX-Appeal, 1988), índice en [timexsinclair.com](https://timexsinclair.com/indiv/wilf-rigter/index.html).
- Paul Farrow, *Chroma 81 Technical Description* (copia en [k1.spdns.de](https://k1.spdns.de/Develop/Projects/zxsp/Info/Chroma81_TechnicalDescription.txt)) y el hilo [Chroma 81 en Sinclair ZX World](https://www.sinclairzxworld.com/viewtopic.php?p=15597).
- Logan y O'Hara, *The Complete Timex TS1000/Sinclair ZX81 ROM Disassembly*.
- Emuladores de referencia: EightyOne (opción WRX), zxsp (`Chroma81.cpp`).

## 1. Idea general

El ZX81 no tiene controlador de vídeo con acceso propio a memoria. La ULA solo sabe:

1. Contar líneas (HSYNC cada 207 T) y un contador de línea dentro del carácter (`LCNTR`, 0–7).
2. Generar o no NMI en cada línea (modo SLOW).
3. Vigilar los ciclos M1 que la CPU hace por encima de 32K y, en el ciclo de refresco siguiente, leer un byte de patrón y sacarlo por el registro de desplazamiento.

Es la CPU la que recorre el fichero de pantalla (D-FILE) **ejecutándolo**: la ROM salta a `D_FILE + 0x8000` y cada byte del D-FILE se busca como si fuera un opcode. La ULA le roba ese byte y le entrega un `NOP` a la CPU, de modo que cada carácter cuesta exactamente un M1 de 4 T y produce 8 píxeles.

## 2. Temporización básica

| Concepto | Valor | Nota |
|---|---|---|
| Reloj | 6,5 MHz de cristal; Z80A a 3,25 MHz | 2 píxeles por T |
| Línea | 207 T = 64 µs (≈ 15,7 kHz) | 128 T de imagen + 64 T de borde/blanking + 15 T de retrazado |
| Carácter | 4 T (un M1) = 8 píxeles | El patrón se desplaza a 6,5 MHz |
| Área de texto | 32 × 24 caracteres = 256 × 192 píxeles | |
| Márgenes a 50 Hz | ≈ 56 líneas arriba, 192 de imagen, ≈ 56 abajo, ≈ 6 de VSYNC | `MARGIN` (0x4028) = 55 |
| Márgenes a 60 Hz | ≈ 32 líneas arriba, 192, ≈ 32 abajo, ≈ 6 de VSYNC | `MARGIN` = 31 |
| VSYNC | ≈ 1 235 T (≈ 6 líneas) **(verificar)** | Lo genera la ROM con `IN`/`OUT` |
| Frame | ≈ 310 líneas × 207 T ≈ 64 000–65 000 T **(verificar)** | Lo define el software, no la ULA |

El frame no es un número fijo de la ULA: sale de cuántas líneas cuenta la ROM (o un programa) entre dos VSYNC. Un emulador que suponga un frame constante romperá programas que generan su propia imagen.

## 3. Mecanismo del vídeo de texto

### 3.1 NOP forzado

En un ciclo M1 con **A15 = 1**:

- Si el bit 6 del byte leído es **0**, la ULA captura el byte (código de carácter) y fuerza el bus de datos de la CPU a `0x00`. La CPU ejecuta `NOP` y el PC avanza.
- Si el bit 6 es **1**, la ULA no lo captura y muestra blanco (paper); la CPU ejecuta el opcode real. En la práctica es el `HALT` (0x76, el `NEWLINE` del D-FILE) que cierra cada línea de texto.

Con A15 = 0 nunca hay captura: el código de la ROM y de la RAM baja se ejecuta normalmente y la salida de vídeo queda en blanco.

El ZX81 tiene resistencias entre la CPU y el bus de datos de la memoria, por eso la ULA puede imponer `0x00` a la CPU mientras la memoria sigue entregando el byte real a la ULA.

### 3.2 Dirección del patrón

Durante T3–T4 del M1 el Z80 pone en el bus la dirección de refresco `I:R` (`I` en A8–A15, `R` en A0–A7). Tras un M1 capturado, la ULA **sustituye A0–A8** por:

| Líneas | Origen |
|---|---|
| A0–A2 | `LCNTR` (línea dentro del carácter, 0–7) |
| A3–A8 | Bits 0–5 del código de carácter |
| A9–A15 | Bits 1–7 de `I` (el bit 0 de `I` queda tapado por A8) |

```
patrón = (I & 0xFE) << 8 | (código & 0x3F) << 3 | LCNTR
```

Con `I = 0x1E` (valor de la ROM) el juego de caracteres está en 0x1E00–0x1FFF. El byte leído se carga en el registro de desplazamiento; si el **bit 7 del código** está a 1, se invierte (vídeo inverso). Los bits 0–5 dan 64 caracteres; con el bit 7, 128 apariencias.

La sustitución de A0–A8 se hace a través de resistencias y **solo llega a la ROM**: la RAM interna y el conector de expansión ven las líneas de la CPU, es decir, el `I:R` original. Por eso poner `I` en 0x40–0x7F para tener un juego de caracteres en RAM **no funciona** en modo texto, y por eso existe la alta resolución WRX (sección 5.2).

### 3.3 Fin de línea: INT por A6

La entrada /INT del Z80 está conectada a **A6**. Durante el refresco A6 es el bit 6 de `R`, así que cuando ese bit vale 0 y las interrupciones están habilitadas, la CPU acepta una INT al final de la instrucción (IM 1, salto a 0x0038).

La ROM carga `R` justo antes de saltar a la línea de forma que el bit 6 pase a 0 tras los 32 caracteres y el `HALT`: la línea de texto termina siempre en el mismo T aunque el D-FILE esté comprimido (líneas cortas en el modelo de 1K; el `HALT` espera hasta la INT). Valor concreto de `R` y desfase exacto: ver la ROM en 0x0038–0x0048 y la rutina `DISPLAY` **(verificar con el desensamblado)**.

La rutina de INT decrementa el contador de líneas de barrido, vuelve a ejecutar la misma fila del D-FILE 8 veces (`LCNTR` 0–7) y pasa a la siguiente.

El reconocimiento de la INT **sincroniza también el HSYNC de la ULA** (y, en el ZX81, reinicia el contador de la NMI), de modo que el inicio de cada línea queda alineado con la instrucción que la generó (Nocash: la interrupción "fuerza HSYNC").

### 3.4 Márgenes: NMI y WAIT

En modo **SLOW** el generador de NMI de la ULA emite una NMI en cada HSYNC (cada 207 T). La rutina NMI (0x0066) cuenta las líneas de margen en `A'` (con `EX AF,AF'`) y devuelve el control al programa de usuario, que avanza a trozos durante los márgenes superior e inferior (≈ 25 % del tiempo de CPU). Cuando el contador llega a cero, la ROM entra en la rutina de pantalla y espera con `HALT` (0x0079).

Para que la primera línea de imagen empiece en un T exacto, mientras hay petición de NMI la ULA **activa /WAIT** y la CPU queda en espera hasta que la NMI se reconoce; el `HALT` sí se completa sin esperas. Así la NMI llega siempre con la misma fase respecto al HSYNC aunque la instrucción interrumpida tenga duración variable. (El *WAITMOD* de Wilf Rigter limita esa espera de 14 T al `HALT` de 0x0079.)

En modo **FAST** la NMI está desactivada: el programa corre a plena velocidad y no hay imagen salvo cuando la ROM la genera esperando una tecla.

### 3.5 Puertos relacionados con el vídeo

| Acceso | Efecto en el vídeo |
|---|---|
| `IN` 0xFE (A0 = 0) | Con la NMI desactivada: **inicia VSYNC** y pone `LCNTR` a 0 (y la salida de cinta a nivel bajo). Devuelve teclado, enlace 50/60 Hz (bit 6) y cinta (bit 7) |
| `OUT` a cualquier puerto | **Termina VSYNC** y reanuda `LCNTR` |
| `OUT` 0xFE (A0 = 0) | Activa el generador de NMI (SLOW) |
| `OUT` 0xFD (A1 = 0) | Desactiva el generador de NMI (FAST) |

Detalles de teclado, cinta e impresora en `Docs/maquinas-sinclair-es.md` sección 3.

### 3.6 ZX80

Mismo mecanismo de NOP forzado, patrón (`I = 0x0E`, caracteres en 0x0E00) e INT por A6, pero **sin generador de NMI**: el programa no avanza mientras hay imagen y la pantalla desaparece al ejecutar.

## 4. Diagrama de una línea de texto

```
T:   0          inicio         +128 T (32 × M1 de 4 T)      +HALT ... INT     207
     | borde izq | c0 c1 c2 ... c31 (NOP forzados)          | HALT → INT 0x0038 | HSYNC
                    │
                    └─ por cada M1 capturado:
                       T1–T2: lee D-FILE[n] (A15=1) → ULA guarda código, CPU recibe 0x00
                       T3–T4: refresco I:R → ULA cambia A0–A8 → ROM entrega patrón
                              → registro de desplazamiento (invertido si bit 7)
```

La posición del primer píxel dentro de la línea (desfase entre el M1 y el píxel en pantalla, y entre la INT y el HSYNC) depende de la ROM y del retardo de la ULA **(verificar con EightyOne o hardware antes de fijarlo en una especificación)**.

## 5. Modos de alta resolución

Todos aprovechan que la ULA lee un byte en el refresco de **cualquier** M1 capturado, no solo los que lanza la ROM.

### 5.1 Pseudo hi-res (patrones de la ROM)

- La rutina de pantalla la escribe el programa. Cada línea de barrido ejecuta una tira de "códigos" terminada en `RET` (0xC9) y fuerza `LCNTR = 0` con un VSYNC muy corto en cada línea, de modo que el patrón usado es siempre la línea 0 del "carácter".
- Cambiando `I` en cada línea (0x00, 0x08, 0x0C...) y eligiendo el código, cada celda de 8×1 toma un byte cualquiera de la ROM como patrón. No es un bitmap libre: solo hay los patrones que existan en la ROM.
- Resolución efectiva 256 × 192 con celdas de 8 × 1; ≈ 6 176 bytes de "pantalla". Ejemplos: *Rock Crush*, *Madjump II* (`I = 0x00`), *Bipods*. El TS1500 (ROM parcheada) puede verse distinto.
- Funciona en un ZX81 sin modificar.

### 5.2 True hi-res (WRX, Wilf Rigter)

- `I` apunta a RAM (0x40–0x7F). Como la RAM ve las líneas originales de la CPU (sección 3.2), el byte del patrón se lee de la dirección **`I:R` sin sustituir**: un bitmap real. No hacen falta `HALT` ni `RET` al final de línea.
- Solo se incrementan los 7 bits bajos de `R` (el bit 7 se conserva), así que cada fila del bitmap debe estar alineada a 32 bytes dentro de un bloque de 128.
- Los RAM packs externos normalmente no responden durante el refresco (ignoran o bloquean `/RFSH`, `/MREQ`): WRX necesita una modificación (dos diodos y un pull-up de 4K7) o RAM que lo admita. Con el 1K interno cabe solo una imagen pequeña (*WRX1K*).
- Resolución 256 × 192 bitmap (WRX16: 16 veces la del modo texto). Ejemplos: *Starfight*, demos de Guus Flater, *Highres Chess* (192 líneas hi-res + texto), *MAXDEMO* (320 × 256).
- En EightyOne es una opción (WRX) porque depende del hardware de RAM.

### 5.3 Juegos de caracteres en RAM/ROM extra (UDG)

| Ampliación | Funcionamiento |
|---|---|
| Quicksilva CHRS | 1K RAM en 0x8400–0x87FF, 128 caracteres (64 + 64 invertidos); el acceso de la ULA a 0x1E00 se redirige a esa RAM |
| dk'tronics / Kayde 4K | ROM en 0x2000–0x2FFF con 7 juegos de 64 símbolos, elegidos con `I = 0x20 + 2N` |
| CHR$128 (p. ej. en Chroma 81, ZXpand) | RAM en 0x2000–0x3FFF; el bit 7 del código elige entre 128 caracteres en vez de invertir **(verificar)** |
| QS Hi-Res, Memotech HRG | Placas con ROM/EPROM propia (0x2800 / 0x2000) que generan bitmaps; fuera del alcance inicial |

## 6. Chroma 81 (Paul Farrow)

Interfaz moderna (CPLD) que añade salida SCART/RGB, 16K de RAM, RS232, emulación de cartucho/QS CHRS y **color**. El color y la RAM en 0xC000–0xFFFF dependen del **interruptor de configuración 6**.

### 6.1 Puerto de control 0x7FEF

Totalmente decodificado (`0111 1111 1110 1111`).

**Escritura**:

| Bits | Función |
|---|---|
| 0–2 | Color del borde en formato GRB (bit 2 = G, bit 1 = R, bit 0 = B) |
| 3 | Brillo del borde (1 = 100 %, 0 = 69 %) |
| 4 | Modo: 0 = mapeo por código de carácter (modo 0), 1 = fichero de atributos (modo 1) |
| 5 | 1 = color activado |
| 6–7 | Reservados, escribir 0 |

**Lectura**: bit 5 = 0 si los modos de color están disponibles (interruptor 6 en ON). Sin Chroma o con el interruptor apagado se lee 0xFF.

Con el interruptor 6 apagado el color queda forzado a desactivado, modo 0 y borde blanco brillante (bits 3–0 = 1111).

### 6.2 Formato de color

Un byte por celda o línea, igual en ambos modos:

```
bit 7   6 5 4   3   2 1 0
    Bp  G R B   Bi  G R B
    └─ papel ─┘ └─ tinta ─┘
```

- Tinta en bits 0–3 (GRB + brillo), papel en bits 4–7.
- 15 colores distintos (negro brillante = negro). Nivel brillante 100 %, normal 69 %.
- El orden GRB (bit 2 = G) coincide con el del Spectrum (`G R B` en bits 2-1-0).

### 6.3 Modo 0: color por código de carácter

- Tabla en **0xC000–0xC3FF** (1 024 bytes): 8 bytes por carácter, uno por línea de píxeles. Primero los 64 caracteres normales, después los 64 invertidos.
- Para un carácter con código `c` en la línea `LCNTR`:

  ```
  índice = (c & 0x3F) | ((c & 0x80) >> 1)    ; 0–127
  color  = mem[0xC000 + índice * 8 + LCNTR]
  ```

- Color distinto por línea de cada carácter. La tabla se consulta en cada frame, así que puede cambiarse en caliente; sobrevive a `NEW`/`RAND USR 0` y a `LOAD`, por lo que **colorea programas existentes** sin modificarlos.

### 6.4 Modo 1: fichero de atributos

- El atributo de cada posición del D-FILE está en **`0xC000 | dirección`**: con el D-FILE en 0x4000–0x7FFF, el fichero de atributos está 0x8000 por encima, con la misma forma (incluidos los huecos de los `NEWLINE`, que quedan libres).
- Como la CPU ejecuta el D-FILE en `D_FILE + 0x8000`, la dirección del M1 capturado con los bits 14 y 15 a 1 es directamente la del atributo:

  ```
  atributo = mem[M1_address | 0xC000]
  ```

- Un atributo por celda de 8 × 8 (una tinta y un papel). Solo lo usan programas escritos para él.
- Requiere el D-FILE en 0x4000–0x7FFF; con el D-FILE en 0x2000–0x3FFF el modo 1 no funciona.

### 6.5 Otras notas

- El color funciona en SLOW y en FAST.
- El borde y las zonas en blanco (sin carácter capturado) usan el color del borde **(verificar si las zonas tras el `HALT` usan borde o papel)**.
- WRX en Chroma: interruptor 2; los refrescos leen de la RAM interna de la interfaz (incluida 0xC000–0xFFFF con el color activo). El modo CHR$128 exige WRX desactivado. Cómo se combina el color con un bitmap WRX (qué índice o atributo usa) no está documentado **(verificar con zxsp/EightyOne)**.
- El documento técnico no dice en qué ciclo lee la Chroma el color respecto al M1 y al refresco; para el emulador basta con calcularlo en el mismo punto en que se calcula el patrón.

## 7. Consecuencias para el emulador

- **El bus debe conocer el refresco**: hoy `IZ80Bus.FetchOpcode(address)` no recibe `I` ni `R`. Un `Zx81Bus` necesita, en cada M1, la dirección del opcode, el valor de `I:R` del refresco (valor de `R` en el ciclo, antes del incremento **(verificar con la convención del Core)**) y poder devolver `0x00` a la CPU. Ampliar el contrato (p. ej. un `FetchOpcode(address, refresh)` o un genérico de máquina) tiene coste en el camino caliente y debe medirse con los benchmarks de CPU.
- **Regla por M1** (solo con A15 = 1 y bit 6 del byte = 0):

  ```
  código = mem[address]
  si I < 0x40:  patrón = mem[(I & 0xFE) << 8 | (código & 0x3F) << 3 | LCNTR]   ; texto / pseudo hi-res / UDG
  si no:        patrón = mem[I << 8 | R]                                     ; WRX, si la RAM lo admite
  si código & 0x80: patrón = ~patrón
  CPU recibe 0x00
  ```

  Con Chroma 81, añadir el color (modo 0 o 1) en el mismo punto. Todo es aritmética de enteros y lecturas de tabla: sin ramas extra para el caso sin Chroma si se elige la variante de máquina por genérico.
- **Líneas físicas, no frames fijos**: HSYNC cada 207 T (realineado en el reconocimiento de INT), `LCNTR` controlado por `IN 0xFE`/`OUT`, NMI con /WAIT. La salida de vídeo debe escribirse por T (2 píxeles por T) en un buffer de líneas y el frame cerrarse en el VSYNC que genere el software, con un límite de seguridad si no llega.
- **INT por A6**: `IntActive` depende del bit 6 de `R` en el refresco del último M1, no de un temporizador.
- **NMI y WAIT**: el Core necesita una entrada NMI y modelar la espera de /WAIT hasta el reconocimiento; afecta al recuento de T de la instrucción interrumpida.
- **Paleta**: en monocromo basta con índices 0/1; con Chroma, 16 índices (GRB + brillo), compatibles con la presentación por índices de paleta de `Specs/spec-frontend-blazor.md`.
