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

using ZXSinclair.Net.Core.Z80;
using ZXSinclair.Net.Fuse;

// Reporting and filtering live outside the per-instruction execution path.
string? filter = null;
var verbose = false;
var listSkipped = false;
var recordEvents = true;
var maxFailures = 10;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--no-events": recordEvents = false; break;
        case "--verbose": verbose = true; break;
        case "--list-skipped": listSkipped = true; break;
        case "--filter" when i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal):
            filter = args[++i];
            break;
        case "--max-failures" when i + 1 < args.Length && int.TryParse(args[i + 1], out var limit) && limit >= 0:
            maxFailures = limit;
            i++;
            break;
        default:
            Console.Error.WriteLine($"Invalid or incomplete argument: {args[i]}");
            Console.Error.WriteLine("Usage: [--filter <prefix>] [--verbose] [--max-failures <n >= 0>] [--list-skipped] [--no-events]");
            Environment.ExitCode = 2;
            return;
    }
}
var testsIn = FuseTestFile.LoadInputs();
var testsExpected = FuseTestFile.LoadExpected();
var selected = testsIn.Where(t => filter is null || t.Base.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToArray();
Console.WriteLine($"FUSE: {testsIn.Count} tests loaded, {testsExpected.Count} expected results, {testsExpected.Values.Sum(t => t.Events.Length)} bus events.");
if (filter is not null) Console.WriteLine($"FUSE filter '{filter}': {selected.Length} selected.");
var passed = 0;
var failed = 0;
var skipped = 0;
var failuresBySection = new Dictionary<FuseMismatchKind, int>();
var failuresByOpcode = new SortedDictionary<string, int>(StringComparer.Ordinal);
foreach (var test in selected)
{
    if (!testsExpected.TryGetValue(test.Base.Name, out var expected))
    {
        failed++;
        Console.Error.WriteLine($"FAIL {test.Base.Name}: missing expected result");
        Count(FuseMismatchKind.Register, failuresBySection);
        Count(test.Base.Name.Split('_')[0], failuresByOpcode);
        continue;
    }
    var state = new FuseTestBusState { Events = recordEvents ? new() : null };
    for (var address = 0; address < state.Memory.Length; address += 4)
    {
        state.Memory[address] = 0xDE;
        state.Memory[address + 1] = 0xAD;
        state.Memory[address + 2] = 0xBE;
        state.Memory[address + 3] = 0xEF;
    }
    foreach (var block in test.Base.Memories)
    {
        var address = block.Address;
        foreach (var value in block.Data)
            state.Memory[address++] = value;
    }
    var initialMemory = (byte[])state.Memory.Clone();
    var cpu = new Z80Cpu<FuseTestBus>(new(state));
    cpu.Registers = FuseCpuState.Load(test.Base);
    cpu.Execute(test.Base.Line2.endtstates);
    if (cpu.UnimplementedOpcodes != 0)
    {
        skipped++;
        if (listSkipped)
        {
            var end = cpu.LastUnimplementedAddress;
            var bytes = Enumerable.Range(0, 4).Select(i => state.Memory[(ushort)(end - 4 + i)].ToString("x2"));
            Console.WriteLine($"SKIP {test.Base.Name}: opcode {state.Memory[(ushort)(end - 1)]:x2} at {(ushort)(end - 1):x4}; bytes before PC={end:x4}: {string.Join(" ", bytes)} (prefix/opcode context)");
        }
        continue;
    }
    var report = new FuseReport(expected, in cpu.Registers, state.Cycles, initialMemory,
        address => state.Memory[address], state.Events);
    if (!report.Failed)
    {
        passed++;
        continue;
    }
    failed++;
    foreach (var section in report.Mismatches.Select(m => m.Kind == FuseMismatchKind.State ? FuseMismatchKind.Register : m.Kind).Distinct())
        Count(section, failuresBySection);
    Count(initialMemory[test.Base.Line1.pc].ToString("x2"), failuresByOpcode);
    Console.Error.WriteLine(failed <= maxFailures
        ? report.Format(test, expected, initialMemory, state.Events, verbose)
        : report.Summary(test.Base.Name));
}
Console.WriteLine($"FUSE: {passed} passed / {failed} failed / {skipped} skipped");
Console.WriteLine("FUSE failures by section: " + string.Join(", ",
    new[] { FuseMismatchKind.Register, FuseMismatchKind.Flags, FuseMismatchKind.Memory, FuseMismatchKind.Event, FuseMismatchKind.TStates }
        .Select(kind => $"{kind}={failuresBySection.GetValueOrDefault(kind)}")));
Console.WriteLine("FUSE failures by opcode prefix: " + (failuresByOpcode.Count == 0 ? "none" :
    string.Join(", ", failuresByOpcode.Select(pair => $"{pair.Key}={pair.Value}"))));
if (failed != 0) Environment.ExitCode = 1;

static void Count<TKey>(TKey key, IDictionary<TKey, int> counts) where TKey : notnull =>
    counts[key] = counts.TryGetValue(key, out var value) ? value + 1 : 1;
