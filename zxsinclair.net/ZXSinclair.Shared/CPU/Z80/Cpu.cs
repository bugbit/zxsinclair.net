﻿#region LICENSE
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
using System.Threading.Tasks;
using System.Text;

namespace ZXSinclair.CPU.Z80
{
    public class Cpu : IDisposable
    {
        protected Regs mRegs = new Regs();
        protected int mTStates;
        protected ESignals mSignals = ESignals.None;
        protected Action[] mInstrucciones;
        private bool disposedValue;

        public Regs Regs { get => mRegs; private set => mRegs = value; }
        public int TStates => mTStates;
        public ESignals Signals { get => mSignals; set => mSignals = value; }
        public IBuses Buses { get; private set; } = new Buses { Memory = MemoryNull<int, byte>.Instance, IOPort = IOPortNull.Instance };

        public Cpu()
        {
            Reset();
            mInstrucciones = new Action[256];

            Parallel.For(0, 256, i => mInstrucciones[i] = NOP);
        }

        public void Reset()
        {
            mRegs.Reset();
            mTStates = 0;
        }

        public void ExecInstruction()
        {
            Signals |= ESignals.M1;
            AddCycles(1);
            Signals |= ESignals.WAIT;
            AddCycles(1);
            Signals &= ~(ESignals.WAIT | ESignals.M1);

            Regs.RefreshR();

            var pInstr = Buses.Memory.ReadMemory(Regs.GetPCAndInc());

            AddCycles(2);

            mInstrucciones[pInstr].Invoke();
        }

        public void NOP() { }       

        protected void AddCycles(int argTStates) => mTStates += argTStates;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminar el estado administrado (objetos administrados)
                    Buses?.Dispose();
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                Buses = null;
                Regs = null;
                mInstrucciones = null;
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
}
