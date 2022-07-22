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
var pathexe = Path.GetDirectoryName(assembly.Location);
var path = pathexe + "/../../../../ZXSinclair.Net";
//var x = File.ReadAllLines(path + "/Program.cs");
var embeddedProvider = new EmbeddedFileProvider(assembly);
var regs8 = new[] { "A", "B", "C", "D", "E", "H", "L" };
var regs16 = new[] { "BC", "DE", "HL" };
var regs = regs8.Concat(regs16).ToArray();
var opcodesBase = new Opcodes()
{
    FileDat = "data/opcodes_base.dat",
    FileOpCodesTemplate = "templates/z80Cpu.opcodes.txt",
    FileEnumTemplate = "templates/z800OpCodes.txt",
    FileZ80Enum = "Hardware/Z80/Z80OpCodes.cs",
    FileZ80Opcodes = "Hardware/Z80/Z80Cpu.opcodes.cs"
};

Dictionary<string, Func<OpCodeArgs, StringBuilder, bool>> opcodesGenerators = new Dictionary<string, Func<OpCodeArgs, StringBuilder, bool>>
{
    [nameof(LD)] = LD
};

string BuildId(string[] line)
    => string.Join('_', line.Skip(1)).Replace(" ", "_").Replace(",", "_").Replace("(", "MM_").Replace(")", "_MM").Replace("'", "_");

//await GenerateZ80RegsLd();
await WriterOpcodes(opcodesBase);

async Task<string> ReadTxtFileEmb(string key)
{
    using (var stream = embeddedProvider.GetFileInfo(key).CreateReadStream())
    {
        using (var reader = new StreamReader(stream))
        {
            var x = await reader.ReadToEndAsync();

            return x;
        }
    }
}

async Task WriteCode(string file, string fileTemplate, StringBuilder code)
{
    var x = await ReadTxtFileEmb(fileTemplate);
    var str2 = x.Replace("{{CODE}}", code.ToString());

    await File.WriteAllTextAsync(Path.Combine(path, file), str2);
}

async Task GenerateZ80RegsLd()
{
    var str = new StringBuilder();
    var query0 =
    (
        from r1 in regs8
        from r2 in regs8
        where r1 != r2
        select new { r1, r2 }
    ).Concat
    (
        from r1 in regs16
        from r2 in regs16
        where r1 != r2
        select new { r1, r2 }
    );
    var query =
    (
        from q in query0
        select $"\tpublic void Set{q.r1}_{q.r2}() => {q.r1} = {q.r2};"
    ).ToList();
    query.ForEach(s => str.AppendLine(s));

    await WriteCode("Hardware/Z80/Z80Regs.ld.cs", "templates/z80regs_ld.txt", str);
}

OpCodeArgs BuildArgs(string line)
{
    var pSplit = line.Split(' ');
    var pArgs = new OpCodeArgs
    {
        Id = BuildId(pSplit),
        HexCode = pSplit[0],
        OpCode = (pSplit.Length > 1) ? pSplit[1] : null,
        Params = (pSplit.Length > 2) ? pSplit[2].Split(',') : new string[0]
    };

    return pArgs;
}

bool RunOpcode(OpCodeArgs args, StringBuilder lines)
{
    if (!opcodesGenerators.TryGetValue(args.OpCode, out var generator))
        return false;

    return generator.Invoke(args, lines);
}

async Task WriterOpcodes(Opcodes opcodes)
{
    Console.Write($"reading {opcodes.FileDat} ...");

    using (var stream = embeddedProvider.GetFileInfo("data/opcodes_base.dat").CreateReadStream())
    {
        using (var reader = new StreamReader(stream))
        {
            string? line;
            var enums = new StringBuilder();
            var lines = new StringBuilder();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                var args = BuildArgs(line);
                bool ok = false;
                string? msgerr = null;

                Console.Write($"\nGenerate {line} ");
                enums.AppendLine($"\t{args.Id} = {args.HexCode},");
                lines.AppendLine($"\t\t\t// {line}");
                lines.AppendLine($"\t\t\tcase (byte)Z80OpCodes.{args.Id}:");
                if (!string.IsNullOrWhiteSpace(args.OpCode))
                {
                    try
                    {
                        ok = RunOpcode(args, lines);
                        if (!ok)
                        {
                            lines.AppendLine("\t\t\t// not implement");
                            lines.AppendLine("\t\t\t#if Z80_OPCODES_TEST");
                            lines.AppendLine("\t\t\tinstrNotImp=true;");
                            lines.AppendLine("\t\t\t#endif");
                            msgerr = "not implement";
                        }
                        lines.AppendLine("break;");
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

            await WriteCode(opcodes.FileZ80Enum, opcodes.FileEnumTemplate, enums);
            await WriteCode(opcodes.FileZ80Opcodes, opcodes.FileOpCodesTemplate, lines);
        }
    }
}

bool LD(OpCodeArgs args, StringBuilder lines)
{
    return false;
}