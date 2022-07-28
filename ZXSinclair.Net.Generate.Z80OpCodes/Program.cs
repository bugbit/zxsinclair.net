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

// Page 90 (LD A, (nn))

using System.Text.RegularExpressions;

const string cArgsRegisterPlusD = "(REGISTER+dd)";

var assembly = Assembly.GetExecutingAssembly();
var pathexe = Path.GetDirectoryName(assembly.Location);
var path = pathexe + "/../../../../ZXSinclair.Net";
//var x = File.ReadAllLines(path + "/Program.cs");
var embeddedProvider = new EmbeddedFileProvider(assembly);
var regs8 = new[] { "A", "B", "C", "D", "E", "H", "L" };
var regs16 = new[] { "BC", "DE", "HL" };
var regs16m = new[] { "(BC)", "(DE)", "(HL)" };
var regs = regs8.Concat(regs16).ToArray();
var rgegs16pipe = string.Join('|', regs16);
var regex = new Regex($@"\(({rgegs16pipe})\)", RegexOptions.Compiled);
var opcodesBase = new Opcodes()
{
    FileDat = "data/opcodes_base.dat",
    FileOpCodesTemplate = "templates/z80Cpu.opcodes.txt",
    FileEnumTemplate = "templates/z80OpCodes.txt",
    FileZ80Enum = "Hardware/Z80/Z80OpCodes.cs",
    FileZ80Opcodes = "Hardware/Z80/Z80Cpu.opcodes.cs",
    EnumName = "Z80OpCodes",
    BuilderBefore = GenerateZ80CpuBefore
};
var opcodesDD = new Opcodes()
{
    FileDat = "data/opcodes_ddfd.dat",
    FileOpCodesTemplate = "templates/z80Cpu.opcodesdd.txt",
    FileEnumTemplate = "templates/z80OpCodesDD.txt",
    FileZ80Enum = "Hardware/Z80/Z80OpCodesDD.cs",
    FileZ80Opcodes = "Hardware/Z80/Z80Cpu.opcodesdd.cs",
    EnumName = "Z80OpCodesDD",
    Register = "IX",
    BuilderBefore = GenerateZ80CpuIXIYBefore
};
var opcodesFD = new Opcodes()
{
    FileDat = "data/opcodes_ddfd.dat",
    FileOpCodesTemplate = "templates/z80Cpu.opcodesfd.txt",
    FileEnumTemplate = "templates/z80OpCodesFD.txt",
    FileZ80Enum = "Hardware/Z80/Z80OpCodesFD.cs",
    FileZ80Opcodes = "Hardware/Z80/Z80Cpu.opcodesfd.cs",
    EnumName = "Z80OpCodesFD",
    Register = "IY",
    BuilderBefore = GenerateZ80CpuIXIYBefore
};

Dictionary<string, Func<OpCodeArgs, StringBuilder, bool>> opcodesGenerators = new Dictionary<string, Func<OpCodeArgs, StringBuilder, bool>>
{
    [nameof(NOP)] = NOP,
    [nameof(LD)] = LD,
    [nameof(shift)] = shift
};

string BuildId(string[] line, Opcodes opcodes)
{
    var id =
        string.Join('_', line.Skip(1)).Replace(" ", "_").Replace(",", "_").Replace("(", "MM_").Replace(")", "_MM").Replace("'", "_")
        .Replace("+", "_PLUS_");

    if (!string.IsNullOrWhiteSpace(opcodes.Register))
        id = id.Replace("REGISTER", opcodes.Register);

    return id;
}

await GenerateZ80RegsLd();

await WriterOpcodes(opcodesBase);
await WriterOpcodes(opcodesDD);
await WriterOpcodes(opcodesFD);

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

async Task WriteCode(string file, string fileTemplate, StringBuilder code, StringBuilder? before = null)
{
    var x = await ReadTxtFileEmb(fileTemplate);

    if (before != null)
        x = x.Replace("{{BEFORE}}", before.ToString());

    x = x.Replace("{{CODE}}", code.ToString());

    await File.WriteAllTextAsync(Path.Combine(path, file), x);
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
    regs8.Select(r => $"public void Set{r}_n(byte n) => {r} = n;").ToList().ForEach(s => str.AppendLine(s));

    await WriteCode("Hardware/Z80/Z80Regs.ld.cs", "templates/z80regs_ld.txt", str);
}

void GenerateZ80CpuBefore(StringBuilder str, Opcodes opcodes)
{
    (
        from r16 in regs16
        from r8 in regs8
        select
$@"
    public void LD_M_{r16}_M_{r8}()
    {{
        var r = Regs;

        WriteMemory(r.{r16}, r.{r8});
    }}"
    ).ToList().ForEach(s => str.AppendLine(s));
}

