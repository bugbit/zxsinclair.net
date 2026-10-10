# Plan: test de navegador (Playwright) para medir Test2 y TestPutImage

Fecha: 2026-10-10
Estado: implementado y verificado con Chrome instalado; instalación de Chromium pendiente por timeout de descarga

## Objetivo
Crear un test de navegador con Playwright para `ZXSinclair.Net.Web.Prototype.Client` que mida cuánto tardan los
botones **Test2** y **TestPutImage** del componente `Emulator.razor` (pulsación en frío y estadísticas en caliente).

## Contexto
- `Emulator.razor` (Home `/`): canvas 320x240 y tres botones Blazor `@onclick`.
  - **Test2** (`Test2Async`): rellena en C# (hilo UI de WASM) un `byte[320*240*4]` y llama
    `module.InvokeVoidAsync("putImagen2D", data)` → `pixels.set` + `putImageData` en `Emulator.razor.js`.
  - **TestPutImage** (`TestPutImageAsync`): llama a `TestPutImage()` de `Emulator.razor.js`, que pasa por
    `Clients/ClientWorkTest.razor.js` → `postMessage` al Web Worker `Workers/WorkTest.razor.js`, que arranca su
    propio runtime .NET (`dotnet.create()`), ejecuta `[JSExport] WorkTest.TestPutImage()`
    (`ZXSinclair.Net.Web.Prototype.Workers/WorkTest.cs`) y devuelve los bytes; luego `putImagen2D`.
    **La primera pulsación incluye el arranque del runtime del worker** (frío) y las siguientes no (caliente).
- El servidor es `ZXSinclair.Net.Web.Prototype` (host Blazor Web App, perfil `http` en `http://localhost:5218`;
  ya existe `.claude/launch.json` con `web-prototype`). En Release el cliente tiene `RunAOTCompilation`, que solo
  se aplica al **publicar**; con `dotnet run` (Debug) el WASM va interpretado, así que los tiempos dependen mucho
  de la configuración.
- No hay ningún proyecto de test para la web ni dependencia de Playwright. Los tests existentes son xUnit
  (`ZXSinclair.Net.Core.Tests`: xunit 2.9.3, Microsoft.NET.Test.Sdk 17.14.1).
- Hoy no hay señal en la página de "handler terminado" ni de "módulo JS cargado", así que Playwright no puede
  medir con precisión desde fuera (el clic de Playwright mediría además su propia latencia CDP).
- Bug visto: `dispose()` en `Emulator.razor.js` usa `failRequests` y `workerError`, que no existen en ese módulo
  (ReferenceError al desmontar el componente).
- Enfoque: medir **dentro de la página** (Stopwatch en el handler C#, que cubre todo el trabajo: relleno/worker +
  interop + `putImageData`) y publicar el resultado a JS; Playwright pulsa, espera el resultado y agrega
  estadísticas. Test en C# con xUnit + `Microsoft.Playwright`, coherente con el resto del repo.

## Pasos
1. **Instrumentar el prototipo** (cambios mínimos, fuera del hot path del emulador):
   - `Emulator.razor.js`: en `initCanvas` marcar `canvas.dataset.ready = "1"`; añadir
     `export function reportTiming(name, ms)` que haga `(globalThis.__zxTimings ??= []).push({ name, ms })`.
   - `Emulator.razor`: en `Test2Async` y `TestPutImageAsync` tomar `Stopwatch.GetTimestamp()` al inicio y
     `Stopwatch.GetElapsedTime(start)` tras el `await` de la llamada JS; después (fuera de la medida) llamar
     `module.InvokeVoidAsync("reportTiming", "Test2"|"TestPutImage", elapsed.TotalMilliseconds)`.
     Añadir `id`/`data-testid` a los botones (`test2`, `test-put-image`) para selectores estables.
   - Corregir `dispose()` de `Emulator.razor.js` (quitar `failRequests(...)` y `workerError = null`) para que el
     cierre de página no genere errores de consola que ensucien el test.
2. **Crear el proyecto** `ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype.Tests/` (xUnit, `net10.0`),
   con `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk` (mismas versiones que Core.Tests) y
   `Microsoft.Playwright` (última estable, vía `dotnet add package`). Sin referencia al Core. Añadirlo a
   `zxsinclair.net.slnx`. Todos los `.cs` con la cabecera `#region LICENSE`.
3. **Fixture de servidor** `PrototypeServerFixture` (`IAsyncLifetime`, `ICollectionFixture`):
   - Si existe la variable `ZX_PROTOTYPE_URL`, usar ese servidor ya arrancado (permite medir una publicación
     Release/AOT servida aparte).
   - Si no, lanzar `dotnet run --project ../ZXSinclair.Net.Web.Prototype --no-launch-profile
     --urls http://127.0.0.1:<puerto libre>` con `ASPNETCORE_ENVIRONMENT=Development`, sondear `GET /` hasta 200
     (timeout ~120 s, primer build incluido) y matar el árbol de procesos en `DisposeAsync`.
   - Crear `IPlaywright` + navegador Chromium headless (`ZX_PLAYWRIGHT_HEADED=1` opcional para verlo).
