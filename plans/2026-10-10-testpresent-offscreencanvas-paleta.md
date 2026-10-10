# Plan: TestPresent — índices de paleta + OffscreenCanvas en el worker (WebGL2 y 2D)

Fecha: 2026-10-10
Estado: implementado y verificado en Debug con Chrome (headless y headed)

## Objetivo
Prototipar la presentación recomendada por `Specs/spec-frontend-blazor.md` §6.1 (1 byte/píxel con índice de
paleta, `OffscreenCanvas` transferido al worker, WebGL2 con textura `R8UI` + paleta 16×1 en shader y alternativa
2D) como nuevo botón **TestPresent**, y medirlo con el test Playwright existente para compararlo con Test2 y
TestPutImage.

## Contexto
- `TestPutImage` genera RGBA (300 KB) en C# dentro del worker, lo copia al marshalling (`byte[]` → `Uint8Array`),
  lo clona con `postMessage` y lo vuelve a copiar con `pixels.set` en el hilo principal.
- El test Playwright ya implementado (`plans/2026-10-10-test-playwright-tiempos-botones.md`) midió en Debug
  interpretado: Test2 mediana 3,0 ms; TestPutImage mediana 12,8 ms (frío 174,8 ms).
- `Client/Components/Emulator.razor`: un canvas 320×240 (2D en hilo principal, `initCanvas` marca `data-ready`),
  botones `test2`/`test-put-image` con `Stopwatch` y `reportTiming(name, ms)` → `globalThis.__zxTimings`.
- `Client/Clients/ClientWorkTest.razor.js`: worker perezoso compartido, `pendingRequests` por `requestId`,
  `failRequests`, `dispose`. `Client/Workers/WorkTest.razor.js`: `dotnet.create()`,
  `setModuleImports('emulator', { createResult })`, switch de comandos.
- `Workers/WorkTest.cs`: `[JSImport] CreateResult`, `[JSExport] Test/TestPutImage` (buffer `_data1` ya reutilizado).
- `Tests/EmulatorButtonTimingTests.cs`: frío + 5 calentamiento + N medidas, comprobación de píxel con
  `document.querySelector('canvas').getContext('2d')`, fallo con errores de consola/HTTP, JSON en `TestResults/`.
  `PrototypeServerFixture.cs`: servidor propio o `ZX_PROTOTYPE_URL`, canal `ZX_PLAYWRIGHT_CHANNEL`, headless.
- Decisiones:
  - **Canvas aparte** para TestPresent: tras `transferControlToOffscreen()` el hilo principal no puede usar ese
    canvas, y Test2/TestPutImage siguen siendo la referencia de la vía actual.
  - **Mismo worker** que TestPutImage (no un tercer runtime .NET); el frío de TestPresent mide solo la
    transferencia y la inicialización gráfica si se ejecuta después de TestPutImage, y se documenta así.
  - **Backend elegible por query string** (`/?present=webgl2|2d`, por defecto `webgl2` con caída a 2D) para que el
    test mida ambos en contextos separados.

## Pasos
1. **C# del worker** (`ZXSinclair.Net.Web.Prototype.Workers/WorkTest.cs`):
   - `private static readonly byte[] sFrame = new byte[320 * 240];`
   - `[JSImport("present", "emulator")] static partial void Present([JSMarshalAs<JSType.MemoryView>] Span<byte> frame);`
   - `[JSExport] internal static int TestPresent()`: `index = Random.Shared.Next(0, 16)`,
     `sFrame.AsSpan().Fill((byte)index)`, `Present(sFrame)`, devuelve `index`. Sin reservas por llamada.
