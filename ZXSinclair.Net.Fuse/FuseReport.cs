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

using System.Text;
using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Fuse;

public sealed class FuseReport
{
    public IReadOnlyList<FuseMismatch> Mismatches { get; }
    public bool Failed => Mismatches.Count != 0;

    public FuseReport(clsTestExpected expected, in Z80Registers actual, int cycles,
        byte[] initialMemory, Func<ushort, byte> read, IReadOnlyList<Z80BusEvent>? events, int maxMemoryRanges = 16)
    {
        var differences = new List<FuseMismatch>();
        differences.AddRange(FuseCpuState.Diff(expected.Base, in actual, cycles));
        differences.AddRange(FuseComparison.DiffMemory(FuseComparison.ExpectedMemory(initialMemory, expected), read, maxMemoryRanges));
        if (events is not null) differences.AddRange(FuseComparison.DiffEvents(expected, events));
        Mismatches = differences;
    }

    public string Format(clsTestIn input, clsTestExpected expected, byte[] initialMemory,
        IReadOnlyList<Z80BusEvent>? events, bool verbose = false)
    {
        var result = new StringBuilder();
        result.AppendLine($"FAIL {input.Base.Name}");
        var pc = input.Base.Line1.pc;
        var bytes = Enumerable.Range(0, 4).Select(i => initialMemory[(ushort)(pc + i)].ToString("x2"));
        result.AppendLine($"Programa en PC={pc:x4}: {string.Join(" ", bytes)}");
        var r = input.Base.Line1;
        var s = input.Base.Line2;
        result.AppendLine("Estado inicial (af bc de hl af' bc' de' hl' ix iy sp pc):");
        result.AppendLine($"{r.af:x4} {r.bc:x4} {r.de:x4} {r.hl:x4} {r.af_:x4} {r.bc_:x4} {r.de_:x4} {r.hl_:x4} {r.ix:x4} {r.iy:x4} {r.sp:x4} {r.pc:x4}");
        result.AppendLine("i r iff1 iff2 im halted end_tstates:");
        result.AppendLine($"{s.i:x2} {s.r:x2} {s.iff1} {s.iff2} {s.im} {s.halted} {s.endtstates}");
        Section("Registros", m => m.Kind is FuseMismatchKind.Register or FuseMismatchKind.State);
        Section("Flags", m => m.Kind == FuseMismatchKind.Flags);
        Section("Memoria", m => m.Kind == FuseMismatchKind.Memory);
        Section("Eventos", m => m.Kind == FuseMismatchKind.Event, events is null ? "no comparados (--no-events)" : null);
        Section("T-states", m => m.Kind == FuseMismatchKind.TStates);
        if (verbose && events is not null)
        {
            result.AppendLine("Eventos completos, esperado | real:");
            for (var i = 0; i < Math.Max(expected.Events.Length, events.Count); i++)
            {
                var left = i < expected.Events.Length ? expected.Events[i].ToBusEvent().ToString() : "<missing>";
                var right = i < events.Count ? events[i].ToString() : "<missing>";
                result.AppendLine($"{i,5} | {left,-24} | {right}");
            }
        }
        return result.ToString().TrimEnd();

        void Section(string title, Func<FuseMismatch, bool> predicate, string? absent = null)
        {
            result.AppendLine(title + ":");
            var selected = Mismatches.Where(predicate).ToArray();
            if (selected.Length == 0) result.AppendLine("  " + (absent ?? "sin diferencias"));
            foreach (var mismatch in selected)
            {
                foreach (var line in mismatch.ToString().Split(Environment.NewLine))
                    result.AppendLine("  " + line);
            }
        }
    }

    public string Summary(string name) => $"FAIL {name}: " + string.Join(", ",
        Mismatches.Select(m => m.Kind is FuseMismatchKind.State ? FuseMismatchKind.Register : m.Kind).Distinct());
}
