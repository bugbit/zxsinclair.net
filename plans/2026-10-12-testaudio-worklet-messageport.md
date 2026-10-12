# Plan: botón TestAudio (do de 20 ms por frame desde el worker al AudioWorklet)

Fecha: 2026-10-12
Estado: implementado y verificado automáticamente; escucha manual pendiente

## Objetivo

Añadir al prototipo `ZXSinclair.Net.Web.Prototype` un botón TestAudio (start/stop) que, en el Web Worker .NET,
genere cada 20 ms (un "RunFrame") un frame de un do (C4, 261,626 Hz) y lo reproduzca de forma continua en el
navegador mediante un AudioWorklet, con el consumo de audio como reloj que dispara cada frame, sin reservas de
buffers por frame.

## Contexto

- El prototipo ya valida el vídeo: TestPresent hace C# en el worker → `present(Span<byte>)` con
  `JSType.MemoryView` → `copyTo` a un buffer preasignado → OffscreenCanvas (`Specs/prototipo-web-worker-render.md`).
- Ficheros implicados: `ZXSinclair.Net.Web.Prototype.Workers/WorkTest.cs` (`[JSExport]`/`[JSImport]`),
  `ZXSinclair.Net.Web.Prototype.Client/Workers/WorkTest.razor.js` (script del worker, `setModuleImports('emulator', ...)`,
  `switch` de comandos), `Clients/ClientWorkTest.razor.js` (RPC `request(command, payload, transfer)`),
  `Components/Emulator.razor(.js)` (botones con `data-testid`, `reportTiming`, `presentReady` cacheada),
  `ZXSinclair.Net.Web.Prototype.Tests` (Playwright, colección `"Prototype browser"`, `Category=Browser`).
- Diseño final del audio (`Specs/spec-frontend-blazor.md` §4.2, §6.2; `Specs/spec-sonido.md` §7): el consumo de audio
  es el reloj maestro; un cruce `pushAudio(Span<byte>)` por frame (floats como bytes porque `MemoryView` no admite
  `Span<float>`); transporte SAB (ring buffer) o, sin SAB, `MessageChannel` con `Float32Array` transferidos que el
  worklet encola y devuelve. El worklet solo copia 128 muestras por cuántum; en falta atenúa la última muestra.
- El host no envía COOP/COEP.
- **¿Hace falta una cola?** Sí, pero no de `AudioBufferSourceNode` en el hilo principal: una **FIFO fija dentro del
  AudioWorklet** con un **pool cerrado de buffers** que circulan worker → worklet → worker.
- Decisiones del usuario: transporte **MessagePort** (sin COOP/COEP) y botón **alternar start/stop** con bucle
  continuo y estadísticas.

Diseño:

```
Hilo principal (gesto)            Worker (.NET + JS)                          AudioWorklet (tiempo real)
AudioContext + addModule          InitAudio(port2, sampleRate) ─► C# InitAudio
MessageChannel ─port1─► node.port ─────────────────────────────────────────►  FIFO (capacidad = pool)
               ─port2─► worker    StartAudio: prellenar N frames ──samples──►  process(): copia 128/cuántum
                                  ◄──────────── buffer devuelto + stats ─────  buffer agotado → postMessage back
                                  al recibir: RunAudioFrame() → pushAudio ──►  …
```

- La devolución de un buffer es el tick del reloj: dispara el siguiente frame en el worker. Sin `setTimeout`, sin
  crecimiento de cola.
- El worklet arranca a consumir cuando la FIFO tiene el objetivo de frames (evita la falta inicial, spec-sonido 7.4).

## Pasos

