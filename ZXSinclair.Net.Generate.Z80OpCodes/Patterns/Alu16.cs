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

internal sealed class Add16Pattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "ADD" && opcode.Operands.Length == 2
        && (opcode.Operands[0] is { Kind: OperandKind.Register16, Text: "HL" }
            || opcode.Operands[0].Kind == OperandKind.IndexPair)
        && opcode.Operands[1].Kind is OperandKind.Register16 or OperandKind.IndexPair;

    public string EmitBody(Opcode opcode, EmitContext context)
    {
        var destination = Value(opcode.Operands[0], context);
        return $"{destination} = Add16({destination}, {Value(opcode.Operands[1], context)});";
    }
}

internal sealed class AdcSbc16Pattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic is "ADC" or "SBC" && opcode.Operands.Length == 2
        && opcode.Operands[0] is { Kind: OperandKind.Register16, Text: "HL" }
        && opcode.Operands[1].Kind == OperandKind.Register16;

    public string EmitBody(Opcode opcode, EmitContext context) =>
        $"{(opcode.Mnemonic == "ADC" ? "Adc16" : "Sbc16")}({Value(opcode.Operands[1], context)});";
}

internal sealed class IncDec16Pattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic is "INC" or "DEC" && opcode.Operands.Length == 1
        && opcode.Operands[0].Kind is OperandKind.Register16 or OperandKind.IndexPair;

    public string EmitBody(Opcode opcode, EmitContext context) =>
        $"bus.Internal(Registers.IR, 2);\n{Value(opcode.Operands[0], context)}{(opcode.Mnemonic == "INC" ? "++" : "--")};";
}

