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

// FUSE Z80 test runner. The Z80 CPU is being rewritten in ZXSinclair.Net.Core (Specs/spec-cpu-z80.md);
// until it exists, this program only loads and validates the FUSE test files.

var assembly = Assembly.GetExecutingAssembly();
var embeddedProvider = new EmbeddedFileProvider(assembly);

if (!DebugBuild())
{
    Console.Error.WriteLine("Run the FUSE tests in Debug configuration; Release omits the parser assertions.");
    Environment.ExitCode = 2;
    return;
}

var testsIn = await ReadTestsIn();
var testsExpected = await ReadTestsExpected();
var missing = testsIn.Where(t => !testsExpected.ContainsKey(t.Base.Name)).Select(t => t.Base.Name).ToList();

foreach (var name in missing)
    Console.Error.WriteLine($"FAIL {name}: missing expected result");

Console.WriteLine($"FUSE: {testsIn.Count} tests loaded, {testsExpected.Count} expected results, {testsExpected.Values.Sum(t => t.Events.Length)} bus events.");
Console.WriteLine("No Z80 CPU in ZXSinclair.Net.Core yet: no test was executed.");
if (missing.Count != 0)
    Environment.ExitCode = 1;

async Task<string[]> ReadLinesTxtFileEmb(string key)
{
    using var stream = embeddedProvider.GetFileInfo(key).CreateReadStream();
    using var reader = new StreamReader(stream);
    var lines = new List<string>();
    string? line;

    while ((line = await reader.ReadLineAsync()) != null)
        lines.Add(line);

    return lines.ToArray();
}

async Task<List<clsTestIn>> ReadTestsIn()
{
    var lines = await ReadLinesTxtFileEmb("data/tests.in");
    var i = 0;
    var tests = new List<clsTestIn>();
    string name;

    while (i < lines.Length)
    {
        do
        {
            if (i >= lines.Length)
                return tests;
            name = lines[i++];
        } while (string.IsNullOrEmpty(name));

        var test = new clsTestIn();

        tests.Add(test);
        test.Base.Name = name;
        test.Base.Line1.read(lines[i++]);
        test.Base.Line2.read(lines[i++]);
        test.Base.Memories = clsTestMemory.Read(lines, ref i);
    }

    return tests;
}

async Task<IDictionary<string, clsTestExpected>> ReadTestsExpected()
{
    var lines = await ReadLinesTxtFileEmb("data/tests.expected");
    var i = 0;
    var tests = new Dictionary<string, clsTestExpected>();
    string name;

    while (i < lines.Length)
    {
        do
        {
            if (i >= lines.Length)
                return tests;
            name = lines[i++];
        } while (string.IsNullOrEmpty(name));

        var test = new clsTestExpected();

        tests.Add(name, test);
        test.Base.Name = name;
        test.Events = clsTestEvent.ReadEvents(lines, ref i);
        test.Base.Line1.read(lines[i++]);
        test.Base.Line2.read(lines[i++]);
        test.Base.Memories = clsTestMemory.Read(lines, ref i);
    }

    return tests;
}

static bool DebugBuild()
{
#if DEBUG
    return true;
#else
    return false;
#endif
}