4. **Test** `EmulatorButtonTimingTests` (`[Trait("Category", "Browser")]`):
   - Navegar a `/`, esperar `canvas[data-ready="1"]` (WASM cargado y módulo importado).
   - Helper `ClickAndMeasureAsync(selector, name)`: lee el nº de entradas de `__zxTimings` con ese nombre, hace
     clic, `WaitForFunctionAsync` hasta que aumente y devuelve el `ms` nuevo. Medir también el tiempo de pared
     del lado Playwright (clic → resultado) como dato secundario.
   - Por botón: 1 pulsación **fría** reportada aparte (relevante sobre todo en TestPutImage por el arranque del
     worker), luego ~5 de calentamiento descartadas y N=30 medidas (configurable con `ZX_TIMING_ITERATIONS`).
   - Comprobación funcional: tras cada medida leer un píxel del canvas con `getImageData` y verificar alfa 255 y
     verde/azul 0 (se pintó la imagen).
   - Fallar si hay mensajes `console` de tipo error o `pageerror` durante el test.
   - Informe vía `ITestOutputHelper`: frío, min, mediana, p95, max y media en ms por botón, más configuración
     (URL, navegador/versión). Escribir también `TestResults/button-timings.json` para comparar ejecuciones.
     El test no impone umbrales de tiempo (es medición, no regresión).
5. **Documentar** en `CLAUDE.md` y en `AGENTS.md` (sección Commands/Projects de cada uno, con el mismo
   contenido): instalación del navegador
   (`pwsh ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype.Tests/bin/Debug/net10.0/playwright.ps1 install chromium`),
   comando `dotnet test ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype.Tests --logger "console;verbosity=detailed"`,
   las variables `ZX_PROTOTYPE_URL`/`ZX_TIMING_ITERATIONS`/`ZX_PLAYWRIGHT_HEADED`, y que con `dotnet run` el
   WASM es Debug/interpretado (para AOT: `dotnet publish -c Release` y apuntar `ZX_PROTOTYPE_URL`).
6. **Verificar**:
   - `dotnet build zxsinclair.net.slnx` sin warnings nuevos.
   - Instalar Chromium y ejecutar el test; comprobar que imprime la tabla de tiempos de ambos botones y que el
     JSON se genera.
   - Probar el modo `ZX_PROTOTYPE_URL` con el servidor arrancado aparte (`web-prototype` de `.claude/launch.json`).
   - `dotnet test zxsinclair.net.slnx --filter "Category!=Browser"` sigue igual (Core 2832, generador 156).

## Riesgos / cosas a vigilar
- Debug vs Release/AOT: los números con `dotnet run` no representan el rendimiento final; el informe debe decir
  qué se midió.
- `putImageData` es síncrono, pero la presentación real ocurre en el siguiente frame; la medida no incluye el
  compositor (se podría añadir opcionalmente una espera a `requestAnimationFrame`).
- Resolución de `performance.now()` reducida (~100 µs) sin aislamiento cross-origin; suficiente para ms.
- Arranque del servidor en el test: primer build lento, puertos ocupados, procesos huérfanos si el test aborta
  (matar el árbol con `Process.Kill(entireProcessTree: true)`).
- Ruido de medida (GC del runtime WASM, máquina cargada): usar mediana/p95 y no umbrales.
- Si el proyecto se añade al `.slnx`, `dotnet test` de la solución lo ejecutará: requiere navegadores instalados;
  el trait `Category=Browser` permite excluirlo.

## Fuera de alcance
- Optimizar Test2/TestPutImage (p. ej. transferir `ArrayBuffer` o `SharedArrayBuffer` desde el worker).
- Medir la ruta WebGL (`useWebGL` está a `false`).
- Integración en CI y caché de navegadores Playwright.
- Benchmarks del emulador real (no existe aún `SpectrumEmulator`).

## Resultado de ejecución (2026-10-10)

- Se implementaron la instrumentación, el proyecto xUnit/Playwright, el fixture y los informes; se actualizaron AGENTS.md y CLAUDE.md.
- `dispose()` ya estaba corregido antes de ejecutar el plan. Se declaró un favicon vacío en App.razor para evitar el 404 de `/favicon.ico` detectado por la comprobación de consola.
- Playwright 1.63.0 se instaló desde NuGet. La instalación de Chromium falló tras cinco timeouts del CDN. Se añadió `ZX_PLAYWRIGHT_CHANNEL=chrome|msedge` como opción; Chromium sigue siendo el navegador predeterminado.
- El test pasó con Chrome 154.0.8037.98 headless, con servidor propio y con `ZX_PROTOTYPE_URL=http://localhost:5218` (perfil `http` de web-prototype). Cada ejecución tomó una pulsación inicial, cinco de calentamiento y treinta medidas por botón; píxel y ausencia de errores de consola verificados.
- Servidor propio, Debug interpretado: Test2 frío 9,5 ms, mediana 3,0 ms, p95 3,2 ms; TestPutImage frío 174,8 ms, mediana 12,8 ms, p95 15,1 ms. Son tiempos del handler, sin compositor.
- Informes locales ignorados por Git: `TestResults/button-timings-owned-server.json` y `TestResults/button-timings.json` (segunda ejecución con servidor externo). No se midió Release/AOT.
- Build de solución: 0 warnings/errores. Regresión sin Browser: Core 2853 y generador 174, todos correctos (las cifras del plan estaban desactualizadas). FUSE: 1335 pasados, 0 fallos, 0 omitidos; 8 con convención BIT (HL).
- El build y las pruebas requirieron ejecución fuera del sandbox para permitir procesos auxiliares de MSBuild y navegador. Los servidores creados se cerraron al terminar.