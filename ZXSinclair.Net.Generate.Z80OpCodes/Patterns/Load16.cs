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

internal abstract class Load16Pattern : IPattern
{
    protected static bool Pair(Operand operand) => operand.Kind is OperandKind.Register16 or OperandKind.IndexPair;
    protected abstract string Mnemonic { get; }
    protected abstract bool Matches(Operand[] operands);
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == Mnemonic && Matches(opcode.Operands);
    public abstract string EmitBody(Opcode opcode, EmitContext context);
}

internal sealed class LoadPairImmediate : Load16Pattern
{
    protected override string Mnemonic => "LD";
    protected override bool Matches(Operand[] o) => o.Length == 2 && Pair(o[0]) && o[1].Kind == OperandKind.Immediate16;
    public override string EmitBody(Opcode opcode, EmitContext context) =>
        $"{Value(opcode.Operands[0], context)} = ReadPc16();";
}

internal sealed class LoadPairAbsolute : Load16Pattern
{
    protected override string Mnemonic => "LD";
    protected override bool Matches(Operand[] o) => o.Length == 2 && Pair(o[0]) && o[1].Kind == OperandKind.AbsoluteMemory;
    public override string EmitBody(Opcode opcode, EmitContext context) =>
        $"{Value(opcode.Operands[0], context)} = LoadWordAbsolute();";
}

internal sealed class StoreAbsolutePair : Load16Pattern
{
    protected override string Mnemonic => "LD";
    protected override bool Matches(Operand[] o) => o.Length == 2 && o[0].Kind == OperandKind.AbsoluteMemory && Pair(o[1]);
    public override string EmitBody(Opcode opcode, EmitContext context) =>
        $"StoreWordAbsolute({Value(opcode.Operands[1], context)});";
}

internal sealed class LoadStackPointer : Load16Pattern
{
    protected override string Mnemonic => "LD";
    protected override bool Matches(Operand[] o) => o.Length == 2 && o[0].Kind == OperandKind.Register16
        && o[0].Text == "SP" && (o[1].Kind == OperandKind.IndexPair || o[1].Kind == OperandKind.Register16 && o[1].Text == "HL");
    public override string EmitBody(Opcode opcode, EmitContext context) =>
        $"bus.Internal(Registers.IR, 2);\nRegisters.SP = {Value(opcode.Operands[1], context)};";
}

internal sealed class PushPair : Load16Pattern
{
    protected override string Mnemonic => "PUSH";
    protected override bool Matches(Operand[] o) => o.Length == 1 && Pair(o[0]);
    public override string EmitBody(Opcode opcode, EmitContext context) =>
        $"PushWithDelay({Value(opcode.Operands[0], context)});";
}

internal sealed class PopPair : Load16Pattern
{
    protected override string Mnemonic => "POP";
    protected override bool Matches(Operand[] o) => o.Length == 1 && Pair(o[0]);
    public override string EmitBody(Opcode opcode, EmitContext context) =>
        $"{Value(opcode.Operands[0], context)} = Pop();";
}

