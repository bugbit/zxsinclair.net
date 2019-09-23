using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHighPerformanceTimer
{
    sealed class Z80
    {
        private IClock mClock;
        private byte[] mMemory = new byte[32768];
        private Action[] mInstrs = new Action[256];
        private UInt16 mPC = 0;

        public Z80(IClock argClock)
        {
            mClock = argClock;
            for (var i = 0; i < 256;)
                mInstrs[i++] = NOP;

            for (var i = 0; i < mMemory.Length; i++)
                mMemory[i] = (byte)(i % 255);
        }

        public void Reset() => mPC = 0;

        public void Execute()
        {
            var pByte = mMemory[mPC++];

            switch (pByte)
            {
                case 0:
                    mClock.AddCycle(2);
                    break;
                case 1:
                    mClock.AddCycle(2);
                    break;
                case 2:
                    mClock.AddCycle(2);
                    break;
                case 3:
                    mClock.AddCycle(2);
                    break;
                case 4:
                    mClock.AddCycle(2);
                    break;
                case 5:
                    mClock.AddCycle(2);
                    break;
                case 6:
                    mClock.AddCycle(2);
                    break;
                case 7:
                    mClock.AddCycle(2);
                    break;
                case 8:
                    mClock.AddCycle(2);
                    break;
                case 9:
                    mClock.AddCycle(2);
                    break;
                case 10:
                    mClock.AddCycle(2);
                    break;
                default:
                    mClock.AddCycle(2);
                    break;
            }
        }

        public void Execute2()
        {
            var pInstr = mInstrs[mMemory[mPC++]];

            mClock.AddCycle(2);
            pInstr.Invoke();
        }

        private void NOP() => mClock.AddCycle(2);
        private void Instr1() => mClock.AddCycle(2);
        private void Instr2() => mClock.AddCycle(2);
        private void Instr3() => mClock.AddCycle(2);
        private void Instr4() => mClock.AddCycle(2);
        private void Instr5() => mClock.AddCycle(2);
        private void Instr12() => mClock.AddCycle(2);
        private void Instr22() => mClock.AddCycle(2);
        private void Instr32() => mClock.AddCycle(2);
        private void Instr42() => mClock.AddCycle(2);
        private void Instr52() => mClock.AddCycle(2);
    }
}
