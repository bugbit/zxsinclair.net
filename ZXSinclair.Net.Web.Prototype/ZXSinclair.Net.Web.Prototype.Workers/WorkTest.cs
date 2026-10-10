#region LICENSE
/*
    ZXSinclair Emulador ZX Computers make in .Net and .Net CORE
    Copyright (C) 2016 Oscar Hernandez Bano
    This file is part of ZXSincalir.Net.
    ZXSincalir.Net is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/
#endregion

using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace ZXSinclair.Net.Web.Prototype.Workers;

struct Result
{
    public int Value { get; set; }
    public byte[]? Data { get; set; }
}


[SupportedOSPlatform("browser")]
public partial class WorkTest
{
    [JSImport("createResult", "emulator")]
    private static partial JSObject CreateResult(int value, byte[] data);

    [JSExport]
    internal static JSObject Test()
    {
        return CreateResult(42, new byte[] { 1, 2, 3, 4, 5 });
    }

    [JSExport]
    internal static JSObject TestPutImage()
    {
        byte color = (byte)Random.Shared.Next(0, 255);
        byte[] data = new byte[320 * 240 * 4];

        for (int i = 0; i < data.Length; i += 4)
        {
            data[i] = color;     // Rojo; verde y azul quedan en 0.
            data[i + 3] = 255; // Alfa opaco.
        }

        return CreateResult(42, data);
    }
}
