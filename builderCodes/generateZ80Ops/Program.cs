generateZ80Ops.Opcodes opcodesBase = new()
{
    FileDat = "opcodes_base.dat",
    FileZ80EnumH = "z80_enum_opcodes.h",
    FileZ80OpcodesH = "z80_opcodes.h"
};

string[] regs8Bits = new[] { "A", "B", "C", "D", "E", "H", "L" };

Dictionary<string, string> regsToCPP = regs8Bits.ToDictionary(r => r, r => $"__z{r}__");

Dictionary<string, Func<generateZ80Ops.OpCodeArgs, List<string>, bool>> opcodesGenerators = new Dictionary<string, Func<generateZ80Ops.OpCodeArgs, List<string>, bool>>
{
    [nameof(LD)] = LD
};

string BuildId(string[] line)
    => string.Join('_', line.Skip(1)).Replace(" ", "_").Replace(",", "_").Replace("(", "MM_").Replace(")", "_MM").Replace("'", "_");

generateZ80Ops.OpCodeArgs BuildArgs(string line)
{
    var pSplit = line.Split(' ');
    var pArgs = new generateZ80Ops.OpCodeArgs
    {
        Id = BuildId(pSplit),
        HexCode = pSplit[0],
        OpCode = (pSplit.Length > 1) ? pSplit[1] : null,
        Params = (pSplit.Length > 2) ? pSplit[2].Split(',') : new string[0]
    };

    return pArgs;
}

bool RunOpcode(generateZ80Ops.OpCodeArgs args, List<string> lines)
{
    if (!opcodesGenerators.TryGetValue(args.OpCode, out var generator))
        return false;

    return generator.Invoke(args, lines);
}

async Task WriterOpcodes(generateZ80Ops.Opcodes opcodes)
{
    var dir = Path.GetDirectoryName(typeof(generateZ80Ops.Opcodes).Assembly.Location) ?? "";

    Console.Write($"reading {opcodes.FileDat} ...");

    var linesDat = (await File.ReadAllLinesAsync(opcodes.FileDat)).ToList();
    var enums = new List<string>();
    var lines = new List<string>();

    linesDat.RemoveAll(l => l.StartsWith("#") || string.IsNullOrWhiteSpace(l));
    foreach (var line in linesDat)
    {
        var args = BuildArgs(line);
        bool ok = false;
        string? msgerr = null;

        Console.Write($"\nGenerate {line} ");
        enums.Add($"{args.Id} = {args.HexCode},");
        lines.Add($"// {line}");
        lines.Add($"case {args.Id}:");
        if (!string.IsNullOrWhiteSpace(args.OpCode))
        {
            try
            {
                ok = RunOpcode(args, lines);
                if (!ok)
                {
                    lines.Add("// not implement");
                    lines.Add("#ifdef Z80_OPCODES_TEST");
                    lines.Add("instrNotImp=true;");
                    lines.Add("#endif");
                    msgerr = "not implement";
                }
                lines.Add("break;");
            }
            catch (Exception ex)
            {
                msgerr = ex.Message;
                ok = false;
            }
        }
        else
            ok = true;
        if (ok)
            Console.Write("ok");
        else
        {
            var color = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(msgerr);
            Console.ForegroundColor = color;
        }
    }
    Console.WriteLine();

    var enuml = enums[enums.Count - 1];

    enums[enums.Count - 1] = enuml.Substring(0, enuml.Length - 1);
    Console.Write($"Writing {opcodes.FileZ80EnumH}");
    await File.WriteAllLinesAsync(Path.Combine(dir, opcodes.FileZ80EnumH), enums);
    Console.Write($"\nWriting {opcodes.FileZ80EnumH}");
    await File.WriteAllLinesAsync(Path.Combine(dir, opcodes.FileZ80OpcodesH), lines);
    Console.WriteLine();
}

bool LD(generateZ80Ops.OpCodeArgs args, List<string> lines)
{
    if (args.Params.Length == 2)
    {
        if (regsToCPP.TryGetValue(args.Params[0], out var r1cpp) && regsToCPP.TryGetValue(args.Params[1], out var r2cpp))
        {
            lines.Add($"__zLD_R_R1__({r1cpp},{r2cpp});");

            return true;
        }
    }
    return false;
}

await WriterOpcodes(opcodesBase);
