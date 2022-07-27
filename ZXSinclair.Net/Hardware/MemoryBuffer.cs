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

using System.Runtime.InteropServices;

namespace ZXSinclair.Net.Hardware
{
    public unsafe class MemoryBuffer : IMemoryBuffer
    {
        private bool disposedValue;

        public MemoryBuffer(int size = 0, byte* ptr = null, int offset = 0)
        {
            if (size < 0)
                throw new InvalidOperationException();

            Offset = offset;
            Size = size;
            if (ptr != null)
            {
                Buffer = IntPtr.Zero;
                BufferPtr = ptr;
            }
            else if (size > 0)
            {
                Buffer = Marshal.AllocHGlobal(size);
                if (Buffer == IntPtr.Zero)
                    throw new OutOfMemoryException();
                BufferPtr = (byte*)Buffer.ToPointer();
            }
            else
            {
                Buffer = IntPtr.Zero;
                BufferPtr = null;
            }
        }

        public int Offset { get; }
        public int Size { get; }
        public IntPtr Buffer { get; private set; }
        public byte* BufferPtr { get; protected set; }

        public MemoryBuffer New(int offset, int size)
        {
            if (offset > Size || offset + size > Size)
                new MemoryBuffer(0);

            return new MemoryBuffer(size, BufferPtr + size, offset);
        }

        public int CopyTo(byte[] b)
        {
            Debug.Assert(b != null);
            var l = Math.Min(Size, b.Length);

            Marshal.Copy(Buffer, b, 0, l);

            return l;
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
                if (Buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Buffer);
                    Buffer = IntPtr.Zero;
                }
                // TODO: establecer los campos grandes como NULL
                BufferPtr = null;
                disposedValue = true;
            }
        }

        // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        ~MemoryBuffer()
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

    public unsafe class MemoryBuffer8Bit : MemoryBuffer, IMemoryBuffer<byte>
    {
        public MemoryBuffer8Bit(int size = 0, byte* ptr = null) : base(size, ptr)
        {
        }

        public byte Read(int address)
        {
            if (address >= Size)
                return 0;

            return *(BufferPtr + address);
        }

        public void Write(int address, byte data)
        {
            if (address >= Size)
                return;

            *(BufferPtr + address) = data;
        }

        // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        ~MemoryBuffer8Bit()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: false);
        }
    }
}