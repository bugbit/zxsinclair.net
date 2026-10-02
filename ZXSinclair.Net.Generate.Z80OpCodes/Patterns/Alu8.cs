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

internal sealed class Alu8Pattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic is "ADD" or "ADC" or "SUB" or "SBC" or "AND" or "XOR" or "OR" or "CP"
        && opcode.Operands.Length is 1 or 2
        && (opcode.Operands.Length == 1 || opcode.Operands[0] is { Kind: OperandKind.Register8, Text: "A" })
        && (opcode.Operands[^1].Kind is OperandKind.Register8 or OperandKind.IndexHigh
            or OperandKind.IndexLow or OperandKind.Immediate8 or OperandKind.IndexedMemory
            || opcode.Operands[^1] is { Kind: OperandKind.MemoryPair, Text: "(HL)" });

    public string EmitBody(Opcode opcode, EmitContext context)
    {
        var helper = opcode.Mnemonic switch
        {
            "ADD" => "Add8", "ADC" => "Adc8", "SUB" => "Sub8", "SBC" => "Sbc8",
            "AND" => "And8", "XOR" => "Xor8", "OR" => "Or8", "CP" => "Cp8",
            _ => throw new GeneratorException($"Unknown ALU operation {opcode.Mnemonic}."),
        };
        return $"{helper}({Value(opcode.Operands[^1], context)});";
    }
}

internal sealed class IncDec8Register : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic is "INC" or "DEC" && opcode.Operands.Length == 1
        && opcode.Operands[0].Kind is OperandKind.Register8 or OperandKind.IndexHigh or OperandKind.IndexLow;

    public string EmitBody(Opcode opcode, EmitContext context)
    {
        var target = Value(opcode.Operands[0], context);
        return $"{target} = {(opcode.Mnemonic == "INC" ? "Inc8" : "Dec8")}({target});";
    }
}

internal sealed class IncDec8Memory : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic is "INC" or "DEC" && opcode.Operands.Length == 1
        && (opcode.Operands[0].Kind == OperandKind.IndexedMemory
            || opcode.Operands[0] is { Kind: OperandKind.MemoryPair, Text: "(HL)" });

    public string EmitBody(Opcode opcode, EmitContext context) =>
        $"{(opcode.Mnemonic == "INC" ? "IncMemory" : "DecMemory")}({Address(opcode.Operands[0], context)});";
}

