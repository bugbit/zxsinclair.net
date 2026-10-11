# Prototipo web: worker .NET y render

Documenta el prototipo `ZXSinclair.Net.Web.Prototype` tal como está implementado (2026-10-10): el Web Worker con
runtime .NET propio, el protocolo de mensajes con el hilo principal, las tres rutas de render que se comparan
(Test2, TestPutImage y TestPresent) y su medición con Playwright. Es la base experimental de la fase 0 de
[spec-frontend-blazor.md](spec-frontend-blazor.md); no contiene emulación.

Planes de origen: [plans/2026-10-10-test-playwright-tiempos-botones.md](../plans/2026-10-10-test-playwright-tiempos-botones.md)
y [plans/2026-10-10-testpresent-offscreencanvas-paleta.md](../plans/2026-10-10-testpresent-offscreencanvas-paleta.md),
cada uno con su sección de resultados.

## 1. Proyectos

| Proyecto | Tipo | Papel |
|---|---|---|
| `ZXSinclair.Net.Web.Prototype` | ASP.NET Core (Blazor Web App) | Host. `App.razor` monta `Routes` en `InteractiveWebAssembly`; perfil `http` en `http://localhost:5218`. Declara un favicon vacío (`data:,`) para que no haya un 404 que rompa la comprobación de errores del test |
| `ZXSinclair.Net.Web.Prototype.Client` | Blazor WebAssembly | Componente `Components/Emulator.razor` y los módulos JS: `Components/Emulator.razor.js` (hilo principal), `Clients/ClientWorkTest.razor.js` (cliente del worker) y `Workers/WorkTest.razor.js` (script del worker). En Release: `RunAOTCompilation`, `InvariantGlobalization`, `PublishTrimmed` (solo al publicar) |
| `ZXSinclair.Net.Web.Prototype.Workers` | Biblioteca `net10.0`, `AllowUnsafeBlocks` | `WorkTest.cs`: `[JSExport]`/`[JSImport]` que ejecuta el runtime del worker. El cliente la referencia para que su ensamblado se publique en `_framework/` |
| `ZXSinclair.Net.Web.Prototype.Tests` | xUnit + Microsoft.Playwright | Medición de los botones en navegador (sección 6). Sin referencia al Core |

`Clients/ClientWorkTest.razor` y `Workers/WorkTest.razor` son ficheros vacíos (solo un comentario): existen para
que Blazor trate los `.razor.js` asociados como recursos estáticos y los publique en
`Clients/ClientWorkTest.razor.js` y `Workers/WorkTest.razor.js`.

## 2. Hilos

```
Hilo principal                                         Web Worker (module)
──────────────                                         ───────────────────
Blazor (.NET UI)                                       WorkTest.razor.js
  Emulator.razor ──IJSObjectReference──►                 dotnet.create()   (runtime .NET propio)
  Emulator.razor.js                                      setModuleImports('emulator', { createResult, present })
    ClientWorkTest.razor.js ──postMessage(cmd)──►        getAssemblyExports('ZXSinclair.Net.Web.Prototype.Workers.dll')
                            ◄──postMessage(response)──   WorkTest.cs  [JSExport]/[JSImport]
  <canvas main-canvas>  (2D, hilo principal)
  <canvas present-canvas> ──transferControlToOffscreen──► OffscreenCanvas (WebGL2 o 2D, en el worker)
```

- El worker se crea **perezosamente** en la primera petición (`getWorker()`), como `new Worker(new URL('../Workers/WorkTest.razor.js', document.baseURI), { type: 'module' })`.
- El script del worker importa `../_framework/dotnet.js` y arranca el runtime nada más cargarse
  (`runtimeReady`); cada mensaje espera a `runtimeReady`. Por eso la **primera** petición incluye el arranque
  del runtime (≈ 175–180 ms en Debug) y las siguientes no.
