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
/// E => Pins (no address)
/// R => Register
/// </summary>
/// <typeparam name="D"></typeparam>
public interface ICpu<A, D, E, R> : IDisposable, IReset where E : Enum where R : IReset
{
    IMemoryBuffer<D>? MemoryBuffer { get; set; }
    IMemory<A, D> Memory { get; set; }
    E Pins { get; set; }
    R Regs { get; }
    ITicks Ticks { get; }
    D ReadOpCode(A address);
    D ReadMemory(A address);
    void WriteMemory(A address, D data);
    void Instrfetch();
    void ExecOpCode(D opcode);
}