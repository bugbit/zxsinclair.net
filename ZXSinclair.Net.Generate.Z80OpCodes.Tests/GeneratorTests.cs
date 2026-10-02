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

using System.Text;
using ZXSinclair.Net.Generate.Z80OpCodes.Emit;
using ZXSinclair.Net.Generate.Z80OpCodes.Model;
using ZXSinclair.Net.Generate.Z80OpCodes.Patterns;
using ZXSinclair.Net.Generate.Z80OpCodes.Tables;

namespace ZXSinclair.Net.Generate.Z80OpCodes.Tests;

public class GeneratorTests
{
    private static IReadOnlyDictionary<OpcodeTableKind, Opcode[]> Tables() => OpcodeTableReader.ReadAll();
    private static GeneratedDispatch[] Generate() => DispatchEmitter.Generate(Tables(), PatternCatalog.Default);

    [Fact]
    public void Parser_ReadsAllTables()
    {
        var tables = Tables();
        Assert.Equal(5, tables.Count);
        foreach (var (kind, entries) in tables)
        {
            Assert.Equal(256, entries.Length);
            Assert.Equal(Enumerable.Range(0, 256).Select(i => (byte)i), entries.Select(o => o.Byte));
            Assert.All(entries, o => Assert.Equal(kind, o.Table));
        }
        foreach (var kind in new[] { OpcodeTableKind.Base, OpcodeTableKind.CB, OpcodeTableKind.DDFDCB })
            Assert.DoesNotContain(tables[kind], o => o.Kind is OpcodeKind.Absent or OpcodeKind.Hole);
        Assert.Equal(178, tables[OpcodeTableKind.ED].Count(o => o.Kind == OpcodeKind.Hole));
    }

    [Fact]
    public void Parser_ResolvesAliases()
    {
        var tables = Tables();
        foreach (var value in new[] { 0x44, 0x4C, 0x54, 0x5C, 0x64, 0x6C, 0x74, 0x7C })
        {
            var opcode = tables[OpcodeTableKind.ED][value];
            Assert.Equal("NEG", opcode.Mnemonic);
            Assert.Empty(opcode.Operands);
            Assert.Equal(value == 0x7C ? OpcodeKind.Instruction : OpcodeKind.Alias, opcode.Kind);
        }
        for (var value = 0x40; value <= 0x47; value++)
        {
            var opcode = tables[OpcodeTableKind.DDFDCB][value];
            Assert.Equal("BIT", opcode.Mnemonic);
            Assert.Equal(new[] { new Operand(OperandKind.Bit, "0", 0),
                new Operand(OperandKind.IndexedMemory, "(REGISTER+dd)") }, opcode.Operands);
            Assert.Equal(value == 0x47 ? OpcodeKind.Instruction : OpcodeKind.Alias, opcode.Kind);
        }
        var aliases = OpcodeTableReader.Read(new StringReader("0x44\n0x4c\n0x7c NEG"), OpcodeTableKind.ED, "aliases.dat");
        Assert.Equal(new SourceLocation("aliases.dat", 1), aliases[0x44].Source);
        Assert.Equal(new SourceLocation("aliases.dat", 2), aliases[0x4C].Source);
    }

    [Fact]
    public void Parser_MarksPrefixes()
    {
        var tables = Tables();
        foreach (var value in new[] { 0xCB, 0xDD, 0xED, 0xFD })
            Assert.Equal(OpcodeKind.Prefix, tables[OpcodeTableKind.Base][value].Kind);
        Assert.Equal(OpcodeKind.Prefix, tables[OpcodeTableKind.DDFD][0xCB].Kind);
        Assert.Equal(OperandKind.PrefixTable, tables[OpcodeTableKind.DDFD][0xCB].Operands[0].Kind);
    }

