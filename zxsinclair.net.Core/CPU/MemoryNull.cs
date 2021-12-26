using System;
using System.Collections.Generic;
using System.Text;

namespace ZXSinclair.CPU
{
    public class MemoryNull<A, D> : IMemory<A, D>
    {
        private static readonly Lazy<MemoryNull<A, D>> mInstance = new Lazy<MemoryNull<A, D>>(() => new MemoryNull<A, D>());

        private MemoryNull() { }

        public static IMemory<A, D> Instance => mInstance.Value;

        public D ReadMemory(A argMemory) => default(D);

        public void WriteMemory(A argMemory, D argData) { }

        void IDisposable.Dispose()
        {
        }
    }
}
