﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestHighPerformanceTimer
{
    class Program
    {
        const int num_steps = 100000000;

        static void Main(string[] args)
        {
            //SerialPi();
            //SerialPi();
            //SerialPi();
            var x = HighResolutionDateTime.IsAvailable;

            var pHighResolutionDateTime = HighResolutionDateTime.UtcNow;

            Thread.Sleep(314);

            var pHighResolutionDateTimeDiff = HighResolutionDateTime.UtcNow - pHighResolutionDateTime;

            var pStopwatch = Stopwatch.StartNew();

            Thread.Sleep(314);

            pStopwatch.Stop();

            //SerialPi();
            //var xx = 1;
            //for (var i = 4; i > 0; i--)
            //    xx += i;





            Console.WriteLine("Stopwatch");
            Console.WriteLine("---------------------------------");
            Console.WriteLine($"IsHighResolution:{Stopwatch.IsHighResolution}");
            long frequency = Stopwatch.Frequency;
            Console.WriteLine($"Timer frequency in ticks per second = {frequency}");
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
            Console.WriteLine($"Timer is accurate within {nanosecPerTick} nanoseconds");

            var pElapsedMS = (double)pStopwatch.ElapsedTicks * 1000L / Stopwatch.Frequency;
            var pElapsedNS = (double)pStopwatch.ElapsedTicks * 1000L * 1000L * 1000L / Stopwatch.Frequency;

            Console.WriteLine($"SerialPi elapse ms: {pElapsedMS}");
            Console.WriteLine($"SerialPi elapse ns: {pElapsedNS}");

            //Console.WriteLine($""pCounter1.Elapsed. + ": " + result);
            Console.WriteLine("---------------------------------");
            Console.WriteLine();
            Console.WriteLine("HighResolutionDateTime");
            Console.WriteLine("---------------------------------");
            Console.WriteLine($"SerialPi elapse ms: {pHighResolutionDateTimeDiff.Ticks / (double)TimeSpan.TicksPerMillisecond}");
            Console.WriteLine($"SerialPi elapse ns: {pHighResolutionDateTimeDiff.Ticks / ((double)TimeSpan.TicksPerMillisecond / (1000L * 1000L))}");
            Console.WriteLine("---------------------------------");
            Console.WriteLine();

            var pFreq2 = new HiResTimer();
            var pZX8081Z80Freq = (long)(3.25 * 1000 * 1000);
            var pRes = frequency / (double)pZX8081Z80Freq;

            /*
                3250000
                10000000

                3 ticks -> 1 cycle
             */

            var pCycle = 1;

            while (pCycle <= 30)
            {
                var pTick1 = Stopwatch.GetTimestamp();

                pCycle++;

                var pTick2 = Stopwatch.GetTimestamp();
                var pTicks = pTick2 - pTick1;

                var pTick12 = pFreq2.Value;

                pCycle++;

                var pTick22 = pFreq2.Value;
                var pTicks2 = pTick22 - pTick12;

                var pSW = Stopwatch.StartNew();

                pCycle++;

                pSW.Stop();

                var pTicks3 = pSW.ElapsedTicks;

                var pTick14 = DateTime.UtcNow.Ticks;

                pCycle++;

                var pTick24 = DateTime.UtcNow.Ticks;
                var pTicks4 = pTick24 - pTick14;

                Console.WriteLine($"Stopwatch 1 cycle : {pTicks}");
                Console.WriteLine($"HiResTimer 1 cycle : {pTicks2}");
                Console.WriteLine($"new Stopwatch 1 cycle : {pTicks3}");
                Console.WriteLine($"DateTime 1 cycle : {pTicks4}");
                Console.WriteLine();
            }

            Console.ReadLine();
        }

        /// <summary>Estimates the value of PI using a for loop.</summary>
        static double SerialPi()
        {
            double sum = 0.0;
            double step = 1.0 / (double)num_steps;
            for (int i = 0; i < num_steps; i++)
            {
                double x = (i + 0.5) * step;
                sum = sum + 4.0 / (1.0 + x * x);
            }
            return step * sum;
        }

    }
}