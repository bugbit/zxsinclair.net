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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZXSinclair.Machines
{
    public class Machine : IMachine
    {
        protected IMemory[] mMemories;
        protected Action[] mOpCodes;
        protected int mTStates;
        protected int mTSatesFetchOpCode = 1;
        protected int mTSatesReadMem = 1;
        protected int mTSatesWriteMem = 1;
        protected int mTSatesToFrame = int.MaxValue;
        protected int mTSatesCounterFrame;
        private int mFPS = 50;
        protected SemaphoreSlim mSemFrame = new SemaphoreSlim(0, 1);
        protected SemaphoreSlim mSemUIFrame = new SemaphoreSlim(0, 1);
        protected CancellationTokenSource mFinishToken = new CancellationTokenSource();
        private bool disposedValue;

        public IMemory[] Memories => mMemories;
        public int TStates => mTStates;
        public int FPS => mFPS;

        public Machine()
        {
            mMemories = CreateMemories();
            Reset();
            mOpCodes = new Action[256];

            Parallel.For(0, 256, i => mOpCodes[i] = NOP);
            FillTableOpCodes();
        }

        public virtual void Reset()
        {
            mTStates = 0;
            mTSatesCounterFrame = mTSatesToFrame;
            ResetMemories();
            mSemFrame = new SemaphoreSlim(0, 1);
        }
        public virtual byte PeekByte(int argAddress) => MemoryNull.Instance.ReadMemory(argAddress);

        public virtual void Poke(int argAddress, byte argData) => MemoryNull.Instance.WriteMemory(argAddress, argData);

        public void ExecInstruction()
        {
            var pOpCode = FetchOpCode();

            AddCycles(mTSatesFetchOpCode);

            ExecOpCode(pOpCode);
        }

        public Task Start() => Task.Factory.StartNew(async () => await StartTaskAsync());

        public void StepFrame()
        {
            while (!mFinishToken.IsCancellationRequested && mTSatesCounterFrame > 0)
                ExecInstruction();
            mTSatesCounterFrame += mTSatesToFrame;
        }

        public async Task SignalFrame()
        {
            await mSemFrame.WaitAsync(mFinishToken.Token);
            if (mFinishToken.IsCancellationRequested)
                return;
            mSemUIFrame.Release();
        }

        protected virtual IMemory[] CreateMemories() => new[] { MemoryNull.Instance };

        protected virtual void ResetMemories() { }

        protected virtual byte FetchOpCode() => 0;
        protected virtual void ExecOpCode(int argOpCode) => mOpCodes[argOpCode].Invoke();
        protected void ExecOpCode(Action[] argOpCodes, int argOpCode) => argOpCodes[argOpCode].Invoke();
        protected void ExecInstruction(Action[] argOpCodes)
        {
            var pOpCode = FetchOpCode();

            AddCycles(mTSatesFetchOpCode);

            ExecOpCode(argOpCodes, pOpCode);
        }
        protected byte ReadMemByteNotTStates(int argAddress) => PeekByte(argAddress);
        protected byte ReadMemByte(int argAddress)
        {
            AddCycles(mTSatesReadMem);

            return ReadMemByteNotTStates(argAddress);
        }
        protected void PokeMemByte(int argAddress, byte argData)
        {
            AddCycles(mTSatesWriteMem);

            Poke(argAddress, argData);
        }
        protected void NOP() { }

        protected void FillTableOpCodes(IDictionary<byte, Action> argOpCodes)
        {
            FillTableOpCodes(mOpCodes, argOpCodes);
        }

        protected void FillTableOpCodes(Action[] argOpCodesArr, IDictionary<byte, Action> argOpCodes)
        {
            Parallel.ForEach(argOpCodes, kv => argOpCodesArr[kv.Key] = kv.Value);
        }

        protected virtual void FillTableOpCodes() { }

        protected void AddCycles(int argTStates)
        {
            mTStates += argTStates;
            mTSatesCounterFrame -= argTStates;
        }

        protected async Task StartTaskAsync()
        {
            while (!mFinishToken.IsCancellationRequested)
            {
                StepFrame();
                if (mFinishToken.IsCancellationRequested)
                    return;
                mSemFrame.Release();
                await mSemUIFrame.WaitAsync(mFinishToken.Token);
                if (mFinishToken.IsCancellationRequested)
                    return;
            }
        }

        protected void Step() => ExecInstruction();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminar el estado administrado (objetos administrados)
                    mFinishToken.Cancel();
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                if (mMemories != null)
                {
                    for (var i = 0; i < mMemories.Length; i++)
                        mMemories[i] = null;
                    mMemories = null;
                }
                disposedValue = true;
            }
        }

        // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        // ~Machine()
        // {
        //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
