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
using System.Text;

namespace ZXSinclair.CPU.Z80
{
    public class Cpu : IDisposable
    {
        protected Regs mRegs = new Regs();
        protected int mTStates;
        protected ESignals mSignals = ESignals.None;
        protected IBuses mBuses;
        private bool disposedValue;

        public Regs Regs { get => mRegs; private set => mRegs = value; }
        public int TStates => mTStates;
        public ESignals Signals { get => mSignals; set => mSignals = value; }
        public IBuses Buses { get => mBuses; set => mBuses = value; }

        public void ExecInstruction()
        {
            Signals |= ESignals.M1;
            AddCycles(1);
            Signals |= ESignals.WAIT;
            AddCycles(1);
            Signals &= ~(ESignals.WAIT | ESignals.M1);

            Regs.R++;

            var pInstr = Buses.ReadMemory(Regs.PC);

            AddCycles(2);
        }

        public void Ld_R1_R2(Action<byte> argSet, Func<byte> argGet)
        {
            argSet.Invoke(argGet.Invoke());
            AddCycles(2);
        }
        public void Ld_R1_R2(Action argSetGet)
        {
            argSetGet.Invoke();
            AddCycles(2);
        }
        public void LdA_B_1()
        {
            mRegs.A = mRegs.B;
            AddCycles(2);
        }

        public void LdA_B_2() => Ld_R1_R2(mRegs.CreateSetterA(), mRegs.CreateGetterB());

        public void LdA_B_3() => Ld_R1_R2(() => mRegs.A = mRegs.B);
        public void LdA_B_4()
        {
            mRegs.SetA_B();
            mRegs.PC += 2;
        }
        public void LdA_B_5() => Ld_R1_R2(mRegs.CreateA_B());

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
                disposedValue = true;
                Buses = null;
                Regs = null;
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
