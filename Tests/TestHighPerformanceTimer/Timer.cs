using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TestHighPerformanceTimer
{
    interface ITimer
    {
        long Frequency { get; }
        bool IsHighResolution { get; }
        long Ticks { get; }
    }

    abstract class Timer : ITimer
    {
        private const string lib = "kernel32.dll";
        [DllImport(lib)]
        private static extern int QueryPerformanceFrequency(ref Int64 frequency);

        public long Frequency { get; }
        public bool IsHighResolution { get; }
        abstract public long Ticks { get; }

        protected Timer(long argFreg, bool argIsHighRes)
        {
            Frequency = argFreg;
            IsHighResolution = argIsHighRes;
        }

        public static ITimer CreateTimer()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 5)
            {
                var pFreq = CalcQPFrequency();

                return (pFreq != 0 && pFreq != 1000) ? (ITimer)new WinHiResTimer(pFreq, true) : new WinLowResTimer(Stopwatch.Frequency, false);
            }

            return new SysTimer();
        }

        private static long CalcQPFrequency()
        {
            // Query the high-resolution timer only if it is supported.
            // A returned frequency of 1000 typically indicates that it is not
            // supported and is emulated by the OS using the same value that is
            // returned by Environment.TickCount.
            // A return value of 0 indicates that the performance counter is
            // not supported.
            long frequency = 0;
            int returnVal = QueryPerformanceFrequency(ref frequency);

            return (returnVal != 0 && frequency != 100) ? frequency : 0;
        }
    }

    sealed class SysTimer : Timer
    {
        public SysTimer() : base(Stopwatch.Frequency, Stopwatch.IsHighResolution)
        {
        }

        public override long Ticks => Stopwatch.GetTimestamp();
    }

    sealed class WinHiResTimer : Timer
    {
        private const string lib = "kernel32.dll";

        [DllImport(lib)]
        private static extern int QueryPerformanceCounter(ref Int64 count);

        internal WinHiResTimer(long argFreg, bool argIsHighRes) : base(argFreg, argIsHighRes)
        {
        }

        public override long Ticks
        {
            get
            {
                long tickCount = 0;

                QueryPerformanceCounter(ref tickCount);

                return tickCount;
            }
        }


    }

    sealed class WinLowResTimer : Timer
    {
        internal WinLowResTimer(long argFreg, bool argIsHighRes) : base(argFreg, argIsHighRes)
        {
        }

        public override long Ticks => DateTime.UtcNow.Ticks;
    }
}
