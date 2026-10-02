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

var assembly = Assembly.GetExecutingAssembly();
var embeddedProvider = new EmbeddedFileProvider(assembly);

if (!TestInstrumentationEnabled())
{
    Console.Error.WriteLine("Run the FUSE tests in Debug configuration; Release omits test instrumentation and parser assertions.");
    Environment.ExitCode = 2;
    return;
}

if (args.Any(arg => arg != "--no-events"))
{
    Console.Error.WriteLine("Usage: ZXSinclair.Net.Test [--no-events]");
    Environment.ExitCode = 2;
    return;
}

var compareEvents = !args.Contains("--no-events");
var endianPassed = endiantest();
await z80opcodestest();
if (!endianPassed)
    Environment.ExitCode = 1;

async Task<string[]> ReadLinesTxtFileEmb(string key)
{
    using (var stream = embeddedProvider.GetFileInfo(key).CreateReadStream())
    {
        using (var reader = new StreamReader(stream))
        {
            var lines = new List<string>();
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
                lines.Add(line);

            return lines.ToArray();
        }
    }
}

bool endiantest()
{
    var regs = new Z80Regs { A = 0x40, F = 0x1f, B = 0x40, C = 0x1f, D = 0x40, E = 0x1f, H = 0x40, L = 0x1f };
    var passed = regs.AF == 0x401f && regs.BC == 0x401f && regs.DE == 0x401f && regs.HL == 0x401f;
    if (!passed)
        Console.Error.WriteLine("FAIL endiantest: register byte aliases differ from 401f.");
    return passed;
}

async Task z80opcodestest()
{
    Console.WriteLine("z80opcodestest");

    var testsin = await readTestsIn();
    var testsexpected = await readTestsExpected();

    RunTests(testsin, testsexpected);
}

async Task<List<clsTestIn>> readTestsIn()
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

async Task<IDictionary<string, clsTestExpected>> readTestsExpected()
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

unsafe void RunTests(List<clsTestIn> testsin, IDictionary<string, clsTestExpected> testsexpected)
{
    using var mb = new MemoryBuffer8Bit(0x10000);
    var m0 = new byte[mb.Size];
    var m = new MemoryRam16Bits<byte>(mb);
    using var z80 = new Z80Cpu(mb, m);
#if Z80_OPCODES_TEST
    z80.BusEvents = compareEvents ? new List<Z80BusEvent>(64) : null;
#endif
    var passed = 0;
    var failed = 0;
    var skipped = 0;

    foreach (var t in testsin)
    {
        PrepareTestCpu(z80, t);
        mb.CopyTo(m0);
        do
        {
            z80.Instrfetch();
#if Z80_OPCODES_TEST
            if (z80.instrNotImp)
                break;
#endif
        } while (z80.Ticks.TStates < t.Base.Line2.endtstates);
#if Z80_OPCODES_TEST
        if (z80.instrNotImp)
        {
            skipped++;
            continue;
        }
#endif
        var error = testsexpected.TryGetValue(t.Base.Name, out var expected)
            ? CompareTest(z80, m0, expected)
            : "missing expected result";
        if (error is null)
            passed++;
        else
        {
            failed++;
            Console.Error.WriteLine($"FAIL {t.Base.Name}: {error}");
        }
    }

    Console.WriteLine($"FUSE: {passed} passed / {failed} failed / {skipped} skipped (unimplemented); bus events {(compareEvents ? "enabled" : "disabled")}");
    if (failed != 0)
        Environment.ExitCode = 1;
}

void PrepareTestCpu(Z80Cpu cpu, clsTestIn t)
{
    var r = cpu.Regs;
    var m = cpu.MemoryBuffer;

    cpu.Reset();
    r.SetAF_nn(t.Base.Line1.af);
    r.SetBC_nn(t.Base.Line1.bc);
    r.SetDE_nn(t.Base.Line1.de);
    r.SetHL_nn(t.Base.Line1.hl);
    r.SetIX_nn(t.Base.Line1.ix);
    r.SetIY_nn(t.Base.Line1.iy);
    r.SetSP_nn(t.Base.Line1.sp);
    r.SetPC_nn(t.Base.Line1.pc);
    r.SetI_n((byte)t.Base.Line2.i);
    r.SetR_n((byte)t.Base.Line2.r);

    for (var i = 0; i < 0x10000;)
    {
        m.Write((ushort)i, 0xde);
        m.Write((ushort)i++, 0xad);
        m.Write((ushort)i++, 0xbe);
        m.Write((ushort)i++, 0xef);

    }
    foreach (var mm in t.Base.Memories)
    {
        var a = mm.Address;

        foreach (var d in mm.Data)
            m.Write(a++, d);
    }
}

string? CompareTest(Z80Cpu cpu, byte[] m0, clsTestExpected t)
{
    var r = cpu.Regs;
    var l1 = t.Base.Line1;
    var l2 = t.Base.Line2;
    var registers = new (string Name, ushort Actual, ushort Expected)[]
    {
        ("AF", r.AF, l1.af), ("BC", r.BC, l1.bc), ("DE", r.DE, l1.de), ("HL", r.HL, l1.hl),
        ("IX", r.IX, l1.ix), ("IY", r.IY, l1.iy), ("SP", r.SP, l1.sp), ("PC", r.PC, l1.pc),
        ("I", r.I, l2.i), ("R", r.R, l2.r)
    };
    foreach (var (name, actual, expected) in registers)
        if (actual != expected)
            return $"{name}: expected {expected:x4}, actual {actual:x4}";
    if (cpu.Ticks.TStates != l2.endtstates)
        return $"T-states: expected {l2.endtstates}, actual {cpu.Ticks.TStates}";

    var expectedMemory = (byte[])m0.Clone();
    foreach (var block in t.Base.Memories)
    {
        var address = block.Address;
        foreach (var data in block.Data)
            expectedMemory[address++] = data;
    }
    var memory = cpu.MemoryBuffer!;
    for (var address = 0; address < expectedMemory.Length; address++)
    {
        var actual = memory.Read((ushort)address);
        if (actual != expectedMemory[address])
            return $"memory {address:x4}: expected {expectedMemory[address]:x2}, actual {actual:x2}";
    }

#if Z80_OPCODES_TEST
    if (compareEvents)
    {
        var actualEvents = cpu.BusEvents!;
        for (var i = 0; i < Math.Max(t.Events.Length, actualEvents.Count); i++)
        {
            Z80BusEvent? expected = i < t.Events.Length ? t.Events[i].ToBusEvent() : null;
            Z80BusEvent? actual = i < actualEvents.Count ? actualEvents[i] : null;
            if (expected != actual)
                return $"bus event {i}: expected [{expected?.ToString() ?? "<missing>"}], actual [{actual?.ToString() ?? "<missing>"}] (counts {t.Events.Length}/{actualEvents.Count})";
        }
    }
#endif
    return null;
}
static bool TestInstrumentationEnabled()
{
#if Z80_OPCODES_TEST
    return true;
#else
    return false;
#endif
}