void GenerateZ80CpuIXIYBefore(StringBuilder str, Opcodes opcodes)
{
    (
        from r8 in regs8
        select
$@"
    public void LD_M_{opcodes.Register}_PLUS_D_M_{r8}()
    {{
        var r = Regs;
        var d = (sbyte)ReadMemory(Regs.GetPCAndInc());

        Ticks.AddCycles(5);

        var nn =r.Get{opcodes.Register}_d(d);

        WriteMemory(nn, r.{r8});
    }}"
    ).ToList().ForEach(s => str.AppendLine(s));
}

OpCodeArgs BuildArgs(string line, Opcodes opcodes)
{
    var pSplit = line.Split(' ');
    var pArgs = new OpCodeArgs
    {
        Id = BuildId(pSplit, opcodes),
        HexCode = pSplit[0],
        OpCode = (pSplit.Length > 1) ? pSplit[1] : null,
        Params = (pSplit.Length > 2) ? pSplit[2].Split(',') : new string[0],
        Register = opcodes.Register
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
    Console.WriteLine($"reading {opcodes.FileDat} ...");

    var t = opcodes.BuilderBefore != null
        ? Task<StringBuilder?>.Factory.StartNew(() => { var lines = new StringBuilder(); opcodes.BuilderBefore(lines, opcodes); return lines; })
        : Task.FromResult<StringBuilder?>(null); //new StringBuilder();    

    using (var stream = embeddedProvider.GetFileInfo(opcodes.FileDat).CreateReadStream())
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

                var args = BuildArgs(line, opcodes);
                bool ok = false;
                string? msgerr = null;

                Console.Write($"\nGenerate {line} ");
                enums.AppendLine($"\t{args.Id} = {args.HexCode},");
                lines.AppendLine($"\t\t\t// {line}");
                lines.AppendLine($"\t\t\tcase (byte){opcodes.EnumName}.{args.Id}:");
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
                        lines.AppendLine("\t\t\tbreak;");
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

            var before = await t;

            await WriteCode(opcodes.FileZ80Opcodes, opcodes.FileOpCodesTemplate, lines, before);
        }
    }
}

bool NOP(OpCodeArgs args, StringBuilder lines)
{
    lines.AppendLine($"\t\t\tNop();");

    return true;
}

bool LD(OpCodeArgs args, StringBuilder lines)
{
    if (args.Params.Length != 2)
        return false;

    var p1 = args.Params[0];
    var p2 = args.Params[1];
    Match m;

    if (p1 == p2)
        return true;

    if (regs8.Contains(p1))
    {
        // LD r, r'
        if (regs8.Contains(p2))
        {
            lines.AppendLine($"\t\t\tRegs.Set{p1}_{p2}();");

            return true;
        }

        //LD r,n
        if (p2 == "nn")
        {
            lines.AppendLine($"\t\t\tRegs.Set{p1}_n(ReadMemory(Regs.GetPCAndInc()));");

            return true;
        }

        //LD r, (HL)
        //LD A, (BC)
        //LD A, (DE)
        if (regs16m.Contains(p2))
        {
            var p22 = p2.Replace("(", "").Replace(")", "");

            lines.AppendLine($"\t\t\tRegs.Set{p1}_n(Read_M_{p22}_M());");

            return true;
        }

        // LD r, (IX+d)
        // LD r, (IY+d)
        if (p2 == cArgsRegisterPlusD)
        {
            lines.AppendLine($"\t\t\tRegs.Set{p1}_n(Read_M_{args.Register}_PLUS_D_M());");

            return true;
        }
    }
    else if ((m = regex.Match(p1)).Success)
    {
        // LD (HL), r
        if (regs8.Contains(p2))
        {
            lines.AppendLine($"\t\t\tLD_M_{m.Groups[1].Value}_M_{p2}();");

            return true;
        }
    }
    else if (p1 == cArgsRegisterPlusD)
    {
        // LD (IX+d), n
        // LD (IY+d), n
        if (regs8.Contains(p2))
        {
            lines.AppendLine($"\t\t\tLD_M_{args.Register}_PLUS_D_M_{p2}();");

            return true;
        }
    }

    return false;
}

bool shift(OpCodeArgs args, StringBuilder lines)
{
    if (args.Params.Length == 1)
    {
        if (args.Params[0] == "DD")
        {
            lines.AppendLine($"\t\t\tInstrfetchDD();");

            return true;
        }
        if (args.Params[0] == "FD")
        {
            lines.AppendLine($"\t\t\tInstrfetchFD();");

            return true;
        }
    }
    return false;
}