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
    public void EdHolePattern_ImplementsOnlyEdHolesWithoutFlags()
    {
        var pattern = new EdHolePattern();
        foreach (var opcode in Tables().Values.SelectMany(t => t))
        {
            Assert.Equal(opcode.Table == OpcodeTableKind.ED && opcode.Kind == OpcodeKind.Hole,
                pattern.Matches(opcode));
            Assert.False(((IPattern)pattern).WritesFlags(opcode));
        }
        var ed = Generate().Single(d => d.Table == OpcodeTableKind.ED);
        Assert.Equal(1, ed.Text.Split("// ED hole: two NOPs").Length - 1);
    }

    [Fact]
    public void Im0Table_MatchesReferenceList()
    {
        byte[] excluded =
        [
            0x01,0x06,0x0E,0x10,0x11,0x16,0x18,0x1E,0x20,0x21,0x22,0x26,0x28,0x2A,0x2E,
            0x30,0x31,0x32,0x36,0x38,0x3A,0x3E,0xC2,0xC3,0xC4,0xC6,0xCA,0xCB,0xCC,0xCD,0xCE,
            0xD2,0xD3,0xD4,0xD6,0xDA,0xDB,0xDC,0xDD,0xDE,0xE2,0xE4,0xE6,0xEA,0xEC,0xED,0xEE,
            0xF2,0xF4,0xF6,0xFA,0xFC,0xFD,0xFE,
        ];
        var bits = Im0TableEmitter.BuildBits(Generate().Single(d => d.Table == OpcodeTableKind.Base));
        Assert.Equal(32, bits.Length);
        for (var value = 0; value < 256; value++)
            Assert.Equal(!excluded.Contains((byte)value), (bits[value >> 3] & (1 << (value & 7))) != 0);
        var pilot = DispatchEmitter.Generate(Tables(), new PatternCatalog(new NopPattern()))
            .Single(d => d.Table == OpcodeTableKind.Base);
        var pilotBits = Im0TableEmitter.BuildBits(pilot);
        Assert.Equal((byte)1, pilotBits[0]);
        Assert.All(pilotBits.Skip(1), b => Assert.Equal((byte)0, b));
    }

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
        Assert.Contains("case 0xeb: // EX DE,HL", text);
        Assert.Contains("ExchangeDEHL();\n                Registers.Q = 0;", SourceFormat.Normalize(text));
        Assert.DoesNotContain("ExecuteMain(", text);
        foreach (var value in new[] { 0xCB, 0xDD, 0xED, 0xFD })
            Assert.DoesNotContain($"case 0x{value:x2}:", text);
        Assert.False(result.Single(r => r.Table == OpcodeTableKind.DDFD).Opcodes[0xE3].Implemented);
    }

    [Theory]
    [InlineData((int)OpcodeTableKind.Base, 0x00, 1)]
    [InlineData((int)OpcodeTableKind.Base, 0x06, 2)]
    [InlineData((int)OpcodeTableKind.Base, 0x01, 3)]
    [InlineData((int)OpcodeTableKind.Base, 0x18, 2)]
    [InlineData((int)OpcodeTableKind.Base, 0x3A, 3)]
    [InlineData((int)OpcodeTableKind.Base, 0xDB, 2)]
    [InlineData((int)OpcodeTableKind.Base, 0xCD, 3)]
    [InlineData((int)OpcodeTableKind.Base, 0x7E, 1)]
    [InlineData((int)OpcodeTableKind.CB, 0x46, 2)]
    [InlineData((int)OpcodeTableKind.ED, 0x43, 4)]
    [InlineData((int)OpcodeTableKind.ED, 0xB0, 2)]
    [InlineData((int)OpcodeTableKind.DDFD, 0x36, 4)]
    [InlineData((int)OpcodeTableKind.DDFD, 0x46, 3)]
    [InlineData((int)OpcodeTableKind.DDFD, 0x21, 4)]
    [InlineData((int)OpcodeTableKind.DDFDCB, 0x06, 4)]
    [InlineData((int)OpcodeTableKind.DDFDCB, 0x00, 4)]
    public void Opcode_LengthMatchesReference(int table, int value, int length)
    {
        // Instruction lengths from the Zilog manual, including prefix bytes (DD/FD CB d counts as three).
        Assert.Equal(length, Tables()[(OpcodeTableKind)table][value].Length);
    }

    [Fact]
    public void SingleByteBodies_DoNotReadThroughPc()
    {
        // Helpers that read operand bytes through PC. Extend this list whenever a new helper does,
        // so an opcode that reads operands without declaring them cannot reach the IM 0 table.
        string[] pcReaders = ["ReadPc(", "ReadPc16(", "ReadPcDiscarded(", "ReadPc16Discarded(", "IndexedAddress<",
            "InAccumulator(", "OutAccumulator(", "StoreIndexedImmediate<", "JumpAbsolute(", "JumpRelative(",
            "CallAbsolute(", "DecrementJumpNonZero(", "LoadWordAbsolute(", "StoreWordAbsolute("];
        var main = Generate().Single(d => d.Table == OpcodeTableKind.Base);
        var singleByte = main.Opcodes.Where(o => o.Implemented && o.Opcode.Length == 1).ToArray();
        Assert.NotEmpty(singleByte);
        foreach (var item in singleByte)
            foreach (var reader in pcReaders)
                Assert.False(item.Body.Contains(reader, StringComparison.Ordinal),
                    $"0x{item.Opcode.Byte:X2} {item.Opcode.Comment} has Length 1 but calls {reader}");
        foreach (var item in main.Opcodes.Where(o => o.Implemented && o.Opcode.Length > 1))
            Assert.True(pcReaders.Any(reader => item.Body.Contains(reader, StringComparison.Ordinal)),
                $"0x{item.Opcode.Byte:X2} {item.Opcode.Comment} has Length {item.Opcode.Length} but reads no operand");
    }

    [Fact]
    public void Patterns_IgnoreNonInstructions()
    {
        foreach (var opcode in Tables().Values.SelectMany(t => t))
            foreach (var kind in new[] { OpcodeKind.Prefix, OpcodeKind.Hole, OpcodeKind.Absent })
            {
                var copy = opcode with { Kind = kind };
                // Only EdHolePattern may claim a hole, and only in the ED table.
                if (kind == OpcodeKind.Hole && opcode.Table == OpcodeTableKind.ED)
                    Assert.IsType<EdHolePattern>(PatternCatalog.Default.Resolve(copy));
                else
                    Assert.Null(PatternCatalog.Default.Resolve(copy));
            }
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
        Assert.Contains("case 0x7c: // NEG", ed);
    }

    [Fact]
    public void Output_StartsWithLicenseHeader()
    {
        var root = GeneratorApplication.FindRoot(AppContext.BaseDirectory);
        var source = SourceFormat.Normalize(File.ReadAllText(Path.Combine(root, "ZXSinclair.Net.Core", "Z80", "Z80Cpu.cs")));
        var license = source[..(source.IndexOf("#endregion", StringComparison.Ordinal) + "#endregion".Length)];
        foreach (var dispatch in Im0TableEmitter.Sources(Generate()))
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
        var first = Im0TableEmitter.Sources(Generate());
        var second = Im0TableEmitter.Sources(Generate());
        Assert.Equal(first.Select(d => d.FileName), second.Select(d => d.FileName));
        for (var i = 0; i < first.Length; i++)
            Assert.Equal(Encoding.UTF8.GetBytes(first[i].Text), Encoding.UTF8.GetBytes(second[i].Text));
    }

    [Fact]
    public void Output_MatchesCommittedFiles()
    {
        var root = GeneratorApplication.FindRoot(AppContext.BaseDirectory);
        foreach (var dispatch in Im0TableEmitter.Sources(Generate()))
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
                OpcodeTableKind.Base or OpcodeTableKind.DDFD => 252,
                OpcodeTableKind.ED => 256,
                OpcodeTableKind.CB or OpcodeTableKind.DDFDCB => 256,
                _ => 0,
            };
            Assert.Equal(expected, dispatch.Opcodes.Count(o => o.Implemented));
            Assert.Equal(dispatch.Table is OpcodeTableKind.Base or OpcodeTableKind.DDFD ? 1 : 0,
                dispatch.Text.Split(dispatch.Table == OpcodeTableKind.Base
                    ? "case 0x00: ExecuteMain00(); return; // NOP" : "case 0x00: Registers.Q = 0; break; // NOP").Length - 1);
            Assert.Equal(dispatch.Opcodes.Any(o => !o.Implemented),
                dispatch.Text.Contains("default: Unimplemented(); break;", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void MainDispatch_UsesInlineHelpersAndDirectReturns()
    {
        var main = Generate().Single(d => d.Table == OpcodeTableKind.Base);
        Assert.Contains("MethodImplOptions.AggressiveInlining", main.Text);
        Assert.Contains("if (opcode == 0)", main.Text);
        Assert.Contains("ExecuteMainDispatch(opcode);", main.Text);
        Assert.Contains("private void ExecuteMainDispatch(byte opcode)", main.Text);
        foreach (var item in main.Opcodes.Where(o => o.Implemented && o.Body.Length != 0))
        {
            Assert.Contains($"case 0x{item.Opcode.Byte:x2}: ExecuteMain{item.Opcode.Byte:X2}(); return;", main.Text);
            Assert.Contains($"private void ExecuteMain{item.Opcode.Byte:X2}()", main.Text);
            foreach (var line in item.Body.Split('\n'))
                Assert.Contains("        " + line, main.Text);
        }
        Assert.Contains("case 0x40: ExecuteMain40(); return; // LD B,B", main.Text);
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
            Assert.Equal("ReturnFromInterrupt();\nRegisters.Q = 0;", ed.Opcodes[value].Body);
            Assert.Contains($"case 0x{value:x2}:", ed.Text);
        }
        Assert.Equal(1, ed.Text.Split("ReturnFromInterrupt();").Length - 1);
    }

    private sealed class OpcodeZeroPattern(string body, bool writesFlags = false) : IPattern
    {
        public bool Matches(Opcode opcode) => opcode.Table == OpcodeTableKind.Base && opcode.Byte == 0;
        public string EmitBody(Opcode opcode, EmitContext context) => body;
        public bool WritesFlags(Opcode opcode) => writesFlags;
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("Registers.A = 1;", false)]
    [InlineData("", true)]
    public void MainDispatch_FastPathRequiresEmptyNonFlagWritingOpcodeZero(string body, bool writesFlags)
    {
        var main = DispatchEmitter.Generate(Tables(), new PatternCatalog(new OpcodeZeroPattern(body, writesFlags)))
            .Single(d => d.Table == OpcodeTableKind.Base);
        Assert.Equal(body.Length == 0 && !writesFlags, main.Text.Contains("if (opcode == 0)"));
        Assert.Equal(body.Length == 0 && !writesFlags, main.Text.Contains("private void ExecuteMainDispatch(byte opcode)"));
        var pending = DispatchEmitter.Generate(Tables(), new PatternCatalog())
            .Single(d => d.Table == OpcodeTableKind.Base);
        Assert.DoesNotContain("if (opcode == 0)", pending.Text);
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
        Assert.Contains("252 implemented / 0 pending / 4 prefixes", text);
        Assert.Contains("256 implemented / 0 pending", text);
        Assert.Contains("178 holes (0 pending)", text);
        Assert.Equal(3, text.Split("256 implemented / 0 pending").Length - 1);
        Assert.Contains("178 holes (0 pending) / 19 aliases", text);
        Assert.DoesNotContain("0xFB slttrap [Hole]", text);
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
            Assert.Equal(6, files.Length);
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

    [Theory]
    [InlineData(0, 0x80, "Add8(Registers.B);")]
    [InlineData(0, 0xD6, "Sub8(ReadPc());")]
    [InlineData(0, 0xB8, "Cp8(Registers.B);")]
    [InlineData(3, 0xBC, "Cp8(TIndex.High(ref Registers));")]
    [InlineData(3, 0xAE, "Xor8(bus.Read(IndexedAddress<TIndex>()));")]
    [InlineData(0, 0x34, "IncMemory(Registers.HL);")]
    [InlineData(3, 0x2D, "TIndex.Low(ref Registers) = Dec8(TIndex.Low(ref Registers));")]
    public void Alu8Patterns_EmitExpectedBodies(int table, int value, string expected)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.Equal(expected, pattern.EmitBody(opcode, new EmitContext(opcode.Table)));
    }

    [Theory]
    [InlineData(0, 0x09)]
    [InlineData(2, 0x4A)]
    [InlineData(2, 0x42)]
    [InlineData(0, 0x03)]
    [InlineData(2, 0x44)]
    public void Alu8Patterns_RejectOtherGroups(int table, int value) =>
        Assert.Null(new PatternCatalog(new Alu8Pattern(), new IncDec8Register(), new IncDec8Memory())
            .Resolve(Tables()[(OpcodeTableKind)table][value]));

    [Fact]
    public void Patterns_WritesFlagsMatchesSpec()
    {
        var flags = new HashSet<int>(Enumerable.Range(0x80, 0x40));
        foreach (var value in new[] { 0x04, 0x05, 0x0C, 0x0D, 0x14, 0x15, 0x1C, 0x1D,
            0x24, 0x25, 0x2C, 0x2D, 0x34, 0x35, 0x3C, 0x3D, 0xC6, 0xCE, 0xD6,
            0xDE, 0xE6, 0xEE, 0xF6, 0xFE, 0x27, 0x2F, 0x37, 0x3F, 0x09, 0x19, 0x29, 0x39,
            0x07, 0x0F, 0x17, 0x1F })
            flags.Add(value);
        var tables = Tables();
        foreach (var table in new[] { OpcodeTableKind.Base, OpcodeTableKind.ED, OpcodeTableKind.DDFD,
            OpcodeTableKind.CB, OpcodeTableKind.DDFDCB })
        foreach (var original in tables[table])
        {
            var opcode = table == OpcodeTableKind.DDFD && original.Kind == OpcodeKind.Absent
                ? tables[OpcodeTableKind.Base][original.Byte] with { Table = table } : original;
            var pattern = PatternCatalog.Default.Resolve(opcode);
            if (pattern is null) continue;
            var expected = table is OpcodeTableKind.CB or OpcodeTableKind.DDFDCB ? opcode.Byte < 0x80
                : table == OpcodeTableKind.ED
                ? opcode.Byte is 0x40 or 0x48 or 0x50 or 0x58 or 0x60 or 0x68 or 0x70 or 0x78 or 0xA2 or 0xAA or 0xB2 or 0xBA or 0xA3 or 0xAB or 0xB3 or 0xBB or 0xA0 or 0xA1 or 0xA8 or 0xA9 or 0xB0 or 0xB1 or 0xB8 or 0xB9 or 0x67 or 0x6F or 0x42 or 0x4A or 0x52 or 0x5A or 0x62 or 0x6A or 0x72 or 0x7A or 0x57 or 0x5F or 0x44 or 0x4C or 0x54 or 0x5C or 0x64 or 0x6C or 0x74 or 0x7C
                : flags.Contains(opcode.Byte);
            Assert.Equal(expected, pattern.WritesFlags(opcode));
        }
    }

    [Fact]
    public void Dispatch_AppendsQWrite()
    {
        foreach (var dispatch in Generate())
        foreach (var item in dispatch.Opcodes.Where(o => o.Implemented))
        {
            var pattern = PatternCatalog.Default.Resolve(item.Opcode)!;
            var raw = pattern.EmitBody(item.Opcode, new(dispatch.Table));
            Assert.Equal(SourceFormat.Normalize(raw).Trim(), item.PatternBody);
            var q = pattern.WritesFlags(item.Opcode) ? "Registers.Q = Registers.F;" : "Registers.Q = 0;";
            Assert.Equal((item.PatternBody.Length == 0 ? "" : item.PatternBody + "\n") + q, item.Body);
        }
    }

    [Theory]
    [InlineData(0, 0x27, "DecimalAdjust();")]
    [InlineData(0, 0x2F, "Complement();")]
    [InlineData(0, 0x37, "SetCarry();")]
    [InlineData(0, 0x3F, "ComplementCarry();")]
    [InlineData(0, 0x76, "Halt();")]
    [InlineData(0, 0xF3, "Registers.IFF1 = Registers.IFF2 = false;")]
    [InlineData(0, 0xFB, "EnableInterrupts();")]
    [InlineData(2, 0x44, "Negate();")]
    [InlineData(2, 0x4E, "Registers.IM = 0;")]
    [InlineData(2, 0x76, "Registers.IM = 1;")]
    [InlineData(2, 0x7E, "Registers.IM = 2;")]
    public void ControlPatterns_EmitExpectedBodies(int table, int value, string expected)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        Assert.Equal(expected, PatternCatalog.Default.Resolve(opcode)!.EmitBody(opcode, new(opcode.Table)));
    }

    [Fact]
    public void ControlPatterns_GroupNegAndImAliases()
    {
        var ed = Generate().Single(d => d.Table == OpcodeTableKind.ED);
        Assert.Equal(8, ed.Opcodes.Count(o => o.Implemented && o.PatternBody == "Negate();"));
        Assert.Equal(4, ed.Opcodes.Count(o => o.Implemented && o.PatternBody == "Registers.IM = 0;"));
        Assert.Equal(1, ed.Text.Split("Negate();").Length - 1);
        Assert.Equal(1, ed.Text.Split("Registers.IM = 0;").Length - 1);
    }

    [Theory]
    [InlineData(0, 0x09, "Registers.HL = Add16(Registers.HL, Registers.BC);")]
    [InlineData(3, 0x29, "TIndex.Pair(ref Registers) = Add16(TIndex.Pair(ref Registers), TIndex.Pair(ref Registers));")]
    [InlineData(2, 0x7A, "Adc16(Registers.SP);")]
    [InlineData(2, 0x52, "Sbc16(Registers.DE);")]
    [InlineData(3, 0x23, "bus.Internal(Registers.IR, 2);\nTIndex.Pair(ref Registers)++;")]
    [InlineData(0, 0x3B, "bus.Internal(Registers.IR, 2);\nRegisters.SP--;")]
    public void Alu16Patterns_EmitExpectedBodies(int table, int value, string expected)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.Equal(expected, pattern.EmitBody(opcode, new(opcode.Table)));
    }

    [Fact]
    public void Alu16Patterns_AreDisjointFromAlu8()
    {
        var tables = Tables();
        var word = new PatternCatalog(new Add16Pattern(), new AdcSbc16Pattern(), new IncDec16Pattern());
        var bytes = new PatternCatalog(new Alu8Pattern(), new IncDec8Register(), new IncDec8Memory());
        foreach (var opcode in tables.Values.SelectMany(t => t))
        {
            if (word.Resolve(opcode) is not null) Assert.Null(bytes.Resolve(opcode));
            if (bytes.Resolve(opcode) is not null) Assert.Null(word.Resolve(opcode));
        }
    }

    [Theory]
    [InlineData(0, 0x07, "Rlca();")]
    [InlineData(1, 0x11, "Registers.C = Rl(Registers.C);")]
    [InlineData(1, 0x2E, "var value = bus.Read(Registers.HL);\nbus.Internal(Registers.HL, 1);\nvalue = Sra(value);\nbus.Write(Registers.HL, value);")]
    [InlineData(4, 0x36, "var value = bus.Read(address);\nbus.Internal(address, 1);\nvalue = Sll(value);\nbus.Write(address, value);")]
    [InlineData(4, 0x0C, "var value = bus.Read(address);\nbus.Internal(address, 1);\nvalue = Rrc(value);\nbus.Write(address, value);\nRegisters.H = value;")]
    [InlineData(2, 0x6F, "Rld();")]
    public void RotatePatterns_EmitExpectedBodies(int table, int value, string expected)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.True(pattern.WritesFlags(opcode));
        Assert.Equal(expected, pattern.EmitBody(opcode, new(opcode.Table)));
    }

    [Theory]
    [InlineData(1, 0x40)] [InlineData(1, 0x86)] [InlineData(4, 0xFE)]
    public void RotatePatterns_DoNotMatchBitOperations(int table, int value)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        IPattern[] patterns = [new RotateAccumulatorPattern(), new RotateRegisterPattern(),
            new RotateMemoryPattern(), new RotateMemoryCopyPattern(), new RotateDigitPattern()];
        Assert.All(patterns, pattern => Assert.False(pattern.Matches(opcode)));
    }

    [Fact]
    public void Output_OmitsDefaultWhenTableComplete()
    {
        foreach (var dispatch in Generate())
            Assert.Equal(dispatch.Table is OpcodeTableKind.Base or OpcodeTableKind.DDFD,
                dispatch.Text.Contains("default: Unimplemented(); break;", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(1, 0x7F, "BitTest(0x80, Registers.A);")]
    [InlineData(1, 0x46, "var value = bus.Read(Registers.HL);\nbus.Internal(Registers.HL, 1);\nBitTestMemory(0x01, value);")]
    [InlineData(4, 0x40, "var value = bus.Read(address);\nbus.Internal(address, 1);\nBitTestMemory(0x01, value);")]
    [InlineData(1, 0xD9, "Registers.C = (byte)(Registers.C | 0x08);")]
    [InlineData(1, 0xBE, "var value = bus.Read(Registers.HL);\nbus.Internal(Registers.HL, 1);\nvalue = (byte)(value & 0x7F);\nbus.Write(Registers.HL, value);")]
    [InlineData(4, 0xCD, "var value = bus.Read(address);\nbus.Internal(address, 1);\nvalue = (byte)(value | 0x02);\nbus.Write(address, value);\nRegisters.L = value;")]
    public void BitPatterns_EmitExpectedBodies(int table, int value, string expected)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.Equal(expected, pattern.EmitBody(opcode, new(opcode.Table)));
    }

    [Fact]
    public void BitPatterns_AreDisjointFromRotations()
    {
        var bits = new PatternCatalog(new BitTestRegisterPattern(), new BitTestMemoryPattern(),
            new SetResRegisterPattern(), new SetResMemoryPattern(), new SetResMemoryCopyPattern());
        var rotations = new PatternCatalog(new RotateAccumulatorPattern(), new RotateRegisterPattern(),
            new RotateMemoryPattern(), new RotateMemoryCopyPattern(), new RotateDigitPattern());
        foreach (var opcode in Tables().Values.SelectMany(t => t))
        {
            if (bits.Resolve(opcode) is not null) Assert.Null(rotations.Resolve(opcode));
            if (rotations.Resolve(opcode) is not null) Assert.Null(bits.Resolve(opcode));
        }
    }

    [Theory]
    [InlineData(0, 0xEB, "(Registers.DE, Registers.HL) = (Registers.HL, Registers.DE);", false)]
    [InlineData(0, 0x08, "Registers.ExchangeAF();", false)]
    [InlineData(0, 0xD9, "Registers.Exx();", false)]
    [InlineData(0, 0xE3, "Registers.HL = ExchangeStack(Registers.HL);", false)]
    [InlineData(3, 0xE3, "TIndex.Pair(ref Registers) = ExchangeStack(TIndex.Pair(ref Registers));", false)]
    [InlineData(2, 0xA0, "BlockLoad(1);", true)]
    [InlineData(2, 0xA8, "BlockLoad(-1);", true)]
    [InlineData(2, 0xB0, "BlockLoadRepeat(1);", true)]
    [InlineData(2, 0xB8, "BlockLoadRepeat(-1);", true)]
    [InlineData(2, 0xA1, "BlockCompare(1);", true)]
    [InlineData(2, 0xA9, "BlockCompare(-1);", true)]
    [InlineData(2, 0xB1, "BlockCompareRepeat(1);", true)]
    [InlineData(2, 0xB9, "BlockCompareRepeat(-1);", true)]
    public void BlockPatterns_EmitExpectedBodies(int table, int value, string expected, bool flags)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.Equal(expected, pattern.EmitBody(opcode, new(opcode.Table)));
        Assert.Equal(flags, pattern.WritesFlags(opcode));
    }

    [Theory]
    [InlineData(0, 0xDB, "InAccumulator();", false)]
    [InlineData(0, 0xD3, "OutAccumulator();", false)]
    [InlineData(2, 0x40, "Registers.B = InRegister();", true)]
    [InlineData(2, 0x48, "Registers.C = InRegister();", true)]
    [InlineData(2, 0x70, "InRegister();", true)]
    [InlineData(2, 0x71, "OutRegister(0);", false)]
    [InlineData(2, 0x41, "OutRegister(Registers.B);", false)]
    [InlineData(2, 0xA2, "BlockIn(1);", true)]
    [InlineData(2, 0xAA, "BlockIn(-1);", true)]
    [InlineData(2, 0xB2, "BlockInRepeat(1);", true)]
    [InlineData(2, 0xBA, "BlockInRepeat(-1);", true)]
    [InlineData(2, 0xA3, "BlockOut(1);", true)]
    [InlineData(2, 0xAB, "BlockOut(-1);", true)]
    [InlineData(2, 0xB3, "BlockOutRepeat(1);", true)]
    [InlineData(2, 0xBB, "BlockOutRepeat(-1);", true)]
    public void IoPatterns_EmitExpectedBodies(int table, int value, string expected, bool flags)
    {
        var opcode = Tables()[(OpcodeTableKind)table][value];
        var pattern = PatternCatalog.Default.Resolve(opcode);
        Assert.NotNull(pattern);
        Assert.Equal(expected, pattern.EmitBody(opcode, new(opcode.Table)));
        Assert.Equal(flags, pattern.WritesFlags(opcode));
    }

    [Fact]
    public void BitAliases_ShareOneBodyPerBit()
    {
        var indexed = Generate().Single(d => d.Table == OpcodeTableKind.DDFDCB);
        for (var bit = 0; bit < 8; bit++)
        {
            var body = $"var value = bus.Read(address);\nbus.Internal(address, 1);\nBitTestMemory(0x{1 << bit:X2}, value);";
            Assert.Equal(8, indexed.Opcodes.Count(o => o.PatternBody == body));
            Assert.Equal(1, indexed.Text.Split($"BitTestMemory(0x{1 << bit:X2}, value);").Length - 1);
        }
    }
}
