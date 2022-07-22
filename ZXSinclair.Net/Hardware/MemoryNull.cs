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

public class MemoryNull<D> : IMemory<D>
{
    public MemoryNull(IMemoryBuffer<D> buffer) => Buffer = buffer;

    public IMemoryBuffer<D> Buffer { get; }

    public D Read(ITicks ticks, uint address)
    {
        throw new NotImplementedException();
    }

    public D Read(uint address)
    {
        throw new NotImplementedException();
    }

    public D ReadOpCode(ITicks ticks, uint address)
    {
        throw new NotImplementedException();
    }

    public void Write(ITicks ticks, uint address, D data)
    {
        throw new NotImplementedException();
    }

    public void Write(uint address, D data)
    {
        throw new NotImplementedException();
    }
}
