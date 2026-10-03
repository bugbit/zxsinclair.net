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

namespace ZXSinclair.Net.Fuse;

public sealed record FuseConvention(byte IgnoredFlags, string Reason);

/// <summary>Explicit fixture limitations; never used by the CPU.</summary>
public static class FuseConventions
{
    public static IReadOnlyDictionary<string, FuseConvention> Cases { get; } =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, FuseConvention>(
            new Dictionary<string, FuseConvention>(StringComparer.Ordinal)
            {
                ["cb46"] = BitHl, ["cb4e"] = BitHl, ["cb56"] = BitHl, ["cb5e"] = BitHl,
                ["cb66"] = BitHl, ["cb6e"] = BitHl, ["cb76"] = BitHl, ["cb7e"] = BitHl,
            });

    private static FuseConvention BitHl => new(0x28,
        "FUSE calcula F5/F3 de BIT n,(HL) con el valor leído, no con MEMPTR");

    public static FuseConvention? ForCase(string name) => Cases.GetValueOrDefault(name);
}
