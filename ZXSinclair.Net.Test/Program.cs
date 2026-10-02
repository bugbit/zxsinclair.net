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

// FUSE Z80 test runner (Debug or Release). Cases with unimplemented instructions are counted as skipped.

var testsIn = FuseTestFile.LoadInputs();
var testsExpected = FuseTestFile.LoadExpected();
var missing = testsIn.Where(t => !testsExpected.ContainsKey(t.Base.Name)).Select(t => t.Base.Name).ToList();

foreach (var name in missing)
    Console.Error.WriteLine($"FAIL {name}: missing expected result");

Console.WriteLine($"FUSE: {testsIn.Count} tests loaded, {testsExpected.Count} expected results, {testsExpected.Values.Sum(t => t.Events.Length)} bus events.");
var passed = 0;
var failed = missing.Count;
var skipped = 0;
var recordEvents = !args.Contains("--no-events");
foreach (var test in testsIn)
{
    if (!testsExpected.TryGetValue(test.Base.Name, out var expected))
        continue;
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
        continue;
    }
    var error = FuseCpuState.Compare(expected.Base, in cpu.Registers, state.Cycles)
        ?? FuseComparison.CompareMemory(FuseComparison.ExpectedMemory(initialMemory, expected),
            address => state.Memory[address]);
    if (error is null && recordEvents)
        error = FuseComparison.CompareEvents(expected, state.Events!);
    if (error is null)
        passed++;
    else
    {
        failed++;
        Console.Error.WriteLine($"FAIL {test.Base.Name}: {error}");
    }
}
Console.WriteLine($"FUSE: {passed} passed / {failed} failed / {skipped} skipped");
if (failed != 0)
    Environment.ExitCode = 1;
