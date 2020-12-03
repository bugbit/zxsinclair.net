using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using static System.Console;

namespace ZXSinclair.Perform
{
    public static class Tests
    {
        static void Main(string[] args) => Run();

        public static void Run()
        {
            ExecTests(5, typeof(CPUPerformTest), nameof(CPUPerformTest.TestExecInstructionMethod));
            WriteLine("Pulse ENTER para finalizar");
            ReadLine();
        }

        private static void ExecTests(int n, Type type, params string[] methods)
        {
            var pActions =
                from m in methods
                select new { m, a = type.GetMethod(m, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) };
            var pObj = Activator.CreateInstance(type);

            foreach (var a in pActions)
            {
                for (var j = 0; j < n; j++)
                {
                    var pTicks1 = Stopwatch.GetTimestamp();

                    a.a.Invoke(pObj, null);

                    var pTicks2 = Stopwatch.GetTimestamp();
                    var pTicks = (int)(pTicks2 - pTicks1);
                    var pElapsedMS = (pTicks) * 1000.0 / Stopwatch.Frequency;

                    WriteLine($"{n} {a.m} {pElapsedMS} ms");
                }
            }
        }
    }
}
