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

using System.Globalization;
using ZXSinclair.Net.Generate.Z80OpCodes.Model;

namespace ZXSinclair.Net.Generate.Z80OpCodes.Tables;

internal static class OpcodeTableReader
{
    public static string FileName(OpcodeTableKind table) => table switch
    {
        OpcodeTableKind.Base => "opcodes_base.dat",
        OpcodeTableKind.CB => "opcodes_cb.dat",
        OpcodeTableKind.ED => "opcodes_ed.dat",
        OpcodeTableKind.DDFD => "opcodes_ddfd.dat",
        OpcodeTableKind.DDFDCB => "opcodes_ddfdcb.dat",
        _ => throw new ArgumentOutOfRangeException(nameof(table)),
    };

    public static IReadOnlyDictionary<OpcodeTableKind, Opcode[]> ReadAll()
    {
        var tables = new Dictionary<OpcodeTableKind, Opcode[]>();
        var assembly = typeof(OpcodeTableReader).Assembly;
        foreach (var table in Enum.GetValues<OpcodeTableKind>())
        {
            var file = FileName(table);
            using var stream = assembly.GetManifestResourceStream($"ZXSinclair.Net.Generate.Z80OpCodes.data.{file}")
                ?? throw new GeneratorException($"{file}: embedded resource not found.");
            using var reader = new StreamReader(stream);
            tables.Add(table, Read(reader, table, file));
        }
        return tables;
    }

    public static Opcode[] Read(TextReader reader, OpcodeTableKind table, string file)
    {
        var slots = new Opcode?[256];
        var aliases = new List<(byte Byte, SourceLocation Source)>();
        var lineNumber = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            var source = new SourceLocation(file, lineNumber);
            var split = line.IndexOfAny([' ', '\t']);
            var token = split < 0 ? line : line[..split];
            if (!token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || token.Length != 4
                || !byte.TryParse(token.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                throw new GeneratorException(source, $"Invalid opcode byte '{token}'.");
            if (slots[value] is not null)
                throw new GeneratorException(source, $"Duplicate opcode 0x{value:X2}.");
            var body = split < 0 ? "" : line[(split + 1)..].Trim();
            if (body.Length == 0)
            {
                slots[value] = new(table, value, "", [], OpcodeKind.Alias, source);
                aliases.Add((value, source));
                continue;
            }
            var opcode = ParseInstruction(table, value, body, source);
            slots[value] = opcode;
            foreach (var alias in aliases)
                slots[alias.Byte] = opcode with { Byte = alias.Byte, Kind = OpcodeKind.Alias, Source = alias.Source };
            aliases.Clear();
        }
        if (aliases.Count != 0)
            throw new GeneratorException(aliases[0].Source, "Alias has no following instruction.");
        var result = new Opcode[256];
        for (var i = 0; i < result.Length; i++)
        {
            if (slots[i] is { } opcode)
                result[i] = opcode;
            else if (table is OpcodeTableKind.ED or OpcodeTableKind.DDFD)
                result[i] = new(table, (byte)i, "", [], table == OpcodeTableKind.ED ? OpcodeKind.Hole : OpcodeKind.Absent,
                    new(file, 0));
            else
                throw new GeneratorException(new SourceLocation(file, lineNumber + 1), $"Missing opcode 0x{i:X2} in complete table.");
        }
        return result;
    }

    private static Opcode ParseInstruction(OpcodeTableKind table, byte value, string body, SourceLocation source)
    {
        var split = body.IndexOfAny([' ', '\t']);
        var mnemonic = split < 0 ? body : body[..split];
        var args = split < 0 ? "" : body[(split + 1)..].Trim();
        var kind = mnemonic == "shift" ? OpcodeKind.Prefix
            : mnemonic == "slttrap" && table == OpcodeTableKind.ED ? OpcodeKind.Hole : OpcodeKind.Instruction;
        if (kind == OpcodeKind.Hole)
        {
            if (args.Length != 0) throw new GeneratorException(source, "slttrap takes no operands.");
            return new(table, value, mnemonic, [], kind, source);
        }
        if (table == OpcodeTableKind.DDFDCB && mnemonic == "LD")
        {
            var comma = args.IndexOf(',');
            if (comma < 0) throw new GeneratorException(source, "Indexed CB copy requires a destination and an inner instruction.");
            var copy = OperandParser.Parse(args[..comma].Trim(), "LD", 0, source);
            var innerBody = args[(comma + 1)..].Trim();
            var innerSplit = innerBody.IndexOfAny([' ', '\t']);
            if (copy.Kind != OperandKind.Register8 || copy.Text == "F" || innerSplit < 0)
                throw new GeneratorException(source, "Invalid indexed CB copy.");
            var innerName = innerBody[..innerSplit];
            if (innerName is not ("RLC" or "RRC" or "RL" or "RR" or "SLA" or "SRA" or "SLL" or "SRL" or "RES" or "SET"))
                throw new GeneratorException(source, $"Invalid indexed CB inner instruction '{innerName}'.");
            var operands = ParseOperands(innerBody[(innerSplit + 1)..], innerName, source);
            if (operands.Length != (innerName is "RES" or "SET" ? 2 : 1)
                || operands[^1].Kind != OperandKind.IndexedMemory)
                throw new GeneratorException(source, "Invalid indexed CB inner operands.");
            return new(table, value, mnemonic, [copy], kind, source, copy, new(innerName, operands));
        }
        var parsed = ParseOperands(args, mnemonic, source);
        if (kind == OpcodeKind.Prefix && parsed.Length != 1)
            throw new GeneratorException(source, "Prefix requires one table name.");
        return new(table, value, mnemonic, parsed, kind, source);
    }

    private static Operand[] ParseOperands(string args, string mnemonic, SourceLocation source) =>
        args.Length == 0 ? [] : args.Split(',').Select((arg, i) => OperandParser.Parse(arg.Trim(), mnemonic, i, source)).ToArray();
}
