generateZ80Ops.Opcodes opcodesBase = new()
{
    FileDat = "opcodes_base.dat",
    FileZ80EnumH = "z80_enum_opcodes.h",
    FileZ80OpcodesH = "z80_opcodes.h"
};

static string BuildId(string line)
    => line.Replace(" ", "_").Replace(",", "_").Replace("(", "MM_").Replace(")", "_MM");

static generateZ80Ops.OpCodeArgs BuildArgs(string line)
{
    var pSplit = line.Split(' ');
    var pArgs = new generateZ80Ops.OpCodeArgs
    {
        Id = BuildId(line),
        HexCode = pSplit[0],
        OpCode = (pSplit.Length > 1) ? pSplit[1] : null,
        Params = (pSplit.Length > 2) ? pSplit[2].Split(',') : new string[0]
    };

    return pArgs;
}

static void RunOpcode(generateZ80Ops.OpCodeArgs args, List<string> lines)
{
    var method = typeof(Program).GetMethod(args.OpCode, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

    if (method != null)
        method.Invoke(null, new object[] { args, lines });
}

static async Task WriterOpcodes(generateZ80Ops.Opcodes opcodes)
{
    var dir = Path.GetDirectoryName(typeof(generateZ80Ops.Opcodes).Assembly.Location);

    Console.Write($"reading {opcodes.FileDat} ...");

    var linesDat = (await File.ReadAllLinesAsync(opcodes.FileDat)).ToList();
    var enums = new List<string>();
    var lines = new List<string>();

    linesDat.RemoveAll(l => l.StartsWith("#") || string.IsNullOrWhiteSpace(l));
    foreach (var line in linesDat)
    {
        var args = BuildArgs(line);

        Console.Write($"\nGenerate {line}");
        enums.Add($"{args.Id} = {args.HexCode}");
        lines.Add($"// {line}");
        lines.Add($"case {args.Id}:");
        if (!string.IsNullOrWhiteSpace(args.OpCode))
        {
            RunOpcode(args, lines);
            lines.Add("break;");
        }
    }

}

static void LD(generateZ80Ops.OpCodeArgs args, List<string> lines)
{
}

await WriterOpcodes(opcodesBase);