- Los fallos de arranque no rompen el worker: `runtimeReady.catch(() => {})` evita el rechazo no gestionado y
  el error llega a cada petición por el canal de respuesta.

## 3. Protocolo de mensajes

`ClientWorkTest.razor.js` implementa un RPC mínimo sobre `postMessage`:

- `request(command, payload = {}, transfer = [])` asigna `requestId` creciente, guarda `{ resolve, reject }` en
  `pendingRequests` y envía `{ ...payload, command, requestId }` con la lista de transferibles.
- El worker responde siempre `{ command: 'response', requestId, result }` o `{ command: 'response', requestId, error: message }`;
  el cliente resuelve o rechaza la promesa correspondiente e ignora respuestas desconocidas.
- `error` y `messageerror` del worker llaman a `failRequests(error)`: guarda el error, termina el worker y
  rechaza todas las peticiones pendientes. Mientras `workerError` esté fijado, `getWorker()` lo relanza.
- `dispose()` (llamado desde `Emulator.razor.js` al desmontar el componente) hace `failRequests('Emulator disposed.')`
  y después limpia `workerError`, de modo que una nueva instancia del componente crea un worker nuevo.

| Comando | Payload / transferibles | Trabajo en el worker | Resultado |
|---|---|---|---|
| `Test` | — | `WorkTest.Test()` | `{ value: 42, data: Uint8Array[1..5] }` |
| `TestPutImage` | — | `WorkTest.TestPutImage()` | `{ value: 42, data: Uint8Array(307200) }` RGBA |
| `InitPresent` | `{ canvas, width, height, backend }`, transfiere `[canvas]` | `initPresent(...)` | `{ backend, renderer }` |
| `TestPresent` | — | `WorkTest.TestPresent()` medido con `performance.now()` | `{ index, workerMs }` |
| `ReadPresentPixel` | — | redibuja y lee el píxel (0,0) | `[r, g, b, a]` |

## 4. Interop C# del worker (`WorkTest.cs`)

| Miembro | Firma | Notas |
|---|---|---|
| `CreateResult` | `[JSImport("createResult","emulator")] JSObject (int, byte[])` | `byte[]` se **copia** a un `Uint8Array` nuevo en el marshalling |
| `Test` | `[JSExport] JSObject ()` | Prueba básica de ida y vuelta |
| `TestPutImage` | `[JSExport] JSObject ()` | Rellena un RGBA 320×240 (`_data1`, reutilizado) con un rojo aleatorio y alfa 255, byte a byte |
| `Present` | `[JSImport("present","emulator")] void ([JSMarshalAs<JSType.MemoryView>] Span<byte>)` | **Sin copia**: JS recibe una vista sobre la memoria de WASM |
| `TestPresent` | `[JSExport] int ()` | `index = Random.Shared.Next(0,16)`, `sFrame.AsSpan().Fill((byte)index)` sobre `byte[320*240]` estático, `Present(sFrame)`, devuelve `index`. Sin reservas por llamada |

Reglas de `JSType.MemoryView` con `Span<byte>`:

- La vista (`IMemoryView`) solo es válida **durante la llamada síncrona** al import; después queda liberada y
  el heap de WASM puede moverse o crecer. Por eso `present` copia inmediatamente con
  `view.copyTo(frameView)` a un `Uint8Array` preasignado.
- La API `copyTo(target: TypedArray, sourceOffset?: number)` se verificó en el `dotnet.d.ts` del runtime
  10.0.12. No se usa `_unsafe_create_view` (API interna; solo se justificaría con una medición).
- Un `Span` no puede usarse en un import asíncrono ni guardarse en JS.

## 5. Rutas de render

### 5.1 Comparativa

