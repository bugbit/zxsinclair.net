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

using ZXSinclair.Net.Generate.Z80OpCodes.Model;
using static ZXSinclair.Net.Generate.Z80OpCodes.Emit.OperandEmitter;

namespace ZXSinclair.Net.Generate.Z80OpCodes.Patterns;

internal static class BitEmission
{
    public static bool Operands(Instruction instruction) => instruction.Operands.Length == 2
        && instruction.Operands[0].Kind == OperandKind.Bit;

    public static bool Memory(Operand operand) => operand.Kind == OperandKind.IndexedMemory
        || operand is { Kind: OperandKind.MemoryPair, Text: "(HL)" };

    public static string Mask(Instruction instruction, bool reset = false)
    {
        var mask = 1 << instruction.Operands[0].Value!.Value;
        return $"0x{(reset ? mask ^ 0xFF : mask):X2}";
    }

    public static string Operation(Instruction instruction, string value) =>
        $"(byte)({value} {(instruction.Mnemonic == "SET" ? "|" : "&")} {Mask(instruction, instruction.Mnemonic == "RES")})";

    public static string MemoryBody(Instruction instruction, EmitContext context)
    {
        var address = Address(instruction.Operands[1], context);
        return $"var value = bus.Read({address});\nbus.Internal({address}, 1);\nvalue = {Operation(instruction, "value")};\nbus.Write({address}, value);";
    }
}

internal sealed class BitTestRegisterPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.Table == OpcodeTableKind.CB
        && opcode.Mnemonic == "BIT" && BitEmission.Operands(new(opcode.Mnemonic, opcode.Operands))
        && opcode.Operands[1].Kind == OperandKind.Register8;
    public string EmitBody(Opcode opcode, EmitContext context) =>
        $"BitTest({BitEmission.Mask(new(opcode.Mnemonic, opcode.Operands))}, {Value(opcode.Operands[1], context)});";
}

internal sealed class BitTestMemoryPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.Table is OpcodeTableKind.CB or OpcodeTableKind.DDFDCB
        && opcode.Mnemonic == "BIT" && opcode.InnerInstruction is null
        && BitEmission.Operands(new(opcode.Mnemonic, opcode.Operands)) && BitEmission.Memory(opcode.Operands[1]);
    public string EmitBody(Opcode opcode, EmitContext context)
    {
        var address = Address(opcode.Operands[1], context);
        return $"var value = bus.Read({address});\nbus.Internal({address}, 1);\nBitTestMemory({BitEmission.Mask(new(opcode.Mnemonic, opcode.Operands))}, value);";
    }
}

internal sealed class SetResRegisterPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Table == OpcodeTableKind.CB
        && opcode.Mnemonic is "SET" or "RES" && BitEmission.Operands(new(opcode.Mnemonic, opcode.Operands))
        && opcode.Operands[1].Kind == OperandKind.Register8;
    public string EmitBody(Opcode opcode, EmitContext context)
    {
        var target = Value(opcode.Operands[1], context);
        return $"{target} = {BitEmission.Operation(new(opcode.Mnemonic, opcode.Operands), target)};";
    }
}

internal sealed class SetResMemoryPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Table is OpcodeTableKind.CB or OpcodeTableKind.DDFDCB
        && opcode.Mnemonic is "SET" or "RES" && opcode.InnerInstruction is null
        && BitEmission.Operands(new(opcode.Mnemonic, opcode.Operands)) && BitEmission.Memory(opcode.Operands[1]);
    public string EmitBody(Opcode opcode, EmitContext context) =>
        BitEmission.MemoryBody(new(opcode.Mnemonic, opcode.Operands), context);
}

internal sealed class SetResMemoryCopyPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Table == OpcodeTableKind.DDFDCB
        && opcode.CopyTo is { Kind: OperandKind.Register8 } && opcode.InnerInstruction is { } inner
        && inner.Mnemonic is "SET" or "RES" && BitEmission.Operands(inner)
        && inner.Operands[1].Kind == OperandKind.IndexedMemory;
    public string EmitBody(Opcode opcode, EmitContext context) =>
        BitEmission.MemoryBody(opcode.InnerInstruction!, context) + $"\n{Value(opcode.CopyTo!, context)} = value;";
}
