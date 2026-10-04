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

using System.Numerics;

namespace ZXSinclair.Net.Core.Z80;

public static class Z80Flags
{
    public const byte C = 0x01, N = 0x02, PV = 0x04, F3 = 0x08,
        H = 0x10, F5 = 0x20, Z = 0x40, S = 0x80;
    // Private arrays exposed as read-only spans so no code can corrupt the shared tables
    // (Specs/spec-correcciones-revision-2.md 4).
    private static readonly byte[] sz53 = new byte[256];
    private static readonly byte[] sz53P = new byte[256];
    private static readonly byte[] parity = new byte[256];
    private static readonly byte[] inc = new byte[256];
    private static readonly byte[] dec = new byte[256];

    public static ReadOnlySpan<byte> SZ53 => sz53;
    public static ReadOnlySpan<byte> SZ53P => sz53P;
    public static ReadOnlySpan<byte> Parity => parity;
    public static ReadOnlySpan<byte> Inc => inc;
    public static ReadOnlySpan<byte> Dec => dec;

    static Z80Flags()
    {
        for (var value = 0; value < 256; value++)
        {
            sz53[value] = (byte)((value & (S | F5 | F3)) | (value == 0 ? Z : 0));
            parity[value] = (byte)((BitOperations.PopCount((uint)value) & 1) == 0 ? PV : 0);
            sz53P[value] = (byte)(sz53[value] | parity[value]);
            inc[value] = (byte)(sz53[value] | ((value & 0x0F) == 0 ? H : 0) | (value == 0x80 ? PV : 0));
            dec[value] = (byte)(sz53[value] | N | ((value & 0x0F) == 0x0F ? H : 0) | (value == 0x7F ? PV : 0));
        }
    }
}