| Ruta | Dónde se genera | Formato | Copias por llamada | Dónde se pinta |
|---|---|---|---|---|
| **Test2** | C# en el hilo de la UI (`Emulator.razor`) | RGBA, 307 200 B (array nuevo por llamada) | marshalling `byte[]`→JS, `pixels.set` | `main-canvas`, 2D, hilo principal (`putImagen2D`) |
| **TestPutImage** | C# en el worker | RGBA, 307 200 B (buffer reutilizado) | marshalling `byte[]`→`Uint8Array`, clonado estructurado de `postMessage`, `pixels.set` | `main-canvas`, 2D, hilo principal |
| **TestPresent** | C# en el worker | índices de paleta, 76 800 B (buffer reutilizado) | `copyTo` a buffer preasignado (y subida a GPU en WebGL2) | `present-canvas` transferido, en el worker |

En el hilo principal, `putImagen2D(data)` hace `pixels.set(data)` sobre un `ImageData` creado una vez en
`initCanvas` y `c2d.putImageData(imageData, 0, 0)`. `initCanvas` marca `canvas.dataset.ready = "1"` cuando el
contexto 2D existe (señal que espera el test). La ruta WebGL del hilo principal (`useWebGL`) existe pero el
componente la llama con `false`.

### 5.2 TestPresent: transferencia del canvas

`Emulator.razor.js` → `TestPresent(canvasElement)`:

1. La primera vez para un elemento dado, crea **y cachea** una promesa `presentReady` antes de transferir, para
   que dos clics seguidos no transfieran el canvas dos veces.
2. Lee el backend de `?present=` (`webgl2` por defecto, o `2d`; otro valor es error).
3. `canvasElement.transferControlToOffscreen()` y `WorkTest.InitPresent(offscreen, width, height, backend)`
   (el `OffscreenCanvas` va en la lista de transferibles).
4. Guarda la respuesta en `canvasElement.dataset.backend` y `dataset.renderer`.
5. Cada llamada espera `presentReady` y después `WorkTest.TestPresent()`.

Una inicialización rechazada **queda rechazada**: el canvas ya está transferido y no se puede reintentar. Solo un
elemento `<canvas>` nuevo (una nueva instancia del componente) vuelve a intentarlo. `dispose()` olvida el
elemento y la promesa; al terminar el worker, el canvas transferido se queda sin contenido.

### 5.3 TestPresent: inicialización en el worker (`initPresent`)

- Solo una vez por worker (`initialized` se fija al entrar, también si falla).
- Reserva `frameView = new Uint8Array(width * height)`.
- **Elección de backend**:
  - Si se pide `webgl2`, `getContext('webgl2', { alpha: false, antialias: false })`.
    - Devuelve `null` → WebGL2 no disponible; no se ha creado contexto y se **cae a 2D**.
    - Devuelve contexto → cualquier fallo posterior (asignación de shader/programa/VAO/textura, compilación,
      enlace, `getError()` al final) **se lanza** con su log. Un canvas con contexto WebGL2 devuelve `null` en
      `getContext('2d')`, así que no hay caída posible.
  - Si se pide `2d` o WebGL2 devolvió `null`: `getContext('2d')`; si también es `null`, error.
- Responde `{ backend, renderer }`. En WebGL2, `renderer` es `UNMASKED_RENDERER_WEBGL` si existe
  `WEBGL_debug_renderer_info` (si no, `RENDERER`); permite saber si es GPU o SwiftShader. En 2D es
  `'Canvas 2D (acceleration not queried)'`.

**WebGL2**:

- `UNPACK_ALIGNMENT = 1`.
- Textura de índices en la unidad 0: `texStorage2D(R8UI, width, height)`, `NEAREST`, `CLAMP_TO_EDGE` (las
  texturas enteras exigen `NEAREST`).
- Textura de paleta en la unidad 1: `RGBA8` 16×1, `NEAREST`, subida una vez.
- Triángulo a pantalla completa generado en el vertex shader con `gl_VertexID` y VAO vacío (sin buffers de vértices).
- Fragment shader: `texelFetch(indices, ivec2(gl_FragCoord.xy), 0).r` → `texelFetch(colors, ivec2(index, 0), 0)`.
  Muestreo 1:1 sin escalado; como `gl_FragCoord` tiene origen abajo, la imagen sale invertida verticalmente
  (indiferente para un relleno uniforme; habrá que tenerlo en cuenta con imágenes reales).