2. **JS del worker** (`Client/Workers/WorkTest.razor.js`), presentación en el propio worker:
   - Paleta Spectrum fija de 16 colores (`0x00`/`0xD7` normal, `0xFF` brillo) como `Uint8Array(64)` y como
     `Uint32Array(16)` (`0xAABBGGRR`).
   - `initPresent(canvas, w, h, backend)`: si `backend==='webgl2'`, llamar a
     `getContext('webgl2', { alpha:false, antialias:false })` y distinguir dos casos:
     - **Devuelve `null`** (WebGL2 no disponible): no se ha creado ningún contexto, así que se cae a 2D.
     - **Devuelve un contexto**: inicializar `UNPACK_ALIGNMENT=1`, textura índices `texStorage2D(R8UI)` NEAREST,
       textura paleta `RGBA8` 16×1 NEAREST, programa (vertex de triángulo a pantalla completa con `gl_VertexID`,
       fragment con `usampler2D` + `texelFetch` en la paleta) y VAO vacío. Si falla la compilación/enlace de
       shaders (`getShaderParameter`/`getProgramParameter` con log), la creación de texturas o `getError()` tras
       la inicialización, **lanzar el error** con el detalle: el canvas ya está ligado a WebGL2 y no admite un
       contexto 2D, así que no hay caída posible.
     Si `backend==='2d'` o WebGL2 devolvió `null`: `getContext('2d')`, `ImageData` y `Uint32Array` de salida
     preasignados; si también devuelve `null`, lanzar error. Buffer `frameView = new Uint8Array(w*h)`
     preasignado. Guardar el backend efectivo.
   - `present(view)` (registrado en `setModuleImports` junto a `createResult`): `view.copyTo(frameView)`;
     WebGL2 → `texSubImage2D(RED_INTEGER, UNSIGNED_BYTE)` + `drawArrays(TRIANGLES, 0, 3)`; 2D → bucle
     `out[i] = pal[frameView[i]]` + `putImageData`. Sin reservas por frame.
   - Comandos nuevos: `InitPresent` (recibe el `OffscreenCanvas` transferido, responde `{ backend }` o el error
     de inicialización por el canal `error` existente),
     `TestPresent` (mide `performance.now()` alrededor del export, responde `{ index, workerMs }`),
     `ReadPresentPixel` (vuelve a dibujar el último frame y lee un píxel con `readPixels`/`getImageData` en la
     misma tarea, responde RGBA; solo para verificación, fuera de la medida).
3. **Cliente JS** (`Client/Clients/ClientWorkTest.razor.js`): extraer un helper `request(command, payload, transfer)`
   que reutilice `pendingRequests` (Test/TestPutImage pasan a usarlo) y exportar `InitPresent(canvas, w, h, backend)`
   (con `[canvas]` como transferible), `TestPresent()`, `ReadPresentPixel()`.
4. **Componente** (`Client/Components/Emulator.razor` y `.razor.js`):
   - Segundo `<canvas data-testid="present-canvas" width="320" height="240">`; `data-testid="main-canvas"` en el
     actual; botón `<button data-testid="test-present">TestPresent</button>`.
   - En `.razor.js`: `TestPresent(canvasElement)` que en la primera llamada lee `?present=` de `location.search`,
     hace `transferControlToOffscreen()` e `InitPresent` (una sola vez, con guarda), guarda el backend efectivo en
     `canvasElement.dataset.backend`, y después `await WorkTest.TestPresent()`; devuelve `{ index, workerMs }`.
     Si `InitPresent` falla, guardar ese error y relanzarlo en las llamadas siguientes (el canvas ya está
     transferido y no se puede reintentar).
     `readPresentPixel()` exportado para el test. `reportTiming(name, ms, extra)` añade campos opcionales
     (`workerMs`, `index`, `backend`).
   - `TestPresentAsync` en C#: `Stopwatch` alrededor de `InvokeAsync<PresentResult>("TestPresent", presentCanvas)`
     y `reportTiming("TestPresent", ms, extra)` fuera de la medida (record `PresentResult(int Index, double WorkerMs)`).
