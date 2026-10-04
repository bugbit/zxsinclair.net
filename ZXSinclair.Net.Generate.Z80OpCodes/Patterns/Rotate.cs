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

internal static class RotateEmission
{
    public static string? Helper(string mnemonic) => mnemonic switch
    {
        "RLC" => "Rlc", "RRC" => "Rrc", "RL" => "Rl", "RR" => "Rr",
        "SLA" => "Sla", "SRA" => "Sra", "SLL" => "Sll", "SRL" => "Srl",
        _ => null,
    };

    public static string MemoryBody(Instruction instruction, EmitContext context)
    {
        var address = Address(instruction.Operands[0], context);
        return $"var value = bus.Read({address});\nbus.Internal({address}, 1);\nvalue = {Helper(instruction.Mnemonic)}(value);\nbus.Write({address}, value);";
    }
}

internal sealed class RotateAccumulatorPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.IsInstruction()
        && opcode.Mnemonic is "RLCA" or "RRCA" or "RLA" or "RRA" && opcode.Operands.Length == 0;
    public string EmitBody(Opcode opcode, EmitContext context) => opcode.Mnemonic switch
    {
        "RLCA" => "Rlca();", "RRCA" => "Rrca();", "RLA" => "Rla();", "RRA" => "Rra();",
        _ => throw new GeneratorException($"Unknown accumulator rotation {opcode.Mnemonic}."),
    };
}

internal sealed class RotateRegisterPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Table == OpcodeTableKind.CB
        && RotateEmission.Helper(opcode.Mnemonic) is not null && opcode.Operands.Length == 1
        && opcode.Operands[0].Kind == OperandKind.Register8;
    public string EmitBody(Opcode opcode, EmitContext context)
    {
        var target = Value(opcode.Operands[0], context);
        return $"{target} = {RotateEmission.Helper(opcode.Mnemonic)}({target});";
    }
}

internal sealed class RotateMemoryPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Table is OpcodeTableKind.CB or OpcodeTableKind.DDFDCB
        && opcode.InnerInstruction is null && RotateEmission.Helper(opcode.Mnemonic) is not null
        && opcode.Operands.Length == 1
        && (opcode.Operands[0].Kind == OperandKind.IndexedMemory
            || opcode.Operands[0] is { Kind: OperandKind.MemoryPair, Text: "(HL)" });
    public string EmitBody(Opcode opcode, EmitContext context) =>
        RotateEmission.MemoryBody(new(opcode.Mnemonic, opcode.Operands), context);
}

internal sealed class RotateMemoryCopyPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Table == OpcodeTableKind.DDFDCB
        && opcode.CopyTo is { Kind: OperandKind.Register8 }
        && opcode.InnerInstruction is { Operands.Length: 1 } inner
        && RotateEmission.Helper(inner.Mnemonic) is not null
        && inner.Operands[0].Kind == OperandKind.IndexedMemory;
    public string EmitBody(Opcode opcode, EmitContext context) =>
        RotateEmission.MemoryBody(opcode.InnerInstruction!, context) + $"\n{Value(opcode.CopyTo!, context)} = value;";
}

internal sealed class RotateDigitPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Table == OpcodeTableKind.ED
        && opcode.Mnemonic is "RLD" or "RRD" && opcode.Operands.Length == 0;
    public string EmitBody(Opcode opcode, EmitContext context) =>
        opcode.Mnemonic == "RLD" ? "Rld();" : "Rrd();";
}

