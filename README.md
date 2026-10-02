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

Los cuatro proyectos usan `net10.0`. Instala el SDK de .NET 10 y su runtime. La interfaz Blazor WebAssembly está prevista; todavía no está implementada.

Ejecuta desde la raíz del repositorio:

```sh
dotnet restore zxsinclair.net.slnx
dotnet build zxsinclair.net.slnx -c Debug --no-restore -m:1
dotnet build zxsinclair.net.slnx -c Release --no-restore -m:1
dotnet run --project ZXSinclair.Net.Test -c Debug
dotnet run --project ZXSinclair.Net.Generate.Z80OpCodes -c Debug
```

Las pruebas son un ejecutable con `Debug.Assert`: deben ejecutarse en Debug. Los opcodes sin implementar se omiten. El generador sobrescribe los archivos de opcodes del núcleo; modifica sus tablas o plantillas y revisa el resultado. El ejecutable principal sigue siendo un punto de entrada provisional que imprime `Hello, World!`.