1. **C# — `ZXSinclair.Net.Web.Prototype.Workers/WorkTest.cs`** (mismo estilo que `TestPresent`):
   - Campos estáticos: `sAudio = new float[4096]` (cubre 96 kHz × 20 ms), `sSampleRate`, `sPhase` (double),
     `sPhaseStep`, acumulador fraccionario de muestras por frame (`sampleRate × 20 / 1000`; 882/960/1920 exactos en
     las frecuencias habituales, pero genérico).
   - `[JSImport("pushAudio","emulator")] static partial void PushAudio([JSMarshalAs<JSType.MemoryView>] Span<byte> samples);`
   - `[JSExport] int InitAudio(int sampleRate)`: valida rango, fija paso de fase `2π·261.6256/sampleRate`, fase 0;
     devuelve muestras nominales por frame.
   - `[JSExport] int RunAudioFrame()`: calcula `n` con el acumulador, rellena `sAudio[0..n)` con
     `0.25f * MathF.Sin(phase)` manteniendo **fase continua entre frames** (sin clics en las uniones), envuelve la
     fase, `PushAudio(MemoryMarshal.AsBytes(sAudio.AsSpan(0, n)))`, devuelve `n`. Sin reservas.
   - `[JSExport] double MeasureLastAudioFrame()` (solo verificación, fuera del bucle): estima la frecuencia por
     cruces por cero ascendentes del último frame en `sAudio` (y/o pico), para que el test compruebe que es un do.

2. **Worklet nuevo — `ZXSinclair.Net.Web.Prototype.Client/Worklets/AudioTestWorklet.razor` (vacío, solo comentario)
   + `AudioTestWorklet.razor.js`**, con el mismo truco que `Workers/WorkTest.razor` para publicarlo como recurso estático:
   - `class ZxAudioTestProcessor extends AudioWorkletProcessor`, `registerProcessor('zx-audio-test', ...)`.
   - `this.port.onmessage` recibe `{ port }` (extremo del worker). Por ese puerto llegan
     `{ type: 'samples', buffer: Float32Array, length }`, `{ type: 'start', target }` y `{ type: 'stop' }`.
   - FIFO como array circular preasignado (capacidad 8) de `{ buffer, length }` e índice de lectura dentro del buffer
     de cabeza. `process(_, outputs)`: copia a `outputs[0][0]` (128 muestras) desde uno o dos buffers; al agotar uno
     lo devuelve al worker transferido junto con `{ consumed, underruns, queued }`.
   - Estado `primed`: no consume hasta tener `target` frames. Tras `stop`, al vaciarse vuelve a reposo sin contar
     faltas. En falta repite la última muestra atenuada y cuenta la falta. `process` devuelve siempre `true`.

3. **JS del worker — `ZXSinclair.Net.Web.Prototype.Client/Workers/WorkTest.razor.js`**:
   - Añadir `pushAudio` a `setModuleImports('emulator', { createResult, present, pushAudio })`.
   - Estado de audio: `audioPort`, pool de `POOL = target + 1` entradas `Float32Array(4096)` creadas en `InitAudio`,
     lista de libres, `running`, contadores (`frames`; `dropped` si no hay buffer libre — nunca se reserva uno nuevo —;
     suma/máx de ms por `RunAudioFrame()` con `performance.now()`; últimas stats del worklet).
   - `pushAudio(view)`: toma un buffer libre, `view.copyTo(new Uint8Array(buf.buffer, 0, n * 4))` (o una vista
     `Uint8Array` cacheada por entrada al volver el `ArrayBuffer`), y
     `audioPort.postMessage({ type: 'samples', buffer, length: n }, [buffer.buffer])`.
   - Comandos nuevos en el `switch`: `InitAudio { port, sampleRate, target }` (transfiere `[port]`; llama a C#
     `InitAudio`; arma `port.onmessage` para buffers devueltos), `StartAudio` (envía `start`, prellena `target`
     frames), `StopAudio` (deja de producir, envía `stop`), `AudioStats` (contadores + `MeasureLastAudioFrame()`).
   - Al recibir un buffer devuelto: vuelve a libres y, si `running`, ejecuta un `RunAudioFrame()` (punto donde en el
     diseño final irá `RunFrame()` + `Present()`).

