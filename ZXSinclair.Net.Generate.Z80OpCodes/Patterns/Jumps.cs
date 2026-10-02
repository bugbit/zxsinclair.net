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

internal abstract class JumpPattern : IPattern
{
    protected abstract string Mnemonic { get; }
    protected abstract bool Matches(Operand[] operands);
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == Mnemonic && Matches(opcode.Operands);
    public abstract string EmitBody(Opcode opcode, EmitContext context);

    protected static bool Target(Operand[] operands, OperandKind kind) =>
        operands.Length == 1 && operands[0].Kind == kind
        || operands.Length == 2 && operands[0].Kind == OperandKind.Condition && operands[1].Kind == kind;
    protected static string Test(Opcode opcode, EmitContext context) =>
        opcode.Operands.Length == 2 ? Value(opcode.Operands[0], context) : "true";
}

internal sealed class JumpAbsolutePattern : JumpPattern
{
    protected override string Mnemonic => "JP";
    protected override bool Matches(Operand[] o) => Target(o, OperandKind.Immediate16);
    public override string EmitBody(Opcode opcode, EmitContext context) => $"JumpAbsolute({Test(opcode, context)});";
}

internal sealed class JumpRegister : JumpPattern
{
    protected override string Mnemonic => "JP";
    protected override bool Matches(Operand[] o) => o.Length == 1
        && (o[0].Kind == OperandKind.IndexPair || o[0].Kind == OperandKind.Register16 && o[0].Text == "HL");
    public override string EmitBody(Opcode opcode, EmitContext context) => $"Registers.PC = {Value(opcode.Operands[0], context)};";
}

internal sealed class JumpRelativePattern : JumpPattern
{
    protected override string Mnemonic => "JR";
    protected override bool Matches(Operand[] o) => Target(o, OperandKind.RelativeOffset);
    public override string EmitBody(Opcode opcode, EmitContext context) => $"JumpRelative({Test(opcode, context)});";
}

internal sealed class DecrementJump : JumpPattern
{
    protected override string Mnemonic => "DJNZ";
    protected override bool Matches(Operand[] o) => o.Length == 1 && o[0].Kind == OperandKind.RelativeOffset;
    public override string EmitBody(Opcode opcode, EmitContext context) => "DecrementJumpNonZero();";
}

internal sealed class CallPattern : JumpPattern
{
    protected override string Mnemonic => "CALL";
    protected override bool Matches(Operand[] o) => Target(o, OperandKind.Immediate16);
    public override string EmitBody(Opcode opcode, EmitContext context) => $"CallAbsolute({Test(opcode, context)});";
}

internal sealed class ReturnPattern : JumpPattern
{
    protected override string Mnemonic => "RET";
    protected override bool Matches(Operand[] o) => o.Length == 0;
    public override string EmitBody(Opcode opcode, EmitContext context) => "Return();";
}

internal sealed class ReturnConditionalPattern : JumpPattern
{
    protected override string Mnemonic => "RET";
    protected override bool Matches(Operand[] o) => o.Length == 1 && o[0].Kind == OperandKind.Condition;
    public override string EmitBody(Opcode opcode, EmitContext context) => $"ReturnConditional({Value(opcode.Operands[0], context)});";
}

internal sealed class ReturnFromInterruptPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && (opcode.Mnemonic is "RETN" or "RETI") && opcode.Operands.Length == 0;
    public string EmitBody(Opcode opcode, EmitContext context) => "ReturnFromInterrupt();";
}

internal sealed class RestartPattern : JumpPattern
{
    protected override string Mnemonic => "RST";
    protected override bool Matches(Operand[] o) => o.Length == 1 && o[0].Kind == OperandKind.RestartAddress;
    public override string EmitBody(Opcode opcode, EmitContext context) => $"Restart(0x{opcode.Operands[0].Value:X2});";
}