- `viewport(0, 0, width, height)`.

**2D**: `createImageData(width, height)` y una vista `Uint32Array` sobre sus datos, preasignadas.

### 5.4 Paleta

16 colores Spectrum: bit 0 azul, bit 1 rojo, bit 2 verde, bit 3 brillo; nivel 0xD7 sin brillo y 0xFF con brillo;
los dos negros son negro. Se guarda dos veces:

- `palette`: `Uint8Array(64)` RGBA, para la textura WebGL2.
- `palette32`: `Uint32Array(16)` para la conversión 2D. En little-endian vale `0xAABBGGRR`; si el host fuera
  big-endian se recalcula para que los bytes en memoria sigan siendo RGBA.

### 5.5 Presentación por frame (`present` → `drawFrame`)

- `present(view)`: `view.copyTo(frameView)` y `drawFrame()`.
- WebGL2: `texSubImage2D(TEXTURE_2D, 0, 0, 0, w, h, RED_INTEGER, UNSIGNED_BYTE, frameView)` y
  `drawArrays(TRIANGLES, 0, 3)`.
- 2D: `out[i] = palette32[frameView[i]]` para todo el frame y `putImageData(image, 0, 0)`.
- Sin reservas en JS por frame. El `OffscreenCanvas` se presenta al terminar la tarea del worker.

### 5.6 Lectura de verificación (`ReadPresentPixel`)

Redibuja el último frame y lee el píxel (0,0) **en la misma tarea**, porque WebGL puede descartar el buffer de
dibujo tras presentarlo (`preserveDrawingBuffer` es falso). WebGL2 usa `readPixels` (comprueba `getError()`);
2D usa `getImageData`. `readPixels` sincroniza con la GPU: solo se usa para verificar, nunca en la ruta medida.

## 6. Medición

### 6.1 Instrumentación en la página