5. **Test** (`Tests/EmulatorButtonTimingTests.cs`):
   - Cambiar la comprobación de píxel de Test2/TestPutImage a `[data-testid=main-canvas]`.
   - Añadir medición de TestPresent para `webgl2` y `2d`, cada uno en un contexto nuevo con `/?present=<b>`,
     después de pulsar una vez TestPutImage para que el runtime del worker esté arrancado (el frío de TestPresent =
     transferencia + init gráfico; documentarlo en el informe). Mismo esquema frío + 5 + N.
   - Verificación por muestra: `readPresentPixel()` debe coincidir con la entrada de paleta del `index` devuelto.
   - Comprobar `data-backend`: si se pidió `webgl2` y quedó `2d` (WebGL2 devolvió `null`), fallar con mensaje
     claro (o reportarlo como no disponible si `ZX_ALLOW_WEBGL_FALLBACK=1`). Un error de inicialización de WebGL2
     con contexto ya creado nunca se trata como caída: llega como error de página/consola y el test falla.
   - Informe/JSON: filas `TestPresent (webgl2)` y `TestPresent (2d)` con estadísticas de handler, pared Playwright
     y `workerMs`, además del backend efectivo y el renderer WebGL (`UNMASKED_RENDERER` si está disponible, para
     saber si fue GPU o SwiftShader).
   - `PrototypeServerFixture`: si hace falta en headless, añadir `--enable-unsafe-swiftshader` a los argumentos de
     lanzamiento (y anotar en el informe que es WebGL por software).
6. **Documentar** en `CLAUDE.md` y `AGENTS.md` (mismo texto): botón TestPresent, `?present=webgl2|2d`, nueva
   variable si se añade, y que en headless WebGL2 suele ir por SwiftShader (para GPU real:
   `ZX_PLAYWRIGHT_HEADED=1`). Licencia `#region LICENSE` en los `.cs` tocados.
7. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings nuevos.
   - Test Browser con servidor propio (Debug) en headless y en headed; comprobar que las tres mediciones aparecen,
     píxeles correctos y sin errores de consola.
   - Comprobar a mano con `preview_start web-prototype` que TestPresent pinta en el segundo canvas con ambos
     backends y que Test2/TestPutImage siguen funcionando.
   - Regresión: `dotnet test zxsinclair.net.slnx --filter "Category!=Browser"` (Core y generador sin cambios).
   - Anotar en el plan los resultados comparados (TestPutImage vs TestPresent webgl2/2d).

## Riesgos / cosas a vigilar
- **WebGL2 en headless**: Chromium reciente ya no cae a SwiftShader automáticamente; puede no haber WebGL2 o ir
  por software. Los tiempos webgl2 en headless no representan GPU real; registrar el renderer.
- **Caída a 2D solo antes de crear contexto**: un canvas (u `OffscreenCanvas`) con contexto WebGL2 ya creado
  devuelve `null` en `getContext('2d')`; por eso la caída solo se permite cuando `getContext('webgl2')` devuelve
  `null`, y cualquier fallo posterior de shaders/texturas se propaga.
- **`IMemoryView` en JS**: la vista de un `Span` solo es válida durante la llamada; confirmar la API
  (`copyTo`/`set`/`slice`) del runtime .NET 10 antes de implementar. No usar `_unsafe_create_view` salvo medición
  que lo justifique.
- **Transferencia única**: `transferControlToOffscreen()` falla si se llama dos veces o si el canvas ya tiene
  contexto; si Blazor recreara el elemento habría que transferir de nuevo. Al terminar el worker (dispose) el
  canvas queda sin contenido.
- **Medida del frame**: el `OffscreenCanvas` del worker se presenta al terminar la tarea; el handler mide hasta la
  respuesta, no hasta el compositor (igual que las otras mediciones).
- **`readPixels`** fuerza sincronización con la GPU: solo en el comando de verificación, nunca en la ruta medida.
- **Orden y estado compartido del worker**: TestPresent y TestPutImage comparten runtime; el frío de cada uno
  depende del orden, que el test fija y el informe declara.
- Debug interpretado vs Release/AOT: la comparación válida es entre botones en la misma configuración.

## Fuera de alcance
- Escalado, scanlines/CRT en el shader, framebuffer 352×296 con borde y el contrato `SpectrumEmulator`.
- Alternativa sin `OffscreenCanvas` (buffers transferidos al hilo principal) y `SharedArrayBuffer`.
- Optimizar Test2/TestPutImage (relleno con `uint`, `MemoryView` + transferencia).
- Medir Release/AOT publicado (posible con `ZX_PROTOTYPE_URL`, pero no forma parte de este plan).
- Actualizar `Specs/spec-frontend-blazor.md` con los resultados (se puede hacer después con los números).

