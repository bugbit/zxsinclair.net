using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHighPerformanceTimer
{
    interface IClock
    {
        int Cycle { get; }

        void Reset();
        void AddCycle(int argCycles);
    }

    class Clock : IClock
    {
        protected ITimer mTimer;

        public Clock(ITimer argTimer) => mTimer = argTimer;

        public int Cycle { get; private set; }

        virtual public void Reset() => Cycle = 0;

        virtual public void AddCycle(int argCycles) => Cycle += argCycles;
    }
}
