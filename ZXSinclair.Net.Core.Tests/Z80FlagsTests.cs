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

using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Core.Tests;

public class Z80FlagsTests
{
    [Fact]
    public void TablesMatchEveryByte()
    {
        Assert.Equal(256, Z80Flags.SZ53.Length);
        Assert.Equal(256, Z80Flags.SZ53P.Length);
        Assert.Equal(256, Z80Flags.Parity.Length);
        for (var value = 0; value < 256; value++)
        {
            var bits = 0;
            for (var bit = 0; bit < 8; bit++)
                bits += (value >> bit) & 1;
            var sz53 = (value & 0xA8) | (value == 0 ? 0x40 : 0);
            var parity = (bits % 2) == 0 ? 4 : 0;
            Assert.Equal(sz53, Z80Flags.SZ53[value]);
            Assert.Equal(parity, Z80Flags.Parity[value]);
            Assert.Equal(sz53 | parity, Z80Flags.SZ53P[value]);
        }
        Assert.Equal(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 },
            new[] { Z80Flags.C, Z80Flags.N, Z80Flags.PV, Z80Flags.F3,
                Z80Flags.H, Z80Flags.F5, Z80Flags.Z, Z80Flags.S });
    }

    [Fact]
    public void Tables_AreReadOnly()
    {
        // Shared lookup tables must not be writable from outside (Specs/spec-correcciones-revision-2.md 4).
        var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
        Assert.DoesNotContain(typeof(Z80Flags).GetFields(flags), field => field.FieldType.IsArray);
        foreach (var name in new[] { "SZ53", "SZ53P", "Parity", "Inc", "Dec" })
        {
            var property = typeof(Z80Flags).GetProperty(name, flags);
            Assert.NotNull(property);
            Assert.Equal(typeof(ReadOnlySpan<byte>), property!.PropertyType);
            Assert.Null(property.SetMethod);
        }
    }
}