    [Fact]
    public void Parser_TreatsSlttrapAsEdHole()
    {
        var ed = Tables()[OpcodeTableKind.ED];
        Assert.Equal(OpcodeKind.Hole, ed[0xFB].Kind);
        Assert.Equal("slttrap", ed[0xFB].Mnemonic);
        Assert.Equal(OpcodeKind.Hole, ed[0].Kind);
        Assert.NotNull(ed[0xFB].Source);
    }

    [Theory]
    [InlineData("0x00 NOP\n0x00 NOP", 2)]
    [InlineData("0x44", 1)]
    [InlineData("0x100 NOP", 1)]
    [InlineData("0xGG NOP", 1)]
    [InlineData("0x01 LD B,unknown", 1)]
    [InlineData("0x01 LD B,", 1)]
    [InlineData("0x01 LD ,B", 1)]
    [InlineData("0x01 RST A", 1)]
    [InlineData("0x01 RST 09", 1)]
    [InlineData("0x01 IM A", 1)]
    [InlineData("0x01 IM 3", 1)]
    [InlineData("0x01 BIT C,(HL)", 1)]
    [InlineData("0x01 BIT 8,(HL)", 1)]
    public void Parser_RejectsMalformedLines(string text, int line)
    {
        var error = Assert.Throws<GeneratorException>(() =>
            OpcodeTableReader.Read(new StringReader(text), OpcodeTableKind.ED, "bad.dat"));
        Assert.StartsWith($"bad.dat:{line}:", error.Message);
    }

    [Fact]
    public void Parser_RejectsIncompleteRequiredTables()
    {
        foreach (var table in new[] { OpcodeTableKind.Base, OpcodeTableKind.CB, OpcodeTableKind.DDFDCB })
        {
            var error = Assert.Throws<GeneratorException>(() =>
                OpcodeTableReader.Read(new StringReader("0x00 NOP"), table, "short.dat"));
            Assert.Contains("short.dat:2: Missing opcode 0x01", error.Message);
        }
    }

    [Fact]
    public void Parser_ClassifiesEveryOperand()
    {
        var tables = Tables();
        var all = tables.Values.SelectMany(t => t).SelectMany(o => o.Operands.Concat(o.InnerInstruction?.Operands ?? []));
        var kinds = all.Select(o => o.Kind).ToHashSet();
        Assert.Equal(Enum.GetValues<OperandKind>().Order(), kinds.Order());
        Assert.Equal(OperandKind.Condition, tables[OpcodeTableKind.Base][0xD8].Operands[0].Kind);
        Assert.Equal(OperandKind.Register8, tables[OpcodeTableKind.Base][0x41].Operands[1].Kind);
        Assert.Equal(OperandKind.SpecialRegister, tables[OpcodeTableKind.ED][0x47].Operands[0].Kind);
        Assert.Equal(new Operand(OperandKind.Register16, "AF'"), tables[OpcodeTableKind.Base][8].Operands[1]);
        Assert.Equal(new Operand(OperandKind.RestartAddress, "8", 8), tables[OpcodeTableKind.Base][0xCF].Operands[0]);
        Assert.Equal(new Operand(OperandKind.Constant, "0", 0), tables[OpcodeTableKind.ED][0x71].Operands[1]);
        Assert.Equal(new Operand(OperandKind.InterruptMode, "2", 2), tables[OpcodeTableKind.ED][0x7E].Operands[0]);
        Assert.Equal(OperandKind.Register8, tables[OpcodeTableKind.ED][0x70].Operands[0].Kind);
    }

    [Fact]
    public void Parser_ParsesDdfdcbCopyForms()
    {
        var table = Tables()[OpcodeTableKind.DDFDCB];
        Assert.Equal(new Operand(OperandKind.Register8, "B"), table[0].CopyTo);
        Assert.Equal("RLC", table[0].InnerInstruction!.Mnemonic);
        Assert.Single(table[0].InnerInstruction!.Operands);
        Assert.Equal("RES", table[0x80].InnerInstruction!.Mnemonic);
        Assert.Equal(new Operand(OperandKind.Bit, "0", 0), table[0x80].InnerInstruction!.Operands[0]);
        Assert.Equal(new Operand(OperandKind.IndexedMemory, "(REGISTER+dd)"), table[0x80].InnerInstruction!.Operands[1]);
        Assert.Equal("LD B,RES 0,(REGISTER+dd)", table[0x80].Comment);
        Assert.Null(table[6].CopyTo);
        Assert.Null(table[6].InnerInstruction);
        Assert.Equal("RLC", table[6].Mnemonic);
        Assert.Null(table[0x86].CopyTo);
    }