4. **Cliente — `Clients/ClientWorkTest.razor.js`**: `InitAudio(port, sampleRate, target)` con `[port]` en
   transferibles, `StartAudio()`, `StopAudio()`, `AudioStats()`.

5. **Hilo principal — `Components/Emulator.razor.js`**: `export async function TestAudio()`:
   - Primera vez: cachear una promesa `audioReady` (como `presentReady`): `new AudioContext({ latencyHint: 'interactive' })`
     (sin forzar `sampleRate`), `ctx.resume()` lo antes posible dentro del gesto,
     `audioWorklet.addModule(new URL('../Worklets/AudioTestWorklet.razor.js', document.baseURI))`,
     `new AudioWorkletNode(ctx, 'zx-audio-test', { numberOfInputs: 0, outputChannelCount: [1] })`,
     `connect(ctx.destination)`, `MessageChannel`: `port1` → `node.port.postMessage({ port }, [port])`,
     `port2` → `WorkTest.InitAudio(port2, ctx.sampleRate, target)`. `target` desde `?audioFrames=` (por defecto 3 ≈ 60 ms; 2–6).
   - Alterna `StartAudio`/`StopAudio`; devuelve `{ running, sampleRate, samplesPerFrame }`; guarda
     `data-audio-state` y `data-sample-rate` en un elemento para el test.
   - Exportar `audioStats()` para Playwright; `dispose()` para el audio, cierra el `AudioContext` y olvida la promesa.

6. **Componente — `Components/Emulator.razor`**: botón `data-testid="test-audio"` con `TestAudioAsync`, texto
   TestAudio/StopAudio según el estado, `reportTiming("TestAudio", ms, { running })` como los demás (mide el
   arranque/parada, no el bucle).

7. **Test Playwright — nuevo `ZXSinclair.Net.Web.Prototype.Tests/EmulatorAudioTests.cs`** (colección
   `"Prototype browser"`, `Trait Category=Browser`, misma comprobación de errores de consola/HTTP que `WithPageAsync`):
   - Clic en `test-audio`, esperar `data-audio-state=running`, esperar ~3 s (`ZX_AUDIO_SECONDS`), leer `audioStats()`.
   - Aserciones de corrección (sin umbrales de rendimiento): frames producidos > 0 y coherentes con las muestras
     consumidas; muestras consumidas ≈ segundos × `sampleRate` (±25 %); `dropped == 0`; frecuencia estimada
     261,6 Hz ±2 %.
   - Informar `sampleRate`, `target`, faltas tras el arranque, media/máx ms por `RunAudioFrame()`,
     `outputLatency`/`baseLatency`; JSON en `TestResults/audio-loop.json`.
   - Segundo clic: `data-audio-state=stopped`; tras 200 ms los frames dejan de crecer.
   - Si Chromium headless no arranca el `AudioContext` por la política de autoplay, añadir
     `--autoplay-policy=no-user-gesture-required` a los argumentos de lanzamiento de `PrototypeServerFixture`.

8. **Documentación**: nueva sección "Audio" en `Specs/prototipo-web-worker-render.md` (filas del protocolo
   `InitAudio`/`StartAudio`/`StopAudio`/`AudioStats`, interop `pushAudio`, pool/FIFO/reloj por devolución,
   resultados medidos); actualizar §7 (floats como bytes con `MemoryView` verificado, AudioWorklet + MessagePort
   verificado) y §8 Pendiente; en `CLAUDE.md` (sección Browser y descripción de la spec del prototipo) mencionar
   TestAudio y las variables nuevas (`audioFrames`, `ZX_AUDIO_SECONDS`).

9. **Verificación**:
   ```bash
   dotnet build zxsinclair.net.slnx
   dotnet test ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype.Tests --logger "console;verbosity=detailed"
   dotnet test zxsinclair.net.slnx --filter "Category!=Browser"
   ```
   Manual: lanzar el host (perfil `http`, `http://localhost:5218`), pulsar TestAudio y oír un do continuo sin
   cortes; pulsar de nuevo para pararlo; probar `?audioFrames=2` para ver faltas con poco margen.

