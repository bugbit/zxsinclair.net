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
public abstract class Cpu<D, E, R> : ICpu<D, E, R> where E : Enum where R : IReset, new()
{
    private bool disposedValue;

    public IMemoryBuffer<D>? MemoryBuffer { get; set; }
    public IMemory<D> Memory { get; set; }
    public E Pins { get; set; }
    public R Regs { get; } = new R();
    public ITicks Ticks { get; } = new Ticks();

    public Cpu(IMemoryBuffer<D> buffer, IMemory<D>? memory = null)
    {
        MemoryBuffer = buffer;
        Memory = memory ?? new MemoryNull<D>(buffer);
    }

    public virtual D ReadOpCode(ushort address) => Memory.ReadOpCode(address);

    public virtual D ReadMemory(ushort address) => Memory.Read(address);

    public abstract void Instrfetch();

    public abstract void ExecOpCode(D opcode);

    public virtual void Reset()
    {
        Ticks.Reset();
        Regs.Reset();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: eliminar el estado administrado (objetos administrados)
            }

            // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
            MemoryBuffer?.Dispose();
            // TODO: establecer los campos grandes como NULL
            MemoryBuffer = null;
            disposedValue = true;
        }
    }

    // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
    ~Cpu()
    {
        // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
