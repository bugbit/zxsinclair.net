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

using ZXSinclair.Net.Core.Memory;

namespace ZXSinclair.Net.Core.Machines;

/// <summary>Memory maps of the supported machines (Specs/spec-buses-memoria.md, section 4).</summary>
public static class Layouts
{
    /// <summary>Spectrum 16K/48K: region 0 = ROM, region 1 = RAM.</summary>
    public const int SpectrumRom = 0;
    public const int SpectrumRam = 1;

    /// <summary>Spectrum 128K/+2: regions 0-1 = ROM 0-1, regions 2-9 = RAM banks 0-7.</summary>
    public const int Spectrum128Rom0 = 0;
    public const int Spectrum128Rom1 = 1;
    public const int Spectrum128FirstRamBank = 2;

    /// <summary>ZX81: region 0 = ROM 8K, region 1 = RAM.</summary>
    public const int Zx81Rom = 0;
    public const int Zx81Ram = 1;

    public const int Spectrum128BankSize = 0x4000;

    public static int Spectrum128RamBank(int bank) => Spectrum128FirstRamBank + (bank & 7);

    /// <summary>16K ROM at 0000-3FFF, 16K RAM at 4000-7FFF, nothing at 8000-FFFF (reads 0xFF).</summary>
    public static MemoryLayout Spectrum16K()
    {
        var layout = new MemoryLayout(14);

        layout.AddRom(0x4000);
        layout.AddRam(0x4000);

        return layout
            .Map(0x0000, 0x4000, SpectrumRom)
            .Map(0x4000, 0x4000, SpectrumRam);
    }

    /// <summary>16K ROM at 0000-3FFF, 48K RAM at 4000-FFFF.</summary>
    public static MemoryLayout Spectrum48K()
    {
        var layout = new MemoryLayout(14);

        layout.AddRom(0x4000);
        layout.AddRam(0xC000);

        return layout
            .Map(0x0000, 0x4000, SpectrumRom)
            .Map(0x4000, 0xC000, SpectrumRam);
    }

    /// <summary>128K and grey +2, power-on state: ROM 0, bank 5, bank 2, bank 0.</summary>
    public static MemoryLayout Spectrum128K()
    {
        var layout = new MemoryLayout(14);

        layout.AddRom(Spectrum128BankSize);
        layout.AddRom(Spectrum128BankSize);
        for (var bank = 0; bank < 8; bank++)
            layout.AddRam(Spectrum128BankSize);

        return layout
            .Map(0x0000, 0x4000, Spectrum128Rom0)
            .Map(0x4000, 0x4000, Spectrum128RamBank(5))
            .Map(0x8000, 0x4000, Spectrum128RamBank(2))
            .Map(0xC000, 0x4000, Spectrum128RamBank(0));
    }

    /// <summary>ZX81 with the internal 1K: ROM mirrored at 0000-3FFF and 8000-BFFF, RAM repeated over 4000-7FFF and C000-FFFF.</summary>
    public static MemoryLayout Zx81_1K() => Zx81(0x0400);

    /// <summary>ZX81 with a 16K RAM pack (internal RAM disabled).</summary>
    public static MemoryLayout Zx81_16K() => Zx81(0x4000);

    private static MemoryLayout Zx81(int ramSize)
    {
        var layout = new MemoryLayout(10);

        layout.AddRom(0x2000);
        layout.AddRam(ramSize);

        return layout
            .Map(0x0000, 0x4000, Zx81Rom)
            .Map(0x4000, 0x4000, Zx81Ram)
            .Map(0x8000, 0x4000, Zx81Rom)
            .Map(0xC000, 0x4000, Zx81Ram);
    }
}