    [Theory]
    [InlineData("0x00 LD F,RLC (REGISTER+dd)")]
    [InlineData("0x00 LD B,unknown (REGISTER+dd)")]
    [InlineData("0x00 LD B,RES 0,(HL)")]
    [InlineData("0x00 LD B,RES C,(REGISTER+dd)")]
    public void Parser_RejectsInvalidCopies(string text)
    {
        var error = Assert.Throws<GeneratorException>(() =>
            OpcodeTableReader.Read(new StringReader(text), OpcodeTableKind.DDFDCB, "copy.dat"));
        Assert.StartsWith("copy.dat:1:", error.Message);
    }

    private sealed class TestPattern(params string[] mnemonics) : IPattern
    {
        public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
            && mnemonics.Contains(opcode.Mnemonic)
            && (opcode.Mnemonic != "EX" || opcode.Comment == "EX DE,HL");
        public string EmitBody(Opcode opcode, EmitContext context) => opcode.Mnemonic == "NOP" ? "" : "ExchangeDEHL();";
    }

    [Fact]
    public void Indexed_AbsentOpcodesUseBaseBody()
    {
        var tables = Tables();
        var result = DispatchEmitter.Generate(tables, new PatternCatalog(new TestPattern("NOP", "EX")));
        foreach (var value in new[] { 0x00, 0xEB })
        {
            var main = result.Single(r => r.Table == OpcodeTableKind.Base).Opcodes[value];
            var indexed = result.Single(r => r.Table == OpcodeTableKind.DDFD).Opcodes[value];
            Assert.True(indexed.Implemented);
            Assert.Equal(main.Body, indexed.Body);
            Assert.Equal(main.Opcode.Comment, indexed.Opcode.Comment);
            Assert.Equal(OpcodeTableKind.DDFD, indexed.Opcode.Table);
        }
        var text = result.Single(r => r.Table == OpcodeTableKind.DDFD).Text;
        Assert.Contains("case 0xeb: ExchangeDEHL(); break; // EX DE,HL", text);
        Assert.DoesNotContain("ExecuteMain(", text);
        foreach (var value in new[] { 0xCB, 0xDD, 0xED, 0xFD })
            Assert.DoesNotContain($"case 0x{value:x2}:", text);
        Assert.False(result.Single(r => r.Table == OpcodeTableKind.DDFD).Opcodes[0xE3].Implemented);
    }

    [Fact]
    public void Patterns_AreDisjoint()
    {
        foreach (var opcode in Tables().Values.SelectMany(t => t))
            PatternCatalog.Default.Resolve(opcode);
        var conflict = new PatternCatalog(new NopPattern(), new NopPattern());
        var error = Assert.Throws<GeneratorException>(() => DispatchEmitter.Generate(Tables(), conflict));
        Assert.Contains("opcodes_base.dat:4:", error.Message);
        Assert.Contains("Multiple patterns", error.Message);
    }

    [Fact]
    public void Load8Patterns_RejectAll16BitLoads()
    {
        var catalog = new PatternCatalog(new LoadRegisterRegister(), new LoadRegisterImmediate(),
            new LoadRegisterIndirect(), new StoreIndirectRegister(), new StoreIndirectImmediate(),
            new LoadAccumulatorAbsolute(), new StoreAbsoluteAccumulator(), new LoadRegisterIndexed(),
            new StoreIndexedRegister(), new StoreIndexedImmediate(), new LoadAccumulatorSpecial(), new StoreSpecialAccumulator());
        foreach (var opcode in Tables().Values.SelectMany(t => t).Where(o => o.Mnemonic == "LD"
            && o.Operands.Any(p => p.Kind is OperandKind.Register16 or OperandKind.IndexPair or OperandKind.Immediate16)))
            Assert.Null(catalog.Resolve(opcode));
    }

