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

internal sealed class InImmediatePattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Mnemonic == "IN" && opcode.Operands.Length == 2
        && opcode.Operands[0] is { Kind: OperandKind.Register8, Text: "A" }
        && opcode.Operands[1].Kind == OperandKind.PortImmediate;
    public string EmitBody(Opcode opcode, EmitContext context) => "InAccumulator();";
}

internal sealed class OutImmediatePattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Mnemonic == "OUT" && opcode.Operands.Length == 2
        && opcode.Operands[0].Kind == OperandKind.PortImmediate
        && opcode.Operands[1] is { Kind: OperandKind.Register8, Text: "A" };
    public string EmitBody(Opcode opcode, EmitContext context) => "OutAccumulator();";
}

internal sealed class InRegisterPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Table == OpcodeTableKind.ED && opcode.Mnemonic == "IN"
        && opcode.Operands.Length == 2 && opcode.Operands[0].Kind == OperandKind.Register8
        && opcode.Operands[1].Kind == OperandKind.PortC;
    public string EmitBody(Opcode opcode, EmitContext context) => opcode.Operands[0].Text == "F"
        ? "InRegister();" : $"{Value(opcode.Operands[0], context)} = InRegister();";
}

internal sealed class OutRegisterPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Table == OpcodeTableKind.ED && opcode.Mnemonic == "OUT"
        && opcode.Operands.Length == 2 && opcode.Operands[0].Kind == OperandKind.PortC
        && (opcode.Operands[1].Kind == OperandKind.Register8
            || opcode.Operands[1] is { Kind: OperandKind.Constant, Value: 0 });
    public string EmitBody(Opcode opcode, EmitContext context) =>
        $"OutRegister({Value(opcode.Operands[1], context)});";
}

internal sealed class BlockIoPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.IsInstruction() && opcode.Table == OpcodeTableKind.ED && opcode.Operands.Length == 0
        && opcode.Mnemonic is "INI" or "IND" or "INIR" or "INDR" or "OUTI" or "OUTD" or "OTIR" or "OTDR";
    public string EmitBody(Opcode opcode, EmitContext context)
    {
        var input = opcode.Mnemonic.StartsWith("IN", StringComparison.Ordinal);
        var repeat = opcode.Mnemonic.EndsWith('R');
        var step = opcode.Mnemonic.Contains('D') ? -1 : 1;
        return $"Block{(input ? "In" : "Out")}{(repeat ? "Repeat" : "")}({step});";
    }
}