- Botones con `data-testid`: `test2`, `test-put-image`, `test-present`; canvas `main-canvas` y `present-canvas`.
- Cada handler de `Emulator.razor` toma `Stopwatch.GetTimestamp()` al inicio y `GetElapsedTime` tras el `await`
  de la llamada JS; después, fuera de la medida, llama a `reportTiming(name, ms, extra)`, que añade
  `{ name, ms, ...extra, backend }` a `globalThis.__zxTimings`. TestPresent añade `index` y `workerMs`
  (record C# `PresentResult(int Index, double WorkerMs)`), y `backend` sale de `present-canvas.dataset.backend`.
- El tiempo del handler cubre todo el trabajo hasta la respuesta (relleno, interop, ida y vuelta al worker,
  pintado), pero no la composición en pantalla.

### 6.2 Test Playwright (`EmulatorButtonTimingTests`)

- `PrototypeServerFixture`: usa `ZX_PROTOTYPE_URL` si existe; si no, arranca
  `dotnet run -c Debug --no-launch-profile --urls http://127.0.0.1:<puerto libre>` (Development, WASM
  interpretado), espera `GET /` 200 (120 s) y mata el árbol de procesos al final. Navegador Chromium headless
  por defecto; `ZX_PLAYWRIGHT_CHANNEL=chrome|msedge`, `ZX_PLAYWRIGHT_HEADED=1`. En headless se permite
  SwiftShader con `--enable-unsafe-swiftshader`.
- Contextos de navegador separados: uno para Test2/TestPutImage y uno por backend de TestPresent
  (`/?present=webgl2`, `/?present=2d`). Antes de TestPresent se pulsa TestPutImage una vez para arrancar el
  runtime del worker: el frío de TestPresent es transferencia + inicialización gráfica.
- Por botón: 1 pulsación fría, 5 de calentamiento descartadas y `ZX_TIMING_ITERATIONS` medidas (30 por defecto).
- Comprobaciones por muestra: píxel de `main-canvas` (G = 0, B = 0, A = 255) para Test2/TestPutImage;
  `ReadPresentPixel` igual a la entrada de paleta del `index` devuelto para TestPresent; el backend efectivo debe
  ser el pedido (no se admite caída de WebGL2 a 2D). Falla con cualquier error de consola, `pageerror` o HTTP ≥ 400.
- Informe (`ITestOutputHelper`) y `TestResults/button-timings.json` (ignorado por Git): frío, min, mediana, p95
  (rango más cercano), max y media de los relojes handler, pared de Playwright y worker; URL, navegador,
  configuración del servidor, backend y renderer.

Comandos y variables: sección "Browser button timing tests" de `CLAUDE.md`/`AGENTS.md`.

### 6.3 Resultados (2026-10-10)

Debug interpretado, Chrome 154.0.8037.98, renderer ANGLE/NVIDIA GeForce RTX 5060 Ti/Direct3D11 (no SwiftShader)
tanto en headless como con ventana. Medianas en ms, 30 muestras:

| Botón | Handler headless | Worker headless | Handler con ventana | Worker con ventana |
|---|---:|---:|---:|---:|
| Test2 | 3,000 | — | 3,000 | — |
| TestPutImage | 12,200 | — | 13,750 | — |
| TestPresent (webgl2) | 14,650 | 0,100 | 14,350 | 0,100 |
| TestPresent (2d) | 14,800 | 0,200 | 14,550 | 0,200 |

Primeras pulsaciones (headless): Test2 9,7; TestPutImage 180,6 (incluye arranque del runtime del worker);
TestPresent WebGL2 21,2 y 2D 18,8 (worker ya arrancado). p95 del handler: 3,3 / 13,4 / 15,9 / 15,5.

Interpretación:

- El trabajo de TestPresent dentro del worker (relleno + `copyTo` + subida/conversión + dibujo) es de 0,1–0,2 ms.
- El tiempo del handler de TestPresent no mejora al de TestPutImage: lo domina el recorrido asíncrono
  Blazor → JS → `postMessage` → worker → respuesta → Blazor, no el render. En el diseño final
  ([spec-frontend-blazor.md](spec-frontend-blazor.md) §4) no hay ida y vuelta por frame con la UI: el bucle lo
  conduce el JS del worker y el hilo principal no toca el vídeo, así que el coste relevante es el del worker.
- No se midió throughput continuo, composición, Release/AOT ni reservas del runtime/interop.

## 7. Relación con la especificación del front end

Puntos de [spec-frontend-blazor.md](spec-frontend-blazor.md) §14 que el prototipo aclara parcialmente:

- `JSType.MemoryView` con `Span<byte>` funciona en .NET 10 (10.0.12) con `copyTo`; `Span<float>` sigue sin probar.
- `OffscreenCanvas` con WebGL2 transferido a un worker funciona en Chrome 154 (Windows, GPU NVIDIA). Safari y
  otros navegadores siguen sin verificar.
- `dotnet.create()` en el worker: no se ha verificado si carga los ensamblados de la UI; la primera pulsación
  de TestPutImage puede reutilizar recursos ya descargados por el runtime principal.

Diferencias con el diseño final: el prototipo comparte un worker de prueba entre botones, usa 320×240 en vez de
352×296, no escala ni aplica efectos en el shader, no tiene bucle por frame ni audio, y no usa COOP/COEP.

## 8. Pendiente

- Medir Release/AOT publicado (`ZX_PROTOTYPE_URL`).
- Medir frames continuos dentro del worker (sin ida y vuelta con la UI) y reservas de JS/C# por frame.
- Corregir la inversión vertical del shader antes de usarlo con imágenes reales.
- Verificar `OffscreenCanvas` + WebGL2 en Safari y Firefox.
