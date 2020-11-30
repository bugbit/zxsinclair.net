using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ZXSinclair.Perform
{
    public static class Tests
    {
        public static Action<string> WriteTest { get; set; }

        public static void Run()
        {
            ExecTests(5, typeof(CPUPerformTest), nameof(CPUPerformTest.TestExecInstructionMethod));
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

                    WriteTest?.Invoke($"{n} {a.m} {pElapsedMS} ms");
                }
            }
        }
    }
}