    [Theory]
    [InlineData(0, 0x41, "Registers.B = Registers.C;")]
    [InlineData(0, 0x40, "")]
    [InlineData(0, 0x0A, "Registers.A = bus.Read(Registers.BC);\nRegisters.WZ = (ushort)(Registers.BC + 1);")]
    [InlineData(3, 0x36, "StoreIndexedImmediate<TIndex>();")]
    [InlineData(3, 0x66, "var address = IndexedAddress<TIndex>();\nRegisters.H = bus.Read(address);")]
    [InlineData(3, 0x26, "TIndex.High(ref Registers) = ReadPc();")]
    [InlineData(2, 0x57, "LoadAFromSpecial(Registers.I);")]
    public void Load8Patterns_EmitExpectedBodies(int tableId, int value, string expected)
    {
        var table = (OpcodeTableKind)tableId;
        var opcode = Tables()[table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.Equal(expected, pattern.EmitBody(opcode, new(table)));
    }

    [Theory]
    [InlineData(0, 0x01, "Registers.BC = ReadPc16();")]
    [InlineData(3, 0x21, "TIndex.Pair(ref Registers) = ReadPc16();")]
    [InlineData(0, 0x2A, "Registers.HL = LoadWordAbsolute();")]
    [InlineData(2, 0x6B, "Registers.HL = LoadWordAbsolute();")]
    [InlineData(2, 0x63, "StoreWordAbsolute(Registers.HL);")]
    [InlineData(2, 0x73, "StoreWordAbsolute(Registers.SP);")]
    [InlineData(3, 0xF9, "bus.Internal(Registers.IR, 2);\nRegisters.SP = TIndex.Pair(ref Registers);")]
    [InlineData(0, 0xF5, "PushWithDelay(Registers.AF);")]
    [InlineData(3, 0xE1, "TIndex.Pair(ref Registers) = Pop();")]
    public void Load16Patterns_EmitExpectedBodies(int tableId, int value, string expected)
    {
        var table = (OpcodeTableKind)tableId;
        var opcode = Tables()[table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.Equal(expected, pattern.EmitBody(opcode, new(table)));
    }

    [Theory]
    [InlineData(0xE3)] // EX (SP),HL
    [InlineData(0x09)] // ADD HL,BC
    [InlineData(0x03)] // INC BC
    [InlineData(0xE9)] // JP (HL)
    public void Load16Patterns_RejectOtherGroups(int value) =>
        Assert.Null(new PatternCatalog(new LoadPairImmediate(), new LoadPairAbsolute(),
            new StoreAbsolutePair(), new LoadStackPointer(), new PushPair(), new PopPair())
            .Resolve(Tables()[OpcodeTableKind.Base][value]));

    [Fact]
    public void Output_GroupsAliasesAndRetainsComments()
    {
        var result = DispatchEmitter.Generate(Tables(), new PatternCatalog(new TestPattern("NEG")));
        var ed = result.Single(r => r.Table == OpcodeTableKind.ED).Text;
        foreach (var value in new[] { 0x44, 0x4C, 0x54, 0x5C, 0x64, 0x6C, 0x74, 0x7C })
            Assert.Contains($"case 0x{value:x2}:", ed);
        Assert.Equal(1, ed.Split("ExchangeDEHL();").Length - 1);
        Assert.Contains("break; // NEG", ed);
    }

    [Fact]
    public void Output_StartsWithLicenseHeader()
    {
        var root = GeneratorApplication.FindRoot(AppContext.BaseDirectory);
        var source = SourceFormat.Normalize(File.ReadAllText(Path.Combine(root, "ZXSinclair.Net.Core", "Z80", "Z80Cpu.cs")));
        var license = source[..(source.IndexOf("#endregion", StringComparison.Ordinal) + "#endregion".Length)];
        foreach (var dispatch in Generate())
        {
            Assert.StartsWith(license, SourceFormat.Normalize(dispatch.Text));
            Assert.Contains("// <auto-generated>", dispatch.Text);
            Assert.Contains("#nullable enable", dispatch.Text);
            Assert.DoesNotContain(root, dispatch.Text);
        }
    }

    [Fact]
    public void Output_IsDeterministic()
    {
        var first = Generate();
        var second = Generate();
        Assert.Equal(first.Select(d => d.FileName), second.Select(d => d.FileName));
        for (var i = 0; i < first.Length; i++)
            Assert.Equal(Encoding.UTF8.GetBytes(first[i].Text), Encoding.UTF8.GetBytes(second[i].Text));
    }

    [Fact]
    public void Output_MatchesCommittedFiles()
    {
        var root = GeneratorApplication.FindRoot(AppContext.BaseDirectory);
        foreach (var dispatch in Generate())
        {
            var path = Path.Combine(root, "ZXSinclair.Net.Core", "Z80", "Generated", dispatch.FileName);
            Assert.True(File.Exists(path), $"Generated file is missing: {path}. Run the generator.");
            Assert.Equal(SourceFormat.Normalize(dispatch.Text), SourceFormat.Normalize(File.ReadAllText(path)));
        }
    }

    [Fact]
    public void Output_MatchesExpectedCoverage()
    {
        foreach (var dispatch in Generate())
        {
            var expected = dispatch.Table switch
            {
                OpcodeTableKind.Base or OpcodeTableKind.DDFD => 135,
                OpcodeTableKind.ED => 20,
                _ => 0,
            };
            Assert.Equal(expected, dispatch.Opcodes.Count(o => o.Implemented));
            Assert.Equal(dispatch.Table is OpcodeTableKind.Base or OpcodeTableKind.DDFD ? 1 : 0,
                dispatch.Text.Split(dispatch.Table == OpcodeTableKind.Base
                    ? "case 0x00: return; // NOP" : "case 0x00: break; // NOP").Length - 1);
            Assert.Contains("default: Unimplemented(); break;", dispatch.Text);
        }
    }

    [Fact]
    public void MainDispatch_UsesInlineHelpersAndDirectReturns()
    {
        var main = Generate().Single(d => d.Table == OpcodeTableKind.Base);
        Assert.Contains("MethodImplOptions.AggressiveInlining", main.Text);
        Assert.Contains("if (opcode == 0) return;", main.Text);
        Assert.Contains("ExecuteMainDispatch(opcode);", main.Text);
        Assert.Contains("private void ExecuteMainDispatch(byte opcode)", main.Text);
        foreach (var item in main.Opcodes.Where(o => o.Implemented && o.Body.Length != 0))
        {
            Assert.Contains($"case 0x{item.Opcode.Byte:x2}: ExecuteMain{item.Opcode.Byte:X2}(); return;", main.Text);
            Assert.Contains($"private void ExecuteMain{item.Opcode.Byte:X2}()", main.Text);
            foreach (var line in item.Body.Split('\n'))
                Assert.Contains("        " + line, main.Text);
        }
        Assert.Contains("case 0x40: return; // LD B,B", main.Text);
    }

    [Theory]
    [InlineData(0, 0xC3, "JumpAbsolute(true);")]
    [InlineData(0, 0xC2, "JumpAbsolute((Registers.F & Z80Flags.Z) == 0);")]
    [InlineData(0, 0xE9, "Registers.PC = Registers.HL;")]
    [InlineData(3, 0xE9, "Registers.PC = TIndex.Pair(ref Registers);")]
    [InlineData(0, 0x18, "JumpRelative(true);")]
    [InlineData(0, 0x38, "JumpRelative((Registers.F & Z80Flags.C) != 0);")]
    [InlineData(0, 0x10, "DecrementJumpNonZero();")]
    [InlineData(0, 0xEC, "CallAbsolute((Registers.F & Z80Flags.PV) != 0);")]
    [InlineData(0, 0xC9, "Return();")]
    [InlineData(0, 0xF8, "ReturnConditional((Registers.F & Z80Flags.S) != 0);")]
    [InlineData(0, 0xCF, "Restart(0x08);")]
    public void JumpPatterns_EmitExpectedBodies(int table, int value, string expected)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.Equal(expected, pattern.EmitBody(opcode, new((OpcodeTableKind)table)));
    }

    [Fact]
    public void JumpPatterns_GroupAllInterruptReturnAliases()
    {
        var ed = Generate().Single(d => d.Table == OpcodeTableKind.ED);
        foreach (var value in new[] { 0x45, 0x4D, 0x55, 0x5D, 0x65, 0x6D, 0x75, 0x7D })
        {
            Assert.Equal("ReturnFromInterrupt();", ed.Opcodes[value].Body);
            Assert.Contains($"case 0x{value:x2}:", ed.Text);
        }
        Assert.Equal(1, ed.Text.Split("ReturnFromInterrupt();").Length - 1);
    }

    private sealed class OpcodeZeroPattern(string body) : IPattern
    {
        public bool Matches(Opcode opcode) => opcode.Table == OpcodeTableKind.Base && opcode.Byte == 0;
        public string EmitBody(Opcode opcode, EmitContext context) => body;
    }

    [Theory]
    [InlineData("")]
    [InlineData("Registers.A = 1;")]
    public void MainDispatch_FastPathRequiresEmptyOpcodeZero(string body)
    {
        var main = DispatchEmitter.Generate(Tables(), new PatternCatalog(new OpcodeZeroPattern(body)))
            .Single(d => d.Table == OpcodeTableKind.Base);
        Assert.Equal(body.Length == 0, main.Text.Contains("if (opcode == 0) return;"));
        Assert.Equal(body.Length == 0, main.Text.Contains("private void ExecuteMainDispatch(byte opcode)"));
        var pending = DispatchEmitter.Generate(Tables(), new PatternCatalog())
            .Single(d => d.Table == OpcodeTableKind.Base);
        Assert.DoesNotContain("if (opcode == 0) return;", pending.Text);
    }

    [Fact]
    public void OperandEmitter_TranslatesAllClasses()
    {
        foreach (var opcode in Tables().Values.SelectMany(t => t))
        {
            var context = new EmitContext(opcode.Table);
            foreach (var operand in opcode.Operands.Concat(opcode.InnerInstruction?.Operands ?? []))
            {
                if (operand.Kind == OperandKind.PrefixTable) continue;
                Assert.NotEmpty(OperandEmitter.Value(operand, context));
            }
        }
        var source = new SourceLocation("operand.dat", 1);
        string Emit(string text, string mnemonic, int position = 0) =>
            OperandEmitter.Value(OperandParser.Parse(text, mnemonic, position, source), new(OpcodeTableKind.DDFD));
        Assert.Equal("Registers.AF_", Emit("AF'", "EX", 1));
        Assert.Equal("TIndex.High(ref Registers)", Emit("REGISTERH", "LD"));
        Assert.Equal("TIndex.Low(ref Registers)", Emit("REGISTERL", "LD"));
        Assert.Equal("TIndex.Pair(ref Registers)", Emit("REGISTER", "LD"));
        Assert.Equal("ReadPc()", Emit("nn", "LD"));
        Assert.Equal("ReadPc16()", Emit("nnnn", "LD"));
        Assert.Equal("bus.Read(ReadPc16())", Emit("(nnnn)", "LD"));
        Assert.Equal("bus.Read(Registers.HL)", Emit("(HL)", "LD"));
        Assert.Equal("bus.Read(IndexedAddress<TIndex>())", Emit("(REGISTER+dd)", "LD"));
        Assert.Equal("bus.Read(address)", OperandEmitter.Value(new(OperandKind.IndexedMemory, "(REGISTER+dd)"), new(OpcodeTableKind.DDFDCB)));
        Assert.Equal("Registers.BC", Emit("(C)", "IN"));
        Assert.Equal("(ushort)((Registers.A << 8) | ReadPc())", Emit("(nn)", "OUT"));
        Assert.Equal("(sbyte)ReadPc()", Emit("offset", "JR"));
        Assert.Equal("(Registers.F & Z80Flags.C) != 0", Emit("C", "JP"));
        Assert.Equal("Registers.C", Emit("C", "LD"));
        Assert.Equal("(Registers.F & Z80Flags.PV) == 0", Emit("PO", "RET"));
        Assert.Equal("(Registers.F & Z80Flags.S) == 0", Emit("P", "JP"));
        Assert.Equal("56", Emit("38", "RST"));
    }

    [Fact]
    public void Coverage_IncludesHolesAliasesAndPendingDetails()
    {
        var output = new StringWriter();
        CoverageReport.Write(Generate(), output, true);
        var text = output.ToString();
        Assert.Contains("135 implemented / 117 pending / 4 prefixes", text);
        Assert.Contains("20 implemented / 58 pending", text);
        Assert.Contains("178 holes (178 pending) / 19 aliases", text);
        Assert.Contains("0xFB slttrap [Hole]", text);
        Assert.DoesNotContain("0x00 NOP", text);
    }

    [Fact]
    public void Check_DetectsEditsAndMissingFilesWithoutWriting()
    {
        var directory = Path.Combine(Path.GetTempPath(), "zx-generator-" + Guid.NewGuid().ToString("N"));
        try
        {
            var output = new StringWriter();
            var error = new StringWriter();
            int Run(params string[] options) => GeneratorApplication.Run(["--output", directory, .. options], directory, output, error);
            Assert.Equal(1, Run("--check"));
            Assert.False(Directory.Exists(directory));
            Assert.Equal(0, Run());
            var files = Directory.GetFiles(directory);
            Assert.Equal(5, files.Length);
            Assert.All(files, f => Assert.False(File.ReadAllBytes(f).Take(3).SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF })));
            var before = files.ToDictionary(f => f, File.GetLastWriteTimeUtc);
            Assert.Equal(0, Run());
            Assert.All(files, f => Assert.Equal(before[f], File.GetLastWriteTimeUtc(f)));
            Assert.Equal(0, Run("--check"));
            var path = Path.Combine(directory, "Z80Cpu.Main.g.cs");
            File.WriteAllText(path, SourceFormat.Normalize(File.ReadAllText(path)));
            Assert.Equal(0, Run("--check"));
            File.AppendAllText(path, "// edited\n");
            var edited = File.ReadAllBytes(path);
            Assert.Equal(1, Run("--check"));
            Assert.Equal(edited, File.ReadAllBytes(path));
            Assert.Contains("Z80Cpu.Main.g.cs", output.ToString());
            Assert.Empty(error.ToString());
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Theory]
    [InlineData("--unknown")]
    [InlineData("--output")]
    [InlineData("--output", "--check")]
    public void Cli_InvalidArgumentsReturnTwo(params string[] args)
    {
        var error = new StringWriter();
        Assert.Equal(2, GeneratorApplication.Run(args, Directory.GetCurrentDirectory(), new StringWriter(), error));
        Assert.NotEmpty(error.ToString());
    }

    [Fact]
    public void Root_NotFoundReportsSolutionName()
    {
        var error = Assert.Throws<GeneratorException>(() => GeneratorApplication.FindRoot(Path.GetPathRoot(AppContext.BaseDirectory)!));
        Assert.Contains("zxsinclair.net.slnx", error.Message);
    }
}
