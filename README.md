# zxsinclair.net
ZXSinclair Emulador ZX Computers make in .Net CORE C# Blazor WebAssembly

## Rendimiento ante todo

La optimización y la velocidad son la esencia de este proyecto y lo que lo distingue de otros emuladores parecidos. El objetivo es un código C# muy eficiente, optimizado y rápido, capaz de emular el Z80 y la ULA con precisión de ciclo a velocidad completa, también en Blazor WebAssembly.

Principios para el código del núcleo de emulación (ejecución de instrucciones, acceso a memoria y bus, temporización, vídeo):

- Sin reservas de memoria, LINQ, boxing, closures ni `async` en el bucle de ejecución.
- Mínimas llamadas por interfaz o virtuales: clases `sealed`, tipos concretos, structs, `switch` y tablas precalculadas.
- Operaciones de bits sobre enteros en lugar de utilidades con coste oculto como `Enum.HasFlag`.
- Toda abstracción en el camino crítico debe justificar su coste, y los cambios de rendimiento se respaldan con mediciones (BenchmarkDotNet).

## Desarrollo con .NET 10

Todos los proyectos usan `net10.0`. Instala el SDK de .NET 10 y su runtime. La interfaz Blazor WebAssembly está prevista; todavía no está implementada.

El emulador se está reescribiendo en `ZXSinclair.Net.Core` a partir de las especificaciones de `Specs/` (`spec-buses-memoria.md`, `spec-cpu-z80.md`). La CPU Z80 del Core todavía no existe.

Ejecuta desde la raíz del repositorio:

```sh
dotnet restore zxsinclair.net.slnx
dotnet build zxsinclair.net.slnx -c Debug --no-restore -m:1
dotnet build zxsinclair.net.slnx -c Release --no-restore -m:1
dotnet test ZXSinclair.Net.Core.Tests
dotnet run --project ZXSinclair.Net.Test
```

`ZXSinclair.Net.Core.Tests` contiene los tests xUnit del Core. `ZXSinclair.Net.Test` es el ejecutor de los tests Z80 de FUSE (en Debug o Release); hoy omite todos los casos porque la CPU aún no tiene instrucciones. Los ficheros FUSE, su parser y el bus de pruebas están en la librería `ZXSinclair.Net.Fuse`, compartida con los tests xUnit. El generador de instrucciones (`ZXSinclair.Net.Generate.Z80OpCodes`) se rehará desde cero; de momento solo conserva las tablas de opcodes de FUSE.