## Resultado de ejecución (2026-10-10)

- Implementados TestPresent, el canvas independiente transferido una vez al worker, el Span/MemoryView síncrono, la paleta de 16 colores y los backends WebGL2 R8UI y 2D. Los buffers de índices y conversión se reutilizan. No se ha medido la asignación total del runtime/interop.
- Se conservan los cambios previos del usuario: `_data1` reutilizado en TestPutImage y ubicación del proyecto Tests en la solución.
- La API `IMemoryView.copyTo(target: TypedArray, sourceOffset?: number)` se verificó en el `dotnet.d.ts` del runtime local 10.0.12. No se usa `_unsafe_create_view`.
- El test usa tres contextos: Test2/TestPutImage de referencia y uno por backend de TestPresent. Cada botón tiene una primera pulsación, cinco de calentamiento y treinta muestras. Antes de TestPresent se arranca el worker con una pulsación de TestPutImage; su primera medida no incluye el arranque del runtime.
- Ambas ejecuciones Browser pasaron en Chrome 154.0.8037.98: headless y headed, píxel esperado tras cada llamada, backend efectivo correcto y sin errores de consola/página/HTTP. Se permite SwiftShader solo en headless con el flag del plan, pero el renderer real de ambas ejecuciones fue ANGLE/NVIDIA GeForce RTX 5060 Ti/Direct3D11, no SwiftShader. No se añadió ZX_ALLOW_WEBGL_FALLBACK: el test exige el backend solicitado.

Medianas en ms, Debug interpretado (30 muestras por fila):

| Botón | Handler headless | Worker headless | Handler headed | Worker headed |
|---|---:|---:|---:|---:|
| Test2 | 3,000 | no medido | 3,000 | no medido |
| TestPutImage | 12,200 | no medido | 13,750 | no medido |
| TestPresent (webgl2) | 14,650 | 0,100 | 14,350 | 0,100 |
| TestPresent (2d) | 14,800 | 0,200 | 14,550 | 0,200 |

- Primeras pulsaciones headless: Test2 9,7 ms; TestPutImage 180,6 ms; TestPresent WebGL2 21,2 ms y 2D 18,8 ms (worker ya arrancado). p95 del handler: 3,3 / 13,4 / 15,9 / 15,5 ms, respectivamente.
- La ruta de presentación dentro del worker es corta, pero estas muestras no demuestran una mejora del tiempo completo del handler frente a TestPutImage. El tiempo completo incluye el recorrido asíncrono entre Blazor y el worker. No se midió throughput continuo ni compositor. La lectura de verificación redibuja y sincroniza la GPU entre llamadas, fuera del intervalo medido.
- Informes locales ignorados por Git: `TestResults/button-timings-present-headless.json`, `TestResults/button-timings-present-headed.json` y `TestResults/button-timings.json` (última ejecución).
- Comprobación visual realizada en Chrome mediante CUA sobre el perfil `http` de web-prototype, en `localhost:5218/?present=webgl2` y `?present=2d`: ambos canvases pintan; Test2 y TestPutImage siguen operativos. Capturas locales: `TestResults/testpresent-webgl2.png` y `TestResults/testpresent-2d.png`. `preview_start` no está disponible en esta sesión; el navegador integrado agotó el timeout de apertura. Se usó Chrome como alternativa. La pestaña y el servidor de comprobación se cerraron al terminar.
- Build: 0 warnings/errores. Regresión sin Browser: Core 2853, generador 174, todos pasados. FUSE: 1335 pasados, 0 fallos, 0 omitidos; 8 con la convención declarada de BIT (HL).
- No se midió Release/AOT, fuera del alcance del plan. No se instaló Chromium ni se modificó la especificación del frontend. AGENTS.md y CLAUDE.md contienen la misma documentación del nuevo botón y los límites de la medida.