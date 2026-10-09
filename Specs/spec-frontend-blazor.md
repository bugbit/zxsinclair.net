# Especificación: front end Blazor WebAssembly

Arquitectura del front end web del emulador: en qué hilo se ejecuta cada parte, cómo llegan las entradas (teclado, joystick) al emulador y cómo salen la pantalla y el sonido. Define también los contratos que el Core debe ofrecer para que el front end no dependa de su implementación interna. El rendimiento es el requisito principal (ver `README.md`): **ninguna parte de .NET del hilo principal participa en el camino por frame**, y el camino por frame no reserva memoria ni en C# ni en JavaScript.

Los detalles de generación de vídeo (ULA, borde, multicolor) y de sonido (beeper, AY) tendrán su propia especificación; aquí solo se fija su contrato de salida.

## 1. Fuentes

- Microsoft Learn: [ASP.NET Core Blazor con .NET en Web Workers](https://learn.microsoft.com/es-es/aspnet/core/blazor/blazor-with-dotnet-on-web-workers?view=aspnetcore-10.0) (.NET 10: script de worker propio con `dotnet.create()` y `getAssemblyExports`).
- Microsoft Learn: interoperabilidad `[JSImport]`/`[JSExport]` (`System.Runtime.InteropServices.JavaScript`), tipo `JSType.MemoryView`.
- MDN: Web Workers, `OffscreenCanvas`, `AudioWorklet`, `SharedArrayBuffer`, `Atomics`, `crossOriginIsolated` (COOP/COEP).
- `Docs/maquinas-sinclair-es.md`: frecuencia de frame y frecuencia de lectura del teclado de cada ROM.
- `Specs/spec-buses-memoria.md`: modelos, T-states por frame, puerto 0xFE.

Donde un dato no está confirmado se marca **(verificar)**.

## 2. Decisión: Web Worker con runtime .NET propio

En .NET 10 hay dos formas de sacar la emulación del hilo principal:

| | Hilos de .NET (`WasmEnableThreads`) | Web Worker con runtime propio |
|---|---|---|
| Estado en .NET 10 | Experimental, no soportado en Blazor **(verificar en cada versión)** | Soportado y documentado |
| COOP/COEP | Obligatorio: sin él la aplicación no arranca | Opcional: solo para `SharedArrayBuffer`; hay alternativa con `postMessage` |
| Interop JS desde el hilo del emulador | Con afinidad de hilo: canvas, WebGL y audio acaban en el hilo principal | El JS del worker tiene `OffscreenCanvas` y el puerto del `AudioWorklet` |
| Runtime de la UI | Todo multihilo (memoria compartida y atómicos) | El normal, sin cambios |
| Coste | — | Segundo runtime: descarga compartida y cacheada, memoria adicional, arranque una vez |

**Se elige el Web Worker.** El emulador es un único hilo que no necesita compartir objetos .NET con la UI; lo único que se comparte (matriz de teclado, muestras de audio) son unos pocos bytes que caben en un `SharedArrayBuffer` de JavaScript. Si los hilos de .NET pasan a ser estables, el cambio queda limitado a la capa de la sección 4, porque el Core no depende de ninguno de los dos modelos.

Del artículo de Microsoft se toma el arranque del runtime en el worker; **no** se toma el patrón petición/respuesta con JSON (`requestId`, `InvokeAsync`) para el camino por frame, ni la plantilla `blazorwebworker`/`WebWorkerClient` (.NET 11). Ese patrón solo se usa para las órdenes de control (sección 7).

## 3. Hilos y responsabilidades

```
┌───────────────────────────────┐  órdenes (postMessage)        ┌──────────────────────────────┐
│ Hilo principal                │ ────────────────────────────► │ Worker del emulador          │
│ - Blazor: UI, menús, ficheros │  entrada (SAB o postMessage)  │ - runtime .NET propio        │
│ - emulator-host.js: teclado,  │ ────────────────────────────► │ - emulator-worker.js: ritmo  │
│   AudioContext, canvas        │ ◄──────────────────────────── │ - SpectrumEmulator.RunFrame()│
│                               │  estado, errores, estadística │ - pinta en OffscreenCanvas   │
└───────────────────────────────┘                               └──────────────┬───────────────┘
                                                                               │ muestras: ring buffer SAB
                                 ┌──────────────────────────────┐              │ o MessagePort directo
                                 │ AudioWorklet (hilo de audio) │ ◄────────────┘
                                 │ reloj maestro                │
                                 └──────────────────────────────┘
```

| Hilo | Código | Hace | No hace |
|---|---|---|---|
| Principal | Blazor (.NET) y `emulator-host.js` | UI, carga de ROM/cintas/snapshots, captura de teclado, creación del `AudioContext`, transferencia del canvas | Nada por frame en .NET; el teclado lo trata JS puro |
| Worker | `emulator-worker.js` y el ensamblado de exports (.NET) | Ritmo de ejecución, `RunFrame()`, presentación del frame, envío de audio | Bucles infinitos en C# |
| Audio | `audio-worklet.js` | Consumir muestras a la frecuencia del dispositivo | Emular |

## 4. El worker del emulador

### 4.1 El bucle lo controla JavaScript

Un worker solo procesa mensajes (`onmessage`) cuando devuelve el control a su bucle de eventos. Por eso **C# nunca ejecuta un bucle infinito**: expone `RunFrame()` con `[JSExport]`, que emula exactamente un frame y vuelve, y el JS del worker decide cuántos frames ejecutar y cuándo ceder.

```js
function tick() {
  let frames = 0;
  while (needFrame() && frames < MaxBurst) { exports.RunFrame(); frames++; }
  if (frames > 0) exports.Present();          // solo el último frame de la ráfaga
  schedule(tick);                             // cede: entran mensajes de control y entrada
}
```

- `needFrame()` depende del reloj maestro (sección 4.2).
- `MaxBurst` (orientativo: 4) evita que el worker se quede sin ceder tras una pausa larga; si se supera, se descartan los frames atrasados en vez de acelerar.
- `schedule` usa `setTimeout` con el retardo hasta que haga falta el siguiente frame. `Atomics.wait` no se usa en el worker porque bloquearía la entrada de mensajes. **(verificar)** la granularidad real de `setTimeout` en workers de Chrome, Firefox y Safari; si no basta, autoenvío con `MessageChannel`.

### 4.2 Reloj maestro: el audio

El Spectrum va a 50,08 Hz (48K) o 50,02 Hz (128K/+2) y el monitor a 60, 120 o 144 Hz. Sincronizar con `requestAnimationFrame` produce cortes de audio o deriva. El reloj es **el consumo de audio**:

- `needFrame()` es cierto mientras las muestras pendientes en el ring buffer (sección 6) estén por debajo del objetivo. Objetivo inicial: 2 frames (~40 ms) de latencia; configurable.
- Sin audio (antes del primer gesto del usuario, o silenciado) el reloj es `performance.now()`: frames debidos = tiempo transcurrido × frecuencia de frame de la máquina, con acumulador fraccionario.
- **Modo turbo** (carga de cinta acelerada): `needFrame()` siempre cierto con un presupuesto de tiempo por `tick` (orientativo: 12 ms), audio silenciado y vídeo presentado una vez por `tick`.

### 4.3 Arranque

1. El hilo principal crea el worker (`type: 'module'`) y le envía `init` con el modelo, la ROM, la frecuencia de muestreo y los recursos transferibles: el `OffscreenCanvas` y, según el modo, los `SharedArrayBuffer` o el `MessagePort` del worklet.
2. El worker ejecuta `dotnet.create()`, registra sus funciones con `setModuleImports('zx', {...})` (las que llama C# por `[JSImport]`) y obtiene `getAssemblyExports('ZXSinclair.Net.Web.Emulator')`.
3. El worker llama a `Init(...)` exportado, que crea el `SpectrumEmulator`, y responde `ready`.

**(verificar)** si `dotnet.create()` en el worker carga todos los ensamblados de la aplicación Blazor; si es así, valorar un `_framework` propio del worker para no cargar la UI.

### 4.4 Cruces JS↔WASM por frame

| Dirección | Llamada | Tipo | Cuándo |
|---|---|---|---|
| JS → C# | `RunFrame()` | `[JSExport]`, sin parámetros | Cada frame |
| C# → JS | `readInput(Span<byte> state)` | `[JSImport]`, `JSType.MemoryView` | Al empezar cada frame |
| C# → JS | `pushAudio(Span<byte> samples)` | `[JSImport]`, `JSType.MemoryView` | Al terminar cada frame |
| JS → C# | `Present()` | `[JSExport]` | Último frame de cada ráfaga |
| C# → JS | `present(Span<byte> pixels)` | `[JSImport]`, `JSType.MemoryView` | Dentro de `Present()` |

- `JSType.MemoryView` sobre `Span<T>` no copia: JS recibe una vista de la memoria de WASM **válida solo durante la llamada**; JS copia lo que necesite con `copyTo`/`set` a buffers preasignados.
- `MemoryView` admite `Span<byte>`, `Span<int>` y `Span<double>`, no `Span<float>`: las muestras `float` se pasan como bytes con `MemoryMarshal.AsBytes` y JS las copia sobre una vista `Uint8Array` de un `Float32Array` **(verificar)** el soporte de tipos en .NET 10.
- No se cruza la frontera por instrucción ni por acceso a puerto: cuatro o cinco cruces por frame tienen un coste despreciable frente a los 69 888 T emulados.

## 5. Entrada

### 5.1 Captura (hilo principal, JS puro)

- `emulator-host.js` escucha `keydown`/`keyup` en el elemento del emulador (no globalmente, para no capturar teclas cuando el foco está en un formulario de la UI) y llama a `preventDefault()` en las teclas asignadas.
- Se mapea `KeyboardEvent.code` (posición física, independiente de la distribución del teclado) a una o varias teclas del Spectrum (semifila, bit). Las combinaciones (`Backspace` → CAPS SHIFT + 0, cursores → CAPS SHIFT + 5..8) comparten teclas, así que el estado se recalcula desde el conjunto de teclas del host pulsadas, con recuento por tecla del Spectrum.
- En `blur` y `visibilitychange` se sueltan todas las teclas.
- Joystick Kempston (cursores o `Gamepad API`) produce un byte de estado con el mismo mecanismo.

### 5.2 Estado, no eventos

Se envía **el estado completo**, no la secuencia de eventos: es idempotente y no hay colas que crezcan. Formato en el host, activo a nivel alto (1 = pulsada):

| Byte | Contenido |
|---|---|
| 0–7 | Semifilas 0–7 del teclado, bits 0–4 |
| 8 | Kempston: bits 0–4 derecha, izquierda, abajo, arriba, fuego |
| 9–15 | Reservado |

Para que una pulsación más corta que un frame no se pierda, el host mantiene además 16 bytes de "pulsadas desde la última lectura". El estado efectivo que lee el worker es `actual | pulsadasDesdeLectura`, y la lectura pone a cero el segundo bloque (`Atomics.exchange` sobre palabras de 32 bits con SAB; con `postMessage`, el worker hace el OR al recibir cada mensaje y lo limpia al leer).

### 5.3 Transporte

- **Con SAB**: un `SharedArrayBuffer` de 32 bytes (16 de estado y 16 de pulsadas). El host escribe con `Atomics.store`/`Atomics.or`; `readInput` del worker lo copia sobre el `Span<byte>` de C#.
- **Sin SAB**: el host envía `{type: 'input', state: Uint8Array(16)}` en cada cambio; el worker guarda el último estado y el OR de pulsadas.

### 5.4 Aplicación en el Core

La entrada se aplica **una vez al principio de cada frame**. Es suficiente porque la ROM del 48K/128K barre el teclado una vez por interrupción (`Docs/maquinas-sinclair-es.md`), y un juego que lea el puerto varias veces en el mismo frame ve un estado coherente. El Core recibe el estado con una llamada en bloque (sección 8) y lo convierte a activo a nivel bajo una vez; `ReadUlaPort` sigue leyendo su array de 8 bytes sin cambios.

## 6. Salida

### 6.1 Vídeo

- **Core**: un framebuffer de **1 byte por píxel con el índice de paleta** (0–15: bits 0–2 color, bit 3 brillo), que incluye el borde. Dimensiones provisionales 352 × 296 (256 × 192 de papel más 48 de borde a izquierda, derecha y arriba, y 56 abajo); las fija la especificación de vídeo. La paleta RGB la define el Core y el worker la recibe una vez en `Init`.
- **Presentación (recomendada)**: el hilo principal hace `canvas.transferControlToOffscreen()` y transfiere el `OffscreenCanvas` al worker. `present` sube el framebuffer como textura WebGL2 de un canal (`R8`) con `texSubImage2D` y un shader aplica la paleta; el escalado (entero o con suavizado) y los efectos opcionales (scanlines, CRT) también van en el shader. El hilo principal no toca el vídeo.
- **Alternativa sin WebGL2**: contexto 2D en el `OffscreenCanvas`; JS convierte índices a RGBA con una tabla `Uint32Array` de 16 entradas sobre un `ImageData` preasignado y llama a `putImageData`.
- **Alternativa sin `OffscreenCanvas`**: el worker copia el framebuffer a un `ArrayBuffer` y lo transfiere (`postMessage(buf, [buf])`) al hilo principal, que lo pinta en `requestAnimationFrame` y devuelve el buffer; dos buffers en circulación, sin reservas por frame. **(verificar)** si algún navegador objetivo la necesita.

### 6.2 Sonido

- **Core**: al terminar cada frame expone las muestras del frame, mono, `float` en [-1, 1], a la frecuencia de muestreo fijada en `Init` (la del `AudioContext`, normalmente 48 000 o 44 100 Hz). El número de muestras por frame no es entero (48K a 48 kHz: 48 000 × 69 888 / 3 500 000 = 958,46), así que el Core lleva un acumulador fraccionario y entrega 958 o 959. El filtrado (integración del nivel del beeper por intervalo de muestra) y el AY los fija la especificación de sonido.
- **Con SAB**: ring buffer de un solo productor (worker) y un solo consumidor (worklet): `Float32Array` de capacidad potencia de 2 (orientativo: 16 384 muestras) más una cabecera `Int32Array` con índice de escritura, índice de lectura y contador de faltas. `pushAudio` copia las muestras a un buffer temporal preasignado y de ahí al anillo en, como mucho, dos tramos.
- **Sin SAB**: el hilo principal crea un `MessageChannel` y transfiere un extremo al worklet (por `AudioWorkletNode.port`) y otro al worker. El worker envía cada frame un `Float32Array` transferido; el worklet lo encola y devuelve los buffers vacíos para reutilizarlos. El audio no pasa por el hilo principal.
- **Worklet**: `process()` consume 128 muestras por cuántum. Si faltan, repite la última muestra atenuándola hacia 0 (evita clics) e incrementa el contador de faltas, que el worker publica en las estadísticas.
- El `AudioContext` solo arranca tras un gesto del usuario: hasta entonces el reloj es `performance.now()` (sección 4.2) y `pushAudio` descarta las muestras.

## 7. Órdenes de control

Mensajes poco frecuentes por `postMessage`, con `id` para emparejar la respuesta. Los datos binarios (ROM, snapshot, cinta) se transfieren como `ArrayBuffer`. En el worker llegan a métodos `[JSExport]`; en esas llamadas sí se permiten copias y reservas.

| Mensaje | Datos | Respuesta |
|---|---|---|
| `init` | modelo, ROM(s), frecuencia de muestreo, canvas, SAB o puerto | `ready` o `error` |
| `reset` | — | `ok` |
| `pause` / `resume` | — | `ok` |
| `setSpeed` | `normal` o `turbo` | `ok` |
| `setModel` | modelo, ROM(s) | `ok` o `error` |
| `loadSnapshot`, `insertTape` | bytes (formatos en sus especificaciones) | `ok` o `error` |

El worker envía sin petición `stats` una vez por segundo: frames emulados por segundo, media y máximo de ms por `RunFrame()`, nivel del ring buffer y faltas de audio. La UI de Blazor los muestra; es también la herramienta de medida en el navegador (sección 10).

Blazor llama a `emulator-host.js` con `[JSImport]` para estas órdenes; las respuestas vuelven por `[JSExport]` o promesas. Ninguna de ellas está en el camino por frame.

## 8. Contrato del Core

El Core sigue sin referencias a JS ni a Blazor. Nuevo tipo `SpectrumEmulator` (`ZXSinclair.Net.Core/Machines/Spectrum/`), `sealed`, que agrupa máquina, `Z80Cpu<SpectrumBus>`, vídeo y sonido:

| Miembro | Contrato |
|---|---|
| `SpectrumEmulator(SpectrumModel model, int audioSampleRate)` | Reserva todos los buffers; nada se reserva después |
| `SetInput(ReadOnlySpan<byte> state)` | Estado de la sección 5.2 (16 bytes, activo a nivel alto); copia sin reservar |
| `RunFrame()` | Ejecuta hasta `TStatesPerFrame`, completa vídeo y audio del frame, llama a `EndFrame()`. Síncrono, sin `async`, sin reservas |
| `ReadOnlySpan<byte> FrameBuffer`, `FrameWidth`, `FrameHeight` | Índices de paleta del último frame completo |
| `ReadOnlySpan<uint> Palette` | 16 colores RGBA |
| `ReadOnlySpan<float> AudioSamples` | Muestras del último frame (sección 6.2) |
| `Reset()`, `LoadRom(...)` | Como en `SpectrumMachine` |

En `SpectrumMachine` se añade `SetKeyboard(ReadOnlySpan<byte> halfRows)` (8 bytes, activo a nivel alto), equivalente a ocho llamadas a `SetKey` sin bucle por bit.

Como el contrato es independiente del navegador, `SpectrumEmulator` se prueba con xUnit y se mide con BenchmarkDotNet como el resto del Core, y serviría igual para un front end de escritorio.

## 9. Proyectos y despliegue

| Proyecto | Tipo | Contenido |
|---|---|---|
| `ZXSinclair.Net.Web` | Blazor WebAssembly (independiente) | UI; `wwwroot/js/emulator-host.js`, `emulator-worker.js`, `audio-worklet.js` |
| `ZXSinclair.Net.Web.Emulator` | Biblioteca `net10.0`, `[SupportedOSPlatform("browser")]`, `AllowUnsafeBlocks` | `EmulatorExports` (`[JSExport]`) y `EmulatorImports` (`[JSImport]`); referencia al Core. Sin componentes Razor |

- Release con `RunAOTCompilation=true`, `InvariantGlobalization=true` y recorte (trimming). SIMD de WASM activo (valor por defecto).
- **COOP/COEP**: `Cross-Origin-Opener-Policy: same-origin` y `Cross-Origin-Embedder-Policy: require-corp` habilitan `SharedArrayBuffer`. El host comprueba `crossOriginIsolated` y elige el modo SAB o el modo `postMessage`; los dos están soportados y probados. Con COEP, todo recurso de otro origen (fuentes, CDN) debe servir CORP/CORS o alojarse en la aplicación. En GitHub Pages, donde no se pueden fijar cabeceras, se usa `coi-serviceworker`. **(verificar)** cómo fijar las cabeceras en el servidor de desarrollo de Blazor WebAssembly.
- La ROM no se incluye en el repositorio; su origen (fichero del usuario o distribución) se decide en otra especificación.

## 10. Rendimiento y criterios de aceptación

- Presupuesto: 20 ms por frame en tiempo real. Objetivo: `RunFrame()` con AOT **≤ 3 ms** de media (≥ 6 veces tiempo real) en un portátil de gama media, para dejar margen a turbo y a navegadores lentos. Los benchmarks del Core con JIT x64 no sirven de referencia: WASM es varias veces más lento **(verificar)** la relación real con AOT e intérprete.
- El camino por frame no reserva memoria en C# (`GC.GetAllocatedBytesForCurrentThread` sin cambios tras 1 000 frames, comprobable en xUnit) ni en JavaScript (sin `new` ni `slice` en `tick`, `readInput`, `present` y `pushAudio`).
- 10 minutos ejecutando sin faltas de audio en modo normal, con el objetivo de latencia de 2 frames.
- Latencia de entrada: una pulsación aparece en el siguiente frame emulado.
- Las estadísticas de la sección 7 son la medida en el navegador; se registran en esta especificación como en `spec-cpu-z80.md` 8.4.

## 11. Pruebas

- **xUnit (Core)**: `SetInput`/`SetKeyboard` (activo alto a bajo, semifilas combinadas, pulsación corta); `RunFrame` deja `TStates` en el rango del frame siguiente; número de muestras acumulado en N frames = N × frecuencia × T por frame / reloj, con error < 1 muestra; dimensiones del framebuffer; sin reservas tras el calentamiento.
- **Navegador (Playwright)**, cuando exista la UI: arranque en modo SAB y en modo `postMessage`, entrada de teclado visible en pantalla y estadísticas sin faltas de audio.

## 12. Fases

| # | Fase | Depende de | Resultado |
|---|---|---|---|
| 0 | Prototipo: worker con runtime, `RunFrame()` sobre una ROM del usuario, estadísticas | — | Medida de ms/frame con intérprete y con AOT |
| 1 | Entrada y vídeo | Especificación de vídeo | Pantalla y teclado con SAB y `postMessage` |
| 2 | Sonido y reloj de audio | Especificación de sonido | Beeper con el audio como reloj maestro |
| 3 | Órdenes de control | Especificaciones de snapshot y cinta | Carga de SNA/Z80/TAP, turbo |

## 13. Fuera de alcance

- Generación de vídeo y sonido (especificaciones propias), AY, formatos de snapshot y cinta.
- Depurador, teclado virtual táctil, PWA y guardado de estado.
- ZX81 (vídeo generado por la CPU).

## 14. Pendiente de verificar

- Estado de `WasmEnableThreads` en cada versión de .NET.
- Si `dotnet.create()` en el worker carga los ensamblados de la UI.
- Tipos admitidos por `JSType.MemoryView` en .NET 10 (`Span<float>`).
- Granularidad de `setTimeout` en workers.
- Soporte de `OffscreenCanvas` con WebGL2 en los navegadores objetivo (especialmente Safari).
- Cabeceras COOP/COEP en el servidor de desarrollo.
- Factor de rendimiento AOT/intérprete frente a JIT.
