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

namespace ZXSinclair.Net.Hardware;

/// <summary>
/// D => Data
/// </summary>
/// <typeparam name="D"></typeparam>
public class Memory<D> : MemoryBase<D>, ICpuCycles
{
    public Memory(IMemoryBuffer<D> buffer, int readCycles, int fetchCycles, int writeCycles) : base(buffer)
    {
        ReadCycles = readCycles;
        FetchCycles = fetchCycles;
        WriteCycles = writeCycles;
    }

    public Memory(IMemoryBuffer<D> buffer, ICpuCycles cycles) : this(buffer, cycles.ReadCycles, cycles.FetchCycles, cycles.WriteCycles) { }

    public int ReadCycles { get; }
    public int FetchCycles { get; }
    public int WriteCycles { get; }

    public override D Read(ITicks ticks, ushort address)
    {
        ticks.AddCycles(ReadCycles);

        return base.Read(ticks, address);
    }

    public override D ReadOpCode(ITicks ticks, ushort address)
    {
        ticks.AddCycles(FetchCycles);

        return base.ReadOpCode(ticks, address);
    }

    public override void Write(ITicks ticks, ushort address, D data)
    {
        ticks.AddCycles(WriteCycles);

        base.Write(ticks, address, data);
    }

}