## Riesgos / cosas a vigilar

- **Objetos JS por frame**: `postMessage` con transferencia crea envoltorios nuevos (`ArrayBuffer`/`Float32Array`)
  en cada ida y vuelta; la memoria de muestras sí se reutiliza. Es el coste inherente al transporte sin SAB;
  medirlo y documentarlo frente al anillo SAB del diseño final.
- **Jitter del worker**: el siguiente frame depende de que el worker procese el mensaje antes de que se vacíe el
  margen (`target − 1` frames). Con `target = 2` solo hay 20 ms de margen; por eso el valor por defecto es 3 y es
  configurable.
- **Activación por gesto**: el clic de Blazor llega a JS de forma asíncrona; `resume()` debe llamarse dentro de la
  ventana de activación transitoria (≈ 5 s en Chrome). Comprobar `ctx.state === 'running'`.
- Headless sin dispositivo de audio: verificar que Chromium sigue tirando del grafo; si no, flag de autoplay o
  `ZX_PLAYWRIGHT_HEADED=1`.
- Pestaña oculta: los workers se ralentizan; como el reloj es el worklet, solo aparecerán faltas. Se reporta, no se
  corrige.
- Clic audible al parar/arrancar: el worklet atenúa a 0 en falta; aceptable para el prototipo.
- `MathF.Sin` por muestra (~960 por frame) es despreciable, pero no representa el coste de la síntesis real del
  beeper/AY.

## Fuera de alcance

- Ring buffer con `SharedArrayBuffer`/`Atomics` y cabeceras COOP/COEP (siguiente prototipo, para comparar).
- Integrar audio y vídeo en un bucle único `RunFrame()` + `Present()` con estadísticas `stats` periódicas.
- Síntesis real (beeper, AY), estéreo, remuestreo y medición Release/AOT.

## Resultado (2026-10-12)

Implementados C4 continuo en C#, interop de floats como bytes, pool transferido, FIFO fija del worklet,
reloj por devolución, botón start/stop y estadísticas. La parada espera el drenaje y la devolución de todos
los buffers antes de permitir el reinicio; el test cubre ese ciclo con objetivos 3 y 2.
El rango de muestreo admitido es 8 000–192 000 Hz; el buffer de 4096 cubre el máximo de 3840 muestras por frame.
La estimación de frecuencia interpola cruces ascendentes para evitar la cuantización a una muestra.

Verificación:

- `dotnet build zxsinclair.net.slnx -c Debug -m:1`: 0 advertencias, 0 errores.
- Suite Browser, `--no-build`, `ZX_PLAYWRIGHT_CHANNEL=chrome`: 3 casos correctos, incluidos vídeo y ambos objetivos de audio.
- Tests de solución, `--no-build --filter "Category!=Browser"`: Core 2853 y generador 174 correctos.
- Runner FUSE, `--no-build`: 1335 correctos, 0 fallos y 0 omitidos; 8 convenciones BIT (HL).

La primera compilación en sandbox falló con MSB4216/MSB4027 al crear el proceso auxiliar WASM. Pasó fuera
del sandbox. El primer intento Browser no encontró el Chromium de Playwright; se verificó con Chrome
154.0.8037.98 instalado, headless, Debug interpretado. No se añadió el flag de autoplay.

A 48 kHz: frecuencia 261,625607 Hz; faltas/descartes 0/0 en ambos objetivos. Media/máximo por frame:
objetivo 3, 0,094/0,900 ms; objetivo 2, 0,077/0,900 ms. El informe por objetivo está en
`TestResults/audio-loop.json` (ignorado); los resultados se recogen en la spec del prototipo.
No se realizó escucha manual, ejecución con ventana ni Release/AOT. No se midieron reservas o GC del
transporte; solo se reutiliza el almacenamiento de muestras, con nuevos envoltorios y mensajes JS.
