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

namespace ZXSinclair.Net.Core.Abstractions;

/// <summary>
/// Block access to a memory address space, for loading and saving (ROMs, snapshots).
/// Not used in the emulation hot path.
/// </summary>
public interface IMemoryBuffer<TAddress, TData>
    where TAddress : struct
    where TData : struct
{
    /// <summary>Size of the address space.</summary>
    int Size { get; }

    /// <summary>Writes <paramref name="data"/> starting at <paramref name="address"/>, following the memory map (read-only areas are not modified).</summary>
    void CopyFrom(TAddress address, ReadOnlySpan<TData> data);

    /// <summary>Reads the address space starting at <paramref name="address"/> into <paramref name="destination"/>.</summary>
    void CopyTo(TAddress address, Span<TData> destination);
}
