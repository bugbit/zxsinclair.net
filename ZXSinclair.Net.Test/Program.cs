var assembly = Assembly.GetExecutingAssembly();
var embeddedProvider = new EmbeddedFileProvider(assembly);

endiantest();
await z80opcodestest();

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

void endiantest()
{
    Console.WriteLine("endiantest");

    var regs = new Z80Regs();
    var w = 0x4000 + 0x1F;
    var h = (byte)0x40;
    var l = (byte)0x1f;

    regs.A = h;
    regs.F = l;

    Debug.Assert(regs.AF == w);

    regs.B = h;
    regs.C = l;

    Debug.Assert(regs.BC == w);

    regs.D = h;
    regs.E = l;

    Debug.Assert(regs.DE == w);

    regs.H = h;
    regs.L = l;

    Debug.Assert(regs.HL == w);

    /* var wb=new[]{ 0x12,0x34 };
    var w = 0x1234;

    regs.A = 0x34;
    regs.F = 0x12;

    Debug.Assert(regs.AF == w); */
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
    var mb = new MemoryBuffer8Bit(0x10000);
    var m0 = new byte[mb.Size];
    var m = new MemoryRam16Bits<byte>(mb);
    var z80 = new Z80Cpu(mb, m);

    foreach (var t in testsin)
    {
        PrepareTestCpu(z80, t);
        mb.CopyTo(m0);
        do
        {
            z80.Instrfetch();
        } while (z80.Ticks.TStates < t.Base.Line2.endtstates);
#if Z80_OPCODES_TEST
        if (z80.instrNotImp)
            continue;
#endif
        Debug.Assert(testsexpected.TryGetValue(t.Base.Name, out var t2));
        CompareTest(z80, m0, t2);
    }
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

void CompareTest(Z80Cpu cpu, byte[] m0, clsTestExpected t)
{
    var r = cpu.Regs;
    var m = cpu.MemoryBuffer;
    var l1 = t.Base.Line1;
    var l2 = t.Base.Line2;

    Debug.Assert(r.AF == l1.af);
    Debug.Assert(r.BC == l1.bc);
    Debug.Assert(r.DE == l1.de);
    Debug.Assert(r.HL == l1.hl);
    Debug.Assert(r.IX == l1.ix);
    Debug.Assert(r.IY == l1.iy);
    Debug.Assert(r.SP == l1.sp);
    Debug.Assert(r.PC == l1.pc);
    Debug.Assert(r.I == l2.i);
    Debug.Assert(r.R == l2.r);
    Debug.Assert(cpu.Ticks.TStates == l2.endtstates);

    var j = 0;
    var me = t.Base.Memories;

    for (var i = 0; i < 0x10000; i++)
    {
        if (m.Read((ushort)i) == m0[i])
            continue;

        Debug.Assert(j < me.Length);

        var mm = me[j++];

        Debug.Assert(mm.Address == i);

        foreach (var d in mm.Data)
        {
            var d1 = m.Read((ushort)i);
            var d2 = m0[i++];

            Debug.Assert(d == d1);
            Debug.Assert(d1 != d2);
        }
    }
}