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

namespace ZXSinclair.Net.Core.Machines.Spectrum;

/// <summary>
/// Port 0x7FFD of the 128K and grey +2 (spec, section 4.3). Paging rewrites page table entries and
/// contention flags; memory is never copied.
/// </summary>
public sealed class Spectrum128Paging
{
    public const int RamBankMask = 0x07;
    public const int ShadowScreenBit = 0x08;
    public const int RomSelectBit = 0x10;
    public const int LockBit = 0x20;

    private const int RomPage = 0;
    private const int TopPage = 3;

    private readonly PagedMemory memory;
    private readonly byte[] contendedPages;

    internal Spectrum128Paging(PagedMemory memory, byte[] contendedPages)
    {
        this.memory = memory;
        this.contendedPages = contendedPages;
        Reset();
    }

    /// <summary>Last value accepted by the port.</summary>
    public byte Value { get; private set; }

    public bool Locked => (Value & LockBit) != 0;
    public int RamBank => Value & RamBankMask;
    public int Rom => (Value & RomSelectBit) != 0 ? 1 : 0;

    /// <summary>RAM bank the ULA displays: 5, or 7 with the shadow screen.</summary>
    public int ScreenBank => (Value & ShadowScreenBit) != 0 ? 7 : 5;

    /// <summary>Physical offset of the displayed screen (see <see cref="PagedMemory.ReadPhysical"/>).</summary>
    public int ScreenBase { get; private set; }

    /// <summary>Writes the port. Ignored once the lock bit has been set, until <see cref="Reset"/>.</summary>
    public void Write(byte value)
    {
        if (Locked)
            return;

        Apply(value);
    }

    public void Reset() => Apply(0);

    private void Apply(byte value)
    {
        Value = value;
        memory.MapPage(RomPage, Rom == 0 ? Layouts.Spectrum128Rom0 : Layouts.Spectrum128Rom1);
        memory.MapPage(TopPage, Layouts.Spectrum128RamBank(RamBank));
        contendedPages[TopPage] = (RamBank & 1) != 0 ? SpectrumMachine.Contended : SpectrumMachine.Uncontended;
        ScreenBase = memory.GetRegionBase(Layouts.Spectrum128RamBank(ScreenBank));
    }
}
