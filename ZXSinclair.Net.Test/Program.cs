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
    var testsin = await readTestsIn();
    var testsexpected = await readTestsExpected();
}

async Task<IDictionary<string, clsTestIn>> readTestsIn()
{
    var lines = await ReadLinesTxtFileEmb("data/tests.in");
    var i = -1;
    var tests = new Dictionary<string, clsTestIn>();

    while (i < lines.Length)
    {
        do
        {
            if (i >= lines.Length)
                return tests;
            i++;
        } while (lines[i].Length == 0);

        var test = new clsTestIn();
        var name = lines[i++];

        tests.Add(name, test);
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
    var i = -1;
    var tests = new Dictionary<string, clsTestExpected>();

    while (i < lines.Length)
    {
        do
        {
            if (i >= lines.Length)
                return tests;
            i++;
        } while (lines[i].Length == 0);

        var test = new clsTestExpected();
        var name = lines[i++];

        tests.Add(name, test);
        test.Base.Name = name;
        test.Events = clsTestEvent.ReadEvents(lines, ref i);
        test.Base.Line1.read(lines[i++]);
        test.Base.Line2.read(lines[i++]);
        test.Base.Memories = clsTestMemory.Read(lines, ref i);
    }

    return tests;
}